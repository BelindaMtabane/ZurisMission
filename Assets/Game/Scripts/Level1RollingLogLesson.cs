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
        Transform parent = transform.parent != null ? transform.parent : transform;
        Level1Primitives.MakeRollingLog(parent, 1, transform.position.z + 18f, 2, 13f);
        Debug.Log("[Level1] Tutorial rolling log spawned");
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
