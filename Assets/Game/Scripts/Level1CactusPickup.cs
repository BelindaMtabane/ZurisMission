using UnityEngine;

/// <summary>
/// Cactus water source: refills player body water and bucket while the player passes through.
/// </summary>
public class Level1CactusPickup : MonoBehaviour
{
    [SerializeField] private float playerWaterAmount = Level1Config.CactusPlayerWater;
    [SerializeField] private float bucketWaterAmount = Level1Config.CactusBucketWater;
    [SerializeField] private float collectInterval = Level1Config.CactusCollectInterval;

    float cooldown;

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud == null) return;

        if (hud.PlayerWater >= hud.MaxPlayerWater && hud.BucketWater >= hud.MaxBucketWater)
        {
            return;
        }

        hud.CollectCactusWater(playerWaterAmount, bucketWaterAmount);
        Level1FeedbackUI.Show(
            $"+{playerWaterAmount:0} WATER  +{bucketWaterAmount:0} BUCKET",
            new Color(0.25f, 0.82f, 1f),
            1.1f);
        cooldown = collectInterval;
    }
}
