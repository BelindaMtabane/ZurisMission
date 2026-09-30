using UnityEngine;

/// <summary>
/// Small ground-level dam berms with AQUIS water in the basin (springs, pools, wells).
/// </summary>
public static class AquisWaterVisuals
{
    const string ResourcesPool = "Water/PoolWater";
    const string ResourcesRiver = "Water/RiverWater";
    const float UnityPlaneSize = 10f;

    static readonly Color BermSand = new Color(0.74f, 0.58f, 0.36f);
    static readonly Color BermDark = new Color(0.52f, 0.38f, 0.24f);
    static readonly Color BasinMud = new Color(0.58f, 0.44f, 0.28f);
    static readonly Color FallbackShallow = new Color(0.15f, 0.52f, 0.88f);
    static readonly Color FallbackDeep = new Color(0.08f, 0.32f, 0.68f);

    static Material _poolMat;
    static Material _riverMat;

    public static bool IsAvailable => PoolMaterial != null;

    static Material PoolMaterial
    {
        get
        {
            if (_poolMat == null)
                _poolMat = Resources.Load<Material>(ResourcesPool);
            return _poolMat;
        }
    }

    static Material RiverMaterial
    {
        get
        {
            if (_riverMat == null)
                _riverMat = Resources.Load<Material>(ResourcesRiver);
            return _riverMat;
        }
    }

    public static void BuildLevel1SpringVisuals(Transform root, Color shallow, Color deep)
    {
        BuildGroundDamVisuals(root, shallow, deep);
    }

    public static void BuildLevel2PoolVisuals(Transform root, Color shallow, Color deep)
    {
        BuildGroundDamVisuals(root, shallow, deep);
    }

    public static void BuildGroundDamVisuals(Transform pickupRoot, Color shallow, Color deep)
    {
        if (pickupRoot == null) return;
        if (pickupRoot.Find("GroundDam") != null) return;

        GameObject damRoot = new GameObject("GroundDam");
        damRoot.transform.SetParent(pickupRoot, false);
        damRoot.transform.localPosition = Vector3.zero;
        damRoot.transform.localRotation = Quaternion.identity;
        PopulateDamGeometry(damRoot.transform, shallow, deep, 1f);
    }

    static void PopulateDamGeometry(Transform damRoot, Color shallow, Color deep, float sizeScale)
    {
        float basinW = RunnerVisualScale.F(3.65f * sizeScale);
        float basinD = RunnerVisualScale.F(2.75f * sizeScale);
        float wallH = RunnerVisualScale.F(0.52f * sizeScale);
        float wallT = RunnerVisualScale.F(0.32f * sizeScale);
        float spillH = RunnerVisualScale.F(0.22f * sizeScale);
        float halfW = basinW * 0.5f;
        float halfD = basinD * 0.5f;
        float wallCenterY = wallH * 0.5f;
        float spillCenterY = spillH * 0.5f;

        AddBermCube(damRoot, "DamWall_Back",
            new Vector3(0f, wallCenterY, -halfD + wallT * 0.5f),
            new Vector3(basinW + wallT * 2f, wallH, wallT), BermDark);
        AddBermCube(damRoot, "DamWall_Left",
            new Vector3(-halfW + wallT * 0.5f, wallCenterY, 0f),
            new Vector3(wallT, wallH, basinD + wallT * 0.5f), BermSand);
        AddBermCube(damRoot, "DamWall_Right",
            new Vector3(halfW - wallT * 0.5f, wallCenterY, 0f),
            new Vector3(wallT, wallH, basinD + wallT * 0.5f), BermSand);
        AddBermCube(damRoot, "DamWall_Spill",
            new Vector3(0f, spillCenterY, halfD - wallT * 0.5f),
            new Vector3(basinW * 0.58f, spillH, wallT), BermSand);

        AddBermCube(damRoot, "DamCap_Back",
            new Vector3(0f, wallH + RunnerVisualScale.F(0.04f), -halfD + wallT * 0.5f),
            new Vector3(basinW + wallT * 1.6f, RunnerVisualScale.F(0.07f), wallT * 0.85f), BermDark);

        float floorY = RunnerVisualScale.F(0.015f);
        AddBasinFloor(damRoot, new Vector3(0f, floorY, -RunnerVisualScale.F(0.06f)), basinW * 0.92f, basinD * 0.82f);

        float waterZ = -RunnerVisualScale.F(0.08f);
        float waterY = RunnerVisualScale.F(0.09f);
        AddWaterPlane(damRoot, "PoolBase", new Vector3(0f, waterY * 0.55f, waterZ),
            basinW * 0.86f, basinD * 0.72f, deep, true);
        AddWaterPlane(damRoot, "PoolSurface", new Vector3(0f, waterY, waterZ),
            basinW * 0.82f, basinD * 0.68f, shallow, false);

        AttachSpringEffects(damRoot, sizeScale, basinW, basinD);
        AddGroundSpillPatches(damRoot, basinW, basinD, sizeScale);
        ScatterDamGrass(damRoot, basinW, basinD, sizeScale);
        ScatterDamFlowers(damRoot, basinW, basinD, sizeScale);
    }

