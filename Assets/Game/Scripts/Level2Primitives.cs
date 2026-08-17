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

    public static GameObject Visual(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Color color, string name)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
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
        box.center = new Vector3(0f, centerY, 0f);
        box.size = new Vector3(width, height, depth);
        return box;
    }

    static void KinematicBody(GameObject root)
    {
        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb == null) rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public static GameObject MakeWaterDroplet(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("WaterDroplet");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.85f);

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.72f, 0.72f, 0.72f), WaterDrop, "Drop");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.95f, 0f), new Vector3(0.42f, 0.58f, 0.42f), WaterDropDeep, "Tip");
        GameObject glow = Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.6f, 0f), new Vector3(1.05f, 1.05f, 1.05f), WaterGlow, "Glow");
        MakeTransparent(glow.GetComponent<Renderer>(), WaterGlow);
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.28f, 0.85f, 0.1f), new Vector3(0.16f, 0.16f, 0.16f), Color.white, "Sparkle");

        TallTrigger(root, 1.5f, 1.5f, 4f, 1.2f);
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

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 3.2f, 0f), new Vector3(1.7f, 3.2f, 1.7f), BaobabTrunk, "Trunk");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.1f, 0f), new Vector3(2.05f, 1.1f, 2.05f), BaobabBark, "Base");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(1.4f, 5.6f, 0.2f), new Vector3(0.28f, 1.1f, 0.28f), BaobabBark, "BranchR");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(-1.35f, 5.5f, -0.15f), new Vector3(0.26f, 1.0f, 0.26f), BaobabBark, "BranchL");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 6.6f, 0f), new Vector3(4.4f, 2.4f, 4.4f), BaobabCanopy, "Canopy");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(1.5f, 6.2f, 0.6f), new Vector3(2.2f, 1.4f, 2.2f), BaobabLeaf, "CanopyR");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(-1.4f, 6.15f, -0.5f), new Vector3(2.0f, 1.3f, 2.0f), BaobabLeaf, "CanopyL");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.4f, 1.6f, 0.7f), new Vector3(0.32f, 0.42f, 0.32f), WaterDrop, "Dew");

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
                Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.35f, 0.55f, 0.35f), PipeMetal, "Pipe");
                break;
            case Level2MaterialKind.Nails:
                Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.5f, 0.15f, 0.5f), NailMetal, "Nails");
                Visual(PrimitiveType.Cube, root.transform, new Vector3(0.15f, 0.55f, 0f), new Vector3(0.08f, 0.35f, 0.08f), NailMetal, "Nail1");
                break;
            default:
                Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0.35f, 0.55f), HammerHead, "Head");
                Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.12f, 0f), new Vector3(0.12f, 0.35f, 0.12f), HammerHandle, "Handle");
                break;
        }

        TallTrigger(root, 1.4f, 1.4f);
        KinematicBody(root);
        Level2MaterialPickup pickup = root.AddComponent<Level2MaterialPickup>();
        pickup.Setup(kind, 10);
        root.AddComponent<Level2PickupBob>();
        return root;
    }

    public static GameObject MakeBubbleShieldPickup(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("BubbleShieldPickup");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.6f);

        GameObject bubble = Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.8f, 0f), new Vector3(1.15f, 1.15f, 1.15f), BubblePickup, "Bubble");
        MakeTransparent(bubble.GetComponent<Renderer>(), new Color(0.45f, 0.88f, 1f, 0.4f));
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.8f, 0f), new Vector3(0.35f, 0.35f, 0.35f), Color.white, "Core");

        TallTrigger(root, 1.6f, 1.6f);
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

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.85f, 0.85f, 0.85f), SpeedFruit, "Fruit");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 1.05f, 0f), new Vector3(0.18f, 0.28f, 0.08f), SpeedLeaf, "Leaf");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0.22f, 0.55f, 0f), new Vector3(0.08f, 0.08f, 0.55f), Color.white, "Trail");

        TallTrigger(root, 1.5f, 1.5f);
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

        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(1.5f, 1.1f, 1.3f), Rock, "Boulder");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.35f, 0.85f, 0.1f), new Vector3(0.7f, 0.55f, 0.65f), Rock, "Chunk");

        TallTrigger(root, 1.6f, 1.5f, 4f, 1.2f);
        KinematicBody(root);
        Level2Obstacle obstacle = root.AddComponent<Level2Obstacle>();
        obstacle.Setup(Level2ObstacleKind.Rock, false);
        return root;
    }

    public static GameObject MakeMudPuddle(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("MudPuddle");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.06f, 0f), new Vector3(2.4f, 0.06f, 2.4f), MudPuddle, "Puddle");
        GameObject shine = Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0.15f, 0.1f, 0.1f), new Vector3(1.4f, 0.04f, 1.4f), MudShine, "Shine");
        MakeTransparent(shine.GetComponent<Renderer>(), MudShine);

        TallTrigger(root, 2.2f, 2.2f, 3.5f, 1.2f);
        KinematicBody(root);
        Level2Obstacle obstacle = root.AddComponent<Level2Obstacle>();
        obstacle.Setup(Level2ObstacleKind.MudPuddle, true);
        return root;
    }

    public static GameObject MakePoisonPlant(Transform parent, int lane, float z, float spawnProgress)
    {
        GameObject root = new GameObject("PoisonPlant");
        root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z);

        GameObject plantBody = new GameObject("PlantBody");
        plantBody.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cylinder, plantBody.transform, new Vector3(0f, 0.7f, 0f), new Vector3(0.35f, 0.7f, 0.35f), PoisonPlant, "Stem");
        Visual(PrimitiveType.Capsule, plantBody.transform, new Vector3(-0.45f, 1.15f, 0f), new Vector3(0.4f, 0.7f, 0.22f), PoisonLeaf, "LeafL");
        Visual(PrimitiveType.Capsule, plantBody.transform, new Vector3(0.45f, 1.2f, 0f), new Vector3(0.4f, 0.75f, 0.22f), PoisonLeaf, "LeafR");
        Visual(PrimitiveType.Sphere, plantBody.transform, new Vector3(0f, 1.55f, 0f), new Vector3(0.55f, 0.45f, 0.55f), PoisonPlant, "Bulb");

        GameObject gasEmitter = new GameObject("GasEmitter");
        gasEmitter.transform.SetParent(root.transform, false);
        gasEmitter.transform.localPosition = new Vector3(0f, 1.7f, 0f);

        GameObject gasSphere = Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 15f, 0f), new Vector3(30f, 30f, 30f), PoisonGas, "PoisonGasSphere");
        MakeTransparent(gasSphere.GetComponent<Renderer>(), PoisonGas);
        gasSphere.SetActive(false);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        warning.transform.localPosition = new Vector3(0f, 2.6f, 0f);
        Visual(PrimitiveType.Cube, warning.transform, new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.9f, 0.14f), Warning, "Sign");
        Visual(PrimitiveType.Cylinder, warning.transform, new Vector3(0f, -0.2f, 0f), new Vector3(0.12f, 0.4f, 0.12f), Warning, "Pole");
        warning.SetActive(false);

        SphereCollider gasCol = root.AddComponent<SphereCollider>();
        gasCol.isTrigger = true;
        gasCol.center = new Vector3(0f, 15f, 0f);
        gasCol.radius = 15f;

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

        GameObject warningRoot = new GameObject("MonsterWarning");
        warningRoot.transform.SetParent(root.transform, false);
        warningRoot.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        Visual(PrimitiveType.Cylinder, warningRoot.transform, new Vector3(0f, 0.05f, 0f), new Vector3(2.2f, 0.05f, 2.2f), Warning, "GroundCircle");
        Visual(PrimitiveType.Sphere, warningRoot.transform, new Vector3(0.4f, 0.25f, 0.2f), new Vector3(0.35f, 0.22f, 0.35f), MudPuddle, "Splash");
        warningRoot.SetActive(false);

        GameObject visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Capsule, visualRoot.transform, new Vector3(0f, 0.85f, 0f), new Vector3(1.0f, 0.85f, 1.0f), MonsterBody, "Body");
        Visual(PrimitiveType.Sphere, visualRoot.transform, new Vector3(0f, 1.55f, 0.3f), new Vector3(0.7f, 0.7f, 0.7f), MonsterBody, "Head");
        Visual(PrimitiveType.Sphere, visualRoot.transform, new Vector3(-0.18f, 1.62f, 0.55f), new Vector3(0.14f, 0.14f, 0.14f), MonsterEye, "EyeL");
        Visual(PrimitiveType.Sphere, visualRoot.transform, new Vector3(0.18f, 1.62f, 0.55f), new Vector3(0.14f, 0.14f, 0.14f), MonsterEye, "EyeR");
        visualRoot.SetActive(false);

        KinematicBody(root);
        Level2MudMonster monster = root.AddComponent<Level2MudMonster>();
        monster.Setup(lane, spawnProgress, warningRoot, visualRoot);
        return root;
    }

    public static GameObject MakeMudBall(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("MudBall");
        if (parent != null) root.transform.SetParent(parent, false);
        root.transform.position = Level2Ground.LanePosition(lane, z, 0.7f);

        Visual(PrimitiveType.Sphere, root.transform, Vector3.zero, new Vector3(1.5f, 1.5f, 1.5f), MudPuddle, "Ball");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.25f, 0.2f, 0.1f), new Vector3(0.7f, 0.55f, 0.7f), Rock, "Chunk");

        SphereCollider col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.85f;
        KinematicBody(root);
        root.AddComponent<Level2MudBall>();
        return root;
    }
}
