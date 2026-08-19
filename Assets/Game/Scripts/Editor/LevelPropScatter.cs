using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Randomly scatters village props (wells, boreholes, tanks, houses, NPCs)
/// across a level's full runner length, clear of the 4 lane positions
/// (-4.6, -0.6, 3.4, 7.4) using the established safe decoration band
/// (X -20..-12 left / +12..+20 right of the lanes).
/// </summary>
public static class LevelPropScatter
{
    const float NpcScale = 2.8f;
    const float HouseScale = 1.6f;

    static readonly string[] Houses =
    {
        "Assets/PolyRonin/Desert Village/Prefabs/House1.prefab",
        "Assets/PolyRonin/Desert Village/Prefabs/House2.prefab",
        "Assets/PolyRonin/Desert Village/Prefabs/House3.prefab",
        "Assets/PolyRonin/Desert Village/Prefabs/House4.prefab",
        "Assets/PolyRonin/Desert Village/Prefabs/House6.prefab",
    };

    static readonly string[] Npcs =
    {
        "Assets/DenysAlmaral/CityPeople/Prefabs/city/casual_Female_G.prefab",
        "Assets/DenysAlmaral/CityPeople/Prefabs/city/casual_Male_G.prefab",
        "Assets/DenysAlmaral/CityPeople/Prefabs/elder/elder_Female_A.prefab",
        "Assets/DenysAlmaral/CityPeople/Prefabs/little_kids/little_boy_B.prefab",
        "Assets/DenysAlmaral/CityPeople/Prefabs/downtown/casual_Female_K.prefab",
    };

    const string Jar1 = "Assets/PolyRonin/Desert Village/Prefabs/Jar1.prefab";
    const string Jar2 = "Assets/PolyRonin/Desert Village/Prefabs/Jar2.prefab";
    const string Jar3 = "Assets/PolyRonin/Desert Village/Prefabs/Jar3.prefab";
    const string Silo = "Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Farm House/Farm_House_Silo_01.prefab";

    class Placed { public float x, z, r; }
    static List<Placed> placed;
    static System.Random rng;

    // ── Level 1: MainGame — wells (visible water) + houses ─────────────────
    [MenuItem("Tools/Scatter/Clear + Scatter Level1")]
    public static void ScatterLevel1()
    {
        DestroyByPrefix("House_L1_", "WaterJar_L1_", "Well_", "Borehole_");
        placed = new List<Placed>();
        rng = new System.Random(101);
        for (int i = 0; i < 7; i++) SpawnWell(25f, 970f);
        for (int i = 0; i < 6; i++) SpawnHouse(25f, 970f);
        Log("Level1");
    }

    // ── Level 2: boreholes (jar clusters) + NPCs + a few houses ─────────────
    [MenuItem("Tools/Scatter/Clear + Scatter Level2")]
    public static void ScatterLevel2()
    {
        DestroyByPrefix("Borehole_L2_", "NPC_L2_", "House_L2_");
        placed = new List<Placed>();
        rng = new System.Random(202);
        for (int i = 0; i < 8; i++) SpawnBorehole(25f, 1350f);
        for (int i = 0; i < 5; i++) SpawnHouse(25f, 1350f);
        for (int i = 0; i < 10; i++) SpawnNpc(25f, 1350f);
        Log("Level2");
    }

    // ── Level 3: tanks + houses + NPCs (much longer level) ──────────────────
    [MenuItem("Tools/Scatter/Clear + Scatter Level3")]
    public static void ScatterLevel3()
    {
        DestroyByPrefix("Tank_L3_", "House_L3_", "NPC_L3_");
        placed = new List<Placed>();
        rng = new System.Random(303);
        for (int i = 0; i < 16; i++) SpawnTank(25f, 7050f);
        for (int i = 0; i < 16; i++) SpawnHouse(25f, 7050f);
        for (int i = 0; i < 20; i++) SpawnNpc(25f, 7050f);
        Log("Level3");
    }

    static void Log(string level) =>
        Debug.Log($"[LevelPropScatter] {level}: placed {placed.Count} items (some spots may have been skipped if no free space was found).");

