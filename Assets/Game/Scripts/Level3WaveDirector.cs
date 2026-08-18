using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the alternating snake / warthog wave pattern for Level 3.
///
/// Wave schedule (by level progress 0..1):
///   0.20 – 0.50  Snake wave
///   0.50 – 0.75  Warthog wave
///   0.75 – 0.90  Snake wave
///   0.90 – 0.95  Combined  (snakes + warthogs)
///   0.95 – 1.00  CombinedHard  (snakes + warthogs, harder difficulty)
/// </summary>
public class Level3WaveDirector : MonoBehaviour
{
    public static Level3WaveDirector Instance { get; private set; }

    // Readable wave type used by the layout director to decide what to spawn
    public enum WaveMode { None, Snakes, Warthogs, Combined, CombinedHard }

    public WaveMode CurrentMode { get; private set; } = WaveMode.None;

    // Progress thresholds that define each wave window
    struct WaveWindow
    {
        public float start;
        public float end;
        public WaveMode mode;
    }

    static readonly WaveWindow[] Windows =
    {
        new WaveWindow { start = 0.20f, end = 0.50f, mode = WaveMode.Snakes        },
        new WaveWindow { start = 0.50f, end = 0.75f, mode = WaveMode.Warthogs      },
        new WaveWindow { start = 0.75f, end = 0.90f, mode = WaveMode.Snakes        },
        new WaveWindow { start = 0.90f, end = 0.95f, mode = WaveMode.Combined      },
        new WaveWindow { start = 0.95f, end = 1.00f, mode = WaveMode.CombinedHard  },
    };

    Transform player;
    WaveMode lastAnnouncedMode = WaveMode.None;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        float p = Level3Progress.Normalized(player.position.z);
        WaveMode mode = WaveMode.None;

        for (int i = 0; i < Windows.Length; i++)
        {
            if (p >= Windows[i].start && p < Windows[i].end)
            {
                mode = Windows[i].mode;
                break;
            }
        }

        if (mode != CurrentMode)
        {
            CurrentMode = mode;
            AnnounceTransition(mode);
        }
    }

    void AnnounceTransition(WaveMode mode)
    {
        if (mode == lastAnnouncedMode) return;
        lastAnnouncedMode = mode;

        switch (mode)
        {
            case WaveMode.Snakes:
                // One holistic popup for the whole animal challenge window.
                Level3FeedbackUI.Show(
                    "SNAKES INCOMING! JUMP OR DODGE THE LANES!",
                    new Color(0.45f, 0.95f, 0.4f),
                    2f);
                break;
            case WaveMode.Warthogs:
                // One holistic popup for the whole animal challenge window.
                Level3FeedbackUI.Show(
                    "WARTHOGS CHARGING! JUMP OR DODGE THE LANES!",
                    new Color(1f, 0.55f, 0.15f),
                    2f);
                break;
            case WaveMode.Combined:
                Level3FeedbackUI.Show(
                    "FINAL PUSH! SNAKES + WARTHOGS — STAY ALERT!",
                    new Color(1f, 0.25f, 0.15f),
                    2.5f);
                break;
            case WaveMode.CombinedHard:
                Level3FeedbackUI.Show("SURVIVAL SPRINT — MAXIMUM DANGER!", new Color(1f, 0.1f, 0.05f), 3f);
                break;
            case WaveMode.None:
                break;
        }
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
