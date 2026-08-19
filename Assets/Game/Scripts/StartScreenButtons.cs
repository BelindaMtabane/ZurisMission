using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Auto-creates a styled Start / Settings / Exit button panel in the
/// StartScreen scene. Uses SceneManager.sceneLoaded so it fires on every
/// scene change, not just the first scene of the session.
/// </summary>
public class StartScreenButtons : MonoBehaviour
{
    // Static flag — resets every Play session (domain reload resets statics).
    static bool _startScreenShown = false;

    // ── Session bootstrap — redirect + register for every scene load ───────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        // Register callback now so it fires on every future scene change.
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);

        string current = SceneManager.GetActiveScene().name;

        // If Play was pressed while a gameplay scene was open, redirect to
        // StartScreen first so the opening screen is always shown.
        if (!_startScreenShown && current != "StartScreen")
        {
            _startScreenShown = true;   // prevent redirect loop
            SceneManager.LoadScene("StartScreen");
            return;                     // sceneLoaded callback handles the rest
        }

        TryCreate(current);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "StartScreen") return;
        _startScreenShown = true;
        if (FindFirstObjectByType<StartScreenButtons>() != null) return;
        new GameObject("StartScreenButtons").AddComponent<StartScreenButtons>();
    }

    // ── Colour helpers ────────────────────────────────────────────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    static readonly Color ColOverlay    = C(0.04f, 0.08f, 0.04f, 0.55f);
    static readonly Color ColPanelSolid = C(0.05f, 0.10f, 0.05f, 0.92f);
    static readonly Color ColTitle      = C(1.00f, 0.82f, 0.20f);
    static readonly Color ColSubtitle   = C(1.00f, 0.97f, 0.85f);
    static readonly Color ColBtnStart   = C(0.10f, 0.55f, 0.10f, 0.90f);
    static readonly Color ColBtnSettings= C(0.15f, 0.40f, 0.65f, 0.90f);
    static readonly Color ColBtnExit    = C(0.55f, 0.10f, 0.10f, 0.85f);
    static readonly Color ColBtnText    = C(1.00f, 1.00f, 1.00f);
    static readonly Color ColPanelText  = C(0.30f, 0.19f, 0.09f);   // dark brown — readable on parchment/wood panels

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

    static Sprite SettingsBtnSprite() => UiKitSprite("UIKit_07");
    static Sprite WoodFrameSprite()   => UiKitSprite("UIKit_10");

    static void ApplyPanelSprite(Image img)
    {
        Sprite s = Panel1();
        if (s == null) { img.color = ColPanelSolid; return; }
        img.sprite = s;
        img.type   = Image.Type.Sliced;
        img.color  = Color.white;
    }

    GameObject card;
    GameObject settingsPanel;
    GameObject creditsPanel;
    TMP_Text soundBtnLabel;
    bool soundOn = true;

    // ══════════════════════════════════════════════════════════════════════
    void Awake() => BuildUI();

    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        // ── Canvas ─────────────────────────────────────────────────────────
        var cvGO = new GameObject("SSB_Canvas");
        cvGO.transform.SetParent(transform);
        var cv = cvGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 200;
        var cs = cvGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        // ── Central card  (35 %→65 % wide, 15 %→75 % tall) ────────────────
        card = MakeRect(cvGO, "SSB_Card");
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.35f, 0.15f);
        cardRT.anchorMax = new Vector2(0.65f, 0.75f);
        cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;
        card.AddComponent<Image>().color = ColOverlay;

        // ── Title ──────────────────────────────────────────────────────────
        MakeTxt(card, "SSB_Title",
                "ZURI'S MISSION",
                36, FontStyles.Bold, ColTitle,
                new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.96f));

        // ── Subtitle ───────────────────────────────────────────────────────
        MakeTxt(card, "SSB_Sub",
                "Help Zuri bring water to her village",
                26, FontStyles.Bold, ColSubtitle,
                new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.74f));

        // ── NEW GAME ───────────────────────────────────────────────────────
        var btnStart = MakeButton(card, "SSB_StartBtn",
                                  "START GAME", ColBtnStart, LoadUISprite("newgameBTN"),
                                  new Vector2(0.10f, 0.44f), new Vector2(0.90f, 0.60f));
        btnStart.onClick.AddListener(() => SceneManager.LoadScene("MainGame"));

        // ── SETTINGS ───────────────────────────────────────────────────────
        var btnSettings = MakeButton(card, "SSB_SettingsBtn",
                                     "SETTINGS", ColBtnSettings, SettingsBtnSprite(),
                                     new Vector2(0.20f, 0.26f), new Vector2(0.80f, 0.42f));
        btnSettings.onClick.AddListener(OpenSettings);

        // ── EXIT ───────────────────────────────────────────────────────────
        var btnExit = MakeButton(card, "SSB_ExitBtn",
                                 "EXIT", ColBtnExit, LoadUISprite("exitBTN"),
                                 new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.24f));
        btnExit.onClick.AddListener(() => Application.Quit());

        BuildSettingsPanel(cvGO);
        BuildCreditsPanel(cvGO);
    }

    // ── Settings panel: Credits / Sound / Close ─────────────────────────────
    void BuildSettingsPanel(GameObject canvasGO)
    {
        settingsPanel = MakeRect(canvasGO, "SSB_SettingsPanel");
        var rt = settingsPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.15f);
        rt.anchorMax = new Vector2(0.65f, 0.75f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        ApplyPanelSprite(settingsPanel.AddComponent<Image>());

        MakeTxt(settingsPanel, "SP_Title", "SETTINGS", 32, FontStyles.Bold, ColPanelText,
                new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.94f));

        var btnCredits = MakeButton(settingsPanel, "SP_CreditsBtn",
                                    "CREDITS", ColBtnSettings, WoodFrameSprite(),
                                    new Vector2(0.15f, 0.54f), new Vector2(0.85f, 0.70f),
                                    overlayLabel: true);
        btnCredits.onClick.AddListener(OpenCredits);

        var soundBtnGO = MakeButton(settingsPanel, "SP_SoundBtn",
                                    "SOUND: ON", ColBtnSettings, WoodFrameSprite(),
                                    new Vector2(0.15f, 0.34f), new Vector2(0.85f, 0.50f),
                                    overlayLabel: true);
        soundBtnLabel = soundBtnGO.GetComponentInChildren<TMP_Text>();
        soundBtnGO.onClick.AddListener(ToggleSound);

        var btnClose = MakeButton(settingsPanel, "SP_CloseBtn",
                                  "CLOSE", ColBtnExit, WoodFrameSprite(),
                                  new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.24f),
                                  overlayLabel: true);
        btnClose.onClick.AddListener(CloseSettings);

        settingsPanel.SetActive(false);
    }

    // ── Credits panel ────────────────────────────────────────────────────
    void BuildCreditsPanel(GameObject canvasGO)
    {
        creditsPanel = MakeRect(canvasGO, "SSB_CreditsPanel");
        var rt = creditsPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.15f);
        rt.anchorMax = new Vector2(0.65f, 0.75f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        ApplyPanelSprite(creditsPanel.AddComponent<Image>());

        MakeTxt(creditsPanel, "CP_Title", "CREATED BY", 32, FontStyles.Bold, ColPanelText,
                new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.94f));

        MakeTxt(creditsPanel, "CP_Names", "Belinda Mtabane\nHarriet Manda",
                28, FontStyles.Bold, ColPanelText,
                new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.74f));

        var btnBack = MakeButton(creditsPanel, "CP_BackBtn",
                                 "BACK", ColBtnSettings, WoodFrameSprite(),
                                 new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.24f),
                                 overlayLabel: true);
        btnBack.onClick.AddListener(() =>
        {
            creditsPanel.SetActive(false);
            settingsPanel.SetActive(true);
        });

        creditsPanel.SetActive(false);
    }

    void OpenSettings()
    {
        card.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false);
        card.SetActive(true);
    }

    void OpenCredits()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    void ToggleSound()
    {
        soundOn = !soundOn;
        AudioListener.volume = soundOn ? 1f : 0f;
        if (soundBtnLabel != null) soundBtnLabel.text = soundOn ? "SOUND: ON" : "SOUND: OFF";
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static GameObject MakeRect(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static TMP_Text MakeTxt(GameObject parent, string name, string text,
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

    static Button MakeButton(GameObject parent, string name, string label,
                              Color bgCol, Sprite artSprite, Vector2 aMin, Vector2 aMax,
                              bool overlayLabel = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;

        if (artSprite != null)
        {
            // Art asset already has its own label baked in — no text overlay needed.
            img.sprite      = artSprite;
            img.color       = Color.white;

            // Inset the rect to match the sprite's own aspect ratio, centered
            // within the anchor slot. (AspectRatioFitter's Parent modes force
            // full-stretch anchors to the immediate parent, which would blow
            // this button out to the whole card — not what we want here.)
            // Force a layout pass first: rt.rect is unreliable immediately
            // after (re)parenting/anchoring, before Canvas has resolved it.
            Canvas.ForceUpdateCanvases();
            float boxW = rt.rect.width;
            float boxH = rt.rect.height;
            float targetAspect = artSprite.rect.width / artSprite.rect.height;
            float fitW, fitH;
            if (targetAspect > boxW / boxH) { fitW = boxW; fitH = boxW / targetAspect; }
            else { fitH = boxH; fitW = boxH * targetAspect; }
            float dx = (boxW - fitW) * 0.5f;
            float dy = (boxH - fitH) * 0.5f;
            rt.offsetMin = new Vector2(dx, dy);
            rt.offsetMax = new Vector2(-dx, -dy);

            colors.normalColor      = Color.white;
            colors.highlightedColor = Color.Lerp(Color.white, Color.yellow, 0.2f);
            colors.pressedColor     = Color.Lerp(Color.white, Color.gray, 0.3f);
            colors.selectedColor    = Color.white;

            // Generic wood frames have no baked-in text — overlay the label.
            if (overlayLabel)
            {
                MakeTxt(go, name + "_Lbl", label, 20, FontStyles.Bold, ColBtnText,
                        new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f));
            }
        }
        else
        {
            img.color = bgCol;
            colors.normalColor      = bgCol;
            colors.highlightedColor = Color.Lerp(bgCol, Color.white, 0.25f);
            colors.pressedColor     = Color.Lerp(bgCol, Color.black, 0.25f);
            colors.selectedColor    = bgCol;

            MakeTxt(go, name + "_Lbl", label, 20, FontStyles.Bold, ColBtnText,
                    new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f));
        }

        btn.colors        = colors;
        btn.targetGraphic = img;
        return btn;
    }

    static Sprite LoadUISprite(string name)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("UI/" + name);
        return sprites.Length > 0 ? sprites[0] : null;
    }
}
