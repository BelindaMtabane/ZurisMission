using UnityEngine;

public class PlayerHUDBase : MonoBehaviour
{
    public HUDControls hudControls;
    public PlayerMovement playerMovement;
    private void OnTriggerEnter(Collider other)
    {
        if (hudControls == null) return;
        hudControls = FindFirstObjectByType<HUDControls>(); // Updated to use the recommended method

        if (other.CompareTag("Dam/WaterBUCK/WaterDROP"))
        {
            //hudControls.WaterIncreaseManager();
        }
        if (other.CompareTag("SpeedBoast"))
        {
            // hudControls.SpeedControls(10f);
        }
        if (other.CompareTag("SlowDown"))
        {
            // hudControls.SpeedControls(2f);
        }
        if (other.CompareTag("AnimalAttack") || other.CompareTag("Obstacle"))
        {
            // hudControls.HealthDecreaseManager();
        }
        if (other.CompareTag("Materials"))
        {
            // hudControls.SystemBuild();
        }
        if (other.CompareTag("FruitPickup") || other.CompareTag("Herbs"))
        {
            //hudControls.HealthIncreaseManager();
        }
        if (other.CompareTag("EndLevel"))
        {
            //hudControls.LevelProgress();
        }
    }
}
