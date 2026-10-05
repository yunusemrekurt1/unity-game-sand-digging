using UnityEngine;

/// <summary>
/// KumAna objesine eklenir.
/// - Sprite'ın texture'ını Read/Write üzerinden okunabilir kopyaya çevirir.
/// - Mouse / Touch basılı tutulup sürüklenince piksellerin alpha'sını 0 yapar.
/// - Sprite yatay/dikey sündürülmüş olsa bile fırça DAİMA tam yuvarlak kazar
///   (Scale oranına göre normalizasyon).
/// - Pikseller silindikten sonra PolygonCollider2D'nin path'lerini günceller
///   (component yok edilip yeniden eklenmez - bkz. PolygonColliderYenileVeComposite),
///   CompositeOperation = Merge yapar ve geometriyi yeniler.
///
/// ÖNEMLİ: Kullanılan Sprite'ın Import ayarlarında:
///   * Mesh Type       : Full Rect
///   * Read/Write      : Enabled
///   * (İsteğe bağlı)  : Compression = None  (daha net kazma için)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CompositeCollider2D))]
public class KumKazici : MonoBehaviour
{
    [Header("Fırça Ayarları")]
    [Tooltip("Dünya biriminde (unit) fırça yarıçapı. Kamera büyüklüğüne göre ayarlayın.")]
    public float firciYariCapDunya = 0.35f;

    [Tooltip("Fırça her frame kaydırıldığında ara noktalarda da kazsın (çizgi boşluklarını önler).")]
    public bool surekliKazi = true;

    [Header("Collider Yenileme")]
    [Tooltip("Her kazma sonrası collider yeniden üretilir. Performans için her N framede bir yap.")]
    public int colliderYenilemeFrameAraligi = 2;

    // Referanslar
    private SpriteRenderer m_SpriteRenderer;
    private Sprite m_RuntimeSprite;
    private Texture2D m_Texture;
    private CompositeCollider2D m_CompositeCollider;
    private PolygonCollider2D m_PolygonCollider;

    // Önceki mouse pozisyonu (sürekli kazma için)
    private Vector3 m_OncekiDunyaPoz;
    private bool m_KaziyorMu;
    private int m_FrameSayaci;

    // DÜZELTME: Camera.main her çağrıldığında Unity içeride "MainCamera" tag'li
    // objeyi sahnede arar (FindGameObjectWithTag benzeri bir maliyeti var).
    // Her kazma noktası için (ve sürekli kazımada her ara adım için) bu aramayı
    // tekrar tekrar yapmak yerine kamerayı bir kez cache'liyoruz.
    private Camera m_Kamera;

    void Awake()
    {
        m_SpriteRenderer   = GetComponent<SpriteRenderer>();
        m_CompositeCollider = GetComponent<CompositeCollider2D>();
        m_Kamera = Camera.main;

        // Rigidbody2D'nin Static olduğundan ve CompositeCollider'ın Polygons olduğundan emin ol
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        m_CompositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;

        // Sprite texture'ının yazılabilir bir kopyasını oluştur
        HazirlaYazilabilirSprite();

        // Başlangıç collider'ı
        PolygonColliderYenileVeComposite();
    }

