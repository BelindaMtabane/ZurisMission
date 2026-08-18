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
    public static readonly Color Warthog     = new Color(0.78f, 0.55f, 0.30f);   // light tan-brown, clearly visible
    public static readonly Color WarthogSnout = new Color(0.65f, 0.42f, 0.22f);   // slightly darker for contrast
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
        go.transform.localPosition = RunnerVisualScale.V(localPos);
        go.transform.localScale = RunnerVisualScale.L3V(scale);
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
        box.center = new Vector3(0f, RunnerVisualScale.L3(centerY), 0f);
        box.size = new Vector3(RunnerVisualScale.L3(width), RunnerVisualScale.L3(height), RunnerVisualScale.L3(depth));
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
            BuildNails(root.transform);
        }
        else if (kind == "Hammer")
        {
            BuildHammer(root.transform);
        }
        else if (kind == "Tape")
        {
            BuildTape(root.transform);
        }
        else // Pipe (default)
        {
            BuildPipe(root.transform);
        }

        TallTrigger(root, 1.8f, 1.8f, 3.5f, 1.2f);
        Kinematic(root);
        root.AddComponent<Level3MaterialPickup>().Setup(kind, Level3Config.DefaultMaterialPickup);
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    // ── Pipe ──────────────────────────────────────────────────────────────────
    // A thick horizontal pipe section lying on the ground — clearly a cylinder tube.
    static void BuildPipe(Transform root)
    {
        // Main pipe body — horizontal cylinder (rotated 90° on Z so it lies flat)
        GameObject body = Visual(PrimitiveType.Cylinder, root,
            new Vector3(0f, 0.85f, 0f),
            new Vector3(0.55f, 1.4f, 0.55f),
            PipeMetal, "PipeBody");
        body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        // Left end cap — darker disc sealing the tube end
        Color capColor = new Color(0.38f, 0.40f, 0.45f);
        GameObject capL = Visual(PrimitiveType.Cylinder, root,
            new Vector3(-1.35f, 0.85f, 0f),
            new Vector3(0.58f, 0.12f, 0.58f),
            capColor, "CapL");
        capL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        // Right end cap
        GameObject capR = Visual(PrimitiveType.Cylinder, root,
            new Vector3(1.35f, 0.85f, 0f),
            new Vector3(0.58f, 0.12f, 0.58f),
            capColor, "CapR");
        capR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        // Collar rings — decorative bands near each end (slightly wider, darker)
        Color collarColor = new Color(0.30f, 0.32f, 0.36f);
        GameObject collarL = Visual(PrimitiveType.Cylinder, root,
            new Vector3(-1.05f, 0.85f, 0f),
            new Vector3(0.65f, 0.14f, 0.65f),
            collarColor, "CollarL");
        collarL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        GameObject collarR = Visual(PrimitiveType.Cylinder, root,
            new Vector3(1.05f, 0.85f, 0f),
            new Vector3(0.65f, 0.14f, 0.65f),
            collarColor, "CollarR");
        collarR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
    }

    // ── Nails ─────────────────────────────────────────────────────────────────
    // Five nails arranged in a fan: each nail has a long shaft and a wide flat head.
    static void BuildNails(Transform root)
    {
        Color head  = NailMetal;
        Color shaft = new Color(0.50f, 0.50f, 0.54f);

        // Nail positions and tilt angles (fan spread)
        float[] xOffsets = { -0.55f, -0.27f, 0f, 0.27f, 0.55f };
        float[] tilts    = { -22f,   -10f,   0f,  10f,   22f   };

        for (int i = 0; i < 5; i++)
        {
            float x   = xOffsets[i];
            float tilt = tilts[i];
            float baseY = 0.12f;

            // Shaft — long thin cylinder standing upright, slightly tilted
            GameObject shaft_go = Visual(PrimitiveType.Cylinder, root,
                new Vector3(x, baseY + 0.6f, 0f),
                new Vector3(0.07f, 0.6f, 0.07f),
                shaft, $"Shaft_{i}");
            shaft_go.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);

            // Head — flat wide cylinder on top of the shaft
            GameObject head_go = Visual(PrimitiveType.Cylinder, root,
                new Vector3(x + Mathf.Sin(tilt * Mathf.Deg2Rad) * 0.55f, baseY + 1.22f, 0f),
                new Vector3(0.22f, 0.055f, 0.22f),
                head, $"Head_{i}");
            head_go.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }

        // Small wooden board they rest on — so they don't float in mid-air
        Visual(PrimitiveType.Cube, root,
            new Vector3(0f, 0.06f, 0f),
            new Vector3(1.5f, 0.1f, 0.5f),
            new Color(0.52f, 0.34f, 0.16f), "Board");
    }

    // ── Tape ──────────────────────────────────────────────────────────────────
    // Donut shape: wide outer roll + hollow inner hole + a dangling strip of tape.
    static void BuildTape(Transform root)
    {
        Color tapeColor  = Tape;                              // yellow-amber
        Color tapeEdge   = new Color(0.75f, 0.58f, 0.08f);  // slightly darker edge
        Color tapeStrip  = new Color(0.96f, 0.84f, 0.28f);  // bright tape face
        Color core       = new Color(0.88f, 0.88f, 0.88f);  // grey cardboard inner core

        float rollY = 1.0f;

        // Outer roll body — flat wide cylinder (the reel)
        Visual(PrimitiveType.Cylinder, root,
            new Vector3(0f, rollY, 0f),
            new Vector3(1.1f, 0.28f, 1.1f),
            tapeColor, "OuterRoll");

        // Inner core cylinder — slightly taller so it pokes through the hole
        Visual(PrimitiveType.Cylinder, root,
            new Vector3(0f, rollY, 0f),
            new Vector3(0.36f, 0.34f, 0.36f),
            core, "InnerCore");

        // Top face ring — shows the tape wound on top
        Visual(PrimitiveType.Cylinder, root,
            new Vector3(0f, rollY + 0.27f, 0f),
            new Vector3(1.08f, 0.04f, 1.08f),
            tapeStrip, "TopFace");

        // Bottom face ring
        Visual(PrimitiveType.Cylinder, root,
            new Vector3(0f, rollY - 0.27f, 0f),
            new Vector3(1.08f, 0.04f, 1.08f),
            tapeStrip, "BottomFace");

        // Edge band around the outer rim — slightly darker to show depth
        Visual(PrimitiveType.Cylinder, root,
            new Vector3(0f, rollY, 0f),
            new Vector3(1.14f, 0.22f, 1.14f),
            tapeEdge, "RimBand");

        // Dangling tape strip hanging off the side — three thin cubes cascading down
        Color stripColor = tapeStrip;
        Visual(PrimitiveType.Cube, root,
            new Vector3(1.06f, rollY - 0.05f, 0f),
            new Vector3(0.12f, 0.32f, 0.58f),
            stripColor, "StripTop");
        Visual(PrimitiveType.Cube, root,
            new Vector3(1.14f, rollY - 0.38f, 0f),
            new Vector3(0.10f, 0.26f, 0.56f),
            stripColor, "StripMid");
        Visual(PrimitiveType.Cube, root,
            new Vector3(1.18f, rollY - 0.62f, 0f),
            new Vector3(0.08f, 0.18f, 0.52f),
            stripColor, "StripBot");
    }

    // ── Hammer ────────────────────────────────────────────────────────────────
    // Classic T-shape: long wooden handle + chunky metal head with a striking face.
    static void BuildHammer(Transform root)
    {
        float handleBaseY = 0.1f;
        float handleH     = 1.55f;   // total handle length
        float headY       = handleBaseY + handleH + 0.18f;

        // Handle — long rounded rectangular shaft
        Visual(PrimitiveType.Cube, root,
            new Vector3(0f, handleBaseY + handleH * 0.5f, 0f),
            new Vector3(0.18f, handleH, 0.18f),
            HammerHandle, "Handle");

        // Grip wrap — slightly darker band at the bottom of the handle
        Visual(PrimitiveType.Cube, root,
            new Vector3(0f, handleBaseY + 0.22f, 0f),
            new Vector3(0.22f, 0.3f, 0.22f),
            new Color(0.28f, 0.16f, 0.06f), "GripWrap");

        // Main head body — wide horizontal block (the bulk of the hammerhead)
        Visual(PrimitiveType.Cube, root,
            new Vector3(0f, headY, 0f),
            new Vector3(1.05f, 0.38f, 0.36f),
            HammerHead, "HeadBody");

        // Striking face — brighter flat plate on the right side
        Visual(PrimitiveType.Cube, root,
            new Vector3(0.54f, headY, 0f),
            new Vector3(0.12f, 0.36f, 0.34f),
            new Color(0.70f, 0.70f, 0.74f), "StrikeFace");

        // Poll (back face) — slightly rounded flat plate on the left
        Visual(PrimitiveType.Cube, root,
            new Vector3(-0.54f, headY, 0f),
            new Vector3(0.10f, 0.32f, 0.32f),
            new Color(0.60f, 0.60f, 0.64f), "Poll");

        // Neck — narrowing block between handle top and head (wedge look)
        Visual(PrimitiveType.Cube, root,
            new Vector3(0f, headY - 0.24f, 0f),
            new Vector3(0.26f, 0.22f, 0.26f),
            HammerHead, "Neck");

        // Top chamfer — small bevel on the crown of the head
        Visual(PrimitiveType.Cube, root,
            new Vector3(0f, headY + 0.19f, 0f),
            new Vector3(0.92f, 0.08f, 0.30f),
            new Color(0.48f, 0.48f, 0.52f), "TopChamfer");
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

        // Warning: just an empty container — no yellow cylinder
        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        warning.SetActive(true);

        // Body — wide tan-brown rectangle
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.65f, 0f),   new Vector3(2.6f, 1.0f, 1.2f), Warthog,      "Body");
        // Head — slightly darker snout box
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.4f, 0.80f, 0f), new Vector3(0.85f, 0.65f, 0.9f), WarthogSnout, "Head");
        // Tusks — two small white cubes on each side of the snout
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.85f, 0.58f,  0.25f), new Vector3(0.25f, 0.18f, 0.18f), Color.white, "TuskL");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.85f, 0.58f, -0.25f), new Vector3(0.25f, 0.18f, 0.18f), Color.white, "TuskR");
        // Legs — four short dark brown stubs
        Visual(PrimitiveType.Cube, visual.transform, new Vector3( 0.7f, 0.18f,  0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegFL");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3( 0.7f, 0.18f, -0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegFR");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(-0.7f, 0.18f,  0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegBL");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(-0.7f, 0.18f, -0.45f), new Vector3(0.3f, 0.36f, 0.3f), WarthogSnout, "LegBR");
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
        // Tall neon-yellow outer column — reaches high into the sky
        GameObject outer = Visual(PrimitiveType.Cylinder, bolt.transform,
            new Vector3(0f, 22f, 0f), new Vector3(0.75f, 22f, 0.75f), Lightning, "Outer");
        MakeTransparent(outer.GetComponent<Renderer>(), new Color(1f, 0.95f, 0.05f, 0.88f));
        // Bright white-yellow inner core — taller than the outer for a glow-through effect
        GameObject core = Visual(PrimitiveType.Cylinder, bolt.transform,
            new Vector3(0f, 22f, 0f), new Vector3(0.28f, 23f, 0.28f), LightningCore, "Core");
        MakeTransparent(core.GetComponent<Renderer>(), new Color(1f, 1f, 0.85f, 0.95f));
        // Wide ground-flash ring at the base
        GameObject flash = Visual(PrimitiveType.Cylinder, bolt.transform,
            new Vector3(0f, 0.05f, 0f), new Vector3(2.8f, 0.07f, 2.8f), Lightning, "GroundFlash");
        MakeTransparent(flash.GetComponent<Renderer>(), new Color(1f, 0.9f, 0.1f, 0.78f));
        bolt.SetActive(false);

        BoxCollider hitbox = root.AddComponent<BoxCollider>();
        hitbox.isTrigger = true;
        hitbox.center = RunnerVisualScale.L3V(new Vector3(0f, 11f, 0f));
        hitbox.size = RunnerVisualScale.L3V(new Vector3(2.8f, 22f, 2.8f));

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
        // One single large vertical column from sky to ground.
        const float colCenterY = 20f;
        const float colHalfH   = 20f;
        const float colRadius  = 1.1f;

        GameObject root = new GameObject("AcidRain");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        GameObject warnOuter = Visual(PrimitiveType.Cylinder, warning.transform,
            new Vector3(0f, colCenterY, 0f),
            new Vector3(colRadius, colHalfH, colRadius),
            Acid, "WarnOuter");
        MakeTransparent(warnOuter.GetComponent<Renderer>(), new Color(0.45f, 0.92f, 0.22f, 0.16f));

        GameObject active = new GameObject("ActiveRoot");
        active.transform.SetParent(root.transform, false);
        GameObject outer = Visual(PrimitiveType.Cylinder, active.transform,
            new Vector3(0f, colCenterY, 0f),
            new Vector3(colRadius, colHalfH, colRadius),
            Acid, "Outer");
        MakeTransparent(outer.GetComponent<Renderer>(), new Color(0.45f, 0.95f, 0.22f, 0.30f));

        warning.SetActive(false);
        active.SetActive(false);

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, RunnerVisualScale.L3(colCenterY), 0f);
        box.size = RunnerVisualScale.L3V(new Vector3(colRadius * 2.4f, colHalfH * 2f, colRadius * 2.4f));

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
        box.center = RunnerVisualScale.L3V(new Vector3(0f, 0.78f, 0f));
        box.size = RunnerVisualScale.L3V(new Vector3(width, 2.2f, 1.9f));
        Kinematic(root);
        root.AddComponent<Level3RollingLog>().Setup(14f);
        return root;
    }
}
