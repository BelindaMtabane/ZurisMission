using UnityEngine;
using UnityEngine.Rendering;

public static class Level2Primitives
{
    public static readonly Color WaterDrop = new Color(0.35f, 0.82f, 1f);
    public static readonly Color WaterDropDeep = new Color(0.12f, 0.52f, 0.92f);
    public static readonly Color WaterGlow = new Color(0.72f, 0.94f, 1f, 0.55f);
    public static readonly Color BaobabTrunk = new Color(0.48f, 0.34f, 0.18f);
    public static readonly Color BaobabBark = new Color(0.38f, 0.26f, 0.14f);
    public static readonly Color BaobabLeaf = new Color(0.22f, 0.58f, 0.22f);
    public static readonly Color BaobabCanopy = new Color(0.30f, 0.66f, 0.28f);
    public static readonly Color Rock = new Color(0.45f, 0.32f, 0.22f);
    public static readonly Color MudPuddle = new Color(0.36f, 0.24f, 0.12f);
    public static readonly Color MudShine = new Color(0.48f, 0.34f, 0.16f, 0.75f);
    public static readonly Color PipeMetal = new Color(0.55f, 0.58f, 0.62f);
    public static readonly Color NailMetal = new Color(0.62f, 0.62f, 0.66f);
    public static readonly Color HammerHandle = new Color(0.45f, 0.28f, 0.12f);
    public static readonly Color HammerHead = new Color(0.55f, 0.55f, 0.58f);
    public static readonly Color MonsterBody = new Color(0.32f, 0.22f, 0.12f);
    public static readonly Color MonsterEye = new Color(0.95f, 0.85f, 0.15f);
    public static readonly Color Warning = new Color(1f, 0.72f, 0.12f);
    public static readonly Color BubblePickup = new Color(0.45f, 0.88f, 1f);
    public static readonly Color PoisonPlant = new Color(0.18f, 0.52f, 0.16f);
    public static readonly Color PoisonLeaf = new Color(0.28f, 0.72f, 0.18f);
    public static readonly Color PoisonGas = new Color(0.32f, 0.92f, 0.22f, 0.32f);
    public static readonly Color SpeedFruit = new Color(1f, 0.42f, 0.12f);
    public static readonly Color SpeedLeaf = new Color(0.22f, 0.72f, 0.18f);
    public static readonly Color JumpBoost = new Color(0.55f, 0.38f, 1f);
    public static readonly Color JumpBoostGlow = new Color(0.72f, 0.62f, 1f);
    public static readonly Color CactusGreen = new Color(0.18f, 0.62f, 0.22f);
    public static readonly Color CactusArm = new Color(0.24f, 0.72f, 0.28f);
    public static readonly Color HealthFruit = new Color(0.92f, 0.18f, 0.22f);
    public static readonly Color HealthLeaf = new Color(0.22f, 0.72f, 0.18f);
    public static readonly Color WaterPool = new Color(0.22f, 0.58f, 0.92f);
    public static readonly Color WaterPoolDeep = new Color(0.12f, 0.42f, 0.78f);
    public static readonly Color LogBark = new Color(0.42f, 0.26f, 0.12f);
    public static readonly Color LogDark = new Color(0.28f, 0.16f, 0.08f);
    public static readonly Color Warthog = new Color(0.78f, 0.55f, 0.30f);
    public static readonly Color WarthogSnout = new Color(0.65f, 0.42f, 0.22f);

