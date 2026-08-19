using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shared lane alignment and movement feel for MainGame, Level2, and Level3.
/// </summary>
public static class RunnerPlayerSetup
{
    static readonly string[] LaneMarkerNames = { "LaneSpawn1", "LaneSpawn2", "LaneSpawn3", "LaneSpawn4" };

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

        LevelLanes.ConfigureForActiveScene();
        AlignLaneMarkers();
        SnapPlayerToCenterLane(player);

        PlayerController controller = player != null
            ? player.GetComponent<PlayerController>()
            : Object.FindFirstObjectByType<PlayerController>();
        controller?.ApplyRunnerMovementFeel();

        LegacyLaneUi.Hide();
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

    static Transform FindPlayer()
    {
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform;

        GameObject player = GameObject.Find("Player");
        return player != null ? player.transform : null;
    }
}
