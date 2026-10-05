using UnityEngine;

/// <summary>
/// Hazine (Top) objesine eklenir. Sadece basit referans/marker görevi görür.
/// Ölünce LevelManager'a haber verir, kasaya girince de kasaya haber verir.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Top : MonoBehaviour
{
    [Header("Görsel")]
    public SpriteRenderer sr;

    [Header("Renk (Bölüm 9-10 için)")]
    public TopRengi renk = TopRengi.Notr;

    void Reset()
    {
        sr = GetComponent<SpriteRenderer>();
        gameObject.tag = "Top";

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;

        var cc = GetComponent<CircleCollider2D>();
        cc.radius = 0.4f;
    }

    void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        RenkUygula();
    }

    public void RenkAyarla(TopRengi yeniRenk)
    {
        renk = yeniRenk;
        RenkUygula();
    }

    private void RenkUygula()
    {
        if (sr == null) return;
        switch (renk)
        {
            case TopRengi.Kirmizi: sr.color = new Color(0.95f, 0.25f, 0.25f); break;
            case TopRengi.Yesil:   sr.color = new Color(0.25f, 0.85f, 0.35f); break;
            case TopRengi.Mavi:    sr.color = new Color(0.25f, 0.55f, 0.95f); break;
            case TopRengi.Sari:    sr.color = new Color(0.98f, 0.85f, 0.25f); break;
            default:               sr.color = Color.white; break;
        }
    }

    /// <summary>
    /// Topu yok et ve LevelManager'a haber ver (diken/bomba için).
    /// </summary>
    public void Yokol()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.TopKaybedildi(this);
        Destroy(gameObject);
    }
}

public enum TopRengi
{
    Notr,
    Kirmizi,
    Yesil,
    Mavi,
    Sari
}
