using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DesertPlatformerBuilder
{
    const string Dir = "Assets/DesertPlatformer/";
    static Sprite S(string n) { return AssetDatabase.LoadAssetAtPath<Sprite>(Dir + "Sprites/" + n + ".png"); }

    static GameObject Make(string n, Transform parent = null)
    {
        var g = new GameObject(n);
        if (parent) g.transform.SetParent(parent, false);
        return g;
    }
    static SpriteRenderer Spr(GameObject g, string sprite, int order)
    {
        var r = g.AddComponent<SpriteRenderer>(); r.sprite = S(sprite); r.sortingOrder = order; return r;
    }
    static void Tiled(SpriteRenderer r, float w, float h)
    {
        r.drawMode = SpriteDrawMode.Tiled; r.size = new Vector2(w, h); r.tileMode = SpriteTileMode.Continuous;
    }

    static void FixImports()
    {
        AssetDatabase.Refresh();
        foreach (var f in Directory.GetFiles(Dir + "Sprites", "*.png"))
        {
            var path = f.Replace('\\', '/');
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) continue;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.filterMode = FilterMode.Bilinear;
            imp.maxTextureSize = 4096;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.Center;
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
        }
    }

    static void Ground(Transform parent, float a, float b, float top)
    {
        float w = b - a, h = top + 8f, cx = (a + b) / 2f;
        var g = Make("Ground", parent);
        g.transform.position = new Vector3(cx, top - h / 2f, 0);
        var col = g.AddComponent<BoxCollider2D>(); col.size = new Vector2(w, h);
        var t = Make("Top", g.transform); t.transform.position = new Vector3(cx, top - 0.5f, 0);
        Tiled(Spr(t, "ground_top", 0), w, 1f);
        var d = Make("Dirt", g.transform); d.transform.position = new Vector3(cx, top - 1f - (h - 1f) / 2f, 0);
        Tiled(Spr(d, "ground_dirt", 0), w, h - 1f);
    }

    static void Platform(Transform parent, float cx, float w, float top)
    {
        var g = Make("Platform", parent);
        g.transform.position = new Vector3(cx, top - 0.15f, 0);
        Tiled(Spr(g, "platform", 1), w, 0.3f);
        var c = g.AddComponent<BoxCollider2D>(); c.size = new Vector2(w, 0.3f); c.usedByEffector = true;
        var e = g.AddComponent<PlatformEffector2D>(); e.useOneWay = true; e.surfaceArc = 160f;
    }

    static void Item(Transform p, string sprite, float x, float y, DesertItem.Kind k, Vector2 size)
    {
        var g = Make(sprite, p); g.transform.position = new Vector3(x, y, 0);
        Spr(g, sprite, 5);
        var c = g.AddComponent<BoxCollider2D>(); c.isTrigger = true; c.size = size;
        g.AddComponent<DesertItem>().kind = k;
    }

    static void Decor(Transform p, string sprite, float x, float top, int order = -10, float scale = 1f)
    {
        var g = Make(sprite, p); var r = Spr(g, sprite, order);
        g.transform.localScale = Vector3.one * scale;
        g.transform.position = new Vector3(x, top - 0.05f + r.sprite.bounds.size.y * scale / 2f, 0);
    }

    static void Block(Transform p, float x, float y)
    {
        var g = Make("IceBlock", p); g.transform.position = new Vector3(x, y, 0);
        Spr(g, "ice_block", 4);
        g.AddComponent<BoxCollider2D>().size = new Vector2(0.98f, 0.98f);
        var rb = g.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f; rb.mass = 1.5f; rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    static void Layer(Transform p, string sprite, int order, float speed, float scale)
    {
        var g = Make(sprite, p); Spr(g, sprite, order);
        g.transform.localScale = Vector3.one * scale;
        g.AddComponent<DesertParallax>().speed = speed;
    }

    static RectTransform UI(string n, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var g = new GameObject(n, typeof(RectTransform));
        var r = g.GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = anchor;
        r.anchoredPosition = pos; r.sizeDelta = size;
        return r;
    }

    static Text MakeText(RectTransform r, int size, TextAnchor al)
    {
        var t = r.gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.fontStyle = FontStyle.Bold; t.color = Color.white; t.alignment = al;
        var o = r.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, 0.7f); o.effectDistance = new Vector2(3, -3);
        return t;
    }

    [MenuItem("Desert Platformer/Build Level Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        FixImports();
        if (S("fox_idle") == null)
        {
            EditorUtility.DisplayDialog("Desert Platformer", "Sprites not found. The folder must be Assets/DesertPlatformer/Sprites.", "OK");
            return;
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var spawn = new Vector3(0f, -3.0f, 0f);

        // Camera
        var cg = Make("Main Camera"); cg.tag = "MainCamera";
        var cam = cg.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5.4f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.45f, 0.4f, 0.72f);
        cg.AddComponent<AudioListener>();
        cg.transform.position = new Vector3(0, 0, -10);

        // Background
        var bg = Make("Background").transform;
        Layer(bg, "sky", -100, 1f, 1.5f);
        Layer(bg, "mountains_far", -90, 0.9f, 1f);
        Layer(bg, "mountains_near", -80, 0.75f, 1f);
        Layer(bg, "pines_far", -70, 0.6f, 1f);
        Layer(bg, "pines_near", -60, 0.35f, 1f);

        // Level
        var lv = Make("Level").transform;
        float[][] ground = { new[] { -12f, 6f, -3.5f }, new[] { 6f, 9.5f, -1.5f }, new[] { 9.5f, 22f, -3.5f }, new[] { 22f, 40f, -1f }, new[] { 43f, 66f, -3.5f } };
        foreach (var s in ground) Ground(lv, s[0], s[1], s[2]);
        Platform(lv, 14f, 3f, -1.2f);
        Platform(lv, 19f, 2.5f, -1.9f);
        foreach (var x in new[] { -12.3f, 66.3f })
        {
            var w = Make("Wall", lv); w.transform.position = new Vector3(x, 0, 0);
            w.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 30f);
        }

        // Decoration
        foreach (var x in new[] { -8f, -3f, 12.5f, 20.5f, 38f }) Decor(lv, "pine_green", x, x > 20 ? (x > 30 ? -1f : -3.5f) : -3.5f);
        foreach (var x in new[] { 25f }) Decor(lv, "pine_green", x, -1f);
        foreach (var x in new[] { 46f, 51f, 60f }) Decor(lv, "pine_green", x, -3.5f);
        Decor(lv, "snowman", 10.8f, -3.5f);
        Decor(lv, "igloo", 55f, -3.5f);
        foreach (var x in new[] { -9.5f, 64f }) Decor(lv, "pine_green", x, -3.5f, -15, 1.5f);
        Decor(lv, "pine_green", 34f, -1f, -15, 1.5f);
        foreach (var x in new[] { -10f, 28.5f, 58f }) Decor(lv, "mound", x, x > 20 && x < 40 ? -1f : -3.5f, -8);
        foreach (var x in new[] { -5f, 11.5f, 57.8f }) Decor(lv, "bush", x, -3.5f, -6);
        Decor(lv, "bush", 29f, -1f, -6);
        foreach (var x in new[] { 20.9f, 44.5f }) Decor(lv, "crystals", x, -3.5f, -5);
        Decor(lv, "rock", 23f, -1f, -8);
        Decor(lv, "rock", 3.8f, -3.5f, -8);
        Decor(lv, "signpost", 1.5f, -3.5f, -7);
        Decor(lv, "signpost", 23.5f, -1f, -7);

        // Pushable ice blocks
        Block(lv, 14f, -0.7f);
        Block(lv, 28f, -0.5f);
        Block(lv, 12f, -3.0f);

        // Coins, hazards, goal
        float[][] coins = {
            new[]{3f,-2.6f}, new[]{4f,-2.6f}, new[]{5f,-2.6f}, new[]{7.2f,-0.6f}, new[]{8.2f,-0.6f},
            new[]{12f,-2.6f}, new[]{13f,-2.6f}, new[]{14f,-0.5f}, new[]{15f,-0.5f}, new[]{19f,-1.2f},
            new[]{25f,0.1f}, new[]{26f,0.1f}, new[]{27f,0.1f}, new[]{35f,0.1f}, new[]{36f,0.1f},
            new[]{41.5f,0.4f}, new[]{47f,-2.6f}, new[]{48f,-2.6f}, new[]{49.5f,-2.6f}, new[]{52f,-2.6f} };
        foreach (var c in coins) Item(lv, "coin", c[0], c[1], DesertItem.Kind.Coin, new Vector2(0.7f, 0.7f));
        Item(lv, "spikes", 16.5f, -3.2f, DesertItem.Kind.Hazard, new Vector2(0.8f, 0.4f));
        Item(lv, "spikes", 31f, -0.7f, DesertItem.Kind.Hazard, new Vector2(0.8f, 0.4f));
        Item(lv, "spikes", 49f, -3.2f, DesertItem.Kind.Hazard, new Vector2(0.8f, 0.4f));
        Item(lv, "flag", 62f, -2.6f, DesertItem.Kind.Goal, new Vector2(0.8f, 1.8f));

        // Player
        var pg = Make("Player"); pg.transform.position = spawn;
        Spr(pg, "fox_idle", 10);
        var rb = pg.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f; rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var cc = pg.AddComponent<CapsuleCollider2D>(); cc.size = new Vector2(0.5f, 0.82f); cc.offset = new Vector2(0f, -0.02f);
        var sp = pg.AddComponent<DesertPlayer>();
        sp.sparkle = S("sparkle"); sp.idle = S("fox_idle"); sp.jump = S("fox_jump"); sp.run = new[] { S("fox_run1"), S("fox_run2") };

        var sc = cg.AddComponent<DesertCamera>(); sc.target = pg.transform; sc.minX = 0f; sc.maxX = 56f;

        // UI
        var cv = Make("Canvas");
        var canvas = cv.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = cv.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; cs.referenceResolution = new Vector2(1920, 1080); cs.matchWidthOrHeight = 0.5f;
        cv.AddComponent<GraphicRaycaster>();
        var hearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var r = UI("Heart" + i, cv.transform, new Vector2(0, 1), new Vector2(40 + i * 90, -40), new Vector2(72, 72));
            hearts[i] = r.gameObject.AddComponent<Image>(); hearts[i].sprite = S("heart_full");
        }
        var ic = UI("CoinIcon", cv.transform, new Vector2(1, 1), new Vector2(-190, -40), new Vector2(72, 72));
        ic.gameObject.AddComponent<Image>().sprite = S("coin");
        var coinText = MakeText(UI("CoinText", cv.transform, new Vector2(1, 1), new Vector2(-40, -40), new Vector2(140, 72)), 56, TextAnchor.MiddleRight);
        coinText.text = "0";
        var msg = MakeText(UI("Message", cv.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600, 300)), 90, TextAnchor.MiddleCenter);
        msg.gameObject.SetActive(false);

        var gm = Make("GameManager").AddComponent<DesertGame>();
        gm.hearts = hearts; gm.heartFull = S("heart_full"); gm.heartEmpty = S("heart_empty");
        gm.coinText = coinText; gm.message = msg; gm.player = sp; gm.spawn = spawn;

        // Save
        Directory.CreateDirectory(Dir + "Scenes");
        string path = Dir + "Scenes/DesertLevel.unity";
        EditorSceneManager.SaveScene(scene, path);
        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(x => x.path == path)) list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
        EditorUtility.DisplayDialog("Desert Platformer", "Level built. Press Play.\nMove: A/D or arrows. Jump: Space or W.", "OK");
    }
}
