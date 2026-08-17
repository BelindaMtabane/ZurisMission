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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != "MainGame") return;
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
        ClearExistingGameplay(player);
        BuildLayout();
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

        // 15 water pools
        int[] poolLanes = { 1, 3, 1, 2, 0, 3, 1, 2, 1, 3, 0, 3, 1, 1, 3 };
        float[] poolProgress = { 0.05f, 0.09f, 0.13f, 0.18f, 0.24f, 0.30f, 0.38f, 0.45f, 0.52f, 0.58f, 0.64f, 0.71f, 0.78f, 0.86f, 0.94f };
        for (int i = 0; i < poolLanes.Length; i++)
        {
            WaterPool(root, poolLanes[i], poolProgress[i]);
        }

        // 20 material pickups (hammer / brick / cement)
        int[] matLanes = { 0, 1, 1, 2, 3, 3, 0, 1, 3, 1, 2, 3, 0, 1, 2, 3, 1, 3, 0, 2 };
        float[] matProgress = { 0.06f, 0.11f, 0.16f, 0.21f, 0.26f, 0.31f, 0.36f, 0.41f, 0.46f, 0.51f, 0.56f, 0.61f, 0.66f, 0.70f, 0.74f, 0.78f, 0.82f, 0.86f, 0.90f, 0.96f };
        for (int i = 0; i < matLanes.Length; i++)
        {
            MaterialTool(root, matLanes[i], matProgress[i], MaterialCycle[materialIndex % MaterialCycle.Length]);
            materialIndex++;
        }

        // 5 aloe plants
        Aloe(root, 1, 0.35f);
        Aloe(root, 3, 0.48f);
        Aloe(root, 2, 0.62f);
        Aloe(root, 3, 0.75f);
        Aloe(root, 0, 0.88f);

        // Super fruits + jumpable logs + hazards through progression
        SuperFruit(root, 3, 0.08f);
        Log(root, 3, 0.12f);
        SuperFruit(root, 1, 0.19f);
        DustDevil(root, 2, 0.23f);

        Rock(root, 0, 0.27f);
        Log(root, 3, 0.29f);
        Snake(root, 2, 0.32f);
        SuperFruit(root, 3, 0.34f);
        Cluster(root, new[] { 1 }, 0.36f);
        DustDevil(root, 3, 0.39f);
        Snake(root, 1, 0.42f);

        Sand(root, 2, 0.47f);
        Health(root, 3, 0.49f);
        Snake(root, 3, 0.53f);
        Log(root, 1, 0.55f);
        Cluster(root, new[] { 2, 3 }, 0.57f);
        SuperFruit(root, 0, 0.60f);
        DustDevil(root, 3, 0.63f);

        Health(root, 2, 0.65f);
        Log(root, 3, 0.67f);
        SuperFruit(root, 3, 0.69f);

        Snake(root, 1, 0.72f);
        Barrier(root, 3, 0.74f);
        DustDevil(root, 0, 0.76f);
        Snake(root, 3, 0.78f);
        Log(root, 2, 0.80f);
        Log(root, 3, 0.815f);
        Snake(root, 3, 0.82f);
        SuperFruit(root, 2, 0.84f);
        Cluster(root, new[] { 0 }, 0.85f);
        Snake(root, 2, 0.87f);

        // End challenge: heat waves tighten + snakes + extra water
        WaterPool(root, 1, 0.80f);
        WaterPool(root, 2, 0.835f);
        Snake(root, 0, 0.855f);
        Snake(root, 3, 0.875f);
        WaterPool(root, 3, 0.905f);
        Snake(root, 1, 0.925f);
        Snake(root, 2, 0.945f);
        WaterPool(root, 0, 0.965f);

        Sand(root, 0, 0.89f);
        DustDevil(root, 3, 0.91f);
        Rock(root, 1, 0.93f);
        Log(root, 3, 0.935f);
        Barrier(root, 3, 0.94f);
        SuperFruit(root, 3, 0.95f);
        Cluster(root, new[] { 2 }, 0.96f);
        Log(root, 0, 0.965f);
        Snake(root, 3, 0.97f);
        DustDevil(root, 1, 0.98f);
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

    static void DustDevil(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeDustDevil(root, lane, Z(progress));
    }

    static void Barrier(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeLowCactusBarrier(root, lane, Z(progress));
    }

    static void Snake(Transform root, int lane, float progress)
    {
        Level1Primitives.MakeSnake(root, lane, Z(progress));
    }
}
