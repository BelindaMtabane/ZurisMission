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
#if UNITY_EDITOR
        // Editor-only testing switch: Tools > Zuri > Skip Start Screen When Testing
        if (UnityEditor.EditorPrefs.GetBool("Zuri.SkipStartScreen", false))
        {
            _startScreenShown = true;
            TryCreate(current);
            return;
        }
#endif

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

    static void ApplyPanelSprite(Image img)
    {
        AdventureUI.ApplyPanel(img);
        if (img.sprite == null) img.color = ColPanelSolid;
    }

    GameObject card;
    GameObject settingsPanel;
    GameObject creditsPanel;
    TMP_Text soundBtnLabel;
    TMP_Text volumeLabel;
    Slider volumeSlider;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        BuildUI();
        GameAudio.OnChanged += RefreshSoundUi;
        RefreshSoundUi();
    }

    void OnDestroy()
    {
        GameAudio.OnChanged -= RefreshSoundUi;
    }

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
        var cardImg = card.AddComponent<Image>();
        AdventureUI.ApplyPanel(cardImg);
        if (cardImg.sprite == null) cardImg.color = ColOverlay;
        else cardImg.color = new Color(1f, 1f, 1f, 0.98f);

        // ── Title ──────────────────────────────────────────────────────────
        MakeTxt(card, "SSB_Title",
                "ZURI'S MISSION",
                36, FontStyles.Bold, ColTitle,
                new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.96f));

        MakeTxt(card, "SSB_Sub",
                "Help Zuri bring water to her village",
                18, FontStyles.Bold, ColSubtitle,
                new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.80f));

        var btnL2 = MakeButton(card, "SSB_Level2Btn",
                               "LEVEL 2", ColBtnSettings, AdventureUI.BtnMedium,
                               new Vector2(0.08f, 0.28f), new Vector2(0.48f, 0.40f),
                               overlayLabel: true);
        btnL2.onClick.AddListener(() => LoadGameplay(SceneCatalog.Level2));

        var btnL3 = MakeButton(card, "SSB_Level3Btn",
                               "LEVEL 3", ColBtnStart, AdventureUI.BtnMedium,
                               new Vector2(0.52f, 0.28f), new Vector2(0.92f, 0.40f),
                               overlayLabel: true);
        btnL3.onClick.AddListener(() => LoadGameplay(SceneCatalog.Level3));

        var btnSettings = MakeButton(card, "SSB_SettingsBtn",
                                     "SETTINGS", ColBtnSettings, AdventureUI.BtnSetting,
                                     new Vector2(0.20f, 0.16f), new Vector2(0.80f, 0.26f));
        btnSettings.onClick.AddListener(OpenSettings);

        var btnExit = MakeButton(card, "SSB_ExitBtn",
                                 "EXIT", ColBtnExit, AdventureUI.BtnQuit,
                                 new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.14f));
        btnExit.onClick.AddListener(() => Application.Quit());

        // Created last so its hitbox sits above Level 2/3 if anything overlaps.
        var btnStart = MakeButton(card, "SSB_StartBtn",
                                  "NEW GAME", ColBtnStart, AdventureUI.BtnMedium,
                                  new Vector2(0.10f, 0.46f), new Vector2(0.90f, 0.66f),
                                  overlayLabel: true);
        btnStart.onClick.AddListener(StartNewGame);
        btnStart.transform.SetAsLastSibling();

        BuildSettingsPanel(cvGO);
        BuildCreditsPanel(cvGO);
        GameAudio.HookAllButtons();
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
                new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.97f));

        var btnCredits = MakeButton(settingsPanel, "SP_CreditsBtn",
                                    "CREDITS", ColBtnSettings, AdventureUI.BtnMedium,
                                    new Vector2(0.15f, 0.68f), new Vector2(0.85f, 0.82f),
                                    overlayLabel: true);
        btnCredits.onClick.AddListener(OpenCredits);

        var soundBtnGO = MakeButton(settingsPanel, "SP_SoundBtn",
                                    "SOUND ON", ColBtnSettings, AdventureUI.BtnMedium,
                                    new Vector2(0.15f, 0.52f), new Vector2(0.85f, 0.66f),
                                    overlayLabel: true);
        soundBtnLabel = soundBtnGO.GetComponentInChildren<TMP_Text>();
        soundBtnGO.onClick.AddListener(ToggleSound);

        volumeLabel = MakeTxt(settingsPanel, "SP_VolumeLbl", "VOLUME  100", 18, FontStyles.Bold, ColPanelText,
                new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.51f));

        volumeSlider = BuildVolumeSlider(settingsPanel,
                new Vector2(0.10f, 0.30f), new Vector2(0.90f, 0.42f));
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        var btnClose = MakeButton(settingsPanel, "SP_CloseBtn",
                                  "CLOSE", ColBtnExit, AdventureUI.BtnMedium,
                                  new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.24f),
                                  overlayLabel: true);
        btnClose.onClick.AddListener(CloseSettings);

        settingsPanel.SetActive(false);
        RefreshSoundUi();
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
                                 "BACK", ColBtnSettings, AdventureUI.BtnMedium,
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
        RefreshSoundUi();
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false);
        card.SetActive(true);
    }

    static void StartNewGame()
    {
        Debug.Log("[StartScreen] New Game -> Level 1 (MainGame)");
        LoadGameplay(SceneCatalog.MainGame);
    }

    static void LoadGameplay(string sceneName)
    {
        Time.timeScale = 1f;
        SceneLoadOverlay.Load(sceneName);
    }

    void OpenCredits()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    void ToggleSound()
    {
        bool turningOff = !GameAudio.Muted;
        if (turningOff) GameAudio.PlayClick();
        GameAudio.ToggleMuted();
        if (!turningOff) GameAudio.PlayClick();
    }

    void OnVolumeChanged(float value)
    {
        GameAudio.SetVolume(value);
    }

    void RefreshSoundUi()
    {
        GameAudio.Ensure();
        bool on = !GameAudio.Muted && GameAudio.Volume > 0.001f;
        if (soundBtnLabel != null) soundBtnLabel.text = on ? "SOUND ON" : "SOUND OFF";
        if (volumeLabel != null) volumeLabel.text = $"VOLUME  {Mathf.RoundToInt(GameAudio.Volume * 100)}";
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(GameAudio.Volume);
    }

    Slider BuildVolumeSlider(GameObject parent, Vector2 aMin, Vector2 aMax)
    {
        Slider slider = AdventureUI.BuildHorizontalBar(
            parent.transform, "SP_VolumeSlider", AdventureUI.FillGreen,
            aMin, aMax, out _);
        slider.interactable = true;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.transition = Selectable.Transition.None;
        Image track = slider.GetComponent<Image>();
        if (track != null) track.raycastTarget = true;
        if (slider.fillRect != null) slider.fillRect.GetComponent<Image>().raycastTarget = false;
        slider.value = GameAudio.Volume;
        return slider;
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
        t.raycastTarget    = false;
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
            img.sprite = artSprite;
            img.color  = Color.white;
            img.type   = Image.Type.Simple;
            img.preserveAspect = true;

            colors.normalColor      = Color.white;
            colors.highlightedColor = Color.Lerp(Color.white, Color.yellow, 0.2f);
            colors.pressedColor     = Color.Lerp(Color.white, Color.gray, 0.3f);
            colors.selectedColor    = Color.white;

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
}
