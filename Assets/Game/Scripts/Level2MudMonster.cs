using UnityEngine;

/// <summary>
/// Mud Monster pops up in a lane and rolls a mud ball toward the player.
/// </summary>
public class Level2MudMonster : MonoBehaviour
{
    const float TriggerDistance = 52f;

    [SerializeField] private int laneIndex;
    [SerializeField] private float spawnProgress;
    [SerializeField] private GameObject warningRoot;
    [SerializeField] private GameObject visualRoot;

    enum Phase { Wait, Throw, Done }

    Phase phase = Phase.Wait;
    float ballSpeed;
    float ballDamage;
    Transform player;
    Level2MudBall activeBall;

    public void Setup(int lane, float progress, GameObject warning, GameObject visuals)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        spawnProgress = progress;
        warningRoot = warning;
        visualRoot = visuals;

        if (spawnProgress <= 0.20f)
        {
            ballSpeed = 14f;
            ballDamage = 8f;
        }
        else if (spawnProgress <= 0.65f)
        {
            ballSpeed = 18f;
            ballDamage = 16f;
        }
        else if (spawnProgress <= 0.90f)
        {
            ballSpeed = 22f;
            ballDamage = 26f;
        }
        else
        {
            ballSpeed = 26f;
            ballDamage = 38f;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
            visualRoot.transform.localScale = Vector3.one;
            visualRoot.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        }

        if (warningRoot != null) warningRoot.SetActive(false);
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        CachePlayer();
        if (player == null) return;

        Vector3 p = transform.position;
        p.x = LevelLanes.X(laneIndex);
        p.y = Level2Ground.SurfaceY;
        transform.position = p;

        switch (phase)
        {
            case Phase.Wait:
                if (player.position.z > transform.position.z - TriggerDistance)
                {
                    PopUpNow();
                }
                break;

            case Phase.Throw:
                if (visualRoot != null)
                {
                    visualRoot.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 8f) * 0.04f);
                }

                if (activeBall == null || activeBall.HasPassed)
                {
                    phase = Phase.Done;
                    Level2MudMonsterDirector.ReleaseMonster(this);
                    Destroy(gameObject, 0.35f);
                }
                break;
        }
    }

    void PopUpNow()
    {
        Level2MudMonsterDirector.TryStartMonster(this);
        phase = Phase.Throw;

        if (warningRoot != null) warningRoot.SetActive(false);
        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
            visualRoot.transform.localScale = Vector3.one;
            visualRoot.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        }

        SpawnMudBall();
    }

    void SpawnMudBall()
    {
        GameObject ball = Level2Primitives.MakeMudBall(transform.parent, laneIndex, transform.position.z - 1.2f);
        activeBall = ball.GetComponent<Level2MudBall>();
        activeBall?.Launch(laneIndex, ballSpeed, ballDamage, player);
    }

    void OnDestroy()
    {
        Level2MudMonsterDirector.ReleaseMonster(this);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}

public class Level2MudBall : MonoBehaviour
{
    [SerializeField] private int laneIndex;
    [SerializeField] private float speed = 16f;
    [SerializeField] private float damage = 12f;

    Transform player;
    bool hit;
    bool passed;

    public bool HasPassed => passed;

    public void Launch(int lane, float rollSpeed, float healthDamage, Transform target)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        speed = rollSpeed;
        damage = healthDamage;
        player = target;
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
            if (player == null) return;
        }

        Vector3 p = transform.position;
        p.x = LevelLanes.X(laneIndex);
        p.y = Level2Ground.SurfaceY + 0.7f;
        p.z -= speed * Time.deltaTime;
        transform.position = p;
        transform.Rotate(Vector3.right, speed * 40f * Time.deltaTime, Space.World);

        if (!hit && player.position.z >= transform.position.z - 1.6f)
        {
            TryHit();
        }

        if (transform.position.z < player.position.z - 14f)
        {
            passed = true;
            Destroy(gameObject);
        }
    }

    void TryHit()
    {
        hit = true;

        if (Level2BubbleShield.TryBlockMudBall())
        {
            passed = true;
            Destroy(gameObject);
            return;
        }

        PlayerController controller = player.GetComponent<PlayerController>();
        float laneDelta = Mathf.Abs(player.position.x - LevelLanes.X(laneIndex));
        if (laneDelta > 3.2f || (controller != null && !controller.IsGrounded && player.position.y > Level2Ground.SurfaceY + 1.4f))
        {
            passed = true;
            return;
        }

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-Level2Config.MudBallHealthDamage, "A mud ball hit you!");
        controller?.ApplySpeedModifier(
            controller.CurrentSpeed * Level2Config.MudBallSlowMultiplier,
            Level2Config.MudBallSlowDuration);
        passed = true;
        Destroy(gameObject);
    }
}
