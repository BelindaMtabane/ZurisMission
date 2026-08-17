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
    const string SceneName = "Level2";

    static readonly Level2MaterialKind[] MaterialCycle =
    {
        Level2MaterialKind.Pipe,
        Level2MaterialKind.Nails,
        Level2MaterialKind.Hammer
    };

    [SerializeField] float mudSlowMultiplier = Level2MudSlowEffect.DefaultMultiplier;
    [SerializeField] float mudSlowDuration = Level2MudSlowEffect.DefaultDuration;

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
        if (sceneName != SceneName) return;
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
        if (SceneManager.GetActiveScene().name != SceneName)
        {
            Destroy(this);
            return;
        }

        Transform player = FindPlayer();
        Level2Progress.BindFromScene(player);
        RunnerLevelPacing.Apply(SceneName);
        ConfigureLanes(player);
        ClearExistingGameplay(player);
        try
        {
            BuildLayout();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        RefreshGroundTiles();

        Debug.Log("[Level2] Mudlands layout built.");
    }

    static void ConfigureLanes(Transform player)
    {
        LevelLanes.ConfigureForActiveScene();
        AlignLaneMarkers();
        SnapPlayerToNearestLane(player);

        Debug.Log($"[Level2] Lanes centered on path x={LevelLanes.PathCenterX:F2} " +
                  $"[{LevelLanes.X(0):F1}, {LevelLanes.X(1):F1}, {LevelLanes.X(2):F1}, {LevelLanes.X(3):F1}]");
    }

    static void AlignLaneMarkers()
    {
        string[] markerNames = { "LaneSpawn1", "LaneSpawn2", "LaneSpawn3", "LaneSpawn4" };
        for (int i = 0; i < markerNames.Length; i++)
        {
            GameObject marker = GameObject.Find(markerNames[i]);
            if (marker == null) continue;

            Vector3 pos = marker.transform.position;
            pos.x = LevelLanes.X(i);
            marker.transform.position = pos;
            marker.SetActive(true);
        }

        Lanemanager2 laneManager = FindFirstObjectByType<Lanemanager2>();
        if (laneManager == null || laneManager.laneSpawnsPositions == null) return;

        for (int i = 0; i < laneManager.laneSpawnsPositions.Length && i < LevelLanes.Count; i++)
        {
            if (laneManager.laneSpawnsPositions[i] == null) continue;

            Vector3 pos = laneManager.laneSpawnsPositions[i].position;
            pos.x = LevelLanes.X(i);
            laneManager.laneSpawnsPositions[i].position = pos;
        }
    }

    static void SnapPlayerToNearestLane(Transform player)
    {
        if (player == null) return;

        Vector3 pos = player.position;
        pos.x = LevelLanes.X(LevelLanes.Count / 2);
        player.position = pos;
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

        // === 0–5% OPENING: droplet, tall baobab, material ===
        WaterDrop(root, 2, 0.018f);
        Baobab(root, 1, 0.032f);
        MaterialPickup(root, 3, 0.042f, NextMaterial(ref materialIndex));

        // === 5–8% ROCKS ===
        Rock(root, 0, 0.055f);
        WaterDrop(root, 2, 0.065f);
        Rock(root, 3, 0.074f);
        MaterialPickup(root, 1, 0.078f, NextMaterial(ref materialIndex));

        // === 8–11% MUD PUDDLE ===
        MudPuddle(root, 1, 0.088f);
        WaterDrop(root, 3, 0.098f);
        MaterialPickup(root, 2, 0.105f, NextMaterial(ref materialIndex));

        // === 11–14% POISON PLANT ===
        Poison(root, 0, 0.118f);
        WaterDrop(root, 2, 0.128f);
        Baobab(root, 3, 0.136f);

        // === 14–17% FIRST MUD MONSTER ===
        Monster(root, 2, 0.148f);
        MaterialPickup(root, 1, 0.158f, NextMaterial(ref materialIndex));
        WaterDrop(root, 3, 0.165f);

        // === 17–20% BUBBLE THEN MONSTER ===
        BubbleShield(root, 2, 0.175f);
        Monster(root, 2, 0.188f);
        WaterDrop(root, 1, 0.196f);

        // === 20–35% MAIN GAMEPLAY (two hazards at a time) ===
        Rocks(root, new[] { 0, 3 }, 0.208f);
        MudPuddle(root, 1, 0.218f);
        WaterDrop(root, 2, 0.226f);
        Poison(root, 3, 0.235f);
        MaterialPickup(root, 0, 0.242f, NextMaterial(ref materialIndex));
        Monster(root, 1, 0.252f);
        Rocks(root, new[] { 1, 2 }, 0.262f);
        WaterDrop(root, 0, 0.270f);
        MudPuddle(root, 3, 0.278f);
        Baobab(root, 2, 0.286f);
        Poison(root, 1, 0.295f);
        MaterialPickup(root, 3, 0.302f, NextMaterial(ref materialIndex));
        Monster(root, 0, 0.312f);
        Rocks(root, new[] { 2, 3 }, 0.322f);
        WaterDrop(root, 1, 0.330f);
        MudPuddle(root, 0, 0.338f);
        MaterialPickup(root, 2, 0.345f, NextMaterial(ref materialIndex));

        // === 35–50% SPEED FRUIT + JUMP BOOST RISK ROUTES ===
        SpeedFruit(root, 1, 0.358f);
        Poison(root, 2, 0.358f);
        WaterDrop(root, 0, 0.368f);
        JumpBoost(root, 3, 0.378f);
        Rock(root, 1, 0.378f);
        Monster(root, 0, 0.390f);
        MaterialPickup(root, 2, 0.398f, NextMaterial(ref materialIndex));
        SpeedFruit(root, 2, 0.410f);
        Poison(root, 3, 0.410f);
        MudPuddle(root, 1, 0.420f);
        JumpBoost(root, 0, 0.430f);
        Rocks(root, new[] { 1, 2 }, 0.430f);
        Baobab(root, 3, 0.440f);
        Monster(root, 2, 0.450f);
        WaterDrop(root, 1, 0.458f);
        SpeedFruit(root, 0, 0.468f);
        Poison(root, 1, 0.468f);
        MaterialPickup(root, 3, 0.476f, NextMaterial(ref materialIndex));
        JumpBoost(root, 2, 0.486f);
        MudPuddle(root, 0, 0.492f);

        // === 50–65% FIRST MAJOR COMBINATION ===
        MudPuddle(root, 1, 0.508f);
        Monster(root, 2, 0.516f);
        Rock(root, 0, 0.524f);
        BubbleShield(root, 3, 0.534f);
        WaterDrop(root, 2, 0.542f);
        MudPuddle(root, 0, 0.552f);
        Monster(root, 3, 0.560f);
        Rocks(root, new[] { 1, 2 }, 0.568f);
        MaterialPickup(root, 0, 0.576f, NextMaterial(ref materialIndex));
        Poison(root, 2, 0.586f);
        SpeedFruit(root, 1, 0.586f);
        Monster(root, 0, 0.598f);
        MudPuddle(root, 3, 0.606f);
        JumpBoost(root, 2, 0.614f);
        Rock(root, 1, 0.622f);
        BubbleShield(root, 0, 0.632f);
        WaterDrop(root, 3, 0.640f);
        Baobab(root, 1, 0.648f);

        // === 65–75% RECOVERY / FUN ===
        WaterDrop(root, 2, 0.658f);
        Baobab(root, 0, 0.666f);
        SpeedFruit(root, 3, 0.674f);
        JumpBoost(root, 1, 0.682f);
        MaterialPickup(root, 2, 0.690f, NextMaterial(ref materialIndex));
        WaterDrop(root, 0, 0.698f);
        BubbleShield(root, 2, 0.706f);
        Baobab(root, 3, 0.714f);
        MaterialPickup(root, 1, 0.722f, NextMaterial(ref materialIndex));
        WaterDrop(root, 2, 0.730f);
        SpeedFruit(root, 0, 0.738f);
        JumpBoost(root, 3, 0.746f);

        // === 75–90% DIFFICULTY INCREASE ===
        Poison(root, 0, 0.758f);
        MudPuddle(root, 2, 0.766f);
        Monster(root, 3, 0.774f);
        Rock(root, 1, 0.782f);
        MaterialPickup(root, 2, 0.790f, NextMaterial(ref materialIndex));
        Poison(root, 3, 0.800f);
        MudPuddle(root, 1, 0.808f);
        Monster(root, 0, 0.816f);
        Rocks(root, new[] { 1, 2 }, 0.824f);
        SpeedFruit(root, 3, 0.832f);
        JumpBoost(root, 0, 0.840f);
        Poison(root, 2, 0.848f);
        MudPuddle(root, 0, 0.856f);
        Monster(root, 1, 0.864f);
        Rock(root, 3, 0.872f);
        BubbleShield(root, 2, 0.880f);
        WaterDrop(root, 1, 0.888f);
        MaterialPickup(root, 0, 0.894f, NextMaterial(ref materialIndex));

        // === 90–100% FINAL CHALLENGE ===
        Poison(root, 0, 0.905f);
        MudPuddle(root, 2, 0.912f);
        Monster(root, 3, 0.920f);
        SpeedFruit(root, 1, 0.928f);
        Rock(root, 0, 0.936f);
        JumpBoost(root, 2, 0.944f);
        WaterDrop(root, 3, 0.952f);
        Baobab(root, 1, 0.960f);
        MaterialPickup(root, 2, 0.968f, NextMaterial(ref materialIndex));
        WaterDrop(root, 0, 0.976f);
        MaterialPickup(root, 3, 0.984f, NextMaterial(ref materialIndex));
        WaterDrop(root, 2, 0.992f);

        MaterialPickup(root, 0, 0.102f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 3, 0.21f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 0, 0.33f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 2, 0.48f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 1, 0.62f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 3, 0.72f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 0, 0.82f, NextMaterial(ref materialIndex));
        MaterialPickup(root, 2, 0.94f, NextMaterial(ref materialIndex));
    }

    static Level2MaterialKind NextMaterial(ref int materialIndex)
    {
        Level2MaterialKind kind = MaterialCycle[materialIndex % MaterialCycle.Length];
        materialIndex++;
        return kind;
    }

    static float Z(float progress) => Level2Progress.WorldZ(progress);

    static void WaterDrop(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeWaterDroplet(root, lane, Z(progress));
    }

    static void Baobab(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeBaobab(root, lane, Z(progress));
    }

    static void MaterialPickup(Transform root, int lane, float progress, Level2MaterialKind kind)
    {
        Level2Primitives.MakeMaterial(root, lane, Z(progress), kind);
    }

    static void Rock(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeRock(root, lane, Z(progress));
    }

    static void Rocks(Transform root, int[] lanes, float progress)
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            Level2Primitives.MakeRock(root, lanes[i], Z(progress));
        }
    }

    static void MudPuddle(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeMudPuddle(root, lane, Z(progress));
    }

    static void Poison(Transform root, int lane, float progress)
    {
        Level2Primitives.MakePoisonPlant(root, lane, Z(progress), progress);
    }

    static void Monster(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeMudMonster(root, lane, Z(progress), progress);
    }

    static void BubbleShield(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeBubbleShieldPickup(root, lane, Z(progress));
    }

    static void SpeedFruit(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeSpeedFruit(root, lane, Z(progress));
    }

    static void JumpBoost(Transform root, int lane, float progress)
    {
        Level2Primitives.MakeJumpBoost(root, lane, Z(progress));
    }

    static void RefreshGroundTiles()
    {
        GroundSpawnner spawner = FindFirstObjectByType<GroundSpawnner>();
        if (spawner != null)
        {
            spawner.EnsureGroundLinked();
        }
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
