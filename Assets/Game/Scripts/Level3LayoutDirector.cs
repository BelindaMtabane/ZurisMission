using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-40)]
public class Level3LayoutDirector : MonoBehaviour
{
    const string RootName = "Level3_Layout";
    const string SceneName = "Level3";

    static readonly string[] MaterialCycle = { "Pipe", "Nails", "Hammer" };

    [SerializeField] float mudSlowMultiplier = Level3MudSlowEffect.DefaultMultiplier;
    [SerializeField] float mudSlowDuration = Level3MudSlowEffect.DefaultDuration;
    [SerializeField] float snakeSlow = 4f;
    [SerializeField] float snakeMedium = 6f;
    [SerializeField] float snakeFast = 8f;
    [SerializeField] float warthogSlow = 5f;
    [SerializeField] float warthogMedium = 7f;
    [SerializeField] float warthogFast = 9f;
    [SerializeField] int tank1Cost = 5;
    [SerializeField] int tank2Cost = 10;
    [SerializeField] int tank3Cost = 2;

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
        if (FindFirstObjectByType<Level3LayoutDirector>() != null) return;
        new GameObject("Level3LayoutDirector").AddComponent<Level3LayoutDirector>();
    }

    void Awake()
    {
        Level3MudSlowEffect.Multiplier = mudSlowMultiplier;
        Level3MudSlowEffect.Duration = mudSlowDuration;
        Level3EnemySpeeds.SnakeSlow = snakeSlow;
        Level3EnemySpeeds.SnakeMedium = snakeMedium;
        Level3EnemySpeeds.SnakeFast = snakeFast;
        Level3EnemySpeeds.WarthogSlow = warthogSlow;
        Level3EnemySpeeds.WarthogMedium = warthogMedium;
        Level3EnemySpeeds.WarthogFast = warthogFast;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneName)
        {
            Destroy(this);
            return;
        }

        Transform player = FindPlayer();
        Level3Progress.BindFromScene(player);
        RunnerLevelPacing.Apply(SceneName);
        LevelLanes.ConfigureForActiveScene();
        if (player != null)
        {
            Vector3 pos = player.position;
            pos.x = LevelLanes.X(1);
            player.position = pos;
        }

        ClearExistingGameplay(player);
        try
        {
            BuildLayout();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }

        Debug.Log("[Level3] Final level layout built.");
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
            "SpeedBoast", "Obstacle", "Heat&Disease", "AnimalAttack", "SlowDown",
            "PipeFix1", "PipeFix2", "PipeFix3", "PipeFix"
        };

        for (int t = 0; t < tags.Length; t++)
        {
            GameObject[] found;
            try { found = GameObject.FindGameObjectsWithTag(tags[t]); }
            catch { continue; }

            for (int i = 0; i < found.Length; i++)
            {
                GameObject go = found[i];
                if (go == null) continue;
                if (go.CompareTag("Player") || go.CompareTag("EndLvl3End")) continue;
                if (go.layer == 5) continue;
                if (player != null && go.transform.IsChildOf(player)) continue;
                go.SetActive(false);
            }
        }

        string[] named =
        {
            "PipeFix", "Tanklvl1", "Tanklvl2", "Tanklvl3",
            "HeatWaveDirector", "SnakePassDirector", "Snakes"
        };
        for (int i = 0; i < named.Length; i++)
        {
            GameObject go = GameObject.Find(named[i]);
            if (go != null) go.SetActive(false);
        }

        DisableBehaviours<PipeControlslevel3>();
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

        Transform root = new GameObject(RootName).transform;
        Level3PipeRepair repair = gameObject.GetComponent<Level3PipeRepair>();
        if (repair == null) repair = gameObject.AddComponent<Level3PipeRepair>();
        repair.ApplyCosts(tank1Cost, tank2Cost, tank3Cost);

        int m = 0;

        // 0–15% intro
        Bucket(root, 2, 0.02f);
        Mat(root, 1, 0.035f, ref m);
        Health(root, 3, 0.05f);
        Rock(root, 0, 0.06f);
        Mat(root, 2, 0.08f, ref m);
        Rock(root, 3, 0.10f);
        Bucket(root, 1, 0.12f);
        Mat(root, 0, 0.14f, ref m);

        // 15–30% mud puddles
        Mud(root, 1, 0.16f);
        Mat(root, 3, 0.175f, ref m);
        Mud(root, 0, 0.19f);
        Rock(root, 2, 0.205f);
        Mud(root, 3, 0.22f);
        Bucket(root, 2, 0.235f);
        Mud(root, 1, 0.25f);
        Rock(root, 0, 0.265f);
        Mat(root, 2, 0.28f, ref m);
        Health(root, 1, 0.29f);

        // 30–45% tank 1
        Mat(root, 0, 0.31f, ref m);
        Mat(root, 3, 0.33f, ref m);
        Mud(root, 2, 0.345f);
        Rock(root, 1, 0.36f);
        Bucket(root, 0, 0.375f);
        Tank(root, repair, 0, 2, 0.40f);
        Mat(root, 1, 0.42f, ref m);
        Health(root, 3, 0.435f);

        // 45–60% acid rain + leaf
        Leaf(root, 2, 0.46f);
        Acid(root, 0.48f, new[] { 0, 1 });
        Mat(root, 3, 0.50f, ref m);
        Acid(root, 0.52f, new[] { 2, 3 });
        Leaf(root, 1, 0.535f);
        Mud(root, 0, 0.55f);
        Acid(root, 0.57f, new[] { 1, 2 });
        Bucket(root, 3, 0.585f);
        Mat(root, 2, 0.595f, ref m);

        // 60–75% snakes
        Snake(root, 0.62f, Level3EnemyPace.Slow, true);
        Mat(root, 1, 0.635f, ref m);
        Snake(root, 0.65f, Level3EnemyPace.Medium, false);
        Mud(root, 2, 0.665f);
        Snake(root, 0.68f, Level3EnemyPace.Fast, true);
        Leaf(root, 0, 0.695f);
        Rock(root, 3, 0.71f);
        Snake(root, 0.73f, Level3EnemyPace.Medium, false);
        Bucket(root, 2, 0.74f);

        // 75–85% warthogs + mud
        Warthog(root, 0.755f, Level3EnemyPace.Slow, true);
        Mud(root, 1, 0.765f);
        Warthog(root, 0.78f, Level3EnemyPace.Medium, false);
        Snake(root, 0.792f, Level3EnemyPace.Fast, true);
        Mud(root, 0, 0.805f);
        Warthog(root, 0.82f, Level3EnemyPace.Fast, false);
        Mat(root, 2, 0.83f, ref m);
        Health(root, 3, 0.84f);

        // 85–92% tank 2
        Acid(root, 0.855f, new[] { 0, 3 });
        Snake(root, 0.865f, Level3EnemyPace.Medium, true);
        Mat(root, 1, 0.875f, ref m);
        Warthog(root, 0.885f, Level3EnemyPace.Medium, false);
        Tank(root, repair, 1, 2, 0.90f);
        Leaf(root, 0, 0.912f);

        // 92–96% recovery
        Bucket(root, 2, 0.925f);
        Mat(root, 1, 0.932f, ref m);
        Health(root, 3, 0.94f);
        Leaf(root, 0, 0.948f);
        Mat(root, 2, 0.955f, ref m);

        // 96–98% tank 3
        Tank(root, repair, 2, 1, 0.968f);
        Bucket(root, 3, 0.975f);

        // 98–100% boss
        Level3BossDirector boss = new GameObject("Level3Boss").AddComponent<Level3BossDirector>();
        boss.transform.SetParent(root, false);
        boss.Setup(root, 0.985f);
    }

    static float Z(float p) => Level3Progress.WorldZ(p);

    static void Rock(Transform root, int lane, float p) => Level3Primitives.MakeRock(root, lane, Z(p));
    static void Mud(Transform root, int lane, float p) => Level3Primitives.MakeMudPuddle(root, lane, Z(p));
    static void Bucket(Transform root, int lane, float p) => Level3Primitives.MakeBucket(root, lane, Z(p));
    static void Health(Transform root, int lane, float p) => Level3Primitives.MakeHealth(root, lane, Z(p));
    static void Leaf(Transform root, int lane, float p) => Level3Primitives.MakeLeaf(root, lane, Z(p));
    static void Acid(Transform root, float p, int[] lanes) => Level3Primitives.MakeAcidRain(root, Z(p), lanes, p);
    static void Snake(Transform root, float p, Level3EnemyPace pace, bool right) => Level3Primitives.MakeSnake(root, Z(p), pace, right);
    static void Warthog(Transform root, float p, Level3EnemyPace pace, bool right) => Level3Primitives.MakeWarthog(root, Z(p), pace, right);

    static void Mat(Transform root, int lane, float p, ref int index)
    {
        Level3Primitives.MakeMaterial(root, lane, Z(p), MaterialCycle[index % MaterialCycle.Length]);
        index++;
    }

    static void Tank(Transform root, Level3PipeRepair repair, int tank, int lane, float p)
    {
        Level3Primitives.MakeTank(root, tank, lane, Z(p), repair);
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
