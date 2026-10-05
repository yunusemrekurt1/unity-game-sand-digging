using UnityEngine;

/// <summary>
/// Diken veya Bomba.
///   patla=false: sadece topu yok eder (klasik diken).
///   patla=true : Top değer değmez patlar, etrafındaki kumu DAİRESEL olarak
///                temizler (krater açar), patlama yarıçapındaki tüm topları yok eder.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Diken : MonoBehaviour
{
    [Header("Davranış")]
    public bool patla = false;
    public bool kendiniDeYokEt = false;

    [Header("Bomba Ayarları (patla=true iken)")]
    [Tooltip("Kumda açılacak krater yarıçapı (dünya birimi).")]
    public float kraterYariCapi = 1.4f;
    [Tooltip("Bu yarıçaptaki tüm toplar imha edilir.")]
    public float topImhaYariCapi = 1.8f;

    [Header("Efekt (opsiyonel)")]
    public GameObject patlamaEfektiPrefab;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Top top = other.GetComponent<Top>();
        if (top == null) return;

        if (!patla)
        {
            top.Yokol();
            return;
        }

        // --- BOMBA PATLAMA ---
        Vector3 merkez = transform.position;

        // 1) Kumda krater aç
        var kum = FindAnyObjectByType<KumKazici>();
        if (kum != null)
            kum.KumaKraterAc(merkez, kraterYariCapi);

        // 2) Efekt
        if (patlamaEfektiPrefab != null)
            Instantiate(patlamaEfektiPrefab, merkez, Quaternion.identity);

        // 3) Yarıçaptaki tüm topları yok et
        var toplar = GameObject.FindGameObjectsWithTag("Top");
        foreach (var t in toplar)
        {
            if (t == null) continue;
            float d = Vector2.Distance(t.transform.position, merkez);
            if (d <= topImhaYariCapi)
            {
                var tt = t.GetComponent<Top>();
                if (tt != null) tt.Yokol();
            }
        }

        if (kendiniDeYokEt)
            Destroy(gameObject);
    }
}
