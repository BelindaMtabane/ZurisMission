using UnityEngine;

/// <summary>
/// Instantiates Pandazole Nature Environment Pack meshes as visual-only children
/// of the existing gameplay roots (triggers and scripts stay unchanged).
/// </summary>
public static class NaturePackVisuals
{
    const string PrefabFolder = "Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs";
    const string ResourcesFolder = "NaturePack/";

    static readonly string[] CactusNames =
    {
        "Cactus_01_A", "Cactus_04_A", "Cactus_08_A", "Cactus_12_A", "Cactus_15_A",
        "Cactus_19_A", "Cactus_20_A", "Cactus_24_A", "Cactus_27_A", "Cactus_31_A",
        "Cactus_01_B", "Cactus_08_B", "Cactus_12_B", "Cactus_19_B", "Cactus_24_B"
    };

    static readonly string[] LogNames =
    {
        "Log_01", "Log_02", "Log_03", "Log_04", "Log_05", "Log_06", "Log_07", "Log_08"
    };

    static readonly string[] RockNames =
    {
        "HardRock_01", "HardRock_02", "HardRock_06", "HardRock_08", "HardRock_12",
        "HardRock_15", "HardRock_18", "SoftRock_03", "SoftRock_06", "SoftRock_13"
    };

    static readonly string[] TreeNames =
    {
        "Tree_01_Fall", "Tree_04_Fall", "Tree_08_Fall", "Tree_12_Fall", "Tree_16_Fall", "Tree_20_Fall"
    };

    static readonly string[] BushNames =
    {
        "Bush_02", "Bush_07", "Bush_10", "Bush_13", "Thorns_02", "Thorns_05", "Thorns_14"
    };

    static readonly string[] ThornBushNames =
    {
        "Thorns_02", "Thorns_05", "Thorns_14", "Thorns_02", "Thorns_05"
    };

    static readonly string[] GrassNames =
    {
        "Grass_01", "Grass_03", "Grass_07", "Grass_11", "Grass_14", "Grass_19",
        "Grass_22", "Grass_26", "Grass_29", "Grass_35", "Grass_08", "Grass_16"
    };

    static readonly string[] FlowerNames =
    {
        "Flower_01", "Flower_03", "Flower_06", "Flower_09", "Flower_12",
        "Flower_15", "Flower_19", "Flower_22", "Flower_26", "Flower_30"
    };

    static readonly System.Collections.Generic.Dictionary<string, GameObject> Cache = new();

    public static bool Available => LoadPrefab(CactusNames[0]) != null;

    public static void EnsureSceneObjectMaterials(GameObject go) => EnsureUrpMaterials(go);

    /// <summary>Global scale for Pandazole cacti (barrel / round types were oversized).</summary>
    public const float CactusVisualScale = 0.82f;

    public static bool AttachCactus(Transform parent, float targetHeight, float yaw = float.NaN)
    {
        return AttachPlant(parent, Pick(CactusNames, parent), targetHeight * CactusVisualScale, yaw);
    }

    public static bool AttachRock(Transform parent, float targetHeight, Vector3 localPos = default)
    {
        GameObject go = Spawn(parent, Pick(RockNames, parent), localPos, applyPlantTint: false);
        if (go == null) return false;
        FitHeight(go.transform, targetHeight);
        ClampFootprint(go.transform, targetHeight * 1.25f);
        PlantOnGround(go.transform);
        RandomYaw(go.transform, parent);
        return true;
    }

    public static bool AttachTree(Transform parent, float targetHeight)
    {
        if (!AttachPlant(parent, Pick(TreeNames, parent), targetHeight)) return false;
        Transform visual = parent.childCount > 0 ? parent.GetChild(parent.childCount - 1) : parent;
        ClampFootprint(visual, Mathf.Min(4.2f, targetHeight * 0.55f));
        return true;
    }

    public static bool AttachBush(Transform parent, float targetHeight, Vector3 localPos = default)
    {
        GameObject go = Spawn(parent, PickBush(parent), localPos);
        if (go == null) return false;
        FitHeight(go.transform, targetHeight);
        ClampFootprint(go.transform, targetHeight * 1.35f);
        PlantOnGround(go.transform);
        RandomYaw(go.transform, parent);
        return true;
    }

    public static bool AttachGrass(Transform parent, float targetHeight, Vector3 localPos = default)
    {
        GameObject go = Spawn(parent, Pick(GrassNames, parent), localPos);
        if (go == null) return false;
        FitHeight(go.transform, targetHeight);
        ClampFootprint(go.transform, targetHeight * 1.1f);
        PlantOnGround(go.transform);
        RandomYaw(go.transform, parent);
        return true;
    }

