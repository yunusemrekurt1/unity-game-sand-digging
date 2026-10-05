using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD + Kazan/Kaybet panelleri. Kazanınca yıldızları tek tek animasyonla gösterir.
/// </summary>
public class UIYonetici : MonoBehaviour
{
    [Header("HUD")]
    public Text hedefText;
    public Text durumText;

    [Header("Win Panel")]
    public GameObject kazandinPanel;
    public Image[] yildizSlots = new Image[3];     // 3 yıldız slotu
    public Sprite yildizSprite;                    // dolu yıldız sprite
    public AudioSource yildizSes;

    [Header("Lose Panel")]
    public GameObject kaybettinPanel;

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnKazandi  += Kazandi;
            LevelManager.Instance.OnKaybetti += Kaybetti;
        }
        if (kazandinPanel   != null) kazandinPanel.SetActive(false);
        if (kaybettinPanel  != null) kaybettinPanel.SetActive(false);

        // Yıldızları başlangıçta gizle
        foreach (var s in yildizSlots)
            if (s != null) { s.transform.localScale = Vector3.zero; s.color = new Color(1,1,1,0); }
    }

    void Update()
    {
        if (LevelManager.Instance == null) return;
        if (hedefText != null)
        {
            int u = LevelManager.Instance.UlasanTop;
            int yildiz = YildizSistemi.YildizHesapla(u);
            hedefText.text = $"★ {u}/9  ({yildiz} yildiz)";
        }

        if (durumText != null)
        {
            int maks = LevelManager.Instance.maksimumKayip;
            if (maks >= 0)
                durumText.text = $"Kayip: {LevelManager.Instance.KaybedilenTop}/{maks}";
            else
                durumText.text = $"Top: {LevelManager.Instance.ToplamTop}";
        }
    }

    void Kazandi(int yildiz)
    {
        if (kazandinPanel != null) kazandinPanel.SetActive(true);
        StartCoroutine(YildizAnimasyonu(yildiz));
    }

    IEnumerator YildizAnimasyonu(int yildiz)
    {
        yield return new WaitForSeconds(0.4f);
        for (int i = 0; i < 3; i++)
        {
            if (yildizSlots[i] == null) continue;
            if (i >= yildiz) continue;

            yield return new WaitForSeconds(0.35f);
            if (yildizSes != null) yildizSes.Play();

            // Pop animasyon: küçükten büyüyerek gelsin
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                float s = Mathf.LerpUnclamped(0f, 1.3f, EaseOutBack(t));
                yildizSlots[i].transform.localScale = Vector3.one * s;
                yildizSlots[i].color = new Color(1f, 1f, 1f, Mathf.Clamp01(t));
                yield return null;
            }
            // Yerine otur
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 8f;
                yildizSlots[i].transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t);
                yield return null;
            }
        }
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    void Kaybetti() { if (kaybettinPanel != null) kaybettinPanel.SetActive(true); }

    public void BtnTekrar()   { LevelManager.Instance.BolumuYenidenBaslat(); }
    public void BtnSonraki()  { LevelManager.Instance.SonrakiBolum(); }
    public void BtnAnaMenu()  { LevelManager.Instance.AnaMenuyeDon(); }
}
