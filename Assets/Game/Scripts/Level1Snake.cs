using UnityEngine;

/// <summary>
/// Snake hazard: spawns ahead of the player with a lane warning, then rolls toward them.
/// Later snakes appear closer for a tougher finish.
/// </summary>
public class Level1Snake : MonoBehaviour
{
    const float ActivationLookahead = 18f;
    const float MinLeadDistance = 28f;
    const float EarlyLeadDistance = 70f;
    const float LateLeadDistance = 34f;
    const float ChallengeProgressStart = 0.70f;
    const float EarlyWarningDuration = 2.2f;
    const float LateWarningDuration = 1.35f;
    const float WarningCreepSpeed = 7.5f;
    const float MidWarningCreepSpeed = 9f;
    const float LateWarningCreepSpeed = 10.5f;
    const float ApproachSpeed = 14.5f;
    const float MidApproachSpeed = 16.5f;
    const float LateApproachSpeed = 18.5f;
    const float MidChallengeProgress = Level1Config.SnakeIntroProgress;
    const float DespawnBehind = 14f;
    const float WaterDamage = 2f;

    [SerializeField] private int laneIndex;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private GameObject warningRoot;
    [SerializeField] private float spawnProgress;

    enum Phase
    {
        Wait,
        Warning,
        Queued,
        Approach
    }

    Phase phase = Phase.Wait;
    float warningTimer;
    bool hit;
    bool approachPopupShown;
    bool placedAhead;
    float triggerZ;
    Transform player;

    public void Setup(int lane, GameObject visuals, float progress, GameObject warning)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        visualRoot = visuals;
        warningRoot = warning;
        spawnProgress = progress;
        triggerZ = transform.position.z;

        if (visualRoot != null) visualRoot.SetActive(false);
        if (warningRoot != null) warningRoot.SetActive(false);
    }

    public float SpawnProgress => spawnProgress;

    void Start()
    {
        CachePlayer();
        if (visualRoot != null) visualRoot.SetActive(false);
        if (warningRoot != null) warningRoot.SetActive(false);
    }

    void OnDestroy()
    {
        Level1SnakeDirector.ReleaseSnake(this);
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        CachePlayer();
        if (player == null) return;

        if (phase == Phase.Wait)
        {
            if (player.position.z >= triggerZ - ActivationLookahead)
            {
                PlaceAheadOfPlayer();
                phase = Phase.Warning;
                BeginWarning();
            }

            return;
        }

        LockLanePosition();

        if (phase == Phase.Warning)
        {
            warningTimer -= Time.deltaTime;
            PulseWarning();
            MoveTowardPlayer(GetCreepSpeed());

            if (warningTimer <= 0f)
            {
                EndWarning();
                phase = Phase.Queued;
            }

            return;
        }

        if (phase == Phase.Queued)
        {
            MoveTowardPlayer(GetCreepSpeed());

            if (!Level1SnakeDirector.TryStartSnake(this))
            {
                return;
            }

            phase = Phase.Approach;
            Debug.Log($"[Level1] Snake charging lane {LevelLanes.DisplayNumber(laneIndex)} (lead was {GetLeadDistance():F0}m)");
            return;
        }

        MoveTowardPlayer(GetApproachSpeed());
        LockLanePosition();

        if (transform.position.z < player.position.z - DespawnBehind)
        {
            Level1SnakeDirector.ReleaseSnake(this);
            Destroy(gameObject);
        }
    }

    void MoveTowardPlayer(float speed)
    {
        transform.position += Vector3.back * (speed * Time.deltaTime);
    }

    float GetCreepSpeed()
    {
        if (spawnProgress <= MidChallengeProgress)
        {
            return WarningCreepSpeed;
        }

        if (spawnProgress <= ChallengeProgressStart)
        {
            float midT = Mathf.InverseLerp(MidChallengeProgress, ChallengeProgressStart, spawnProgress);
            return Mathf.Lerp(WarningCreepSpeed, MidWarningCreepSpeed, midT);
        }

        float lateT = Mathf.InverseLerp(ChallengeProgressStart, 1f, spawnProgress);
        return Mathf.Lerp(MidWarningCreepSpeed, LateWarningCreepSpeed, lateT);
    }

    float GetApproachSpeed()
    {
        if (spawnProgress <= MidChallengeProgress)
        {
            return ApproachSpeed;
        }

        if (spawnProgress <= ChallengeProgressStart)
        {
            float midT = Mathf.InverseLerp(MidChallengeProgress, ChallengeProgressStart, spawnProgress);
            return Mathf.Lerp(ApproachSpeed, MidApproachSpeed, midT);
        }

        float lateT = Mathf.InverseLerp(ChallengeProgressStart, 1f, spawnProgress);
        return Mathf.Lerp(MidApproachSpeed, LateApproachSpeed, lateT);
    }

    void PlaceAheadOfPlayer()
    {
        if (placedAhead || player == null) return;

        placedAhead = true;
        float lead = GetLeadDistance();
        float z = player.position.z + lead;
        z = Mathf.Max(z, player.position.z + MinLeadDistance);

        Vector3 p = transform.position;
        p.x = LevelLanes.X(laneIndex);
        p.y = Level1Ground.SurfaceY + 0.45f;
        p.z = z;
        transform.position = p;
    }

    void LockLanePosition()
    {
        float x = LevelLanes.X(laneIndex);
        Vector3 p = transform.position;
        p.x = x;
        p.y = Level1Ground.SurfaceY + 0.45f;
        transform.position = p;
    }

    float GetLeadDistance()
    {
        if (spawnProgress <= ChallengeProgressStart)
        {
            return EarlyLeadDistance;
        }

        float t = Mathf.InverseLerp(ChallengeProgressStart, 1f, spawnProgress);
        return Mathf.Lerp(EarlyLeadDistance, LateLeadDistance, t);
    }

    float GetWarningDuration()
    {
        if (spawnProgress <= ChallengeProgressStart)
        {
            return EarlyWarningDuration;
        }

        float t = Mathf.InverseLerp(ChallengeProgressStart, 1f, spawnProgress);
        return Mathf.Lerp(EarlyWarningDuration, LateWarningDuration, t);
    }

    void BeginWarning()
    {
        warningTimer = GetWarningDuration();

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        if (warningRoot != null)
        {
            warningRoot.SetActive(true);
        }

        ShowApproachWarning();
    }

    void ShowApproachWarning()
    {
        if (approachPopupShown) return;
        approachPopupShown = true;
        Level1FeedbackUI.Show(
            $"SNAKE APPROACHING! Lane {LevelLanes.DisplayNumber(laneIndex)} — use A or D!",
            new Color(1f, 0.45f, 0.2f),
            2.2f);
    }

    void PulseWarning()
    {
        if (warningRoot == null) return;

        float pulse = 1f + Mathf.Sin(Time.time * 7f) * 0.18f;
        warningRoot.transform.localScale = new Vector3(pulse, pulse, pulse);
    }

    void EndWarning()
    {
        if (warningRoot != null) warningRoot.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hit) return;
        if (!other.CompareTag("Player")) return;
        if (phase != Phase.Approach) return;

        hit = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.DrainPlayerWater(WaterDamage);
        Level1FeedbackUI.Show(
            $"-{WaterDamage:0} WATER (snake bite!)",
            new Color(0.85f, 0.35f, 0.15f),
            1.4f);
        Debug.Log($"[Level1] Snake hit — water -{WaterDamage:0}");
    }

    void CachePlayer()
    {
        if (player != null) return;

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;

        if (player == null)
        {
            GameObject go = GameObject.Find("Player");
            if (go != null) player = go.transform;
        }
    }
}
