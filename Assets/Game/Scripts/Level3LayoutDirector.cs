using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-40)]
public class Level3LayoutDirector : MonoBehaviour
{
    const string RootName = "Level3_Layout";
    const string SceneName = "Level3";

    static readonly string[] MaterialCycle = { "Pipe", "Nails", "Tape", "Hammer" };

    static readonly (int lane, float p)[] Tank1Pipes =
    {
        (1, 0.004f), (2, 0.05f), (0, 0.10f), (3, 0.16f),
        (1, 0.22f), (2, 0.28f), (0, 0.34f), (3, 0.40f)
    };

    static readonly (int lane, float p)[] Tank2Pipes =
    {
        (0, 0.42f), (3, 0.46f), (1, 0.50f), (2, 0.54f),
        (0, 0.58f), (3, 0.62f), (1, 0.66f), (2, 0.70f),
        (0, 0.74f), (3, 0.77f), (1, 0.80f), (2, 0.83f),
        (0, 0.86f), (3, 0.88f), (1, 0.90f)
    };

    static readonly (int lane, float p)[] Tank3Pipes =
    {
        (2, 0.88f), (1, 0.92f), (3, 0.95f), (0, 0.97f), (2, 0.99f)
    };

    [Header("Spawn Distances (world units)")]
    [SerializeField] float initialSpawnDistance = 16f;
    [SerializeField] float initialSpawnBuffer = 8f;
    [SerializeField] int initialSpawnCount = 8;
    [SerializeField] float visibleSpawnDistance = 80f;
    [SerializeField] float minimumObjectSpacing = 18f;
    [SerializeField] float minimumHazardSpacing = 26f;
    [SerializeField] float minimumPipeSpacing = 80f;
    [SerializeField] bool enableSpawnDebug;

    [Header("Enemy Speeds")]
    [SerializeField] float snakeSlow = 4f;
    [SerializeField] float snakeMedium = 6f;
    [SerializeField] float snakeFast = 8f;
    [SerializeField] float warthogSlow = 14f;
    [SerializeField] float warthogMedium = 18f;
    [SerializeField] float warthogFast = 22f;

    Transform layoutRoot;
    Transform player;
    float cleanupTimer;
    int materialIndex;
    int dropletIndex;
    int healthIndex;
    int snakeLaneIndex;
    int acidLaneIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => BootScene(scene.name);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() => BootScene(SceneManager.GetActiveScene().name);

    static void BootScene(string sceneName)
    {
        if (sceneName != SceneName) return;
        if (FindFirstObjectByType<Level3LayoutDirector>() != null) return;
        new GameObject("Level3LayoutDirector").AddComponent<Level3LayoutDirector>();
    }

    void Awake()
    {
        ApplySpawnSettings();
        Level3EnemySpeeds.SnakeSlow = snakeSlow;
        Level3EnemySpeeds.SnakeMedium = snakeMedium;
        Level3EnemySpeeds.SnakeFast = snakeFast;
        Level3EnemySpeeds.WarthogSlow = warthogSlow;
        Level3EnemySpeeds.WarthogMedium = warthogMedium;
        Level3EnemySpeeds.WarthogFast = warthogFast;
    }

    void ApplySpawnSettings()
    {
        Level3Config.InitialSpawnDistance = Mathf.Max(12f, initialSpawnDistance);
        Level3Config.InitialSpawnBuffer = Mathf.Max(0f, initialSpawnBuffer);
        Level3Config.InitialSpawnCount = Mathf.Max(4, initialSpawnCount);
        Level3Config.VisibleSpawnDistance = Mathf.Max(60f, visibleSpawnDistance);
        Level3Config.MinimumObjectSpacing = Mathf.Max(8f, minimumObjectSpacing);
        Level3Config.MinimumHazardSpacing = Mathf.Max(12f, minimumHazardSpacing);
        Level3Config.MinimumPipeSpacing = Mathf.Max(40f, minimumPipeSpacing);
        Level3Config.EnableSpawnDebug = enableSpawnDebug;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneName)
        {
            Destroy(this);
            return;
        }

