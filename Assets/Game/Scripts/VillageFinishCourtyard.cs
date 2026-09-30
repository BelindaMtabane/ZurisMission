using UnityEngine;

/// <summary>
/// U-shaped village finale at the run end — houses face the road; optional water tank/silo in the center.
/// </summary>
public static class VillageFinishCourtyard
{
    const string ResourcesFolder = "Village/";
    const float HouseScale = 1.58f;
    const float ArmDepth = 34f;
    const float BackDepth = 11f;
    const float HouseSpacingZ = 7.2f;
    const float HouseSpacingX = 7.4f;

    static readonly string[] HouseNames =
    {
        "House1", "House2", "House3", "House4", "House6", "House5"
    };

    const string TankResourceName = "Farm_House_Silo_01";

    public static void Build(Transform parent, float finishZ, float groundY)
    {
        if (parent == null) return;

        LevelLanes.ConfigureForActiveScene();
        float pathCenter = LevelLanes.PathCenterX;
        LevelLanes.GetVillageHouseBand(out float houseBandInner, out _);
        float armX = houseBandInner + 1.25f;

        float openZ = finishZ - 3.5f;
        float backZ = finishZ + BackDepth;
        float armStartZ = openZ - ArmDepth;

        Transform root = parent.Find("VillageFinishCourtyard");
        if (root == null)
        {
            var host = new GameObject("VillageFinishCourtyard");
            host.transform.SetParent(parent, false);
            root = host.transform;
        }
        else
        {
            for (int c = root.childCount - 1; c >= 0; c--)
                Object.Destroy(root.GetChild(c).gameObject);
        }

        int houseIndex = 0;

        int leftCount = Mathf.Max(3, Mathf.CeilToInt(ArmDepth / HouseSpacingZ));
        for (int i = 0; i < leftCount; i++)
        {
            float t = (i + 0.5f) / leftCount;
            float z = Mathf.Lerp(armStartZ, backZ - 2f, t);
            SpawnHouse(root, HouseNames[houseIndex++ % HouseNames.Length], new Vector3(pathCenter - armX, groundY, z), 90f, groundY);
        }

        for (int i = 0; i < leftCount; i++)
        {
            float t = (i + 0.5f) / leftCount;
            float z = Mathf.Lerp(armStartZ, backZ - 2f, t);
            SpawnHouse(root, HouseNames[houseIndex++ % HouseNames.Length], new Vector3(pathCenter + armX, groundY, z), -90f, groundY);
        }

        int backCount = Mathf.Max(4, Mathf.CeilToInt((armX * 2f) / HouseSpacingX));
        for (int i = 0; i < backCount; i++)
        {
            float t = (i + 0.5f) / backCount;
            float x = Mathf.Lerp(pathCenter - armX + 1.2f, pathCenter + armX - 1.2f, t);
            SpawnHouse(root, HouseNames[houseIndex++ % HouseNames.Length], new Vector3(x, groundY, backZ), 180f, groundY);
        }

        if (SceneCatalog.IsLevel1(SceneCatalog.ActiveName))
        {
            float tankZ = finishZ + 4.5f;
            float leftX = LevelLanes.X(0) - 4.8f;
            float rightX = LevelLanes.X(LevelLanes.Count - 1) + 4.8f;
            VillageSideHouses.PlaceWrappedTank(root, leftX, groundY, tankZ, 0, YoyoTankColor.Red);
            VillageSideHouses.PlaceWrappedTank(root, rightX, groundY, tankZ, 1, YoyoTankColor.Red);
        }
        else if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName))
        {
            float sideZ = finishZ + 4.5f;
            float backZTanks = finishZ + 8.2f;
            float leftX = LevelLanes.X(0) - 4.8f;
            float rightX = LevelLanes.X(LevelLanes.Count - 1) + 4.8f;
            VillageSideHouses.PlaceWrappedTank(root, leftX, groundY, sideZ, 0, YoyoTankColor.Green);
            GameObject center = VillageSideHouses.PlaceWrappedTank(root, pathCenter, groundY, backZTanks, 1, YoyoTankColor.Green);
            if (center != null) center.transform.rotation = Quaternion.identity;
            VillageSideHouses.PlaceWrappedTank(root, rightX, groundY, sideZ, 2, YoyoTankColor.Green);
        }
        else
        {
            SpawnCenterTank(root, new Vector3(pathCenter, groundY, finishZ + 4.5f), groundY);
        }
    }

    static void SpawnHouse(Transform root, string houseName, Vector3 position, float yaw, float groundY)
    {
        GameObject prefab = LoadHouse(houseName);
        if (prefab == null) return;

        GameObject instance = Object.Instantiate(prefab, root);
        instance.name = $"FinishHouse_{houseName}_{root.childCount}";
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * HouseScale;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        GroundToSurface(instance.transform, groundY);
        StripColliders(instance);
        NaturePackVisuals.EnsureSceneObjectMaterials(instance);
    }

    static void SpawnCenterTank(Transform root, Vector3 position, float groundY)
    {
        GameObject prefab = LoadTankPrefab();
        if (prefab != null)
        {
            GameObject tank = Object.Instantiate(prefab, root);
            tank.name = "FinishVillageTank";
            tank.transform.position = position;
            tank.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            tank.transform.localScale = Vector3.one * 1.35f;
            GroundToSurface(tank.transform, groundY);
            StripColliders(tank);
            NaturePackVisuals.EnsureSceneObjectMaterials(tank);
            return;
        }

        SpawnFallbackTank(root, position);
    }

    static void SpawnFallbackTank(Transform root, Vector3 position)
    {
        GameObject tank = new GameObject("FinishVillageTank");
        tank.transform.SetParent(root, false);
        tank.transform.position = position;

        Color metal = new Color(0.55f, 0.6f, 0.66f);
        Color water = new Color(0.28f, 0.52f, 0.82f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "TankBody";
        body.transform.SetParent(tank.transform, false);
        body.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        body.transform.localScale = new Vector3(2.4f, 1.35f, 2.4f);
        ApplyColor(body, metal);

        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fill.name = "TankFill";
        fill.transform.SetParent(tank.transform, false);
        fill.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        fill.transform.localScale = new Vector3(2.05f, 0.75f, 2.05f);
        ApplyColor(fill, water);

        StripColliders(tank);
    }

    static void ApplyColor(GameObject go, Color c)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;
        Material m = r.material;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }

    static GameObject LoadHouse(string houseName)
    {
        GameObject prefab = Resources.Load<GameObject>(ResourcesFolder + houseName);
#if UNITY_EDITOR
        if (prefab == null)
        {
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/PolyRonin/Desert Village/Prefabs/{houseName}.prefab");
        }
#endif
        return prefab;
    }

    static GameObject LoadTankPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>(ResourcesFolder + TankResourceName);
#if UNITY_EDITOR
        if (prefab == null)
        {
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Farm House/Farm_House_Silo_01.prefab");
        }
#endif
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

    static void StripColliders(GameObject go)
    {
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] != null) Object.Destroy(cols[i]);
        }
    }
}
