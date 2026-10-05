using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Oyun içi pause/restart/menu butonları. Her bölüm sahnesine otomatik eklenir.
/// ESC tuşuna basılınca da açılır (PC/WebGL).
/// </summary>
public class OyunIciMenu : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject pausePanel;
    public Button pauseButon;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButon != null)
        {
            pauseButon.onClick.RemoveAllListeners();
            pauseButon.onClick.AddListener(Duraklat);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null && pausePanel.activeSelf) Devam();
            else Duraklat();
        }
    }

    public void Duraklat()
    {
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Devam()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void BtnRestart()
    {
        Time.timeScale = 1f;
        if (LevelManager.Instance != null) LevelManager.Instance.BolumuYenidenBaslat();
    }

    public void BtnMenu()
    {
        Time.timeScale = 1f;
        if (LevelManager.Instance != null) LevelManager.Instance.AnaMenuyeDon();
    }
}
