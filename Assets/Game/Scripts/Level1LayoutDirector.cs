using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds Level 1 pickups, obstacles, and snakes in MainGame. Does not touch Level2/Level3.
/// </summary>
[DefaultExecutionOrder(-40)]
public class Level1LayoutDirector : MonoBehaviour
{
    const string RootName = "Level1_Layout";

    static readonly Level1MaterialKind[] MaterialCycle =
    {
        Level1MaterialKind.Hammer,
        Level1MaterialKind.Brick,
        Level1MaterialKind.CementBag
    };

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
        if (sceneName != SceneCatalog.MainGame) return;
        if (FindFirstObjectByType<Level1LayoutDirector>() != null) return;

        GameObject host = new GameObject("Level1LayoutDirector");
        host.AddComponent<Level1LayoutDirector>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.MainGame)
        {
            Destroy(this);
            return;
        }

        Transform player = FindPlayer();
        Level1Progress.BindFromScene(player);
        RunnerPlayerSetup.Apply(SceneCatalog.MainGame, player);
        Level1Pacing.Apply();
        ClearExistingGameplay(player);
        try
        {
            BuildLayout();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        EnsureHeatWave(player);
        EnsureLowWaterMonitor();
        Debug.Log($"[Level1] Layout ready startZ={Level1Progress.StartZ:F1} endZ={Level1Progress.EndZ:F1}");
    }

    void EnsureLowWaterMonitor()
    {
        if (FindFirstObjectByType<Level1LowWaterMonitor>() == null)
        {
            gameObject.AddComponent<Level1LowWaterMonitor>();
        }
    }

    void EnsureHeatWave(Transform player)
    {
        Level1HeatWave heat = FindFirstObjectByType<Level1HeatWave>();
        if (heat == null)
        {
            heat = gameObject.AddComponent<Level1HeatWave>();
        }

        heat.BindProgress(player);
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
            "SpeedBoast", "Obstacle", "Heat&Disease", "AnimalAttack", "SlowDown"
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
                if (go.CompareTag("Player") || go.CompareTag("EndLevel1")) continue;
                if (go.layer == 5) continue;
                if (player != null && go.transform.IsChildOf(player)) continue;
                go.SetActive(false);
            }
        }

        DisableByName("DryBushlands_Layout");
        DisableByName("Snakes");
        DisableByName("Snakes (1)");
        DisableByName("HeatWaveDirector");
        DisableByName("SnakePassDirector");
        DisableByName("HeatWaveFlash");
        DisableByName("CarLeft");

        GameObject[] all = FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            GameObject go = all[i];
            if (go == null) continue;
            if (player != null && go.transform.IsChildOf(player)) continue;

            string n = go.name;
            if (n.StartsWith("Pick") || n.StartsWith("obstacle") || n.StartsWith("Obstacle"))
            {
                go.SetActive(false);
                continue;
            }

            if (n.StartsWith("Cactus_"))
            {
                Collider[] cols = go.GetComponentsInChildren<Collider>(true);
                for (int c = 0; c < cols.Length; c++)
                {
                    if (cols[c] != null) cols[c].enabled = false;
                }
            }
        }

        DisableBehaviours<HeatWaveDirector>();
        DisableBehaviours<SnakePassDirector>();
        DisableBehaviours<Level1DryBushlandsBuilder>();
        DisableBehaviours<Level1DustDevilSpin>();
        DisableBehaviours<BushlandHazard>();
        DisableBehaviours<SnakePassHazard>();
        DisableBehaviours<PickupCollectable>();
        DisableBehaviours<MovingObstacle>();
        DisableBehaviours<ObstacleRunner>();
        DisableBehaviours<Spawner>();
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
            if (found[i] != null) found[i].enabled = false;
        }
    }

    void BuildLayout()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            Destroy(existing);
        }

        Level1LayoutPlacement.Reset();

        Transform root = new GameObject(RootName).transform;
        int materialIndex = 0;

        // ── LEARN (0–22%): one object per lane slot ──
        MaterialTool(root, 0, 0.03f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        CactusWater(root, 3, 0.06f);
        MaterialTool(root, 2, 0.08f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Rock(root, 3, 0.10f);
        MaterialTool(root, 1, 0.125f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        CactusWater(root, 0, 0.14f);
        Sand(root, 2, 0.16f);
        MaterialTool(root, 3, 0.175f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Rock(root, 1, 0.20f);
        Health(root, 2, 0.21f);
        CactusWater(root, 0, 0.225f);
        MaterialTool(root, 1, 0.075f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // ── PRACTICE (23–50%): danger then recovery on separate lane slots ──
        CactusWater(root, 1, 0.265f);
        MaterialTool(root, 2, 0.285f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Snake(root, 3, 0.305f);
        CactusWater(root, 0, 0.325f);
        MaterialTool(root, 1, 0.345f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Sand(root, 2, 0.365f);
        Snake(root, 1, 0.395f);
        CactusWater(root, 2, 0.415f);
        Rock(root, 0, 0.435f);
        MaterialTool(root, 3, 0.455f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Snake(root, 3, 0.475f);
        CactusWall(root, new[] { true, false, true, false }, 0.495f);
        MaterialTool(root, 1, 0.515f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 0, 0.405f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // ── COMBINE (51–72%): tutorial log reserves lanes 1–2 at ~53%; keep other spawns clear ──
        RollingLogLesson(root, 0.510f);
        CactusWater(root, 0, 0.565f);
        Snake(root, 3, 0.585f);
        RollingLog(root, 2, 0.610f, 2, 13f);
        CactusWater(root, 1, 0.640f);
        MaterialTool(root, 0, 0.665f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        RockClusterLanes(root, new[] { 0, 3 }, 0.690f);
        Snake(root, 2, 0.715f);
        CactusWater(root, 3, 0.740f);
        Sand(root, 1, 0.760f);
        Health(root, 0, 0.775f);
        MaterialTool(root, 2, 0.575f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // ── RECOVERY (73–76%) ──
        CactusWater(root, 2, 0.795f);
        MaterialTool(root, 3, 0.810f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // ── CHALLENGE (77–88%) ──
        Snake(root, 1, 0.830f);
        RollingLog(root, 3, 0.855f, 2, 14f);
        CactusWater(root, 0, 0.880f);
        Rock(root, 2, 0.905f);
        Snake(root, 3, 0.930f);
        MaterialTool(root, 1, 0.805f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // ── FINISH (89–100%) ──
        CactusWater(root, 1, 0.950f);
        MaterialTool(root, 2, 0.970f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Health(root, 3, 0.960f);
        RollingLog(root, 0, 0.985f, 2, 15f);
        MaterialTool(root, 0, 0.995f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        PlaceWaterPools(root);
        EnsureFinishTrigger();
    }

    static void EnsureFinishTrigger()
    {
        GameObject ender = GameObject.Find("Ender1");
        if (ender == null)
        {
            ender = new GameObject("Ender1");
        }

        ender.SetActive(true);
        try
        {
            ender.tag = "EndLevel1";
        }
        catch
        {
            // Tag may already exist in the project.
        }

        float pathCenter = (LevelLanes.X(0) + LevelLanes.X(LevelLanes.Count - 1)) * 0.5f;
        ender.transform.position = new Vector3(pathCenter, 4f, Level1Progress.EndZ);
        ender.transform.rotation = Quaternion.identity;
        ender.transform.localScale = Vector3.one;

        BoxCollider box = ender.GetComponent<BoxCollider>();
        if (box == null) box = ender.AddComponent<BoxCollider>();
        box.isTrigger = true;
        float width = Mathf.Abs(LevelLanes.X(0) - LevelLanes.X(LevelLanes.Count - 1)) + 16f;
        box.size = new Vector3(width, 18f, 20f);
        box.center = Vector3.zero;

        if (ender.GetComponent<Level1FinishGate>() == null)
        {
            ender.AddComponent<Level1FinishGate>();
        }
    }

    static void PlaceWaterPools(Transform root)
    {
        int[] lanes = { 1, 0, 2, 3, 3, 0, 1, 2, 0, 1, 3, 2, 0, 1 };
        float[] progress =
        {
            0.04f, 0.102f, 0.188f, 0.240f,
            0.270f, 0.358f, 0.440f, 0.508f,
            0.600f, 0.670f, 0.765f, 0.820f,
            0.903f, 0.925f
        };

        for (int i = 0; i < lanes.Length && i < progress.Length; i++)
        {
            WaterPool(root, lanes[i], progress[i]);
        }
    }

    static int[] BlockedLanes(bool[] blockedLanes)
    {
        if (blockedLanes == null) return System.Array.Empty<int>();

        int count = 0;
        for (int lane = 0; lane < LevelLanes.Count && lane < blockedLanes.Length; lane++)
        {
            if (blockedLanes[lane]) count++;
        }

        if (count == 0) return System.Array.Empty<int>();

        int[] lanes = new int[count];
        int index = 0;
        for (int lane = 0; lane < LevelLanes.Count && lane < blockedLanes.Length; lane++)
        {
            if (!blockedLanes[lane]) continue;
            lanes[index++] = lane;
        }

        return lanes;
    }

    static void CactusWall(Transform root, bool[] blockedLanes, float progress)
    {
        int[] lanes = BlockedLanes(blockedLanes);
        if (!Level1LayoutPlacement.TryReserveLanes(lanes, progress, out float usedProgress)) return;
        Level1Primitives.MakeCactusWall(root, blockedLanes, Z(usedProgress));
    }

    static void RockClusterLanes(Transform root, int[] lanes, float progress)
    {
        if (!Level1LayoutPlacement.TryReserveLanes(lanes, progress, out float usedProgress)) return;
        Level1Primitives.MakeRockClusterLanes(root, lanes, Z(usedProgress));
    }

    static void RollingLogLesson(Transform root, float progress)
    {
        float lessonZ = Z(progress);
        float spawnProgress = Level1LayoutPlacement.ProgressFromWorldZ(lessonZ + 18f);
        Level1LayoutPlacement.TryReserveLanes(new[] { 1, 2 }, spawnProgress, out _);
        Level1Primitives.MakeRollingLogLesson(root, lessonZ);
    }

    static void CactusWater(Transform root, int lane, float progress)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeCactusWater(root, usedLane, Z(usedProgress));
    }

    static float Z(float progress) => Level1Progress.WorldZ(progress);

    static void WaterPool(Transform root, int lane, float progress)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeWaterPool(root, usedLane, Z(usedProgress));
    }

    static void MaterialTool(Transform root, int lane, float progress, Level1MaterialKind kind)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeMaterialTool(root, usedLane, Z(usedProgress), kind);
    }

    static void Aloe(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeAloePlant(root, lane, Z(progress));
    }

    static void SuperFruit(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeSuperFruit(root, lane, Z(progress));
    }

    static void RollingLog(Transform root, int lane, float progress, int laneSpan = 2, float speed = 13f)
    {
        int[] lanes = Level1LayoutPlacement.GetRollingLogLanes(lane, laneSpan);
        if (!Level1LayoutPlacement.TryReserveLanes(lanes, progress, out float usedProgress)) return;
        Level1Primitives.MakeRollingLog(root, lane, Z(usedProgress), laneSpan, speed);
    }

    static void Log(Transform root, int lane, float progress)
    {
        RollingLog(root, lane, progress);
    }

    static void Health(Transform root, int lane, float progress)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeHealthPickup(root, usedLane, Z(usedProgress));
    }

    static void Cluster(Transform root, int[] lanes, float progress)
    {
        Level1Primitives.MakeCactusCluster(root, lanes, Z(progress));
    }

    static void Rock(Transform root, int lane, float progress)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeRockCluster(root, usedLane, Z(usedProgress));
    }

    static void Sand(Transform root, int lane, float progress)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeSandPit(root, usedLane, Z(usedProgress));
    }

    static void BlackPit(Transform root, int lane, float progress)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeBlackPit(root, usedLane, Z(usedProgress));
    }

    static void Barrier(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeLowCactusBarrier(root, lane, Z(progress));
    }

    static void Snake(Transform root, int lane, float progress)
    {
        if (!Level1LayoutPlacement.TryReserve(lane, progress, out float usedProgress, out int usedLane)) return;
        Level1Primitives.MakeSnake(root, usedLane, Z(usedProgress), usedProgress);
    }
}
