using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RunStateManager : MonoBehaviour
{
    public static RunStateManager Instance { get; private set; }

    public enum RunState
    {
        WaitingToStart,
        Playing,
        Finishing,
        Paused,
        Dead,
        Victory
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject pausePanel;

    private TMP_Text deathReasonText;
    private string lastDeathReason = "You lost.";
    private TMP_Text victoryTitleText;
    private TMP_Text victoryMessageText;
    private GameObject startPrompt;
    private bool victorySequenceStarted;
    private bool finishAnimationCompleted;
    private bool beginRequested;
    private float waitingToStartSeconds;
    private static Sprite promptPanelSprite;
    private static TMP_FontAsset promptFont;

    // ── Shared end-screen palette (matches StartScreenButtons) ─────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    static readonly Color ColScrim     = C(0.02f, 0.02f, 0.02f, 0.72f);
    static readonly Color ColGoldTitle = Color.white;
    static readonly Color ColCream     = Color.white;
    static readonly Color ColLoseTitle = C(1f, 0.35f, 0.25f);      // bright red — "YOU LOST"
    static readonly Color ColLoseText  = Color.white;
    static readonly Color ColBtnRetry  = C(0.55f, 0.10f, 0.10f, 0.92f);
    static readonly Color ColBtnNext   = C(0.10f, 0.55f, 0.10f, 0.92f);
    static readonly Color ColBtnText   = Color.white;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = SceneCatalog.StartScreen;
    [SerializeField] private string nextScene = "";

    public RunState CurrentState { get; private set; } = RunState.WaitingToStart;
    public bool IsPlaying => CurrentState == RunState.Playing;

    public event Action OnRunStarted;
    public event Action OnRunDied;
    public event Action OnRunVictory;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        EnsureStartPrompt();
        SetState(RunState.WaitingToStart);
    }

    void Update()
    {
        if (CurrentState != RunState.WaitingToStart) return;

        waitingToStartSeconds += Time.unscaledDeltaTime;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        Gamepad gamepad = Gamepad.current;
        Touchscreen touchscreen = Touchscreen.current;
        bool keyboardPressed = keyboard != null && keyboard.anyKey.wasPressedThisFrame;
        bool mousePressed = mouse != null
                            && (mouse.leftButton.wasPressedThisFrame
                                || mouse.rightButton.wasPressedThisFrame);
        bool touchPressed = touchscreen != null
                            && touchscreen.primaryTouch.press.wasPressedThisFrame;
        bool legacyPressed = false;
        try
        {
            legacyPressed = Input.anyKeyDown;
        }
        catch (InvalidOperationException)
        {
            // The project can be configured for the new Input System only.
        }
        bool gamepadPressed = gamepad != null
                              && (gamepad.buttonSouth.wasPressedThisFrame
                                  || gamepad.buttonNorth.wasPressedThisFrame
                                  || gamepad.buttonEast.wasPressedThisFrame
                                  || gamepad.buttonWest.wasPressedThisFrame
                                  || gamepad.startButton.wasPressedThisFrame);
        if (keyboardPressed || mousePressed || touchPressed || legacyPressed || gamepadPressed)
        {
            RequestRunStart();
            return;
        }

        // Fail-safe for platforms/editor focus states that do not deliver an
        // initial input event. Zuri still shows her idle/get-ready pose first,
        // then gameplay can never remain permanently stuck on this panel.
        if (waitingToStartSeconds >= 4f) RequestRunStart();
    }

    public void SetupPanels(GameObject death, GameObject victory, GameObject pause)
    {
        if (death != null) deathPanel = death;
        if (victory != null) victoryPanel = victory;
        if (pause != null) pausePanel = pause;
    }

    public void SetupScenes(string mainMenu, string next)
    {
        if (!string.IsNullOrEmpty(mainMenu)) mainMenuScene = mainMenu;
        nextScene = next ?? "";
    }

    public void NotifyDeath(string reason = null)
    {
        if (CurrentState != RunState.Playing
            && !(CurrentState == RunState.Finishing && finishAnimationCompleted)) return;
        lastDeathReason = string.IsNullOrEmpty(reason) ? "You lost." : reason;
        EnsureLosePanel();
        SetState(RunState.Dead);
    }

    public void NotifyVictory(string title = null, string message = null)
    {
        if (CurrentState == RunState.Finishing)
        {
            if (!finishAnimationCompleted) return;
            EnsureVictoryPanel(title, message);
            SetState(RunState.Victory);
            return;
        }
        if (CurrentState != RunState.Playing) return;

        EnsureVictoryPanel(title, message);
        BeginFinishSequence(() => SetState(RunState.Victory));
    }

    public void Pause()
    {
        if (CurrentState != RunState.Playing) return;
        SetState(RunState.Paused);
    }

    public void Resume()
    {
        if (CurrentState != RunState.Paused) return;
        SetState(RunState.Playing);
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void GoToNextScene()
    {
        Time.timeScale = 1f;
        string scene = string.IsNullOrEmpty(nextScene) ? mainMenuScene : nextScene;
        SceneLoadOverlay.Load(scene);
    }

    void SetState(RunState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case RunState.WaitingToStart:
                Time.timeScale = 1f;
                SetPanels(false, false, false);
                PauseGameMenu.Hide();
                if (startPrompt != null) startPrompt.SetActive(true);
                break;

            case RunState.Playing:
                Time.timeScale = 1f;
                SetPanels(false, false, false);
                PauseGameMenu.Hide();
                if (startPrompt != null) startPrompt.SetActive(false);
                OnRunStarted?.Invoke();
                break;

            case RunState.Finishing:
                Time.timeScale = 1f;
                SetPanels(false, false, false);
                PauseGameMenu.Hide();
                if (startPrompt != null) startPrompt.SetActive(false);
                break;

            case RunState.Paused:
                Time.timeScale = 0f;
                SetPanels(false, false, false);
                PauseGameMenu.Show();
                break;

            case RunState.Dead:
                Time.timeScale = 0f;
                PauseGameMenu.Hide();
                EnsureLosePanel();
                if (deathReasonText != null)
                {
                    deathReasonText.text = lastDeathReason;
                }
                SetPanels(true, false, false);
                OnRunDied?.Invoke();
                break;

            case RunState.Victory:
                // NotifyVictory() already built/updated the panel with the
                // real title+message before calling SetState — calling
                // EnsureVictoryPanel() again with no args here would stomp
                // that copy back to the generic "YOU WIN" defaults.
                Time.timeScale = 0f;
                PauseGameMenu.Hide();
                SetPanels(false, true, false);
                OnRunVictory?.Invoke();
                break;
        }
    }

    void RequestRunStart()
    {
        if (beginRequested || CurrentState != RunState.WaitingToStart) return;
        beginRequested = true;
        SetState(RunState.Playing);
        // Always release any stale lock immediately so movement cannot remain
        // frozen if the scene was started by a UI click.
        FindFirstObjectByType<PlayerController>()?.SetInputLocked(false);
    }

    public void BeginFinishSequence(Action onComplete)
    {
        if (CurrentState != RunState.Playing || victorySequenceStarted) return;
        victorySequenceStarted = true;
        finishAnimationCompleted = false;
        StartCoroutine(PlayFinishSequence(onComplete));
    }

    IEnumerator PlayFinishSequence(Action onComplete)
    {
        SetState(RunState.Finishing);
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            yield return player.PlayFinishSequence();
        }

        finishAnimationCompleted = true;
        onComplete?.Invoke();

        // HUD win/lose only after walk + tender placement have finished.
        if (CurrentState == RunState.Finishing)
        {
            EnsureVictoryPanel("LEVEL COMPLETE", "Great work! Continue to the next level.");
            SetState(RunState.Victory);
        }
    }

    void EnsureStartPrompt()
    {
        if (startPrompt != null) return;

        GameObject canvasObject = new GameObject("StartRunCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 180;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        startPrompt = new GameObject("PressAnyKeyPrompt");
        startPrompt.transform.SetParent(canvasObject.transform, false);
        Image panel = startPrompt.AddComponent<Image>();
        panel.sprite = GetPromptPanelSprite();
        panel.type = Image.Type.Sliced;
        panel.color = Color.white;
        Button startButton = startPrompt.AddComponent<Button>();
        startButton.targetGraphic = panel;
        startButton.onClick.AddListener(RequestRunStart);
        RectTransform panelRect = startPrompt.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.32f, 0.38f);
        panelRect.anchorMax = new Vector2(0.68f, 0.60f);
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;

        TMP_FontAsset adventureFont = GetPromptFont();
        TMP_Text title = EndTxt(startPrompt, "Title", "GET READY", 46, FontStyles.Bold,
            new Color(0.24f, 0.82f, 0.22f),
            new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.88f));
        if (adventureFont != null) title.font = adventureFont;
        title.enableVertexGradient = true;
        title.colorGradient = new VertexGradient(
            new Color(0.65f, 1f, 0.26f),
            new Color(0.65f, 1f, 0.26f),
            new Color(0.08f, 0.58f, 0.12f),
            new Color(0.08f, 0.58f, 0.12f));
        title.outlineWidth = 0.38f;
        title.outlineColor = new Color(0.025f, 0.12f, 0.035f, 1f);
        TMP_Text instruction = EndTxt(startPrompt, "Instruction", "PRESS ANY KEY TO PLAY", 27, FontStyles.Bold,
            new Color(0.64f, 1f, 0.38f), new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.51f));
        if (adventureFont != null) instruction.font = adventureFont;
        instruction.enableAutoSizing = true;
        instruction.fontSizeMin = 18f;
        instruction.fontSizeMax = 27f;
        instruction.overflowMode = TextOverflowModes.Overflow;
        instruction.outlineWidth = 0.32f;
        instruction.outlineColor = new Color(0.025f, 0.10f, 0.035f, 1f);
    }

    static TMP_FontAsset GetPromptFont()
    {
        if (promptFont != null) return promptFont;

        // The NEW GAME lettering is baked into its button image, so use the
        // closest installed chunky display face and create a complete dynamic
        // TMP asset. This also avoids the incomplete sample SDF that rendered
        // the instruction line blank.
        string[] preferred =
        {
            "Cooper Black",
            "Arial Rounded MT Bold",
            "Showcard Gothic",
            "Arial Black",
            "Arial"
        };
        string[] installed = Font.GetOSInstalledFontNames();
        string selected = "Arial";
        for (int p = 0; p < preferred.Length; p++)
        {
            for (int i = 0; i < installed.Length; i++)
            {
                if (!string.Equals(installed[i], preferred[p], StringComparison.OrdinalIgnoreCase)) continue;
                selected = installed[i];
                p = preferred.Length;
                break;
            }
        }

        Font dynamicFont = Font.CreateDynamicFontFromOSFont(selected, 96);
        if (dynamicFont != null)
        {
            promptFont = TMP_FontAsset.CreateFontAsset(dynamicFont);
            if (promptFont != null) promptFont.name = "AdventurePromptFont";
        }

        return promptFont != null ? promptFont : TMP_Settings.defaultFontAsset;
    }

    static Sprite GetPromptPanelSprite()
    {
        if (promptPanelSprite != null) return promptPanelSprite;

        const int width = 128;
        const int height = 64;
        const int radius = 15;
        const int inset = 5;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "StartPromptAdventurePanel";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = Color.clear;
        Color frame = new Color(0.12f, 0.055f, 0.23f, 0.98f);
        Color rim = new Color(0.39f, 0.22f, 0.55f, 1f);
        Color innerTop = new Color(0.08f, 0.25f, 0.12f, 0.97f);
        Color innerBottom = new Color(0.025f, 0.105f, 0.055f, 0.97f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!InsideRoundedRect(x, y, width, height, radius))
                {
                    texture.SetPixel(x, y, clear);
                    continue;
                }

                if (!InsideRoundedRect(x - inset, y - inset,
                        width - inset * 2, height - inset * 2, radius - inset))
                {
                    texture.SetPixel(x, y, y > height * 0.55f ? rim : frame);
                    continue;
                }

                float t = y / (float)(height - 1);
                texture.SetPixel(x, y, Color.Lerp(innerBottom, innerTop, t));
            }
        }

        texture.Apply(false, true);
        promptPanelSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(22f, 22f, 22f, 22f));
        promptPanelSprite.name = "StartPromptAdventurePanel";
        return promptPanelSprite;
    }

    static bool InsideRoundedRect(int x, int y, int width, int height, int radius)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return false;
        if (x >= radius && x < width - radius) return true;
        if (y >= radius && y < height - radius) return true;

        float cx = x < radius ? radius : width - radius - 1;
        float cy = y < radius ? radius : height - radius - 1;
        float dx = x - cx;
        float dy = y - cy;
        return dx * dx + dy * dy <= radius * radius;
    }

    void SetPanels(bool death, bool victory, bool pause)
    {
        if (deathPanel != null) deathPanel.SetActive(death);
        if (victoryPanel != null) victoryPanel.SetActive(victory);
        if (pausePanel != null) pausePanel.SetActive(pause);
    }

    void EnsureLosePanel()
    {
        if (deathReasonText != null) return;

        Canvas canvas = MakeEndScreenCanvas("LoseCanvas");

        deathPanel = new GameObject("LosePanel");
        deathPanel.transform.SetParent(canvas.transform, false);
        Image bg = deathPanel.AddComponent<Image>();
        bg.color = ColScrim;
        RectTransform rt = deathPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject inner = new GameObject("Inner");
        inner.transform.SetParent(deathPanel.transform, false);
        Image innerImg = inner.AddComponent<Image>();
        ApplyPanelSprite(innerImg);
        RectTransform innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = new Vector2(0.22f, 0.18f);
        innerRt.anchorMax = new Vector2(0.78f, 0.82f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;

        EndTxt(inner, "Title", "YOU LOST", 42, FontStyles.Bold, ColLoseTitle,
               new Vector2(0.05f, 0.79f), new Vector2(0.95f, 0.94f));

        deathReasonText = EndTxt(inner, "Reason", lastDeathReason, 24, FontStyles.Bold, ColLoseText,
               new Vector2(0.07f, 0.31f), new Vector2(0.93f, 0.73f));
        deathReasonText.enableAutoSizing = true;
        deathReasonText.fontSizeMin = 17f;
        deathReasonText.fontSizeMax = 24f;
        deathReasonText.overflowMode = TextOverflowModes.Ellipsis;
        deathReasonText.lineSpacing = -4f;

        var restartButton = EndButton(inner, "Restart", "RESTART", ColBtnRetry,
            new Vector2(0.06f, 0.17f), new Vector2(0.48f, 0.29f),
            AdventureUI.BtnMedium, overlayLabel: true);
        restartButton.onClick.AddListener(RestartRun);

        var menuButton = EndButton(inner, "MainMenu", "MAIN MENU", new Color(0.20f, 0.28f, 0.42f, 0.95f),
            new Vector2(0.52f, 0.17f), new Vector2(0.94f, 0.29f),
            AdventureUI.BtnMedium, overlayLabel: true);
        menuButton.onClick.AddListener(GoToMainMenu);

        string activeScene = SceneManager.GetActiveScene().name;
        string testScene = SceneCatalog.IsLevel1(activeScene) ? SceneCatalog.Level2
            : SceneCatalog.IsLevel2(activeScene) ? SceneCatalog.Level3
            : "";
        if (!string.IsNullOrEmpty(testScene))
        {
            string testLabel = testScene == SceneCatalog.Level2 ? "GO TO LEVEL 2" : "GO TO LEVEL 3";
            var testButton = EndButton(inner, "TestNextLevel", testLabel, ColBtnNext,
                new Vector2(0.25f, 0.035f), new Vector2(0.75f, 0.145f),
                AdventureUI.BtnMedium, overlayLabel: true);
            testButton.onClick.AddListener(() => GoToScene(testScene));
        }

        deathPanel.SetActive(false);
        GameAudio.HookAllButtons();
    }

    void EnsureVictoryPanel(string title = null, string message = null)
    {
        if (victoryPanel != null && victoryTitleText != null)
        {
            ApplyVictoryCopy(title, message);
            return;
        }

        if (victoryPanel != null && string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message))
        {
            return;
        }

        Canvas canvas = MakeEndScreenCanvas("VictoryCanvas");

        victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(canvas.transform, false);
        Image bg = victoryPanel.AddComponent<Image>();
        bg.color = ColScrim;
        RectTransform rt = victoryPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject inner = new GameObject("Inner");
        inner.transform.SetParent(victoryPanel.transform, false);
        Image innerImg = inner.AddComponent<Image>();
        ApplyPanelSprite(innerImg);
        RectTransform innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = new Vector2(0.28f, 0.18f);
        innerRt.anchorMax = new Vector2(0.72f, 0.82f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;

        victoryTitleText = EndTxt(inner, "Title", "YOU WIN", 44, FontStyles.Bold, ColGoldTitle,
               new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.92f));

        victoryMessageText = EndTxt(inner, "Message", "You completed the level.", 26, FontStyles.Bold, ColCream,
               new Vector2(0.06f, 0.44f), new Vector2(0.94f, 0.72f));

        string nextLabel = nextScene == SceneCatalog.Level3 ? "START LEVEL 3"
            : nextScene == SceneCatalog.Level2 ? "START LEVEL 2"
            : nextScene == SceneCatalog.StartScreen || string.IsNullOrEmpty(nextScene) ? "MAIN MENU"
            : "NEXT LEVEL";
        var nextBtn = EndButton(inner, "NextLevel", nextLabel, ColBtnNext,
                                 new Vector2(0.15f, 0.10f), new Vector2(0.85f, 0.28f),
                                 AdventureUI.BtnMedium, overlayLabel: true);
        nextBtn.onClick.AddListener(GoToNextScene);

        ApplyVictoryCopy(title, message);
        victoryPanel.SetActive(false);
        GameAudio.HookAllButtons();
    }

    void ApplyVictoryCopy(string title, string message)
    {
        if (victoryTitleText != null)
        {
            victoryTitleText.text = string.IsNullOrEmpty(title) ? "YOU WIN" : title;
        }

        if (victoryMessageText != null)
        {
            victoryMessageText.text = string.IsNullOrEmpty(message)
                ? "You completed the level."
                : message;
        }
    }

    void GoToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        Time.timeScale = 1f;
        SceneLoadOverlay.Load(sceneName);
    }

    // ── Shared end-screen helpers ────────────────────────────────────────
    Canvas MakeEndScreenCanvas(string name)
    {
        // Always create a dedicated canvas rather than reusing "any" canvas
        // found in the scene — reusing was order-dependent (which canvas
        // FindFirstObjectByType happened to return first) and would let
        // other HUD canvases render on top of the scrim/panel depending on
        // creation order. sortingOrder 200 matches EndLevelDialogue so end
        // screens always sit above the gameplay HUD (sortingOrder 140).
        GameObject canvasGo = new GameObject(name);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void ApplyPanelSprite(Image img)
    {
        AdventureUI.ApplyPanel(img);
        if (img.sprite == null) img.color = new Color(0.30f, 0.20f, 0.12f, 0.96f);
    }

    static TMP_Text EndTxt(GameObject parent, string name, string text,
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
        t.outlineColor     = new Color32(25, 12, 35, 235);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }

    static Button EndButton(GameObject parent, string name, string label, Color bgCol,
                             Vector2 aMin, Vector2 aMax, Sprite artSprite = null,
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
            // Wood-bordered art sprite — aspect-fit it within the slot instead
            // of stretching (mirrors StartScreenButtons.MakeButton's handling).
            img.sprite = artSprite;
            img.color  = Color.white;

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

            // Some wood frames (generic ones) have no baked-in text —
            // overlay the dynamic label on top in that case.
            if (overlayLabel)
            {
                EndTxt(go, "Label", label, 18, FontStyles.Bold, ColBtnText,
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

            EndTxt(go, "Label", label, 18, FontStyles.Bold, ColBtnText,
                   new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f));
        }

        btn.colors        = colors;
        btn.targetGraphic = img;
        return btn;
    }
}

