using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private Text deathReasonText;
    private string lastDeathReason = "You lost.";
    private Text victoryTitleText;
    private Text victoryMessageText;

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
                Time.timeScale = 0f;
                EnsureVictoryPanel();
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

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("LoseCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        deathPanel = new GameObject("LosePanel");
        deathPanel.transform.SetParent(canvas.transform, false);
        Image bg = deathPanel.AddComponent<Image>();
        bg.color = new Color(0.45f, 0.18f, 0.08f, 0.94f);
        RectTransform rt = deathPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject inner = new GameObject("Inner");
        inner.transform.SetParent(deathPanel.transform, false);
        Image innerImg = inner.AddComponent<Image>();
        innerImg.color = new Color(0.85f, 0.35f, 0.08f, 0.95f);
        RectTransform innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = new Vector2(0.18f, 0.28f);
        innerRt.anchorMax = new Vector2(0.82f, 0.72f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;

        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(inner.transform, false);
        Text title = titleGo.AddComponent<Text>();
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (title.font == null)
        {
            title.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        title.text = "YOU LOST";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(0.45f, 0.22f, 0.08f);
        title.fontSize = 42;
        title.fontStyle = FontStyle.Bold;
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.62f);
        titleRt.anchorMax = new Vector2(0.95f, 0.95f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;

        GameObject reasonGo = new GameObject("Reason");
        reasonGo.transform.SetParent(inner.transform, false);
        deathReasonText = reasonGo.AddComponent<Text>();
        deathReasonText.font = title.font;
        deathReasonText.text = lastDeathReason;
        deathReasonText.alignment = TextAnchor.MiddleCenter;
        deathReasonText.color = new Color(0.35f, 0.16f, 0.06f);
        deathReasonText.fontSize = 24;
        deathReasonText.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform reasonRt = reasonGo.GetComponent<RectTransform>();
        reasonRt.anchorMin = new Vector2(0.08f, 0.18f);
        reasonRt.anchorMax = new Vector2(0.92f, 0.62f);
        reasonRt.offsetMin = Vector2.zero;
        reasonRt.offsetMax = Vector2.zero;

        GameObject restartGo = new GameObject("Restart");
        restartGo.transform.SetParent(inner.transform, false);
        Image restartImg = restartGo.AddComponent<Image>();
        restartImg.color = new Color(0.55f, 0.22f, 0.08f);
        Button btn = restartGo.AddComponent<Button>();
        btn.onClick.AddListener(RestartRun);
        RectTransform btnRt = restartGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.3f, 0.04f);
        btnRt.anchorMax = new Vector2(0.7f, 0.16f);
        btnRt.offsetMin = Vector2.zero;
        btnRt.offsetMax = Vector2.zero;

        GameObject btnLabel = new GameObject("Label");
        btnLabel.transform.SetParent(restartGo.transform, false);
        Text btnText = btnLabel.AddComponent<Text>();
        btnText.font = title.font;
        btnText.text = "Restart";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontSize = 22;
        RectTransform btnLabelRt = btnLabel.GetComponent<RectTransform>();
        btnLabelRt.anchorMin = Vector2.zero;
        btnLabelRt.anchorMax = Vector2.one;
        btnLabelRt.offsetMin = Vector2.zero;
        btnLabelRt.offsetMax = Vector2.zero;

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

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("VictoryCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(canvas.transform, false);
        Image bg = victoryPanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.28f, 0.22f, 0.94f);
        RectTransform rt = victoryPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject inner = new GameObject("Inner");
        inner.transform.SetParent(victoryPanel.transform, false);
        Image innerImg = inner.AddComponent<Image>();
        innerImg.color = new Color(0.18f, 0.62f, 0.48f, 0.95f);
        RectTransform innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = new Vector2(0.16f, 0.22f);
        innerRt.anchorMax = new Vector2(0.84f, 0.78f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(inner.transform, false);
        victoryTitleText = titleGo.AddComponent<Text>();
        victoryTitleText.font = font;
        victoryTitleText.alignment = TextAnchor.MiddleCenter;
        victoryTitleText.color = new Color(0.06f, 0.22f, 0.16f);
        victoryTitleText.fontSize = 40;
        victoryTitleText.fontStyle = FontStyle.Bold;
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.68f);
        titleRt.anchorMax = new Vector2(0.95f, 0.96f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;

        GameObject messageGo = new GameObject("Message");
        messageGo.transform.SetParent(inner.transform, false);
        victoryMessageText = messageGo.AddComponent<Text>();
        victoryMessageText.font = font;
        victoryMessageText.alignment = TextAnchor.MiddleCenter;
        victoryMessageText.color = new Color(0.05f, 0.18f, 0.14f);
        victoryMessageText.fontSize = 22;
        victoryMessageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform messageRt = messageGo.GetComponent<RectTransform>();
        messageRt.anchorMin = new Vector2(0.08f, 0.22f);
        messageRt.anchorMax = new Vector2(0.92f, 0.68f);
        messageRt.offsetMin = Vector2.zero;
        messageRt.offsetMax = Vector2.zero;

        GameObject nextGo = new GameObject("NextLevel");
        nextGo.transform.SetParent(inner.transform, false);
        Image nextImg = nextGo.AddComponent<Image>();
        nextImg.color = new Color(0.08f, 0.38f, 0.28f);
        Button nextBtn = nextGo.AddComponent<Button>();
        nextBtn.onClick.AddListener(GoToNextScene);
        RectTransform nextRt = nextGo.GetComponent<RectTransform>();
        nextRt.anchorMin = new Vector2(0.28f, 0.06f);
        nextRt.anchorMax = new Vector2(0.72f, 0.18f);
        nextRt.offsetMin = Vector2.zero;
        nextRt.offsetMax = Vector2.zero;

        GameObject nextLabel = new GameObject("Label");
        nextLabel.transform.SetParent(nextGo.transform, false);
        Text nextText = nextLabel.AddComponent<Text>();
        nextText.font = font;
        nextText.text = nextScene == SceneCatalog.Level3 ? "Start Level 3"
            : nextScene == SceneCatalog.Level2 ? "Start Level 2"
            : string.IsNullOrEmpty(nextScene) ? "Continue"
            : "Next Level";
        nextText.alignment = TextAnchor.MiddleCenter;
        nextText.color = Color.white;
        nextText.fontSize = 22;
        RectTransform nextLabelRt = nextLabel.GetComponent<RectTransform>();
        nextLabelRt.anchorMin = Vector2.zero;
        nextLabelRt.anchorMax = Vector2.one;
        nextLabelRt.offsetMin = Vector2.zero;
        nextLabelRt.offsetMax = Vector2.zero;

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
}
