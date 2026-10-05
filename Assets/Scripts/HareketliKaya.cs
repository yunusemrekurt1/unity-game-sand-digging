using UnityEngine;

/// <summary>
/// Hareketli Kaya. Bölüm 9-10'da kullanılır.
/// İki nokta arasında ping-pong hareket eder. Normal Rigidbody2D (Kinematic)
/// olduğu için toplar fiziksel olarak çarpıp sekebilir.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HareketliKaya : MonoBehaviour
{
    [Header("Hareket")]
    public Vector2 yonVektoru = new Vector2(2f, 0f); // Saniyede kaç birim
    public float mesafe = 3f;                         // Toplam yer değiştirme

    private Vector2 m_Baslangic;
    private float m_T;
    private Rigidbody2D m_Rb;

    void Reset()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    void Start()
    {
        m_Baslangic = transform.position;
        m_Rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        m_T += Time.fixedDeltaTime;
        // Ping-pong: 0..1..0..
        float s = Mathf.PingPong(m_T * (yonVektoru.magnitude / Mathf.Max(0.01f, mesafe)), 1f);
        Vector2 hedef = m_Baslangic + yonVektoru.normalized * (s * mesafe);
        m_Rb.MovePosition(hedef);
    }
}