    public static GameObject Visual(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Color color, string name)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = RunnerVisualScale.V(localPos);
        go.transform.localScale = RunnerVisualScale.V(scale);
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.material.color = color;
        Collider col = go.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        return go;
    }

    public static void MakeTransparent(Renderer renderer, Color color)
    {
        if (renderer == null) return;

        Material source = renderer.sharedMaterial;
        Shader shader = source != null ? source.shader : null;
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Standard");
        }

        if (shader == null) return;

        Material mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        renderer.material = mat;
    }

    public static BoxCollider TallTrigger(GameObject root, float width, float depth, float height = 10f, float centerY = 4f)
    {
        BoxCollider box = root.GetComponent<BoxCollider>();
        if (box == null) box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, RunnerVisualScale.F(centerY), 0f);
        box.size = new Vector3(RunnerVisualScale.F(width), RunnerVisualScale.F(height), RunnerVisualScale.F(depth));
        return box;
    }

    static void KinematicBody(GameObject root)
    {
        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb == null) rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    static void BuildPipe(Transform root)
    {
        GameObject body = Visual(PrimitiveType.Cylinder, root, new Vector3(0f, 1.1f, 0f), new Vector3(0.85f, 1.9f, 0.85f), PipeMetal, "PipeBody");
        body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        Color capColor = new Color(0.38f, 0.40f, 0.45f);
        GameObject capL = Visual(PrimitiveType.Cylinder, root, new Vector3(-1.85f, 1.1f, 0f), new Vector3(0.9f, 0.16f, 0.9f), capColor, "CapL");
        capL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        GameObject capR = Visual(PrimitiveType.Cylinder, root, new Vector3(1.85f, 1.1f, 0f), new Vector3(0.9f, 0.16f, 0.9f), capColor, "CapR");
        capR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        Color collarColor = new Color(0.30f, 0.32f, 0.36f);
        GameObject collarL = Visual(PrimitiveType.Cylinder, root, new Vector3(-1.4f, 1.1f, 0f), new Vector3(1.0f, 0.18f, 1.0f), collarColor, "CollarL");
        collarL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        GameObject collarR = Visual(PrimitiveType.Cylinder, root, new Vector3(1.4f, 1.1f, 0f), new Vector3(1.0f, 0.18f, 1.0f), collarColor, "CollarR");
        collarR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
    }

    static void BuildNails(Transform root)
    {
        Color head = NailMetal;
        Color shaft = new Color(0.50f, 0.50f, 0.54f);
        float[] xOffsets = { -0.8f, -0.4f, 0f, 0.4f, 0.8f };
        float[] tilts = { -22f, -10f, 0f, 10f, 22f };

        for (int i = 0; i < 5; i++)
        {
            float x = xOffsets[i];
            float tilt = tilts[i];
            GameObject shaftGo = Visual(PrimitiveType.Cylinder, root, new Vector3(x, 1.0f, 0f), new Vector3(0.12f, 0.9f, 0.12f), shaft, $"Shaft_{i}");
            shaftGo.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            GameObject headGo = Visual(PrimitiveType.Cylinder, root, new Vector3(x + Mathf.Sin(tilt * Mathf.Deg2Rad) * 0.8f, 1.9f, 0f), new Vector3(0.38f, 0.08f, 0.38f), head, $"Head_{i}");
            headGo.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }

        Visual(PrimitiveType.Cube, root, new Vector3(0f, 0.1f, 0f), new Vector3(2.2f, 0.16f, 0.75f), new Color(0.52f, 0.34f, 0.16f), "Board");
    }

    static void BuildHammer(Transform root)
    {
        Visual(PrimitiveType.Cube, root, new Vector3(0f, 1.15f, 0f), new Vector3(0.28f, 2.1f, 0.28f), HammerHandle, "Handle");
        Visual(PrimitiveType.Cube, root, new Vector3(0f, 0.35f, 0f), new Vector3(0.34f, 0.45f, 0.34f), new Color(0.28f, 0.16f, 0.06f), "GripWrap");
        Visual(PrimitiveType.Cube, root, new Vector3(0f, 2.25f, 0f), new Vector3(1.55f, 0.55f, 0.5f), HammerHead, "HeadBody");
        Visual(PrimitiveType.Cube, root, new Vector3(0.8f, 2.25f, 0f), new Vector3(0.18f, 0.52f, 0.48f), new Color(0.70f, 0.70f, 0.74f), "StrikeFace");
        Visual(PrimitiveType.Cube, root, new Vector3(-0.8f, 2.25f, 0f), new Vector3(0.16f, 0.48f, 0.46f), new Color(0.60f, 0.60f, 0.64f), "Poll");
    }

    public static GameObject MakeWaterDroplet(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("WaterDroplet");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.85f);

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.85f, 0f), new Vector3(1.15f, 1.15f, 1.15f), WaterDrop, "Drop");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.45f, 0f), new Vector3(0.65f, 0.9f, 0.65f), WaterDropDeep, "Tip");
        GameObject glow = Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.9f, 0f), new Vector3(1.55f, 1.55f, 1.55f), WaterGlow, "Glow");
        MakeTransparent(glow.GetComponent<Renderer>(), WaterGlow);
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.4f, 1.25f, 0.15f), new Vector3(0.22f, 0.22f, 0.22f), Color.white, "Sparkle");

        TallTrigger(root, 2.0f, 2.0f, 4.5f, 1.4f);
        KinematicBody(root);
        root.AddComponent<Level2WaterDropletPickup>();
        root.AddComponent<Level2PickupBob>();
        return root;
    }

    public static GameObject MakeBaobab(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("Baobab");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        GameObject tree = new GameObject("Tree");
        tree.transform.SetParent(root.transform, false);
        if (!NaturePackVisuals.AttachTree(tree.transform, 8.5f))
        {
            Visual(PrimitiveType.Cylinder, tree.transform, new Vector3(0f, 3.2f, 0f), new Vector3(1.7f, 3.2f, 1.7f), BaobabTrunk, "Trunk");
            Visual(PrimitiveType.Cylinder, tree.transform, new Vector3(0f, 1.1f, 0f), new Vector3(2.05f, 1.1f, 2.05f), BaobabBark, "Base");
            Visual(PrimitiveType.Cylinder, tree.transform, new Vector3(1.4f, 5.6f, 0.2f), new Vector3(0.28f, 1.1f, 0.28f), BaobabBark, "BranchR");
            Visual(PrimitiveType.Cylinder, tree.transform, new Vector3(-1.35f, 5.5f, -0.15f), new Vector3(0.26f, 1.0f, 0.26f), BaobabBark, "BranchL");
            Visual(PrimitiveType.Sphere, tree.transform, new Vector3(0f, 6.6f, 0f), new Vector3(4.4f, 2.4f, 4.4f), BaobabCanopy, "Canopy");
            Visual(PrimitiveType.Sphere, tree.transform, new Vector3(1.5f, 6.2f, 0.6f), new Vector3(2.2f, 1.4f, 2.2f), BaobabLeaf, "CanopyR");
            Visual(PrimitiveType.Sphere, tree.transform, new Vector3(-1.4f, 6.15f, -0.5f), new Vector3(2.0f, 1.3f, 2.0f), BaobabLeaf, "CanopyL");
            tree.transform.localScale = Vector3.one * RunnerVisualScale.TreeBoost;
        }
        Visual(PrimitiveType.Sphere, tree.transform, new Vector3(0.4f, 1.6f, 0.7f), new Vector3(0.32f, 0.42f, 0.32f), WaterDrop, "Dew");

        GameObject pickup = new GameObject("PickupTrigger");
        pickup.transform.SetParent(root.transform, false);
        pickup.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        TallTrigger(pickup, 2.2f, 2.2f, 3.2f, 0.6f);
        pickup.AddComponent<Level2BaobabPickup>();

        KinematicBody(root);
        return root;
    }

    public static GameObject MakeMaterial(Transform parent, int lane, float z, Level2MaterialKind kind)
    {
        GameObject root = new GameObject($"Material_{kind}");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.5f);

        switch (kind)
        {
            case Level2MaterialKind.Pipe:
                BuildPipe(root.transform);
                break;
            case Level2MaterialKind.Nails:
                BuildNails(root.transform);
                break;
            default:
                BuildHammer(root.transform);
                break;
        }

        TallTrigger(root, 2.2f, 2.2f, 4f, 1.4f);
        KinematicBody(root);
        Level2MaterialPickup pickup = root.AddComponent<Level2MaterialPickup>();
        pickup.Setup(kind, Level2Config.DefaultMaterialPickup);
        root.AddComponent<Level2PickupBob>();
        return root;
    }

    public static GameObject MakeBubbleShieldPickup(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("BubbleShieldPickup");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.6f);

        GameObject bubble = Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.1f, 0f), new Vector3(1.7f, 1.7f, 1.7f), BubblePickup, "Bubble");
        MakeTransparent(bubble.GetComponent<Renderer>(), new Color(0.45f, 0.88f, 1f, 0.4f));
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.5f, 0.5f, 0.5f), Color.white, "Core");

        TallTrigger(root, 2.0f, 2.0f);
        KinematicBody(root);
        root.AddComponent<Level2BubbleShieldPickup>();
        root.AddComponent<Level2PickupBob>();
        return root;
    }

    public static GameObject MakeSpeedFruit(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("SpeedFruit");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.55f);

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.8f, 0f), new Vector3(1.25f, 1.25f, 1.25f), SpeedFruit, "Fruit");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 1.5f, 0f), new Vector3(0.28f, 0.4f, 0.12f), SpeedLeaf, "Leaf");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0.32f, 0.8f, 0f), new Vector3(0.12f, 0.12f, 0.8f), Color.white, "Trail");

        TallTrigger(root, 2.0f, 2.0f);
        KinematicBody(root);
        root.AddComponent<Level2SpeedFruitPickup>();
        root.AddComponent<Level2PickupBob>();
        return root;
    }

    public static GameObject MakeJumpBoost(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("JumpBoost");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.5f);

        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.22f, 0.7f, 0.22f), JumpBoost, "Shaft");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.85f, 0f), new Vector3(0.55f, 0.18f, 0.18f), JumpBoostGlow, "Head");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(-0.22f, 0.72f, 0f), new Vector3(0.28f, 0.16f, 0.16f), JumpBoost, "WingL");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0.22f, 0.72f, 0f), new Vector3(0.28f, 0.16f, 0.16f), JumpBoost, "WingR");

        TallTrigger(root, 1.5f, 1.5f);
        KinematicBody(root);
        root.AddComponent<Level2JumpBoostPickup>();
        root.AddComponent<Level2PickupBob>();
        return root;
    }

    public static GameObject MakeRock(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("Rock");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        if (!NaturePackVisuals.AttachRock(root.transform, 1.7f))
        {
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.7f, 0f), new Vector3(2.1f, 1.5f, 1.8f), Rock, "Boulder");
            Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.5f, 1.2f, 0.15f), new Vector3(1.1f, 0.85f, 1.0f), Rock, "Chunk");
        }

        TallTrigger(root, 2.2f, 2.0f, 4.5f, 1.4f);
        KinematicBody(root);
        Level2Obstacle obstacle = root.AddComponent<Level2Obstacle>();
        obstacle.Setup(Level2ObstacleKind.Rock, true);
        return root;
    }

    public static GameObject MakeMudPuddle(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("MudPuddle");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        // Keep ordinary mud hazards irregular so they cannot be mistaken for
        // the round potholes that always contain a mud monster.
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.10f, 0f), new Vector3(3.4f, 0.10f, 2.15f), MudPuddle, "MudSplat");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(-1.45f, 0.10f, 0.62f), new Vector3(1.35f, 0.08f, 1.05f), MudPuddle * 0.90f, "MudBlobL");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(1.38f, 0.10f, -0.48f), new Vector3(1.15f, 0.08f, 0.92f), MudPuddle * 0.94f, "MudBlobR");
        GameObject shine = Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.35f, 0.17f, 0.05f), new Vector3(1.7f, 0.035f, 0.82f), MudShine, "Shine");
        MakeTransparent(shine.GetComponent<Renderer>(), MudShine);

        TallTrigger(root, 3.2f, 3.2f, 3.8f, 1.2f);
        KinematicBody(root);
        Level2Obstacle obstacle = root.AddComponent<Level2Obstacle>();
        obstacle.Setup(Level2ObstacleKind.MudPuddle, true);
        return root;
    }

    public static GameObject MakePoisonPlant(Transform parent, int lane, float z, float spawnProgress)
    {
        int rightLane = Mathf.Min(lane + 1, LevelLanes.Count - 1);
        float spanWidth = Mathf.Abs(LevelLanes.X(rightLane) - LevelLanes.X(lane)) + 2.4f;

        GameObject root = new GameObject("PoisonPlant");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        GameObject plantBody = new GameObject("PlantBody");
        plantBody.transform.SetParent(root.transform, false);
        if (!NaturePackVisuals.AttachBush(plantBody.transform, 3.6f))
        {
            Visual(PrimitiveType.Cylinder, plantBody.transform, new Vector3(0f, 1.05f, 0f), new Vector3(0.9f, 1.05f, 0.9f), PoisonPlant, "Stem");
            Visual(PrimitiveType.Capsule, plantBody.transform, new Vector3(-0.85f, 1.7f, 0.1f), new Vector3(0.7f, 1.15f, 0.4f), PoisonLeaf, "LeafL");
            Visual(PrimitiveType.Capsule, plantBody.transform, new Vector3(0.85f, 1.8f, -0.1f), new Vector3(0.7f, 1.2f, 0.4f), PoisonLeaf, "LeafR");
            Visual(PrimitiveType.Capsule, plantBody.transform, new Vector3(0f, 1.55f, 0.7f), new Vector3(0.55f, 0.95f, 0.35f), PoisonLeaf, "LeafF");
            Visual(PrimitiveType.Sphere, plantBody.transform, new Vector3(0f, 2.15f, 0f), new Vector3(1.15f, 0.95f, 1.15f), PoisonPlant, "Bulb");
            Visual(PrimitiveType.Sphere, plantBody.transform, new Vector3(0.35f, 2.35f, 0.2f), new Vector3(0.35f, 0.28f, 0.35f), new Color(0.45f, 0.95f, 0.22f), "GlowSpot");
            plantBody.transform.localScale = Vector3.one * RunnerVisualScale.PlantBoost;
        }

        float laneCenterX = (LevelLanes.X(rightLane) - LevelLanes.X(lane)) * 0.5f;
        float plantTopY = RunnerVisualScale.V(new Vector3(0f, 2.15f, 0f)).y * RunnerVisualScale.PlantBoost;
        float gasCenterY = plantTopY + RunnerVisualScale.F(0.35f);
        const float GasSizeBoost = 1.2f;
        float gasDiameter = spanWidth * 0.95f * GasSizeBoost;
        float gasHeight = RunnerVisualScale.F(3.2f) * RunnerVisualScale.PlantBoost * GasSizeBoost;

        GameObject gasSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gasSphere.name = "PoisonGasSphere";
        gasSphere.transform.SetParent(root.transform, false);
        gasSphere.transform.localPosition = new Vector3(laneCenterX, gasCenterY, 0f);
        gasSphere.transform.localScale = new Vector3(gasDiameter, gasHeight, gasDiameter);
        Object.Destroy(gasSphere.GetComponent<Collider>());
        Renderer gasRenderer = gasSphere.GetComponent<Renderer>();
        if (gasRenderer != null)
        {
            gasRenderer.material.color = PoisonGas;
            MakeTransparent(gasRenderer, PoisonGas);
        }
        gasSphere.SetActive(false);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        warning.transform.localPosition = new Vector3(laneCenterX, gasCenterY + RunnerVisualScale.F(1.2f), 0f);
        Visual(PrimitiveType.Cube, warning.transform, new Vector3(0f, 0.5f, 0f), new Vector3(1.5f, 1.1f, 0.16f), Warning, "Sign");
        Visual(PrimitiveType.Cylinder, warning.transform, new Vector3(0f, -0.2f, 0f), new Vector3(0.16f, 0.5f, 0.16f), Warning, "Pole");
        warning.SetActive(false);

        SphereCollider gasCol = root.AddComponent<SphereCollider>();
        gasCol.isTrigger = true;
        gasCol.center = new Vector3(laneCenterX, gasCenterY, 0f);
        gasCol.radius = Mathf.Max(gasDiameter, gasHeight) * 0.5f;

        BoxCollider plantCol = root.AddComponent<BoxCollider>();
        plantCol.isTrigger = true;
        plantCol.center = RunnerVisualScale.PlantV(new Vector3(0f, 1.1f, 0f));
        plantCol.size = RunnerVisualScale.PlantV(new Vector3(1.8f, 2.4f, 1.8f));

        KinematicBody(root);
        Level2PoisonPlant plant = root.AddComponent<Level2PoisonPlant>();
        plant.Setup(spawnProgress, warning, gasSphere);
        return root;
    }

    public static GameObject MakeMudMonster(Transform parent, int lane, float z, float spawnProgress)
    {
        GameObject root = new GameObject("MudMonster");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.1f);

        GameObject warningRoot = new GameObject("BrownPothole");
        warningRoot.transform.SetParent(root.transform, false);
        warningRoot.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        Visual(PrimitiveType.Cylinder, warningRoot.transform, new Vector3(0f, 0.02f, 0f), new Vector3(3.2f, 0.07f, 3.2f), new Color(0.24f, 0.13f, 0.055f), "DarkMudHole");
        Visual(PrimitiveType.Cylinder, warningRoot.transform, new Vector3(0f, -0.01f, 0f), new Vector3(3.8f, 0.045f, 3.8f), MudPuddle, "MudRim");
        Visual(PrimitiveType.Sphere, warningRoot.transform, new Vector3(1.4f, 0.12f, 0.55f), new Vector3(0.55f, 0.18f, 0.55f), MudShine, "MudSplash");

        GameObject visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(root.transform, false);
        GameObject monsterPrefab = Resources.Load<GameObject>("Monsters/SlimePBR");
