using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Clears legacy Level 2 scene pickups/obstacles and builds the mudlands layout.
/// Does not touch MainGame or Level3.
/// </summary>
[DefaultExecutionOrder(-40)]
public class Level2LayoutDirector : MonoBehaviour
{
    const string RootName = "Level2_Layout";

    static readonly Level2MaterialKind[] MaterialCycle =
    {
        Level2MaterialKind.Pipe,
        Level2MaterialKind.Nails,
        Level2MaterialKind.Hammer
    };

    [SerializeField] float mudSlowMultiplier = Level2MudSlowEffect.DefaultMultiplier;
    [SerializeField] float mudSlowDuration = Level2MudSlowEffect.DefaultDuration;
    readonly List<float>[] reservedLaneZs = { new List<float>(), new List<float>(), new List<float>(), new List<float>() };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BootScene(scene.name);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        BootScene(SceneManager.GetActiveScene().name);
    }

    static void BootScene(string sceneName)
    {
        if (sceneName != SceneCatalog.Level2) return;
        if (FindFirstObjectByType<Level2LayoutDirector>() != null) return;

        GameObject host = new GameObject("Level2LayoutDirector");
        host.AddComponent<Level2LayoutDirector>();
    }

    void Awake()
    {
        Level2MudSlowEffect.Multiplier = mudSlowMultiplier;
        Level2MudSlowEffect.Duration = mudSlowDuration;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.Level2)
        {
            Destroy(this);
            return;
        }

        Transform player = FindPlayer();
        Level2Progress.BindFromScene(player);
        RunnerPlayerSetup.Apply(SceneCatalog.Level2, player);
        RunnerLevelPacing.Apply(SceneCatalog.Level2);
        ClearExistingGameplay(player);
        try
        {
            BuildLayout();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        EnsureFinishTrigger();
        RefreshGroundTiles();

        Debug.Log("[Level2] Mudlands layout built.");
    }

    static Transform FindPlayer()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform;

        GameObject p = GameObject.Find("Player");
        return p != null ? p.transform : null;
    }

    void ClearExistingGameplay(Transform player)
    {
        string[] tags =
        {
            "WaterDROP", "DamWaterBUCK", "Materials", "Herbs", "FruitPickup",
            "SpeedBoast", "Obstacle", "Heat&Disease", "AnimalAttack", "SlowDown", "Finish"
        };

        for (int t = 0; t < tags.Length; t++)
        {
            GameObject[] found;
            try
            {
                found = GameObject.FindGameObjectsWithTag(tags[t]);
            }
            catch
            {
                continue;
            }

            for (int i = 0; i < found.Length; i++)
            {
                GameObject go = found[i];
                if (go == null) continue;
                if (go.CompareTag("Player") || go.CompareTag("EndLevel2")) continue;
                if (go.layer == 5) continue;
                if (player != null && go.transform.IsChildOf(player)) continue;
                go.SetActive(false);
            }
        }

        string[] namedRoots =
        {
            "Snakes", "Snakes (1)", "HeatWaveDirector", "SnakePassDirector",
            "HeatWaveFlash", "CarLeft", "CarLeft (1)", "Animal1 - Chase", "Pipe",
            "Pick1 - speed", "Pick2 - fruit", "Pick3 - waterDROP",
            "Pick5 - Materials", "Pick5 - Materials (1)", "Pick5 - Materials (2)",
            "obstacle1 - glass", "obstacle2 - pits", "obstacle3 - mud",
            "obstacle6 - trees", "obstacle7 - rocks",
            "River", "River (1)", "River (2)", "River (3)", "Heat&Disease"
        };

        for (int i = 0; i < namedRoots.Length; i++)
        {
            DisableByName(namedRoots[i]);
        }

        GameObject[] all = FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            GameObject go = all[i];
            if (go == null) continue;
            if (player != null && go.transform.IsChildOf(player)) continue;

            string n = go.name;
            if (n.StartsWith("Pick") || n.StartsWith("obstacle") || n.StartsWith("Obstacle") || n.StartsWith("River"))
            {
                go.SetActive(false);
            }
        }

        DisableBehaviours<Lanemanager2>();
        DisableBehaviours<HeatWaveDirector>();
        DisableBehaviours<Level1HeatWave>();
        DisableBehaviours<SnakePassDirector>();
        DisableBehaviours<BushlandHazard>();
        DisableBehaviours<SnakePassHazard>();
        DisableBehaviours<PickupCollectable>();
        DisableBehaviours<MovingObstacle>();
        DisableBehaviours<ObstacleRunner>();
        DisableBehaviours<Spawner>();
        DisableBehaviours<ithappy.Animals_FREE.CreatureMover>();

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
        if (existing != null)
        {
            Destroy(existing);
        }

        Transform root = new GameObject(RootName).transform;
        int materialIndex = 0;
        int healthIndex = 0;
        for (int i = 0; i < reservedLaneZs.Length; i++) reservedLaneZs[i].Clear();

        // === 0–15% TUTORIAL: cactus, mud, rocks, pickups — room to learn ===
        WaterDrop(root, 2, 0.010f);
        MudPuddle(root, 0, 0.022f);
        MaterialPickup(root, 1, 0.034f, NextMaterial(ref materialIndex));
        HealthFruit(root, 2, 0.046f, NextHealth(ref healthIndex));
        WaterPool(root, 1, 0.058f);
        Cactus(root, 3, 0.070f);
        Rock(root, 1, 0.082f);
        MudPuddle(root, 3, 0.090f);
        MaterialPickup(root, 2, 0.094f, NextMaterial(ref materialIndex));
        MudPuddle(root, 2, 0.106f);
        Baobab(root, 0, 0.118f);
        WaterDrop(root, 3, 0.140f);
        HealthFruit(root, 0, 0.148f, NextHealth(ref healthIndex));

        // === 15–30% warthogs enter, still dodgeable ===
        Warthog(root, 0.168f, true, 36f);
        WaterDrop(root, 1, 0.182f);
        MaterialPickup(root, 0, 0.194f, NextMaterial(ref materialIndex));
        Rock(root, 3, 0.206f);
        MudPuddle(root, 2, 0.218f);
        MudPuddle(root, 0, 0.226f);
        HealthFruit(root, 3, 0.230f, NextHealth(ref healthIndex));
        WaterPool(root, 1, 0.242f);
        Warthog(root, 0.272f, false, 36f);
        MaterialPickup(root, 2, 0.286f, NextMaterial(ref materialIndex));
        BubbleShield(root, 3, 0.298f);

        // === 30–45% logs, warthogs, light poison — gaps between ===
        RollingLog(root, 1, 0.318f, 2);
        MaterialPickup(root, 0, 0.334f, NextMaterial(ref materialIndex));
        HealthFruit(root, 3, 0.358f, NextHealth(ref healthIndex));
        Poison(root, 0, 0.372f);
        WaterDrop(root, 1, 0.386f);
        Warthog(root, 0.402f, true, 38f);
        MudPuddle(root, 3, 0.416f);
        MudPuddle(root, 1, 0.422f);
        WaterPool(root, 0, 0.428f);
        RollingLog(root, 2, 0.444f, 2);
        MaterialPickup(root, 3, 0.458f, NextMaterial(ref materialIndex));

        // === 45–60% mid-run: denser hazards with dodge lanes ===
        Monster(root, 2, 0.470f);
        Cactus(root, 1, 0.482f);
        WaterPool(root, 3, 0.490f);
        Rock(root, 0, 0.500f);
        Warthog(root, 0.512f, false, 38f);
        MudPuddle(root, 2, 0.524f);
        MudPuddle(root, 0, 0.538f);
        MaterialPickup(root, 0, 0.532f, NextMaterial(ref materialIndex));
        Poison(root, 2, 0.544f);
        Baobab(root, 3, 0.550f);
        HealthFruit(root, 1, 0.554f, NextHealth(ref healthIndex));
        RollingLog(root, 1, 0.566f, 2);
        BubbleShield(root, 1, 0.588f);
        Monster(root, 0, 0.600f);
        Rock(root, 3, 0.610f);
        WaterDrop(root, 2, 0.618f);
        MudPuddle(root, 1, 0.628f);
        MudPuddle(root, 3, 0.636f);

        // === 60–75% mixed pressure ===
        Warthog(root, 0.640f, true, 40f);
        MaterialPickup(root, 2, 0.658f, NextMaterial(ref materialIndex));
        Poison(root, 3, 0.668f);
        Rock(root, 1, 0.678f);
        HealthFruit(root, 1, 0.686f, NextHealth(ref healthIndex));
        RollingLog(root, 1, 0.698f, 2);
        MudPuddle(root, 2, 0.708f);
        MudPuddle(root, 1, 0.740f);
        Rock(root, 0, 0.716f);
        BubbleShield(root, 2, 0.726f);
        Monster(root, 3, 0.736f);
        WaterPool(root, 0, 0.754f);
        Warthog(root, 0.766f, false, 42f);
        Poison(root, 0, 0.776f);
        Baobab(root, 2, 0.780f);

        // === 75–100% finish stretch — more obstacles, still recoverable ===
        RollingLog(root, 2, 0.786f, 2);
        MaterialPickup(root, 3, 0.802f, NextMaterial(ref materialIndex));
        Rock(root, 0, 0.810f);
        Monster(root, 2, 0.820f);
        HealthFruit(root, 1, 0.828f, NextHealth(ref healthIndex));
        MudPuddle(root, 3, 0.836f);
        MudPuddle(root, 0, 0.844f);
        Warthog(root, 0.846f, true, 43f);
        Poison(root, 1, 0.856f);
        WaterDrop(root, 2, 0.872f);
        RollingLog(root, 0, 0.882f, 2);
        Warthog(root, 0.892f, false, 43f);
        MaterialPickup(root, 2, 0.900f, NextMaterial(ref materialIndex));
        Rock(root, 3, 0.908f);
        WaterPool(root, 1, 0.916f);
        Monster(root, 0, 0.926f);
        HealthFruit(root, 3, 0.934f, NextHealth(ref healthIndex));
        Baobab(root, 0, 0.950f);
        MudPuddle(root, 1, 0.958f);
        MudPuddle(root, 3, 0.972f);
        MaterialPickup(root, 1, 0.966f, NextMaterial(ref materialIndex));
        Warthog(root, 0.974f, true, 45f);
        WaterPool(root, 2, 0.982f);
        HealthFruit(root, 0, 0.990f, NextHealth(ref healthIndex));

        // Extra recovery pickups so the player can finish with full materials, health, water, and bucket
        MaterialPickup(root, 0, 0.055f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 3, 0.155f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 1, 0.255f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 2, 0.355f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 0, 0.455f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 3, 0.555f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 1, 0.655f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 2, 0.755f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 0, 0.855f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 3, 0.955f, NextMaterial(ref materialIndex));

        WaterDrop(root, 0, 0.088f);
        WaterDrop(root, 2, 0.188f);
        WaterDrop(root, 1, 0.388f);
        WaterDrop(root, 3, 0.588f);
        WaterDrop(root, 0, 0.788f);
        WaterDrop(root, 2, 0.888f);

        HealthFruit(root, 1, 0.118f, NextHealth(ref healthIndex));
        HealthFruit(root, 2, 0.318f, NextHealth(ref healthIndex));
        HealthFruit(root, 0, 0.518f, NextHealth(ref healthIndex));
        HealthFruit(root, 3, 0.618f, NextHealth(ref healthIndex));
        HealthFruit(root, 1, 0.718f, NextHealth(ref healthIndex));
        HealthFruit(root, 2, 0.838f, NextHealth(ref healthIndex));
        HealthFruit(root, 1, 0.918f, NextHealth(ref healthIndex));
    }

    static float NextHealth(ref int healthIndex)
    {
        float amount = Level2Config.HealthAmounts[healthIndex % Level2Config.HealthAmounts.Length];
        healthIndex++;
        return amount;
    }

    static Level2MaterialKind NextMaterial(ref int materialIndex)
    {
        Level2MaterialKind kind = MaterialCycle[materialIndex % MaterialCycle.Length];
        materialIndex++;
        return kind;
    }

    static float Z(float progress) => Level2Progress.WorldZ(progress);

    bool IsReserved(int lane, float z, float gap)
    {
        if (lane < 0 || lane >= reservedLaneZs.Length) return true;
        List<float> used = reservedLaneZs[lane];
        for (int i = 0; i < used.Count; i++)
        {
            if (Mathf.Abs(used[i] - z) < gap) return true;
        }
        return false;
    }

    void ReserveLane(int lane, float z)
    {
        if (lane < 0 || lane >= reservedLaneZs.Length) return;
        reservedLaneZs[lane].Add(z);
    }

    int FindFreeLane(int preferredLane, float z, float gap)
    {
        int start = Mathf.Clamp(preferredLane, 0, LevelLanes.Count - 1);
        for (int offset = 0; offset < LevelLanes.Count; offset++)
        {
            int lane = (start + offset) % LevelLanes.Count;
            if (!IsReserved(lane, z, gap)) return lane;
        }
        return start;
    }

    float FindFreeZ(int lane, float z, float gap)
    {
        float candidate = z;
        int guard = 0;
        while (IsReserved(lane, candidate, gap) && guard < 12)
        {
            candidate += Mathf.Max(4f, gap * 0.6f);
            guard++;
        }
        return candidate;
    }

    int FindFreeSpanStart(int preferredStartLane, int laneSpan, float z, float gap)
    {
        int maxStart = Mathf.Max(0, LevelLanes.Count - laneSpan);
        int start = Mathf.Clamp(preferredStartLane, 0, maxStart);
        for (int offset = 0; offset <= maxStart; offset++)
        {
            int laneStart = (start + offset) % (maxStart + 1);
            bool free = true;
            for (int i = 0; i < laneSpan; i++)
            {
                if (IsReserved(laneStart + i, z, gap))
                {
                    free = false;
                    break;
                }
            }
            if (free) return laneStart;
        }
        return start;
    }

    float FindFreeZAllLanes(float z, float gap)
    {
        float candidate = z;
        int guard = 0;
        while (guard < 14)
        {
            bool blocked = false;
            for (int lane = 0; lane < LevelLanes.Count; lane++)
            {
                if (IsReserved(lane, candidate, gap))
                {
                    blocked = true;
                    break;
                }
            }
            if (!blocked) return candidate;
            candidate += Mathf.Max(4f, gap * 0.6f);
            guard++;
        }
        return candidate;
    }

    void ReserveSpan(int startLane, int laneSpan, float z)
    {
        for (int i = 0; i < laneSpan; i++) ReserveLane(startLane + i, z);
    }

    void ReserveAllLanes(float z)
    {
        for (int lane = 0; lane < LevelLanes.Count; lane++) ReserveLane(lane, z);
    }

    void PlacePickup(int preferredLane, float progress, float gap, System.Action<int, float> spawn)
    {
        float z = Z(progress);
        int lane = FindFreeLane(preferredLane, z, gap);
        z = FindFreeZ(lane, z, gap);
        spawn(lane, z);
        ReserveLane(lane, z);
    }

    void WaterDrop(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumObjectSpacing, (freeLane, z) =>
            Level2Primitives.MakeWaterDroplet(root, freeLane, z));
    }

    void Baobab(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumHazardSpacing, (freeLane, z) =>
            Level2Primitives.MakeBaobab(root, freeLane, z));
    }

    void MaterialPickup(Transform root, int lane, float progress, Level2MaterialKind kind)
    {
        PlacePickup(lane, progress, Level2Config.MinimumObjectSpacing, (freeLane, z) =>
            Level2Primitives.MakeMaterial(root, freeLane, z, kind));
    }

    void Rock(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumHazardSpacing, (freeLane, z) =>
            Level2Primitives.MakeRock(root, freeLane, z));
    }

    void Rocks(Transform root, int[] lanes, float progress)
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            Rock(root, lanes[i], progress);
        }
    }

    void MudPuddle(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumHazardSpacing, (freeLane, z) =>
            Level2Primitives.MakeMudPuddle(root, freeLane, z));
    }

    void Poison(Transform root, int lane, float progress)
    {
        float z = Z(progress);
        int startLane = FindFreeSpanStart(lane, 2, z, Level2Config.MinimumHazardSpacing);
        z = FindFreeZ(startLane, z, Level2Config.MinimumHazardSpacing);
        Level2Primitives.MakePoisonPlant(root, startLane, z, progress);
        ReserveSpan(startLane, 2, z);
    }

    void Monster(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumHazardSpacing, (freeLane, z) =>
            Level2Primitives.MakeMudMonster(root, freeLane, z, progress));
    }

    void BubbleShield(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumObjectSpacing, (freeLane, z) =>
            Level2Primitives.MakeBubbleShieldPickup(root, freeLane, z));
    }

    void SpeedFruit(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumObjectSpacing, (freeLane, z) =>
            Level2Primitives.MakeSpeedFruit(root, freeLane, z));
    }

    void JumpBoost(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumObjectSpacing, (freeLane, z) =>
            Level2Primitives.MakeJumpBoost(root, freeLane, z));
    }

    void RollingLog(Transform root, int lane, float progress, int laneSpan = 2)
    {
        int span = Mathf.Clamp(laneSpan, 2, 3);
        float z = Z(progress);
        int startLane = FindFreeSpanStart(lane, span, z, Level2Config.MinimumHazardSpacing);
        z = FindFreeZAllLanes(z, Level2Config.MinimumHazardSpacing);
        float speed = progress >= 0.65f ? 15f : 12f;
        Level2Primitives.MakeRollingLog(root, startLane, z, span, speed);
        ReserveSpan(startLane, span, z);
    }

    void Cactus(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumHazardSpacing, (freeLane, z) =>
            Level2Primitives.MakeCactus(root, freeLane, z));
    }

    void HealthFruit(Transform root, int lane, float progress, float amount = 15f)
    {
        PlacePickup(lane, progress, Level2Config.MinimumObjectSpacing, (freeLane, z) =>
            Level2Primitives.MakeHealthFruit(root, freeLane, z, amount));
    }

    void WaterPool(Transform root, int lane, float progress)
    {
        PlacePickup(lane, progress, Level2Config.MinimumHazardSpacing, (freeLane, z) =>
            Level2Primitives.MakeWaterPool(root, freeLane, z));
    }

    void Warthog(Transform root, float progress, bool goRight, float speed = 38f)
    {
        float z = FindFreeZAllLanes(Z(progress), Level2Config.MinimumHazardSpacing);
        Level2Primitives.MakeWarthog(root, z, goRight, speed);
        ReserveAllLanes(z);
    }

    static void RefreshGroundTiles()
    {
        GroundSpawnner spawner = FindFirstObjectByType<GroundSpawnner>();
        if (spawner != null)
        {
            spawner.EnsureGroundLinked();
        }
    }

    static void EnsureFinishTrigger()
    {
        float endZ = FindMountainEndZ();
        if (endZ > Level2Progress.StartZ + 10f)
        {
            Level2Progress.EndZ = endZ;
        }

        GameObject ender = GameObject.Find("Ender2");
        if (ender == null)
        {
            ender = new GameObject("Ender2");
        }

        ender.SetActive(true);
        try
        {
            ender.tag = "EndLevel2";
        }
        catch
        {
            // Tag may already be assigned in the scene.
        }

        float pathCenter = (LevelLanes.X(0) + LevelLanes.X(LevelLanes.Count - 1)) * 0.5f;
        ender.transform.position = new Vector3(pathCenter, 4f, Level2Progress.EndZ);
        ender.transform.rotation = Quaternion.identity;
        ender.transform.localScale = Vector3.one;

        BoxCollider box = ender.GetComponent<BoxCollider>();
        if (box == null) box = ender.AddComponent<BoxCollider>();
        box.isTrigger = true;
        float width = Mathf.Abs(LevelLanes.X(0) - LevelLanes.X(LevelLanes.Count - 1)) + 16f;
        box.size = new Vector3(width, 18f, 20f);
        box.center = Vector3.zero;

        if (ender.GetComponent<Level2FinishGate>() == null)
        {
            ender.AddComponent<Level2FinishGate>();
        }
    }

    static float FindMountainEndZ()
    {
        float endZ = Level2Progress.EndZ;
        GameObject environment = GameObject.Find("Environment");
        if (environment == null) return endZ;

        float maxZ = endZ;
        Transform[] transforms = environment.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] == null) continue;
            maxZ = Mathf.Max(maxZ, transforms[i].position.z);
        }

        return maxZ + 8f;
    }

    static void DisableByName(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) go.SetActive(false);
    }

    static void DisableBehaviours<T>() where T : Behaviour
    {
        T[] found = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null)
            {
                found[i].enabled = false;
            }
        }
    }
}
