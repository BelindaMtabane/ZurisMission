using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Names the HUD change when body water or the bucket goes up or down.
/// Heat ticks are grouped so the loss line does not repeat every frame.
/// </summary>
public class Level1ResourceToasts : MonoBehaviour
{
    PlayerResources resources;
    float lastWater = -1f;
    float lastBucket = -1f;
    float nextLossToast;
    float nextGainToast;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (!SceneCatalog.IsLevel1(sceneName)) return;
        if (FindFirstObjectByType<Level1ResourceToasts>() != null) return;
        new GameObject("Level1ResourceToasts").AddComponent<Level1ResourceToasts>();
    }

    void OnEnable()
    {
        resources = FindFirstObjectByType<PlayerResources>();
        if (resources == null) return;
        lastWater = resources.PlayerWater;
        lastBucket = resources.BucketWater;
        resources.OnChanged += OnResourcesChanged;
    }

    void OnDisable()
    {
        if (resources != null) resources.OnChanged -= OnResourcesChanged;
    }

    void OnResourcesChanged()
    {
        if (resources == null) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) 
        {
            lastWater = resources.PlayerWater;
            lastBucket = resources.BucketWater;
            return;
        }

        float water = resources.PlayerWater;
        float bucket = resources.BucketWater;

        if (water > lastWater + 0.5f && Time.time >= nextGainToast)
        {
            Level1FeedbackUI.Show("Body water up", new Color(0.35f, 0.85f, 1f), 1.1f);
            nextGainToast = Time.time + 1.4f;
        }
        else if (water < lastWater - 0.5f && Time.time >= nextLossToast)
        {
            Level1FeedbackUI.Show("Body water down", new Color(1f, 0.45f, 0.2f), 1.1f);
            nextLossToast = Time.time + 3.5f;
        }

        if (bucket > lastBucket + 0.5f && Time.time >= nextGainToast)
        {
            Level1FeedbackUI.Show("Bucket up", new Color(0.45f, 0.75f, 1f), 1.1f);
            nextGainToast = Time.time + 1.4f;
        }

        lastWater = water;
        lastBucket = bucket;
    }
}
