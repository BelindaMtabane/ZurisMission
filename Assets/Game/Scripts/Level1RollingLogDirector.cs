using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Spawns a rolling horizontal log toward the player every 10 seconds in MainGame.
/// </summary>
public class Level1RollingLogDirector : MonoBehaviour
{
    const float StartProgress = 0.12f;
    const float SpawnInterval = 10f;
    const float SpawnAhead = 55f;

    Transform player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != "MainGame") return;
        if (FindFirstObjectByType<Level1RollingLogDirector>() != null) return;

        GameObject host = new GameObject("Level1RollingLogDirector");
        host.AddComponent<Level1RollingLogDirector>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainGame")
        {
            Destroy(gameObject);
            return;
        }

        Level1Progress.BindFromScene(FindPlayer());
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (Level1Progress.Normalized(PlayerZ()) < StartProgress)
        {
            yield return null;
        }

        while (true)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(SpawnInterval);
            SpawnLog();
        }
    }

    void SpawnLog()
    {
        CachePlayer();
        if (player == null) return;

        float z = player.position.z + SpawnAhead;
        float centerX = GetGroundCenterX();
        Vector3 pos = new Vector3(centerX, Level1Ground.SurfaceY + 0.35f, z);

        Level1Primitives.MakeRollingLog(pos, 0);
        Debug.Log("[Level1] Rolling log spawned across full path width");
    }

    static float GetGroundCenterX()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            return ground.transform.position.x;
        }

        return 0f;
    }

    void CachePlayer()
    {
        if (player != null) return;
        player = FindPlayer();
    }

    static Transform FindPlayer()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform;
        GameObject p = GameObject.Find("Player");
        return p != null ? p.transform : null;
    }

    float PlayerZ()
    {
        CachePlayer();
        return player != null ? player.position.z : Level1Progress.StartZ;
    }
}