    /// <summary>
    /// Orijinal sprite'ı bozmadan, aynı pivot ve pixelsPerUnit ile yazılabilir bir
    /// runtime Texture2D + Sprite oluşturur.
    /// </summary>
    private void HazirlaYazilabilirSprite()
    {
        Sprite orj = m_SpriteRenderer.sprite;
        if (orj == null)
        {
            Debug.LogError("KumKazici: SpriteRenderer'da sprite yok!");
            return;
        }

        Texture2D kaynak = orj.texture;

        // Yazılabilir kopya oluştur (RGBA32 - alpha kazma için şart)
        m_Texture = new Texture2D(kaynak.width, kaynak.height, TextureFormat.RGBA32, false);
        m_Texture.filterMode = kaynak.filterMode;
        m_Texture.wrapMode   = kaynak.wrapMode;

        // Orijinal pikselleri kopyala (Read/Write Enabled olmalı!)
        m_Texture.SetPixels(kaynak.GetPixels());
        m_Texture.Apply();

        // Yeni sprite (orijinal pivot, rect ve pixelsPerUnit korunur)
        Rect rect = new Rect(0, 0, m_Texture.width, m_Texture.height);
        Vector2 pivot = new Vector2(
            orj.pivot.x / orj.rect.width,
            orj.pivot.y / orj.rect.height
        );

        m_RuntimeSprite = Sprite.Create(
            m_Texture,
            rect,
            pivot,
            orj.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect,    // <-- FullRect şart
            Vector4.zero,
            true                         // generateFallbackPhysicsShape: alpha'dan şekil üret
        );

        m_SpriteRenderer.sprite  = m_RuntimeSprite;
        m_SpriteRenderer.drawMode = SpriteDrawMode.Simple; // Simple şart
    }

    void Update()
    {
        // Pause sırasında kazma
        if (Time.timeScale == 0f) { m_KaziyorMu = false; return; }

        // --- Girdi (mouse + touch) ---
        bool basliyor = Input.GetMouseButtonDown(0);
        bool basili   = Input.GetMouseButton(0);
        bool birakti  = Input.GetMouseButtonUp(0);

        if (basliyor)
        {
            m_KaziyorMu = true;
            m_OncekiDunyaPoz = EkranDanDunyaya(Input.mousePosition);
            KaziNoktada(m_OncekiDunyaPoz);
        }
        else if (basili && m_KaziyorMu)
        {
            Vector3 yeniPoz = EkranDanDunyaya(Input.mousePosition);
            if (surekliKazi)
            {
                // İki nokta arasında boşluk bırakmadan kazmak için interpolasyon
                float mesafe = Vector3.Distance(m_OncekiDunyaPoz, yeniPoz);
                int adimSayisi = Mathf.Max(1, Mathf.CeilToInt(mesafe / (firciYariCapDunya * 0.5f)));
                for (int i = 1; i <= adimSayisi; i++)
                {
                    Vector3 p = Vector3.Lerp(m_OncekiDunyaPoz, yeniPoz, i / (float)adimSayisi);
                    KaziNoktada(p);
                }
            }
            else
            {
                KaziNoktada(yeniPoz);
            }
            m_OncekiDunyaPoz = yeniPoz;
        }
        else if (birakti)
        {
            m_KaziyorMu = false;
        }
    }

    private Vector3 EkranDanDunyaya(Vector3 ekran)
    {
        if (m_Kamera == null) m_Kamera = Camera.main; // güvenlik: sahne değişse bile null kalmasın
        if (m_Kamera == null) return ekran;
        // Kamera hareket ettiği için MUTLAKA ScreenToWorldPoint
        ekran.z = -m_Kamera.transform.position.z;
        return m_Kamera.ScreenToWorldPoint(ekran);
    }

    /// <summary>
    /// Dışarıdan (örn. Bomba) çağrılabilir: verilen dünya noktasında büyük
    /// dairesel bir krater açar ve collider'ı anında yeniler.
    /// </summary>
    public void KumaKraterAc(Vector3 dunyaPoz, float yariCapDunya)
    {
        if (m_Texture == null || m_RuntimeSprite == null) return;

        float eskiFirca = firciYariCapDunya;
        firciYariCapDunya = yariCapDunya;
        KaziNoktada(dunyaPoz);
        firciYariCapDunya = eskiFirca;

        // Bomba gibi büyük olaylarda hemen collider'ı yenile
        PolygonColliderYenileVeComposite();
        m_FrameSayaci = 0;
    }

