using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the 3-minute countdown for Level 3.
/// Exposes Urgency (0=start, 1=expired) so other systems can scale difficulty.
/// </summary>
public class Level3TimeLimit : MonoBehaviour
{
    public static Level3TimeLimit Instance { get; private set; }

    public float Urgency { get; private set; }      // 0..1
    public float Remaining { get; private set; }    // seconds left

    float endTime;
    bool triggered;

    // Track which second-based warnings have fired
    bool warn120, warn90, warn60, warn30, warn20, warn10;

    // Optional on-screen timer text created at runtime
    TMP_Text timerText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Level3") return;
        endTime = Time.time + Level3Config.Level3TimeLimitSeconds;
        Remaining = Level3Config.Level3TimeLimitSeconds;
        BuildTimerUI();
    }

    void Update()
    {
        if (triggered) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (SceneManager.GetActiveScene().name != "Level3") return;

        Remaining = Mathf.Max(0f, endTime - Time.time);
        Urgency = 1f - (Remaining / Level3Config.Level3TimeLimitSeconds);

        UpdateTimerUI();
        FireWarnings();

        if (Remaining <= 0f)
        {
            triggered = true;
            Level3FeedbackUI.Show("TIME'S UP! You needed to repair all tanks!", new Color(1f, 0.2f, 0.1f), 3f);
            RunStateManager.Instance?.NotifyDeath("Time's up! Repair the tanks before 3 minutes.");
        }
    }

    void FireWarnings()
    {
        if (!warn120 && Remaining <= 120f)
        {
            warn120 = true;
            Level3FeedbackUI.Show("2 MINUTES LEFT — HURRY!", new Color(1f, 0.8f, 0.1f), 2f);
        }
        else if (!warn90 && Remaining <= 90f)
        {
            warn90 = true;
            Level3FeedbackUI.Show("90 SECONDS! FIX THOSE PIPES!", new Color(1f, 0.65f, 0.05f), 2f);
        }
        else if (!warn60 && Remaining <= 60f)
        {
            warn60 = true;
            Level3FeedbackUI.Show("1 MINUTE LEFT — FINAL PUSH!", new Color(1f, 0.35f, 0.05f), 2.5f);
        }
        else if (!warn30 && Remaining <= 30f)
        {
            warn30 = true;
            Level3FeedbackUI.Show("30 SECONDS! ALL TANKS MUST BE FULL!", new Color(1f, 0.15f, 0.05f), 2.5f);
        }
        else if (!warn20 && Remaining <= 20f)
        {
            warn20 = true;
            Level3FeedbackUI.Show("20 SECONDS!!!", new Color(1f, 0.05f, 0.05f), 1.5f);
        }
        else if (!warn10 && Remaining <= 10f)
        {
            warn10 = true;
            Level3FeedbackUI.Show("10 SECONDS! GO GO GO!", new Color(1f, 0.05f, 0.05f), 1.2f);
        }
    }

    // ─── UI timer in top-left ────────────────────────────────────────────────

    void BuildTimerUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject go = new GameObject("Level3Timer");
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(18f, -18f);
        rt.sizeDelta = new Vector2(200f, 44f);

        timerText = go.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) timerText.font = TMP_Settings.defaultFontAsset;
        timerText.fontSize = 26f;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Left;
        timerText.raycastTarget = false;
        timerText.text = "3:00";
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int mins = Mathf.FloorToInt(Remaining / 60f);
        int secs = Mathf.CeilToInt(Remaining % 60f);
        if (secs == 60) { mins++; secs = 0; }
        timerText.text = $"{mins}:{secs:D2}";

        // Colour shifts warm→red as time shrinks
        if (Remaining > 60f)
            timerText.color = Color.Lerp(Color.white, new Color(1f, 0.8f, 0.1f), Urgency * 1.3f);
        else if (Remaining > 20f)
            timerText.color = new Color(1f, 0.55f, 0.05f);
        else
            timerText.color = new Color(1f, 0.1f, 0.05f);
    }
}
