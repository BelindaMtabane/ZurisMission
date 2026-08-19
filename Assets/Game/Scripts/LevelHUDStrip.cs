using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Mission-status inventory strip for MainGame (Level 1) and Level2.
/// Auto-creates itself — no scene setup needed.
///
/// Horizontal strip at the bottom-centre of the screen (same position as
/// the Level 3 InventoryUI) showing 5 live stat columns:
///   Health | Water Bucket | Hydration | Materials | Village Progress
///
/// The header line shows a mission-specific hint for each level.
/// </summary>
public class LevelHUDStrip : MonoBehaviour
{
    public static LevelHUDStrip Instance { get; private set; }

    // ── Auto-create in MainGame and Level2 on every scene load ────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        // Register for ALL future scene loads so this fires every time,
        // not just on the first scene of the session.
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);   // handle current scene too
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "MainGame" && sceneName != "Level2") return;
        if (FindFirstObjectByType<LevelHUDStrip>() != null) return;
        new GameObject("LevelHUDStrip").AddComponent<LevelHUDStrip>();
    }

    // ── Live data source ───────────────────────────────────────────────────
    HUDControls _hud;
    string      _sceneName;
    Canvas      _canvas;   // stored so we can hide/show the whole strip

    /// <summary>Hide or show the HUD strip (call when overlays open/close).</summary>
    public void SetVisible(bool v) { if (_canvas) _canvas.gameObject.SetActive(v); }

    // 5 stat columns: Health, Water Bucket, Hydration, Materials, Village %
    readonly RectTransform[] _fills  = new RectTransform[5];
    readonly TMP_Text[]      _labels = new TMP_Text[5];

    // ── Colour palette (matches InventoryUI theme) ─────────────────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    static readonly Color ColPanel   = C(0.06f, 0.10f, 0.06f, 0.45f);
    static readonly Color ColHeader  = C(0.14f, 0.42f, 0.14f, 0.65f);
    static readonly Color ColGold    = C(1.00f, 0.82f, 0.20f);
    static readonly Color ColSubHdr  = C(0.45f, 0.85f, 0.45f);
    static readonly Color ColBarBg   = C(0.16f, 0.16f, 0.16f, 0.65f);
    static readonly Color ColRowBg   = C(0.10f, 0.16f, 0.10f, 0.25f);

    // Per-column accent colours
    static readonly Color ColHealth  = C(0.20f, 0.80f, 0.20f);   // green  → changes on low health
    static readonly Color ColBucket  = C(0.30f, 0.60f, 1.00f);   // water blue
    static readonly Color ColHydrat  = C(0.20f, 0.90f, 0.90f);   // cyan
    static readonly Color ColMats    = C(0.85f, 0.60f, 0.20f);   // amber
    static readonly Color ColVillage = C(1.00f, 0.82f, 0.20f);   // gold

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance  = this;
        _sceneName = SceneManager.GetActiveScene().name;
        BuildUI();
    }

    void Start() => _hud = FindFirstObjectByType<HUDControls>();

    // ══════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (_hud == null) { _hud = FindFirstObjectByType<HUDControls>(); return; }

        float health  = _hud.Health;
        float bucket  = _hud.BucketWater;
        float hydrat  = _hud.PlayerWater;
        float mats    = _hud.MaterialLevel;
        float village = _hud.VillageProgressPercent;

        float[] vals = { health, bucket, hydrat, mats, village };

        for (int i = 0; i < 5; i++)
        {
            _fills[i].anchorMax = new Vector2(Mathf.Clamp01(vals[i] / 100f), 1f);
        }

        // ── Health bar: green → yellow → red ──────────────────────────────
        var hImg = _fills[0].GetComponent<Image>();
        if      (health >= 60) hImg.color = ColHealth;
        else if (health >= 30) hImg.color = C(0.85f, 0.72f, 0.10f);
        else                   hImg.color = C(0.80f, 0.15f, 0.15f);

        // ── Hydration bar: cyan → yellow → red ────────────────────────────
        var dImg = _fills[2].GetComponent<Image>();
        if      (hydrat >= 60) dImg.color = ColHydrat;
        else if (hydrat >= 30) dImg.color = C(0.85f, 0.72f, 0.10f);
        else                   dImg.color = C(0.80f, 0.15f, 0.15f);

        // ── Stat value text ────────────────────────────────────────────────
        _labels[0].text = $"{health:F0} / 100";
        _labels[1].text = $"{bucket:F0} / 100";
        _labels[2].text = $"{hydrat:F0} / 100";
        _labels[3].text = $"{mats} / 100";
        _labels[4].text = $"{village:F0}%";
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI CONSTRUCTION
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        // ── Canvas ─────────────────────────────────────────────────────────
        var cvGO = new GameObject("LvlHUD_Canvas");
        cvGO.transform.SetParent(transform);
        var cv = cvGO.AddComponent<Canvas>();
        _canvas         = cv;                           // store for SetVisible()
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 140;
        var cs = cvGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        // ── Horizontal strip (same footprint as Level 3 InventoryUI) ───────
        var strip = Img(cvGO, "LvlStrip", ColPanel, V(0.22f, 0.02f), V(0.82f, 0.19f));

        // Header bar
        Img(strip, "StripHdr", ColHeader, V(0f, 0.87f), V(1f, 1f));
        Txt(strip, "StripTitle", "MISSION STATUS", 16, FontStyles.Bold, ColGold,
            V(0f, 0.87f), V(1f, 1f));

        // Level-specific mission hint beneath header
        string hint = _sceneName == "MainGame"
            ? "Collect water for the village — stay hydrated and avoid hazards!"
            : "Navigate the river, open floodgates and irrigate the farmlands!";
        Txt(strip, "MissionHint", hint, 10, FontStyles.Italic, ColSubHdr,
            V(0.01f, 0.72f), V(0.99f, 0.86f));

        // ── 5 stat columns ─────────────────────────────────────────────────
        // Span the full strip width (panel-relative 0→1) in 5 equal columns.
        float   colW   = 0.185f;
        float[] colX   = { 0.010f, 0.202f, 0.394f, 0.586f, 0.778f };
        Color[] clrs   = { ColHealth, ColBucket, ColHydrat, ColMats, ColVillage };

        // Column headers differ per level to match the mission context
        string[] lvl1Names = { "Health",       "Water Bucket", "Hydration",    "Materials",  "Village %" };
        string[] lvl2Names = { "Health",       "Water Level",  "Hydration",    "Materials",  "Village %" };
        string[] colNames  = _sceneName == "MainGame" ? lvl1Names : lvl2Names;

        for (int i = 0; i < 5; i++)
        {
            float x0 = colX[i], x1 = x0 + colW;

            // Column background tint
            Img(strip, $"ColBg{i}", ColRowBg, V(x0, 0.01f), V(x1, 0.71f));

            // Stat name
            var lbl = Txt(strip, $"ColLbl{i}", colNames[i], 11, FontStyles.Normal,
                          clrs[i], V(x0 + 0.004f, 0.50f), V(x1 - 0.004f, 0.70f));
            lbl.alignment = TextAlignmentOptions.Center;

            // Bar background
            var barBg = Img(strip, $"BarBg{i}", ColBarBg,
                            V(x0 + 0.006f, 0.24f), V(x1 - 0.006f, 0.48f));

            // Bar fill (anchor-driven)
            var fGO = new GameObject($"BarFill{i}");
            fGO.transform.SetParent(barBg.transform, false);
            fGO.AddComponent<Image>().color = clrs[i];
            _fills[i] = fGO.GetComponent<RectTransform>();
            _fills[i].anchorMin = V(0, 0);
            _fills[i].anchorMax = V(1, 1);   // updated every frame in Update()
            _fills[i].offsetMin = _fills[i].offsetMax = Vector2.zero;

            // Value label (e.g. "85 / 100" or "42%")
            _labels[i] = Txt(strip, $"ValLbl{i}", "---", 11, FontStyles.Bold,
                              Color.white,
                              V(x0 + 0.004f, 0.03f), V(x1 - 0.004f, 0.22f));
            _labels[i].alignment = TextAlignmentOptions.Center;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static Vector2 V(float x, float y) => new Vector2(x, y);

    static GameObject Img(GameObject parent, string name, Color col,
                           Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = col;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

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
