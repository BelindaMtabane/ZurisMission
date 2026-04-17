using UnityEngine;

public class PlayerHUDBase : MonoBehaviour
{
    public HUDControls hudControls;
    public PlayerMovement playerMovement;
    private void OnTriggerEnter(Collider other)
    {
        if (hudControls == null) return;
        hudControls = FindFirstObjectByType<HUDControls>(); // Updated to use the recommended method

        if (other.CompareTag("DamWaterBUCK"))
        {
            hudControls.WaterIncreaseManager();
            Debug.Log("Player water level increased!");
        }
        if (other.CompareTag("WaterDROP"))
        {
            hudControls.PlayerWaterINC();
            Debug.Log("Player water level decreased!");
        }
        if (other.CompareTag("Heat&Disease"))
        {
            hudControls.PlayerWaterDEC();
            Debug.Log("Player water system level increased!");
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
            hudControls.HealthDecreaseManager();
            Debug.Log("Player health decreased by animal or obstacle!");
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
            hudControls.SceneChange(1f);
        }/*
        if (other.CompareTag("EndLevel2"))
        {
            hudControls.LevelProgress();
            hudControls.SceneChange(2f);
        }
        if (other.CompareTag("EndLevel3"))
        {
            hudControls.LevelProgress();
            hudControls.SceneChange(3f);
        }*/

        if (other.CompareTag("PipeFix1"))
        {
            hudControls.TankProgressINC(1f);
            Debug.Log("Pipe 1 hit");
        }
        if (other.CompareTag("PipeFix2"))
        {
            hudControls.TankProgressINC(2f);
            Debug.Log("Pipe 2 hit");
        }
        if (other.CompareTag("PipeFix3"))
        {
            hudControls.TankProgressINC(3f);
            Debug.Log("Pipe 3 hit");
        }
        if (other.CompareTag("PipeHit"))
        {
            hudControls.TankDEC();
            Debug.Log("Hit the pipe");
        }
    }
}
