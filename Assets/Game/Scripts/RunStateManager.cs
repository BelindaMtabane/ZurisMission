using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RunStateManager : MonoBehaviour
{
    public static RunStateManager Instance { get; private set; }

    public enum RunState
    {
        Playing,
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

    // ── Shared end-screen palette (matches StartScreenButtons) ─────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    static readonly Color ColScrim     = C(0.02f, 0.02f, 0.02f, 0.72f);
    static readonly Color ColGoldTitle = C(0.55f, 0.32f, 0.04f);
    static readonly Color ColCream     = C(0.32f, 0.20f, 0.10f);
    static readonly Color ColLoseTitle = C(0.80f, 0.04f, 0.04f);   // red — "YOU LOST"
    static readonly Color ColLoseText  = C(0.02f, 0.02f, 0.02f);   // black — reason text
    static readonly Color ColBtnRetry  = C(0.55f, 0.10f, 0.10f, 0.92f);
    static readonly Color ColBtnNext   = C(0.10f, 0.55f, 0.10f, 0.92f);
    static readonly Color ColBtnText   = Color.white;

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

    static readonly Dictionary<string, Sprite> uiKitCache = new();
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

    static Sprite RestartButtonSprite() => UiKitSprite("UIKit_12");
    static Sprite WoodFrameSprite()     => UiKitSprite("UIKit_10");

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = SceneCatalog.StartScreen;
    [SerializeField] private string nextScene = "";

    public RunState CurrentState { get; private set; } = RunState.Playing;
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
        SetState(RunState.Playing);
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
        if (CurrentState != RunState.Playing) return;
        lastDeathReason = string.IsNullOrEmpty(reason) ? "You lost." : reason;
        EnsureLosePanel();
        SetState(RunState.Dead);
    }

    public void NotifyVictory(string title = null, string message = null)
    {
        if (CurrentState != RunState.Playing) return;
        EnsureVictoryPanel(title, message);
        SetState(RunState.Victory);
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
        if (string.IsNullOrEmpty(nextScene)) return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextScene);
    }

    void SetState(RunState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case RunState.Playing:
                Time.timeScale = 1f;
                SetPanels(false, false, false);
                OnRunStarted?.Invoke();
                break;

            case RunState.Paused:
                Time.timeScale = 0f;
                SetPanels(false, false, true);
                break;

            case RunState.Dead:
                Time.timeScale = 0f;
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
                SetPanels(false, true, false);
                OnRunVictory?.Invoke();
                break;
        }
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
        innerRt.anchorMin = new Vector2(0.30f, 0.22f);
        innerRt.anchorMax = new Vector2(0.70f, 0.78f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;

        EndTxt(inner, "Title", "YOU LOST", 44, FontStyles.Bold, ColLoseTitle,
               new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.90f));

        deathReasonText = EndTxt(inner, "Reason", lastDeathReason, 28, FontStyles.Bold, ColLoseText,
               new Vector2(0.06f, 0.40f), new Vector2(0.94f, 0.68f));

        var btn = EndButton(inner, "Restart", "RESTART", ColBtnRetry,
                             new Vector2(0.22f, 0.10f), new Vector2(0.78f, 0.28f), RestartButtonSprite());
        btn.onClick.AddListener(RestartRun);

        deathPanel.SetActive(false);
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
            : string.IsNullOrEmpty(nextScene) ? "CONTINUE"
            : "NEXT LEVEL";
        var nextBtn = EndButton(inner, "NextLevel", nextLabel, ColBtnNext,
                                 new Vector2(0.15f, 0.10f), new Vector2(0.85f, 0.28f),
                                 WoodFrameSprite(), overlayLabel: true);
        nextBtn.onClick.AddListener(GoToNextScene);

        ApplyVictoryCopy(title, message);
        victoryPanel.SetActive(false);
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
        Sprite s = Panel1();
        if (s != null)
        {
            img.sprite = s;
            img.type   = Image.Type.Sliced;
            img.color  = Color.white;
        }
        else
        {
            img.color = new Color(0.30f, 0.20f, 0.12f, 0.96f);
        }
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
