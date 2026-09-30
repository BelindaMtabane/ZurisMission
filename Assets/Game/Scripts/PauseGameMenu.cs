using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pause menu in the same panel style as the start screen.
/// Resume, Settings, and Main Menu work now. Tutorial opens a holding page
/// until the tutorial clip is added.
/// </summary>
public class PauseGameMenu : MonoBehaviour
{
    public static PauseGameMenu Instance { get; private set; }

    static readonly Color ColScrim = new Color(0.04f, 0.08f, 0.04f, 0.55f);
    static readonly Color ColPanelSolid = new Color(0.05f, 0.10f, 0.05f, 0.92f);
    static readonly Color ColTitle = new Color(1f, 0.82f, 0.20f);
    static readonly Color ColSubtitle = new Color(1f, 0.97f, 0.85f);
    static readonly Color ColBtnStart = new Color(0.10f, 0.55f, 0.10f, 0.90f);
    static readonly Color ColBtnSettings = new Color(0.15f, 0.40f, 0.65f, 0.90f);
    static readonly Color ColBtnExit = new Color(0.55f, 0.10f, 0.10f, 0.85f);
    static readonly Color ColBtnText = Color.white;
    static readonly Color ColPanelText = new Color(0.30f, 0.19f, 0.09f);

    GameObject root;
    GameObject card;
    GameObject settingsPanel;
    GameObject creditsPanel;
    GameObject tutorialPanel;
    TMP_Text soundBtnLabel;
    TMP_Text volumeLabel;
    Slider volumeSlider;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (!SceneCatalog.IsRunnerScene(sceneName)) return;
        if (FindFirstObjectByType<PauseGameMenu>() != null) return;
        new GameObject("PauseGameMenu").AddComponent<PauseGameMenu>();
    }

    public static void Show()
    {
        if (!SceneCatalog.IsRunnerScene(SceneCatalog.ActiveName)) return;
        if (Instance == null)
            new GameObject("PauseGameMenu").AddComponent<PauseGameMenu>();
        Instance.Open();
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance.Close();
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
        GameAudio.OnChanged += RefreshSoundUi;
        Close();
    }

    void OnDestroy()
    {
        GameAudio.OnChanged -= RefreshSoundUi;
        if (Instance == this) Instance = null;
    }

    void Open()
    {
        if (root != null) root.SetActive(true);
        ShowCard();
        RefreshSoundUi();
    }

    void Close()
    {
        if (root != null) root.SetActive(false);
    }

    void ShowCard()
    {
        if (card != null) card.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    void BuildUI()
    {
        root = new GameObject("PauseMenuCanvas");
        root.transform.SetParent(transform, false);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        GameObject scrim = MakeRect(root, "Scrim");
        Stretch(scrim);
        Image scrimImg = scrim.AddComponent<Image>();
        scrimImg.color = ColScrim;
        scrimImg.raycastTarget = true;

        card = MakeRect(root, "PauseCard");
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.35f, 0.12f);
        cardRt.anchorMax = new Vector2(0.65f, 0.88f);
        cardRt.offsetMin = cardRt.offsetMax = Vector2.zero;
        Image cardImg = card.AddComponent<Image>();
        AdventureUI.ApplyPanel(cardImg);
        if (cardImg.sprite == null) cardImg.color = ColPanelSolid;
        else cardImg.color = new Color(1f, 1f, 1f, 0.98f);

        MakeTxt(card, "Title", "DROP BY DROP", 36, FontStyles.Bold, ColTitle,
            new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.96f));
        MakeTxt(card, "Sub", "PAUSED", 22, FontStyles.Bold, ColSubtitle,
            new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.84f));

        Button resume = MakeButton(card, "Resume", "RESUME", ColBtnStart, AdventureUI.BtnResume ?? AdventureUI.BtnMedium,
            new Vector2(0.12f, 0.58f), new Vector2(0.88f, 0.72f), false);
        resume.onClick.AddListener(OnResume);

        Button settings = MakeButton(card, "Settings", "SETTINGS", ColBtnSettings, AdventureUI.BtnSetting ?? AdventureUI.BtnMedium,
            new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.55f), false);
        settings.onClick.AddListener(OpenSettings);

        Button tutorial = MakeButton(card, "Tutorial", "TUTORIAL", ColBtnSettings, AdventureUI.BtnMedium,
            new Vector2(0.12f, 0.26f), new Vector2(0.88f, 0.39f), true);
        tutorial.onClick.AddListener(OpenTutorial);

        Button mainMenu = MakeButton(card, "MainMenu", "MAIN MENU", ColBtnExit, AdventureUI.BtnMedium,
            new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.22f), true);
        mainMenu.onClick.AddListener(OnMainMenu);

        BuildSettings(root);
        BuildCredits(root);
        BuildTutorial(root);
        GameAudio.HookAllButtons();
    }

    void BuildSettings(GameObject canvasGO)
    {
        settingsPanel = MakeRect(canvasGO, "Settings");
        RectTransform rt = settingsPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.12f);
        rt.anchorMax = new Vector2(0.65f, 0.88f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = settingsPanel.AddComponent<Image>();
        AdventureUI.ApplyPanel(img);
        if (img.sprite == null) img.color = ColPanelSolid;

        MakeTxt(settingsPanel, "Title", "SETTINGS", 32, FontStyles.Bold, ColPanelText,
            new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.96f));

        Button credits = MakeButton(settingsPanel, "Credits", "CREDITS", ColBtnSettings, AdventureUI.BtnMedium,
            new Vector2(0.15f, 0.66f), new Vector2(0.85f, 0.80f), true);
        credits.onClick.AddListener(OpenCredits);

        Button sound = MakeButton(settingsPanel, "Sound", "SOUND ON", ColBtnSettings, AdventureUI.BtnMedium,
            new Vector2(0.15f, 0.50f), new Vector2(0.85f, 0.64f), true);
        soundBtnLabel = sound.GetComponentInChildren<TMP_Text>();
        sound.onClick.AddListener(ToggleSound);

        volumeLabel = MakeTxt(settingsPanel, "VolumeLabel", "VOLUME  100", 18, FontStyles.Bold, ColPanelText,
            new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.48f));

        volumeSlider = AdventureUI.BuildHorizontalBar(
            settingsPanel.transform, "Volume", AdventureUI.FillGreen,
            new Vector2(0.10f, 0.28f), new Vector2(0.90f, 0.38f), out _);
        volumeSlider.interactable = true;
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;
        volumeSlider.transition = Selectable.Transition.None;
        Image track = volumeSlider.GetComponent<Image>();
        if (track != null) track.raycastTarget = true;
        if (volumeSlider.fillRect != null)
        {
            Image fill = volumeSlider.fillRect.GetComponent<Image>();
            if (fill != null) fill.raycastTarget = false;
        }
        volumeSlider.onValueChanged.AddListener(GameAudio.SetVolume);
        volumeSlider.value = GameAudio.Volume;

        Button close = MakeButton(settingsPanel, "Close", "CLOSE", ColBtnExit, AdventureUI.BtnMedium,
            new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.22f), true);
        close.onClick.AddListener(ShowCard);
        settingsPanel.SetActive(false);
    }

    void BuildCredits(GameObject canvasGO)
    {
        creditsPanel = MakeRect(canvasGO, "Credits");
        RectTransform rt = creditsPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.12f);
        rt.anchorMax = new Vector2(0.65f, 0.88f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = creditsPanel.AddComponent<Image>();
        AdventureUI.ApplyPanel(img);
        if (img.sprite == null) img.color = ColPanelSolid;

        MakeTxt(creditsPanel, "Title", "CREATED BY", 32, FontStyles.Bold, ColPanelText,
            new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.94f));
        MakeTxt(creditsPanel, "Names", "Belinda Mtabane\nHarriet Manda", 28, FontStyles.Bold, ColPanelText,
            new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.74f));

        Button back = MakeButton(creditsPanel, "Back", "BACK", ColBtnSettings, AdventureUI.BtnMedium,
            new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.24f), true);
        back.onClick.AddListener(() =>
        {
            creditsPanel.SetActive(false);
            settingsPanel.SetActive(true);
        });
        creditsPanel.SetActive(false);
    }

    void BuildTutorial(GameObject canvasGO)
    {
        tutorialPanel = MakeRect(canvasGO, "Tutorial");
        RectTransform rt = tutorialPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.12f);
        rt.anchorMax = new Vector2(0.65f, 0.88f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = tutorialPanel.AddComponent<Image>();
        AdventureUI.ApplyPanel(img);
        if (img.sprite == null) img.color = ColPanelSolid;

        MakeTxt(tutorialPanel, "Title", "TUTORIAL", 32, FontStyles.Bold, ColPanelText,
            new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.94f));
        MakeTxt(tutorialPanel, "Body",
            "The tutorial clip will play here.\nSend the video and this page will teach the buttons.",
            22, FontStyles.Bold, ColPanelText,
            new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.74f));

        Button back = MakeButton(tutorialPanel, "Back", "BACK", ColBtnSettings, AdventureUI.BtnMedium,
            new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.24f), true);
        back.onClick.AddListener(ShowCard);
        tutorialPanel.SetActive(false);
    }

    void OpenSettings()
    {
        card.SetActive(false);
        tutorialPanel.SetActive(false);
        creditsPanel.SetActive(false);
        settingsPanel.SetActive(true);
        RefreshSoundUi();
    }

    void OpenCredits()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    void OpenTutorial()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneCatalog.Tutorial);
}

    void OnResume()
    {
        if (RunStateManager.Instance != null)
            RunStateManager.Instance.Resume();
        else
            Close();
    }

    void OnMainMenu()
    {
        Time.timeScale = 1f;
        if (RunStateManager.Instance != null)
            RunStateManager.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene(SceneCatalog.StartScreen);
    }

    void ToggleSound()
    {
        bool turningOff = !GameAudio.Muted;
        if (turningOff) GameAudio.PlayClick();
        GameAudio.ToggleMuted();
        if (!turningOff) GameAudio.PlayClick();
    }

    void RefreshSoundUi()
    {
        GameAudio.Ensure();
        bool on = !GameAudio.Muted && GameAudio.Volume > 0.001f;
        if (soundBtnLabel != null) soundBtnLabel.text = on ? "SOUND ON" : "SOUND OFF";
        if (volumeLabel != null) volumeLabel.text = $"VOLUME  {Mathf.RoundToInt(GameAudio.Volume * 100)}";
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(GameAudio.Volume);
    }

    static void Stretch(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static GameObject MakeRect(GameObject parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static TMP_Text MakeTxt(GameObject parent, string name, string text,
        float size, FontStyles style, Color col, Vector2 aMin, Vector2 aMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = col;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }

    static Button MakeButton(GameObject parent, string name, string label,
        Color bgCol, Sprite artSprite, Vector2 aMin, Vector2 aMax, bool overlayLabel)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        Button btn = go.GetComponent<Button>();
        ColorBlock colors = btn.colors;

        if (artSprite != null)
        {
            img.sprite = artSprite;
            img.color = Color.white;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.Lerp(Color.white, Color.yellow, 0.2f);
            colors.pressedColor = Color.Lerp(Color.white, Color.gray, 0.3f);
            colors.selectedColor = Color.white;
        }
        else
        {
            img.color = bgCol;
            colors.normalColor = bgCol;
            colors.highlightedColor = Color.Lerp(bgCol, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(bgCol, Color.black, 0.25f);
            colors.selectedColor = bgCol;
        }

        if (artSprite == null || overlayLabel)
        {
            MakeTxt(go, name + "Label", label, 20, FontStyles.Bold, ColBtnText,
                new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f));
        }

        btn.colors = colors;
        btn.targetGraphic = img;
        return btn;
    }
}
