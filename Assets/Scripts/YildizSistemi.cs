using UnityEngine;

/// <summary>
/// Yıldız sistemi + PlayerPrefs kalıcı kaydetme.
///
/// Yıldız kuralı (her bölümde 9 top var):
///   3 top ulaştırılırsa  -> 1 yıldız
///   6 top ulaştırılırsa  -> 2 yıldız
///   9 top ulaştırılırsa  -> 3 yıldız
///   3'ten az             -> 0 yıldız (kaybedildi)
/// </summary>
public static class YildizSistemi
{
    public const int BOLUM_TOP_SAYISI = 9;

    public static int YildizHesapla(int ulasan)
    {
        if (ulasan >= 9) return 3;
        if (ulasan >= 6) return 2;
        if (ulasan >= 3) return 1;
        return 0;
    }

    public static string Anahtar(int bolumNo) => $"Stars_Level_{bolumNo:D2}";

    /// <summary>En yüksek yıldız skorunu kaydet (düşürmez).</summary>
    public static void Kaydet(int bolumNo, int yildiz)
    {
        int mevcut = Oku(bolumNo);
        if (yildiz > mevcut)
        {
            PlayerPrefs.SetInt(Anahtar(bolumNo), yildiz);
            PlayerPrefs.Save();
        }
    }

    public static int Oku(int bolumNo) => PlayerPrefs.GetInt(Anahtar(bolumNo), 0);

    public static int ToplamYildiz()
    {
        int toplam = 0;
        for (int i = 1; i <= 10; i++) toplam += Oku(i);
        return toplam;
    }

    /// <summary>Sahnenin ismini (build index'ten değil, name'den) parse eder.</summary>
    public static int BolumNoAktifSahneden()
    {
        string ad = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // Level_01 -> 1
        if (ad.StartsWith("Level_") && int.TryParse(ad.Substring(6), out int n)) return n;
        return 0;
    }
}
