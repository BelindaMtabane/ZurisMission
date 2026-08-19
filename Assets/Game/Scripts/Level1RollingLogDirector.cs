using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Legacy hook kept so existing scenes do not respawn full-width rolling logs.
/// </summary>
public class Level1RollingLogDirector : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneCatalog.MainGame) return;
        Level1RollingLogDirector existing = FindFirstObjectByType<Level1RollingLogDirector>();
        if (existing != null) Destroy(existing.gameObject);
    }

    void Start()
    {
        Destroy(gameObject);
    }
}
