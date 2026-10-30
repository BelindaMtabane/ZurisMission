using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Replaces props that were placed directly in the runner scenes (cliffs, rocks, houses,
/// borehole jars, silo tanks, villagers) with the created prefabs, fitted to the same spot.
/// Runs a few frames after load so the level layout has already moved scenery out of the lanes.
/// </summary>
public sealed class ScenePropSwapper : MonoBehaviour
{
    const int FramesToWait = 5;
    const string HostName = "CreatedScenePropSwaps";
    const float BoreholeHeight = 3.2f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterForSceneLoads()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!SceneCatalog.IsRunnerScene(scene.name)) return;
        new GameObject("ScenePropSwapper").AddComponent<ScenePropSwapper>();
    }

    IEnumerator Start()
    {
        for (int i = 0; i < FramesToWait; i++) yield return null;

        Transform host = new GameObject(HostName).transform;
        float pathCenter = LevelLanes.PathCenterX;
        string wallName = CreatedProps.MountainWallForActiveScene();
        int index = 0;

        foreach (GameObject go in FindSceneRoots(IsSwappable))
        {
            string n = go.name;
            bool boreholeLeftover = (n.StartsWith("Borehole_Water") || (n.StartsWith("Borehole_Jar_") && n.EndsWith("b")))
                                    && CreatedProps.Load(CreatedProps.BoreholeJars) != null;
            if (boreholeLeftover)
            {
                go.SetActive(false);
                continue;
            }

            Bounds b = RendererBounds(go);
            if (b.size.y < 0.05f) continue;

            float faceRoad = b.center.x < pathCenter ? 90f : -90f;
            GameObject replacement = null;

            if (n.StartsWith("Cliff"))
                replacement = SpawnWall(wallName, host, b, pathCenter);
            else if (n.StartsWith("Rock_"))
                replacement = CreatedProps.SpawnFitHeight(CreatedProps.Pick(CreatedProps.Rocks, index), host, Vector3.zero, b.size.y, index * 47f % 360f);
            else if (n.StartsWith("House_"))
                replacement = CreatedProps.SpawnFitHeight(CreatedProps.DesertHouse, host, Vector3.zero, b.size.y, go.transform.eulerAngles.y);
            else if (n.StartsWith("Tank_"))
                replacement = CreatedProps.SpawnFitHeight(CreatedProps.Tank(index), host, Vector3.zero, b.size.y, faceRoad);
            else if (n.StartsWith("Borehole_Jar_") && !n.EndsWith("b"))
                replacement = CreatedProps.SpawnFitHeight(CreatedProps.BoreholeJars, host, Vector3.zero, BoreholeHeight, faceRoad + 180f);
            else if (n.StartsWith("NPC_"))
                replacement = SpawnVillager(host, b, faceRoad, index);

            if (replacement == null) continue;

            replacement.name = $"Created_{replacement.name}_{index}";
            float y = n.StartsWith("NPC_") ? GroundY() : b.min.y;
            replacement.transform.position = new Vector3(b.center.x, y, b.center.z);
            go.SetActive(false);
            index++;
        }

        Destroy(gameObject);
    }

    static bool IsSwappable(string n) =>
        n.StartsWith("Cliff") || n.StartsWith("Rock_A") || n.StartsWith("House_") ||
        n.StartsWith("Borehole_") || n.StartsWith("NPC_") || IsSiloTank(n);

    static bool IsSiloTank(string n) => n.StartsWith("Tank_") && n.Length > 5 && char.IsDigit(n[5]);

    static GameObject SpawnWall(string wallName, Transform host, Bounds b, float pathCenter)
    {
        bool runsAlongZ = b.size.z >= b.size.x;
        float yaw = runsAlongZ ? (b.center.x < pathCenter ? 90f : -90f) : 180f;
        Vector3 size = new Vector3(
            Mathf.Max(b.size.x, b.size.z),
            b.size.y,
            Mathf.Max(2f, Mathf.Min(b.size.x, b.size.z)));
        return CreatedProps.SpawnFitBox(wallName, host, Vector3.zero, size, yaw);
    }

    static GameObject SpawnVillager(Transform host, Bounds b, float faceRoad, int index)
    {
        string villager = CreatedProps.Pick(CreatedProps.Villagers, index);
        float height = Mathf.Clamp(b.size.y, 2.5f, 6f);
        if (villager == "Villager_Child") height *= 0.7f;
        return CreatedProps.SpawnFitHeight(villager, host, Vector3.zero, height, faceRoad);
    }

    static float GroundY()
    {
        if (SceneCatalog.IsLevel2(SceneCatalog.ActiveName)) return Level2Ground.SurfaceY;
        if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName)) return Level3Ground.SurfaceY;
        return Level1Ground.SurfaceY;
    }

    static List<GameObject> FindSceneRoots(System.Func<string, bool> matches)
    {
        var result = new List<GameObject>();
        GameObject[] all = FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            GameObject go = all[i];
            if (go == null || !matches(go.name)) continue;
            if (HasExcludedAncestor(go.transform, matches)) continue;
            result.Add(go);
        }

        return result;
    }

    static bool HasExcludedAncestor(Transform t, System.Func<string, bool> matches)
    {
        for (Transform p = t.parent; p != null; p = p.parent)
        {
            string n = p.name;
            if (matches(n)) return true;
            if (n == HostName || n == "RoadsideBoulderWall" || n == "VillageSideHouses" || n == "VillageFinishCourtyard") return true;
            if (n == "Level1_Layout" || n == "Level2_Layout" || n == "Level3_Layout") return true;
        }

        return false;
    }

    static Bounds RendererBounds(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            if (rends[i] != null) b.Encapsulate(rends[i].bounds);
        return b;
    }
}
