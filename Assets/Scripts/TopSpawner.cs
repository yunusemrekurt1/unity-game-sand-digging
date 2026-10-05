using System.Collections;
using UnityEngine;

/// <summary>
/// Opsiyonel: Belli aralıklarla belli bir noktadan top spawn eder.
/// Bölüm tasarımında "topları manuel sahneye koyma" yerine bunu kullanabilirsiniz.
/// </summary>
public class TopSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject topPrefab;
    public int topSayisi = 5;
    public float araSure = 0.5f;

    [Header("Renk Dağılımı (Bölüm 9-10)")]
    public bool rasgeleRenkVer = false;
    public TopRengi[] renkler = new[] { TopRengi.Kirmizi, TopRengi.Yesil };

    void Start()
    {
        StartCoroutine(SpawnRutini());
    }

    IEnumerator SpawnRutini()
    {
        for (int i = 0; i < topSayisi; i++)
        {
            GameObject go = Instantiate(topPrefab, transform.position, Quaternion.identity);
            if (rasgeleRenkVer && renkler != null && renkler.Length > 0)
            {
                var top = go.GetComponent<Top>();
                if (top != null)
                    top.RenkAyarla(renkler[Random.Range(0, renkler.Length)]);
            }

            if (LevelManager.Instance != null)
                LevelManager.Instance.YeniTopEklendi();

            yield return new WaitForSeconds(araSure);
        }
    }
}
