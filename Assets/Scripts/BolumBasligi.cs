using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bölüm başında büyük "BÖLÜM X" yazısı gösterir, sonra fade-out olur.
/// 1.8 saniye sonra kendini kapatır. Toplar bu süre boyunca beklemez
/// (ayrı bir spawn rutini kontrol eder).
/// </summary>
public class BolumBasligi : MonoBehaviour
{
    public CanvasGroup cg;
    public Text baslik;
    public Text altbaslik;

    public float gosterimSuresi = 1.5f;
    public float fadeSuresi     = 0.5f;

    public static string SonrakiAltBaslik = null; // "FINAL" gibi özel metinler

    void Start()
    {
        int no = YildizSistemi.BolumNoAktifSahneden();
        if (baslik != null) baslik.text = $"BOLUM {no}";
        if (altbaslik != null)
        {
            altbaslik.text = SonrakiAltBaslik ?? AltBaslikOlustur(no);
            SonrakiAltBaslik = null;
        }
        StartCoroutine(Goster());
    }

    string AltBaslikOlustur(int no)
    {
        switch (no)
        {
            case 1: return "Alistirma";
            case 2: return "Derinlere";
            case 3: return "Kaya Labirenti";
            case 4: return "Su Tehlikesi";
            case 5: return "Dikenlere Dikkat";
            case 6: return "Patlama Zamani";
            case 7: return "Cogaltici Devrede";
            case 8: return "Lav Cukuru";
            case 9: return "Renkli Kaos";
            case 10: return "FINAL — Hepsi Birden";
        }
        return "";
    }

    IEnumerator Goster()
    {
        // Fade in (0.3s)
        float t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime / 0.3f; if (cg != null) cg.alpha = t; yield return null; }
        if (cg != null) cg.alpha = 1f;

        // Bekle
        yield return new WaitForSecondsRealtime(gosterimSuresi);

        // Fade out
        t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime / fadeSuresi; if (cg != null) cg.alpha = 1f - t; yield return null; }
        if (cg != null) cg.alpha = 0f;

        gameObject.SetActive(false);
    }
}
