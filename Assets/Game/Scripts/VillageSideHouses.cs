using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns desert-village houses beside the runner path (never on lanes).
/// </summary>
public static class VillageSideHouses
{
    const string ResourcesFolder = "Village/";
    const float HouseScale = 1.85f;
    const float HouseFootprintRadius = 5.2f;

    static readonly string[] HouseNames =
    {
        "House1", "House2", "House3", "House4", "House6", "House5"
    };

    struct Placed
    {
        public float X;
        public float Z;
    }

    static readonly Dictionary<string, GameObject> PrefabCache = new();

    public static void Scatter(Transform parent, float startZ, float endZ, float groundY, int count)
    {
        if (parent == null || count <= 0 || endZ <= startZ) return;

        LevelLanes.ConfigureForActiveScene();
        LevelLanes.GetVillageHouseBand(out float inner, out float outer);
        float pathCenter = LevelLanes.PathCenterX;
        if (SceneCatalog.IsLevel2(SceneCatalog.ActiveName) || SceneCatalog.IsLevel3(SceneCatalog.ActiveName))
        {
            inner = Mathf.Max(LevelLanes.MinAbsXScenery() + 2.5f, 20f);
            outer = inner + 8f;
        }

        Transform root = parent.Find("VillageSideHouses");
        if (root == null)
        {
            var go = new GameObject("VillageSideHouses");
            go.transform.SetParent(parent, false);
            root = go.transform;
        }
        else
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                Object.Destroy(root.GetChild(i).gameObject);
        }

        var placed = new List<Placed>(count);
        float span = endZ - startZ;
        int spawned = 0;

