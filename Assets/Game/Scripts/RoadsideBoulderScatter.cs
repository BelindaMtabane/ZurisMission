using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grey slab boulders (Rock_A_05 style) along both shoulders — visual only, kept off lane pickups.
/// </summary>
public static class RoadsideBoulderScatter
{
    const string ResourcesFolder = "RoadsideBoulders/";

    static readonly string[] GreySlabRocks =
    {
        "Rock_A_05", "Rock_A_04", "Rock_A_03", "Rock_A_02", "Rock_A_01"
    };

    static readonly string[] OuterAccentRocks =
    {
        "Rock_A_04", "Rock_A_05", "Cliff_Small_1", "Cliff_Small_2"
    };

    static readonly string[] EditorPaths =
    {
        "Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Rock/{0}.prefab",
        "Assets/AGWYN's Low Poly Cliffs/Models/{0}.prefab"
    };

    static readonly Dictionary<string, GameObject> Cache = new();

    const float MountainSegmentLength = 12f;
    const float MountainWallHeight = 3.6f;
    const float DeadTreeSpacing = 7f;

    public static void ScatterBoulderWalls(Transform parent, float startZ, float endZ, float groundY)
    {
        if (parent == null || endZ <= startZ) return;

        bool level1 = SceneCatalog.IsLevel1(SceneCatalog.ActiveName);
        string mountainWall = CreatedProps.MountainWallForActiveScene();
        bool useDeadTrees = level1
            && CreatedProps.Load(CreatedProps.DeadTrees[0]) != null
            && CreatedProps.Load(CreatedProps.Rocks[0]) != null;
        bool useMountainWalls = !level1 && CreatedProps.Load(mountainWall) != null;
        if (!useDeadTrees && !useMountainWalls && !HasAnyPrefab()) return;

        LevelLanes.GetBoulderWallBand(out float wallInner, out float wallOuter);
        float minAbsX = wallInner;

        Transform root = parent.Find("RoadsideBoulderWall");
        if (root == null)
        {
            var host = new GameObject("RoadsideBoulderWall");
            host.transform.SetParent(parent, false);
            root = host.transform;
        }
        else
        {
            for (int c = root.childCount - 1; c >= 0; c--)
                Object.Destroy(root.GetChild(c).gameObject);
        }

        if (useDeadTrees)
        {
            ScatterDeadTreesAndRocks(root, startZ, endZ, groundY, wallInner, wallOuter);
            EnsureVisualOnly();
            return;
        }

        if (useMountainWalls)
        {
            ScatterMountainWalls(root, mountainWall, startZ, endZ, groundY, wallInner, wallOuter);
            EnsureVisualOnly();
            return;
        }

        float span = endZ - startZ;
        float step = 4.6f;
        int steps = Mathf.Max(8, Mathf.CeilToInt(span / step));
        int index = 0;

        for (int s = 0; s < steps; s++)
        {
            float z = startZ + (s + 0.48f) / steps * span + (Hash01(s * 61 + 3) - 0.5f) * step * 0.42f;
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                float x = left
                    ? -Mathf.Lerp(wallInner, wallOuter, Hash01(index * 23 + 1))
                    : Mathf.Lerp(wallInner, wallOuter, Hash01(index * 29 + 2));
                x = left ? -Mathf.Max(minAbsX, Mathf.Abs(x)) : Mathf.Max(minAbsX, x);

                string prefabName = GreySlabRocks[(index + s) % GreySlabRocks.Length];
                GameObject prefab = LoadPrefab(prefabName);
                if (prefab == null) continue;

                GameObject boulder = Object.Instantiate(prefab, root);
                boulder.name = $"BoulderWall_{prefabName}_{index}";
                boulder.transform.position = new Vector3(x, groundY, z);
                boulder.transform.rotation = Quaternion.Euler(
                    (Hash01(index * 11) - 0.5f) * 4f,
                    Hash01(index * 17 + 5) * 360f,
                    (Hash01(index * 13 + 7) - 0.5f) * 4f);

                float absX = Mathf.Abs(x);
                float bandT = Mathf.InverseLerp(wallInner, wallOuter, absX);
                float scale = Mathf.Lerp(0.42f, 0.62f, bandT) + Hash01(index * 19 + 9) * 0.06f;
                boulder.transform.localScale = Vector3.one * scale;
                GroundToSurface(boulder.transform, groundY);
                StripAllPhysics(boulder);

                index++;
            }

            if (s % 2 == 0)
            {
                for (int side = 0; side < 2; side++)
                {
                    bool left = side == 0;
                    float outerX = wallOuter + 1.35f + Hash01(s * 37 + side) * 1.55f;
                    float x2 = left ? -outerX : outerX;
                    string accent = OuterAccentRocks[(s + side) % OuterAccentRocks.Length];
                    GameObject prefab2 = LoadPrefab(accent);
                    if (prefab2 == null) continue;

                    GameObject extra = Object.Instantiate(prefab2, root);
                    extra.name = $"BoulderWall_Outer_{accent}_{s}_{side}";
                    extra.transform.position = new Vector3(x2, groundY, z + (Hash01(s * 43) - 0.5f) * 2f);
                    extra.transform.rotation = Quaternion.Euler(0f, Hash01(s * 47 + side) * 360f, 0f);
                    float accentScale = accent.StartsWith("Rock_A")
                        ? 0.68f + Hash01(s * 53 + side) * 0.16f
                        : 0.62f + Hash01(s * 53 + side) * 0.14f;
                    extra.transform.localScale = Vector3.one * accentScale;
                    GroundToSurface(extra.transform, groundY);
                    StripAllPhysics(extra);
                }
            }
        }