#if UNITY_EDITOR
        if (monsterPrefab == null)
        {
            monsterPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/RPG Monster DUO PBR Polyart/Prefabs/PBRDefault/SlimePBR.prefab");
        }
#endif
        if (monsterPrefab != null)
        {
            GameObject model = Object.Instantiate(monsterPrefab, visualRoot.transform);
            model.name = "RPG_MudMonster";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            model.transform.localScale = Vector3.one * 2.6f;
            NaturePackVisuals.EnsureSceneObjectMaterials(model);
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
        }
        else
        {
            Visual(PrimitiveType.Capsule, visualRoot.transform, new Vector3(0f, 1.35f, 0f), new Vector3(1.9f, 1.55f, 1.9f), MonsterBody, "Body");
            Visual(PrimitiveType.Sphere, visualRoot.transform, new Vector3(0f, 2.65f, 0.4f), new Vector3(1.35f, 1.35f, 1.35f), MonsterBody, "Head");
            Visual(PrimitiveType.Sphere, visualRoot.transform, new Vector3(-0.34f, 2.78f, 0.9f), new Vector3(0.28f, 0.28f, 0.28f), MonsterEye, "EyeL");
            Visual(PrimitiveType.Sphere, visualRoot.transform, new Vector3(0.34f, 2.78f, 0.9f), new Vector3(0.28f, 0.28f, 0.28f), MonsterEye, "EyeR");
        }
        visualRoot.SetActive(false);

        SphereCollider potholeTrigger = root.AddComponent<SphereCollider>();
        potholeTrigger.isTrigger = true;
        potholeTrigger.center = new Vector3(0f, 0.25f, 0f);
        potholeTrigger.radius = 2.7f;
        KinematicBody(root);
        root.AddComponent<Level2MudPothole>();
        Level2MudMonster monster = root.AddComponent<Level2MudMonster>();
        monster.Setup(lane, spawnProgress, warningRoot, visualRoot);
        return root;
    }

    public static GameObject MakeMudBall(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("MudBall");
        if (parent != null) root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.7f);

        Visual(PrimitiveType.Sphere, root.transform, Vector3.zero, new Vector3(2.0f, 2.0f, 2.0f), MudPuddle, "Ball");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.35f, 0.25f, 0.15f), new Vector3(0.9f, 0.7f, 0.9f), Rock, "Chunk");

        SphereCollider col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.1f;
        KinematicBody(root);
        root.AddComponent<Level2MudBall>();
        return root;
    }

    public static GameObject MakeRollingLog(Transform parent, int lane, float z, int laneSpan = 2, float speed = 13f)
    {
        int span = Mathf.Clamp(laneSpan, 2, 3);
        int leftLane = Mathf.Clamp(lane, 0, LevelLanes.Count - span);
        int rightLane = leftLane + span - 1;
        float leftX = LevelLanes.X(leftLane);
        float rightX = LevelLanes.X(rightLane);
        float midX = (leftX + rightX) * 0.5f;
        float width = Mathf.Abs(rightX - leftX) + 1.6f;

        GameObject root = new GameObject(span >= 3 ? "RollingLog_3Lane" : "RollingLog_2Lane");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(midX, Level2Ground.SurfaceY, z);

        Transform rollPivot = NaturePackVisuals.CreateLogRollPivot(root.transform);
        if (!NaturePackVisuals.AttachLevel2RollingLog(rollPivot, width))
        {
            GameObject mesh = Visual(PrimitiveType.Cylinder, rollPivot, new Vector3(0f, 0.68f, 0f), new Vector3(1.3f, width * 0.5f, 1.3f), LogBark, "Log");
            mesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Visual(PrimitiveType.Sphere, rollPivot, new Vector3(-width * 0.48f, 0.68f, 0f), new Vector3(1.2f, 1.2f, 1.2f), LogDark, "EndL");
            Visual(PrimitiveType.Sphere, rollPivot, new Vector3(width * 0.48f, 0.68f, 0f), new Vector3(1.2f, 1.2f, 1.2f), LogDark, "EndR");
            NaturePackVisuals.AlignRollingLogPivot(rollPivot);
        }
        else
        {
            NaturePackVisuals.AlignRollingLogPivot(rollPivot);
        }

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = RunnerVisualScale.V(new Vector3(0f, 0.72f, 0f));
        box.size = RunnerVisualScale.V(new Vector3(width, 1.9f, 1.8f));
        KinematicBody(root);
        root.AddComponent<Level2RollingLog>().Setup(speed);
        return root;
    }

    public static GameObject MakeCactus(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("Cactus");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        GameObject body = new GameObject("PlantBody");
        body.transform.SetParent(root.transform, false);
        if (!NaturePackVisuals.AttachCactus(body.transform, 4.2f))
        {
            Visual(PrimitiveType.Cylinder, body.transform, new Vector3(0f, 2.1f, 0f), new Vector3(0.85f, 2.1f, 0.85f), CactusGreen, "Trunk");
            Visual(PrimitiveType.Sphere, body.transform, new Vector3(0.95f, 2.7f, 0f), new Vector3(1.0f, 0.6f, 0.6f), CactusArm, "ArmL");
            Visual(PrimitiveType.Capsule, body.transform, new Vector3(-0.9f, 3.1f, 0f), new Vector3(0.48f, 0.8f, 0.48f), CactusArm, "ArmR");
            body.transform.localScale = Vector3.one * RunnerVisualScale.PlantBoost;
        }

        TallTrigger(root, 2.0f * RunnerVisualScale.PlantBoost, 2.0f * RunnerVisualScale.PlantBoost, 5f * RunnerVisualScale.PlantBoost, 1.8f * RunnerVisualScale.PlantBoost);
        KinematicBody(root);
        Level2Obstacle obstacle = root.AddComponent<Level2Obstacle>();
        obstacle.Setup(Level2ObstacleKind.Cactus, false);
        return root;
    }

    public static GameObject MakeHealthFruit(Transform parent, int lane, float z, float amount = 15f)
    {
        GameObject root = new GameObject("HealthFruit");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.55f);

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.85f, 0f), new Vector3(1.25f, 1.25f, 1.25f), HealthFruit, "Fruit");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 1.55f, 0f), new Vector3(0.28f, 0.42f, 0.12f), HealthLeaf, "Leaf");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.32f, 0.85f, 0f), new Vector3(0.18f, 0.18f, 0.18f), Color.white, "Sparkle");

        TallTrigger(root, 2.0f, 2.0f);
        KinematicBody(root);
        Level2HealthFruitPickup pickup = root.AddComponent<Level2HealthFruitPickup>();
        pickup.Setup(amount);
        root.AddComponent<Level2PickupBob>();
        return root;
    }

    public static GameObject MakeWaterPool(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("WaterPool");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        AquisWaterVisuals.BuildLevel2PoolVisuals(root.transform, WaterPool, WaterPoolDeep);

        TallTrigger(root, 4.8f, 4.2f, 3.8f, 1.2f);
        KinematicBody(root);
        root.AddComponent<Level2WaterPoolPickup>();
        return root;
    }

    public static GameObject MakeWarthog(Transform parent, float z, bool goRight, float speed = 38f)
    {
        GameObject root = new GameObject("Warthog");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(LevelLanes.X(goRight ? 0 : LevelLanes.Count - 1), Level2Ground.SurfaceY, z);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        if (!AttachBrownPig(visual.transform))
        {
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.65f, 0f), new Vector3(2.6f, 1.0f, 1.2f), Warthog, "Body");
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.4f, 0.80f, 0f), new Vector3(0.85f, 0.65f, 0.9f), WarthogSnout, "Head");
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.75f, 0.72f, 0.35f), new Vector3(0.35f, 0.18f, 0.18f), Color.white, "TuskL");
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.75f, 0.72f, -0.35f), new Vector3(0.35f, 0.18f, 0.18f), Color.white, "TuskR");
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(0.7f, 0.18f, 0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegFL");
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(0.7f, 0.18f, -0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegFR");
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(-0.7f, 0.18f, 0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegBL");
            Visual(PrimitiveType.Cube, visual.transform, new Vector3(-0.7f, 0.18f, -0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegBR");
        }

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = RunnerVisualScale.V(new Vector3(0f, 0.65f, 0f));
        box.size = new Vector3(3.1f, 1.45f, 1.8f);
        KinematicBody(root);
        root.AddComponent<Level2Warthog>().Setup(goRight, speed, visual);
        return root;
    }

    static bool AttachBrownPig(Transform parent)
    {
        GameObject source = Resources.Load<GameObject>("Animals/BrownPig");
        if (source == null) return false;

        GameObject pig = Object.Instantiate(source, parent, false);
        pig.name = "BrownPigWarthog";
        pig.transform.localPosition = Vector3.zero;
        pig.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        pig.transform.localScale = Vector3.one;

        Collider[] colliders = pig.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i] != null) colliders[i].enabled = false;

        Animator[] animators = pig.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
            if (animators[i] != null) animators[i].applyRootMotion = false;

        Renderer[] renderers = pig.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Object.Destroy(pig);
            return false;
        }

        Color brownTint = new Color(0.58f, 0.34f, 0.16f, 1f);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            for (int m = 0; m < materials.Length; m++)
            {
                Material material = materials[m];
                if (material == null || !material.name.StartsWith("Pig")) continue;
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", brownTint);
                if (material.HasProperty("_Color")) material.SetColor("_Color", brownTint);
            }
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        pig.transform.localScale *= 2.4f / Mathf.Max(0.01f, bounds.size.y);

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        pig.transform.position += Vector3.up * (parent.position.y - bounds.min.y);
        return true;
    }
}