        for (int slot = 0; slot < count; slot++)
        {
            float t = (slot + 0.5f) / count;
            float baseZ = startZ + t * span;
            bool left = slot % 2 == 0;
            bool placedHouse = false;

            for (int attempt = 0; attempt < 14 && !placedHouse; attempt++)
            {
                float z = baseZ + (Hash01(slot * 31 + attempt * 7) - 0.5f) * (span / count) * 0.45f;
                float x = left
                    ? pathCenter - Mathf.Lerp(inner, outer, Hash01(slot * 17 + attempt))
                    : pathCenter + Mathf.Lerp(inner, outer, Hash01(slot * 19 + attempt + 4));

                if (!IsSpotFree(placed, x, z, HouseFootprintRadius)) continue;
                if (RoadsideOverlapResolver.WouldHouseOverlapBoulders(x, z)) continue;

                string houseName = HouseNames[(slot + attempt) % HouseNames.Length];
                GameObject prefab = LoadHousePrefab(houseName);
                if (prefab == null) break;

                GameObject instance = Object.Instantiate(prefab, root);
                instance.name = $"VillageHouse_{houseName}_{spawned}";
                instance.transform.position = new Vector3(x, groundY, z);
                instance.transform.localScale = Vector3.one * HouseScale;
                instance.transform.rotation = Quaternion.Euler(0f, left ? 90f : -90f, 0f);
                GroundToSurface(instance.transform, groundY);
                StripColliders(instance);
                NaturePackVisuals.EnsureSceneObjectMaterials(instance);
                if (ShouldPlaceHouseTank(spawned))
                {
                    // Place the tank on the road-facing side, beside the house so it is visible.
                    float tankX = x + (left ? 4.4f : -4.4f);
                    float tankZ = z + 5.6f;
                    PlaceHouseTank(root, tankX, groundY, tankZ, spawned);
                }

                placed.Add(new Placed { X = x, Z = z });
                spawned++;
                placedHouse = true;
            }
        }
    }

    static bool IsSpotFree(List<Placed> placed, float x, float z, float radius)
    {
        for (int i = 0; i < placed.Count; i++)
        {
            float dx = placed[i].X - x;
            float dz = placed[i].Z - z;
            float min = radius + HouseFootprintRadius + 2.5f;
            if (dx * dx + dz * dz < min * min) return false;
        }

        return true;
    }

    static GameObject LoadHousePrefab(string houseName)
    {
        if (PrefabCache.TryGetValue(houseName, out GameObject cached) && cached != null)
            return cached;

        GameObject prefab = Resources.Load<GameObject>(ResourcesFolder + houseName);
#if UNITY_EDITOR
        if (prefab == null)
        {
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/PolyRonin/Desert Village/Prefabs/{houseName}.prefab");
        }
#endif
        PrefabCache[houseName] = prefab;
        return prefab;
    }

    static GameObject waterTowerPrefab;
    static bool waterTowerSearched;

    static bool ShouldPlaceHouseTank(int spawned)
    {
        if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName)) return true;
        // Level 2: a few tanks only — about one beside every other house.
        if (SceneCatalog.IsLevel2(SceneCatalog.ActiveName)) return spawned % 2 == 0;
        return false;
    }

    static bool UseGreenHouseTanks => SceneCatalog.IsLevel2(SceneCatalog.ActiveName);

    static readonly Color TankBlue = new Color(0.10f, 0.42f, 0.78f);
    static readonly Color TankDarkBlue = new Color(0.06f, 0.22f, 0.48f);
    static readonly Color TankGreen = new Color(0.18f, 0.62f, 0.28f);
    static readonly Color TankDarkGreen = new Color(0.08f, 0.34f, 0.16f);
    static readonly Color TankRed = new Color(0.82f, 0.14f, 0.10f);
    static readonly Color TankDarkRed = new Color(0.42f, 0.08f, 0.06f);
    static readonly Color TankFrame = new Color(0.27f, 0.30f, 0.32f);

    public static GameObject PlaceWrappedTank(Transform parent, float x, float groundY, float z, int index, YoyoTankColor color)
    {
        Color body;
        Color dark;
        BodyColors(color, out body, out dark);
        return BuildWaterTank(parent, x, groundY, z, index, body, dark, color);
    }

    static void BodyColors(YoyoTankColor color, out Color body, out Color dark)
    {
        if (color == YoyoTankColor.Green)
        {
            body = TankGreen;
            dark = TankDarkGreen;
            return;
        }

        if (color == YoyoTankColor.Red)
        {
            body = TankRed;
            dark = TankDarkRed;
            return;
        }

        body = TankBlue;
        dark = TankDarkBlue;
    }

    static void PlaceHouseTank(Transform parent, float x, float groundY, float z, int index)
    {
        YoyoTankColor wrapColor = UseGreenHouseTanks ? YoyoTankColor.Green : YoyoTankColor.Blue;
        Color body;
        Color dark;
        BodyColors(wrapColor, out body, out dark);
        bool wrapBody = YoyoTankWrap.Get(wrapColor) != null;
        GameObject prefab = wrapBody ? null : LoadWaterTowerPrefab();
        if (prefab != null)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = $"HouseWaterTower_{index}";
            instance.transform.position = new Vector3(x, groundY, z);
            instance.transform.rotation = Quaternion.Euler(0f, Hash01(index * 13) * 360f, 0f);
            FitHeight(instance, 7.4f);
            GroundToSurface(instance.transform, groundY);
            StripColliders(instance);
            NaturePackVisuals.EnsureSceneObjectMaterials(instance);
            return;
        }

        BuildWaterTank(parent, x, groundY, z, index, body, dark, wrapColor);
    }

    static GameObject LoadWaterTowerPrefab()
    {
        if (waterTowerSearched) return waterTowerPrefab;
        waterTowerSearched = true;

        string[] resourceNames =
        {
            "Village/BigWaterTower",
            "Village/WaterTower",
            "Village/Big Water Tower",
            "Village/Water_Tower"
        };
        for (int i = 0; i < resourceNames.Length; i++)
        {
            waterTowerPrefab = Resources.Load<GameObject>(resourceNames[i]);
            if (waterTowerPrefab != null) return waterTowerPrefab;
        }

#if UNITY_EDITOR
        waterTowerPrefab = FindEditorWaterTower();
#endif
        return waterTowerPrefab;
    }

