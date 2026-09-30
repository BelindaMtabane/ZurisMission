using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Separates boulder wall, village houses, and lane pickups so meshes do not intersect.
/// </summary>
public static class RoadsideOverlapResolver
{
    struct Bounds2
    {
        public float MinX, MaxX, MinZ, MaxZ;

        public static Bounds2 From(Bounds b) => new Bounds2
        {
            MinX = b.min.x,
            MaxX = b.max.x,
            MinZ = b.min.z,
            MaxZ = b.max.z
        };

        public bool Overlaps(Bounds2 o, float pad = 0.75f) =>
            MinX - pad < o.MaxX && MaxX + pad > o.MinX
            && MinZ - pad < o.MaxZ && MaxZ + pad > o.MinZ;
    }

    public static bool WouldHouseOverlapBoulders(float x, float z, float radius = 5.5f)
    {
        var probe = new Bounds2
        {
            MinX = x - radius,
            MaxX = x + radius,
            MinZ = z - radius,
            MaxZ = z + radius
        };

        List<Bounds2> boulders = CollectBoulderBounds();
        return OverlapsAny(probe, boulders);
    }

    public static void Resolve()
    {
        LevelLanes.ConfigureForActiveScene();
        LevelLanes.GetBoulderWallBand(out float wallInner, out float wallOuter);
        LevelLanes.GetVillageHouseBand(out float houseInner, out float houseOuter);

        PushBoulderWallIntoBand(wallInner, wallOuter);
        NaturePackVisuals.MovePathRocksFarOut();
        PushBoulderWallIntoBand(wallInner, wallOuter);
        PushBouldersOffRunCorridor();
        NudgeBouldersAwayFromPickups(wallInner, wallOuter);
        PushHousesOutsideWall(houseInner, houseOuter);
        NudgeBouldersAwayFromHouses(wallInner, wallOuter);
        PushHousesOutsideWall(houseInner, houseOuter);
    }

    static void PushBoulderWallIntoBand(float inner, float outer)
    {
        foreach (Transform boulder in FindBoulderTransforms())
        {
            Vector3 p = boulder.position;
            float sign = p.x >= 0f ? 1f : -1f;
            if (Mathf.Abs(p.x) < 0.01f) sign = 1f;
            p.x = sign * Mathf.Clamp(Mathf.Abs(p.x), inner, outer);
            boulder.position = p;
        }
    }

    static void PushBouldersOffRunCorridor()
    {
        float clearX = LevelLanes.PathRockClearAbsX();
        foreach (Transform boulder in FindBoulderTransforms())
        {
            Bounds b = WorldBounds(boulder.gameObject);
            if (b.size.sqrMagnitude < 0.001f) continue;

            Vector3 p = boulder.position;
            if (b.center.x >= 0f && b.min.x < clearX + 1f)
                p.x += clearX + 1f - b.min.x;
            else if (b.center.x < 0f && b.max.x > -clearX - 1f)
                p.x -= b.max.x - (-clearX - 1f);

            boulder.position = p;
        }
    }

