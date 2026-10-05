#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ana Menü + 10 bölümü tek tıkla üretir. 5× daha derin haritalar, 9 top/bölüm,
/// yıldız sistemi, yeni tuzaklar (Su, Lav, Yılan, Krater Bomba, Vortex kasa).
///
/// Kullanım: Tools → Bolumleri Uret → Hepsini Uret
/// </summary>
public static class LevelGenerator
{
    const string SPR = "Assets/Sprites/";
    const string PRE = "Assets/Prefabs/";
    const string SCN = "Assets/Scenes/";

    const int TOP_SAYISI = 9;

    static readonly string[] GerekliTagler = { "Top", "Kaya", "Diken", "Kasa", "Cogaltici", "Yilan" };

    [MenuItem("Tools/Bolumleri Uret/Hepsini Uret")]
    public static void HepsiniUret()
    {
        // Play mode'da çalışamaz
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Play mode'da çalışmaz!",
                "Önce Play (▶) tuşuna basıp oyunu durdur.\n\n" +
                "Sonra tekrar Tools → Bolumleri Uret → Hepsini Uret tıkla.",
                "Tamam");
            return;
        }

        TagleriEkle();
        Directory.CreateDirectory(PRE);
        Directory.CreateDirectory(SCN);
        PrefabUretTumu();

        var sahneler = new List<EditorBuildSettingsScene>();

        // Index 0 = Ana Menü
        string anaPath = AnaMenuUret();
        sahneler.Add(new EditorBuildSettingsScene(anaPath, true));

        // Index 1-10 = Bölümler
        for (int i = 1; i <= 10; i++)
        {
            string path = BolumUret(i);
            sahneler.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = sahneler.ToArray();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(anaPath);

        EditorUtility.DisplayDialog(
            "Başarılı!",
            "1 Ana Menü + 10 bölüm oluşturuldu.\n\n" +
            "Her bölümde 9 top var. 3/6/9 ulaştırırsan 1/2/3 yıldız alırsın.\n\n" +
            "Sağ üstteki ▶ Play tuşuna bas — ana menüden bölüm seç.",
            "Harika!"
        );
    }

    [MenuItem("Tools/Bolumleri Uret/Yildizlari Sifirla")]
    public static void YildizlariSifirla()
    {
        for (int i = 1; i <= 10; i++)
            PlayerPrefs.DeleteKey($"Stars_Level_{i:D2}");
        PlayerPrefs.Save();
        EditorUtility.DisplayDialog("Sıfırlandı", "Tüm yıldızlar silindi.", "Tamam");
    }

    // ==================================================================
    //                            TAG
    // ==================================================================
    static void TagleriEkle()
    {
        var tm = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        foreach (var yeni in GerekliTagler)
        {
            bool var = false;
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == yeni) { var = true; break; }
            if (!var)
            {
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = yeni;
            }
        }
        tm.ApplyModifiedProperties();
    }

    // ==================================================================
    //                          PREFAB
    // ==================================================================
    static Sprite Spr(string ad) => AssetDatabase.LoadAssetAtPath<Sprite>(SPR + ad + ".png");

    static void PrefabUretTumu()
    {
        Uret_Top(); Uret_Kaya(); Uret_Diken(); Uret_Bomba(); Uret_Kasa();
        Uret_Cogaltici(); Uret_HareketliKaya();
        Uret_Su(); Uret_Lav(); Uret_Yilan();
    }

    static void Uret_Top()
    {
        var go = new GameObject("Top"); go.tag = "Top";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("top"); sr.sortingOrder = 10;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var cc = go.AddComponent<CircleCollider2D>(); cc.radius = 0.45f;
        var t = go.AddComponent<Top>(); t.sr = sr;
        go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Top.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Kaya()
    {
        var go = new GameObject("Kaya"); go.tag = "Kaya";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("kaya"); sr.sortingOrder = 8;
        var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Static;
        go.AddComponent<PolygonCollider2D>();
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Kaya.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Diken()
    {
        var go = new GameObject("Diken"); go.tag = "Diken";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("diken"); sr.sortingOrder = 9;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        var d = go.AddComponent<Diken>(); d.patla = false;
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Diken.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Bomba()
    {
        var go = new GameObject("Bomba"); go.tag = "Diken";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("bomba"); sr.sortingOrder = 9;
        var c = go.AddComponent<CircleCollider2D>(); c.isTrigger = true; c.radius = 0.6f;
        var d = go.AddComponent<Diken>();
        d.patla = true; d.kendiniDeYokEt = true;
        d.kraterYariCapi = 1.8f; d.topImhaYariCapi = 2.2f;
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Bomba.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Kasa()
    {
        var go = new GameObject("Kasa"); go.tag = "Kasa";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("kasa"); sr.sortingOrder = 7;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        var k = go.AddComponent<BitisKutusu>();
        k.sr = sr; k.kabulEdilenRenk = TopRengi.Notr;
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Kasa.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Cogaltici()
    {
        var go = new GameObject("Cogaltici"); go.tag = "Cogaltici";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("cogaltici"); sr.sortingOrder = 6;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        var co = go.AddComponent<Cogaltici>();
        co.sr = sr; co.carpan = 3;
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Cogaltici.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_HareketliKaya()
    {
        var go = new GameObject("HareketliKaya"); go.tag = "Kaya";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("kaya");
        sr.color = new Color(0.7f, 0.7f, 0.85f); sr.sortingOrder = 8;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f;
        go.AddComponent<PolygonCollider2D>();
        go.AddComponent<HareketliKaya>();
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "HareketliKaya.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Su()
    {
        var go = new GameObject("Su");
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("su"); sr.sortingOrder = 5;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<Su>();
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Su.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Lav()
    {
        var go = new GameObject("Lav"); go.tag = "Diken";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("lav"); sr.sortingOrder = 9;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<Lav>();
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Lav.prefab");
        Object.DestroyImmediate(go);
    }

    static void Uret_Yilan()
    {
        var go = new GameObject("Yilan"); go.tag = "Yilan";
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("yilan"); sr.sortingOrder = 9;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        var y = go.AddComponent<Yilan>();
        y.sr = sr; y.hiz = 2.5f; y.mesafe = 5f;
        go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        PrefabUtility.SaveAsPrefabAsset(go, PRE + "Yilan.prefab");
        Object.DestroyImmediate(go);
    }

    // ==================================================================
    //                         ANA MENÜ SAHNESİ
    // ==================================================================
    static string AnaMenuUret()
    {
        var sahne = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Kamera
        var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.12f, 0.15f, 0.3f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 0, -10);

        // Canvas
        var cv = YeniCanvas(out var es, out var gr);
        var menu = cv.AddComponent<AnaMenu>();

        var yildizDolu = Spr("yildiz");
        menu.yildizDolu = yildizDolu;
        menu.yildizBos = yildizDolu;

        // === Ana Panel ===
        var anaPanel = new GameObject("AnaPanel");
        anaPanel.transform.SetParent(cv.transform, false);
        anaPanel.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        FullScreenRect(anaPanel);
        var anaImg = anaPanel.AddComponent<Image>();
        anaImg.color = new Color(0.08f, 0.11f, 0.25f, 1f);
        menu.anaPanel = anaPanel;

        var title = YeniText(anaPanel.transform, "KumKral", 130, Color.white,
                             new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.85f));

        var altyazi = YeniText(anaPanel.transform, "Kum Kazma Macerasi", 52,
                               new Color(1f, 0.85f, 0.4f),
                               new Vector2(0.15f, 0.58f), new Vector2(0.85f, 0.66f));

        var oynaBtn = YeniButton(anaPanel.transform, "OYNA", 72,
                                 new Vector2(0.25f, 0.38f), new Vector2(0.75f, 0.48f),
                                 new Color(0.9f, 0.75f, 0.2f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(oynaBtn.onClick, menu.BtnOyna);

        var cikisBtn = YeniButton(anaPanel.transform, "CIKIS", 48,
                                  new Vector2(0.3f, 0.26f), new Vector2(0.7f, 0.33f),
                                  new Color(0.3f, 0.35f, 0.5f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(cikisBtn.onClick, menu.BtnCikis);

        var toplamYildizTxt = YeniText(anaPanel.transform, "Toplam ★: 0/30", 36,
                                       new Color(1f, 0.9f, 0.5f),
                                       new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.17f));
        menu.toplamYildizText = toplamYildizTxt.GetComponent<Text>();

        // === Bölüm Seçme Panel ===
        var bsPanel = new GameObject("BolumSecmePanel");
        bsPanel.transform.SetParent(cv.transform, false);
        bsPanel.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        FullScreenRect(bsPanel);
        var bsImg = bsPanel.AddComponent<Image>();
        bsImg.color = new Color(0.08f, 0.11f, 0.25f, 1f);
        menu.bolumSecmePanel = bsPanel;

        YeniText(bsPanel.transform, "BOLUM SEC", 80, Color.white,
                 new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.98f));

        var geriBtn = YeniButton(bsPanel.transform, "< GERI", 42,
                                 new Vector2(0.03f, 0.91f), new Vector2(0.22f, 0.98f),
                                 new Color(0.25f, 0.3f, 0.45f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(geriBtn.onClick, menu.BtnGeri);

        // 10 bölüm butonu - grid 2x5
        menu.bolumButonlari = new Button[10];
        menu.yildizIkonlari1 = new Image[10];
        menu.yildizIkonlari2 = new Image[10];
        menu.yildizIkonlari3 = new Image[10];

        float startY = 0.78f;
        float btnH = 0.12f;
        float gap = 0.015f;

        for (int i = 0; i < 10; i++)
        {
            int col = i % 2;
            int row = i / 2;
            float x0 = 0.08f + col * 0.47f;
            float x1 = x0 + 0.37f;
            float y1 = startY - row * (btnH + gap);
            float y0 = y1 - btnH;

            var btnGO = new GameObject($"Bolum_{i + 1:D2}");
            btnGO.transform.SetParent(bsPanel.transform, false);
            btnGO.AddComponent<RectTransform>(); // TAMİR EDİLDİ
            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.4f);
            var btn = btnGO.AddComponent<Button>();
            Anchor(btnGO, x0, y0, x1, y1);

            var numTxt = YeniText(btnGO.transform, $"{i + 1}", 60, Color.white,
                                  new Vector2(0, 0.4f), new Vector2(0.35f, 0.95f));

            // 3 yıldız ikonu (kuru)
            for (int s = 0; s < 3; s++)
            {
                var yGO = new GameObject($"Yildiz_{s}");
                yGO.transform.SetParent(btnGO.transform, false);
                yGO.AddComponent<RectTransform>(); // TAMİR EDİLDİ
                var yImg = yGO.AddComponent<Image>();
                yImg.sprite = yildizDolu;
                yImg.color = new Color(1, 1, 1, 0.25f);
                Anchor(yGO, 0.35f + s * 0.2f, 0.2f, 0.55f + s * 0.2f, 0.8f);
                if (s == 0) menu.yildizIkonlari1[i] = yImg;
                if (s == 1) menu.yildizIkonlari2[i] = yImg;
                if (s == 2) menu.yildizIkonlari3[i] = yImg;
            }

            menu.bolumButonlari[i] = btn;
        }

        bsPanel.SetActive(false);

        string path = SCN + "Level_00_AnaMenu.unity";
        EditorSceneManager.SaveScene(sahne, path);
        return path;
    }

    // ==================================================================
    //                       YARDIMCI UI FONKSIYONLARI
    // ==================================================================
    static GameObject YeniCanvas(out GameObject es, out GraphicRaycaster gr)
    {
        var cv = new GameObject("Canvas");
        var c = cv.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = cv.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        gr = cv.AddComponent<GraphicRaycaster>();
        es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        return cv;
    }

    static void FullScreenRect(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Anchor(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static GameObject YeniText(Transform parent, string yazi, int fontSize, Color renk,
                               Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        var t = go.AddComponent<Text>();
        t.text = yazi;
        t.fontSize = fontSize;
        t.color = renk;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    static Button YeniButton(Transform parent, string yazi, int fontSize,
                             Vector2 aMin, Vector2 aMax, Color renk)
    {
        var go = new GameObject("Button_" + yazi);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        var img = go.AddComponent<Image>(); img.color = renk;
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var txt = new GameObject("Text");
        txt.transform.SetParent(go.transform, false);
        txt.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        var t = txt.AddComponent<Text>();
        t.text = yazi; t.fontSize = fontSize; t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        return btn;
    }

    // ==================================================================
    //                         BÖLÜM SAHNESİ
    // ==================================================================
    static string BolumUret(int no)
    {
        var sahne = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Kamera
        var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.4f, 0.65f, 0.85f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 0, -10);
        var takip = camGO.AddComponent<KameraTakip>();
        takip.lerpHizi = 4f; takip.topTag = "Top";

        // LevelManager
        var gm = new GameObject("_GameManager");
        var lm = gm.AddComponent<LevelManager>();
        gm.AddComponent<TopDropZamanlayici>();

        // UI
        BolumUI(out var ui);

        // Kum - 5× daha derin (30..75 unit)
        float derinlik = Mathf.Lerp(30f, 75f, (no - 1) / 9f);
        KumAnaOlustur(new Vector3(0f, -derinlik * 0.5f + 2f, 0f),
                      new Vector3(1.4f, derinlik / 5.12f, 1f));

        // İçerik
        BolumIcerik(no, lm, derinlik);

        // Dünya sınırları (yan duvarlar, toplar uçmasın)
        YanDuvar(new Vector3(-4.2f, -derinlik * 0.5f, 0), new Vector2(0.4f, derinlik + 20));
        YanDuvar(new Vector3(4.2f, -derinlik * 0.5f, 0), new Vector2(0.4f, derinlik + 20));

        string path = SCN + $"Level_{no:D2}.unity";
        EditorSceneManager.SaveScene(sahne, path);
        return path;
    }

    static void YanDuvar(Vector3 pos, Vector2 size)
    {
        var go = new GameObject("YanDuvar");
        go.transform.position = pos;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    static GameObject KumAnaOlustur(Vector3 pos, Vector3 scale)
    {
        var go = new GameObject("KumAna");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("kum"); sr.drawMode = SpriteDrawMode.Simple; sr.sortingOrder = 0;
        var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Static;
        var cc = go.AddComponent<CompositeCollider2D>();
        cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
        var p = go.AddComponent<PolygonCollider2D>();
        p.compositeOperation = Collider2D.CompositeOperation.Merge;
        go.AddComponent<KumKazici>();
        go.transform.position = pos;
        go.transform.localScale = scale;
        return go;
    }

    static void BolumUI(out UIYonetici script)
    {
        var cv = YeniCanvas(out _, out _);
        script = cv.AddComponent<UIYonetici>();
        var pm = cv.AddComponent<OyunIciMenu>();

        // HUD üst sol/sağ
        var hedef = YeniText(cv.transform, "★ 0/9 (0 yildiz)", 46, Color.white,
                             new Vector2(0.02f, 0.93f), new Vector2(0.55f, 0.99f));
        hedef.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
        script.hedefText = hedef.GetComponent<Text>();

        var durum = YeniText(cv.transform, "Kayip: 0/0", 42, new Color(1f, 0.8f, 0.8f),
                             new Vector2(0.55f, 0.93f), new Vector2(0.88f, 0.99f));
        durum.GetComponent<Text>().alignment = TextAnchor.UpperRight;
        script.durumText = durum.GetComponent<Text>();

        // Pause butonu (sağ üst köşe)
        var pauseBtn = YeniButton(cv.transform, "II", 54,
                                  new Vector2(0.88f, 0.92f), new Vector2(0.98f, 0.99f),
                                  new Color(0.1f, 0.15f, 0.3f, 0.85f));
        pm.pauseButon = pauseBtn;

        // Pause Panel
        pm.pausePanel = PausePanel(cv.transform, pm);

        // Bölüm başlığı (intro kartı)
        BolumBasligiOlustur(cv.transform);

        // Kazandın paneli
        script.kazandinPanel = KazanPanel(cv.transform, script, out script.yildizSlots);
        script.yildizSprite = Spr("yildiz");

        // Kaybettin paneli
        script.kaybettinPanel = KaybetPanel(cv.transform, script);
    }

    static GameObject PausePanel(Transform parent, OyunIciMenu pm)
    {
        var p = new GameObject("PausePanel");
        p.transform.SetParent(parent, false);
        p.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        FullScreenRect(p);
        var bg = p.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.08f, 0.2f, 0.92f);

        YeniText(p.transform, "DURAKLATILDI", 96, Color.white,
                 new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.8f));

        var devam = YeniButton(p.transform, "DEVAM ET", 56,
                               new Vector2(0.2f, 0.48f), new Vector2(0.8f, 0.58f),
                               new Color(0.2f, 0.7f, 0.35f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(devam.onClick, pm.Devam);

        var restart = YeniButton(p.transform, "YENIDEN BASLA", 48,
                                 new Vector2(0.2f, 0.36f), new Vector2(0.8f, 0.46f),
                                 new Color(0.9f, 0.55f, 0.2f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restart.onClick, pm.BtnRestart);

        var menu = YeniButton(p.transform, "ANA MENU", 48,
                              new Vector2(0.2f, 0.24f), new Vector2(0.8f, 0.34f),
                              new Color(0.35f, 0.4f, 0.55f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menu.onClick, pm.BtnMenu);

        p.SetActive(false);
        return p;
    }

    static GameObject BolumBasligiOlustur(Transform parent)
    {
        var go = new GameObject("BolumBasligi");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        FullScreenRect(go);
        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;

        // Arka plan (yarı saydam)
        var bg = new GameObject("BG");
        bg.transform.SetParent(go.transform, false);
        bg.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        FullScreenRect(bg);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.55f);

        var baslik = YeniText(go.transform, "BOLUM 1", 180, Color.white,
                              new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.62f));
        var altbaslik = YeniText(go.transform, "Alistirma", 56, new Color(1f, 0.9f, 0.4f),
                                 new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.44f));

        var sc = go.AddComponent<BolumBasligi>();
        sc.cg = cg;
        sc.baslik = baslik.GetComponent<Text>();
        sc.altbaslik = altbaslik.GetComponent<Text>();

        return go;
    }

    static GameObject KazanPanel(Transform parent, UIYonetici script, out Image[] slots)
    {
        var p = new GameObject("KazandinPanel");
        p.transform.SetParent(parent, false);
        p.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        FullScreenRect(p);
        var bg = p.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.5f, 0.3f, 0.88f);

        YeniText(p.transform, "KAZANDIN!", 120, Color.white,
                 new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.88f));

        // 3 yıldız slotu
        slots = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var y = new GameObject($"Yildiz_{i}");
            y.transform.SetParent(p.transform, false);
            y.AddComponent<RectTransform>(); // TAMİR EDİLDİ
            var img = y.AddComponent<Image>();
            img.sprite = Spr("yildiz");
            img.color = new Color(1, 1, 1, 0);
            Anchor(y, 0.15f + i * 0.24f, 0.45f, 0.35f + i * 0.24f, 0.65f);
            slots[i] = img;
        }

        var tekrar = YeniButton(p.transform, "TEKRAR", 52,
                                new Vector2(0.1f, 0.2f), new Vector2(0.4f, 0.3f),
                                new Color(0.3f, 0.35f, 0.5f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(tekrar.onClick, script.BtnTekrar);

        var sonraki = YeniButton(p.transform, "SONRAKI", 52,
                                 new Vector2(0.43f, 0.2f), new Vector2(0.73f, 0.3f),
                                 new Color(0.2f, 0.7f, 0.3f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(sonraki.onClick, script.BtnSonraki);

        var menu = YeniButton(p.transform, "MENU", 52,
                              new Vector2(0.76f, 0.2f), new Vector2(0.93f, 0.3f),
                              new Color(0.25f, 0.3f, 0.45f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menu.onClick, script.BtnAnaMenu);

        p.SetActive(false);
        return p;
    }

    static GameObject KaybetPanel(Transform parent, UIYonetici script)
    {
        var p = new GameObject("KaybettinPanel");
        p.transform.SetParent(parent, false);
        p.AddComponent<RectTransform>(); // TAMİR EDİLDİ
        FullScreenRect(p);
        var bg = p.AddComponent<Image>();
        bg.color = new Color(0.5f, 0.1f, 0.1f, 0.88f);

        YeniText(p.transform, "KAYBETTIN", 120, Color.white,
                 new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f));

        var tekrar = YeniButton(p.transform, "TEKRAR", 60,
                                new Vector2(0.15f, 0.3f), new Vector2(0.55f, 0.42f),
                                new Color(0.9f, 0.5f, 0.2f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(tekrar.onClick, script.BtnTekrar);

        var menu = YeniButton(p.transform, "MENU", 60,
                              new Vector2(0.6f, 0.3f), new Vector2(0.85f, 0.42f),
                              new Color(0.25f, 0.3f, 0.45f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menu.onClick, script.BtnAnaMenu);

        p.SetActive(false);
        return p;
    }

    // ==================================================================
    //                      BÖLÜM İÇERİK TASARIMI
    // ==================================================================
    static GameObject PI(string ad, Vector3 pos)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PRE + ad + ".prefab");
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.position = pos;
        return inst;
    }

    static void TopKur(float x, float y, TopRengi renk = TopRengi.Notr)
    {
        var go = PI("Top", new Vector3(x, y, 0));
        go.GetComponent<Top>().RenkAyarla(renk);
    }

    static void DokuzTop(float topY, TopRengi renk = TopRengi.Notr, float yRandom = 0f)
    {
        // 9 topu üstte 3×3 grid gibi ser
        for (int i = 0; i < 9; i++)
        {
            float x = -3f + (i % 3) * 3f;
            float y = topY + (i / 3) * 0.8f + (yRandom > 0 ? Random.Range(0f, yRandom) : 0f);
            TopKur(x, y, renk);
        }
    }

    static void KarmaTop(float topY)
    {
        // 5 kırmızı + 4 yeşil
        for (int i = 0; i < 5; i++) TopKur(-3f + i * 1.2f, topY, TopRengi.Kirmizi);
        for (int i = 0; i < 4; i++) TopKur(-2.4f + i * 1.2f, topY + 1f, TopRengi.Yesil);
    }

    static void Kasa(float y, float xOffset = 0f, TopRengi renk = TopRengi.Notr, float genislik = 2f)
    {
        var k = PI("Kasa", new Vector3(xOffset, y, 0));
        k.transform.localScale = new Vector3(genislik, 1.2f, 1f);
        var bk = k.GetComponent<BitisKutusu>();
        bk.kabulEdilenRenk = renk;
        if (renk == TopRengi.Kirmizi) bk.sr.color = new Color(0.95f, 0.25f, 0.25f);
        else if (renk == TopRengi.Yesil) bk.sr.color = new Color(0.25f, 0.85f, 0.35f);
    }

    static void BolumIcerik(int no, LevelManager lm, float derinlik)
    {
        float kasaY = -derinlik + 1.5f;
        float topY = 4.5f;

        lm.hedefTopSayisi = 3; // minimum 3 ulaştırılırsa 1 yıldız

        switch (no)
        {
            case 1:
                DokuzTop(topY);
                // Basit engeller
                PI("Kaya", new Vector3(-1.5f, -8f, 0)).transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                PI("Kaya", new Vector3(1.8f, -18f, 0)).transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                Kasa(kasaY, 0, TopRengi.Notr, 2.5f);
                break;

            case 2:
                DokuzTop(topY);
                PI("Kaya", new Vector3(-2f, -6f, 0)).transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                PI("Kaya", new Vector3(2f, -14f, 0)).transform.localScale = new Vector3(0.8f, 0.7f, 1f);
                PI("Kaya", new Vector3(-1f, -22f, 0)).transform.localScale = new Vector3(0.7f, 0.8f, 1f);
                Kasa(kasaY, 0, TopRengi.Notr, 2.5f);
                break;

            case 3:
                DokuzTop(topY);
                // Kaya labirenti
                for (int i = 0; i < 8; i++)
                {
                    float y = -5 - i * 3.5f;
                    float x = (i % 2 == 0) ? -1.5f : 1.5f;
                    PI("Kaya", new Vector3(x, y, 0)).transform.localScale = new Vector3(0.9f, 0.7f, 1f);
                }
                Kasa(kasaY, 0, TopRengi.Notr, 2.2f);
                break;

            case 4:
                DokuzTop(topY);
                lm.maksimumKayip = 6;
                // Su havuzları arasında kayalar
                for (int i = 0; i < 3; i++)
                {
                    var su = PI("Su", new Vector3((i % 2 == 0) ? -1.8f : 1.8f, -7 - i * 9f, 0));
                    su.transform.localScale = new Vector3(0.022f, 0.022f, 1f);
                }
                PI("Kaya", new Vector3(0, -5f, 0)).transform.localScale = new Vector3(0.8f, 0.6f, 1f);
                PI("Kaya", new Vector3(-2f, -20f, 0)).transform.localScale = new Vector3(0.8f, 0.6f, 1f);
                PI("Kaya", new Vector3(2f, -30f, 0)).transform.localScale = new Vector3(0.8f, 0.6f, 1f);
                Kasa(kasaY, 0, TopRengi.Notr, 2.5f);
                break;

            case 5:
                DokuzTop(topY);
                lm.maksimumKayip = 6;
                // Dikenler + su
                PI("Diken", new Vector3(-2.5f, -6f, 0)).transform.localScale = new Vector3(1.2f, 1f, 1f);
                PI("Diken", new Vector3(2.5f, -12f, 0)).transform.localScale = new Vector3(1.2f, 1f, 1f);
                PI("Diken", new Vector3(0f, -20f, 0)).transform.localScale = new Vector3(1.3f, 1f, 1f);
                PI("Diken", new Vector3(-2f, -32f, 0)).transform.localScale = new Vector3(1.2f, 1f, 1f);
                var su5 = PI("Su", new Vector3(1.5f, -25f, 0)); su5.transform.localScale = new Vector3(0.022f, 0.022f, 1f);
                PI("Kaya", new Vector3(-1f, -8f, 0)).transform.localScale = new Vector3(0.7f, 0.6f, 1f);
                Kasa(kasaY, 0, TopRengi.Notr, 2.5f);
                break;

            case 6:
                DokuzTop(topY);
                lm.maksimumKayip = 5;
                // Krater bombalar
                PI("Bomba", new Vector3(-1.5f, -7f, 0));
                PI("Bomba", new Vector3(2f, -14f, 0));
                PI("Bomba", new Vector3(-1f, -23f, 0));
                PI("Bomba", new Vector3(1.5f, -32f, 0));
                PI("Kaya", new Vector3(-2.5f, -10f, 0)).transform.localScale = new Vector3(0.8f, 0.6f, 1f);
                PI("Kaya", new Vector3(2.5f, -25f, 0)).transform.localScale = new Vector3(0.8f, 0.6f, 1f);
                Kasa(kasaY, 0, TopRengi.Notr, 2.5f);
                break;

            case 7:
                DokuzTop(topY);
                lm.maksimumKayip = 6;
                // Çoğaltıcı + dikenler
                var cog = PI("Cogaltici", new Vector3(0, -10f, 0));
                cog.transform.localScale = new Vector3(2.2f, 1.8f, 1f);
                cog.GetComponent<Cogaltici>().carpan = 3;
                PI("Diken", new Vector3(-2.5f, -20f, 0)).transform.localScale = new Vector3(1.2f, 1f, 1f);
                PI("Diken", new Vector3(2.5f, -30f, 0)).transform.localScale = new Vector3(1.2f, 1f, 1f);
                PI("Kaya", new Vector3(-1.5f, -35f, 0)).transform.localScale = new Vector3(0.8f, 0.7f, 1f);
                Kasa(kasaY, 0, TopRengi.Notr, 3f);
                break;

            case 8:
                DokuzTop(topY);
                lm.maksimumKayip = 4;
                // Lav + diken + kaya
                var lav = PI("Lav", new Vector3(0f, -15f, 0)); lav.transform.localScale = new Vector3(0.022f, 0.04f, 1f);
                PI("Diken", new Vector3(-2f, -8f, 0)).transform.localScale = new Vector3(1.2f, 1f, 1f);
                PI("Diken", new Vector3(2f, -25f, 0)).transform.localScale = new Vector3(1.2f, 1f, 1f);
                PI("Bomba", new Vector3(-1.5f, -35f, 0));
                PI("Kaya", new Vector3(-2.5f, -10f, 0)).transform.localScale = new Vector3(0.7f, 0.6f, 1f);
                PI("Kaya", new Vector3(2.5f, -20f, 0)).transform.localScale = new Vector3(0.7f, 0.6f, 1f);
                var su8 = PI("Su", new Vector3(1.5f, -40f, 0)); su8.transform.localScale = new Vector3(0.022f, 0.022f, 1f);
                Kasa(kasaY, 0, TopRengi.Notr, 2.5f);
                break;

            case 9:
                // Renkli
                KarmaTop(topY);
                lm.maksimumKayip = 4;
                // Yılan patroller
                var y1 = PI("Yilan", new Vector3(0f, -15f, 0));
                y1.GetComponent<Yilan>().mesafe = 5f;
                // Ayırıcı kayalar
                PI("Kaya", new Vector3(0, -8f, 0)).transform.localScale = new Vector3(0.4f, 1.5f, 1f);
                PI("Kaya", new Vector3(0, -25f, 0)).transform.localScale = new Vector3(0.4f, 1.5f, 1f);
                // Iki renkli kasa
                Kasa(kasaY, -2.2f, TopRengi.Kirmizi, 1.6f);
                Kasa(kasaY, 2.2f, TopRengi.Yesil, 1.6f);
                break;

            case 10:
                // FINAL
                KarmaTop(topY);
                lm.maksimumKayip = 5;
                // Her şey birlikte
                var cog10 = PI("Cogaltici", new Vector3(0, -8f, 0));
                cog10.transform.localScale = new Vector3(2.5f, 1.5f, 1f);
                cog10.GetComponent<Cogaltici>().carpan = 2;
                // Su bölgesi
                var su10 = PI("Su", new Vector3(-1.5f, -18f, 0)); su10.transform.localScale = new Vector3(0.018f, 0.03f, 1f);
                // Lav
                var lav10 = PI("Lav", new Vector3(2f, -30f, 0)); lav10.transform.localScale = new Vector3(0.015f, 0.03f, 1f);
                // Bomba
                PI("Bomba", new Vector3(0f, -40f, 0));
                PI("Bomba", new Vector3(-2f, -55f, 0));
                // Hareketli kaya
                var hk = PI("HareketliKaya", new Vector3(0, -25f, 0));
                hk.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                hk.GetComponent<HareketliKaya>().yonVektoru = new Vector2(3f, 0f);
                hk.GetComponent<HareketliKaya>().mesafe = 4f;
                // Yılan
                var y10 = PI("Yilan", new Vector3(0, -45f, 0));
                y10.GetComponent<Yilan>().mesafe = 5f;
                // Diken
                PI("Diken", new Vector3(-2.5f, -50f, 0)).transform.localScale = new Vector3(1.1f, 1f, 1f);
                PI("Diken", new Vector3(2.5f, -60f, 0)).transform.localScale = new Vector3(1.1f, 1f, 1f);
                // 2 renkli kasa en altta
                Kasa(kasaY, -2.2f, TopRengi.Kirmizi, 1.6f);
                Kasa(kasaY, 2.2f, TopRengi.Yesil, 1.6f);
                break;
        }
    }
}
#endif