    static void AttachSpringEffects(Transform damRoot, float sizeScale, float basinW, float basinD)
    {
        if (damRoot.GetComponentInChildren<SpringWaterDamFx>(true) != null) return;
        GameObject fxHost = new GameObject("SpringWaterFx");
        fxHost.transform.SetParent(damRoot, false);
        fxHost.transform.localPosition = Vector3.zero;
        SpringWaterDamFx fx = fxHost.AddComponent<SpringWaterDamFx>();
        fx.Configure(sizeScale, basinW, basinD);
    }

    static void AddGroundSpillPatches(Transform damRoot, float basinW, float basinD, float sizeScale)
    {
        if (damRoot.Find("GroundSpill") != null) return;

        GameObject spillRoot = new GameObject("GroundSpill");
        spillRoot.transform.SetParent(damRoot, false);
        spillRoot.transform.localPosition = Vector3.zero;

        Material trailSource = NamuWaterFxLibrary.LoadTrailMaterial();
        float spillStartZ = basinD * 0.32f;
        float trailLen = basinD * 0.42f + basinW * 0.08f;
        float trailW = basinW * 0.38f;
        int segments = 3;

        for (int i = 0; i < segments; i++)
        {
            float t = (i + 0.5f) / segments;
            float segLen = trailLen / segments;
            float z = spillStartZ + segLen * i + segLen * 0.5f;
            float width = Mathf.Lerp(trailW, trailW * 0.55f, t);

            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Plane);
            strip.name = $"SpillTrail_{i}";
            strip.transform.SetParent(spillRoot.transform, false);
            strip.transform.localPosition = new Vector3(0f, RunnerVisualScale.F(0.012f), z);
            strip.transform.localRotation = Quaternion.identity;
            SetPlaneFootprint(strip.transform, width, segLen * 1.05f);

            Collider col = strip.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Renderer rend = strip.GetComponent<Renderer>();
            if (rend == null) continue;
            if (trailSource != null)
            {
                Material wet = new Material(trailSource);
                if (wet.HasProperty("_Scroll")) wet.SetVector("_Scroll", new Vector4(0f, 0.35f + i * 0.08f, 0f, 0f));
                if (wet.HasProperty("_Color")) wet.SetColor("_Color", new Color(0.55f, 0.82f, 1f, 0.72f));
                rend.sharedMaterial = wet;
            }
            else
                rend.material.color = new Color(0.35f, 0.62f, 0.92f, 0.55f);

            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }

        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rim.name = "SpillPool";
        rim.transform.SetParent(spillRoot.transform, false);
        rim.transform.localPosition = new Vector3(0f, RunnerVisualScale.F(0.01f), spillStartZ + trailLen * 0.92f);
        SetPlaneFootprint(rim.transform, trailW * 0.75f, trailW * 0.55f);
        Collider rimCol = rim.GetComponent<Collider>();
        if (rimCol != null) rimCol.enabled = false;
        Renderer rimR = rim.GetComponent<Renderer>();
        if (rimR != null)
        {
            if (trailSource != null)
            {
                Material puddle = new Material(trailSource);
                if (puddle.HasProperty("_Color")) puddle.SetColor("_Color", new Color(0.48f, 0.78f, 1f, 0.65f));
                rimR.sharedMaterial = puddle;
            }
            else
                rimR.material.color = new Color(0.28f, 0.55f, 0.88f, 0.5f);
            rimR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    static void ScatterDamGrass(Transform damRoot, float basinW, float basinD, float sizeScale)
    {
        if (damRoot.Find("DamGrass") != null) return;

        GameObject grassRoot = new GameObject("DamGrass");
        grassRoot.transform.SetParent(damRoot, false);
        grassRoot.transform.localPosition = Vector3.zero;

        int count = Mathf.Max(4, Mathf.RoundToInt((14 + sizeScale * 10) * RunnerFoliageProfile.Current.DamGrassCountScale));
        float padX = basinW * 0.52f + RunnerVisualScale.F(0.35f);
        float padZ = basinD * 0.46f + RunnerVisualScale.F(0.35f);

        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * Mathf.PI * 2f;
            float wobble = Mathf.Sin(i * 2.17f) * 0.22f;
            Vector3 local = new Vector3(
                Mathf.Cos(angle) * (padX + wobble),
                0f,
                Mathf.Sin(angle) * (padZ + wobble * 0.6f) - basinD * 0.05f);

            if (Mathf.Abs(local.x) < basinW * 0.28f && local.z > basinD * 0.05f)
                continue;

            float height = 0.68f + Mathf.Abs(Mathf.Sin(i * 1.73f)) * 0.72f;
            NaturePackVisuals.AttachGrass(grassRoot.transform, height * sizeScale, local);
        }
    }

