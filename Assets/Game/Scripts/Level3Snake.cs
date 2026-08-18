using UnityEngine;

/// <summary>
/// Level 3 snake: visible light-green body that approaches from ahead and passes the player.
/// </summary>
public class Level3Snake : MonoBehaviour
{
    const float DespawnBehind = 18f;

    [SerializeField] int laneIndex;
    [SerializeField] GameObject visualRoot;
    [SerializeField] GameObject warningRoot;
    [SerializeField] float spawnProgress;

    enum Phase { Wait, Approach }
    Phase phase = Phase.Wait;
    bool hit;
    Transform player;

    public void Setup(int lane, GameObject visuals, float progress, GameObject warning)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        visualRoot = visuals;
        warningRoot = warning;
        spawnProgress = progress;
        if (visualRoot != null) visualRoot.SetActive(true);
        if (warningRoot != null) warningRoot.SetActive(true);
    }

    void OnDestroy()
    {
        Level3SnakeDirector.ReleaseSnake(this);
    }

    float StartAhead => Mathf.Max(110f, Level3Config.VisibleSpawnDistance + 30f);

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        Vector3 p = transform.position;
        p.x = LevelLanes.X(laneIndex);
        p.y = Level3Ground.SurfaceY + 0.45f;
        transform.position = p;

        if (phase == Phase.Wait)
        {
            if (player.position.z > transform.position.z - StartAhead)
            {
                Level3SnakeDirector.TryStartSnake(this);
                phase = Phase.Approach;
                Level3FeedbackUI.Show(
                    $"SNAKE AHEAD — LANE {LevelLanes.DisplayNumber(laneIndex)}!",
                    new Color(0.45f, 0.95f, 0.4f),
                    1.2f);
            }
            return;
        }

        transform.position += Vector3.back * (Level3Config.SnakeApproachSpeed * Time.deltaTime);
        if (player != null && transform.position.z < player.position.z - DespawnBehind)
        {
            Level3SnakeDirector.ReleaseSnake(this);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hit || phase != Phase.Approach || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null && !controller.IsGrounded)
        {
            hit = true;
            Level3FeedbackUI.Show("JUMPED OVER SNAKE!", new Color(0.4f, 0.95f, 0.45f), 0.9f);
            return;
        }

        hit = true;
        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-Level3Config.SnakeHealthDamage, "A snake bit you!");
        Level3FeedbackUI.Show("SNAKE BITE!", new Color(0.9f, 0.25f, 0.15f), 1f);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