/// <summary>Shows a persistent loading cover and reliably activates the requested scene.</summary>
public sealed class SceneLoadOverlay : MonoBehaviour
{
    static bool loading;
    TMP_Text status;
    Image fill;

    public static void Load(string sceneName)
    {
        if (loading || string.IsNullOrWhiteSpace(sceneName)) return;

        loading = true;
        Time.timeScale = 1f;
        GameObject host = new GameObject("SceneLoadOverlay");
        DontDestroyOnLoad(host);
        SceneLoadOverlay overlay = host.AddComponent<SceneLoadOverlay>();
        overlay.Build();
        overlay.StartCoroutine(overlay.LoadRoutine(sceneName));
    }

    void Build()
    {
        GameObject canvasObject = new GameObject("Canvas");
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32700;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject shade = new GameObject("Shade");
        shade.layer = 5;
        shade.transform.SetParent(canvasObject.transform, false);
        RectTransform shadeRect = shade.AddComponent<RectTransform>();
        shadeRect.anchorMin = Vector2.zero;
        shadeRect.anchorMax = Vector2.one;
        shadeRect.offsetMin = shadeRect.offsetMax = Vector2.zero;
        shade.AddComponent<Image>().color = new Color(0.015f, 0.04f, 0.055f, 0.98f);

        GameObject title = new GameObject("Title");
        title.layer = 5;
        title.transform.SetParent(shade.transform, false);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.25f, 0.52f);
        titleRect.anchorMax = new Vector2(0.75f, 0.66f);
        titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
        status = title.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) status.font = TMP_Settings.defaultFontAsset;
        status.text = "LOADING NEXT LEVEL...";
        status.fontSize = 42f;
        status.fontStyle = FontStyles.Bold;
        status.alignment = TextAlignmentOptions.Center;
        status.color = Color.white;

        GameObject track = new GameObject("ProgressTrack");
        track.layer = 5;
        track.transform.SetParent(shade.transform, false);
        RectTransform trackRect = track.AddComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.31f, 0.43f);
        trackRect.anchorMax = new Vector2(0.69f, 0.48f);
        trackRect.offsetMin = trackRect.offsetMax = Vector2.zero;
        Image trackImage = track.AddComponent<Image>();
        AdventureUI.ApplyBarTrack(trackImage);

        GameObject fillObject = new GameObject("Fill");
        fillObject.layer = 5;
        fillObject.transform.SetParent(track.transform, false);
        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.015f, 0.14f);
        fillRect.anchorMax = new Vector2(0.985f, 0.86f);
        fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
        fill = fillObject.AddComponent<Image>();
        AdventureUI.ApplyBarFill(fill, AdventureUI.FillBlue);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SceneLoadOverlay] Scene '{sceneName}' is not available in Build Settings.");
            loading = false;
            Destroy(gameObject);
            yield break;
        }

        if (fill != null) fill.fillAmount = 0.15f;
        if (status != null) status.text = "PREPARING NEXT LEVEL...";
        // Draw the loading cover before the reliable synchronous activation.
        yield return null;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        if (fill != null) fill.fillAmount = 1f;
        yield return null;
        loading = false;
        Destroy(gameObject);
    }
}
