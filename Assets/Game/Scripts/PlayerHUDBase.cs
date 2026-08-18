using UnityEngine;

public class PlayerHUDBase : MonoBehaviour
{
    public HUDControls hudControls;

    void Start()
    {
        if (hudControls == null)
        {
            hudControls = FindFirstObjectByType<HUDControls>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        if (hudControls == null)
        {
            hudControls = FindFirstObjectByType<HUDControls>();
        }

        if (hudControls == null) return;

        if (other.GetComponentInParent<BushlandHazard>() != null
            || other.GetComponentInParent<SnakePassHazard>() != null
            || other.GetComponentInParent<Level1Obstacle>() != null
            || other.GetComponentInParent<Level1CactusPickup>() != null
            || other.GetComponentInParent<Level1StatPickup>() != null
            || other.GetComponentInParent<Level1WaterPoolPickup>() != null
            || other.GetComponentInParent<Level1MaterialPickup>() != null
            || other.GetComponentInParent<Level1SuperFruitPickup>() != null
            || other.GetComponentInParent<Level1AloePickup>() != null
            || other.GetComponentInParent<Level1Snake>() != null
            || other.GetComponentInParent<Level2Obstacle>() != null
            || other.GetComponentInParent<Level2WaterDropletPickup>() != null
            || other.GetComponentInParent<Level2BaobabPickup>() != null
            || other.GetComponentInParent<Level2MaterialPickup>() != null
            || other.GetComponentInParent<Level2BubbleShieldPickup>() != null
            || other.GetComponentInParent<Level2SpeedFruitPickup>() != null
            || other.GetComponentInParent<Level2JumpBoostPickup>() != null
            || other.GetComponentInParent<Level2PoisonPlant>() != null
            || other.GetComponentInParent<Level2MudMonster>() != null
            || other.GetComponentInParent<Level2MudBall>() != null
            || other.GetComponentInParent<Level3Obstacle>() != null
            || other.GetComponentInParent<Level3MaterialPickup>() != null
            || other.GetComponentInParent<Level3WaterDropletPickup>() != null
            || other.GetComponentInParent<Level3HealthPickup>() != null
            || other.GetComponentInParent<Level3Snake>() != null
            || other.GetComponentInParent<Level3HorizontalEnemy>() != null
            || other.GetComponentInParent<Level3LightningZone>() != null
            || other.GetComponentInParent<Level3RepairPoint>() != null
            || other.GetComponentInParent<Level3BossRepairPoint>() != null
            || other.GetComponentInParent<Level3AcidRainZone>() != null
            || other.GetComponentInParent<Level3RollingLog>() != null
            || other.GetComponentInParent<Level3SpeedFruitPickup>() != null)
        {
            return;
        }

        if (other.CompareTag("DamWaterBUCK"))
        {
            hudControls.WaterIncreaseManager();
            Debug.Log("Bucket water increased.");
        }
        if (other.CompareTag("WaterDROP"))
        {
            hudControls.DrinkBottle();
            Debug.Log("Water bottle +10 player water.");
        }
        if (other.CompareTag("Heat&Disease"))
        {
            hudControls.PlayerWaterDEC();
            Debug.Log("Player water decreased.");
        }
        if (other.CompareTag("SpeedBoast"))
        {
            hudControls.SpeedControls(40f);
            Debug.Log("Player speed boosted!");
        }
        if (other.CompareTag("SlowDown"))
        {
            hudControls.SpeedControls(15f);
            Debug.Log("Player slowed down!");
        }
        //other.CompareTag("AnimalAttack")
        if (other.CompareTag("AnimalAttack") || other.CompareTag("Obstacle"))
        {
            string n = other.name.ToLowerInvariant();
            if (n.Contains("cactus"))
            {
                hudControls.ChangeBucket(10f);
                Debug.Log("Cactus: bucket +10");
            }
            else
            {
                hudControls.HealthDecreaseManager();
                Debug.Log("Player health decreased by animal or obstacle!");
            }
        }
        if (other.CompareTag("Materials"))
        {
            hudControls.SystemBuild();
            Debug.Log("Player material increased!");
        }
        if (other.CompareTag("FruitPickup") || other.CompareTag("Herbs"))
        {
            hudControls.HealthIncreaseManager();
            Debug.Log("Player health increased!");
        }
        if (other.CompareTag("EndLevel1"))
        {
            hudControls.LevelProgress();
        }
        if (other.CompareTag("EndLevel2"))
        {
            hudControls.LevelProgress();
        }
        if (other.CompareTag("EndLvl3End"))
        {
            hudControls.LevelProgress();
        }
    }
        
}
