using UnityEngine;

/// <summary>
/// Places missing Dry Bushlands set-pieces into MainGame using primitives only.
/// Does not delete or rebuild existing scenery.
/// </summary>
[DefaultExecutionOrder(-50)]
public class Level1DryBushlandsBuilder : MonoBehaviour
{
    static readonly float[] Lanes = { -8f, -4f, 0f, 4f, 8f };
    const string RootName = "DryBushlands_Layout";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBuild()
    {
        // Replaced by Level1LayoutDirector for MainGame.
    }

    void Awake()
    {
        enabled = false;
    }

    void BuildLayout()
    {
        Transform root = new GameObject(RootName).transform;

        BuildTutorialFlats(CreateArea(root, "1_TutorialFlats", 40f));
        BuildJackalZigzag(CreateArea(root, "2_JackalBurrowZigzag", 180f));
        BuildCanyonGrapple(CreateArea(root, "3_CanyonGrapple", 340f));
        BuildAcaciaOasis(CreateArea(root, "4_AcaciaPipeOasis", 500f));
        BuildSandstormTunnel(CreateArea(root, "5_SandstormTunnel", 660f));
        BuildSpiritApproach(CreateArea(root, "6_SpiritApproach", 820f));

        Debug.Log("[Level1] Dry Bushlands areas created in MainGame.");
    }

    Transform CreateArea(Transform root, string name, float z)
    {
        GameObject area = new GameObject(name);
        area.transform.SetParent(root);
        area.transform.position = new Vector3(0f, 0f, z);
        return area.transform;
    }

    void BuildTutorialFlats(Transform area)
    {
        MakeCheckpoint(area, 0f, 8f);
        MakePickup(area, "WaterDROP", Lanes[2], 18f, BushlandColors.WaterBottle);
        MakePickup(area, "Materials", Lanes[3], 32f, BushlandColors.Materials);
        MakeRock(area, Lanes[0], 24f);
        MakeCactus(area, Lanes[4], 40f, true);
        MakePickup(area, "Herbs", Lanes[1], 50f, BushlandColors.Herbs);
        MakeWaterBottle(area, Lanes[2], 60f);
    }

    void BuildJackalZigzag(Transform area)
    {
        int[] pattern = { 0, 4, 1, 3, 2, 0, 4 };
        for (int i = 0; i < pattern.Length; i++)
        {
            MakeCactus(area, Lanes[pattern[i]], 20f + i * 18f, true);
            int safe = Mathf.Clamp(2 - (pattern[i] - 2), 0, 4);
            if (i % 2 == 0)
            {
                MakePickup(area, "WaterDROP", Lanes[safe], 28f + i * 18f, BushlandColors.WaterBottle);
                MakeWaterBottle(area, Lanes[safe], 36f + i * 18f);
            }
        }
        MakeRock(area, Lanes[1], 70f);
        MakeRock(area, Lanes[3], 90f);
    }

    void BuildCanyonGrapple(Transform area)
    {
        MakePipe(area, Lanes[0], 16f, 8f);
        MakePipe(area, Lanes[4], 16f, 8f);
        MakePipeJunction(area, Lanes[2], 40f);
        MakeGlass(area, Lanes[2], 22f);
        MakeGrapplePoint(area, Lanes[1], 28f);
        MakeGrapplePoint(area, Lanes[3], 52f);
        MakePickup(area, "SpeedBoast", Lanes[2], 64f, BushlandColors.Speed);
        MakeCactus(area, Lanes[0], 80f, true);
        MakeCactus(area, Lanes[4], 80f, true);
    }

    void BuildAcaciaOasis(Transform area)
    {
        MakePipeJunction(area, 0f, 24f);
        MakePickup(area, "DamWaterBUCK", Lanes[2], 20f, BushlandColors.BucketFill);
        MakeWaterBottle(area, Lanes[1], 34f);
        MakeWaterBottle(area, Lanes[3], 34f);
        MakePickup(area, "Herbs", Lanes[0], 48f, BushlandColors.Herbs);
        MakePickup(area, "FruitPickup", Lanes[4], 48f, BushlandColors.Fruit);
        MakeCheckpoint(area, 0f, 70f);
        MakeRock(area, Lanes[1], 88f);
    }

