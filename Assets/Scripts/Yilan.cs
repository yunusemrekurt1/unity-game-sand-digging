using UnityEngine;

/// <summary>
/// Yılan. İki nokta arasında devriye gezer (patrolling).
/// Çarpan topu yok eder. Kinematic Rigidbody2D + Trigger collider.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Yilan : MonoBehaviour
{
    [Header("Devriye")]
    public float hiz = 2.5f;
    public float mesafe = 4f;
    public Vector2 yon = new Vector2(1f, 0f);

    [Header("Görsel")]
    public SpriteRenderer sr;

    private Vector2 m_Baslangic;
    private Rigidbody2D m_Rb;
    private float m_T;
    private int m_YonAcilimi = 1;

    void Reset()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        m_Baslangic = transform.position;
        m_Rb = GetComponent<Rigidbody2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        m_T += Time.fixedDeltaTime * hiz;
        float f = Mathf.PingPong(m_T, mesafe) - mesafe * 0.5f;
        Vector2 yeni = m_Baslangic + yon.normalized * f;

        // Yönünü flip et
        float delta = yeni.x - m_Rb.position.x;
        if (sr != null && Mathf.Abs(delta) > 0.001f)
        {
            int yeniYon = delta > 0 ? 1 : -1;
            if (yeniYon != m_YonAcilimi)
            {
                m_YonAcilimi = yeniYon;
                sr.flipX = (yeniYon < 0);
            }
        }

        m_Rb.MovePosition(yeni);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var top = other.GetComponent<Top>();
        if (top == null) return;
        top.Yokol();
    }
}
