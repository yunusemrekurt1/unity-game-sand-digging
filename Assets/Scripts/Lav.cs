using UnityEngine;

/// <summary>
/// Lav / Ateş bölgesi. Top değer değmez yok olur.
/// Görsel olarak kum yüzeyine yerleştirilir (trigger).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Lav : MonoBehaviour
{
    void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var top = other.GetComponent<Top>();
        if (top == null) return;
        top.Yokol();
    }
}
