using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loops the desert ambience track for the whole of Level 1 (MainGame).
/// Auto-creates itself — no scene setup needed.
/// </summary>
public class Level1Ambience : MonoBehaviour
{
    const string ClipPath = "Audio/deserthot";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "MainGame") return;
        if (FindFirstObjectByType<Level1Ambience>() != null) return;
        new GameObject("Level1Ambience").AddComponent<Level1Ambience>();
    }

    void Awake()
    {
        AudioClip clip = Resources.Load<AudioClip>(ClipPath);
        if (clip == null)
        {
            Debug.LogWarning($"[Level1Ambience] Clip not found at Resources/{ClipPath}");
            return;
        }

        var source = gameObject.AddComponent<AudioSource>();
        source.clip         = clip;
        source.loop         = true;
        source.spatialBlend = 0f;     // 2D — constant volume regardless of listener position
        source.volume       = 0.5f;
        source.playOnAwake  = false;
        source.Play();
    }
}
