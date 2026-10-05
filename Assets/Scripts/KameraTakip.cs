using UnityEngine;

/// <summary>
/// Main Camera'ya eklenir.
/// - Sahnedeki EN AŞAĞIDAKİ (Y ekseninde en küçük) topu otomatik bulur
///   ve sadece Y ekseninde Mathf.Lerp ile yumuşak takip eder.
/// - Kameranın başlangıç Y pozisyonundan YUKARI çıkması engellenir
///   (sadece aşağı inebilir - kum kazma oyunu için tipik davranış).
/// - Takip edilecek top manuel olarak atanırsa otomatik arama yapılmaz.
/// </summary>
public class KameraTakip : MonoBehaviour
{
    [Header("Takip")]
    [Tooltip("Manuel atanırsa bu top takip edilir. Boş bırakılırsa otomatik olarak " +
             "\"Top\" tag'li objelerden en aşağıdaki seçilir.")]
    public Transform takipEdilecekTop;

    [Tooltip("Otomatik bulma modunda hangi tag aranır?")]
    public string topTag = "Top";

    [Header("Yumuşatma")]
    [Range(0.01f, 20f)]
    public float lerpHizi = 4f;

    [Tooltip("Kamera ile top arasında dikey ofset (kamera topun ne kadar üstünde kalsın).")]
    public float yOfset = 0f;

    // Başlangıç Y - kamera bunun üstüne asla çıkmayacak
    private float m_BaslangicY;

    // DÜZELTME: Inspector'da elle bir top atanmışsa onu sabit takip et; atanmamışsa
    // HER FRAME en aşağıdaki topu yeniden ara. Eskiden sadece "takipEdilecekTop == null"
    // iken arama yapılıyordu, yani otomatik modda bir kez bir topa kilitleniyor ve o top
    // yok olana kadar (kutuya girene/ölene kadar) başka bir top daha aşağı inmiş olsa
    // bile kamera onu takip etmeye devam ediyordu. Artık gerçekten "en aşağıdaki" topu
    // sürekli takip ediyor.
    private bool m_ManuelAtanan;

    void Start()
    {
        m_BaslangicY = transform.position.y;
        m_ManuelAtanan = takipEdilecekTop != null;
    }

    void LateUpdate()
    {
        if (!m_ManuelAtanan)
        {
            Transform enAsagi = EnAsagidakiTopuBul();
            if (enAsagi != null) takipEdilecekTop = enAsagi;
        }

        if (takipEdilecekTop == null) return;

        float hedefY = takipEdilecekTop.position.y + yOfset;

        // Başlangıç Y'den daha yukarı çıkma (sadece aşağı)
        if (hedefY > m_BaslangicY)
            hedefY = m_BaslangicY;

        float yeniY = Mathf.Lerp(transform.position.y, hedefY, Time.deltaTime * lerpHizi);

        // X ve Z sabit kalır - sadece Y ekseninde takip
        Vector3 p = transform.position;
        p.y = yeniY;
        transform.position = p;
    }

    /// <summary>
    /// Sahnedeki "Top" tag'li objelerden Y ekseninde en aşağıda olanı döndürür.
    /// Yok edilmiş toplar otomatik olarak hariç tutulur.
    /// </summary>
    private Transform EnAsagidakiTopuBul()
    {
        GameObject[] toplar = GameObject.FindGameObjectsWithTag(topTag);
        if (toplar == null || toplar.Length == 0) return null;

        Transform enAsagi = null;
        float enKucukY = float.MaxValue;

        for (int i = 0; i < toplar.Length; i++)
        {
            if (toplar[i] == null) continue;
            float y = toplar[i].transform.position.y;
            if (y < enKucukY)
            {
                enKucukY = y;
                enAsagi = toplar[i].transform;
            }
        }
        return enAsagi;
    }
}