    public static bool AttachFlower(Transform parent, float targetHeight, Vector3 localPos = default, int variant = -1)
    {
        string flowerName = variant >= 0 ? FlowerNames[variant % FlowerNames.Length] : Pick(FlowerNames, parent);
        // Petal colours come from the shared palette; the level plant tint would wash them green.
        GameObject go = Spawn(parent, flowerName, localPos, applyPlantTint: false);
        if (go == null) return false;
        FitHeight(go.transform, targetHeight);
        ClampFootprint(go.transform, targetHeight * 1.25f);
        PlantOnGround(go.transform);
        RandomYaw(go.transform, parent);
        return true;
    }

    public static bool AttachLogAcross(Transform parent, float localY, float width)
    {
        GameObject go = Spawn(parent, Pick(LogNames, parent), Vector3.zero);
        if (go == null) return false;

        AlignLongestAxis(go.transform, Vector3.right);
        Bounds b = WorldBounds(go);
        float current = Mathf.Max(0.01f, b.size.x);
        go.transform.localScale *= width / current;

        b = WorldBounds(go);
        float groundY = parent.position.y;
        go.transform.position += Vector3.up * (groundY - b.min.y + 0.05f);
        return true;
    }

    /// <summary>
    /// Builds a lane-spanning rolling log from normal-proportioned asset segments.
    /// This avoids enlarging one short prop until its diameter becomes oversized.
    /// </summary>
    public static bool AttachLevel2RollingLog(Transform parent, float width)
    {
        const string prefabName = "Log_08";
        const int segmentCount = 2;
        float segmentLength = width / segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = Spawn(parent, prefabName, Vector3.zero);
            if (segment == null) return false;

            segment.name = $"Log_08_RollSegment_{i + 1}";
            AlignLongestAxis(segment.transform, Vector3.right);

            Bounds bounds = WorldBounds(segment);
            float currentLength = Mathf.Max(0.01f, bounds.size.x);
            segment.transform.localScale *= (segmentLength * 1.06f) / currentLength;

            bounds = WorldBounds(segment);
            float targetX = -width * 0.5f + segmentLength * (i + 0.5f);
            segment.transform.position += Vector3.right * (targetX - bounds.center.x);
            segment.transform.position += Vector3.up * (parent.position.y - bounds.min.y + 0.04f);
        }

