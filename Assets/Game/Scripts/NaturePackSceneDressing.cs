using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Replaces scene-placed cactus / rock / tree meshes with Pandazole pack prefabs.
/// </summary>
public class NaturePackSceneDressing : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (_, __) => TryCreate();
        TryCreate();
    }

    static void TryCreate()
    {
        if (!SceneCatalog.IsRunnerScene(SceneManager.GetActiveScene().name)) return;
        if (FindFirstObjectByType<NaturePackSceneDressing>() != null) return;
        new GameObject("NaturePackSceneDressing").AddComponent<NaturePackSceneDressing>();
    }

    void Start()
    {
        if (SceneCatalog.IsLevel1(SceneCatalog.ActiveName))
        {
            NaturePackVisuals.DressSceneObjects();
        }
        AquisWaterVisuals.DressSceneWater();
        NaturePackVisuals.ClearRunnerSceneryColliders();
    }
}
