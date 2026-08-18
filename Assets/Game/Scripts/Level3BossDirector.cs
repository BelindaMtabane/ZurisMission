using UnityEngine;

public class Level3BossDirector : MonoBehaviour
{
    public static bool BossSurvived { get; private set; }

    const float TriggerDistance = 70f;

    [SerializeField] Transform layoutRoot;
    [SerializeField] float spawnProgress = 0.955f;

    enum Phase { Wait, Lightning, Warthog, Snake, Mud, Lightning2, Finale, Done }
    Phase phase = Phase.Wait;
    float timer;
    Transform player;
    bool spawnedLightning;
    bool spawnedWarthog;
    bool spawnedSnake;
    bool spawnedMud;
    bool spawnedLightning2;
    float bossEndZ;

    public void Setup(Transform root, float progress)
    {
        layoutRoot = root;
        spawnProgress = progress;
        BossSurvived = false;
        transform.position = new Vector3(LevelLanes.PathCenterX, Level3Ground.SurfaceY, Level3Progress.WorldZ(progress));
        bossEndZ = transform.position.z + 160f;
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        if (player.position.z > bossEndZ && phase != Phase.Done)
        {
            phase = Phase.Done;
            BossSurvived = true;
            Level3FeedbackUI.Show("FINAL SURVIVAL COMPLETE!", new Color(0.35f, 0.9f, 1f), 2.5f);
            return;
        }

        switch (phase)
        {
            case Phase.Wait:
                if (player.position.z > transform.position.z - TriggerDistance)
                {
                    phase = Phase.Lightning;
                    timer = 3.5f;
                    Level3FeedbackUI.Show("FINAL CHALLENGE — LIGHTNING!", new Color(1f, 0.92f, 0.25f), 2f);
                }
                break;
            case Phase.Lightning:
                if (!spawnedLightning)
                {
                    spawnedLightning = true;
                    SpawnLightningWave(0);
                    SpawnLightningWave(1);
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Warthog;
                    timer = 3.5f;
                    Level3FeedbackUI.Show("WARTHOGS INCOMING!", new Color(1f, 0.55f, 0.15f), 1.6f);
                }
                break;
            case Phase.Warthog:
                if (!spawnedWarthog)
                {
                    spawnedWarthog = true;
                    SpawnWarthogWave(0);
                    SpawnWarthogWave(1);
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Snake;
                    timer = 3.5f;
                }
                break;
            case Phase.Snake:
                if (!spawnedSnake)
                {
                    spawnedSnake = true;
                    SpawnSnakeWave(0);
                    SpawnSnakeWave(1);
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Mud;
                    timer = 3.5f;
                    Level3FeedbackUI.Show("MUD — JUMP OR LOSE MATERIALS!", new Color(0.62f, 0.42f, 0.18f), 1.6f);
                }
                break;
            case Phase.Mud:
                if (!spawnedMud)
                {
                    spawnedMud = true;
                    SpawnMudWave();
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Lightning2;
                    timer = 3.5f;
                }
                break;
            case Phase.Lightning2:
                if (!spawnedLightning2)
                {
                    spawnedLightning2 = true;
                    SpawnLightningWave(2);
                    SpawnLightningWave(3);
                    SpawnPickupWave();
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Finale;
                    Level3FeedbackUI.Show("FINISH STRONG!", new Color(1f, 0.9f, 0.2f), 2f);
                }
                break;
        }
    }

    void SpawnLightningWave(int wave)
    {
        float z = transform.position.z + 8f + wave * 14f;
        Level3Primitives.MakeLightning(layoutRoot, wave % 4, z);
        Level3Primitives.MakeLightning(layoutRoot, (wave + 2) % 4, z + 12f);
    }

    void SpawnWarthogWave(int wave)
    {
        float z = transform.position.z + 22f + wave * 18f;
        Level3Primitives.MakeWarthog(layoutRoot, z, Level3EnemyPace.Medium, wave % 2 == 0);
        Level3Primitives.MakeWarthog(layoutRoot, z + 16f, Level3EnemyPace.Fast, wave % 2 != 0);
    }

    void SpawnSnakeWave(int wave)
    {
        float z = transform.position.z + 38f + wave * 16f;
        Level3Primitives.MakeApproachSnake(layoutRoot, wave % 4, z, spawnProgress);
        Level3Primitives.MakeApproachSnake(layoutRoot, (wave + 1) % 4, z + 14f, spawnProgress);
    }

    void SpawnMudWave()
    {
        float z = transform.position.z + 54f;
        Level3Primitives.MakeMudPuddle(layoutRoot, 1, z);
        Level3Primitives.MakeMudPuddle(layoutRoot, 2, z + 10f);
        Level3Primitives.MakeMudPuddle(layoutRoot, 0, z + 20f);
        Level3Primitives.MakeRock(layoutRoot, 3, z + 28f);
    }

    void SpawnPickupWave()
    {
        float z = transform.position.z + 72f;
        Level3Primitives.MakeWaterDroplet(layoutRoot, 0, z, 20f);
        Level3Primitives.MakeWaterDroplet(layoutRoot, 2, z + 8f, 15f);
        Level3Primitives.MakeMaterial(layoutRoot, 3, z + 16f, "Pipe");
        Level3Primitives.MakeHealth(layoutRoot, 1, z + 24f, 15f);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}

public class Level3BossRepairPoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Level3PipeRepair.AllTanksRepaired)
        {
            Level3FeedbackUI.Show("REPAIR ALL THREE TANKS FIRST", new Color(1f, 0.5f, 0.2f), 1.6f);
        }
    }
}
