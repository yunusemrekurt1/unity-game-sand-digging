using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ana Menü + Bölüm Seçme. Tek sahnede iki panel.
/// "Level_00_AnaMenu" sahnesine yerleştirilir.
/// </summary>
public class AnaMenu : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject anaPanel;
    public GameObject bolumSecmePanel;

    [Header("Bölüm Butonları (10 adet)")]
    public Button[] bolumButonlari = new Button[10];
    public Image[] yildizIkonlari1 = new Image[10];
    public Image[] yildizIkonlari2 = new Image[10];
    public Image[] yildizIkonlari3 = new Image[10];

    [Header("Toplam Yıldız")]
    public Text toplamYildizText;

    [Header("Sprites")]
    public Sprite yildizDolu;
    public Sprite yildizBos;

    void Start()
    {
        GoruntuleAnaPanel();
        BolumButonlariniDoldur();
    }

    void GoruntuleAnaPanel()
    {
        if (anaPanel != null) anaPanel.SetActive(true);
        if (bolumSecmePanel != null) bolumSecmePanel.SetActive(false);
    }

    public void BtnOyna()
    {
        if (anaPanel != null) anaPanel.SetActive(false);
        if (bolumSecmePanel != null) bolumSecmePanel.SetActive(true);
        BolumButonlariniDoldur();
    }

    public void BtnGeri()
    {
        GoruntuleAnaPanel();
    }

    public void BtnCikis()
    {
        Application.Quit();
    }

    void BolumButonlariniDoldur()
    {
        if (toplamYildizText != null)
            toplamYildizText.text = $"Toplam ★: {YildizSistemi.ToplamYildiz()} / 30";

        for (int i = 0; i < bolumButonlari.Length; i++)
        {
            int no = i + 1;
            int yildiz = YildizSistemi.Oku(no);

            // 3 yıldız ikonunu doldur/boşalt
            YildizGoster(yildizIkonlari1, i, yildiz >= 1);
            YildizGoster(yildizIkonlari2, i, yildiz >= 2);
            YildizGoster(yildizIkonlari3, i, yildiz >= 3);

            if (bolumButonlari[i] != null)
            {
                bolumButonlari[i].onClick.RemoveAllListeners();
                bolumButonlari[i].onClick.AddListener(() => BolumYukle(no));
            }
        }
    }

    void YildizGoster(Image[] dizi, int idx, bool dolu)
    {
        if (dizi == null || idx >= dizi.Length || dizi[idx] == null) return;
        dizi[idx].sprite = dolu ? yildizDolu : yildizBos;
        dizi[idx].color  = dolu ? Color.white : new Color(1f, 1f, 1f, 0.25f);
    }

    void BolumYukle(int no)
    {
        string sceneName = $"Level_{no:D2}";
        SceneManager.LoadScene(sceneName);
    }
}
