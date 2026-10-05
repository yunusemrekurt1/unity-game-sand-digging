using UnityEngine;

/// <summary>
/// Su Alanı. İçine giren topun gravityScale'ini ve linearDamping'ini
/// değiştirerek "suda yüzme" hissi verir. Çıkınca orijinal değerlere döndürür.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Su : MonoBehaviour
{
    [Header("Fizik")]
    [Range(0f, 3f)] public float suIcindeGravity = 0.6f;
    [Range(0f, 10f)] public float suDrag = 3f;

    private readonly System.Collections.Generic.Dictionary<int, (float g, float d)> m_Orj = new();

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        if (other.GetComponent<Top>() == null) return;
        int id = rb.GetInstanceID();
        if (m_Orj.ContainsKey(id)) return;
        m_Orj[id] = (rb.gravityScale, rb.linearDamping);
        rb.gravityScale  = suIcindeGravity;
        rb.linearDamping = suDrag;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        int id = rb.GetInstanceID();
        if (!m_Orj.TryGetValue(id, out var orj)) return;
        rb.gravityScale  = orj.g;
        rb.linearDamping = orj.d;
        m_Orj.Remove(id);
    }
}
