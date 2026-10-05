#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Assets/Sprites/ altındaki tüm PNG'lere otomatik olarak doğru Sprite ayarlarını uygular:
///   * Texture Type: Sprite (2D and UI)
///   * Sprite Mode : Single
///   * Mesh Type   : Full Rect  (KumKazici için ŞART)
///   * Read/Write  : Enabled    (KumKazici için ŞART)
///   * Compression : None       (net kazma için)
///   * Filter Mode : Bilinear
///   * Pixels Per Unit: 256     (Kum için uygun)
/// Bu sayede kullanıcı hiçbir import ayarı yapmak zorunda kalmaz.
/// </summary>
public class SpriteImporterAyarlari : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Sadece Assets/Sprites/ altındakilere uygula
        if (!assetPath.Contains("/Assets/Sprites/") && !assetPath.StartsWith("Assets/Sprites/"))
            return;

        TextureImporter t = (TextureImporter)assetImporter;
        t.textureType        = TextureImporterType.Sprite;
        t.spriteImportMode   = SpriteImportMode.Single;
        t.spritePixelsPerUnit = 256f;
        t.isReadable         = true;                    // Read/Write Enabled
        t.textureCompression = TextureImporterCompression.Uncompressed;
        t.filterMode         = FilterMode.Bilinear;
        t.mipmapEnabled      = false;

        // Sprite Mesh Type = FullRect
        TextureImporterSettings s = new TextureImporterSettings();
        t.ReadTextureSettings(s);
        s.spriteMeshType = SpriteMeshType.FullRect;     // ŞART
        s.spriteExtrude  = 0;
        s.spriteGenerateFallbackPhysicsShape = true;    // physics shape üret
        t.SetTextureSettings(s);

        // kum.png için özel physics shape ayarı: alpha'dan şekil üret
        if (assetPath.EndsWith("/kum.png") || assetPath.EndsWith("\\kum.png"))
        {
            t.spritePixelsPerUnit = 100f;
        }
    }
}
#endif
