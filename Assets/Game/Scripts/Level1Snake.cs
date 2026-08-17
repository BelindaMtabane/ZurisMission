using UnityEngine;

/// <summary>
/// Snake hazard for the 4-lane layout: one moves at a time, no warning, travels toward the player, then despawns.
/// </summary>
public class Level1Snake : MonoBehaviour
{
    const float TriggerDistance = 42f;
    const float ApproachSpeed = 16f;
    const float DespawnBehind = 12f;

    [SerializeField] private int laneIndex;
    [SerializeField] private GameObject visualRoot;

    enum Phase
    {
        Wait,
        Queued,
        Approach
    }

    Phase phase = Phase.Wait;
    bool hit;
    Transform player;

    public void Setup(int lane, GameObject visuals)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        visualRoot = visuals;
        if (visualRoot != null) visualRoot.SetActive(false);
    }

    void Start()
    {
        CachePlayer();
        if (visualRoot != null) visualRoot.SetActive(false);
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
            Debug.Log($"[Level1] Snake Spawned Lane {LevelLanes.DisplayNumber(laneIndex)}");
            return;
        }

        transform.position += Vector3.back * (ApproachSpeed * Time.deltaTime);

        if (transform.position.z < player.position.z - DespawnBehind)
        {
            Level1SnakeDirector.ReleaseSnake(this);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hit) return;
        if (!other.CompareTag("Player")) return;
        if (phase != Phase.Approach) return;

        hit = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.DrainPlayerWater(8f);
        Debug.Log("[Level1] Snake hit player — water -8");
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (player == null && pc != null) player = pc.transform;
        if (player == null)
        {
            GameObject go = GameObject.Find("Player");
            if (go != null) player = go.transform;
        }
    }
}
