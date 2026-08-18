using UnityEngine;
using UnityEngine.Rendering;

public static class Level3Primitives
{
    public static readonly Color Rock = new Color(0.46f, 0.33f, 0.22f);
    public static readonly Color Mud = new Color(0.36f, 0.24f, 0.12f);
    public static readonly Color MudShine = new Color(0.5f, 0.36f, 0.16f, 0.7f);
    public static readonly Color PipeMetal = new Color(0.55f, 0.58f, 0.62f);
    public static readonly Color NailMetal = new Color(0.64f, 0.64f, 0.68f);
    public static readonly Color HammerHandle = new Color(0.45f, 0.28f, 0.12f);
    public static readonly Color HammerHead = new Color(0.55f, 0.55f, 0.58f);
    public static readonly Color Bucket = new Color(0.2f, 0.55f, 0.9f);
    public static readonly Color Health = new Color(0.9f, 0.2f, 0.22f);
    public static readonly Color Leaf = new Color(0.32f, 0.78f, 0.28f);
    public static readonly Color Warning = new Color(1f, 0.92f, 0.08f);  // bright yellow warning
    public static readonly Color Lightning = new Color(1f, 0.95f, 0.05f);       // neon yellow outer
    public static readonly Color LightningCore = new Color(1f, 1f, 0.78f);      // bright white-yellow core
    public static readonly Color Droplet = new Color(0.25f, 0.65f, 1f);
    public static readonly Color YellowRepair = new Color(1f, 0.88f, 0.15f);
    public static readonly Color Snake = new Color(0.62f, 0.98f, 0.38f);
    public static readonly Color SnakeDark = new Color(0.28f, 0.72f, 0.22f);
    public static readonly Color SpeedFruit = new Color(0.2f, 0.95f, 0.25f);
    public static readonly Color SpeedFruitGlow = new Color(0.4f, 1f, 0.45f, 0.35f);
    public static readonly Color Tape = new Color(0.95f, 0.78f, 0.2f);
    public static readonly Color TreeTrunk = new Color(0.42f, 0.26f, 0.12f);
    public static readonly Color TreeLeaves = new Color(0.12f, 0.48f, 0.18f);
    public static readonly Color Tank1Pipe = new Color(0.55f, 0.82f, 0.98f);
    public static readonly Color Tank2Pipe = new Color(0.92f, 0.28f, 0.22f);
    public static readonly Color Tank3Pipe = new Color(0.58f, 0.36f, 0.18f);
    public static readonly Color Warthog = new Color(0.42f, 0.28f, 0.16f);
    public static readonly Color Tank = new Color(0.55f, 0.6f, 0.66f);
    public static readonly Color WaterFlow = new Color(0.25f, 0.7f, 1f, 0.55f);
    public static readonly Color Acid = new Color(0.55f, 0.95f, 0.38f, 0.32f);
    public static readonly Color LogBark = new Color(0.42f, 0.26f, 0.12f);
    public static readonly Color LogDark = new Color(0.28f, 0.16f, 0.08f);

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
        if (col != null) Object.Destroy(col);
        return go;
    }

    public static void MakeTransparent(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        Material source = renderer.sharedMaterial;
        Shader shader = source != null ? source.shader : null;
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        }
        if (shader == null) return;

        Material mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        renderer.material = mat;
    }

    public static BoxCollider TallTrigger(GameObject root, float width, float depth, float height = 8f, float centerY = 3f)
    {
        BoxCollider box = root.GetComponent<BoxCollider>();
        if (box == null) box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, centerY, 0f);
        box.size = new Vector3(width, height, depth);
        return box;
    }

    static void Kinematic(GameObject root)
    {
        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb == null) rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public static GameObject MakeRock(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("Rock");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(1.45f, 1.05f, 1.25f), Rock, "Boulder");
        TallTrigger(root, 1.55f, 1.4f);
        Kinematic(root);
        root.AddComponent<Level3Obstacle>().Setup(Level3ObstacleKind.Rock, true);
        return root;
    }

    public static GameObject MakeTree(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("Tree");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 3.6f, 0f), new Vector3(0.7f, 3.6f, 0.7f), TreeTrunk, "Trunk");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 7.6f, 0f), new Vector3(3.4f, 3.2f, 3.4f), TreeLeaves, "Canopy");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.7f, 8.2f, 0.4f), new Vector3(2.1f, 1.9f, 2.1f), TreeLeaves * 0.85f, "CanopyB");
        TallTrigger(root, 2.1f, 2.1f, 9f, 3.6f);
        Kinematic(root);
        root.AddComponent<Level3Obstacle>().Setup(Level3ObstacleKind.Tree, false);
        return root;
    }

    public static GameObject MakeMudPuddle(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("MudPuddle");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(2.3f, 0.05f, 2.3f), Mud, "Puddle");
        GameObject shine = Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0.1f, 0.09f, 0.1f), new Vector3(1.3f, 0.03f, 1.3f), MudShine, "Shine");
        MakeTransparent(shine.GetComponent<Renderer>(), MudShine);
        TallTrigger(root, 2.1f, 2.1f, 3.2f, 1f);
        Kinematic(root);
        root.AddComponent<Level3Obstacle>().Setup(Level3ObstacleKind.MudPuddle, true);
        return root;
    }

    public static GameObject MakeMaterial(Transform parent, int lane, float z, string kind)
    {
        GameObject root = new GameObject($"Material_{kind}");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z, 0.5f);
        if (kind == "Nails")
        {
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.5f, 0.15f, 0.5f), NailMetal, "Box");
        }
        else if (kind == "Hammer")
        {
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0.35f, 0.55f), HammerHead, "Head");
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.12f, 0f), new Vector3(0.12f, 0.35f, 0.12f), HammerHandle, "Handle");
        }
        else if (kind == "Tape")
        {
            Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.4f, 0f), new Vector3(0.7f, 0.18f, 0.7f), Tape, "Roll");
            Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.4f, 0f), new Vector3(0.28f, 0.2f, 0.28f), Color.white, "Core");
        }
        else
        {
            Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.35f, 0.55f, 0.35f), PipeMetal, "Pipe");
        }

        TallTrigger(root, 1.4f, 1.4f);
        Kinematic(root);
        root.AddComponent<Level3MaterialPickup>().Setup(kind, 10);
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    public static GameObject MakeWaterDroplet(Transform parent, int lane, float z, float amount)
    {
        GameObject root = new GameObject("WaterDroplet");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z, 0.55f);
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.65f, 0f), new Vector3(0.55f, 0.75f, 0.55f), Droplet, "Drop");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.95f, 0f), new Vector3(0.25f, 0.35f, 0.25f), Color.white, "Highlight");
        TallTrigger(root, 1.2f, 1.2f);
        Kinematic(root);
        Level3WaterDropletPickup pickup = root.AddComponent<Level3WaterDropletPickup>();
        pickup.Setup(amount);
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    public static GameObject MakeHealth(Transform parent, int lane, float z, float amount = 15f)
    {
        GameObject root = new GameObject("HealthPickup");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z, 0.5f);
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.7f, 0.7f, 0.7f), Health, "Heart");
        TallTrigger(root, 1.4f, 1.4f);
        Kinematic(root);
        Level3HealthPickup hp = root.AddComponent<Level3HealthPickup>();
        hp.Setup(amount);
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    public static GameObject MakeSpeedFruit(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("SpeedFruitPickup");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z, 0.55f);

        GameObject fruit = Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.85f, 0.85f, 0.85f), SpeedFruit, "Fruit");
        MakeTransparent(fruit.GetComponent<Renderer>(), SpeedFruitGlow);
        // Solid core so it's readable even without transparency.
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.55f, 0.55f, 0.55f), SpeedFruit, "Core");

        // Simple trail/particles: small child spheres that the pickup script animates.
        GameObject trail = new GameObject("Trail");
        trail.transform.SetParent(root.transform, false);
        trail.transform.localPosition = new Vector3(0.22f, 0.45f, 0f);
        Visual(PrimitiveType.Cube, trail.transform, Vector3.zero, new Vector3(0.12f, 0.12f, 0.25f), Color.white, "TrailCore");

        TallTrigger(root, 1.5f, 1.5f);
        Kinematic(root);

        root.AddComponent<Level3SpeedFruitPickup>();
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    public static GameObject MakeWarthog(Transform parent, float z, Level3EnemyPace pace, bool goRight)
    {
        GameObject root = new GameObject("Warthog");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(LevelLanes.PathCenterX, Level3Ground.SurfaceY, z);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cylinder, warning.transform, new Vector3(0f, 0.06f, 0f), new Vector3(11f, 0.05f, 1.6f), Warning, "Dust");
        warning.SetActive(true);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.7f, 0f), new Vector3(2.4f, 1.1f, 1.1f), Warthog, "Body");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.2f, 0.85f, 0f), new Vector3(0.9f, 0.7f, 0.8f), Warthog, "Head");
        visual.SetActive(true);

        TallTrigger(root, 2.8f, 1.5f, 5f, 1.4f);
        Kinematic(root);
        root.AddComponent<Level3HorizontalEnemy>().Setup(Level3HorizontalEnemy.EnemyKind.Warthog, pace, goRight, warning, visual);
        return root;
    }

    public static GameObject MakeMudSweep(Transform parent, float z, bool goRight, Level3EnemyPace pace)
    {
        GameObject root = new GameObject("MudSweep");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(LevelLanes.PathCenterX, Level3Ground.SurfaceY, z);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cylinder, warning.transform, new Vector3(0f, 0.06f, 0f), new Vector3(10f, 0.05f, 1.4f), Warning, "Marker");
        warning.SetActive(false);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Sphere, visual.transform, new Vector3(0f, 0.7f, 0f), new Vector3(1.8f, 1.8f, 1.8f), Mud, "MudBall");
        visual.SetActive(false);

        TallTrigger(root, 2.2f, 2.2f, 4f, 1.2f);
        Kinematic(root);
        root.AddComponent<Level3HorizontalEnemy>().Setup(Level3HorizontalEnemy.EnemyKind.Warthog, pace, goRight, warning, visual);
        return root;
    }

    public static GameObject MakeApproachSnake(Transform parent, int lane, float z, float progress)
    {
        GameObject root = new GameObject("Snake");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        warning.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        Visual(PrimitiveType.Cube, warning.transform, Vector3.zero, new Vector3(1.4f, 1.1f, 0.18f), Warning, "Sign");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.04f, 0f), new Vector3(1.7f, 0.04f, 1.7f), Snake, "LaneMarker");
        warning.SetActive(true);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.48f, 0f), new Vector3(0.78f, 0.58f, 4.4f), Snake, "Body");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.55f, -2.3f), new Vector3(0.9f, 0.68f, 0.9f), SnakeDark, "Head");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.42f, 2.2f), new Vector3(0.42f, 0.32f, 0.7f), Snake, "Tail");
        visual.SetActive(true);

        TallTrigger(root, 1.4f, 4.6f, 4f, 1.2f);
        Kinematic(root);
        root.AddComponent<Level3Snake>().Setup(lane, visual, progress, warning);
        return root;
    }

    public static GameObject MakeLightning(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("Lightning");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        warning.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        Visual(PrimitiveType.Cylinder, warning.transform, Vector3.zero, new Vector3(1.6f, 1.2f, 0.2f), Warning, "Sign");
        Visual(PrimitiveType.Cylinder, warning.transform, new Vector3(0f, -0.2f, 0f), new Vector3(0.15f, 0.5f, 0.15f), Warning, "Pole");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.04f, 0f), new Vector3(1.6f, 0.04f, 1.6f), Warning, "GroundMarker");
        warning.SetActive(true);

        GameObject bolt = new GameObject("Bolt");
        bolt.transform.SetParent(root.transform, false);
        // Main neon-yellow cylinder — wide and obvious
        GameObject outer = Visual(PrimitiveType.Cylinder, bolt.transform,
            new Vector3(0f, 5f, 0f), new Vector3(0.7f, 5f, 0.7f), Lightning, "Outer");
        MakeTransparent(outer.GetComponent<Renderer>(), new Color(1f, 0.95f, 0.05f, 0.88f));
        // Bright white-yellow inner core
        GameObject core = Visual(PrimitiveType.Cylinder, bolt.transform,
            new Vector3(0f, 5f, 0f), new Vector3(0.28f, 5.2f, 0.28f), LightningCore, "Core");
        MakeTransparent(core.GetComponent<Renderer>(), new Color(1f, 1f, 0.85f, 0.95f));
        // Wide ground-flash ring
        GameObject flash = Visual(PrimitiveType.Cylinder, bolt.transform,
            new Vector3(0f, 0.05f, 0f), new Vector3(2.4f, 0.07f, 2.4f), Lightning, "GroundFlash");
        MakeTransparent(flash.GetComponent<Renderer>(), new Color(1f, 0.9f, 0.1f, 0.75f));
        bolt.SetActive(false);

        Kinematic(root);
        root.AddComponent<Level3LightningZone>().Setup(lane, warning, bolt);
        return root;
    }

    public static GameObject MakeTankDisplay(Transform parent, int tankIndex, int lane, float z, Level3PipeRepair repair)
    {
        GameObject root = new GameObject($"Tank{tankIndex + 1}_Display");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.2f, 0f), new Vector3(2.2f, 1.2f, 2.2f), Tank, "TankBody");
        GameObject fill = Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.6f, 0f), new Vector3(2f, 0.6f, 2f), new Color(0.3f, 0.55f, 0.85f), "Fill");

        GameObject flow = Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.35f, 1.2f), new Vector3(0.22f, 0.25f, 0.22f), WaterFlow, "Flow");
        MakeTransparent(flow.GetComponent<Renderer>(), WaterFlow);
        flow.SetActive(false);

        Kinematic(root);
        repair?.BindTank(tankIndex, root, flow, fill);
        return root;
    }

    public static GameObject MakePipeRepair(Transform parent, int tankIndex, int lane, float z)
    {
        int leftLane = Mathf.Clamp(lane, 0, LevelLanes.Count - 2);
        int rightLane = leftLane + 1;
        float leftX = LevelLanes.X(leftLane);
        float rightX = LevelLanes.X(rightLane);
        float midX = (leftX + rightX) * 0.5f;
        Color metal = tankIndex == 0 ? Tank1Pipe : tankIndex == 1 ? Tank2Pipe : Tank3Pipe;

        GameObject root = new GameObject($"PipeRepairStructure_Tank{tankIndex + 1}");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(midX, Level3Ground.SurfaceY, z);

        float leftLocal = leftX - midX;
        float rightLocal = rightX - midX;
        const float topY = 3.15f;
        const float intoGround = -0.7f;
        float leftPostX = leftLocal;
        float rightPostX = rightLocal;
        float postThick = tankIndex == 2 ? 0.7f : 0.55f;

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(leftPostX, (topY + intoGround) * 0.5f, 0f), new Vector3(postThick, (topY - intoGround) * 0.5f, postThick), metal, "LeftVerticalPipe");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(rightPostX, (topY + intoGround) * 0.5f, 0f), new Vector3(postThick, (topY - intoGround) * 0.5f, postThick), metal, "RightVerticalPipe");

        float span = Mathf.Abs(rightPostX - leftPostX);
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, topY, 0f), new Vector3(span + postThick, 0.45f, 0.55f), metal, "HorizontalPipe");

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(leftPostX, intoGround, 0f), new Vector3(0.95f, 0.1f, 0.95f), new Color(0.22f, 0.28f, 0.18f), "LeftGroundEntry");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(rightPostX, intoGround, 0f), new Vector3(0.95f, 0.1f, 0.95f), new Color(0.22f, 0.28f, 0.18f), "RightGroundEntry");

        float yellowWidth = span + 1.8f;
        const float yellowDepth = 1.35f;
        GameObject point = new GameObject("YellowRepairSection");
        point.transform.SetParent(root.transform, false);
        point.transform.localPosition = new Vector3(0f, topY, 0f);
        GameObject glow = Visual(PrimitiveType.Cube, point.transform, Vector3.zero, new Vector3(yellowWidth, 0.85f, yellowDepth), YellowRepair, "Glow");
        MakeTransparent(glow.GetComponent<Renderer>(), new Color(1f, 0.9f, 0.12f, 0.55f));

        BoxCollider gate = root.AddComponent<BoxCollider>();
        gate.isTrigger = true;
        gate.center = new Vector3(0f, topY - 0.2f, 0f);
        gate.size = new Vector3(yellowWidth, 2.4f, 2.1f);
        Level3RepairPoint repair = root.AddComponent<Level3RepairPoint>();
        repair.Setup(tankIndex);
        repair.BindFx(point);

        Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, topY + 0.7f, 0f), new Vector3(1.8f, 0.35f, 0.2f), YellowRepair, "TankLabel");

        Kinematic(root);
        if (Level3Config.EnableSpawnDebug)
        {
            Debug.Log($"[Level3 Spawn] Pipe tank={tankIndex + 1} lanes={leftLane + 1}-{rightLane + 1} z={z:F1}");
        }

        return root;
    }

    public static GameObject MakeBossRepair(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("BossYellowPipe");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);
        GameObject glow = Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 1f, 0f), new Vector3(1.6f, 2f, 1.6f), YellowRepair, "Glow");
        MakeTransparent(glow.GetComponent<Renderer>(), new Color(1f, 0.92f, 0.2f, 0.6f));
        TallTrigger(root, 1.8f, 1.8f, 5f, 1.4f);
        Kinematic(root);
        root.AddComponent<Level3BossRepairPoint>();
        return root;
    }

    public static GameObject MakeAcidRain(Transform parent, int lane, float z)
    {
        const float barHeight = 1.65f;
        const float laneBarWidth = 5.2f;
        const float warningWidth = 7.4f;

        GameObject root = new GameObject("AcidRain");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        // Warning is bigger, like lightning's "before strike" signage.
        GameObject warning = Visual(
            PrimitiveType.Cylinder,
            visual.transform,
            new Vector3(0f, barHeight, 0f),
            new Vector3(1.05f, warningWidth * 0.5f, 1.05f),
            Acid,
            "WarningBar");
        warning.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        MakeTransparent(warning.GetComponent<Renderer>(), new Color(0.55f, 0.95f, 0.35f, 0.18f));

        GameObject active = new GameObject("ActiveRoot");
        active.transform.SetParent(visual.transform, false);

        GameObject bar = Visual(
            PrimitiveType.Cylinder,
            active.transform,
            new Vector3(0f, barHeight, 0f),
            new Vector3(1.05f, laneBarWidth * 0.5f, 1.05f),
            Acid,
            "RainBar");
        bar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        MakeTransparent(bar.GetComponent<Renderer>(), new Color(0.55f, 0.95f, 0.35f, 0.26f));

        GameObject mist = Visual(
            PrimitiveType.Cylinder,
            active.transform,
            new Vector3(0f, 0.12f, 0f),
            new Vector3(1.8f, 0.08f, 1.8f),
            Acid,
            "GroundMist");
        MakeTransparent(mist.GetComponent<Renderer>(), new Color(0.5f, 0.92f, 0.32f, 0.22f));

        warning.SetActive(false);
        active.SetActive(false);

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, barHeight, 0f);
        box.size = new Vector3(laneBarWidth, 2.1f, 2.1f);
        Kinematic(root);
        root.AddComponent<Level3AcidRainZone>().Setup(lane, warning, active);
        return root;
    }

    public static GameObject MakeRollingLog(Transform parent, int lane, float z, int laneSpan = 2)
    {
        int span = Mathf.Clamp(laneSpan, 2, 3);
        int leftLane = Mathf.Clamp(lane, 0, LevelLanes.Count - span);
        int rightLane = leftLane + span - 1;
        float leftX = LevelLanes.X(leftLane);
        float rightX = LevelLanes.X(rightLane);
        float midX = (leftX + rightX) * 0.5f;
        float width = Mathf.Abs(rightX - leftX) + 2.6f;

        GameObject root = new GameObject(span >= 3 ? "RollingLog_3Lane" : "RollingLog");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(midX, Level3Ground.SurfaceY, z);

        GameObject mesh = Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.72f, 0f), new Vector3(1.45f, width * 0.5f, 1.45f), LogBark, "Log");
        mesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(-width * 0.48f, 0.72f, 0f), new Vector3(1.35f, 1.35f, 1.35f), LogDark, "EndL");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(width * 0.48f, 0.72f, 0f), new Vector3(1.35f, 1.35f, 1.35f), LogDark, "EndR");

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.78f, 0f);
        box.size = new Vector3(width, 1.45f, 1.7f);
        Kinematic(root);
        root.AddComponent<Level3RollingLog>().Setup(14f);
        return root;
    }
}
