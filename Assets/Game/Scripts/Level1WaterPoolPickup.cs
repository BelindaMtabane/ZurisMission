using UnityEngine;

/// <summary>
/// Water spring / puddle: refills player body water and bucket while the player stands in it.
/// </summary>
public class Level1WaterPoolPickup : MonoBehaviour
{
    [SerializeField] private float bucketAmount = Level1Config.SpringBucketWater;
    [SerializeField] private float playerWaterAmount = Level1Config.SpringPlayerWater;
    [SerializeField] private float collectInterval = Level1Config.SpringCollectInterval;

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

        hud.CollectWaterPool(bucketAmount, playerWaterAmount);
        Level1FeedbackUI.Show(
            $"+{playerWaterAmount:0} WATER  +{bucketAmount:0} BUCKET",
            new Color(0.2f, 0.72f, 1f),
            1f);
        cooldown = collectInterval;
    }
}