    static void NudgeBouldersAwayFromPickups(float wallInner, float wallOuter)
    {
        List<Bounds2> pickups = CollectPickupBounds();
        foreach (Transform boulder in FindBoulderTransforms())
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Bounds2 rock = Bounds2.From(WorldBounds(boulder.gameObject));
                if (!OverlapsAny(rock, pickups)) break;

                Vector3 p = boulder.position;
                p.z += 4.2f;
                float sign = p.x >= 0f ? 1f : -1f;
                p.x = sign * Mathf.Clamp(Mathf.Abs(p.x) + 1.2f, wallInner, wallOuter);
                boulder.position = p;
            }
        }
    }

    static void NudgeBouldersAwayFromHouses(float wallInner, float wallOuter)
    {
        List<Bounds2> houses = CollectHouseBounds();
        foreach (Transform boulder in FindBoulderTransforms())
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Bounds2 rock = Bounds2.From(WorldBounds(boulder.gameObject));
                if (!OverlapsAny(rock, houses)) break;

                Vector3 p = boulder.position;
                p.z += (boulder.GetInstanceID() % 2 == 0 ? 1f : -1f) * 5.5f;
                float sign = p.x >= 0f ? 1f : -1f;
                p.x = sign * Mathf.Clamp(Mathf.Abs(p.x), wallInner, wallOuter - 0.65f);
                boulder.position = p;

                Transform scaleTarget = boulder;
                scaleTarget.localScale *= 0.92f;
            }
        }
    }

    static void PushHousesOutsideWall(float houseInner, float houseOuter)
    {
        Transform village = FindVillageRoot();
        if (village == null) return;

        List<Bounds2> boulders = CollectBoulderBounds();

        for (int i = 0; i < village.childCount; i++)
        {
            Transform house = village.GetChild(i);
            if (house == null) continue;

            for (int attempt = 0; attempt < 18; attempt++)
            {
                float absX = Mathf.Abs(house.position.x);
                float sign = house.position.x >= 0f ? 1f : -1f;
                if (absX < houseInner)
                {
                    house.position = new Vector3(sign * houseInner, house.position.y, house.position.z);
                    continue;
                }

                Bounds2 box = Bounds2.From(WorldBounds(house.gameObject));
                if (!OverlapsAny(box, boulders)) break;

                absX = Mathf.Min(houseOuter, absX + 2.25f);
                house.position = new Vector3(sign * absX, house.position.y, house.position.z);
            }
        }
    }

    static bool OverlapsAny(Bounds2 a, List<Bounds2> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (a.Overlaps(list[i])) return true;
        }

        return false;
    }

    static List<Bounds2> CollectBoulderBounds()
    {
        var list = new List<Bounds2>(128);
        foreach (Transform t in FindBoulderTransforms())
        {
            Bounds b = WorldBounds(t.gameObject);
            if (b.size.sqrMagnitude > 0.001f) list.Add(Bounds2.From(b));
        }

        return list;
    }

    static List<Bounds2> CollectHouseBounds() => CollectNamedRootBounds("VillageSideHouses");

    static List<Bounds2> CollectPickupBounds()
    {
        var list = new List<Bounds2>(128);
        AddPickupBounds<Level1WaterPoolPickup>(list);
        AddPickupBounds<Level1CactusPickup>(list);
        AddPickupBounds<Level1MaterialPickup>(list);
        AddPickupBounds<Level1StatPickup>(list);
        AddPickupBounds<Level1AloePickup>(list);
        AddPickupBounds<Level1SuperFruitPickup>(list);
        AddPickupBounds<Level2WaterPoolPickup>(list);
        AddPickupBounds<Level2MaterialPickup>(list);
        AddPickupBounds<Level2HealthFruitPickup>(list);
        return list;
    }

    static void AddPickupBounds<T>(List<Bounds2> list) where T : Component
    {
        T[] found = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] == null) continue;
            Bounds b = WorldBounds(found[i].gameObject);
            if (b.size.sqrMagnitude > 0.001f) list.Add(Bounds2.From(b));
        }
    }

    static List<Bounds2> CollectNamedRootBounds(string rootName)
    {
        var list = new List<Bounds2>(64);
        Transform root = FindRootByName(rootName);
        if (root == null) return list;

        for (int i = 0; i < root.childCount; i++)
        {
            Bounds b = WorldBounds(root.GetChild(i).gameObject);
            if (b.size.sqrMagnitude > 0.001f) list.Add(Bounds2.From(b));
        }

        return list;
    }

    static IEnumerable<Transform> FindBoulderTransforms()
    {
        var seen = new HashSet<int>();
        var result = new List<Transform>(128);

        GameObject wallRoot = FindRootByName("RoadsideBoulderWall")?.gameObject;
        if (wallRoot != null)
        {
            for (int i = 0; i < wallRoot.transform.childCount; i++)
            {
                Transform c = wallRoot.transform.GetChild(i);
                if (seen.Add(c.GetInstanceID())) result.Add(c);
            }
        }

        GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            GameObject go = all[i];
            if (go == null) continue;
            if (!go.activeInHierarchy) continue;
            if (UnderRunnerLayout(go.transform)) continue;
            if (go.GetComponentInParent<Level1Obstacle>() != null) continue;
            if (go.GetComponentInParent<Level2Obstacle>() != null) continue;

            string n = go.name;
            if (!n.StartsWith("Rock_A") && !n.StartsWith("Rock_") && !n.StartsWith("Cliff")) continue;
            if (n.StartsWith("RockCluster")) continue;
            if (!seen.Add(go.transform.GetInstanceID())) continue;
            result.Add(go.transform);
        }

        return result;
    }

    static bool UnderRunnerLayout(Transform t)
    {
        while (t != null)
        {
            string n = t.name;
            if (n == "Level1_Layout" || n == "Level2_Layout" || n == "Level3_Layout") return true;
            t = t.parent;
        }

        return false;
    }

    static Transform FindVillageRoot() => FindRootByName("VillageSideHouses");

    static Transform FindRootByName(string name)
    {
        GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].name == name)
                return all[i].transform;
        }

        return null;
    }

    static Bounds WorldBounds(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
        {
            if (rends[i] != null) b.Encapsulate(rends[i].bounds);
        }

        return b;
    }
}
