using UnityEngine;

/// <summary>
/// Poison plant: warning, then a visible green gas sphere that grows as the player approaches.
/// </summary>
public class Level2PoisonPlant : MonoBehaviour
{
    const float ApproachStartDistance = 58f;
    const float TriggerDistance = 42f;
    const float ApproachScaleBoost = 1.2f;

    [SerializeField] private float spawnProgress;
    [SerializeField] private GameObject warningRoot;
    [SerializeField] private GameObject gasSphere;
    [SerializeField] private float damagePerTick = 4f;
    [SerializeField] private float waterDrainPerTick = 4f;

    enum Phase { Idle, Warning, GasActive, Clear, Cooldown }

    Phase phase = Phase.Idle;
    float warningTimer = 1.6f;
    float gasTimer = 3.2f;
    float cooldownTimer = 2.2f;
    float tick;
    Vector3 gasBaseScale = Vector3.one;
    Vector3 gasMaxScale = Vector3.one;
    Vector3 warningBaseScale = Vector3.one;
    SphereCollider gasCollider;
    Transform player;

    public void Setup(float progress, GameObject warning, GameObject gas)
    {
        spawnProgress = progress;
        warningRoot = warning;
        gasSphere = gas;
        gasCollider = GetComponent<SphereCollider>();

        if (spawnProgress <= 0.20f)
        {
            warningTimer = 2.2f;
            damagePerTick = 3f;
            waterDrainPerTick = 4f;
        }
        else if (spawnProgress <= 0.65f)
        {
            warningTimer = 1.5f;
            damagePerTick = 5f;
            waterDrainPerTick = 6f;
        }
        else
        {
            warningTimer = 1.1f;
            damagePerTick = 7f;
            waterDrainPerTick = 8f;
        }

        if (warningRoot != null)
        {
            warningBaseScale = warningRoot.transform.localScale;
            warningRoot.SetActive(false);
        }

        if (gasSphere != null)
        {
            gasBaseScale = gasSphere.transform.localScale;
            gasMaxScale = gasBaseScale * ApproachScaleBoost;
            gasSphere.transform.localScale = gasBaseScale;
            gasSphere.SetActive(false);
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
                UpdateApproachGasScale();
                if (player.position.z > transform.position.z - TriggerDistance)
                {
                    phase = Phase.Warning;
                    if (warningRoot != null) warningRoot.SetActive(true);
                    if (gasSphere != null) gasSphere.SetActive(true);
                }
                break;

            case Phase.Warning:
                UpdateApproachGasScale();
                Pulse(warningRoot, warningBaseScale, 8f, 0.16f);
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f)
                {
                    if (warningRoot != null) warningRoot.SetActive(false);
                    if (gasSphere != null) gasSphere.SetActive(true);
                    ApplyGasScale(gasMaxScale);
                    phase = Phase.GasActive;
                }
                break;

            case Phase.GasActive:
                ApplyGasScale(gasMaxScale);
                Pulse(gasSphere, gasMaxScale, 2.4f, 0.08f);
                gasTimer -= Time.deltaTime;
                if (gasTimer <= 0f)
                {
                    if (gasSphere != null) gasSphere.SetActive(false);
                    phase = Phase.Clear;
                    cooldownTimer = 1.4f;
                }
                break;

            case Phase.Clear:
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    phase = Phase.Cooldown;
                }
                break;

            case Phase.Cooldown:
                break;
        }
    }

    void UpdateApproachGasScale()
    {
        if (gasSphere == null || player == null) return;

        float distanceAhead = transform.position.z - player.position.z;
        if (distanceAhead <= 0f || distanceAhead > ApproachStartDistance)
        {
            if (phase == Phase.Idle)
            {
                gasSphere.SetActive(false);
                gasSphere.transform.localScale = gasBaseScale;
                ApplyGasScale(gasBaseScale);
            }
            return;
        }

        if (phase == Phase.Idle || phase == Phase.Warning)
        {
            gasSphere.SetActive(true);
        }

        float t = 1f - Mathf.Clamp01(distanceAhead / ApproachStartDistance);
        Vector3 scale = Vector3.Lerp(gasBaseScale, gasMaxScale, t);
        ApplyGasScale(scale);
    }

    void ApplyGasScale(Vector3 scale)
    {
        if (gasSphere != null)
        {
            gasSphere.transform.localScale = scale;
        }

        if (gasCollider == null) return;

        float radiusScale = Mathf.Max(scale.x, scale.y, scale.z);
        gasCollider.radius = Mathf.Max(gasBaseScale.x, gasBaseScale.y, gasBaseScale.z) * 0.5f * (radiusScale / Mathf.Max(gasBaseScale.x, 0.01f));
    }

    void OnTriggerStay(Collider other)
    {
        if (phase != Phase.GasActive) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (gasSphere == null || !gasSphere.activeSelf) return;
        if (Level2BubbleShield.BlocksEnvironmentalHazards) return;

        tick += Time.deltaTime;
        if (tick < 0.45f) return;
        tick = 0f;

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-damagePerTick, "Poison gas hurt you!");
        hud?.ChangePlayerWater(-waterDrainPerTick, "Poison gas drained your water.");
    }

    static void Pulse(GameObject go, Vector3 baseScale, float speed, float amount)
    {
        if (go == null) return;
        float pulse = 1f + Mathf.Sin(Time.time * speed) * amount;
        go.transform.localScale = baseScale * pulse;
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