        EnsureVisualOnly();
    }

    static void ScatterDeadTreesAndRocks(Transform root, float startZ, float endZ, float groundY, float wallInner, float wallOuter)
    {
        float span = endZ - startZ;
        int steps = Mathf.Max(1, Mathf.CeilToInt(span / DeadTreeSpacing));

        for (int s = 0; s < steps; s++)
        {
            for (int side = 0; side < 2; side++)
            {
                int seed = s * 2 + side;
                bool left = side == 0;
                float z = startZ + (s + 0.5f) / steps * span + (Hash01(seed * 61 + 3) - 0.5f) * DeadTreeSpacing * 0.5f;
                float mag = Mathf.Lerp(wallInner, wallOuter, Hash01(seed * 23 + 1));
                float yaw = Hash01(seed * 17 + 5) * 360f;
                bool tree = Hash01(seed * 41 + 9) < 0.45f;

                string prefabName = tree
                    ? CreatedProps.Pick(CreatedProps.DeadTrees, seed)
                    : CreatedProps.Pick(CreatedProps.Rocks, seed);
                float height = tree
                    ? 5.5f + Hash01(seed * 7) * 2.5f
                    : 1.1f + Hash01(seed * 13) * 1.3f;

                GameObject prop = CreatedProps.SpawnFitHeight(prefabName, root, Vector3.zero, height, yaw);
                if (prop == null) continue;

                prop.name = $"BoulderWall_{prefabName}_{s}_{side}";
                prop.transform.position = new Vector3(left ? -mag : mag, groundY, z);
            }
        }
    }

    static void ScatterMountainWalls(Transform root, string wallName, float startZ, float endZ, float groundY, float wallInner, float wallOuter)
    {
        float depth = Mathf.Max(2.5f, (wallOuter - wallInner) * 0.85f);
        float bandMid = wallInner + depth * 0.5f;
        int segments = Mathf.Max(1, Mathf.CeilToInt((endZ - startZ) / MountainSegmentLength));
        float length = (endZ - startZ) / segments;

        for (int s = 0; s < segments; s++)
        {
            float z = startZ + (s + 0.5f) * length;
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                float height = MountainWallHeight * (0.85f + Hash01(s * 31 + side * 7) * 0.3f);
                GameObject wall = CreatedProps.SpawnFitBox(
                    wallName,
                    root,
                    Vector3.zero,
                    new Vector3(length * 1.04f, height, depth),
                    left ? 90f : -90f);
                if (wall == null) continue;

                wall.name = $"BoulderWall_{wallName}_{s}_{side}";
                wall.transform.position = new Vector3(left ? -bandMid : bandMid, groundY, z);
            }
        }
    }

    static bool HasAnyPrefab()
    {
        for (int i = 0; i < GreySlabRocks.Length; i++)
        {
            if (LoadPrefab(GreySlabRocks[i]) != null) return true;
        }

        return false;
    }

    static GameObject LoadPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (Cache.TryGetValue(name, out GameObject cached) && cached != null) return cached;

        GameObject prefab = Resources.Load<GameObject>(ResourcesFolder + name);
#if UNITY_EDITOR
        if (prefab == null)
        {
            for (int i = 0; i < EditorPaths.Length; i++)
            {
                string path = string.Format(EditorPaths[i], name);
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) break;
            }
        }
#endif
        Cache[name] = prefab;
        return prefab;
    }

    static void GroundToSurface(Transform t, float groundY)
    {
        Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        t.position += Vector3.up * (groundY - b.min.y);
    }

    static void StripAllPhysics(GameObject go)
    {
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] != null) Object.Destroy(cols[i]);
        }

        Rigidbody[] bodies = go.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i] != null) Object.Destroy(bodies[i]);
        }
    }

    /// <summary>Scenery boulders must never block the player or lane pickups.</summary>
    public static void EnsureVisualOnly()
    {
        GameObject[] roots = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject go = roots[i];
            if (go == null || go.name != "RoadsideBoulderWall") continue;
            for (int c = 0; c < go.transform.childCount; c++)
                StripAllPhysics(go.transform.GetChild(c).gameObject);
        }
    }

    static float Hash01(int value)
    {
        uint x = (uint)value;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        return (x & 0xFFFF) / 65535f;
    }
}
