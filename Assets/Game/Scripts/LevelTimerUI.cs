using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Countdown timer for MainGame (Level 1) and Level2 — wood badge in the
/// top-right corner, directly above the LevelHUDStrip status panel.
/// Reaching 0 triggers a game-over via RunStateManager, reusing the same
/// wood-styled Lose panel as running out of water or health.
/// </summary>
public class LevelTimerUI : MonoBehaviour
{
    public static LevelTimerUI Instance { get; private set; }

    [SerializeField] float durationSeconds = 90f;   // 1:30

    float   _remaining;
    bool    _expired;
    TMP_Text _timeText;
    Canvas   _canvas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "MainGame" && sceneName != "Level2") return;
        if (FindFirstObjectByType<LevelTimerUI>() != null) return;
        new GameObject("LevelTimerUI").AddComponent<LevelTimerUI>();
    }

    public void SetVisible(bool v) { if (_canvas) _canvas.gameObject.SetActive(v); }

    // ── Palette ──────────────────────────────────────────────────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    static readonly Color ColLabel  = C(0.30f, 0.19f, 0.09f);
    static readonly Color ColTime   = C(0.20f, 0.35f, 0.10f);
    static readonly Color ColUrgent = C(0.75f, 0.06f, 0.06f);

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

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance   = this;
        _remaining = durationSeconds;
        BuildUI();
    }

    void Update()
    {
        if (_expired) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        _remaining -= Time.deltaTime;
        if (_remaining <= 0f)
        {
            _remaining = 0f;
            _expired   = true;
            RunStateManager.Instance?.NotifyDeath("You ran out of time!");
        }

        int totalSeconds = Mathf.CeilToInt(_remaining);
        int mins = totalSeconds / 60;
        int secs = totalSeconds % 60;
        _timeText.text  = $"{mins}:{secs:00}";
        _timeText.color = _remaining <= 10f ? ColUrgent : ColTime;
    }

    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        var cvGO = new GameObject("LvlTimer_Canvas");
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

        var badge = new GameObject("TimerBadge");
        badge.transform.SetParent(cvGO.transform, false);
        var img = badge.AddComponent<Image>();
        Sprite p1 = Panel1();
        if (p1 != null) { img.sprite = p1; img.type = Image.Type.Sliced; img.color = Color.white; }
        else img.color = new Color(0.30f, 0.20f, 0.12f, 0.96f);
        var rt = badge.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.700f, 0.875f);
        rt.anchorMax = new Vector2(0.985f, 0.985f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Txt(badge, "Lbl", "TIME LEFT", 18, FontStyles.Bold, ColLabel,
            new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.90f));

        _timeText = Txt(badge, "Time", "1:30", 40, FontStyles.Bold, ColTime,
            new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.58f));
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
