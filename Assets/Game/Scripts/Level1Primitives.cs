using UnityEngine;

public static class Level1Primitives
{
    public static readonly Color Cactus = new Color(0.18f, 0.62f, 0.22f);
    public static readonly Color CactusAccent = new Color(0.12f, 0.42f, 0.16f);
    public static readonly Color Rock = new Color(0.48f, 0.32f, 0.18f);
    public static readonly Color Sand = new Color(0.84f, 0.70f, 0.32f);
    public static readonly Color Dust = new Color(0.78f, 0.52f, 0.18f);
    public static readonly Color Dew = new Color(0.35f, 0.75f, 0.85f);
    public static readonly Color WaterPool = new Color(0.12f, 0.55f, 0.95f);
    public static readonly Color WaterPoolDeep = new Color(0.05f, 0.35f, 0.78f);
    public static readonly Color Health = new Color(0.92f, 0.16f, 0.18f);
    public static readonly Color SuperFruit = new Color(0.95f, 0.08f, 0.08f);
    public static readonly Color AloeGreen = new Color(0.42f, 0.88f, 0.38f);
    public static readonly Color AloeLeaf = new Color(0.28f, 0.72f, 0.22f);
    public static readonly Color HammerHandle = new Color(0.45f, 0.28f, 0.12f);
    public static readonly Color HammerHead = new Color(0.55f, 0.55f, 0.58f);
    public static readonly Color BrickRed = new Color(0.72f, 0.28f, 0.18f);
    public static readonly Color CementBag = new Color(0.82f, 0.78f, 0.68f);
    public static readonly Color SnakeBodyGreen = new Color(0.15f, 0.88f, 0.25f);
    public static readonly Color SnakeBodyDarkGreen = new Color(0.12f, 0.72f, 0.20f);
    public static readonly Color SnakeBodyBrown = new Color(0.42f, 0.28f, 0.12f);
    public static readonly Color SnakeHead = new Color(0.10f, 0.68f, 0.16f);
    public static readonly Color SnakeTongue = new Color(0.95f, 0.1f, 0.12f);
    public static readonly Color SnakeWarning = new Color(1f, 0.78f, 0.12f);
    public static readonly Color SnakeWarningAccent = new Color(0.95f, 0.22f, 0.12f);
    public static readonly Color SnakeDangerCactus = new Color(0.55f, 0.12f, 0.14f);
    public static readonly Color SnakeDangerCactusSpike = new Color(0.72f, 0.18f, 0.16f);
    public static readonly Color LogBrown = new Color(0.45f, 0.28f, 0.12f);
    public static readonly Color LogDark = new Color(0.32f, 0.18f, 0.08f);
    public static readonly Color LogBark = new Color(0.38f, 0.22f, 0.10f);

    public const float CactusWallDamage = 3f;
    public const float SandPitDamage = 1.5f;
    public const float RockClusterDamage = 1f;
    public const float DustHazardDamage = 6f;
    public const float LowBarrierDamage = 1f;
    public const float LogDamage = 1f;
    public const float JumpLogHalfLength = 1.85f;
    public const float RollingLogHalfLength = 24f;
    public const float RollingLogRadius = 0.58f;