    /// <summary>
    /// Verilen dünya noktasında fırça yarıçapı kadar alanı saydamlaştırır.
    /// Elips Önleme: transform.lossyScale'e göre X ve Y'yi farklı ölçekleyip
    /// texture uzayında ELİPS çizeriz - bu dünya uzayında TAM DAİRE olarak görünür.
    /// </summary>
    private void KaziNoktada(Vector3 dunyaPoz)
    {
        if (m_Texture == null || m_RuntimeSprite == null) return;

        // Dünya -> Local (objenin local uzayı)
        Vector3 local = transform.InverseTransformPoint(dunyaPoz);

        // Local -> Texture piksel koordinatı
        // local (0,0) noktası sprite pivotuna denk gelir.
        float ppu = m_RuntimeSprite.pixelsPerUnit;
        Vector2 pivotPix = m_RuntimeSprite.pivot; // piksel cinsinden
        int merkezX = Mathf.RoundToInt(local.x * ppu + pivotPix.x);
        int merkezY = Mathf.RoundToInt(local.y * ppu + pivotPix.y);

        // Elips Önleme: objenin X ve Y scale'i farklıysa texture uzayında
        // farklı yarıçaplar kullan. Böylece dünya uzayında her zaman daire olur.
        Vector3 s = transform.lossyScale;
        float sx = Mathf.Abs(s.x) < 0.0001f ? 1f : Mathf.Abs(s.x);
        float sy = Mathf.Abs(s.y) < 0.0001f ? 1f : Mathf.Abs(s.y);

        float rx = (firciYariCapDunya * ppu) / sx; // texture uzayında yatay yarıçap
        float ry = (firciYariCapDunya * ppu) / sy; // texture uzayında dikey  yarıçap

        int baslangicX = Mathf.Max(0, Mathf.FloorToInt(merkezX - rx));
        int bitisX     = Mathf.Min(m_Texture.width  - 1, Mathf.CeilToInt(merkezX + rx));
        int baslangicY = Mathf.Max(0, Mathf.FloorToInt(merkezY - ry));
        int bitisY     = Mathf.Min(m_Texture.height - 1, Mathf.CeilToInt(merkezY + ry));

        if (baslangicX >= bitisX || baslangicY >= bitisY) return;

        int gW = bitisX - baslangicX + 1;
        int gH = bitisY - baslangicY + 1;

        // Bölgeyi toplu oku (SetPixels hızlıdır)
        Color[] bolge = m_Texture.GetPixels(baslangicX, baslangicY, gW, gH);
        bool degistiMi = false;

        float rx2 = rx * rx;
        float ry2 = ry * ry;

        for (int y = 0; y < gH; y++)
        {
            int texY = baslangicY + y;
            float dy = texY - merkezY;
            float dy2rel = (dy * dy) / ry2;
            if (dy2rel > 1f) continue;

            for (int x = 0; x < gW; x++)
            {
                int texX = baslangicX + x;
                float dx = texX - merkezX;
                // Elips denklemi (dx/rx)^2 + (dy/ry)^2 <= 1
                if ((dx * dx) / rx2 + dy2rel <= 1f)
                {
                    int idx = y * gW + x;
                    if (bolge[idx].a > 0f)
                    {
                        bolge[idx].a = 0f;
                        degistiMi = true;
                    }
                }
            }
        }

        if (!degistiMi) return;

        m_Texture.SetPixels(baslangicX, baslangicY, gW, gH, bolge);
        m_Texture.Apply(false);

        // Collider'ı belli aralıklarla yenile (her frame çok pahalı olur)
        m_FrameSayaci++;
        if (m_FrameSayaci >= colliderYenilemeFrameAraligi)
        {
            m_FrameSayaci = 0;
            PolygonColliderYenileVeComposite();
        }
    }