        return true;
    }

    /// <summary>Empty pivot for rolling logs — mesh is parented here, not to the moving root.</summary>
    public static Transform CreateLogRollPivot(Transform logRoot)
    {
        var pivotGo = new GameObject("LogRollVisual");
        pivotGo.transform.SetParent(logRoot, false);
        pivotGo.transform.localPosition = Vector3.zero;
        pivotGo.transform.localRotation = Quaternion.identity;
        return pivotGo.transform;
    }

    /// <summary>After the log mesh is grounded, offset pivot so X-axis spin keeps the log on the ground.</summary>
    public static void AlignRollingLogPivot(Transform rollPivot)
    {
        if (rollPivot == null) return;

        Transform logRoot = rollPivot.parent;
        float groundY = logRoot != null ? logRoot.position.y : rollPivot.position.y;
        Bounds b = WorldBounds(rollPivot.gameObject);
        float radius = Mathf.Max(0.28f, b.center.y - b.min.y);

        for (int i = 0; i < rollPivot.childCount; i++)
            rollPivot.GetChild(i).position += Vector3.down * radius;

        rollPivot.localPosition = new Vector3(0f, radius, 0f);
        rollPivot.localRotation = Quaternion.identity;
    }

    public static void ScatterRoadside(Transform parent, float startZ, float endZ, float groundY, int count)
    {
        if (parent == null || count <= 0) return;

        RunnerFoliageProfile.Settings foliage = RunnerFoliageProfile.Current;

        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.37f) / count;
            float z = Mathf.Lerp(startZ, endZ, t);
            bool left = (i % 2) == 0;
            float roll = Hash01(i * 23 + 11);
            if (roll >= foliage.RoadCactusEnd && roll < foliage.RoadRockEnd) continue;

            bool greenDeco = roll >= foliage.RoadRockEnd;
            float x = SampleRoadsideX(left, Hash01(i * 17 + 3), greenDeco);

            GameObject deco = new GameObject(left ? "NatureDeco_L" : "NatureDeco_R");
            deco.transform.SetParent(parent, false);
            deco.transform.position = new Vector3(x, groundY, z);

            if (roll < foliage.RoadCactusEnd) AttachCactus(deco.transform, 2.65f + Hash01(i) * 1.55f);
            else if (roll < foliage.RoadBushEnd) AttachBush(deco.transform, 1.3f + Hash01(i + 8) * 1.0f);
            else if (roll < foliage.RoadTreeEnd) AttachTree(deco.transform, 7f + Hash01(i + 2) * 3.8f);
            else ScatterGrassPatch(deco.transform, 2 + (int)(Hash01(i + 5) * 3));

            if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName) && i % 2 == 0)
                AttachFlower(deco.transform, 0.85f + Hash01(i + 19) * 0.7f, new Vector3(0.55f, 0f, 0.4f));
        }
    }

    public static void ScatterFlowers(Transform parent, float startZ, float endZ, float groundY, int count)
    {
        if (parent == null || count <= 0 || endZ <= startZ) return;

        Transform root = EnsureSideFoliageRoot(parent);
        int seed = Mathf.RoundToInt(startZ);
        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.28f) / count;
            float z = Mathf.Lerp(startZ, endZ, t) + (Hash01(i * 41 + 7 + seed) - 0.5f) * 6f;
            bool left = i % 2 == 0;
            float x = SampleNearShoulderX(left, Hash01(i * 19 + 5 + seed), 0.05f, 0.55f);

            GameObject cluster = new GameObject(left ? $"FlowerCluster_L_{i}" : $"FlowerCluster_R_{i}");
            cluster.transform.SetParent(root, false);
            cluster.transform.position = new Vector3(x, groundY, z);

            int blooms = 4 + (int)(Hash01(i * 13 + seed) * 4);
            int variant = (int)(Hash01(i * 7 + seed) * FlowerNames.Length);
            for (int b = 0; b < blooms; b++)
            {
                Vector3 local = new Vector3(
                    (Hash01(i * 29 + b * 11 + seed) - 0.5f) * 2.6f,
                    0f,
                    (Hash01(i * 17 + b * 23 + seed) - 0.5f) * 2.6f);
                // Mostly one kind per cluster with the odd different bloom mixed in.
                int kind = b % 3 == 2 ? variant + 1 + b : variant;
                AttachFlower(cluster.transform, 1.0f + Hash01(i + b * 5 + seed) * 0.9f, local, kind);
            }
        }
    }

    /// <summary>Roadside X in the inner scenery shoulder, where low props stay visible beside the lanes.</summary>
    static float SampleNearShoulderX(bool left, float hash01, float minT, float maxT)
    {
        GetRoadsideBand(out float inner, out float outer);
        float mag = Mathf.Lerp(inner, outer, Mathf.Lerp(minT, maxT, hash01));
        return LevelLanes.EnforceRoadsideX(left ? -mag : mag, greenFoliage: false);
    }

    public static void ScatterMudMonsterScenery(Transform parent, float startZ, float endZ, float groundY, int count)
    {
        if (parent == null || count <= 0 || endZ <= startZ) return;

        GameObject prefab = LoadMudMonsterPrefab();
        Transform root = EnsureSideFoliageRoot(parent);
        int seed = Mathf.RoundToInt(startZ);
        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.42f) / count;
            float z = Mathf.Lerp(startZ, endZ, t) + (Hash01(i * 37 + seed) - 0.5f) * 5f;
            bool left = (i + (seed & 1)) % 2 == 0;
            float x = SampleNearShoulderX(left, Hash01(i * 31 + 9 + seed), 0.2f, 0.6f);

            GameObject spot = new GameObject(left ? $"MudMonsterScenery_L_{i}" : $"MudMonsterScenery_R_{i}");
            spot.transform.SetParent(root, false);
            spot.transform.position = new Vector3(x, groundY, z);
            // The model faces local -Z; turn it toward the road and the approaching player.
            spot.transform.rotation = Quaternion.Euler(0f, left ? -40f : 40f, 0f);
            AddMudWallow(spot.transform, 2.4f + Hash01(i * 11 + seed) * 0.8f);

            if (prefab != null)
            {
                GameObject model = Object.Instantiate(prefab, spot.transform);
                model.name = "RPG_MudMonster";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                model.transform.localScale = Vector3.one * (2.15f + Hash01(i) * 0.45f);
                EnsureSceneObjectMaterials(model);
                Collider[] cols = model.GetComponentsInChildren<Collider>(true);
                for (int c = 0; c < cols.Length; c++)
                    if (cols[c] != null) cols[c].enabled = false;
            }
            else
            {
                AttachBush(spot.transform, 1.4f);
            }
        }
    }

    static readonly Color MudWallowColor = new Color(0.30f, 0.19f, 0.08f);

    static void AddMudWallow(Transform parent, float diameter)
    {
        GameObject mud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mud.name = "MudWallow";
        mud.transform.SetParent(parent, false);
        mud.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        mud.transform.localScale = new Vector3(diameter, 0.12f, diameter * 0.8f);
        Collider col = mud.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        Renderer rend = mud.GetComponent<Renderer>();
        if (rend == null) return;
        Material mat = rend.material;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", MudWallowColor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", MudWallowColor);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.55f);
    }

    static GameObject LoadMudMonsterPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Monsters/SlimePBR");
