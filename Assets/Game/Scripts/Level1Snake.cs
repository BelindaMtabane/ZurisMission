using UnityEngine;

/// <summary>
/// Snake hazard: single-lane approach ahead of the player, one active at a time.
/// Snakes placed in the first 25% of the level show a warning before moving.
/// </summary>
public class Level1Snake : MonoBehaviour
{
    const float WarningProgressCutoff = 0.25f;
    const float TriggerDistance = 58f;
    const float WarningDuration = 2.8f;
    const float ApproachSpeed = 8f;
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
    bool dodged;
    bool nearMissShown;
    Transform player;

    public void Setup(int lane, GameObject visuals, float progress, GameObject warning)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        visualRoot = visuals;
        warningRoot = warning;
        spawnProgress = progress;

        if (visualRoot != null) visualRoot.SetActive(false);
        if (warningRoot != null) warningRoot.SetActive(false);
    }

    bool UsesWarning => spawnProgress <= WarningProgressCutoff;

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

        float x = LevelLanes.X(laneIndex);
        Vector3 p = transform.position;
        p.x = x;
        p.y = Level1Ground.SurfaceY + 0.45f;
        transform.position = p;

        if (phase == Phase.Wait)
        {
            if (player.position.z > transform.position.z - TriggerDistance)
            {
                if (UsesWarning)
                {
                    phase = Phase.Warning;
                    BeginWarning();
                }
                else
                {
                    phase = Phase.Queued;
                }
            }

            return;
        }

        if (phase == Phase.Warning)
        {
            warningTimer -= Time.deltaTime;
            PulseWarning();

            if (warningTimer <= 0f)
            {
                EndWarning();
                phase = Phase.Queued;
            }

            return;
        }

        if (phase == Phase.Queued)
        {
            if (!Level1SnakeDirector.TryStartSnake(this))
            {
                return;
            }

            phase = Phase.Approach;
            if (visualRoot != null) visualRoot.SetActive(true);
            Debug.Log($"[Level1] Snake moving lane {LevelLanes.DisplayNumber(laneIndex)}");
            return;
        }

        transform.position += Vector3.back * (ApproachSpeed * Time.deltaTime);

        if (!dodged && !hit && player != null)
        {
            float laneDelta = Mathf.Abs(player.position.x - x);

            if (laneDelta > 2.5f && transform.position.z < player.position.z + 2f && transform.position.z > player.position.z - 8f)
            {
                dodged = true;
                Level1FeedbackUI.Show("GREAT!", new Color(0.35f, 0.95f, 0.45f), 1.2f);
            }
            else if (!nearMissShown && !dodged && laneDelta > 1.8f && laneDelta <= 2.5f
                && transform.position.z < player.position.z + 1f && transform.position.z > player.position.z - 6f)
            {
                nearMissShown = true;
                Level1FeedbackUI.Show("NEAR MISS!", new Color(1f, 0.85f, 0.25f), 0.9f);
            }
        }

        if (transform.position.z < player.position.z - DespawnBehind)
        {
            Level1SnakeDirector.ReleaseSnake(this);
            Destroy(gameObject);
        }
    }

    void BeginWarning()
    {
        warningTimer = WarningDuration;

        if (warningRoot != null)
        {
            warningRoot.SetActive(true);
        }

        Level1FeedbackUI.Show(
            $"SNAKE AHEAD! Lane {LevelLanes.DisplayNumber(laneIndex)} — use A or D!",
            new Color(1f, 0.45f, 0.2f),
            WarningDuration);
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
