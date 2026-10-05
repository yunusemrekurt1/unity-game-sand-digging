using System.Collections;
using UnityEngine;

/// <summary>
/// Sahnedeki tüm topları kademeli olarak spawn etmek yerine, bölüm başında
/// zaten yerleştirilmiş topları kısa aralıklarla "drop" ettirir.
/// Sahne Start'ında tüm topları toplar, Rigidbody'lerini ilk başta donmuş
/// yapar, sonra 0.15s aralıkla aktif eder. Dramatic entrance efekti.
/// </summary>
public class TopDropZamanlayici : MonoBehaviour
{
    public float ilkBeklemeSuresi = 0.3f;
    public float araSure = 0.15f;

    void Start()
    {
        var toplar = GameObject.FindGameObjectsWithTag("Top");
        // Rigidbody'leri başlangıçta dondur
        foreach (var t in toplar)
        {
            var rb = t.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false; // fizik durdu
            }
        }
        StartCoroutine(Drop(toplar));
    }

    IEnumerator Drop(GameObject[] toplar)
    {
        yield return new WaitForSeconds(ilkBeklemeSuresi);
        // Y'ye göre yukarıdan aşağı sırala ki yukaridakiler önce düşsün
        System.Array.Sort(toplar, (a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        foreach (var t in toplar)
        {
            if (t == null) continue;
            var rb = t.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = true;
            yield return new WaitForSeconds(araSure);
        }
    }
}
