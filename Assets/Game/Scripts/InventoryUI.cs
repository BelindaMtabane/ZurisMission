using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Level 3 Inventory UI — auto-creates only in the Level3End scene.
///
/// LAYOUT
/// ──────
/// Bottom-centre horizontal strip  (always visible, Y 0.02 – 0.22)
///   Left half  : MATERIALS COLLECTED — 4 vertical columns
///                (Pipe Parts | Screws | Tank Paste | Tap Parts)
///                each with a colour-coded fill bar and live X/30 count.
///   Right half : REPAIR PROGRESS — 3 vertical columns
///                (Pipe Joint 1 | Water Tap | Storage Tank)
///                each with a progress fill bar and live XX% label.
///
/// Centre USE popup  (only when near a structure, floats above the strip)
///   Shows structure name, current %, exact cost, and the USE button.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "Level3End") return;
        if (FindFirstObjectByType<InventoryUI>() != null) return;
        new GameObject("InventoryUI").AddComponent<InventoryUI>();
    }

    // ── Live data source ───────────────────────────────────────────────────
    PipeControlslevel3 _pipe;

    // Material bars / counts  [0]=PipeParts [1]=Screws [2]=TankPaste [3]=TapParts
    readonly RectTransform[] _matFills  = new RectTransform[4];
    readonly TMP_Text[]      _matCounts = new TMP_Text[4];

    // Structure bars / pct labels  [0]=PipeJoint1 [1]=WaterTap [2]=StorageTank
    readonly RectTransform[] _strFills = new RectTransform[3];
    readonly TMP_Text[]      _strPcts  = new TMP_Text[3];

    // USE panel refs
    GameObject _usePanelGO;
    Button     _useBtn;
    Image      _useBtnImg;
    TMP_Text   _useStructName;
    TMP_Text   _useProgress;
    TMP_Text   _useCost;
    Canvas     _canvas;   // stored so overlays can hide the whole UI

    // ── Zone latch — fixes physics/input timing race ───────────────────────
    // OnTriggerExit fires during FixedUpdate (mid-frame) and clears NearbyPipe
    // to None BEFORE the EventSystem processes the button click.  We latch the
    // zone when the player first approaches, and keep the USE panel alive for
    // _lingerSec seconds after the player walks out so they always have time
    // to click.
    PipeControlslevel3.PipeZone _pinnedZone      = PipeControlslevel3.PipeZone.None;
    float                       _lingerUntil     = 0f;
    const float                 _lingerSec       = 1.2f;   // seconds panel stays after exit

    /// <summary>Hide or show the inventory strip (call when overlays open/close).</summary>
    public void SetVisible(bool v) { if (_canvas) _canvas.gameObject.SetActive(v); }

    // ── Colour palette ─────────────────────────────────────────────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    static readonly Color ColPanel   = C(0.06f, 0.10f, 0.06f, 0.45f);
    static readonly Color ColHeader  = C(0.14f, 0.42f, 0.14f, 0.65f);
    static readonly Color ColGold    = C(1.00f, 0.82f, 0.20f);
    static readonly Color ColDiv     = C(0.28f, 0.58f, 0.28f, 0.70f);
    static readonly Color ColSubHdr  = C(0.45f, 0.85f, 0.45f);
    static readonly Color ColBarBg   = C(0.16f, 0.16f, 0.16f, 0.65f);
    static readonly Color ColGreen   = C(0.13f, 0.48f, 0.13f);
    static readonly Color ColGray    = C(0.30f, 0.30f, 0.30f);
    static readonly Color ColBlue    = C(0.35f, 0.78f, 1.00f);
    static readonly Color ColRowBg   = C(0.10f, 0.16f, 0.10f, 0.25f);

    static readonly Color[] ColMats = {
        C(0.40f, 0.70f, 1.00f),   // Pipe Parts  — blue
        C(0.82f, 0.82f, 0.82f),   // Screws      — silver
        C(0.55f, 0.90f, 0.50f),   // Tank Paste  — green
        C(1.00f, 0.68f, 0.25f),   // Tap Parts   — amber
    };
    static readonly Color[] ColStructs = {
        C(0.40f, 0.70f, 1.00f),   // Pipe Joint 1 — blue
        C(1.00f, 0.68f, 0.25f),   // Water Tap    — amber
        C(0.55f, 0.90f, 0.50f),   // Storage Tank — green
    };

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void Start()
    {
        _pipe = FindFirstObjectByType<PipeControlslevel3>(FindObjectsInactive.Include);
        // Also let PipeControlslevel3.Start() push itself via SetPipe() for safety.
    }

    /// <summary>Called by PipeControlslevel3.Start() so the reference is always fresh.</summary>
    public void SetPipe(PipeControlslevel3 pipe) { _pipe = pipe; }

    // ══════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (_pipe == null)
        {
            _pipe = FindFirstObjectByType<PipeControlslevel3>(FindObjectsInactive.Include);
            return;
        }

        // ── Material fill bars + counts ────────────────────────────────────
        int[] amounts = { _pipe.pipeParts, _pipe.screws, _pipe.tankPaste, _pipe.tapParts };
        for (int i = 0; i < 4; i++)
        {
            float pct = Mathf.Clamp01(amounts[i] / (float)PipeControlslevel3.MatMax);
            _matFills[i].anchorMax = new Vector2(pct, 1f);
            _matCounts[i].text     = $"{amounts[i]}/{PipeControlslevel3.MatMax}";

            var img = _matFills[i].GetComponent<Image>();
            if (amounts[i] >= 10)     img.color = ColMats[i];
            else if (amounts[i] >= 4) img.color = C(0.85f, 0.72f, 0.10f);
            else                      img.color = C(0.80f, 0.15f, 0.15f);
        }

        // ── Structure progress bars + pct labels ───────────────────────────
        int[] pcts = { _pipe.Tank1Amount, _pipe.Tank2Amount, _pipe.Tank3Amount };
        for (int i = 0; i < 3; i++)
        {
            _strFills[i].anchorMax = new Vector2(Mathf.Clamp01(pcts[i] / 100f), 1f);
            _strPcts[i].text       = $"{pcts[i]}%";
        }

        // ── USE panel — zone latch + linger window ─────────────────────────
        // OnTriggerExit fires in FixedUpdate (mid-frame) and can clear NearbyPipe
        // BEFORE the EventSystem delivers the button click.  We latch the zone on
        // approach and keep the panel open for _lingerSec after the player exits.
        bool physNear = _pipe.NearbyPipe != PipeControlslevel3.PipeZone.None;

        if (physNear)
        {
            _pinnedZone  = _pipe.NearbyPipe;          // latch current zone
            _lingerUntil = Time.unscaledTime + _lingerSec;
        }

        bool showPanel = physNear || Time.unscaledTime < _lingerUntil;
        _usePanelGO.SetActive(showPanel);

        if (!showPanel)
        {
            _pinnedZone = PipeControlslevel3.PipeZone.None;
            return;
        }

        // Drive display from the latched zone (valid even if physics cleared NearbyPipe).
        string structName, costLine;
        int    structPct;
        bool   canAfford;

        switch (_pinnedZone)
        {
            case PipeControlslevel3.PipeZone.Pipe1:
                structName = "PIPE JOINT 1";
                structPct  = _pipe.Tank1Amount;
                costLine   = $"{PipeControlslevel3.CostPipeParts}x Pipe Parts  +  {PipeControlslevel3.CostScrews}x Screw";
                canAfford  = _pipe.pipeParts >= PipeControlslevel3.CostPipeParts
                          && _pipe.screws    >= PipeControlslevel3.CostScrews;
                break;
            case PipeControlslevel3.PipeZone.Pipe2:
                structName = "WATER TAP";
                structPct  = _pipe.Tank2Amount;
                costLine   = $"{PipeControlslevel3.CostTapParts}x Tap Parts  +  {PipeControlslevel3.CostScrews}x Screw";
                canAfford  = _pipe.tapParts >= PipeControlslevel3.CostTapParts
                          && _pipe.screws   >= PipeControlslevel3.CostScrews;
                break;
            default: // Pipe3
                structName = "STORAGE TANK";
                structPct  = _pipe.Tank3Amount;
                costLine   = $"{PipeControlslevel3.CostTankPaste}x Tank Paste  +  {PipeControlslevel3.CostScrews}x Screw";
                canAfford  = _pipe.tankPaste >= PipeControlslevel3.CostTankPaste
                          && _pipe.screws    >= PipeControlslevel3.CostScrews;
                break;
        }

        _useStructName.text  = structName;
        _useProgress.text    = $"Progress: {structPct}%";
        _useCost.text        = $"Cost per repair:  {costLine}";
        // interactable drives ColorTint automatically (green = normal, gray = disabled).
        _useBtn.interactable = canAfford;
    }

    void OnUsePressed()
    {
        if (_pipe == null) return;
        // Use current physics zone if still inside, otherwise fall back to the
        // latched zone so the click always works within the linger window.
        var zone = _pipe.NearbyPipe != PipeControlslevel3.PipeZone.None
                   ? _pipe.NearbyPipe
                   : _pinnedZone;
        _pipe.UseAtZone(zone);
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI CONSTRUCTION
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        // ── Shared Canvas ──────────────────────────────────────────────────
        var cvGO = new GameObject("InvUI_Canvas");
        cvGO.transform.SetParent(transform);
        var cv = cvGO.AddComponent<Canvas>();
        _canvas         = cv;                           // store for SetVisible()
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 150;
        var cs = cvGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        // ══════════════════════════════════════════════════════════════════
        // HORIZONTAL INVENTORY STRIP
        // Screen anchors: (0.22, 0.02) → (0.82, 0.19)
        // Inset to avoid the mission panel (left) and GameInfoUI (right).
        // ══════════════════════════════════════════════════════════════════
        var strip = Img(cvGO, "InvStrip", ColPanel, V(0.22f, 0.02f), V(0.82f, 0.19f));

        // ── Main header bar (top of strip) ─────────────────────────────────
        Img(strip, "StripHdr",   ColHeader, V(0f, 0.87f), V(1f, 1f));
        Txt(strip, "StripTitle", "INVENTORY", 16, FontStyles.Bold, ColGold,
            V(0f, 0.87f), V(1f, 1f));

        // ── Section sub-headers ────────────────────────────────────────────
        Txt(strip, "MatSecHdr", "MATERIALS COLLECTED", 10, FontStyles.Bold,
            ColSubHdr, V(0.010f, 0.72f), V(0.540f, 0.86f));
        Txt(strip, "RepSecHdr", "REPAIR PROGRESS", 10, FontStyles.Bold,
            ColSubHdr, V(0.560f, 0.72f), V(0.990f, 0.86f));

        // Vertical divider between the two halves
        Img(strip, "VDiv", ColDiv, V(0.548f, 0.01f), V(0.552f, 0.85f));

        // ── 4 MATERIAL columns  (x range 0.010 – 0.540, width 0.128 each) ──
        // Gaps of ~0.002 between columns keep them visually separate.
        float   matW   = 0.128f;
        float[] matX   = { 0.010f, 0.140f, 0.270f, 0.400f };
        string[] matLbl = { "Pipe Parts", "Screws", "Tank Paste", "Tap Parts" };

        for (int i = 0; i < 4; i++)
        {
            float x0 = matX[i], x1 = x0 + matW;

            // Column background tint
            Img(strip, $"MatBg{i}", ColRowBg, V(x0, 0.01f), V(x1, 0.71f));

            // Material name
            var lbl = Txt(strip, $"MatLbl{i}", matLbl[i], 11, FontStyles.Normal,
                          ColMats[i], V(x0 + 0.004f, 0.50f), V(x1 - 0.004f, 0.70f));
            lbl.alignment = TextAlignmentOptions.Center;

            // Bar background
            var barBg = Img(strip, $"MatBarBg{i}", ColBarBg,
                            V(x0 + 0.006f, 0.24f), V(x1 - 0.006f, 0.48f));

            // Bar fill (anchor-driven, starts full)
            var fGO = new GameObject($"MatFill{i}");
            fGO.transform.SetParent(barBg.transform, false);
            fGO.AddComponent<Image>().color = ColMats[i];
            _matFills[i] = fGO.GetComponent<RectTransform>();
            _matFills[i].anchorMin = V(0, 0);
            _matFills[i].anchorMax = V(1, 1);
            _matFills[i].offsetMin = _matFills[i].offsetMax = Vector2.zero;

            // Count label (e.g. "10/30")
            _matCounts[i] = Txt(strip, $"MatCnt{i}", $"10/{PipeControlslevel3.MatMax}",
                                11, FontStyles.Bold, Color.white,
                                V(x0 + 0.004f, 0.03f), V(x1 - 0.004f, 0.22f));
            _matCounts[i].alignment = TextAlignmentOptions.Center;
        }

        // ── 3 STRUCTURE columns  (x range 0.560 – 0.990, width 0.140 each) ─
        float   strW    = 0.140f;
        float[] strX    = { 0.560f, 0.706f, 0.852f };
        string[] strLbl = { "Pipe Joint 1", "Water Tap", "Storage Tank" };

        for (int i = 0; i < 3; i++)
        {
            float x0 = strX[i], x1 = x0 + strW;

            // Column background tint
            Img(strip, $"StrBg{i}", ColRowBg, V(x0, 0.01f), V(x1, 0.71f));

            // Structure name
            var lbl = Txt(strip, $"StrLbl{i}", strLbl[i], 11, FontStyles.Normal,
                          ColStructs[i], V(x0 + 0.004f, 0.50f), V(x1 - 0.004f, 0.70f));
            lbl.alignment = TextAlignmentOptions.Center;

            // Bar background
            var barBg = Img(strip, $"StrBarBg{i}", ColBarBg,
                            V(x0 + 0.006f, 0.24f), V(x1 - 0.006f, 0.48f));

            // Bar fill (anchor-driven, starts empty)
            var fGO = new GameObject($"StrFill{i}");
            fGO.transform.SetParent(barBg.transform, false);
            fGO.AddComponent<Image>().color = ColStructs[i];
            _strFills[i] = fGO.GetComponent<RectTransform>();
            _strFills[i].anchorMin = V(0, 0);
            _strFills[i].anchorMax = V(0, 1);   // starts at 0 — grows as player repairs
            _strFills[i].offsetMin = _strFills[i].offsetMax = Vector2.zero;

            // Percentage label (e.g. "50%")
            _strPcts[i] = Txt(strip, $"StrPct{i}", "0%", 13, FontStyles.Bold,
                              Color.white,
                              V(x0 + 0.004f, 0.03f), V(x1 - 0.004f, 0.22f));
            _strPcts[i].alignment = TextAlignmentOptions.Center;
        }

        // ══════════════════════════════════════════════════════════════════
        // USE PANEL — floats ABOVE the inventory strip, never overlaps it.
        // Screen anchors: (0.35, 0.235) → (0.65, 0.46)
        // ══════════════════════════════════════════════════════════════════
        _usePanelGO = Img(cvGO, "UsePanel", ColPanel, V(0.35f, 0.205f), V(0.65f, 0.43f));

        // Header
        Img(_usePanelGO, "UseHdrBg", ColHeader, V(0f, 0.82f), V(1f, 1f));
        _useStructName = Txt(_usePanelGO, "UseStructName", "PIPE JOINT 1",
                             20, FontStyles.Bold, ColBlue, V(0f, 0.82f), V(1f, 1f));

        Img(_usePanelGO, "UseDiv", ColDiv, V(0.03f, 0.796f), V(0.97f, 0.801f));

        // Current progress
        _useProgress = Txt(_usePanelGO, "UseProgress", "Progress: 0%",
                           17, FontStyles.Bold, Color.white,
                           V(0.04f, 0.60f), V(0.96f, 0.79f));

        // Cost breakdown
        _useCost = Txt(_usePanelGO, "UseCost", "Cost: 3x Pipe Parts + 1x Screw",
                       13, FontStyles.Italic, C(0.72f, 0.72f, 0.72f),
                       V(0.04f, 0.43f), V(0.96f, 0.60f));
        _useCost.alignment = TextAlignmentOptions.Center;

        // USE button — ColorTint so the player sees a visual response when clicking
        var useBtnGO = Img(_usePanelGO, "UseBtn", ColGreen,
                           V(0.10f, 0.04f), V(0.90f, 0.41f));
        _useBtnImg  = useBtnGO.GetComponent<Image>();
        _useBtn     = useBtnGO.AddComponent<Button>();
        _useBtn.transition    = Selectable.Transition.ColorTint;
        _useBtn.targetGraphic = _useBtnImg;

        var cols = _useBtn.colors;
        cols.normalColor      = ColGreen;
        cols.highlightedColor = new Color(0.25f, 0.65f, 0.25f, 1f);   // lighter green on hover
        cols.pressedColor     = new Color(0.07f, 0.28f, 0.07f, 1f);   // dark green on press
        cols.disabledColor    = ColGray;
        cols.fadeDuration     = 0.08f;
        _useBtn.colors        = cols;

        var btnLbl = Txt(useBtnGO, "UseLbl", "USE", 32, FontStyles.Bold,
                         Color.white, V(0f, 0f), V(1f, 1f));
        btnLbl.alignment = TextAlignmentOptions.Center;
        _useBtn.onClick.AddListener(OnUsePressed);

        _usePanelGO.SetActive(false);
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
