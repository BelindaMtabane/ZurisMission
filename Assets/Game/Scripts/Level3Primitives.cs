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
    public static readonly Color Warning = new Color(1f, 0.78f, 0.15f);
    public static readonly Color Acid = new Color(0.35f, 0.92f, 0.22f, 0.38f);
    public static readonly Color YellowRepair = new Color(1f, 0.88f, 0.15f);
    public static readonly Color Snake = new Color(0.18f, 0.72f, 0.22f);
    public static readonly Color Warthog = new Color(0.42f, 0.28f, 0.16f);
    public static readonly Color Tank = new Color(0.55f, 0.6f, 0.66f);
    public static readonly Color WaterFlow = new Color(0.25f, 0.7f, 1f, 0.55f);

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
        root.AddComponent<Level3Obstacle>().Setup(Level3ObstacleKind.Rock, false);
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

    public static GameObject MakeBucket(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("BucketWater");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z, 0.5f);
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.7f, 0.45f, 0.7f), Bucket, "Bucket");
        TallTrigger(root, 1.4f, 1.4f);
        Kinematic(root);
        root.AddComponent<Level3BucketPickup>();
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    public static GameObject MakeHealth(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("HealthPickup");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z, 0.5f);
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.7f, 0.7f, 0.7f), Health, "Heart");
        TallTrigger(root, 1.4f, 1.4f);
        Kinematic(root);
        root.AddComponent<Level3HealthPickup>();
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    public static GameObject MakeLeaf(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("LeafProtectionPickup");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z, 0.55f);
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.5f, 0f), new Vector3(0.9f, 0.35f, 0.55f), Leaf, "Leaf");
        TallTrigger(root, 1.5f, 1.5f);
        Kinematic(root);
        root.AddComponent<Level3LeafPickup>();
        root.AddComponent<Level3PickupBob>();
        return root;
    }

    public static GameObject MakeSnake(Transform parent, float z, Level3EnemyPace pace, bool goRight)
    {
        GameObject root = new GameObject("Snake");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(LevelLanes.PathCenterX, Level3Ground.SurfaceY, z);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        warning.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        Visual(PrimitiveType.Cylinder, warning.transform, Vector3.zero, new Vector3(10f, 0.04f, 1.2f), Warning, "Shadow");
        warning.SetActive(false);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.35f, 0f), new Vector3(3.2f, 0.45f, 0.55f), Snake, "Body");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.7f, 0.4f, 0f), new Vector3(0.7f, 0.5f, 0.5f), Snake, "Head");
        visual.SetActive(false);

        TallTrigger(root, 3.4f, 1.2f, 4f, 1.2f);
        Kinematic(root);
        root.AddComponent<Level3HorizontalEnemy>().Setup(Level3HorizontalEnemy.EnemyKind.Snake, pace, goRight, warning, visual);
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
        warning.SetActive(false);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.7f, 0f), new Vector3(2.4f, 1.1f, 1.1f), Warthog, "Body");
        Visual(PrimitiveType.Cube, visual.transform, new Vector3(1.2f, 0.85f, 0f), new Vector3(0.9f, 0.7f, 0.8f), Warthog, "Head");
        visual.SetActive(false);

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

    public static GameObject MakeAcidRain(Transform parent, float z, int[] lanes, float progress)
    {
        GameObject root = new GameObject("AcidRain");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(LevelLanes.PathCenterX, Level3Ground.SurfaceY, z);

        GameObject warning = new GameObject("Warning");
        warning.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cube, warning.transform, new Vector3(0f, 2.4f, 0f), new Vector3(8f, 0.12f, 0.2f), Warning, "Bar");
        warning.SetActive(false);

        GameObject rain = new GameObject("Rain");
        rain.transform.SetParent(root.transform, false);
        for (int i = 0; i < lanes.Length; i++)
        {
            float x = LevelLanes.X(lanes[i]) - LevelLanes.PathCenterX;
            GameObject drop = Visual(PrimitiveType.Cylinder, rain.transform, new Vector3(x, 6f, 0f), new Vector3(0.28f, 6f, 0.28f), Acid, $"Drop_{i}");
            MakeTransparent(drop.GetComponent<Renderer>(), Acid);
        }
        rain.SetActive(false);

        float width = Mathf.Abs(LevelLanes.X(3) - LevelLanes.X(0)) + 4f;
        TallTrigger(root, width, 4f, 12f, 4f);
        Kinematic(root);
        root.AddComponent<Level3AcidRainZone>().Setup(progress, lanes, warning, rain);
        return root;
    }

    public static GameObject MakeTank(Transform parent, int tankIndex, int lane, float z, Level3PipeRepair repair)
    {
        GameObject root = new GameObject($"Tank{tankIndex + 1}");
        root.transform.SetParent(parent, false);
        root.transform.position = Level3Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.6f, -2.2f), new Vector3(2.2f, 1.6f, 2.2f), Tank, "TankBody");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0.2f, 1.8f), PipeMetal, "BrokenPipe");

        GameObject flow = Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.5f, 0.4f), new Vector3(0.25f, 0.35f, 0.25f), WaterFlow, "Flow");
        MakeTransparent(flow.GetComponent<Renderer>(), WaterFlow);
        flow.SetActive(false);

        GameObject point = new GameObject("YellowRepair");
        point.transform.SetParent(root.transform, false);
        point.transform.localPosition = new Vector3(0f, 0.7f, 0.2f);
        GameObject glow = Visual(PrimitiveType.Cube, point.transform, Vector3.zero, new Vector3(1.2f, 1.4f, 1.2f), YellowRepair, "Glow");
        MakeTransparent(glow.GetComponent<Renderer>(), new Color(1f, 0.9f, 0.15f, 0.55f));
        TallTrigger(point, 1.6f, 1.6f, 4f, 1f);
        Kinematic(point);
        point.AddComponent<Level3RepairPoint>().Setup(tankIndex);

        Kinematic(root);
        repair?.BindTank(tankIndex, root, flow);
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
}
