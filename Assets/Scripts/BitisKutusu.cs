using System.Collections;
using UnityEngine;

/// <summary>
/// Bitiş Kutusu. Top içeri girince:
///   * Kutuya doğru Vortex çekim (manyetik) uygulanır.
///   * Top kutu merkezine ulaşırken KÜÇÜLEREK yok olur (scale-down animasyon).
///   * Boşluğa serbest düşme görüntüsü engellenir.
///
/// Renk kuralları Bölüm 9-10 için geçerli (kabulEdilenRenk != Notr).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BitisKutusu : MonoBehaviour
{
    [Header("Renk Eşleşmesi (opsiyonel)")]
    public TopRengi kabulEdilenRenk = TopRengi.Notr;

    [Header("Vortex / Animasyon")]
    [Tooltip("Top kutu merkezine çekilirken animasyon süresi.")]
    public float cekimSuresi = 0.45f;

    [Header("Görsel")]
    public SpriteRenderer sr;

    // Aynı topu iki kez işlememek için
    private readonly System.Collections.Generic.HashSet<int> m_Isleniyor = new();

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        RenkUygula();
    }

    private void RenkUygula()
    {
        if (sr == null) return;
        switch (kabulEdilenRenk)
        {
            case TopRengi.Kirmizi: sr.color = new Color(0.95f, 0.25f, 0.25f); break;
            case TopRengi.Yesil:   sr.color = new Color(0.25f, 0.85f, 0.35f); break;
            case TopRengi.Mavi:    sr.color = new Color(0.25f, 0.55f, 0.95f); break;
            case TopRengi.Sari:    sr.color = new Color(0.98f, 0.85f, 0.25f); break;
            default:               sr.color = new Color(0.8f, 0.7f, 0.4f); break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Top top = other.GetComponent<Top>();
        if (top == null) return;

        int id = top.GetInstanceID();
        if (m_Isleniyor.Contains(id)) return;
        m_Isleniyor.Add(id);

        // Yanlış renk - topu yok et (skor değil, kayıp)
        if (kabulEdilenRenk != TopRengi.Notr && top.renk != kabulEdilenRenk)
        {
            top.Yokol();
            return;
        }

        StartCoroutine(VortexAnimasyonu(top));
    }

    IEnumerator VortexAnimasyonu(Top top)
    {
        // Fizik devre dışı
        var rb  = top.GetComponent<Rigidbody2D>();
        var col = top.GetComponent<Collider2D>();
        if (rb != null)
        {
            rb.linearVelocity   = Vector2.zero;
            rb.angularVelocity  = 0f;
            rb.gravityScale     = 0f;
            rb.bodyType         = RigidbodyType2D.Kinematic;
        }
        if (col != null) col.enabled = false;

        Vector3 baslangicPos  = top.transform.position;
        Vector3 baslangicSca  = top.transform.localScale;
        Vector3 hedef         = transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.05f, cekimSuresi);
            float easeIn = t * t; // ease-in quadratic
            top.transform.position   = Vector3.Lerp(baslangicPos, hedef, easeIn);
            top.transform.localScale = Vector3.Lerp(baslangicSca, Vector3.zero, t);
            top.transform.Rotate(0f, 0f, 720f * Time.deltaTime); // dönerek emme efekti
            yield return null;
        }

        // Puan kaydet
        if (LevelManager.Instance != null)
            LevelManager.Instance.TopUlastirildi(top);

        Destroy(top.gameObject);
    }
}