    void BuildSandstormTunnel(Transform area)
    {
        MakeDustDevil(area, Lanes[2], 18f);
        MakeDustDevil(area, Lanes[0], 48f);
        MakeSandPit(area, Lanes[1], 32f);
        MakeSandPit(area, Lanes[3], 62f);
        MakeHeatWave(area, 0f, 80f);
        MakeGlass(area, Lanes[2], 55f);
        MakeCactus(area, Lanes[4], 40f, true);
        MakeWaterBottle(area, Lanes[2], 96f);
    }

    void BuildSpiritApproach(Transform area)
    {
        MakeCheckpoint(area, 0f, 16f);
        MakePickup(area, "Materials", Lanes[0], 30f, BushlandColors.Materials);
        MakePickup(area, "Materials", Lanes[4], 30f, BushlandColors.Materials);
        MakePickup(area, "DamWaterBUCK", Lanes[2], 48f, BushlandColors.BucketFill);
        MakeWaterBottle(area, Lanes[1], 40f);
        MakeWaterBottle(area, Lanes[3], 40f);
        MakeGate(area, 90f);
        MakeHeatWave(area, 0f, 70f);
    }

    void RefineExistingPickups()
    {
        string[] tags =
        {
            "WaterDROP", "DamWaterBUCK", "Materials", "Herbs", "FruitPickup", "SpeedBoast"
        };

        for (int t = 0; t < tags.Length; t++)
        {
            GameObject[] found;
            try
            {
                found = GameObject.FindGameObjectsWithTag(tags[t]);
            }
            catch
            {
                continue;
            }

            for (int i = 0; i < found.Length; i++)
            {
                if (found[i].GetComponent<PickupCollectable>() == null)
                {
                    found[i].AddComponent<PickupCollectable>();
                }
            }
        }
    }

    static GameObject Primitive(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Color color, string name)
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

    void MakeCactus(Transform parent, float x, float z, bool asHazard)
    {
        GameObject cactus = new GameObject("Cactus");
        cactus.transform.SetParent(parent, false);
        cactus.transform.localPosition = new Vector3(x, 0f, z);

        Primitive(PrimitiveType.Cylinder, cactus.transform, new Vector3(0f, 1.4f, 0f), new Vector3(0.6f, 1.4f, 0.6f), BushlandColors.Cactus, "Trunk");
        Primitive(PrimitiveType.Sphere, cactus.transform, new Vector3(0.7f, 1.8f, 0f), new Vector3(0.7f, 0.45f, 0.45f), BushlandColors.Cactus, "ArmL");
        Primitive(PrimitiveType.Capsule, cactus.transform, new Vector3(-0.65f, 2.1f, 0f), new Vector3(0.35f, 0.55f, 0.35f), BushlandColors.Cactus, "ArmR");

        if (asHazard)
        {
            GameObject telegraph = Primitive(PrimitiveType.Cube, cactus.transform, new Vector3(0f, 0.2f, -6f), new Vector3(1.6f, 0.12f, 1.6f), Color.yellow, "Telegraph");
            Collider col = cactus.AddComponent<BoxCollider>();
            ((BoxCollider)col).size = new Vector3(1.4f, 3.2f, 1.2f);
            ((BoxCollider)col).center = new Vector3(0f, 1.5f, 0f);
            BushlandHazard hz = cactus.AddComponent<BushlandHazard>();
            hz.Setup(BushlandHazardType.CactusWall, telegraph, col);
        }
    }

