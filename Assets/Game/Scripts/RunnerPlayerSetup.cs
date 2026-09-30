using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shared lane alignment and movement feel for MainGame, Level2, and Level3.
/// </summary>
public static class RunnerPlayerSetup
{
    static readonly string[] LaneMarkerNames = { "LaneSpawn1", "LaneSpawn2", "LaneSpawn3", "LaneSpawn4" };
    const string ZuriResourcePath = "Characters/Zuri";
    const float ZuriScale = 2.9620357f;

    public static bool IsRunnerScene(string sceneName)
    {
        return sceneName == SceneCatalog.MainGame || sceneName == SceneCatalog.Level2 || sceneName == SceneCatalog.Level3;
    }

    public static void Apply(string sceneName, Transform player = null)
    {
        if (!IsRunnerScene(sceneName)) return;

        if (player == null)
        {
            player = FindPlayer();
        }

        EnsureZuriCharacter(player);
        LevelLanes.ConfigureForActiveScene();
        AlignLaneMarkers();
        SnapPlayerToCenterLane(player);
        SnapPlayerToGround(player);

        PlayerController controller = player != null
            ? player.GetComponent<PlayerController>()
            : Object.FindFirstObjectByType<PlayerController>();
        controller?.ApplyRunnerMovementFeel();

        LegacyLaneUi.Hide();
    }

    static void EnsureZuriCharacter(Transform player)
    {
        if (player == null) return;

        Transform oldCharacter = player.Find("Female");
        Transform zuri = player.Find("Zuri");

        if (zuri == null)
        {
            GameObject zuriPrefab = Resources.Load<GameObject>(ZuriResourcePath);
            if (zuriPrefab == null)
            {
                Debug.LogError($"[RunnerPlayerSetup] Missing Resources/{ZuriResourcePath} character prefab.");
                return;
            }

            GameObject instance = Object.Instantiate(zuriPrefab, player);
            instance.name = "Zuri";
            zuri = instance.transform;

            Vector3 oldPosition = oldCharacter != null
                ? oldCharacter.localPosition
                : new Vector3(0f, -1.18681f, -0.04914f);
            zuri.localPosition = oldPosition;
            zuri.localRotation = Quaternion.identity;
            zuri.localScale = Vector3.one * ZuriScale;
        }

        zuri.gameObject.SetActive(true);
        if (oldCharacter != null)
        {
            oldCharacter.gameObject.SetActive(false);
        }

        PlayerController controller = player.GetComponent<PlayerController>();
        controller?.RefreshCharacterAnimator();
    }

    public static void AlignLaneMarkers()
    {
        for (int i = 0; i < LaneMarkerNames.Length; i++)
        {
            GameObject marker = GameObject.Find(LaneMarkerNames[i]);
            if (marker == null) continue;

            Vector3 pos = marker.transform.position;
            pos.x = LevelLanes.X(i);
            marker.transform.position = pos;
            marker.SetActive(true);
        }

        Lanemanager2 laneManager2 = Object.FindFirstObjectByType<Lanemanager2>();
        AlignLaneSpawnArray(laneManager2 != null ? laneManager2.laneSpawnsPositions : null);

        Lanemanager3 laneManager3 = Object.FindFirstObjectByType<Lanemanager3>();
        AlignLaneSpawnArray(laneManager3 != null ? laneManager3.laneSpawnsPositions : null);
    }

    static void AlignLaneSpawnArray(Transform[] laneSpawns)
    {
        if (laneSpawns == null) return;

        for (int i = 0; i < laneSpawns.Length && i < LevelLanes.Count; i++)
        {
            if (laneSpawns[i] == null) continue;

            Vector3 pos = laneSpawns[i].position;
            pos.x = LevelLanes.X(i);
            laneSpawns[i].position = pos;
        }
    }

    public static void SnapPlayerToCenterLane(Transform player)
    {
        if (player == null) return;

        Vector3 pos = player.position;
        pos.x = LevelLanes.X(LevelLanes.Count / 2);
        player.position = pos;
    }

    public static void SnapPlayerToGround(Transform player)
    {
        if (player == null) return;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SnapToGroundSurface();
            return;
        }

        Vector3 pos = player.position;
        pos.y = StandingRootY(player);
        player.position = pos;
    }

    public static float SurfaceYForActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (SceneCatalog.IsLevel2(sceneName)) return Level2Ground.SurfaceY;
        if (SceneCatalog.IsLevel3(sceneName)) return Level3Ground.SurfaceY;
        return Level1Ground.SurfaceY;
    }

    public static float StandingRootY(Transform player)
    {
        float surfaceY = SurfaceYForActiveScene();
        CapsuleCollider capsule = player != null ? player.GetComponent<CapsuleCollider>() : null;
        float bottomLocal = capsule != null ? capsule.center.y - capsule.height * 0.5f : 0f;
        return surfaceY - bottomLocal + 0.02f;
    }

    static Transform FindPlayer()
    {
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform;

        GameObject player = GameObject.Find("Player");
        return player != null ? player.transform : null;
    }
}
