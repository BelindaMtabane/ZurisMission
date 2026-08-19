using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Mission-status panel for MainGame (Level 1) and Level2.
/// Auto-creates itself — no scene setup needed.
///
/// Wood-framed panel in the top-right corner, under the level timer,
/// showing 4 live stats as icon meters instead of flat bars:
///   Health (leaves) | Water Bucket (droplets) | Hydration (droplets) | Materials (inventory slot)
/// </summary>
public class LevelHUDStrip : MonoBehaviour
{
    public static LevelHUDStrip Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "MainGame" && sceneName != "Level2") return;
        if (FindFirstObjectByType<LevelHUDStrip>() != null) return;
        new GameObject("LevelHUDStrip").AddComponent<LevelHUDStrip>();
    }

    // ── Live data source ───────────────────────────────────────────────────
    HUDControls _hud;
    Canvas      _canvas;

    public void SetVisible(bool v) { if (_canvas) _canvas.gameObject.SetActive(v); }

    // 3 pips per meter row (Health, Water Bucket, Hydration) + Materials text
    Image[]    _healthPips;
    Image[]    _bucketPips;
    Image[]    _hydratPips;
    TMP_Text   _healthVal, _bucketVal, _hydratVal, _matsVal;

    // ── Palette ──────────────────────────────────────────────────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    static readonly Color ColGood   = C(0.20f, 0.75f, 0.20f);
    static readonly Color ColWarn   = C(0.85f, 0.72f, 0.10f);
    static readonly Color ColBad    = C(0.80f, 0.15f, 0.15f);
    static readonly Color ColWater  = C(0.20f, 0.55f, 0.95f);
    static readonly Color ColHydra  = C(0.15f, 0.75f, 0.80f);
    static readonly Color ColEmpty  = C(0.45f, 0.45f, 0.45f, 0.45f);
    static readonly Color ColLabel  = C(0.30f, 0.19f, 0.09f);
    static readonly Color ColHeader = C(1.00f, 0.97f, 0.90f);

    // ── Shared wood-textured UI assets ─────────────────────────────────────
    static Sprite panel1Sprite;
    static Sprite Panel1()
    {
        if (panel1Sprite == null)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("UI/PANEL1");
            panel1Sprite = sprites.Length > 0 ? sprites[0] : null;
        }
        return panel1Sprite;
    }

    static Sprite leafSprite, dropletSprite;
    static Sprite Leaf()    { if (leafSprite == null)    leafSprite    = Resources.Load<Sprite>("UI/leaf_icon"); return leafSprite; }
    static Sprite Droplet() { if (dropletSprite == null) dropletSprite = Resources.Load<Sprite>("UI/droplet_icon"); return dropletSprite; }

    static readonly System.Collections.Generic.Dictionary<string, Sprite> uiKitCache = new();
    static Sprite UiKitSprite(string spriteName)
    {
        if (uiKitCache.TryGetValue(spriteName, out Sprite cached) && cached != null) return cached;
        Sprite[] sprites = Resources.LoadAll<Sprite>("UI/UIKitSheet");
        foreach (var s in sprites)
        {
            if (s.name == spriteName) { uiKitCache[spriteName] = s; return s; }
        }
        return null;
    }
    static Sprite SackSprite() => UiKitSprite("UIKit_31");

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void Start() => _hud = FindFirstObjectByType<HUDControls>();

    // ══════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (_hud == null) { _hud = FindFirstObjectByType<HUDControls>(); return; }

        float health = _hud.Health;
        float bucket = _hud.BucketWater;
        float hydrat = _hud.PlayerWater;
        float mats   = _hud.MaterialLevel;

        SetPips(_healthPips, health, ThresholdColor(health));
        SetPips(_bucketPips, bucket, ColWater);
        SetPips(_hydratPips, hydrat, ThresholdColor(hydrat, ColHydra));

        _healthVal.text = $"{health:F0}/100";
        _bucketVal.text = $"{bucket:F0}/100";
        _hydratVal.text = $"{hydrat:F0}/100";
        _matsVal.text   = $"{mats}/100";
    }

    static Color ThresholdColor(float value, Color goodColor = default)
    {
        if (goodColor == default) goodColor = ColGood;
        if (value >= 60) return goodColor;
        if (value >= 30) return ColWarn;
        return ColBad;
    }

    static void SetPips(Image[] pips, float value, Color filledColor)
    {
        int lit = Mathf.Clamp(Mathf.CeilToInt(value / 100f * pips.Length), 0, pips.Length);
        if (value <= 0f) lit = 0;
        for (int i = 0; i < pips.Length; i++)
        {
            pips[i].color = i < lit ? filledColor : ColEmpty;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI CONSTRUCTION — wood-framed panel, top-right, under the level timer
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        var cvGO = new GameObject("LvlHUD_Canvas");
        cvGO.transform.SetParent(transform);
        var cv = cvGO.AddComponent<Canvas>();
        _canvas         = cv;
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 140;
        var cs = cvGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        // ── Panel: top-right, directly under the level timer badge ─────────
        var panelGO = new GameObject("LvlStrip");
        panelGO.transform.SetParent(cvGO.transform, false);
        var panelImg = panelGO.AddComponent<Image>();
        Sprite p1 = Panel1();
        if (p1 != null) { panelImg.sprite = p1; panelImg.type = Image.Type.Sliced; panelImg.color = Color.white; }
        else panelImg.color = new Color(0.30f, 0.20f, 0.12f, 0.96f);
        var panelRt = panelGO.GetComponent<RectTransform>();
        panelRt.anchorMin = V(0.700f, 0.335f);
        panelRt.anchorMax = V(0.985f, 0.860f);
        panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;
        var strip = panelGO;

        Txt(strip, "StripTitle", "STATUS", 26, FontStyles.Bold, ColHeader,
            V(0.05f, 0.900f), V(0.95f, 0.990f));

        // ── 4 stat rows ──────────────────────────────────────────────────
        _healthVal = BuildPipRow(strip, "Health",    V(0.689f, 0.899f), Leaf(),    3, out _healthPips);
        _bucketVal = BuildPipRow(strip, "Water",     V(0.466f, 0.676f), Droplet(), 3, out _bucketPips);
        _hydratVal = BuildPipRow(strip, "Hydration", V(0.243f, 0.453f), Droplet(), 3, out _hydratPips);
        _matsVal   = BuildInventoryRow(strip, "Materials", V(0.02f, 0.230f));
    }

    // A labeled row of N icon "pips" plus a numeric value on the right.
    TMP_Text BuildPipRow(GameObject parent, string label, Vector2 yBand, Sprite iconSprite, int pipCount, out Image[] pips)
    {
        var row = new GameObject($"Row_{label}");
        row.transform.SetParent(parent.transform, false);
        var rt = row.AddComponent<RectTransform>();
        rt.anchorMin = V(0.04f, yBand.x);
        rt.anchorMax = V(0.96f, yBand.y);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Txt(row, "Lbl", label, 22, FontStyles.Bold, ColLabel,
            V(0f, 0.55f), V(0.60f, 1f)).alignment = TextAlignmentOptions.Left;

        var valTxt = Txt(row, "Val", "--/100", 20, FontStyles.Bold, ColLabel,
            V(0.55f, 0.55f), V(1f, 1f));
        valTxt.alignment = TextAlignmentOptions.Right;

        // Slot background behind the pips, matching the Materials inventory chip.
        var slot = new GameObject("Slot");
        slot.transform.SetParent(row.transform, false);
        var slotImg = slot.AddComponent<Image>();
        slotImg.color = new Color(0.12f, 0.08f, 0.04f, 0.40f);
        var slotRt = slot.GetComponent<RectTransform>();
        slotRt.anchorMin = V(0f, 0.02f);
        slotRt.anchorMax = V(1f, 0.52f);
        slotRt.offsetMin = slotRt.offsetMax = Vector2.zero;

        pips = new Image[pipCount];
        float pipW = 0.9f / pipCount;
        for (int i = 0; i < pipCount; i++)
        {
            var go = new GameObject($"Pip{i}");
            go.transform.SetParent(slot.transform, false);
            var img = go.AddComponent<Image>();
            img.sprite = iconSprite;
            img.preserveAspect = true;
            var prt = go.GetComponent<RectTransform>();
            float x0 = i * pipW;
            prt.anchorMin = V(x0 + 0.02f, 0.10f);
            prt.anchorMax = V(x0 + pipW * 0.82f, 0.90f);
            prt.offsetMin = prt.offsetMax = Vector2.zero;
            pips[i] = img;
        }
        return valTxt;
    }

    // Materials shown as a single inventory slot (sack icon) + count.
    TMP_Text BuildInventoryRow(GameObject parent, string label, Vector2 yBand)
    {
        var row = new GameObject($"Row_{label}");
        row.transform.SetParent(parent.transform, false);
        var rt = row.AddComponent<RectTransform>();
        rt.anchorMin = V(0.04f, yBand.x);
        rt.anchorMax = V(0.96f, yBand.y);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Txt(row, "Lbl", label, 22, FontStyles.Bold, ColLabel,
            V(0f, 0.55f), V(1f, 1f)).alignment = TextAlignmentOptions.Left;

        // Inventory slot chip behind the sack icon
        var slot = new GameObject("Slot");
        slot.transform.SetParent(row.transform, false);
        var slotImg = slot.AddComponent<Image>();
        slotImg.color = new Color(0.12f, 0.08f, 0.04f, 0.55f);
        var slotRt = slot.GetComponent<RectTransform>();
        slotRt.anchorMin = V(0f, 0.02f);
        slotRt.anchorMax = V(0.32f, 0.52f);
        slotRt.offsetMin = slotRt.offsetMax = Vector2.zero;

        var icon = new GameObject("Sack");
        icon.transform.SetParent(slot.transform, false);
        var iconImg = icon.AddComponent<Image>();
        iconImg.sprite = SackSprite();
        iconImg.preserveAspect = true;
        var iconRt = icon.GetComponent<RectTransform>();
        iconRt.anchorMin = V(0.12f, 0.12f);
        iconRt.anchorMax = V(0.88f, 0.88f);
        iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;

        var valTxt = Txt(row, "Val", "0/100", 22, FontStyles.Bold, ColLabel,
            V(0.38f, 0.02f), V(1f, 0.52f));
        valTxt.alignment = TextAlignmentOptions.Left;
        return valTxt;
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static Vector2 V(float x, float y) => new Vector2(x, y);

    static TMP_Text Txt(GameObject parent, string name, string text,
                         float size, FontStyles style, Color col,
                         Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text             = text;
        t.fontSize         = size;
        t.fontStyle        = style;
        t.color            = col;
        t.alignment        = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.Normal;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }
}