    void MakeRock(Transform parent, float x, float z)
    {
        GameObject rock = new GameObject("Rock");
        rock.transform.SetParent(parent, false);
        rock.transform.localPosition = new Vector3(x, 0f, z);
        Primitive(PrimitiveType.Sphere, rock.transform, new Vector3(0f, 0.5f, 0f), new Vector3(2.2f, 1.1f, 1.8f), BushlandColors.Rock, "Boulder");
        Primitive(PrimitiveType.Cube, rock.transform, new Vector3(0.7f, 0.35f, 0.4f), new Vector3(1.1f, 0.7f, 1.1f), BushlandColors.Rock, "Chunk");
        GameObject telegraph = Primitive(PrimitiveType.Cube, rock.transform, new Vector3(0f, 0.2f, -6f), new Vector3(1.8f, 0.1f, 1.8f), new Color(0.7f, 0.35f, 0.1f), "Telegraph");
        BoxCollider col = rock.AddComponent<BoxCollider>();
        col.size = new Vector3(2.2f, 1.4f, 1.8f);
        col.center = new Vector3(0f, 0.6f, 0f);
        rock.AddComponent<BushlandHazard>().Setup(BushlandHazardType.Rock, telegraph, col);
    }

    void MakeDustDevil(Transform parent, float x, float z)
    {
        GameObject devil = new GameObject("DustDevil");
        devil.transform.SetParent(parent, false);
        devil.transform.localPosition = new Vector3(x, 0f, z);

        GameObject body = Primitive(PrimitiveType.Cylinder, devil.transform, new Vector3(0f, 2.2f, 0f), new Vector3(1.4f, 2.2f, 1.4f), BushlandColors.DustDevil, "Column");
        body.GetComponent<Collider>().enabled = false;

        ParticleSystem ps = devil.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = BushlandColors.DustDevil;
        main.startSize = 0.8f;
        main.startLifetime = 1.2f;
        main.startSpeed = 2.5f;

        GameObject telegraph = Primitive(PrimitiveType.Cube, devil.transform, new Vector3(0f, 0.15f, -8f), new Vector3(2.2f, 0.1f, 2.2f), new Color(1f, 0.55f, 0.1f), "Telegraph");
        SphereCollider col = devil.AddComponent<SphereCollider>();
        col.radius = 1.8f;
        col.center = new Vector3(0f, 1.5f, 0f);
        BushlandHazard hz = devil.AddComponent<BushlandHazard>();
        hz.Setup(BushlandHazardType.DustDevil, telegraph, col);
        devil.AddComponent<SpinInPlace>().degreesPerSecond = 140f;
    }

    void MakeSandPit(Transform parent, float x, float z)
    {
        GameObject pit = new GameObject("SandPit");
        pit.transform.SetParent(parent, false);
        pit.transform.localPosition = new Vector3(x, 0.05f, z);

        GameObject disc = Primitive(PrimitiveType.Cylinder, pit.transform, Vector3.zero, new Vector3(3.4f, 0.08f, 3.4f), BushlandColors.SandPit, "Pit");
        Object.Destroy(disc.GetComponent<Collider>());

        GameObject telegraph = Primitive(PrimitiveType.Cylinder, pit.transform, new Vector3(0f, 0.12f, 0f), new Vector3(3.8f, 0.02f, 3.8f), new Color(1f, 0.85f, 0.3f), "Telegraph");
        CapsuleCollider col = pit.AddComponent<CapsuleCollider>();
        col.direction = 1;
        col.radius = 1.6f;
        col.height = 0.6f;
        BushlandHazard hz = pit.AddComponent<BushlandHazard>();
        hz.Setup(BushlandHazardType.SandPit, telegraph, col);
    }

