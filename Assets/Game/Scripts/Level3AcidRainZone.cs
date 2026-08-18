using UnityEngine;

/// <summary>
/// Upright acid-rain column (lane-specific). Warning → countdown → strike → despawn
/// only once the player has passed or collided with it.
/// </summary>
public class Level3AcidRainZone : MonoBehaviour
{
    const float TriggerDistanceFallback = 70f;

    // The rain stays tall from sky to ground; what expands is the falling range.
    const float PulseMinRadius = 10f;
    const float PulseMaxRadius = 20f;
    const float PulseSpeed  = 1.8f;   // slower cycle so the full expansion is visible

    // Despawn only after the player has moved this far past the column
    const float DespawnBehind = 6f;

    enum Phase { Idle, Warning, Countdown, Strike, WaitPass }

    Phase phase = Phase.Idle;
    float timer;
    int countdown;
    bool damaged;
    bool playerCollided;

    [SerializeField] int laneIndex;
    [SerializeField] GameObject warningRoot;
    [SerializeField] GameObject activeRoot;
    BoxCollider strikeCollider;

    // Store base scales for each child so we only pulse the radius.
    Vector3[] activeChildBaseScales;

    Transform player;

    public void Setup(int lane, GameObject warning, GameObject active)
    {
        laneIndex   = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        warningRoot = warning;
        activeRoot  = active;
        damaged     = false;
        playerCollided = false;
        phase       = Phase.Idle;

        if (warningRoot != null) warningRoot.SetActive(false);
        if (activeRoot  != null) activeRoot.SetActive(false);
        strikeCollider = GetComponent<BoxCollider>();

        // Cache each child's base local scale so pulsing is relative
        if (activeRoot != null)
        {
            int count = activeRoot.transform.childCount;
            activeChildBaseScales = new Vector3[count];
            for (int i = 0; i < count; i++)
                activeChildBaseScales[i] = activeRoot.transform.GetChild(i).localScale;
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
                    playerCollided = false;
                    if (warningRoot != null) warningRoot.SetActive(true);
                    if (activeRoot  != null) activeRoot.SetActive(false);

                    // Tutorial window: show a more general "about to pour" hint for the first ~30s.
                    if (Time.time < 30f)
                    {
                        Level3FeedbackUI.Show(
                            "ACID RAIN ABOUT TO POUR! WATCH THE COLUMN!",
                            new Color(0.45f, 0.95f, 0.28f),
                            2f);
                    }
                    else
                    {
                        Level3FeedbackUI.Show(
                            $"ACID RAIN IN LANE {LevelLanes.DisplayNumber(laneIndex)} — {Level3Config.AcidWarningSeconds:0} SECONDS!",
                            new Color(0.45f, 0.95f, 0.28f),
                            Level3Config.AcidWarningSeconds + 1f);
                    }
                }
                break;

            case Phase.Warning:
                timer -= Time.deltaTime;
                PulseWarning();
                if (timer <= 0f)
                {
                    phase     = Phase.Countdown;
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
                        timer = 5f;   // column stays visible and pulsing for 5 seconds
                        if (warningRoot != null) warningRoot.SetActive(false);
                        if (activeRoot  != null) activeRoot.SetActive(true);
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
                // Move to WaitPass once timer is up or player already collided
                if (timer <= 0f || playerCollided)
                {
                    phase = Phase.WaitPass;
                    if (activeRoot != null) activeRoot.SetActive(false);
                    if (warningRoot != null) warningRoot.SetActive(false);
                }
                break;

            case Phase.WaitPass:
                // Destroy only when the player has moved past this object
                if (player.position.z > transform.position.z + DespawnBehind)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    void TryDamagePlayerNow()
    {
        if (damaged || player == null) return;

        // Only damage immediately if the player is actually overlapping the column in Z.
        // The BoxCollider depth is roughly ~2 world units, so we use a small tolerance.
        if (Mathf.Abs(player.position.z - transform.position.z) > 2.2f) return;

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-Level3Config.AcidHealthDamage, "Acid rain burned you!");
        Level3FeedbackUI.Show("ACID RAIN — -10 HEALTH!", new Color(0.3f, 0.85f, 0.15f), 0.6f);
        damaged = true;
        playerCollided = true;
    }

    void ShowCountdown()
    {
        timer = 1f;
        Level3FeedbackUI.Show(countdown.ToString(), new Color(0.45f, 0.95f, 0.18f), 0.95f);
    }

    void OnTriggerEnter(Collider other)  => HandleContact(other);
    void OnTriggerStay(Collider other)   => HandleContact(other);

    void HandleContact(Collider other)
    {
        if (phase != Phase.Strike) return;
        if (damaged) return;
        if (!IsPlayer(other)) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (Level3LeafProtection.TryBlockAcid()) return;

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-Level3Config.AcidHealthDamage, "Acid rain burned you!");
        Level3FeedbackUI.Show("ACID RAIN — -10 HEALTH!", new Color(0.3f, 0.85f, 0.15f), 0.6f);
        damaged = true;
        playerCollided = true;
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
            if (d < bestDist) { bestDist = d; best = i; }
        }
        return best;
    }

    void PulseWarning()
    {
        if (warningRoot == null) return;
        float t = (Mathf.Sin(Time.time * PulseSpeed) + 1f) * 0.5f;
        float radiusScale = Mathf.Lerp(PulseMinRadius, PulseMaxRadius, t);
        warningRoot.transform.localScale = new Vector3(radiusScale, 1f, radiusScale);
    }

    void PulseActive()
    {
        if (activeRoot == null || activeChildBaseScales == null) return;
        float t = (Mathf.Sin(Time.time * PulseSpeed) + 1f) * 0.5f;   // 0..1
        float radiusScale = Mathf.Lerp(PulseMinRadius, PulseMaxRadius, t);

        for (int i = 0; i < activeRoot.transform.childCount && i < activeChildBaseScales.Length; i++)
        {
            Transform child = activeRoot.transform.GetChild(i);
            Vector3 b = activeChildBaseScales[i];
            child.localScale = new Vector3(b.x * radiusScale, b.y, b.z * radiusScale);
        }

        if (strikeCollider != null)
        {
            strikeCollider.center = new Vector3(0f, 20f, 0f);
            strikeCollider.size = new Vector3(2.64f * radiusScale, 40f, 2.64f * radiusScale);
        }
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
