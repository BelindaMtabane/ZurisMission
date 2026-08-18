using UnityEngine;
using UnityEngine.SceneManagement;

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
    }

    void Update()
    {
        if (triggered) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (SceneManager.GetActiveScene().name != "Level3") return;

        Remaining = Mathf.Max(0f, endTime - Time.time);
        Urgency = 1f - (Remaining / Level3Config.Level3TimeLimitSeconds);

        FireWarnings();

        if (Remaining <= 0f)
        {
            triggered = true;
            // Count how many tanks are incomplete so the reason message is specific.
            int filled = 0;
            if (Level3PipeRepair.Instance != null)
            {
                for (int i = 0; i < 3; i++)
                    if (Level3PipeRepair.Instance.GetProgress(i) >= 100) filled++;
            }
            string reason = filled == 0
                ? "Time's up! None of the three tanks were repaired in time."
                : $"Time's up! Only {filled} of 3 tanks were fully repaired.";
            RunStateManager.Instance?.NotifyDeath(reason);
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

}