    static void ScatterDamFlowers(Transform damRoot, float basinW, float basinD, float sizeScale)
    {
        if (damRoot.Find("DamFlowers") != null) return;

        GameObject flowerRoot = new GameObject("DamFlowers");
        flowerRoot.transform.SetParent(damRoot, false);
        flowerRoot.transform.localPosition = Vector3.zero;

        int count = SceneCatalog.IsLevel3(SceneCatalog.ActiveName) ? 10
            : SceneCatalog.IsLevel2(SceneCatalog.ActiveName) ? 7 : 3;
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * Mathf.PI * 2f + 0.35f;
            float radiusX = basinW * (0.53f + (i % 2) * 0.08f);
            float radiusZ = basinD * (0.48f + ((i + 1) % 2) * 0.07f);
            Vector3 local = new Vector3(
                Mathf.Cos(angle) * radiusX,
                0f,
                Mathf.Sin(angle) * radiusZ - basinD * 0.04f);
            NaturePackVisuals.AttachFlower(
                flowerRoot.transform,
                (0.58f + (i % 3) * 0.16f) * sizeScale,
                local);
        }
    }

    public static void DressSceneWater()
    {
        UpgradeRuntimeWaterPickups();
        DressWellsAndLegacyMeshes();
        DressRivers();
    }

    static void UpgradeRuntimeWaterPickups()
    {
        Level1WaterPoolPickup[] l1 = Object.FindObjectsByType<Level1WaterPoolPickup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < l1.Length; i++)
        {
            if (l1[i] == null) continue;
            EnsurePickupDam(l1[i].transform);
        }

        Level2WaterPoolPickup[] l2 = Object.FindObjectsByType<Level2WaterPoolPickup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < l2.Length; i++)
        {
            if (l2[i] == null) continue;
            EnsurePickupDam(l2[i].transform);
        }
    }

    static void EnsurePickupDam(Transform pickupRoot)
    {
        Transform dam = pickupRoot.Find("GroundDam");
        if (dam != null && !DamFxIsCurrent(dam))
            Object.Destroy(dam.gameObject);

        if (pickupRoot.Find("GroundDam") == null)
            BuildGroundDamVisuals(pickupRoot, FallbackShallow, FallbackDeep);
        HideLegacyPoolChildren(pickupRoot);
    }

    static bool DamFxIsCurrent(Transform dam)
    {
        if (dam == null) return false;
        if (dam.Find("DamGrass") == null || dam.Find("DamFlowers") == null || dam.Find("GroundSpill") == null)
            return false;
        Transform fxHost = dam.Find("SpringWaterFx");
        if (fxHost == null || fxHost.GetComponent<SpringWaterDamFx>() == null) return false;
        if (NamuWaterFxLibrary.IsAvailable)
            return fxHost.Find("ProximitySpringEffects/NamuFx") != null;
        return true;
    }

    static void HideLegacyPoolChildren(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == "GroundDam") continue;
            if (child.name == "PoolSurface" || child.name == "Pool" || child.name == "PoolBase"
                || child.name == "Ripple" || child.name == "Splash")
            {
                if (child.parent == root)
                    child.gameObject.SetActive(false);
            }
        }
    }

    static void DressWellsAndLegacyMeshes()
    {
        Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null || t.name != "WellWaterTop") continue;

            ConvertDiscToFlatPatch(t.gameObject);
            Renderer rend = t.GetComponent<Renderer>();
            if (rend != null) ApplyPoolSurface(rend);

            Transform well = t.parent;
            if (well == null) continue;

            Transform existingDam = well.Find("GroundDam");
            if (existingDam != null)
            {
                if (DamFxIsCurrent(existingDam))
                {
                    t.gameObject.SetActive(false);
                    continue;
                }

                Object.Destroy(existingDam.gameObject);
            }

            GameObject damRoot = new GameObject("GroundDam");
            damRoot.transform.SetParent(well, false);
            float surfaceY = ResolveGroundSurfaceY();
            damRoot.transform.localPosition = new Vector3(0f, surfaceY - well.position.y, 0f);
            damRoot.transform.localRotation = Quaternion.identity;
            PopulateDamGeometry(damRoot.transform, FallbackShallow, FallbackDeep, 0.72f);

            t.gameObject.SetActive(false);
        }

        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend == null) continue;
            if (rend.GetComponentInParent<Canvas>() != null) continue;
            if (rend.transform.Find("GroundDam") != null || rend.GetComponentInParent<Level1WaterPoolPickup>() != null
                || rend.GetComponentInParent<Level2WaterPoolPickup>() != null)
                continue;

            string n = rend.gameObject.name;
            if (n == "PoolSurface" || n == "Pool" || n == "PoolBase")
            {
                ConvertDiscToFlatPatch(rend.gameObject);
                if (n == "PoolBase") ApplyPoolDeep(rend);
                else ApplyPoolSurface(rend);
            }
        }
    }

    static float ResolveGroundSurfaceY()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene.Contains("Level2") || scene.Contains("L2")) return Level2Ground.SurfaceY;
        if (scene.Contains("Level3") || scene.Contains("L3")) return Level3Ground.SurfaceY;
        return Level1Ground.SurfaceY;
    }

    static void DressRivers()
    {
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend == null) continue;
            string n = rend.gameObject.name;
            if (n.StartsWith("River") || n.StartsWith("Borehole_Water"))
                ApplyRiverSurface(rend);
        }
    }

    public static void StyleWaterPoolRoot(GameObject root)
    {
        if (root == null) return;
        if (root.transform.Find("GroundDam") == null)
            BuildGroundDamVisuals(root.transform, FallbackShallow, FallbackDeep);
        HideLegacyPoolChildren(root.transform);
    }

    static void AddBermCube(Transform parent, string name, Vector3 localPos, Vector3 size, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = size;
        Collider col = go.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null) rend.material.color = color;
    }

    static void AddBasinFloor(Transform parent, Vector3 localPos, float widthX, float depthZ)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "DamBasinFloor";
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        SetPlaneFootprint(go.transform, widthX, depthZ);
        Collider col = go.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null) rend.material.color = BasinMud;
    }

    static void AddWaterPlane(Transform parent, string name, Vector3 localPos, float widthX, float depthZ, Color fallback, bool deep)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        SetPlaneFootprint(go.transform, widthX, depthZ);
        Collider col = go.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Renderer rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        if (IsAvailable)
        {
            if (deep) ApplyPoolDeep(rend);
            else ApplySpringSurface(rend);
        }
        else
            rend.material.color = fallback;
    }

    static void SetPlaneFootprint(Transform t, float widthX, float depthZ)
    {
        t.localScale = new Vector3(widthX / UnityPlaneSize, 1f, depthZ / UnityPlaneSize);
    }

    static void ConvertDiscToFlatPatch(GameObject go)
    {
        if (go == null) return;
        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;
        if (mf.sharedMesh.name == "Plane") return;

        Vector3 s = go.transform.localScale;
        float widthX = Mathf.Max(Mathf.Abs(s.x), 0.05f);
        float depthZ = Mathf.Max(Mathf.Abs(s.z), 0.05f);

        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mf.sharedMesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(temp);

        MeshCollider meshCol = go.GetComponent<MeshCollider>();
        if (meshCol != null) Object.Destroy(meshCol);

        SetPlaneFootprint(go.transform, widthX, depthZ);
    }

    static void ApplyPoolSurface(Renderer rend)
    {
        ApplySpringSurface(rend);
    }

    static void ApplySpringSurface(Renderer rend)
    {
        if (rend == null) return;
        if (PoolMaterial != null)
        {
            Material spring = new Material(PoolMaterial);
            if (spring.HasProperty("_RippleStrength")) spring.SetFloat("_RippleStrength", 140f);
            if (spring.HasProperty("_WaveSpeed")) spring.SetFloat("_WaveSpeed", 0.38f);
            if (spring.HasProperty("_SurfaceFoamSpeed")) spring.SetFloat("_SurfaceFoamSpeed", 0.42f);
            if (spring.HasProperty("_WaterAlpha")) spring.SetFloat("_WaterAlpha", 0.94f);
            rend.sharedMaterial = spring;
        }

        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
    }

    static void ApplyPoolDeep(Renderer rend)
    {
        if (rend == null) return;
        if (PoolMaterial == null)
        {
            ApplyPoolSurface(rend);
            return;
        }

        Material deep = new Material(PoolMaterial);
        if (deep.HasProperty("_DeepColor"))
            deep.SetColor("_DeepColor", new Color(0.02f, 0.12f, 0.32f, 1f));
        if (deep.HasProperty("_ShallowColor"))
            deep.SetColor("_ShallowColor", deep.GetColor("_ShallowColor") * 0.5f);

        rend.sharedMaterial = deep;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static void ApplyRiverSurface(Renderer rend)
    {
        if (rend == null) return;
        Material mat = RiverMaterial != null ? RiverMaterial : PoolMaterial;
        if (mat != null)
            rend.sharedMaterial = mat;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
    }
}
