using UnityEngine;

/// <summary>
/// Spawns the tutorial rolling log once when the player reaches the lesson point.
/// </summary>
public class Level1RollingLogLesson : MonoBehaviour
{
    const float WarningDistance = 42f;
    const float SpawnDistance = 28f;

    Transform player;
    bool warned;
    bool spawned;

    void Update()
    {
        if (spawned) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        CachePlayer();
        if (player == null) return;

        float dz = player.position.z - transform.position.z;

        if (!warned && dz >= -WarningDistance && dz <= SpawnDistance)
        {
            warned = true;
            Level1FeedbackUI.Show("ROLLING LOG! Press SPACE to jump!", new Color(1f, 0.75f, 0.25f), 2.2f);
        }

        if (!spawned && dz >= -SpawnDistance)
        {
            spawned = true;
            SpawnLog();
        }
    }

    void SpawnLog()
    {
        float centerX = 0f;
        GameObject ground = GameObject.Find("Ground");
        if (ground != null) centerX = ground.transform.position.x;

        Vector3 pos = new Vector3(centerX, Level1Ground.SurfaceY + 0.35f, transform.position.z + 18f);
        Level1Primitives.MakeRollingLog(pos, 0);
        Debug.Log("[Level1] Tutorial rolling log spawned");
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