#if UNITY_EDITOR
        if (prefab == null)
        {
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/RPG Monster DUO PBR Polyart/Prefabs/PBRDefault/SlimePBR.prefab");
        }
#endif
        return prefab;
    }

    /// <summary>Extra trees and grass clumps along the run, kept off lanes.</summary>
    public static void ScatterSideTreesAndGrass(
        Transform parent,
        float startZ,
        float endZ,
        float groundY,
        int treeCount,
        int grassPatchCount)
    {
        if (parent == null || endZ <= startZ) return;

        RunnerFoliageProfile.Settings foliage = RunnerFoliageProfile.Current;
        treeCount = Mathf.Max(0, Mathf.RoundToInt(treeCount * foliage.SideTreeCountScale));
        grassPatchCount = Mathf.Max(0, Mathf.RoundToInt(grassPatchCount * foliage.GrassPatchCountScale));

        Transform foliageRoot = EnsureSideFoliageRoot(parent);
        float span = endZ - startZ;

        for (int i = 0; i < treeCount; i++)
        {
            float t = (i + 0.5f) / treeCount;
            float z = startZ + t * span + (Hash01(i * 29 + 2) - 0.5f) * (span / treeCount) * 0.45f;
            bool left = i % 2 == 0;
            float x = SampleRoadsideX(left, Hash01(i * 13 + 1), greenFoliage: true);

            GameObject spot = new GameObject($"SideTree_{i}");
            spot.transform.SetParent(foliageRoot, false);
            spot.transform.position = new Vector3(x, groundY, z);
            AttachTree(spot.transform, 7.2f + Hash01(i * 7 + 4) * 5.0f);
        }

        for (int p = 0; p < grassPatchCount; p++)
        {
            float t = (p + 0.35f) / grassPatchCount;
            float z = startZ + t * span + (Hash01(p * 23 + 9) - 0.5f) * (span / grassPatchCount) * 0.5f;
            bool left = p % 2 == 0;
            float x = SampleRoadsideX(left, Hash01(p * 19 + 5), greenFoliage: true);

            GameObject patch = new GameObject($"GrassPatch_{p}");
            patch.transform.SetParent(foliageRoot, false);
            patch.transform.position = new Vector3(x, groundY, z);
            ScatterGrassPatch(patch.transform, 3 + (int)(Hash01(p * 11) * 3));
        }
    }

    static void ScatterGrassPatch(Transform parent, int bladeCount)
    {
        int maxBlades = RunnerFoliageProfile.Current.MaxGrassBladesPerPatch;
        bladeCount = Mathf.Clamp(bladeCount, 1, maxBlades);
        for (int b = 0; b < bladeCount; b++)
        {
            Vector3 local = new Vector3(
                (Hash01(parent.GetInstanceID() + b * 31) - 0.5f) * 1.15f,
                0f,
                (Hash01(parent.GetInstanceID() + b * 37 + 4) - 0.5f) * 1.15f);
            AttachGrass(parent, 0.58f + Hash01(b * 43 + parent.GetInstanceID()) * 0.82f, local);
            if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName) && b % 2 == 0)
            {
                Vector3 flowerLocal = local + new Vector3(0.35f, 0f, -0.28f);
                AttachFlower(parent, 0.62f + Hash01(b * 19 + parent.GetInstanceID()) * 0.55f, flowerLocal);
            }
        }
    }

    static Transform EnsureSideFoliageRoot(Transform parent)
    {
        Transform existing = parent.Find("SideFoliage");
        if (existing != null) return existing;

        var go = new GameObject("SideFoliage");
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void GetRoadsideBand(out float inner, out float outer)
    {
        LevelLanes.GetRoadsideSceneryBand(out inner, out outer);
    }

    static float SampleBoulderWallX(float hash01)
    {
        LevelLanes.GetBoulderWallBand(out float inner, out float outer);
        return Mathf.Lerp(inner, outer, hash01);
    }

    static float SampleRoadsideX(bool left, float hash01, bool greenFoliage)
    {
        float inner, outer;
        if (greenFoliage)
            LevelLanes.GetGreenFoliageBand(out inner, out outer);
        else
            GetRoadsideBand(out inner, out outer);

        float t = greenFoliage
            ? Mathf.Lerp(LevelLanes.GreenBandInnerBias, 1f, hash01)
            : Mathf.Lerp(0.55f, 1f, hash01);
        float mag = Mathf.Lerp(inner, outer, t);
        return LevelLanes.EnforceRoadsideX(left ? -mag : mag, greenFoliage);
    }

    /// <summary>Dense bushes/grass/rocks in the shoulder strip to close visible gaps beside lanes.</summary>
    public static void FillRoadsideShoulderGaps(Transform parent, float startZ, float endZ, float groundY)
    {
        if (parent == null || endZ <= startZ) return;

        RunnerFoliageProfile.Settings foliage = RunnerFoliageProfile.Current;
        LevelLanes.GetRoadsideSceneryBand(out float shoulderInner, out float shoulderOuter);
        shoulderOuter = shoulderInner + 5.5f;

        Transform root = EnsureSideFoliageRoot(parent);
        Transform shoulder = root.Find("ShoulderFill");
        if (shoulder == null)
        {
            var go = new GameObject("ShoulderFill");
            go.transform.SetParent(root, false);
            shoulder = go.transform;
        }
        else
        {
            for (int c = shoulder.childCount - 1; c >= 0; c--)
                Object.Destroy(shoulder.GetChild(c).gameObject);
        }

        float span = endZ - startZ;
        // Wider spacing keeps the roadside full without instantiating hundreds
        // of redundant props during every level transition.
        float step = SceneCatalog.IsLevel3(SceneCatalog.ActiveName) ? 24f : 21f;
        int steps = Mathf.Max(6, Mathf.CeilToInt(span / step));
        int index = 0;

        for (int s = 0; s < steps; s++)
        {
            float z = startZ + (s + 0.5f) / steps * span + (Hash01(s * 53 + 9) - 0.5f) * step * 0.35f;
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                float roll = Hash01(index * 23 + 11);
                bool greenDeco = roll < foliage.ShoulderBushEnd || roll >= foliage.ShoulderRockEnd;

                float x = greenDeco
                    ? SampleRoadsideX(left, Hash01(index * 17 + 3), greenFoliage: true)
                    : SampleRoadsideX(left, Hash01(index * 19 + 5), greenFoliage: false);

                GameObject spot = new GameObject(left ? $"Shoulder_L_{index}" : $"Shoulder_R_{index}");
                spot.transform.SetParent(shoulder, false);
                spot.transform.position = new Vector3(x, groundY, z);

                if (roll < foliage.ShoulderGrassPatchEnd)
                    ScatterGrassPatch(spot.transform, 3 + (int)(Hash01(index + 2) * 4));
                else if (roll < foliage.ShoulderBushEnd)
                    AttachBush(spot.transform, 0.85f + Hash01(index + 4) * 0.7f);
                else if (roll < foliage.ShoulderRockEnd)
                {
                    float mag = SampleBoulderWallX(Hash01(index + 6));
                    spot.transform.position = new Vector3(left ? -mag : mag, groundY, spot.transform.position.z);
                    AttachRock(spot.transform, 0.45f + Hash01(index + 6) * 0.5f);
                }
                else if (roll < foliage.ShoulderSingleGrassEnd)
                    AttachGrass(spot.transform, 0.4f + Hash01(index + 8) * 0.45f);
                else
                    AttachTree(spot.transform, 3.8f + Hash01(index + 1) * 2.2f);

                index++;
            }
        }
    }

    public static void DressSceneObjects()
    {
        LevelLanes.ConfigureForActiveScene();
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend == null) continue;
            Transform t = rend.transform;
            if (t.GetComponentInParent<Canvas>() != null) continue;
            if (IsGameplayLayout(t)) continue;

            string n = t.gameObject.name;
            if (n.StartsWith("Cactus"))
                ReplaceRenderer(t, Kind.Cactus);
            else if (n.StartsWith("Rock") || n.StartsWith("Cliff"))
            {
                if (IsTallRoadsideBoulderName(n)) continue;
                if (Mathf.Abs(t.position.x) <= LevelLanes.PathRockClearAbsX()) continue;
                ReplaceRenderer(t, Kind.Rock);
            }
            else if (n.StartsWith("Tree") || n.StartsWith("DeadTree"))
                ReplaceRenderer(t, Kind.Tree);
        }

        ClearRunnerSceneryColliders();
        MovePathRocksFarOut();
        RoadsideOverlapResolver.Resolve();
    }

    /// <summary>Moves or hides scene rocks/cliffs that sit in the run corridor (keeps water/pickups clear).</summary>
    public static void MovePathRocksFarOut()
    {
        LevelLanes.ConfigureForActiveScene();
        float clearAbsX = LevelLanes.PathRockClearAbsX();
        LevelLanes.GetBoulderWallBand(out float targetInner, out float targetOuter);

        var moved = new System.Collections.Generic.HashSet<int>();
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (t == null) continue;
            if (IsGameplayLayout(t)) continue;
            if (t.GetComponentInParent<Level1Obstacle>() != null) continue;
            if (t.GetComponentInParent<Level2Obstacle>() != null) continue;
            if (t.GetComponentInParent<Level3Obstacle>() != null) continue;
            if (t.GetComponentInParent<Canvas>() != null) continue;
            if (t.GetComponentInParent<Level1WaterPoolPickup>() != null) continue;
            if (t.GetComponentInParent<Level1CactusPickup>() != null) continue;

            string n = t.gameObject.name;
            if (!IsPathRockName(n)) continue;
            Transform rockRoot = GetSceneryRockRoot(t);
            if (rockRoot == null) continue;

            int id = rockRoot.GetInstanceID();
            if (moved.Contains(id)) continue;

            float absX = Mathf.Abs(rockRoot.position.x);
            float maxFootprint = GetHorizontalFootprint(rockRoot.gameObject);
            if (absX - maxFootprint * 0.45f > clearAbsX) continue;

            moved.Add(id);
            float targetX = Mathf.Lerp(targetInner, targetOuter - 0.45f, Hash01(id * 37 + 11));
            targetX = Mathf.Clamp(Mathf.Max(targetX, clearAbsX + 2.5f), targetInner, targetOuter - 0.45f);
            float sign = rockRoot.position.x >= 0f ? 1f : -1f;
            if (Mathf.Abs(rockRoot.position.x) < 0.01f)
                sign = (id % 2 == 0) ? 1f : -1f;

            Vector3 p = rockRoot.position;
            p.x = sign * targetX;
            rockRoot.position = p;
        }
    }

    static float GetHorizontalFootprint(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return 1f;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
        {
            if (rends[i] != null) b.Encapsulate(rends[i].bounds);
        }

        return Mathf.Max(b.size.x, b.size.z);
    }

    /// <summary>Legacy name — relocates path rocks to the far shoulder.</summary>
    public static void DeactivatePathBlockingRocks() => MovePathRocksFarOut();

    static bool IsPathRockName(string n)
    {
        if (string.IsNullOrEmpty(n)) return false;
        if (n.StartsWith("Rock_A") || n.StartsWith("Rock_") || n == "Rock") return true;
        if (n.StartsWith("Cliff_") || n.StartsWith("Cliff")) return true;
        if (n.StartsWith("HardRock") || n.StartsWith("SoftRock")) return true;
        if (n.StartsWith("RockCluster") || n.StartsWith("BoulderWall")) return false;
        return false;
    }

    static Transform GetSceneryRockRoot(Transform t)
    {
        Transform current = t;
        Transform best = IsPathRockName(t.gameObject.name) ? t : null;
        while (current.parent != null && !IsGameplayLayout(current.parent))
        {
            current = current.parent;
            if (IsPathRockName(current.gameObject.name))
                best = current;
        }

        return best;
    }

    /// <summary>Scene rocks/cliffs are visual-only — solid colliders block lane movement.</summary>
    public static void ClearRunnerSceneryColliders()
    {
        Collider[] colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col == null || col.isTrigger) continue;
            if (col.CompareTag("Ground") || col.CompareTag("Player")) continue;
            if (IsGameplayLayout(col.transform)) continue;
            if (col.GetComponentInParent<Level1Obstacle>() != null) continue;
            if (col.GetComponentInParent<Level2Obstacle>() != null) continue;
            if (col.GetComponentInParent<Level3Obstacle>() != null) continue;
            if (col.GetComponentInParent<BushlandHazard>() != null) continue;

            Transform t = col.transform;
            if (!IsSceneryColliderRoot(t)) continue;

            Object.Destroy(col);
        }

        RoadsideBoulderScatter.EnsureVisualOnly();
    }

    enum Kind { Cactus, Rock, Tree }

    static bool IsTallRoadsideBoulderName(string n)
    {
        if (string.IsNullOrEmpty(n)) return false;
        if (n.StartsWith("Rock_A")) return true;
        if (n.StartsWith("Cliff_")) return true;
        if (n.StartsWith("BoulderWall")) return true;
        return false;
    }

    static bool AttachPlant(Transform parent, string prefabName, float targetHeight, float yaw = float.NaN)
    {
        GameObject go = Spawn(parent, prefabName, Vector3.zero);
        if (go == null) return false;
        FitHeight(go.transform, targetHeight);
        PlantOnGround(go.transform);
        if (float.IsNaN(yaw)) RandomYaw(go.transform, parent);
        else go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        return true;
    }

    static GameObject Spawn(Transform parent, string prefabName, Vector3 localPos, bool applyPlantTint = true)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab == null) return null;

        GameObject go = Object.Instantiate(prefab, parent, false);
        go.name = prefabName;
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        StripColliders(go);
        EnsureUrpMaterials(go);
        if (applyPlantTint)
            RunnerFoliageProfile.ApplyPlantTint(go);
        return go;
    }

    static GameObject LoadPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (Cache.TryGetValue(name, out GameObject cached) && cached != null) return cached;

        GameObject prefab = Resources.Load<GameObject>(ResourcesFolder + name);
