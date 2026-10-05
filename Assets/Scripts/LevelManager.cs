using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bölüm yöneticisi (Singleton). 9 top ile başlar, yıldız sistemi entegre.
/// Kazanma/kaybetme olaylarını fırlatır ve yıldızı PlayerPrefs'e kaydeder.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Hedef")]
    [Tooltip("Yıldız almak için minimum top sayısı. 3/6/9 eşiklerine göre yıldız verilir.")]
    public int hedefTopSayisi = 3;

    [Tooltip("Maksimum top kaybı. -1 sınırsız.")]
    public int maksimumKayip = -1;

    [Header("Durum (otomatik)")]
    [SerializeField] private int m_UlasanTop = 0;
    [SerializeField] private int m_KaybedilenTop = 0;
    [SerializeField] private int m_ToplamTop = 0;

    [Header("Olaylar")]
    public System.Action<int> OnKazandi;      // parametre: yildiz sayisi
    public System.Action OnKaybetti;

    public int UlasanTop     => m_UlasanTop;
    public int KaybedilenTop => m_KaybedilenTop;
    public int ToplamTop     => m_ToplamTop;

    private bool m_BitmisMi = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // NOT: Bu sayım Awake'te yapılır (Start'ta değil!). Unity, sahnedeki TÜM
        // objelerin Awake'ini çağırdıktan SONRA Start'ları çağırır. Eğer bu sayım
        // Start'ta yapılsaydı, TopSpawner/Cogaltici gibi başka scriptlerin Start'ı
        // bizden ÖNCE çalışıp YeniTopEklendi() çağırmış olabilir ve çift sayım
        // (ya da eksik sayım) oluşurdu. Awake'te yapmak bu yarış durumunu engeller.
        m_ToplamTop = GameObject.FindGameObjectsWithTag("Top").Length;
    }

    public void YeniTopEklendi()   { m_ToplamTop++; }

    public void TopUlastirildi(Top top)
    {
        if (m_BitmisMi) return;
        m_UlasanTop++;
        KontrolEt();
    }

    public void TopKaybedildi(Top top)
    {
        if (m_BitmisMi) return;
        m_KaybedilenTop++;
        KontrolEt();
    }

    private void KontrolEt()
    {
        // 9 topa ulaştıysak tam 3 yıldız - hemen bitir
        if (m_UlasanTop >= 9)
        {
            Bitir(3);
            return;
        }

        // DÜZELTME: Önceden burada "GameObject.FindGameObjectsWithTag("Top").Length"
        // kullanılıyordu. Ancak bir top kutuya ulaştığında/öldüğünde, TopUlastirildi()
        // veya TopKaybedildi() ÖNCE çağrılıyor, Destroy(gameObject) SONRA çağrılıyor.
        // Unity'de Destroy() o obje ve tag'ini o frame'in SONUNA kadar silmez, yani
        // bu KontrolEt() çağrıldığı anda o top hâlâ "Top" tag'iyle sahnede bulunuyordu.
        // Sonuç: son top ulaştığında/öldüğünde sahnedeKalan yanlışlıkla "1" çıkıyor
        // (kendisini sayıyor) ve bölüm hiç bitmeyebiliyordu. Artık sayaçlardan
        // aritmetik olarak hesaplıyoruz - bu, Destroy zamanlamasından bağımsızdır.
        int sahnedeKalan = m_ToplamTop - m_UlasanTop - m_KaybedilenTop;

        if (sahnedeKalan <= 0)
        {
            int yildiz = YildizSistemi.YildizHesapla(m_UlasanTop);
            if (yildiz >= 1) Bitir(yildiz);
            else             Kaybet();
            return;
        }

        // Kayıp sınırı aşıldı mı?
        if (maksimumKayip >= 0 && m_KaybedilenTop >= maksimumKayip)
        {
            // Kayıp sınırı aşıldı ama hedef tutturulduysa yıldız ver
            int yildiz = YildizSistemi.YildizHesapla(m_UlasanTop);
            if (yildiz >= 1) Bitir(yildiz);
            else             Kaybet();
        }
    }

    void Bitir(int yildiz)
    {
        m_BitmisMi = true;
        int no = YildizSistemi.BolumNoAktifSahneden();
        if (no > 0) YildizSistemi.Kaydet(no, yildiz);
        Debug.Log($"[LevelManager] KAZANDI - {yildiz} yildiz");
        OnKazandi?.Invoke(yildiz);
    }

    void Kaybet()
    {
        m_BitmisMi = true;
        Debug.Log("[LevelManager] GAME OVER");
        OnKaybetti?.Invoke();
    }

    public void BolumuYenidenBaslat()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SonrakiBolum()
    {
        int sira = SceneManager.GetActiveScene().buildIndex + 1;
        if (sira < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(sira);
        else
            SceneManager.LoadScene(0);
    }

    public void AnaMenuyeDon()
    {
        SceneManager.LoadScene(0);  // index 0 = AnaMenu
    }
}