    // ── Spot-finding ─────────────────────────────────────────────────────
    static bool TryFindSpot(float zMin, float zMax, float radius, out float x, out float z)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            bool left = rng.NextDouble() < 0.5;
            x = left ? Lerp(-20f, -12f) : Lerp(12f, 20f);
            z = Lerp(zMin, zMax);
            bool ok = true;
            foreach (var p in placed)
            {
                float dx = p.x - x, dz = p.z - z;
                float minDist = p.r + radius + 3f;
                if (dx * dx + dz * dz < minDist * minDist) { ok = false; break; }
            }
            if (ok) return true;
        }
        x = 0; z = 0;
        return false;
    }

    static float Lerp(float a, float b) => a + (float)rng.NextDouble() * (b - a);

    // ── Spawners ─────────────────────────────────────────────────────────
    static GameObject InstantiateAt(string prefabPath, float x, float z, float yRot)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError($"[LevelPropScatter] prefab not found: {prefabPath}"); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = new Vector3(x, 0f, z);
        go.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        return go;
    }

    static void SpawnHouse(float zMin, float zMax)
    {
        if (!TryFindSpot(zMin, zMax, 6f, out float x, out float z)) return;
        string prefabPath = Houses[rng.Next(Houses.Length)];
        bool left = x < 0f;
        var go = InstantiateAt(prefabPath, x, z, left ? 90f : -90f);
        if (go == null) return;
        go.transform.localScale = Vector3.one * HouseScale;
        go.name = $"House_{System.IO.Path.GetFileNameWithoutExtension(prefabPath)}_{placed.Count}";
        placed.Add(new Placed { x = x, z = z, r = 6f });
    }

    static void SpawnNpc(float zMin, float zMax)
    {
        if (!TryFindSpot(zMin, zMax, 1.6f, out float x, out float z)) return;
        string prefabPath = Npcs[rng.Next(Npcs.Length)];
        var go = InstantiateAt(prefabPath, x, z, (float)rng.NextDouble() * 360f);
        if (go == null) return;
        go.transform.localScale = Vector3.one * NpcScale;
        go.name = $"NPC_{System.IO.Path.GetFileNameWithoutExtension(prefabPath)}_{placed.Count}";
        placed.Add(new Placed { x = x, z = z, r = 1.6f });
    }

    static void SpawnTank(float zMin, float zMax)
    {
        if (!TryFindSpot(zMin, zMax, 5f, out float x, out float z)) return;
        var go = InstantiateAt(Silo, x, z, (float)rng.NextDouble() * 360f);
        if (go == null) return;
        go.name = $"Tank_{placed.Count}";
        placed.Add(new Placed { x = x, z = z, r = 5f });
    }

    static void SpawnBorehole(float zMin, float zMax)
    {
        if (!TryFindSpot(zMin, zMax, 2.2f, out float x, out float z)) return;
        var jarA = InstantiateAt(Jar1, x, z, (float)rng.NextDouble() * 360f);
        var jarB = InstantiateAt(Jar2, x + 0.9f, z + 0.4f, (float)rng.NextDouble() * 360f);
        var water = BuildWaterDisc(x + 0.35f, 0.28f, z + 0.15f, 0.55f);
        if (jarA != null) jarA.name = $"Borehole_Jar_{placed.Count}";
        if (jarB != null) jarB.name = $"Borehole_Jar_{placed.Count}b";
        water.name = $"Borehole_Water_{placed.Count}";
        placed.Add(new Placed { x = x, z = z, r = 2.2f });
    }

    static void SpawnWell(float zMin, float zMax)
    {
        if (!TryFindSpot(zMin, zMax, 2.5f, out float x, out float z)) return;
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = $"Well_{placed.Count}";
        body.transform.position = new Vector3(x, 0.5f, z);
        body.transform.localScale = new Vector3(1.6f, 0.5f, 1.6f);
        TintPrimitive(body, new Color(0.55f, 0.5f, 0.45f));

        var water = BuildWaterDisc(x, 0.86f, z, 0.68f);
        water.transform.SetParent(body.transform, true);
        water.name = "WellWaterTop";

        InstantiateAt(Jar3, x + 1.7f, z + 0.6f, (float)rng.NextDouble() * 360f);
        placed.Add(new Placed { x = x, z = z, r = 2.5f });
    }

    static GameObject BuildWaterDisc(float x, float y, float z, float radius)
    {
        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.transform.position = new Vector3(x, y, z);
        disc.transform.localScale = new Vector3(radius, 0.02f, radius);
        TintPrimitive(disc, new Color(0.15f, 0.45f, 0.78f));
        return disc;
    }

    static void TintPrimitive(GameObject go, Color c)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null) return;
        rend.sharedMaterial = GetCachedMaterial(rend.sharedMaterial, c);
    }

    // One material per distinct colour, shared across every spawned object —
    // avoids creating dozens of one-off Material instances (each of which can
    // cost a shader-variant compile stall the first time it's rendered).
    static readonly System.Collections.Generic.Dictionary<Color, Material> materialCache = new();

    static Material GetCachedMaterial(Material template, Color c)
    {
        if (materialCache.TryGetValue(c, out Material cached) && cached != null) return cached;
        var mat = new Material(template);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
        materialCache[c] = mat;
        return mat;
    }

    static void DestroyByPrefix(params string[] prefixes)
    {
        var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var t in all)
        {
            if (t == null) continue;
            foreach (var p in prefixes)
            {
                if (t.name.StartsWith(p))
                {
                    Object.DestroyImmediate(t.gameObject);
                    break;
                }
            }
        }
    }
}
