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
        if (sceneName != "MainGame") return;
        if (FindFirstObjectByType<Level1LayoutDirector>() != null) return;

        GameObject host = new GameObject("Level1LayoutDirector");
        host.AddComponent<Level1LayoutDirector>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainGame")
        {
            Destroy(this);
            return;
        }

        Transform player = FindPlayer();
        Level1Progress.BindFromScene(player);
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
        Debug.Log($"[Level1] Layout ready startZ={Level1Progress.StartZ:F1} endZ={Level1Progress.EndZ:F1}");
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

        Transform root = new GameObject(RootName).transform;
        int materialIndex = 0;

        // === 0–20% LEARN + PLAY ===

        // 0–5% Welcome — cactus teaches lane movement
        CactusWater(root, 1, 0.03f);
        CactusWater(root, 2, 0.045f);
        MaterialTool(root, 0, 0.048f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        CactusWater(root, 3, 0.06f);

        // 5–8% First lane dodge
        Rock(root, 0, 0.065f);
        CactusWater(root, 2, 0.072f);
        Rock(root, 2, 0.078f);
        CactusWater(root, 0, 0.085f);

        // 8–11% Sand pit → reward
        Sand(root, 1, 0.095f);
        CactusWater(root, 3, 0.105f);
        MaterialTool(root, 1, 0.108f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // 11–13% Rock cluster → reward
        RockClusterLanes(root, new[] { 0, 1 }, 0.115f);
        CactusWater(root, 2, 0.125f);

        // 13–15% Extra cactus reward
        CactusWater(root, 2, 0.135f);
        CactusWater(root, 3, 0.14f);
        CactusWater(root, 0, 0.145f);

        // 15–17% First horizontal cactus wall (L3 open)
        CactusWall(root, new[] { true, true, false, true }, 0.155f);
        CactusWater(root, 2, 0.165f);

        // 17–20% First rolling log lesson + safe run
        RollingLogLesson(root, 0.175f);
        CactusWater(root, 2, 0.188f);
        CactusWater(root, 1, 0.195f);
        MaterialTool(root, 3, 0.198f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // === 20–25% BUILD — Heat at 20%, warned snakes before 25% ===
        CactusWater(root, 0, 0.205f);
        CactusWater(root, 3, 0.212f);
        Snake(root, 2, 0.22f);
        CactusWater(root, 1, 0.228f);
        CactusWater(root, 0, 0.235f);
        Snake(root, 1, 0.245f);
        CactusWater(root, 3, 0.252f);
        CactusWater(root, 2, 0.26f);
        CactusWater(root, 1, 0.268f);
        CactusWater(root, 0, 0.276f);
        CactusWater(root, 3, 0.284f);
        CactusWater(root, 2, 0.292f);
        Sand(root, 3, 0.3f);
        MaterialTool(root, 0, 0.303f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        CactusWater(root, 1, 0.31f);
        RockClusterLanes(root, new[] { 2, 3 }, 0.318f);
        CactusWater(root, 0, 0.326f);
        CactusWater(root, 1, 0.334f);
        CactusWater(root, 2, 0.342f);
        CactusWall(root, new[] { false, true, true, false }, 0.35f);
        CactusWater(root, 0, 0.358f);
        Log(root, 2, 0.366f);
        CactusWater(root, 3, 0.374f);
        MaterialTool(root, 1, 0.382f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Snake(root, 2, 0.405f);
        CactusWater(root, 1, 0.415f);
        Rock(root, 0, 0.425f);

        // === 40–60% WEAVE ===
        Snake(root, 1, 0.435f);
        CactusWater(root, 3, 0.448f);
        CactusWater(root, 0, 0.465f);
        Snake(root, 0, 0.482f);
        CactusWater(root, 2, 0.498f);
        MaterialTool(root, 2, 0.512f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        CactusWall(root, new[] { true, false, true, false }, 0.525f);
        CactusWater(root, 1, 0.538f);
        Log(root, 1, 0.552f);
        CactusWater(root, 2, 0.565f);
        CactusWater(root, 0, 0.578f);
        CactusWater(root, 3, 0.592f);
        Snake(root, 1, 0.598f);
        RockClusterLanes(root, new[] { 2, 3 }, 0.612f);

        // === 60–70% RECOVER ===
        CactusWater(root, 1, 0.622f);
        CactusWater(root, 2, 0.632f);
        CactusWater(root, 0, 0.642f);
        CactusWater(root, 3, 0.652f);
        MaterialTool(root, 1, 0.662f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        CactusWater(root, 2, 0.672f);
        CactusWater(root, 1, 0.682f);
        CactusWater(root, 3, 0.692f);
        CactusWater(root, 0, 0.702f);

        // === 70–85% ESCALATE ===
        RockClusterLanes(root, new[] { 0, 1 }, 0.705f);
        Snake(root, 2, 0.715f);
        CactusWater(root, 3, 0.725f);
        Log(root, 0, 0.735f);
        MaterialTool(root, 2, 0.745f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        CactusWall(root, new[] { false, true, true, true }, 0.755f);
        CactusWater(root, 0, 0.765f);
        CactusWater(root, 3, 0.775f);
        Snake(root, 1, 0.785f);
        CactusWater(root, 2, 0.795f);
        Sand(root, 1, 0.805f);
        CactusWater(root, 1, 0.825f);
        Log(root, 2, 0.835f);
        MaterialTool(root, 0, 0.845f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // === 85–95% SURVIVE ===
        CactusWall(root, new[] { true, true, false, true }, 0.855f);
        Snake(root, 2, 0.865f);
        CactusWater(root, 1, 0.875f);
        CactusWater(root, 0, 0.885f);
        CactusWater(root, 3, 0.895f);
        RockClusterLanes(root, new[] { 0, 1 }, 0.915f);
        CactusWater(root, 2, 0.925f);
        MaterialTool(root, 3, 0.935f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        Log(root, 1, 0.945f);

        // === 95–100% FINAL CHALLENGE ===
        CactusWall(root, new[] { true, true, false, true }, 0.955f);
        Snake(root, 1, 0.962f);
        CactusWater(root, 2, 0.972f);
        CactusWater(root, 0, 0.978f);
        Log(root, 3, 0.984f);
        CactusWall(root, new[] { false, true, false, true }, 0.99f);
        CactusWater(root, 1, 0.994f);
        MaterialTool(root, 2, 0.997f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        // Materials + health for win condition
        MaterialTool(root, 3, 0.10f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 0, 0.18f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 2, 0.28f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 1, 0.38f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 3, 0.48f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 0, 0.52f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 2, 0.62f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 1, 0.72f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 3, 0.82f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 0, 0.92f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 2, 0.96f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);
        MaterialTool(root, 1, 0.99f, MaterialCycle[materialIndex++ % MaterialCycle.Length]);

        Health(root, 1, 0.50f);
        Health(root, 3, 0.78f);

        PlaceWaterPools(root);
    }

    static void PlaceWaterPools(Transform root)
    {
        int[] lanes = { 1, 3, 0, 2, 1, 3, 2, 0, 1, 3, 0, 2, 3, 1, 2, 0, 3, 1, 0, 2 };
        float[] progress =
        {
            0.04f, 0.09f, 0.14f, 0.19f, 0.24f, 0.29f, 0.34f, 0.39f, 0.44f, 0.49f,
            0.54f, 0.59f, 0.64f, 0.69f, 0.74f, 0.79f, 0.84f, 0.89f, 0.94f, 0.98f
        };

        for (int i = 0; i < lanes.Length; i++)
        {
            WaterPool(root, lanes[i], progress[i]);
        }
    }

    static void CactusWall(Transform root, bool[] blockedLanes, float progress)
    {
        Level1Primitives.MakeCactusWall(root, blockedLanes, Z(progress));
    }

    static void RockClusterLanes(Transform root, int[] lanes, float progress)
    {
        Level1Primitives.MakeRockClusterLanes(root, lanes, Z(progress));
    }

    static void RollingLogLesson(Transform root, float progress)
    {
        Level1Primitives.MakeRollingLogLesson(root, Z(progress));
    }

    static void CactusWater(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeCactusWater(root, lane, Z(progress));
    }

    static float Z(float progress) => Level1Progress.WorldZ(progress);

    static void WaterPool(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeWaterPool(root, lane, Z(progress));
    }

    static void MaterialTool(Transform root, int lane, float progress, Level1MaterialKind kind)
    {
        Level1Primitives.MakeMaterialTool(root, lane, Z(progress), kind);
    }

    static void Aloe(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeAloePlant(root, lane, Z(progress));
    }

    static void SuperFruit(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeSuperFruit(root, lane, Z(progress));
    }

    static void Log(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeLogBarrier(root, lane, Z(progress));
    }

    static void Health(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeHealthPickup(root, lane, Z(progress));
    }

    static void Cluster(Transform root, int[] lanes, float progress)
    {
        Level1Primitives.MakeCactusCluster(root, lanes, Z(progress));
    }

    static void Rock(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeRockCluster(root, lane, Z(progress));
    }

    static void Sand(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeSandPit(root, lane, Z(progress));
    }

    static void Barrier(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeLowCactusBarrier(root, lane, Z(progress));
    }

    static void Snake(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeSnake(root, lane, Z(progress), progress);
    }
}