    void MakeHeatWave(Transform parent, float x, float z)
    {
        GameObject wave = new GameObject("HeatWave");
        wave.transform.SetParent(parent, false);
        wave.transform.localPosition = new Vector3(x, 1.5f, z);

        GameObject volume = Primitive(PrimitiveType.Cube, wave.transform, Vector3.zero, new Vector3(18f, 3f, 4f), BushlandColors.HeatWave, "Volume");
        Object.Destroy(volume.GetComponent<Collider>());

        GameObject telegraph = Primitive(PrimitiveType.Cube, wave.transform, new Vector3(0f, -1.2f, -7f), new Vector3(10f, 0.12f, 1.4f), new Color(1f, 0.2f, 0f), "Telegraph");
        BoxCollider col = wave.AddComponent<BoxCollider>();
        col.size = new Vector3(18f, 3f, 4f);
        BushlandHazard hz = wave.AddComponent<BushlandHazard>();
        hz.Setup(BushlandHazardType.HeatWave, telegraph, col);
    }

    void MakePipe(Transform parent, float x, float z, float height)
    {
        GameObject pipe = new GameObject("Pipe");
        pipe.transform.SetParent(parent, false);
        pipe.transform.localPosition = new Vector3(x, 0f, z);
        Primitive(PrimitiveType.Cylinder, pipe.transform, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.7f, height * 0.5f, 0.7f), BushlandColors.Pipe, "Stem");
        GameObject telegraph = Primitive(PrimitiveType.Cube, pipe.transform, new Vector3(0f, 0.15f, -6f), new Vector3(1.4f, 0.1f, 1.4f), new Color(0.2f, 0.2f, 0.25f), "Telegraph");
        BoxCollider col = pipe.AddComponent<BoxCollider>();
        col.size = new Vector3(1.2f, height, 1.2f);
        col.center = new Vector3(0f, height * 0.5f, 0f);
        pipe.AddComponent<BushlandHazard>().Setup(BushlandHazardType.Pipe, telegraph, col);
    }

    void MakePipeJunction(Transform parent, float x, float z)
    {
        GameObject j = new GameObject("PipeJunction");
        j.transform.SetParent(parent, false);
        j.transform.localPosition = new Vector3(x, 0f, z);
        Primitive(PrimitiveType.Cylinder, j.transform, new Vector3(0f, 1.6f, 0f), new Vector3(0.55f, 1.6f, 0.55f), BushlandColors.Pipe, "Up");
        GameObject across = Primitive(PrimitiveType.Cylinder, j.transform, new Vector3(0f, 2.4f, 0f), new Vector3(0.45f, 1.5f, 0.45f), BushlandColors.Pipe, "Across");
        across.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        Primitive(PrimitiveType.Cylinder, j.transform, new Vector3(0f, 2.4f, 1.4f), new Vector3(0.4f, 0.9f, 0.4f), BushlandColors.Pipe, "Out");
        GameObject telegraph = Primitive(PrimitiveType.Cube, j.transform, new Vector3(0f, 0.15f, -6f), new Vector3(1.6f, 0.1f, 1.6f), new Color(0.2f, 0.2f, 0.25f), "Telegraph");
        BoxCollider col = j.AddComponent<BoxCollider>();
        col.size = new Vector3(3f, 3.5f, 2.4f);
        col.center = new Vector3(0f, 1.8f, 0.4f);
        j.AddComponent<BushlandHazard>().Setup(BushlandHazardType.Pipe, telegraph, col);
    }

    void MakeGrapplePoint(Transform parent, float x, float z)
    {
        GameObject p = Primitive(PrimitiveType.Sphere, parent, new Vector3(x, 5.5f, z), new Vector3(0.8f, 0.8f, 0.8f), Color.cyan, "GrapplePoint");
        p.AddComponent<GrappleTarget>();
        Object.Destroy(p.GetComponent<Collider>());
        p.AddComponent<SphereCollider>().isTrigger = true;
    }

    void MakeCheckpoint(Transform parent, float x, float z)
    {
        GameObject cp = new GameObject("Checkpoint");
        cp.transform.SetParent(parent, false);
        cp.transform.localPosition = new Vector3(x, 0f, z);
        Primitive(PrimitiveType.Cylinder, cp.transform, new Vector3(0f, 1.2f, 0f), new Vector3(0.35f, 1.2f, 0.35f), new Color(0.9f, 0.9f, 0.75f), "Post");
        Primitive(PrimitiveType.Sphere, cp.transform, new Vector3(0f, 2.6f, 0f), new Vector3(0.9f, 0.9f, 0.9f), BushlandColors.Checkpoint, "Orb");
        Light light = new GameObject("Light").AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.5f, 0.9f, 1f);
        light.range = 12f;
        light.intensity = 1.4f;
        light.transform.SetParent(cp.transform, false);
        light.transform.localPosition = new Vector3(0f, 2.6f, 0f);

        SphereCollider col = cp.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2.2f;
        col.center = new Vector3(0f, 1.4f, 0f);
        cp.AddComponent<BushlandCheckpoint>();
    }

    void MakeGate(Transform parent, float z)
    {
        GameObject gate = new GameObject("SpiritGate");
        gate.transform.SetParent(parent, false);
        gate.transform.localPosition = new Vector3(0f, 0f, z);
        Primitive(PrimitiveType.Cube, gate.transform, new Vector3(-6f, 3f, 0f), new Vector3(1.4f, 6f, 1.4f), new Color(0.55f, 0.4f, 0.2f), "PostL");
        Primitive(PrimitiveType.Cube, gate.transform, new Vector3(6f, 3f, 0f), new Vector3(1.4f, 6f, 1.4f), new Color(0.55f, 0.4f, 0.2f), "PostR");
        Primitive(PrimitiveType.Cube, gate.transform, new Vector3(0f, 6.2f, 0f), new Vector3(14f, 1.2f, 1.4f), new Color(0.5f, 0.35f, 0.18f), "Lintol");
    }

    void MakeGlass(Transform parent, float x, float z)
    {
        GameObject glass = new GameObject("Glass");
        glass.transform.SetParent(parent, false);
        glass.transform.localPosition = new Vector3(x, 0.8f, z);
        Primitive(PrimitiveType.Cube, glass.transform, Vector3.zero, new Vector3(1.6f, 1.6f, 0.2f), BushlandColors.Glass, "Pane");
        Primitive(PrimitiveType.Cube, glass.transform, new Vector3(0.5f, 0.2f, 0.3f), new Vector3(0.7f, 0.7f, 0.12f), BushlandColors.Glass, "Shard");
        GameObject telegraph = Primitive(PrimitiveType.Cube, glass.transform, new Vector3(0f, -0.6f, -6f), new Vector3(1.8f, 0.1f, 1.8f), new Color(0.6f, 0.9f, 1f), "Telegraph");
        BoxCollider col = glass.AddComponent<BoxCollider>();
        col.size = new Vector3(1.8f, 1.8f, 1.2f);
        glass.AddComponent<BushlandHazard>().Setup(BushlandHazardType.Glass, telegraph, col);
    }

    void MakeWaterBottle(Transform parent, float x, float z)
    {
        GameObject bottle = Primitive(PrimitiveType.Cylinder, parent, new Vector3(x, 1.4f, z), new Vector3(0.55f, 0.85f, 0.55f), BushlandColors.WaterBottle, "WaterBottle");
        bottle.tag = "WaterDROP";
        Collider col = bottle.GetComponent<Collider>();
        col.enabled = true;
        col.isTrigger = true;
        bottle.AddComponent<PickupCollectable>();
    }

    void MakePickup(Transform parent, string tag, float x, float z, Color color)
    {
        GameObject p = Primitive(PrimitiveType.Sphere, parent, new Vector3(x, 1.6f, z), Vector3.one * 0.9f, color, "Pickup_" + tag);
        p.tag = tag;
        Collider col = p.GetComponent<Collider>();
        col.enabled = true;
        col.isTrigger = true;
        p.AddComponent<PickupCollectable>();
    }
}

public class SpinInPlace : MonoBehaviour
{
    public float degreesPerSecond = 90f;

    void Update()
    {
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
    }
}

public class BushlandCheckpoint : MonoBehaviour
{
    bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.PlayerWaterINC();
        Debug.Log("[Level1] Checkpoint reached: " + name);
    }
}
