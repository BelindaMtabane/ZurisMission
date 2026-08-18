using UnityEngine;

/// <summary>
/// Warthog that homes onto the player's lane during a charge. Lane changes cannot
/// escape it — the player must jump over it to avoid damage.
/// </summary>
public class Level2Warthog : MonoBehaviour
{
    [SerializeField] bool movingRight = true;
    [SerializeField] float speed = 38f;
    [SerializeField] GameObject visualRoot;

    enum Phase { Wait, Telegraph, Cross, Done }
    Phase phase = Phase.Wait;
    bool hit;
    bool warned;
    Transform player;
    PlayerController playerController;
    float leftX;
    float rightX;
    float crossWidth;
    float telegraphTimer;
    float crossElapsed;
    float spawnZ;

    const float TelegraphSeconds = 0.75f;
    const float WarningLeadSeconds = 0.85f;
    const float CrossStartFraction = 0.40f;
    const float BurstSeconds = 0.5f;
    const float BurstMultiplier = 1.6f;
    const float MinPlayerSpeed = 14f;
    const float MinCrossSpeed = 36f;
    const float MaxCrossSpeed = 48f;
    const float MinTrackSpeed = 92f;
    const float MaxTrackSpeed = 110f;
    const float JumpClearanceY = 1.6f;
    const float HitLaneHalfWidth = 4.2f;
    const float HitDepthHalfWidth = 4.5f;
    const float PlayerClearDistanceZ = 1.25f;
    const float PathEdgePadding = 5f;

    public void Setup(bool goRight, float crossSpeed, GameObject visual)
    {
        movingRight = goRight;
        speed = Mathf.Clamp(crossSpeed, MinCrossSpeed, MaxCrossSpeed);
        visualRoot = visual;

        leftX = LevelLanes.X(0) - PathEdgePadding;
        rightX = LevelLanes.X(LevelLanes.Count - 1) + PathEdgePadding;
        crossWidth = rightX - leftX;
        spawnZ = transform.position.z;

        Vector3 p = transform.position;
        p.x = movingRight ? leftX : rightX;
        p.y = Level2Ground.SurfaceY + 0.7f;
        transform.position = p;

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
            UpdateVisualFacing(p.x, movingRight ? leftX + 1f : rightX - 1f);
        }
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        Vector3 p = transform.position;
        p.y = Level2Ground.SurfaceY + 0.7f;
        transform.position = p;

        float distanceToCrossing = spawnZ - player.position.z;
        if (distanceToCrossing <= 0f && phase != Phase.Cross)
        {
            phase = Phase.Done;
            Destroy(gameObject);
            return;
        }

        if (phase == Phase.Cross)
        {
            CrossStep();
            return;
        }

        if (distanceToCrossing <= 0f) return;

        float playerSpeed = playerController != null
            ? Mathf.Max(playerController.CurrentSpeed, MinPlayerSpeed)
            : MinPlayerSpeed;
        float crossDuration = crossWidth / speed;
        float crossStartDelay = crossDuration * CrossStartFraction;
        float timeToArrival = distanceToCrossing / playerSpeed;

        switch (phase)
        {
            case Phase.Wait:
                if (timeToArrival <= crossStartDelay + WarningLeadSeconds)
                {
                    BeginTelegraph();
                }
                break;

            case Phase.Telegraph:
                telegraphTimer -= Time.deltaTime;
                if (timeToArrival <= crossStartDelay)
                {
                    BeginCross();
                }
                break;
        }
    }

    void OnTriggerEnter(Collider other) => TryHit(other);
    void OnTriggerStay(Collider other) => TryHit(other);

    void TryHit(Collider other)
    {
        if (hit || phase != Phase.Cross) return;
        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerController>() == null) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (IsJumpingOver(other)) return;

        float laneDelta = Mathf.Abs(other.transform.position.x - transform.position.x);
        if (laneDelta > HitLaneHalfWidth) return;

        float depthDelta = Mathf.Abs(other.transform.position.z - spawnZ);
        if (depthDelta > HitDepthHalfWidth) return;

        hit = true;
        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-Level2Config.WarthogHealthDamage, "A warthog charged into you!");
    }

    bool IsJumpingOver(Collider other)
    {
        if (playerController == null)
        {
            playerController = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
        }

        if (playerController != null && playerController.IsGrounded)
        {
            return false;
        }

        return other.transform.position.y > Level2Ground.SurfaceY + JumpClearanceY;
    }

    void BeginTelegraph()
    {
        if (phase != Phase.Wait) return;
        phase = Phase.Telegraph;
        telegraphTimer = TelegraphSeconds;
        if (!warned)
        {
            warned = true;
            Level2FeedbackUI.Show("WARTHOG CHARGING — JUMP!", new Color(1f, 0.55f, 0.15f), 1.2f);
        }
    }

    void BeginCross()
    {
        phase = Phase.Cross;
        crossElapsed = 0f;
    }

    void CrossStep()
    {
        crossElapsed += Time.deltaTime;
        Vector3 p = transform.position;
        float previousX = p.x;
        float playerX = player.position.x;
        float playerZ = player.position.z;

        if (playerZ <= spawnZ + PlayerClearDistanceZ)
        {
            float trackSpeed = GetLiveTrackingSpeed(playerX);
            p.x = Mathf.MoveTowards(p.x, playerX, trackSpeed * Time.deltaTime);
        }
        else
        {
            float exitDir = movingRight ? 1f : -1f;
            p.x += exitDir * GetBurstSpeed() * Time.deltaTime;
        }

        p.z = spawnZ;
        p.y = Level2Ground.SurfaceY + 0.7f;
        UpdateVisualFacing(p.x, previousX);
        ApplyPosition(p);

        if ((movingRight && p.x > rightX) || (!movingRight && p.x < leftX))
        {
            phase = Phase.Done;
            Destroy(gameObject);
        }
    }

    float GetLiveTrackingSpeed(float playerX)
    {
        float burst = GetBurstSpeed();
        float distanceToPlayer = Mathf.Abs(playerX - transform.position.x);
        float playerSpeed = playerController != null
            ? Mathf.Max(playerController.CurrentSpeed, MinPlayerSpeed)
            : MinPlayerSpeed;
        float timeToMeet = Mathf.Max(0.03f, (spawnZ - player.position.z) / playerSpeed);
        float interceptSpeed = distanceToPlayer / timeToMeet;

        // Faster than lane-switch speed so only jumping avoids the hog.
        return Mathf.Clamp(Mathf.Max(burst, interceptSpeed, MinTrackSpeed), MinCrossSpeed, MaxTrackSpeed);
    }

    float GetBurstSpeed()
    {
        return crossElapsed <= BurstSeconds ? speed * BurstMultiplier : speed;
    }

    void ApplyPosition(Vector3 p)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.MovePosition(p);
        }
        else
        {
            transform.position = p;
        }
    }

    void UpdateVisualFacing(float currentX, float previousX)
    {
        if (visualRoot == null) return;
        if (Mathf.Abs(currentX - previousX) < 0.001f) return;

        bool faceRight = currentX > previousX;
        visualRoot.transform.localRotation = faceRight
            ? Quaternion.identity
            : Quaternion.Euler(0f, 180f, 0f);
    }

    void CachePlayer()
    {
        if (player != null) return;
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null) player = playerController.transform;
    }
}
