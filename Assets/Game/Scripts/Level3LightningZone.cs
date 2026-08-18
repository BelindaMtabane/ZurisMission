using UnityEngine;

public class Level3LightningZone : MonoBehaviour
{
    const float TriggerDistanceFallback = 70f;
    const float TutorialWarningSeconds = 30f;

    bool InWarningRange(Transform currentPlayer)
    {
        float ahead = Mathf.Max(Level3Config.VisibleSpawnDistance, TriggerDistanceFallback);
        return currentPlayer.position.z > transform.position.z - ahead;
    }

    [SerializeField] int laneIndex;
    [SerializeField] GameObject warningRoot;
    [SerializeField] GameObject boltRoot;

    enum Phase { Idle, Warning, Countdown, Strike, Clear }
    Phase phase = Phase.Idle;
    float timer;
    int countdown;
    bool damaged;
    Transform player;

    bool ShouldShowCountdown()
    {
        // First ~30 seconds: show the normal countdown so the player learns.
        // After that: show only one warning (no per-second countdown UI).
        return Time.time < TutorialWarningSeconds;
    }

    public void Setup(int lane, GameObject warning, GameObject bolt)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        warningRoot = warning;
        boltRoot = bolt;
        if (boltRoot != null) boltRoot.SetActive(false);
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
                    bool showCountdown = ShouldShowCountdown();
                    timer = showCountdown ? 0.6f : (0.6f + Level3Config.LightningWarningSeconds);

                    if (showCountdown)
                    {
                        Level3FeedbackUI.Show(
                            $"LIGHTNING STRIKE IN LANE {LevelLanes.DisplayNumber(laneIndex)} — {Level3Config.LightningWarningSeconds:0} SECONDS!",
                            new Color(1f, 0.92f, 0.25f),
                            Level3Config.LightningWarningSeconds + 1f);
                    }
                    else
                    {
                        // After tutorial time: only one warning, no countdown numbers.
                        Level3FeedbackUI.Show(
                            "LIGHTNING WILL STRIKE! STAY ALERT!",
                            new Color(1f, 0.92f, 0.25f),
                            2f);
                    }
                }
                break;
            case Phase.Warning:
                timer -= Time.deltaTime;
                PulseWarning();
                if (timer <= 0f)
                {
                    bool showCountdown = ShouldShowCountdown();
                    if (showCountdown)
                    {
                        phase = Phase.Countdown;
                        countdown = Mathf.CeilToInt(Level3Config.LightningWarningSeconds);
                        ShowCountdown();
                    }
                    else
                    {
                        phase = Phase.Strike;
                        timer = 3f;   // bolt stays visible for 3 seconds
                        if (warningRoot != null) warningRoot.SetActive(false);
                        if (boltRoot != null) boltRoot.SetActive(true);
                        Level3FeedbackUI.Show("STRIKE!", new Color(1f, 1f, 0.5f), 0.8f);
                        TryDamagePlayer();
                    }
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
                        timer = 3f;   // bolt stays visible for 3 seconds
                        if (warningRoot != null) warningRoot.SetActive(false);
                        if (boltRoot != null) boltRoot.SetActive(true);
                        Level3FeedbackUI.Show("STRIKE!", new Color(1f, 1f, 0.5f), 0.8f);
                        TryDamagePlayer();
                    }
                    else
                    {
                        ShowCountdown();
                    }
                }
                break;
            case Phase.Strike:
                timer -= Time.deltaTime;
                TryDamagePlayer();
                if (timer <= 0f)
                {
                    phase = Phase.Clear;
                    if (boltRoot != null) boltRoot.SetActive(false);
                }
                break;
        }
    }

    void ShowCountdown()
    {
        timer = 1f;
        Level3FeedbackUI.Show(countdown.ToString(), new Color(1f, 0.85f, 0.2f), 0.95f);
    }

    void TryDamagePlayer()
    {
        if (damaged || player == null) return;

        // Ensure we only damage when the player is actually at the bolt position.
        if (Mathf.Abs(player.position.z - transform.position.z) > 2.0f) return;

        int playerLane = ClosestLane(player.position.x);
        if (playerLane != laneIndex) return;

        PlayerController controller = player.GetComponent<PlayerController>();
        // No "jumped clear" immunity: contact always damages.

        damaged = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-Level3Config.LightningHealthDamage, "Lightning struck you!");
        Level3FeedbackUI.Show("LIGHTNING STRUCK!", new Color(1f, 0.95f, 0.35f), 1.1f);
    }

    void OnTriggerEnter(Collider other) => TryDamagePlayer(other);
    void OnTriggerStay(Collider other) => TryDamagePlayer(other);

    void TryDamagePlayer(Collider other)
    {
        if (damaged || phase != Phase.Strike || other == null) return;
        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerController>() == null) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        Transform target = other.GetComponentInParent<PlayerController>() != null
            ? other.GetComponentInParent<PlayerController>().transform
            : other.transform;
        int playerLane = ClosestLane(target.position.x);
        if (playerLane != laneIndex) return;

        damaged = true;
        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-Level3Config.LightningHealthDamage, "Lightning struck you!");
        Level3FeedbackUI.Show("LIGHTNING STRUCK!", new Color(1f, 0.95f, 0.35f), 1.1f);
    }

    static int ClosestLane(float x)
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
        warningRoot.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 9f) * 0.2f);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