    /// <summary>
    /// Sprite'ı texture'ın GÜNCEL alpha durumuyla yeniden üretir (physics shape
    /// yenilenir) ve AYNI PolygonCollider2D'nin path'lerini bu yeni physics shape'e
    /// göre günceller. GenerateGeometry ile CompositeCollider'ı yeniler.
    ///
    /// DÜZELTME (önemli bug fix): Eski kod her seferinde PolygonCollider2D'yi
    /// Destroy() edip hemen ardından AddComponent ile YENİSİNİ ekliyordu. Ancak
    /// Application.isPlaying==true iken Destroy() component'i O ANDA silmez,
    /// frame'in sonunda siler. Bu yüzden bir anlığına aynı objede İKİ tane
    /// PolygonCollider2D bulunuyordu: biri kazılmadan ÖNCEki (silinmeyi bekleyen,
    /// hâlâ delik yokmuş gibi solid), diğeri kazılmadan SONRAki. CompositeCollider2D
    /// ikisini de o anda geometriye dahil ettiği için, birleşim (Merge) sonucu
    /// yeni açılan deliğin üstü eski collider tarafından "kapatılmış" gibi kalıyor
    /// ve topun kazılan yerden gerçek anlamda düşebilmesi bir-iki frame gecikebiliyordu.
    /// Ayrıca her kazmada component Destroy/AddComponent yapmak gereksiz GC/allocation
    /// yaratıyordu. Çözüm: PolygonCollider2D'yi hiç yok etmeden SAKLIYORUZ, sadece
    /// Sprite.GetPhysicsShape(...) ile güncel konturu okuyup SetPath(...) ile
    /// path'lerini güncelliyoruz.
    /// </summary>
    private static readonly System.Collections.Generic.List<Vector2> s_GeciciPuanlar = new();

    private void PolygonColliderYenileVeComposite()
    {
        if (m_RuntimeSprite == null || m_Texture == null) return;

        // 1) Sprite'ı güncel texture ile yeniden yarat (physics shape yenilenir).
        //    Not: Unity'de runtime bir Sprite'ın physics shape'ini "yerinde" yeniden
        //    hesaplatan bir API yok; bu yüzden Sprite.Create'i yeniden çağırmak şart.
        var eskiSprite = m_RuntimeSprite;
        float ppu = eskiSprite.pixelsPerUnit;
        Vector2 pivotN = new Vector2(
            eskiSprite.pivot.x / eskiSprite.rect.width,
            eskiSprite.pivot.y / eskiSprite.rect.height
        );
        Rect rect = new Rect(0, 0, m_Texture.width, m_Texture.height);

        m_RuntimeSprite = Sprite.Create(
            m_Texture, rect, pivotN, ppu, 0,
            SpriteMeshType.FullRect,
            Vector4.zero,
            true   // physics shape'i alpha'dan yeniden üret
        );
        m_SpriteRenderer.sprite = m_RuntimeSprite;

        // Eski sprite'ı temizle (leak önleme)
        if (Application.isPlaying) Destroy(eskiSprite);
        else DestroyImmediate(eskiSprite);

        // 2) PolygonCollider2D'yi ilk seferde bul/oluştur, SONRAKİ her çağrıda
        //    AYNI component'i kullan (destroy/recreate YOK).
        if (m_PolygonCollider == null)
        {
            m_PolygonCollider = GetComponent<PolygonCollider2D>();
            if (m_PolygonCollider == null)
                m_PolygonCollider = gameObject.AddComponent<PolygonCollider2D>();
            m_PolygonCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        // 3) Yeni sprite'ın physics shape'ini (delikler dahil, birden fazla
        //    parça olabilir) doğrudan collider path'lerine aktar.
        int sekilSayisi = m_RuntimeSprite.GetPhysicsShapeCount();
        if (sekilSayisi <= 0)
        {
            // Kum tamamen kazılıp bitmiş olabilir: geçerli bir path olmadan
            // pathCount=0 bazı Unity sürümlerinde sorun çıkarabildiği için
            // güvenli tarafta kalıp tek, boş bir path bırakıyoruz.
            m_PolygonCollider.pathCount = 1;
            m_PolygonCollider.SetPath(0, System.Array.Empty<Vector2>());
        }
        else
        {
            m_PolygonCollider.pathCount = sekilSayisi;
            for (int i = 0; i < sekilSayisi; i++)
            {
                s_GeciciPuanlar.Clear();
                m_RuntimeSprite.GetPhysicsShape(i, s_GeciciPuanlar);
                m_PolygonCollider.SetPath(i, s_GeciciPuanlar);
            }
        }

        // 4) Composite geometriyi yenile
        m_CompositeCollider.GenerateGeometry();
    }
}