        ApplySpawnSettings();
        player = FindPlayer();
        Level3Progress.BindFromScene(player);
        RunnerLevelPacing.Apply(SceneName);
        LevelLanes.ConfigureForActiveScene();
        if (player != null)
        {
            Vector3 pos = player.position;
            pos.x = LevelLanes.X(1);
            player.position = pos;
            Level3Progress.BindFromScene(player);
        }

        ClearExistingGameplay(player);
        try { BuildLayout(); }
        catch (System.Exception ex) { Debug.LogException(ex); }

        if (Level3Config.EnableSpawnDebug)
        {
            Debug.Log($"[Level3 Spawn] playerZ={Level3Progress.StartZ:F1} contentStart={Level3Progress.ContentStartZ:F1} endZ={Level3Progress.EndZ:F1}");
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != SceneName) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        ApplyUrgencyDifficulty();

        cleanupTimer += Time.deltaTime;
        if (cleanupTimer < 0.6f) return;
        cleanupTimer = 0f;
        RecycleBehindPlayer();
    }

    void ApplyUrgencyDifficulty()
    {
        if (Level3TimeLimit.Instance == null) return;
        float u = Level3TimeLimit.Instance.Urgency; // 0..1

        // Tighten hazard spacing as urgency grows (max 40 % reduction)
        float baseHazard = Level3Config.MinimumHazardSpacing;
        Level3Config.MinimumHazardSpacing = Mathf.Max(14f, baseHazard * (1f - u * 0.40f));

        // Ramp enemy speeds (up to 50 % faster)
        float ramp = 1f + u * 0.5f;
        Level3EnemySpeeds.WarthogSlow   = warthogSlow   * ramp;
        Level3EnemySpeeds.WarthogMedium = warthogMedium * ramp;
        Level3EnemySpeeds.WarthogFast   = warthogFast   * ramp;
        Level3EnemySpeeds.SnakeSlow     = snakeSlow     * ramp;
        Level3EnemySpeeds.SnakeMedium   = snakeMedium   * ramp;
        Level3EnemySpeeds.SnakeFast     = snakeFast     * ramp;
    }

    void RecycleBehindPlayer()
    {
        if (layoutRoot == null) return;
        if (player == null) player = FindPlayer();
        if (player == null) return;

        float cutoff = player.position.z - 50f;
        for (int i = layoutRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = layoutRoot.GetChild(i);
            if (child == null) continue;
            if (child.name.Contains("Tank") || child.name.Contains("Boss")) continue;
            if (child.position.z < cutoff) Destroy(child.gameObject);
        }
    }

    static Transform FindPlayer()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform;
        GameObject p = GameObject.Find("Player");
        return p != null ? p.transform : null;
    }

    void ClearExistingGameplay(Transform currentPlayer)
    {
        string[] tags =
        {
            "WaterDROP", "DamWaterBUCK", "Materials", "Herbs", "FruitPickup",
            "SpeedBoast", "Obstacle", "Heat&Disease", "AnimalAttack", "SlowDown",
            "PipeFix1", "PipeFix2", "PipeFix3", "PipeFix", "PipeHit"
        };

        for (int t = 0; t < tags.Length; t++)
        {
            GameObject[] found;
            try { found = GameObject.FindGameObjectsWithTag(tags[t]); }
            catch { continue; }

            for (int i = 0; i < found.Length; i++)
            {
                GameObject go = found[i];
                if (go == null || go.CompareTag("Player") || go.CompareTag("EndLvl3End")) continue;
                if (go.layer == 5) continue;
                if (currentPlayer != null && go.transform.IsChildOf(currentPlayer)) continue;
                go.SetActive(false);
            }
        }

        string[] named = { "PipeFix", "Tanklvl1", "Tanklvl2", "Tanklvl3", "HeatWaveDirector", "SnakePassDirector", "Snakes" };
        for (int i = 0; i < named.Length; i++)
        {
            GameObject go = GameObject.Find(named[i]);
            if (go != null) go.SetActive(false);
        }

        DisableBehaviours<PipeControlslevel3>();
        DisableBehaviours<Lanemanager3>();
        DisableBehaviours<Lanemanager2>();
        DisableBehaviours<HeatWaveDirector>();
        DisableBehaviours<SnakePassDirector>();
        DisableBehaviours<PickupCollectable>();
        DisableBehaviours<MovingObstacle>();
        DisableBehaviours<Spawner>();

        SpawnObjects spawnObjects = FindFirstObjectByType<SpawnObjects>();
        if (spawnObjects != null)
        {
            spawnObjects.spawnCount = 0;
            spawnObjects.enabled = false;
        }
    }

    void BuildLayout()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Destroy(existing);

        layoutRoot = new GameObject(RootName).transform;
        Level3PipeRepair repair = gameObject.GetComponent<Level3PipeRepair>();
        if (repair == null) repair = gameObject.AddComponent<Level3PipeRepair>();

        materialIndex = 0;
        dropletIndex = 0;
        healthIndex = 0;
        snakeLaneIndex = 0;
        acidLaneIndex = 0;

        // Level 3 is time-limited (~3 minutes).
        if (GetComponent<Level3TimeLimit>() == null)
        {
            gameObject.AddComponent<Level3TimeLimit>();
        }

        float openingEnd = SpawnInitialOpening(layoutRoot);
        SpawnAllPipeRepairs(layoutRoot);
        FillAliveWorld(layoutRoot, openingEnd);
        PlaceTankDisplays(layoutRoot, repair);

        Level3BossDirector boss = new GameObject("Level3Boss").AddComponent<Level3BossDirector>();
        boss.transform.SetParent(layoutRoot, false);
        boss.Setup(layoutRoot, 0.955f);

        FindFirstObjectByType<HUDControls>()?.ApplyLevel3TankProgress(0, 0, 0);
    }

    float SpawnInitialOpening(Transform root)
    {
        float z = Level3Progress.StartZ + Level3Config.InitialSpawnDistance;
        float gap = Mathf.Max(6f, Level3Config.InitialSpawnBuffer);

        DropletAt(root, 1, z);
        z += gap;
        MatAt(root, 2, z);
        z += gap;
        RockAt(root, 0, z);
        z += gap;
        HealthAt(root, 3, z);
        DropletAt(root, 1, z + 4f);
        z += gap + 4f;
        Level3Primitives.MakePipeRepair(root, 0, 1, z);
        z += gap;
        MudAt(root, 2, z);
        DropletAt(root, 1, z + 4f);
        MatAt(root, 3, z + 6f);
        SnakeAt(root, z + 22f, 0.02f);
        WarthogAt(root, z + 48f, Level3EnemyPace.Medium, true);
        TreeAt(root, 0, z + 12f);
        RockAt(root, 3, z + 16f);

        if (Level3Config.EnableSpawnDebug)
        {
            Debug.Log($"[Level3 Spawn] Opening ends at z={z:F1} (playerZ={Level3Progress.StartZ:F1})");
        }

        return z + Level3Config.MinimumObjectSpacing;
    }

    void FillAliveWorld(Transform root, float fromZ)
    {
        float spacing = Level3Config.MinimumObjectSpacing;
        float hazardGap = Level3Config.MinimumHazardSpacing;
        float nextHazardZ = fromZ + 28f;
        int i = 0;

        for (float z = fromZ; z < Level3Progress.EndZ - 40f; z += spacing)
        {
            float p = Level3Progress.Normalized(z);
            int lane = i % 4;
            int beat = i % 6;

            if (p < 0.20f)
            {
                FillIntroBeat(root, beat, z, p, ref nextHazardZ, hazardGap);
            }
            else if (p < 0.95f)
            {
                FillPickupBeat(root, beat, lane, z);
                if (z >= nextHazardZ)
                {
                    // Intensity escalates with both world progress and urgency
                    float urgency = Level3TimeLimit.Instance != null ? Level3TimeLimit.Instance.Urgency : 0f;
                    int baseIntensity = p < 0.40f ? 0 : p < 0.55f ? 1 : p < 0.70f ? 2 : 3;
                    int intensity = Mathf.Min(6, baseIntensity + Mathf.FloorToInt(urgency * 3f));
                    FillHazardBeat(root, p, z, intensity);

                    // Gap shrinks more aggressively when urgency is high
                    float mul = p < 0.40f ? 1.15f : p < 0.70f ? 1f : 0.9f;
                    mul *= 1f - urgency * 0.35f;  // up to 35 % tighter
                    nextHazardZ = z + hazardGap * Mathf.Max(0.5f, mul);
                }
            }
            else
            {
                DropletAt(root, lane, z);
                if (beat == 2) HealthAt(root, (lane + 2) % 4, z);
                if (beat == 4) MatAt(root, (lane + 1) % 4, z);
            }

            i++;
        }
    }

    void FillIntroBeat(Transform root, int beat, float z, float p, ref float nextHazardZ, float hazardGap)
    {
        switch (beat)
        {
            case 0: DropletAt(root, 1, z); break;
            case 1: MatAt(root, 3, z); break;
            case 2: DropletAt(root, 0, z); RockAt(root, 2, z); TreeAt(root, 3, z + 6f); break;
            case 3: HealthAt(root, 2, z); break;
            case 4: MatAt(root, 1, z); break;
            default:
                DropletAt(root, 3, z);
                if (p > 0.12f && z >= nextHazardZ)
                {
                    if ((Mathf.Abs(Mathf.RoundToInt(z)) % 2) == 0) LightningAt(root, 1, z);
                    else AcidAt(root, z);
                    nextHazardZ = z + hazardGap * 1.3f;
                }
                break;
        }

        if (p > 0.18f && beat == 2) SnakeAt(root, z, p);
        if (p > 0.15f && beat == 4) LogAt(root, Mathf.Abs(Mathf.RoundToInt(z)) % 3, z + 8f, p);
    }

    void FillPickupBeat(Transform root, int beat, int lane, float z)
    {
        switch (beat)
        {
            case 0: DropletAt(root, 1, z); break;
            case 1: MatAt(root, 3, z); break;
            case 2:
                DropletAt(root, 0, z);
                HealthAt(root, 2, z + 2f);
                break;
            case 3: MatAt(root, 1, z); RockAt(root, 2, z); break;
            case 4: DropletAt(root, 2, z); break;
            default:
                DropletAt(root, (lane + 1) % 4, z);
                MatAt(root, (lane + 3) % 4, z);

                // Occasional speed-boost pickups during the run.
                if (Random.value < Level3Config.SpeedFruitSpawnChance)
                {
                    int speedLane = Random.Range(0, LevelLanes.Count);
                    SpeedFruitAt(root, speedLane, z + 2.2f);
                }
                break;
        }
    }

    void FillHazardBeat(Transform root, float p, float z, int intensity)
    {
        int lane = Mathf.Abs(Mathf.RoundToInt(z)) % 4;
        Level3EnemyPace pace = p < 0.35f ? Level3EnemyPace.Slow : p < 0.7f ? Level3EnemyPace.Medium : Level3EnemyPace.Fast;
        int kind = (Mathf.Abs(Mathf.RoundToInt(z * 0.2f)) + intensity) % 7;

        switch (kind)
        {
            case 0:
                LightningAt(root, lane, z);
                DropletAt(root, (lane + 2) % 4, z);
                break;
            case 1:
                WarthogAt(root, z, pace, lane % 2 == 0);
                break;
            case 2:
                MudAt(root, lane, z);
                MatAt(root, (lane + 1) % 4, z);
                break;
            case 3:
                SnakeAt(root, z, p);
                DropletAt(root, (lane + 3) % 4, z);
                break;
            case 4:
                AcidAt(root, z);
                HealthAt(root, (lane + 2) % 4, z + 6f);
                break;
            case 5:
                LogAt(root, lane % 3, z, p);
                DropletAt(root, (lane + 3) % 4, z);
                break;
            default:
                if (lane % 2 == 0) RockAt(root, lane, z);
                else TreeAt(root, lane, z);
                HealthAt(root, (lane + 2) % 4, z);
                break;
        }
    }

    void PlaceTankDisplays(Transform root, Level3PipeRepair repair)
    {
        Level3Primitives.MakeTankDisplay(root, 0, 2, Z(0.40f), repair);
        Level3Primitives.MakeTankDisplay(root, 1, 0, Z(0.76f), repair);
        Level3Primitives.MakeTankDisplay(root, 2, 1, Z(0.90f), repair);
    }

    void SpawnAllPipeRepairs(Transform root)
    {
        float lastZ = float.NegativeInfinity;
        SpawnPipeSet(root, Tank1Pipes, 0, ref lastZ);
        lastZ = float.NegativeInfinity;
        SpawnPipeSet(root, Tank2Pipes, 1, ref lastZ);
        lastZ = float.NegativeInfinity;
        SpawnPipeSet(root, Tank3Pipes, 2, ref lastZ);
    }

    void SpawnPipeSet(Transform root, (int lane, float p)[] pipes, int tank, ref float lastZ)
    {
        for (int i = 0; i < pipes.Length; i++)
        {
            float z = Z(pipes[i].p);
            if (z - lastZ < Level3Config.MinimumPipeSpacing)
            {
                z = lastZ + Level3Config.MinimumPipeSpacing;
            }

            if (tank == 0 && i == 0) continue;

            // Randomize which lane-pair the pipe appears on.
            int leftLane = Random.Range(0, LevelLanes.Count - 1); // safe: rightLane = leftLane+1
            Level3Primitives.MakePipeRepair(root, tank, leftLane, z);
            lastZ = z;
        }
    }

    static float Z(float p) => Level3Progress.WorldZ(p);

    void DropletAt(Transform root, int lane, float z)
    {
        float amount = Level3Config.DropletAmounts[dropletIndex % Level3Config.DropletAmounts.Length];
        Level3Primitives.MakeWaterDroplet(root, lane, z, amount);
        dropletIndex++;
        LogSpawn("Droplet", lane, z);
    }

    void MatAt(Transform root, int lane, float z)
    {
        Level3Primitives.MakeMaterial(root, lane, z, MaterialCycle[materialIndex % MaterialCycle.Length]);
        materialIndex++;
        LogSpawn("Material", lane, z);
    }

    void HealthAt(Transform root, int lane, float z)
    {
        float amount = Level3Config.HealthAmounts[healthIndex % Level3Config.HealthAmounts.Length];
        Level3Primitives.MakeHealth(root, lane, z, amount);
        healthIndex++;
        LogSpawn("Health", lane, z);
    }

    static void RockAt(Transform root, int lane, float z) => Level3Primitives.MakeRock(root, lane, z);
    static void TreeAt(Transform root, int lane, float z) => Level3Primitives.MakeTree(root, lane, z);
    static void MudAt(Transform root, int lane, float z) => Level3Primitives.MakeMudPuddle(root, lane, z);
    static void LightningAt(Transform root, int lane, float z) => Level3Primitives.MakeLightning(root, lane, z);
    void AcidAt(Transform root, float z)
    {
        int lane = Random.Range(0, LevelLanes.Count);
        acidLaneIndex++;
        Level3Primitives.MakeAcidRain(root, lane, z);
        LogSpawn("AcidRain", lane, z);
    }

    void LogAt(Transform root, int lane, float z, float p)
    {
        int span = p >= 0.5f ? 3 : 2;
        int startLane = Random.Range(0, LevelLanes.Count - span + 1);
        Level3Primitives.MakeRollingLog(root, startLane, z, span);
        LogSpawn(span >= 3 ? "RollingLog3" : "RollingLog2", startLane, z);
    }

    void SpeedFruitAt(Transform root, int lane, float z)
    {
        Level3Primitives.MakeSpeedFruit(root, lane, z);
    }
    void SnakeAt(Transform root, float z, float p)
    {
        int lane = snakeLaneIndex % 4;
        snakeLaneIndex++;
        Level3Primitives.MakeApproachSnake(root, lane, z, p);
        LogSpawn("Snake", lane, z);
    }
    static void WarthogAt(Transform root, float z, Level3EnemyPace pace, bool right) => Level3Primitives.MakeWarthog(root, z, pace, right);

    static void LogSpawn(string type, int lane, float z)
    {
        if (!Level3Config.EnableSpawnDebug) return;
        Debug.Log($"[Level3 Spawn] {type} lane={lane + 1} z={z:F1} ahead={z - Level3Progress.StartZ:F1} progress={Level3Progress.Normalized(z):P0}");
    }

    static void DisableBehaviours<T>() where T : Behaviour
    {
        T[] found = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null) found[i].enabled = false;
        }
    }
}
