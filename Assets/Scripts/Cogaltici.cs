using UnityEngine;

/// <summary>
/// Çoğaltıcı Bant. İçinden geçen her topu "carpan" kadar klonlar.
/// Örn: carpan = 3 ise -> 1 top girer, 3 top çıkar (2 yeni klonlanır).
/// Aynı top birden çok kez tetiklemesin diye her top için tek seferlik flag.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Cogaltici : MonoBehaviour
{
    [Header("Çarpan")]
    [Min(2)]
    public int carpan = 3;

    [Header("Klonlanan toplara uygulanacak rastgele hız (estetik)")]
    public Vector2 hizDagitimi = new Vector2(1.5f, 0.5f);

    [Header("Görsel (opsiyonel)")]
    public SpriteRenderer sr;

    // Her top yalnızca 1 kez çoğaltılsın
    private readonly System.Collections.Generic.HashSet<int> m_Islenen = new();

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.6f, 0.3f, 0.9f, 0.45f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Top top = other.GetComponent<Top>();
        if (top == null) return;

        int id = top.GetInstanceID();
        if (m_Islenen.Contains(id)) return;
        m_Islenen.Add(id);

        // (carpan - 1) tane yeni klon üret
        for (int i = 1; i < carpan; i++)
        {
            GameObject klon = Instantiate(top.gameObject, top.transform.position, Quaternion.identity);

            // Klon üzerinde tekrar tetiklenmemesi için bu Cogaltici'ye ait id eklemeye gerek yok,
            // çünkü klon yeni bir InstanceID'ye sahip. Aynı bölgede spawn olacağı için küçük bir
            // offset ve rastgele hız veriyoruz ki üst üste binmesin.
            klon.transform.position += (Vector3)(Random.insideUnitCircle * 0.15f);

            var rb = klon.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 rastgele = new Vector2(
                    Random.Range(-hizDagitimi.x, hizDagitimi.x),
                    -Mathf.Abs(Random.Range(0f, hizDagitimi.y))
                );
                rb.linearVelocity = top.GetComponent<Rigidbody2D>().linearVelocity + rastgele;
            }

            if (LevelManager.Instance != null)
                LevelManager.Instance.YeniTopEklendi();
        }
    }
}
