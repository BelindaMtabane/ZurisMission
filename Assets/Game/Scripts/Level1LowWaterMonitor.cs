using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows Level 1 feedback when player body water is running low during heat waves.
/// </summary>
public class Level1LowWaterMonitor : MonoBehaviour
{
    [SerializeField] private float lowThreshold = 35f;
    [SerializeField] private float criticalThreshold = 15f;
    [SerializeField] private float checkInterval = 1.5f;

    bool warnedLow;
    bool warnedCritical;
    float nextCheck;

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (SceneManager.GetActiveScene().name != SceneCatalog.MainGame) return;

        nextCheck -= Time.deltaTime;
        if (nextCheck > 0f) return;
        nextCheck = checkInterval;

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud == null || hud.MaxPlayerWater <= 0f) return;

        float water = hud.PlayerWater;
        if (water <= 0f) return;

        if (!warnedCritical && water <= criticalThreshold)
        {
            warnedCritical = true;
            Level1FeedbackUI.Show("CRITICAL WATER! Find cactus or springs!", new Color(0.95f, 0.2f, 0.15f), 2.4f);
            return;
        }

        if (!warnedLow && water <= lowThreshold)
        {
            warnedLow = true;
            Level1FeedbackUI.Show("Water getting low — drink from cactus!", new Color(0.35f, 0.75f, 1f), 2f);
        }

        if (water > lowThreshold + 8f) warnedLow = false;
        if (water > criticalThreshold + 8f) warnedCritical = false;
    }
}
