using UnityEngine;

/// <summary>
/// Horizontal acid bar (lane-specific) with a warning + countdown before it "strikes".
/// </summary>
public class Level3AcidRainZone : MonoBehaviour
{
    const float TriggerDistanceFallback = 70f;
    // Pulse the active root between this scale range while striking
    const float PulseMin = 0.7f;
    const float PulseMax = 1.4f;
    const float PulseSpeed = 5f;

    enum Phase { Idle, Warning, Countdown, Strike, Clear }

    Phase phase = Phase.Idle;
    float timer;
    int countdown;
    bool damaged;

    [SerializeField] int laneIndex;
    [SerializeField] GameObject warningRoot;
    [SerializeField] GameObject activeRoot;
    Vector3 activeBaseScale;

    Transform player;

    public void Setup(int lane, GameObject warning, GameObject active)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        warningRoot = warning;
        activeRoot = active;

        damaged = false;
        phase = Phase.Idle;

        if (warningRoot != null) warningRoot.SetActive(false);
        if (activeRoot != null)
        {
            activeRoot.SetActive(false);
            activeBaseScale = activeRoot.transform.localScale == Vector3.zero
                ? Vector3.one
                : activeRoot.transform.localScale;
        }
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        CachePlayer();
        if (player == null) return;

        switch (phase)
        {
            case Phase.Idle:
                if (InWarningRange(player))
                {
                    phase = Phase.Warning;
                    timer = 0.6f;
                    damaged = false;
                    if (warningRoot != null) warningRoot.SetActive(true);
                    if (activeRoot != null) activeRoot.SetActive(false);

                    Level3FeedbackUI.Show(
                        $"ACID RAIN IN LANE {LevelLanes.DisplayNumber(laneIndex)} — {Level3Config.AcidWarningSeconds:0} SECONDS!",
                        new Color(0.45f, 0.95f, 0.28f),
                        Level3Config.AcidWarningSeconds + 1f);
                }
                break;

            case Phase.Warning:
                timer -= Time.deltaTime;
                PulseWarning();
                if (timer <= 0f)
                {
                    phase = Phase.Countdown;
                    countdown = Mathf.CeilToInt(Level3Config.AcidWarningSeconds);
                    ShowCountdown();
                }
                break;

            case Phase.Countdown:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    countdown--;
                    if (countdown <= 0)
                    {
                        phase = Phase.Strike;
                        timer = 0.35f;
                        if (warningRoot != null) warningRoot.SetActive(false);
                        if (activeRoot != null) activeRoot.SetActive(true);

                        TryDamagePlayerNow();

                        Level3FeedbackUI.Show("ACID RAIN!", new Color(0.4f, 0.9f, 0.2f), 0.8f);
                    }
                    else
                    {
                        ShowCountdown();
                    }
                }
                break;

            case Phase.Strike:
                timer -= Time.deltaTime;
                PulseActive();
                if (timer <= 0f)
                {
                    phase = Phase.Clear;
                    if (activeRoot != null)
                    {
                        activeRoot.transform.localScale = activeBaseScale;
                        activeRoot.SetActive(false);
                    }
                }
                break;
        }
    }

    void TryDamagePlayerNow()
    {
        if (damaged || player == null) return;

        int playerLane = ClosestLane(player.position.x);
        if (playerLane != laneIndex) return;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null && !controller.IsGrounded)
        {
            Level3FeedbackUI.Show("JUMPED CLEAR!", new Color(0.4f, 0.95f, 0.45f), 0.8f);
            damaged = true;
            return;
        }

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-Level3Config.AcidHealthDamage, "Acid rain burned you!");
        hud?.LoseMaterialPercent(Level3Config.AcidMaterialLossPercent);
        Level3FeedbackUI.Show("ACID RAIN STRIKE!", new Color(0.3f, 0.85f, 0.15f), 0.6f);
        damaged = true;
    }

    void ShowCountdown()
    {
        timer = 1f;
        Level3FeedbackUI.Show(
            countdown.ToString(),
            new Color(0.45f, 0.95f, 0.18f),
            0.95f);
    }

    void OnTriggerStay(Collider other)
    {
        if (phase != Phase.Strike) return;
        if (damaged) return;
        if (!IsPlayer(other)) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (Level3LeafProtection.TryBlockAcid()) return;

        int otherLane = ClosestLane(other.transform.position.x);
        if (otherLane != laneIndex) return;

        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null && !controller.IsGrounded)
        {
            Level3FeedbackUI.Show("JUMPED CLEAR!", new Color(0.4f, 0.95f, 0.45f), 0.8f);
            damaged = true;
            return;
        }

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-Level3Config.AcidHealthDamage, "Acid rain burned you!");
        hud?.LoseMaterialPercent(Level3Config.AcidMaterialLossPercent);
        Level3FeedbackUI.Show("ACID RAIN STRIKE!", new Color(0.3f, 0.85f, 0.15f), 0.6f);
        damaged = true;
    }

    bool InWarningRange(Transform currentPlayer)
    {
        float ahead = Mathf.Max(Level3Config.VisibleSpawnDistance, TriggerDistanceFallback);
        return currentPlayer.position.z > transform.position.z - ahead;
    }

    int ClosestLane(float x)
    {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < LevelLanes.Count; i++)
        {
            float d = Mathf.Abs(x - LevelLanes.X(i));
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    void PulseWarning()
    {
        if (warningRoot == null) return;
        warningRoot.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 9f) * 0.22f);
    }

    void PulseActive()
    {
        if (activeRoot == null) return;
        // Breathe between PulseMin and PulseMax using a sine wave
        float t = (Mathf.Sin(Time.time * PulseSpeed) + 1f) * 0.5f; // 0..1
        float s = Mathf.Lerp(PulseMin, PulseMax, t);
        activeRoot.transform.localScale = activeBaseScale * s;
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }

    static bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        return other.GetComponentInParent<PlayerController>() != null;
    }
}
