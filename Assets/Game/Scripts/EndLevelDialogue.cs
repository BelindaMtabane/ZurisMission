using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// End-of-level dialogue overlay.
/// Auto-creates itself via [RuntimeInitializeOnLoadMethod] — no manual scene setup needed.
/// Call EndLevelDialogue.Instance.ShowForLevel(1|2|3) when the player reaches an Ender.
/// </summary>
public class EndLevelDialogue : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static EndLevelDialogue Instance { get; private set; }

    // ── Auto-create in every scene (fires on every scene load) ────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate();
        TryCreate();   // handle the first scene too
    }

    static void TryCreate()
    {
        if (FindFirstObjectByType<EndLevelDialogue>() != null) return;
        new GameObject("EndLevelDialogue").AddComponent<EndLevelDialogue>();
    }

    // ── UI element refs (built in code) ───────────────────────────────────
    GameObject _canvasGO;
    TMP_Text   _titleText;
    TMP_Text   _statsText;
    TMP_Text   _congratsText;
    TMP_Text   _nextInfoText;
    Button     _continueBtn;

    string _nextScene = "";
    bool   _showing   = false;

    // ── Shared wood-textured UI assets (same sheet as the end-screen panels) ─
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

    static Sprite continueBtnSprite;
    static Sprite ContinueButtonSprite()
    {
        if (continueBtnSprite == null)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("UI/UIKitSheet");
            foreach (var s in sprites)
            {
                if (s.name == "UIKit_04") { continueBtnSprite = s; break; }
            }
        }
        return continueBtnSprite;
    }

    // Dark-brown palette for text sitting on the parchment panel.
    static readonly Color ColDarkText  = new Color(0.28f, 0.17f, 0.08f);
    static readonly Color ColDarkTitle = new Color(0.55f, 0.10f, 0.05f);
    static readonly Color ColDivider   = new Color(0.55f, 0.42f, 0.24f);

    // ── Live stat sources ──────────────────────────────────────────────────
    HUDControls        _hud;
    PipeControlslevel3 _pipe;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void Start()
    {
        _hud  = FindFirstObjectByType<HUDControls>();
        _pipe = FindFirstObjectByType<PipeControlslevel3>();
    }

    // ── Editor / MCP test helper ───────────────────────────────────────────
    /// <summary>Call via Unity MCP editor_invoke_method while in Play Mode.</summary>
    public static void TestShow1() { Instance?.ShowForLevel(1); }
    public static void TestShow2() { Instance?.ShowForLevel(2); }
    public static void TestShow3() { Instance?.ShowForLevel(3); }

    // ── Public API ─────────────────────────────────────────────────────────
    public void ShowForLevel(int level)
    {
        if (_showing) return;
        _showing = true;

        // Re-find stat sources (Start may not have run if called very early)
        if (_hud  == null) _hud  = FindFirstObjectByType<HUDControls>();
        if (_pipe == null) _pipe = FindFirstObjectByType<PipeControlslevel3>();

        Time.timeScale = 0f;

        // Hide every HUD layer so only this overlay is visible
        LevelHUDStrip.Instance?.SetVisible(false);
        InventoryUI.Instance?.SetVisible(false);
        GameInfoUI.Instance?.SetVisible(false);
        // Hide scene Canvas HUD siblings (Material bar, Pause button, etc.)
        // (handled by RunStateManager overlay logic)

        string title, congrats, nextInfo, scene;

        switch (level)
        {
            case 1:
                title    = "LEVEL 1 COMPLETE!";
                congrats =
                    "Zuri has carried water back to the village and kept her community\n" +
                    "hydrated through the heat and danger of the desert.\n" +
                    "Step one of saving the village is done — incredible work!";
                nextInfo =
                    "Level 2  —  The River Crossing\n" +
                    "Zuri must navigate a wild river system and release floodgates to " +
                    "irrigate the farmlands. Watch out for speeding cars, deep river " +
                    "currents, and slippery mud patches. Stay fast and stay healthy!";
                scene = "Level2";
                break;

            case 2:
                title    = "LEVEL 2 COMPLETE!";
                congrats =
                    "Outstanding! The farms are watered and the village is thriving.\n" +
                    "Zuri managed every floodgate and survived every hazard the " +
                    "wilderness threw at her. The community grows stronger each day!";
                nextInfo =
                    "Level 3  —  The Final Repair\n" +
                    "The village water pipes have burst! Zuri must locate and repair " +
                    "three broken pipe sections scattered across a rugged landscape. " +
                    "Collect materials, avoid dangers, and fix every pipe to complete " +
                    "the water system and save the village for good!";
                scene = "Level3End";
                break;

            default: // level 3 / end of game
                title    = "MISSION COMPLETE!";
                congrats =
                    "Zuri has done it! All three pipes are repaired and clean water\n" +
                    "now flows to every home in the village.\n" +
                    "The community is saved — Zuri is a true hero!";
                nextInfo =
                    "End of Zuri's Mission\n" +
                    "Thanks to Zuri's bravery, determination, and hard work, the " +
                    "village will never go without clean water again. The crops grow, " +
                    "the children are healthy, and hope is restored.\n" +
                    "Thank you for playing Zuri's Mission!";
                scene = "";
                break;
        }

        _nextScene         = scene;
        _titleText.text    = title;
        _statsText.text    = BuildStats(level);
        _congratsText.text = congrats;
        _nextInfoText.text = nextInfo;

        _canvasGO.SetActive(true);
    }

    // ── Stats text builder ─────────────────────────────────────────────────
    string BuildStats(int level)
    {
        if (level <= 2 && _hud != null)
            return $"Health:             {_hud.Health:F0} / 100\n" +
                   $"Water Level:     {_hud.PlayerWater:F0} / 100\n" +
                   $"Materials:          {_hud.MaterialLevel} / 100\n" +
                   $"Village Progress: {_hud.VillageProgressPercent:F0}%";

        if (level == 3 && _pipe != null)
            return $"Health:             {_pipe.Health:F0} / 100\n" +
                   $"Materials:          {_pipe.materialLevel} / 100\n" +
                   $"Village Progress: {_pipe.VillageLevel:F0}%\n" +
                   $"Pipes Repaired:  {_pipe.Tank1Amount}%  /  {_pipe.Tank2Amount}%  /  {_pipe.Tank3Amount}%";

        return "(Stats unavailable)";
    }

    // ── Continue button handler ────────────────────────────────────────────
    void OnContinue()
    {
        _showing = false;
        Time.timeScale = 1f;
        _canvasGO.SetActive(false);
        if (!string.IsNullOrEmpty(_nextScene))
            SceneManager.LoadScene(_nextScene);
        // Level 3 end — stays on screen (or you can add main-menu navigation here)
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI construction — all done in code, nothing needed in the Inspector
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        // ── Canvas ─────────────────────────────────────────────────────────
        _canvasGO = new GameObject("ELD_Canvas");
        _canvasGO.transform.SetParent(transform);

        var cv = _canvasGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 200;   // on top of everything

        var cs = _canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;

        _canvasGO.AddComponent<GraphicRaycaster>();

        // ── Dark full-screen overlay ────────────────────────────────────────
        Img(_canvasGO, "Overlay", new Color(0, 0, 0, 0.72f),
            V(0, 0), V(1, 1));

        // ── Main dialogue panel (wood-framed parchment, 9-sliced) ───────────
        var panelGO = Img(_canvasGO, "Panel", Color.white, V(0.12f, 0.03f), V(0.88f, 0.97f));
        var panelImg = panelGO.GetComponent<Image>();
        Sprite panelSprite = Panel1();
        if (panelSprite != null) { panelImg.sprite = panelSprite; panelImg.type = Image.Type.Sliced; }
        else panelImg.color = new Color(0.30f, 0.20f, 0.12f, 0.97f);
        var panel = panelGO;

        // ── Header bar (warm wood tone) ──────────────────────────────────────
        Img(panel, "TopBar", new Color(0.42f, 0.27f, 0.12f, 1f),
            V(0, 0.93f), V(1, 1f));

        // ── Title ───────────────────────────────────────────────────────────
        _titleText = Txt(panel, "Title", "", 52, FontStyles.Bold,
                         ColDarkTitle,
                         V(0.03f, 0.85f), V(0.97f, 0.93f));

        // ── Divider ─────────────────────────────────────────────────────────
        Img(panel, "Div1", ColDivider,
            V(0.03f, 0.838f), V(0.97f, 0.843f));

        // ── Stats header ────────────────────────────────────────────────────
        Txt(panel, "StatsLbl", "—  MISSION STATS  —", 24, FontStyles.Bold,
            ColDarkTitle,
            V(0.03f, 0.795f), V(0.97f, 0.838f));

        // ── Stats body ──────────────────────────────────────────────────────
        _statsText = Txt(panel, "StatsBody", "", 24, FontStyles.Normal,
                         ColDarkText, V(0.06f, 0.68f), V(0.97f, 0.795f));
        _statsText.alignment = TextAlignmentOptions.Left;

        // ── Divider ─────────────────────────────────────────────────────────
        Img(panel, "Div2", ColDivider,
            V(0.03f, 0.668f), V(0.97f, 0.673f));

        // ── Congratulations text ────────────────────────────────────────────
        _congratsText = Txt(panel, "Congrats", "", 22, FontStyles.Italic,
                            ColDarkText,
                            V(0.04f, 0.50f), V(0.96f, 0.668f));

        // ── Divider ─────────────────────────────────────────────────────────
        Img(panel, "Div3", ColDivider,
            V(0.03f, 0.488f), V(0.97f, 0.493f));

        // ── Next mission header ─────────────────────────────────────────────
        Txt(panel, "NextLbl", "—  NEXT MISSION  —", 24, FontStyles.Bold,
            ColDarkTitle,
            V(0.03f, 0.448f), V(0.97f, 0.488f));

        // ── Next mission body ───────────────────────────────────────────────
        _nextInfoText = Txt(panel, "NextBody", "", 21, FontStyles.Normal,
                            ColDarkText,
                            V(0.04f, 0.26f), V(0.96f, 0.448f));
        _nextInfoText.alignment = TextAlignmentOptions.Left;

        // ── Continue button (wood-bordered art sprite, label baked in) ──────
        var btnGO = new GameObject("ContinueBtn");
        btnGO.transform.SetParent(panel.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        var btnRt = btnGO.GetComponent<RectTransform>();
        btnRt.anchorMin = V(0.28f, 0.07f); btnRt.anchorMax = V(0.72f, 0.20f);
        btnRt.offsetMin = btnRt.offsetMax = Vector2.zero;

        _continueBtn = btnGO.AddComponent<Button>();
        _continueBtn.targetGraphic = btnImg;

        Sprite contSprite = ContinueButtonSprite();
        if (contSprite != null)
        {
            btnImg.sprite = contSprite;
            btnImg.color  = Color.white;

            Canvas.ForceUpdateCanvases();
            float boxW = btnRt.rect.width, boxH = btnRt.rect.height;
            float targetAspect = contSprite.rect.width / contSprite.rect.height;
            float fitW, fitH;
            if (targetAspect > boxW / boxH) { fitW = boxW; fitH = boxW / targetAspect; }
            else { fitH = boxH; fitW = boxH * targetAspect; }
            float dx = (boxW - fitW) * 0.5f, dy = (boxH - fitH) * 0.5f;
            btnRt.offsetMin = new Vector2(dx, dy);
            btnRt.offsetMax = new Vector2(-dx, -dy);

            var cols = _continueBtn.colors;
            cols.normalColor      = Color.white;
            cols.highlightedColor = Color.Lerp(Color.white, Color.yellow, 0.2f);
            cols.pressedColor     = Color.Lerp(Color.white, Color.gray, 0.3f);
            _continueBtn.colors   = cols;
        }
        else
        {
            btnImg.color = new Color(0.13f, 0.48f, 0.13f, 1f);
            var cols = _continueBtn.colors;
            cols.normalColor      = new Color(0.13f, 0.48f, 0.13f, 1f);
            cols.highlightedColor = new Color(0.20f, 0.68f, 0.20f, 1f);
            cols.pressedColor     = new Color(0.07f, 0.28f, 0.07f, 1f);
            _continueBtn.colors   = cols;

            var btnTxt = Txt(btnGO, "BtnLabel", "CONTINUE  >>", 32,
                             FontStyles.Bold, Color.white,
                             V(0, 0), V(1, 1));
            btnTxt.alignment = TextAlignmentOptions.Center;
        }

        _continueBtn.onClick.AddListener(OnContinue);

        // ── Hidden by default ───────────────────────────────────────────────
        _canvasGO.SetActive(false);
    }

    // ── UI helpers ─────────────────────────────────────────────────────────
    static Vector2 V(float x, float y) => new Vector2(x, y);

    static GameObject Img(GameObject parent, string name, Color col,
                           Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = col;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
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
        t.text      = text;
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = col;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.Normal;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }
}
