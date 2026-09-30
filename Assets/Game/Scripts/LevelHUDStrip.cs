using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Mission-status panel for MainGame (Level 1) and Level2.
/// Adventure UI Kit panel with progress bars for health, water, hydration, and materials.
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

    HUDControls _hud;
    Canvas      _canvas;

    public void SetVisible(bool v) { if (_canvas) _canvas.gameObject.SetActive(v); }

    Slider   _healthBar;
    Slider   _bucketBar;
    Slider   _hydratBar;
    Slider   _matsBar;
    TMP_Text _healthVal, _bucketVal, _hydratVal, _matsVal;

    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    static readonly Color ColLabel  = C(1f, 0.98f, 0.94f);
    static readonly Color ColHeader = C(1f, 0.92f, 0.55f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        ObstacleGuideHUD.EnsureForScene(SceneManager.GetActiveScene().name);
    }

    void Start() => _hud = FindFirstObjectByType<HUDControls>();

    void Update()
    {
        if (_hud == null) { _hud = FindFirstObjectByType<HUDControls>(); return; }

        float health = _hud.Health;
        float bucket = _hud.BucketWater;
        float hydrat = _hud.PlayerWater;
        float mats   = _hud.MaterialLevel;

        if (_healthBar != null) _healthBar.value = health;
        if (_bucketBar != null) _bucketBar.value = bucket;
        if (_hydratBar != null) _hydratBar.value = hydrat;
        if (_matsBar != null) _matsBar.value = mats;

        _healthVal.text = $"{health:F0}/100";
        _bucketVal.text = $"{bucket:F0}/100";
        _hydratVal.text = $"{hydrat:F0}/100";
        _matsVal.text   = $"{mats}/100";
    }

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

        var panelGO = new GameObject("LvlStrip");
        panelGO.transform.SetParent(cvGO.transform, false);
        var panelImg = panelGO.AddComponent<Image>();
        AdventureUI.ApplyPanel(panelImg);
        if (panelImg.sprite == null) panelImg.color = new Color(0.30f, 0.20f, 0.12f, 0.96f);
        var panelRt = panelGO.GetComponent<RectTransform>();
        panelRt.anchorMin = V(0.805f, 0.470f);
        panelRt.anchorMax = V(0.988f, 0.820f);
        panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

        Txt(panelGO, "StripTitle", "STATUS", 24, FontStyles.Bold, ColHeader,
            V(0.05f, 0.900f), V(0.95f, 0.990f));

        _healthVal = BuildBarRow(panelGO, "Health", V(0.689f, 0.899f), AdventureUI.FillGreen, out _healthBar);
        _bucketVal = BuildBarRow(panelGO, "Water", V(0.466f, 0.676f), AdventureUI.FillBlue, out _bucketBar);
        _hydratVal = BuildBarRow(panelGO, "Hydration", V(0.243f, 0.453f), AdventureUI.FillBlue, out _hydratBar);
        _matsVal   = BuildBarRow(panelGO, "Materials", V(0.02f, 0.230f), AdventureUI.FillOrange, out _matsBar);
    }

    TMP_Text BuildBarRow(GameObject parent, string label, Vector2 yBand, Sprite fillSprite, out Slider slider)
    {
        var row = new GameObject($"Row_{label}");
        row.transform.SetParent(parent.transform, false);
        var rt = row.AddComponent<RectTransform>();
        rt.anchorMin = V(0.04f, yBand.x);
        rt.anchorMax = V(0.96f, yBand.y);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Txt(row, "Lbl", label, 22, FontStyles.Bold, ColLabel,
            V(0f, 0.55f), V(0.62f, 1f)).alignment = TextAlignmentOptions.Left;

        var valTxt = Txt(row, "Val", "0/100", 20, FontStyles.Bold, ColLabel,
            V(0.52f, 0.55f), V(1f, 1f));
        valTxt.alignment = TextAlignmentOptions.Right;

        slider = AdventureUI.BuildHorizontalBar(row.transform, "Bar", fillSprite,
            V(0f, 0.05f), V(1f, 0.50f), out _);
        return valTxt;
    }

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
        t.outlineWidth     = 0.22f;
        t.outlineColor     = new Color(0f, 0f, 0f, 0.85f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }
}