#if UNITY_EDITOR
        if (prefab == null)
        {
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{name}.prefab");
        }
#endif
        Cache[name] = prefab;
        return prefab;
    }

    static void StripColliders(GameObject go)
    {
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] == null) continue;
            cols[i].enabled = false;
            Object.Destroy(cols[i]);
        }
    }

    static void FitHeight(Transform t, float targetHeight)
    {
        Bounds b = WorldBounds(t.gameObject);
        float h = b.size.y;
        if (h < 0.01f || targetHeight <= 0f) return;
        t.localScale *= targetHeight / h;
    }

    static void PlantOnGround(Transform t)
    {
        if (t.parent == null) return;
        Bounds b = WorldBounds(t.gameObject);
        t.position += Vector3.up * (t.parent.position.y - b.min.y);
    }

    static void RandomYaw(Transform t, Transform seed)
    {
        t.localRotation = Quaternion.Euler(0f, Hash01(seed) * 360f, 0f);
    }

    static void AlignLongestAxis(Transform t, Vector3 worldAxis)
    {
        Bounds b = WorldBounds(t.gameObject);
        Vector3 size = b.size;
        Vector3 localLongest = Vector3.up;
        if (size.x >= size.y && size.x >= size.z) localLongest = Vector3.right;
        else if (size.z >= size.y) localLongest = Vector3.forward;

        Vector3 current = t.TransformDirection(localLongest);
        t.rotation = Quaternion.FromToRotation(current, worldAxis) * t.rotation;
    }

    static Bounds WorldBounds(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        bool any = false;
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] == null || !rends[i].enabled) continue;
            if (!any) { b = rends[i].bounds; any = true; }
            else b.Encapsulate(rends[i].bounds);
        }
        return b;
    }

    static void ReplaceRenderer(Transform original, Kind kind)
    {
        if (original.Find("NaturePackSwap") != null) return;

        Renderer[] rends = original.GetComponentsInChildren<Renderer>();
        Bounds b = WorldBounds(original.gameObject);
        float height = Mathf.Max(0.8f, b.size.y);

        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] != null) rends[i].enabled = false;
        }

        GameObject holder = new GameObject("NaturePackSwap");
        holder.transform.SetParent(original, false);
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;
        holder.transform.localScale = Vector3.one;

        bool ok = kind switch
        {
            Kind.Rock => AttachRock(holder.transform, height),
            Kind.Tree => AttachTree(holder.transform, height),
            _ => AttachCactus(holder.transform, height * 0.8f)
        };

        if (!ok)
        {
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] != null) rends[i].enabled = true;
            }
            Object.Destroy(holder);
            return;
        }

        StripColliders(original.gameObject);
    }

    static bool IsSceneryColliderRoot(Transform t)
    {
        while (t != null)
        {
            string n = t.name;
            if (n == "Scene1" || n.StartsWith("NatureDeco_")) return true;
            if (n.StartsWith("Rock_") || n.StartsWith("Rock ") || n == "Rock" || n.StartsWith("Cliff"))
                return true;
            if (n.StartsWith("House_House") || n.StartsWith("VillageHouse_"))
                return true;
            if (n.StartsWith("Cactus_") || n.StartsWith("Tree_") || n.StartsWith("DeadTree"))
                return true;
            if (n.StartsWith("Grass") || n.StartsWith("GrassPatch_") || n.StartsWith("SideTree_"))
                return true;
            if (n == "RoadsideBoulderWall" || n.StartsWith("BoulderWall_"))
                return true;
            if (n == "MapleSideTrees" || n.StartsWith("Maple_"))
                return true;
            if (n == "SideFoliage" || n == "ShoulderFill" || n.StartsWith("Shoulder_"))
                return true;
            if (n == "Level1_Layout" || n == "Level2_Layout" || n == "Level3_Layout") return false;
            t = t.parent;
        }

        return false;
    }

    static void ClampFootprint(Transform t, float maxWidth)
    {
        if (t == null || maxWidth <= 0f) return;
        Bounds b = WorldBounds(t.gameObject);
        float footprint = Mathf.Max(b.size.x, b.size.z);
        if (footprint > maxWidth && footprint > 0.01f)
            t.localScale *= maxWidth / footprint;
    }

    static bool IsGameplayLayout(Transform t)
    {
        while (t != null)
        {
            string n = t.name;
            if (n == "Level1_Layout" || n == "Level2_Layout" || n == "Level3_Layout") return true;
            if (n.StartsWith("CactusWater") || n.StartsWith("RollingLog") || n.StartsWith("RockCluster")) return true;
            if (n.StartsWith("CactusWall") || n.StartsWith("LowCactus") || n == "Cactus") return true;
            t = t.parent;
        }
        return false;
    }

    static Material urpFallback;

    static void EnsureUrpMaterials(GameObject go)
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null) return;

        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            Renderer rend = rends[i];
            if (rend == null) continue;
            Material[] mats = rend.sharedMaterials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                Material src = mats[m];
                if (src == null)
                {
                    mats[m] = MakeMissingMaterialFallback(rend.name, lit);
                    changed = true;
                    continue;
                }
                if (src.shader == lit) continue;
                mats[m] = MakeUrpCopy(src, lit);
                changed = true;
            }
            if (changed) rend.sharedMaterials = mats;
        }
    }

    static Material MakeUrpCopy(Material src, Shader lit)
    {
        if (urpFallback != null && urpFallback.GetTexture("_BaseMap") == GetAlbedo(src))
            return urpFallback;

        Material copy = new Material(lit);
        Texture albedo = GetAlbedo(src);
        Color color = src.HasProperty("_Color") ? src.GetColor("_Color") : Color.white;
        if (src.HasProperty("_BaseColor")) color = src.GetColor("_BaseColor");
        if (copy.HasProperty("_BaseMap") && albedo != null) copy.SetTexture("_BaseMap", albedo);
        if (copy.HasProperty("_BaseColor")) copy.SetColor("_BaseColor", color);
        if (copy.HasProperty("_Smoothness")) copy.SetFloat("_Smoothness", 0.18f);
        urpFallback = copy;
        return copy;
    }

    static Material MakeMissingMaterialFallback(string rendererName, Shader lit)
    {
        string key = (rendererName ?? "").ToLowerInvariant();
        bool wood = key.Contains("trunk") || key.Contains("bark")
                    || key.Contains("branch") || key.Contains("stem");
        Material material = new Material(lit);
        Color color = wood
            ? new Color(0.31f, 0.18f, 0.08f)
            : new Color(0.42f, 0.48f, 0.16f);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
        return material;
    }

    static Texture GetAlbedo(Material src)
    {
        if (src.HasProperty("_BaseMap")) return src.GetTexture("_BaseMap");
        if (src.HasProperty("_MainTex")) return src.GetTexture("_MainTex");
        return src.mainTexture;
    }

    static string Pick(string[] names, Transform seed)
    {
        int idx = Mathf.Abs((seed.position.x * 10f + seed.position.z * 3f).GetHashCode()) % names.Length;
        return names[idx];
    }

    static string PickBush(Transform seed)
    {
        float thornBias = RunnerFoliageProfile.Current.ThornBushBias;
        float roll = Hash01(seed);
        if (roll < thornBias)
            return Pick(ThornBushNames, seed);
        return Pick(BushNames, seed);
    }

    static float Hash01(Transform seed) => Hash01(seed.GetInstanceID());

    static float Hash01(int value)
    {
        uint x = (uint)value;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        return (x & 0xFFFF) / 65535f;
    }
}
