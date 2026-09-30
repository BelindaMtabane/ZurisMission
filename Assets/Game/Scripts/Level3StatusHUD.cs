using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Level 3 bottom-right status: live health, materials (down on use, up on collect),
/// and remaining water-system pipes to fix. Lightning increases the pipe count.
/// </summary>
public class Level3StatusHUD : MonoBehaviour
{
    public static Level3StatusHUD Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != SceneCatalog.Level3) return;
        if (FindFirstObjectByType<Level3StatusHUD>() != null) return;
        new GameObject("Level3StatusHUD").AddComponent<Level3StatusHUD>();
    }

    HUDControls _hud;
    Canvas _canvas;
    Slider _healthBar;
    Slider _matsBar;
    TMP_Text _healthVal;
    TMP_Text _matsVal;
    TMP_Text _pipesVal;

    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    static readonly Color ColLabel = C(1f, 0.98f, 0.94f);
    static readonly Color ColHeader = C(1f, 0.92f, 0.55f);
    static readonly Color ColPipesOk = C(0.45f, 0.95f, 0.5f);
    static readonly Color ColPipesWork = C(1f, 0.88f, 0.2f);

    public void SetVisible(bool v)
    {
        if (_canvas != null) _canvas.gameObject.SetActive(v);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUI();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start() => _hud = FindFirstObjectByType<HUDControls>();

    void Update()
    {
        if (_hud == null)
        {
            _hud = FindFirstObjectByType<HUDControls>();
            if (_hud == null) return;
        }

        float maxHealth = Mathf.Max(1f, _hud.MaxHealth);
        float maxMats = Mathf.Max(1, _hud.MaxMaterial);
        float health = _hud.Health;
        float mats = _hud.MaterialLevel;

        if (_healthBar != null)
        {
            _healthBar.maxValue = maxHealth;
            _healthBar.value = health;
        }

        if (_matsBar != null)
        {
            _matsBar.maxValue = maxMats;
            _matsBar.value = mats;
        }

        if (_healthVal != null) _healthVal.text = $"{health:F0}/{maxHealth:F0}";
        if (_matsVal != null) _matsVal.text = $"{mats:F0}/{maxMats:F0}";

        int pipes = Level3PipeRepair.RemainingPipes;
        if (_pipesVal != null)
        {
            _pipesVal.text = pipes <= 0 ? "CLEAR" : pipes.ToString();
            _pipesVal.color = pipes <= 0 ? ColPipesOk : ColPipesWork;
        }
    }

    void BuildUI()
    {
        var cvGO = new GameObject("Lvl3Status_Canvas");
        cvGO.transform.SetParent(transform);
        var cv = cvGO.AddComponent<Canvas>();
        _canvas = cv;
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 146;
        var cs = cvGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        var panelGO = new GameObject("Lvl3Status");
        panelGO.transform.SetParent(cvGO.transform, false);
        var panelImg = panelGO.AddComponent<Image>();
        AdventureUI.ApplyPanel(panelImg);
        if (panelImg.sprite == null) panelImg.color = new Color(0.10f, 0.16f, 0.12f, 0.96f);
        var panelRt = panelGO.GetComponent<RectTransform>();
        panelRt.anchorMin = V(0.805f, 0.018f);
        panelRt.anchorMax = V(0.988f, 0.330f);
        panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

        Txt(panelGO, "StripTitle", "STATUS", 22, FontStyles.Bold, ColHeader,
            V(0.05f, 0.86f), V(0.95f, 0.98f));

        _healthVal = BuildBarRow(panelGO, "Health", V(0.58f, 0.84f), AdventureUI.FillGreen, out _healthBar);
        _matsVal = BuildBarRow(panelGO, "Materials", V(0.30f, 0.56f), AdventureUI.FillOrange, out _matsBar);

        var pipesRow = new GameObject("Row_Pipes");
        pipesRow.transform.SetParent(panelGO.transform, false);
        var pipesRt = pipesRow.AddComponent<RectTransform>();
        pipesRt.anchorMin = V(0.04f, 0.04f);
        pipesRt.anchorMax = V(0.96f, 0.28f);
        pipesRt.offsetMin = pipesRt.offsetMax = Vector2.zero;

        Txt(pipesRow, "Lbl", "PIPES TO FIX", 18, FontStyles.Bold, ColLabel,
            V(0f, 0.48f), V(1f, 1f)).alignment = TextAlignmentOptions.Left;

        _pipesVal = Txt(pipesRow, "Val", "0", 28, FontStyles.Bold, ColPipesWork,
            V(0f, 0f), V(1f, 0.55f));
        _pipesVal.alignment = TextAlignmentOptions.Right;
    }

    TMP_Text BuildBarRow(GameObject parent, string label, Vector2 yBand, Sprite fillSprite, out Slider slider)
    {
        var row = new GameObject($"Row_{label}");
        row.transform.SetParent(parent.transform, false);
        var rt = row.AddComponent<RectTransform>();
        rt.anchorMin = V(0.04f, yBand.x);
        rt.anchorMax = V(0.96f, yBand.y);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Txt(row, "Lbl", label, 18, FontStyles.Bold, ColLabel,
            V(0f, 0.55f), V(0.58f, 1f)).alignment = TextAlignmentOptions.Left;

        var valTxt = Txt(row, "Val", "0/100", 16, FontStyles.Bold, ColLabel,
            V(0.48f, 0.55f), V(1f, 1f));
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
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = col;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.outlineWidth = 0.22f;
        t.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        t.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }
}