    public static GameObject Visual(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Color color, string name)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = color;
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        return go;
    }

    public static BoxCollider TallTrigger(GameObject root, float width, float depth)
    {
        BoxCollider box = root.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = root.AddComponent<BoxCollider>();
        }

        box.isTrigger = true;
        box.center = new Vector3(0f, 4f, 0f);
        box.size = new Vector3(width, 10f, depth);
        return box;
    }

    public static GameObject MakeCactusWater(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("CactusWater");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.72f, 0f), new Vector3(0.65f, 0.72f, 0.65f), Cactus, "Stem");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0.42f, 1.02f, 0f), new Vector3(0.32f, 0.48f, 0.32f), CactusAccent, "ArmR");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(-0.42f, 1.02f, 0f), new Vector3(0.32f, 0.48f, 0.32f), CactusAccent, "ArmL");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.42f, 0f), new Vector3(0.52f, 0.38f, 0.52f), Cactus, "Top");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.28f, 1.65f, 0.18f), new Vector3(0.32f, 0.32f, 0.32f), Dew, "WaterDrop");

        TallTrigger(root, 1.6f, 1.4f);
        KinematicBody(root);
        AnchorPickup(root);
        root.AddComponent<Level1CactusPickup>();
        return root;
    }

    public static GameObject MakeWaterPool(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("WaterPool");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.1f, 0f), new Vector3(3.4f, 0.1f, 3.4f), WaterPoolDeep, "PoolBase");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.22f, 0f), new Vector3(2.8f, 0.08f, 2.8f), WaterPool, "PoolSurface");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.6f, 0.35f, 0.4f), new Vector3(0.5f, 0.12f, 0.5f), Color.white, "Ripple");

        TallTrigger(root, 3.0f, 3.0f);
        KinematicBody(root);
        AnchorPickup(root);
        root.AddComponent<Level1WaterPoolPickup>();
        return root;
    }

    public static GameObject MakeSuperFruit(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("SuperFruit");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.5f, 0f), new Vector3(1.35f, 1.35f, 1.35f), SuperFruit, "Fruit");
        Visual(PrimitiveType.Capsule, root.transform, new Vector3(0.45f, 2.05f, 0f), new Vector3(0.22f, 0.45f, 0.22f), new Color(0.2f, 0.75f, 0.15f), "Stem");

        TallTrigger(root, 1.8f, 1.6f);
        KinematicBody(root);
        AnchorPickup(root);
        root.AddComponent<Level1SuperFruitPickup>();
        return root;
    }

    public static GameObject MakeAloePlant(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("AloePlant");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Capsule, root.transform, new Vector3(-0.55f, 1.1f, 0f), new Vector3(0.45f, 1.2f, 0.45f), AloeLeaf, "LeafL");
        Visual(PrimitiveType.Capsule, root.transform, new Vector3(0.55f, 1.15f, 0f), new Vector3(0.45f, 1.25f, 0.45f), AloeLeaf, "LeafR");
        Visual(PrimitiveType.Capsule, root.transform, new Vector3(0f, 1.35f, 0f), new Vector3(0.5f, 1.4f, 0.5f), AloeGreen, "LeafCenter");
        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0.35f, 0.35f), new Color(0.35f, 0.55f, 0.2f), "Base");

        TallTrigger(root, 2.0f, 1.8f);
        KinematicBody(root);
        AnchorPickup(root);
        root.AddComponent<Level1AloePickup>();
        return root;
    }

    public static GameObject MakeMaterialTool(Transform parent, int lane, float z, Level1MaterialKind kind)
    {
        GameObject root = new GameObject($"Material_{kind}");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        if (kind == Level1MaterialKind.Hammer)
        {
            Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.0f, 0f), new Vector3(0.18f, 1.0f, 0.18f), HammerHandle, "Handle");
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 1.85f, 0f), new Vector3(0.85f, 0.45f, 0.55f), HammerHead, "Head");
        }
        else if (kind == Level1MaterialKind.Brick)
        {
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(1.2f, 0.55f, 0.75f), BrickRed, "BrickA");
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0.15f, 1.05f, 0.1f), new Vector3(1.1f, 0.5f, 0.7f), BrickRed * 0.9f, "BrickB");
        }
        else
        {
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.85f, 0f), new Vector3(1.1f, 1.1f, 0.65f), CementBag, "Bag");
            Visual(PrimitiveType.Cube, root.transform, new Vector3(0f, 1.55f, 0f), new Vector3(0.95f, 0.25f, 0.55f), CementBag * 0.85f, "BagTop");
        }

        TallTrigger(root, 1.8f, 1.6f);
        KinematicBody(root);
        AnchorPickup(root);
        Level1MaterialPickup pickup = root.AddComponent<Level1MaterialPickup>();
        pickup.Setup(kind, 10);
        return root;
    }

    static GameObject HorizontalLogVisual(Transform parent, Vector3 localPos, float halfLength, float radius, Color color, string name)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        go.transform.localScale = new Vector3(radius, halfLength, radius);
        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = color;
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        return go;
    }

    static GameObject HorizontalCactusVisual(Transform parent, Vector3 localPos, float halfLength, Color color, string name)
    {
        GameObject bar = HorizontalLogVisual(parent, localPos, halfLength, 0.55f, color, name);
        float[] armX = { -0.65f, 0f, 0.65f };
        for (int i = 0; i < armX.Length; i++)
        {
            Visual(PrimitiveType.Sphere, parent, localPos + new Vector3(armX[i], 0.38f, 0f), new Vector3(0.42f, 0.32f, 0.42f), CactusAccent, $"{name}_Arm_{i}");
        }

        return bar;
    }

    static GameObject LogSegment(Transform parent, Vector3 localPos, float length, Color color, string name)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(0.62f, length * 0.5f, 0.62f);
        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = color;
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        return go;
    }

    public static GameObject MakeLogBarrier(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("LogBarrier");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        HorizontalLogVisual(root.transform, new Vector3(0f, 0.62f, 0f), JumpLogHalfLength, 0.52f, LogBrown, "JumpLog");

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.72f, 0f);
        box.size = new Vector3(3.6f, 0.95f, 1.4f);

        Level1Obstacle obstacle = root.AddComponent<Level1Obstacle>();
        obstacle.Setup(Level1ObstacleKind.Log, LogDamage, true);
        return root;
    }

    public static GameObject MakeRollingLog(Vector3 worldPos, int lane)
    {
        GameObject root = new GameObject("RollingLog");
        root.transform.position = worldPos;

        float span = RollingLogHalfLength * 2f;
        HorizontalLogVisual(root.transform, new Vector3(0f, 0.62f, 0f), RollingLogHalfLength, RollingLogRadius, LogDark, "RollingLogMesh");

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.72f, 0f);
        box.size = new Vector3(span, 0.95f, 1.5f);

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        root.AddComponent<Level1RollingLog>();
        return root;
    }

    public static GameObject MakeHealthPickup(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("HealthPickup");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.35f, 0f), new Vector3(1.1f, 1.1f, 1.1f), Health, "Fruit");
        Visual(PrimitiveType.Capsule, root.transform, new Vector3(0.35f, 1.85f, 0f), new Vector3(0.18f, 0.35f, 0.18f), new Color(0.28f, 0.78f, 0.22f), "Leaf");

        TallTrigger(root, 1.6f, 1.4f);
        KinematicBody(root);
        AnchorPickup(root);
        Level1StatPickup pickup = root.AddComponent<Level1StatPickup>();
        pickup.Setup(Level1StatPickupType.Health, 10f);
        return root;
    }

    public static GameObject MakeCactusWall(Transform parent, bool[] blockedLanes, float z)
    {
        GameObject root = new GameObject("CactusWall");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(0f, Level1Ground.SurfaceY, z);

        float minTriggerX = 999f;
        float maxTriggerX = -999f;

        for (int lane = 0; lane < LevelLanes.Count; lane++)
        {
            if (blockedLanes == null || lane >= blockedLanes.Length || !blockedLanes[lane])
            {
                continue;
            }

            float laneX = LevelLanes.X(lane);
            minTriggerX = Mathf.Min(minTriggerX, laneX);
            maxTriggerX = Mathf.Max(maxTriggerX, laneX);

            GameObject pillar = new GameObject($"DangerCactus_L{lane + 1}");
            pillar.transform.SetParent(root.transform, false);
            pillar.transform.localPosition = new Vector3(laneX, 0f, 0f);

            Visual(PrimitiveType.Cube, pillar.transform, new Vector3(0f, 0.55f, 0f), new Vector3(1.1f, 1.1f, 1.1f), SnakeDangerCactus, "Base");
            Visual(PrimitiveType.Cube, pillar.transform, new Vector3(0f, 1.15f, 0f), new Vector3(0.85f, 0.85f, 0.85f), SnakeDangerCactus, "Mid");
            Visual(PrimitiveType.Cube, pillar.transform, new Vector3(0.35f, 1.45f, 0f), new Vector3(0.35f, 0.35f, 0.35f), SnakeDangerCactusSpike, "SpikeR");
            Visual(PrimitiveType.Cube, pillar.transform, new Vector3(-0.35f, 1.45f, 0f), new Vector3(0.35f, 0.35f, 0.35f), SnakeDangerCactusSpike, "SpikeL");

            BoxCollider laneBox = pillar.AddComponent<BoxCollider>();
            laneBox.isTrigger = true;
            laneBox.center = new Vector3(0f, 0.85f, 0f);
            laneBox.size = new Vector3(1.4f, 1.7f, 2.2f);

            Level1Obstacle obstacle = pillar.AddComponent<Level1Obstacle>();
            obstacle.Setup(Level1ObstacleKind.Cactus, CactusWallDamage, false);
        }

        if (minTriggerX < maxTriggerX)
        {
            Visual(PrimitiveType.Cube, root.transform, new Vector3((minTriggerX + maxTriggerX) * 0.5f, 2.4f, 0f),
                new Vector3(Mathf.Max(2f, maxTriggerX - minTriggerX + 2f), 0.08f, 0.08f),
                new Color(1f, 0.85f, 0.15f), "WallWarning");
        }

        return root;
    }

    public static GameObject MakeRockClusterLanes(Transform parent, int[] lanes, float z)
    {
        GameObject root = new GameObject("RockCluster");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(0f, Level1Ground.SurfaceY, z);

        for (int i = 0; i < lanes.Length; i++)
        {
            float laneX = LevelLanes.X(lanes[i]);
            Visual(PrimitiveType.Cube, root.transform, new Vector3(laneX, 0.55f, 0f), new Vector3(1.6f, 1.1f, 1.4f), Rock, $"Boulder_{i}");
            Visual(PrimitiveType.Sphere, root.transform, new Vector3(laneX + 0.5f, 0.35f, 0.35f), new Vector3(0.9f, 0.7f, 0.9f), Rock, $"Chunk_{i}");
        }

        TallTrigger(root, 3.6f, 2.2f);
        Level1Obstacle obstacle = root.AddComponent<Level1Obstacle>();
        obstacle.Setup(Level1ObstacleKind.Rock, RockClusterDamage, false);
        return root;
    }

    public static GameObject MakeRollingLogLesson(Transform parent, float z)
    {
        GameObject root = new GameObject("RollingLogLesson");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(0f, Level1Ground.SurfaceY, z);
        root.AddComponent<Level1RollingLogLesson>();
        return root;
    }

    public static GameObject MakeCactusCluster(Transform parent, int[] lanes, float z)
    {
        GameObject root = new GameObject("CactusCluster");
        root.transform.SetParent(parent, false);

        float minX = LevelLanes.X(lanes[0]);
        float maxX = LevelLanes.X(lanes[0]);
        for (int i = 1; i < lanes.Length; i++)
        {
            float laneX = LevelLanes.X(lanes[i]);
            minX = Mathf.Min(minX, laneX);
            maxX = Mathf.Max(maxX, laneX);
        }

        const float lanePadding = 2.8f;
        minX -= lanePadding;
        maxX += lanePadding;
        float mid = (minX + maxX) * 0.5f;
        float halfWidth = (maxX - minX) * 0.5f;
        root.transform.position = new Vector3(mid, Level1Ground.SurfaceY, z);

        HorizontalCactusVisual(root.transform, new Vector3(0f, 0.72f, 0f), halfWidth, Cactus, "CactusBar");
        int armCount = Mathf.Max(4, lanes.Length * 2);
        for (int i = 0; i < armCount; i++)
        {
            float t = armCount == 1 ? 0.5f : i / (armCount - 1f);
            float localX = Mathf.Lerp(-halfWidth * 0.9f, halfWidth * 0.9f, t);
            Visual(PrimitiveType.Sphere, root.transform, new Vector3(localX, 1.08f, 0f), new Vector3(0.48f, 0.36f, 0.48f), CactusAccent, $"Top_{i}");
        }

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.82f, 0f);
        box.size = new Vector3(halfWidth * 2f, 1.15f, 2.4f);

        Level1Obstacle obstacle = root.AddComponent<Level1Obstacle>();
        obstacle.Setup(Level1ObstacleKind.Cactus, CactusWallDamage, false);
        return root;
    }

    public static GameObject MakeRockCluster(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("RockCluster");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.7f, 0f), new Vector3(1.8f, 1.2f, 1.6f), Rock, "Boulder");
        Visual(PrimitiveType.Cube, root.transform, new Vector3(0.7f, 0.45f, 0.35f), new Vector3(1.0f, 0.8f, 1.0f), Rock, "ChunkA");
        Visual(PrimitiveType.Sphere, root.transform, new Vector3(-0.65f, 0.35f, -0.3f), new Vector3(0.9f, 0.7f, 0.9f), Rock, "ChunkB");

        TallTrigger(root, 2.4f, 2.0f);
        Level1Obstacle obstacle = root.AddComponent<Level1Obstacle>();
        obstacle.Setup(Level1ObstacleKind.Rock, RockClusterDamage, false);
        return root;
    }

    public static GameObject MakeSandPit(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("SandPit");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.12f, 0f), new Vector3(3.2f, 0.12f, 3.2f), Sand, "Pit");

        TallTrigger(root, 3.0f, 3.0f);
        Level1Obstacle obstacle = root.AddComponent<Level1Obstacle>();
        obstacle.Setup(Level1ObstacleKind.SandPit, SandPitDamage, false);
        return root;
    }

    public static GameObject MakeDustDevil(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("DustDevil");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        GameObject spinRoot = new GameObject("SpinVisual");
        spinRoot.transform.SetParent(root.transform, false);

        Visual(PrimitiveType.Cylinder, spinRoot.transform, new Vector3(0f, 0.08f, 0f), new Vector3(3.2f, 0.08f, 3.2f), Dust, "DryGround");
        Visual(PrimitiveType.Cylinder, spinRoot.transform, new Vector3(0f, 2.4f, 0f), new Vector3(1.4f, 2.4f, 1.4f), new Color(0.9f, 0.55f, 0.15f, 0.65f), "Column");
        Visual(PrimitiveType.Cylinder, spinRoot.transform, new Vector3(0f, 4.2f, 0f), new Vector3(0.9f, 1.6f, 0.9f), new Color(1f, 0.65f, 0.2f, 0.5f), "Top");

        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = Dust;
        main.startSize = 1.1f;
        main.startLifetime = 1.6f;
        main.startSpeed = 2.4f;
        var emission = ps.emission;
        emission.rateOverTime = 28f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 2f;

        Level1DustDevilSpin spin = root.AddComponent<Level1DustDevilSpin>();
        spin.SetVisualRoot(spinRoot.transform);

        TallTrigger(root, 3.2f, 3.2f);
        Level1Obstacle obstacle = root.AddComponent<Level1Obstacle>();
        obstacle.Setup(Level1ObstacleKind.DustDevil, DustHazardDamage, false);
        return root;
    }

    public static GameObject MakeLowCactusBarrier(Transform parent, int lane, float z)
    {
        GameObject root = new GameObject("LowCactusBarrier");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z);

        float[] rowZ = { -2.8f, -1.4f, 0f, 1.4f, 2.8f, 4.2f, 5.6f };
        for (int i = 0; i < rowZ.Length; i++)
        {
            float localZ = rowZ[i];
            Visual(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 0.75f, localZ), new Vector3(0.55f, 0.75f, 0.55f), Cactus, $"Stem_{i}");
            Visual(PrimitiveType.Sphere, root.transform, new Vector3(0.45f, 1.05f, localZ), new Vector3(0.45f, 0.35f, 0.35f), CactusAccent, $"Arm_{i}");
            if (i % 2 == 0)
            {
                Visual(PrimitiveType.Sphere, root.transform, new Vector3(-0.4f, 0.95f, localZ + 0.15f), new Vector3(0.38f, 0.32f, 0.32f), Cactus, $"Knob_{i}");
            }
        }

        TallTrigger(root, 2.4f, 9.5f);
        Level1Obstacle obstacle = root.AddComponent<Level1Obstacle>();
        obstacle.Setup(Level1ObstacleKind.Barrier, LowBarrierDamage, true);
        return root;
    }

    public static GameObject MakeSnake(Transform parent, int lane, float z, float spawnProgress)
    {
        GameObject root = new GameObject("Snake");
        root.transform.SetParent(parent, false);
        root.transform.position = Level1Ground.LanePosition(lane, z, 0.45f);

        GameObject warningRoot = new GameObject("SnakeWarning");
        warningRoot.transform.SetParent(root.transform, false);
        warningRoot.transform.localPosition = new Vector3(0f, 2.35f, 7f);
        Visual(PrimitiveType.Cube, warningRoot.transform, new Vector3(0f, 0.55f, 0f), new Vector3(1.5f, 1.1f, 0.18f), SnakeWarning, "Sign");
        Visual(PrimitiveType.Cube, warningRoot.transform, new Vector3(0f, 0.55f, 0.12f), new Vector3(0.35f, 0.75f, 0.12f), SnakeWarningAccent, "Exclamation");
        Visual(PrimitiveType.Cylinder, warningRoot.transform, new Vector3(0f, -0.15f, 0f), new Vector3(0.18f, 0.55f, 0.18f), SnakeWarningAccent, "Pole");
        warningRoot.SetActive(false);

        GameObject body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        float[] segZ = { -2.8f, -1.4f, 0f, 1.4f, 2.8f };
        for (int i = 0; i < segZ.Length; i++)
        {
            Color segColor = i % 2 == 0 ? SnakeBodyGreen : SnakeBodyDarkGreen;
            Visual(PrimitiveType.Cube, body.transform, new Vector3(0f, 0.38f, segZ[i]), new Vector3(0.72f, 0.52f, 1.05f), segColor, $"Seg_{i}");
        }

        GameObject head = new GameObject("Head");
        head.transform.SetParent(root.transform, false);
        Visual(PrimitiveType.Cube, head.transform, new Vector3(0f, 0.48f, 4.1f), new Vector3(0.82f, 0.62f, 0.95f), SnakeHead, "HeadBox");
        Visual(PrimitiveType.Cube, head.transform, new Vector3(0f, 0.52f, 4.65f), new Vector3(0.55f, 0.42f, 0.45f), SnakeHead, "Snout");

        Transform tongueCenter = Visual(PrimitiveType.Cube, head.transform, new Vector3(0f, 0.46f, 5.05f), new Vector3(0.12f, 0.08f, 0.42f), SnakeTongue, "TongueCenter").transform;
        Transform tongueLeft = Visual(PrimitiveType.Cube, head.transform, new Vector3(-0.1f, 0.42f, 5.25f), new Vector3(0.08f, 0.06f, 0.28f), SnakeTongue, "TongueL").transform;
        Transform tongueRight = Visual(PrimitiveType.Cube, head.transform, new Vector3(0.1f, 0.42f, 5.25f), new Vector3(0.08f, 0.06f, 0.28f), SnakeTongue, "TongueR").transform;

        GameObject visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(root.transform, false);
        body.transform.SetParent(visualRoot.transform, true);
        head.transform.SetParent(visualRoot.transform, true);
        visualRoot.SetActive(false);

        Level1SnakeVisual anim = visualRoot.AddComponent<Level1SnakeVisual>();
        anim.Bind(body.transform, head.transform, tongueCenter, tongueLeft, tongueRight);

        GameObject colliderGo = new GameObject("Collider");
        colliderGo.transform.SetParent(root.transform, false);
        TallTrigger(colliderGo, 1.5f, 6.5f);

        KinematicBody(root);
        Level1Snake snake = root.AddComponent<Level1Snake>();
        snake.Setup(lane, visualRoot, spawnProgress, warningRoot);
        return root;
    }

    static void KinematicBody(GameObject root)
    {
        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = root.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    static void AnchorPickup(GameObject root)
    {
        if (root.GetComponent<Level1PickupAnchor>() == null)
        {
            root.AddComponent<Level1PickupAnchor>();
        }
    }

    static float MidX(int[] lanes)
    {
        float sum = 0f;
        for (int i = 0; i < lanes.Length; i++)
        {
            sum += LevelLanes.X(lanes[i]);
        }

        return lanes.Length == 0 ? 0f : sum / lanes.Length;
    }
}