#if UNITY_EDITOR
    static GameObject FindEditorWaterTower()
    {
        string[] folders =
        {
            "Assets/Big Water Tower",
            "Assets/BigWaterTower",
            "Assets/Water Tower",
            "Assets"
        };

        for (int f = 0; f < folders.Length; f++)
        {
            if (f < 3 && !UnityEditor.AssetDatabase.IsValidFolder(folders[f])) continue;
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { folders[f] });
            GameObject best = null;
            int bestScore = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;
                string n = path.Replace("\\", "/").ToLowerInvariant();
                int score = 0;
                if (n.Contains("big water tower") || n.Contains("bigwatertower")) score += 8;
                if (n.Contains("water tower") || n.Contains("watertower") || n.Contains("water_tower")) score += 5;
                if (n.Contains("tower") && n.Contains("water")) score += 4;
                if (score == 0) continue;
                if (n.Contains("fx") || n.Contains("splash") || n.Contains("tile")) continue;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }

            if (best != null) return best;
        }

        return null;
    }
#endif

    static void FitHeight(GameObject go, float targetHeight)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0) return;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
        {
            if (rends[i] != null) b.Encapsulate(rends[i].bounds);
        }

        float height = Mathf.Max(0.15f, b.size.y);
        go.transform.localScale *= targetHeight / height;
    }

    static GameObject BuildWaterTank(
        Transform parent,
        float x,
        float groundY,
        float z,
        int index,
        Color body,
        Color dark,
        YoyoTankColor wrapColor)
    {
        GameObject tank = new GameObject($"HouseWaterTank_{wrapColor}_{index}");
        tank.transform.SetParent(parent, false);
        tank.transform.position = new Vector3(x, groundY, z);

        bool wrapBody = YoyoTankWrap.Get(wrapColor) != null;
        if (wrapBody)
        {
            // Lettering sits on local -Z; turn it toward the road and the approaching camera.
            float towardRoad = x < LevelLanes.PathCenterX ? 1f : -1f;
            tank.transform.rotation = Quaternion.Euler(0f, -36.87f * towardRoad, 0f);
        }

        AddTankPart(PrimitiveType.Cylinder, tank.transform, "TankBody",
            new Vector3(0f, 3.15f, 0f), new Vector3(2.35f, 2.85f, 2.35f), body, wrapBody, wrapColor);
        AddTankPart(PrimitiveType.Cylinder, tank.transform, "TankLid",
            new Vector3(0f, 6.1f, 0f), new Vector3(2.5f, 0.16f, 2.5f), dark);
        AddTankPart(PrimitiveType.Cylinder, tank.transform, "TankBase",
            new Vector3(0f, 0.22f, 0f), new Vector3(2.55f, 0.22f, 2.55f), TankFrame);
        AddTankPart(PrimitiveType.Cube, tank.transform, "TankStand",
            new Vector3(0f, 0.85f, 0f), new Vector3(3.4f, 0.22f, 3.4f), TankFrame);
        AddTankPart(PrimitiveType.Cylinder, tank.transform, "DownPipe",
            new Vector3(-2.05f, 1.7f, 0f), new Vector3(0.18f, 1.7f, 0.18f), dark);

        return tank;
    }

    static void TintRenderers(GameObject go, Color body, Color dark)
    {
        if (go == null) return;
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] == null) continue;
            Color tint = i == 0 || rends[i].name.IndexOf("lid", System.StringComparison.OrdinalIgnoreCase) >= 0
                || rends[i].name.IndexOf("pipe", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? dark
                : body;
            ApplyColor(rends[i], tint);
        }
    }

    static void ApplyColor(Renderer rend, Color color)
    {
        if (rend == null) return;
        Material mat = rend.material;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        mat.color = color;
    }

    static void AddTankPart(
        PrimitiveType type,
        Transform parent,
        string name,
        Vector3 localPosition,
        Vector3 localScale,
        Color color,
        bool yoyoWrap = false,
        YoyoTankColor wrapColor = YoyoTankColor.Green)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        Collider col = part.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Renderer rend = part.GetComponent<Renderer>();
        if (yoyoWrap) YoyoTankWrap.Apply(rend, wrapColor);
        else ApplyColor(rend, color);
    }

    static void GroundToSurface(Transform t, float groundY)
    {
        Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        t.position += Vector3.up * (groundY - b.min.y);
    }

    static void StripColliders(GameObject go)
    {
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] != null) Object.Destroy(cols[i]);
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
