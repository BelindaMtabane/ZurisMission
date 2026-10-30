using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns the hand-made prop prefabs in Resources/Props. Several of them were saved with
/// pivots far away from their geometry, so every spawn is re-centred on its footprint.
/// </summary>
public static class CreatedProps
{
    const string ResourcesFolder = "Props/";

    public const string Hammer = "Hammer";
    public const string Nails = "Nails";
    public const string Tape = "Tape";
    public const string Bricks = "Bricks";
    public const string SandBag = "SandBag";
    public const string DesertHouse = "DesertHouse";
    public const string MountainBrown = "MountainWall_Brown";
    public const string MountainSparse = "MountainWall_Sparse";
    public const string MountainCubeLush = "MountainWall_CubeLush";
    public const string WaterPool = "WaterPool";
    public const string MudMonster = "MudMonster";
    public const string Wolf = "Wolf";
    public const string BoreholeJars = "BoreholeJars";
    public const string Borehole = "Borehole";
    public const string Well = "Well";
    public const string Pig = "Pig";

    /// <summary>Warthog roots ride this far above the ground, so the pig visual is lowered by it.</summary>
    public const float WarthogRideHeight = 0.7f;

    public static readonly string[] DeadTrees = { "DeadTree_A", "DeadTree_B" };

    public static readonly string[] Cacti = { "DesertCactus_A", "DesertCactus_B", "DesertCactus_C" };
    public static readonly string[] Rocks = { "Rock_Boulder", "Rock_Round", "Rock_Slab" };
    public static readonly string[] Logs = { "Log_Long", "Log_Thick" };
    public static readonly string[] Thorns = { "ThornBush_A", "ThornBush_B" };
    public static readonly string[] Villagers =
    {
        "Villager_Hammering", "Villager_CarryingWater", "Villager_Waving", "Villager_Digging", "Villager_Child"
    };

    static readonly Dictionary<string, GameObject> Cache = new();

    public static string Tank(int index) => "Tank" + (Mathf.Abs(index) % 3 + 1);

    public static string Pick(string[] names, int seed) => names[Mathf.Abs(seed) % names.Length];

    /// <summary>Uses the created set when its first prefab exists, otherwise the original asset names.</summary>
    public static string[] PreferCreated(string[] created, string[] original) =>
        Load(created[0]) != null ? created : original;

    public static string MountainWallForActiveScene()
    {
        string scene = SceneCatalog.ActiveName;
        if (SceneCatalog.IsLevel3(scene)) return MountainCubeLush;
        if (SceneCatalog.IsLevel2(scene)) return MountainSparse;
        return MountainBrown;
    }

    public static GameObject Load(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (Cache.TryGetValue(name, out GameObject cached) && cached != null) return cached;

        GameObject prefab = Resources.Load<GameObject>(ResourcesFolder + name);
        Cache[name] = prefab;
        return prefab;
    }

    /// <summary>Uniform scale so the prop is <paramref name="targetHeight"/> tall, bottom-centre on the holder.</summary>
    public static GameObject SpawnFitHeight(string name, Transform parent, Vector3 localPosition, float targetHeight, float yaw = 0f)
    {
        return Spawn(name, parent, localPosition, yaw, b => Vector3.one * (targetHeight / Mathf.Max(0.01f, b.size.y)));
    }

    /// <summary>Uniform scale so the widest horizontal side is <paramref name="targetWidth"/>.</summary>
    public static GameObject SpawnFitWidth(string name, Transform parent, Vector3 localPosition, float targetWidth, float yaw = 0f)
    {
        return Spawn(name, parent, localPosition, yaw, b => Vector3.one * (targetWidth / Mathf.Max(0.01f, Mathf.Max(b.size.x, b.size.z))));
    }

    /// <summary>
    /// Stretches the prop to exactly fill <paramref name="size"/> (x = length, y = height, z = depth in holder space).
    /// Only use with prefabs whose root has no rotation.
    /// </summary>
    public static GameObject SpawnFitBox(string name, Transform parent, Vector3 localPosition, Vector3 size, float yaw = 0f)
    {
        return Spawn(name, parent, localPosition, yaw, b => new Vector3(
            size.x / Mathf.Max(0.01f, b.size.x),
            size.y / Mathf.Max(0.01f, b.size.y),
            size.z / Mathf.Max(0.01f, b.size.z)));
    }

    static GameObject Spawn(string name, Transform parent, Vector3 localPosition, float yaw, System.Func<Bounds, Vector3> scaleFor)
    {
        GameObject prefab = Load(name);
        if (prefab == null) return null;

        GameObject instance = Object.Instantiate(prefab);
        instance.name = name;
        StripPhysics(instance);

        Bounds b = RendererBounds(instance);
        Vector3 rootPos = instance.transform.position;
        Vector3 bottomCentre = new Vector3(b.center.x, b.min.y, b.center.z);
        Vector3 s = scaleFor(b);

        GameObject holder = new GameObject(name);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        holder.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 prefabScale = instance.transform.localScale;
        Quaternion prefabRotation = instance.transform.rotation;
        instance.transform.SetParent(holder.transform, false);
        instance.transform.localRotation = prefabRotation;
        instance.transform.localScale = Vector3.Scale(prefabScale, s);
        instance.transform.localPosition = -Vector3.Scale(bottomCentre - rootPos, s);
        return holder;
    }

    static Bounds RendererBounds(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            if (rends[i] != null) b.Encapsulate(rends[i].bounds);
        return b;
    }

    static void StripPhysics(GameObject go)
    {
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] == null) continue;
            cols[i].enabled = false;
            if (Application.isPlaying) Object.Destroy(cols[i]);
        }

        Rigidbody[] bodies = go.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
            if (bodies[i] != null && Application.isPlaying) Object.Destroy(bodies[i]);
    }
}
