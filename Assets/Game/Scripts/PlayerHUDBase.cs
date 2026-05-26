using UnityEngine;

public class PlayerHUDBase : MonoBehaviour
{
    public HUDControls hudControls;
    PlayerMovement playerMovement;

    private void OnTriggerEnter(Collider other)
    {
        if (hudControls == null)
            hudControls = FindFirstObjectByType<HUDControls>();
        if (hudControls == null) return;

        if (other.CompareTag("DamWaterBUCK"))
        {
            hudControls.WaterIncreaseManager();        // Post() fires inside
            hudControls.VillageProgressIncrease();
            Debug.Log("Player water level increased!");
        }
        if (other.CompareTag("WaterDROP"))
        {
            hudControls.PlayerWaterINC();              // Post() fires inside
            hudControls.VillageProgressIncrease();
            Debug.Log("Player water level increased!");
        }
        if (other.CompareTag("Heat&Disease"))
        {
            hudControls.PlayerWaterDEC();              // Post() fires inside
            hudControls.VillageProgressDecrease();
            Debug.Log("Player water system level decreased!");
        }
        if (other.CompareTag("SpeedBoast"))
        {
            hudControls.SpeedControls(40f);            // Post() fires inside
            Debug.Log("Player speed boosted!");
        }
        if (other.CompareTag("SlowDown"))
        {
            hudControls.SpeedControls(15f);            // Post() fires inside
            Debug.Log("Player slowed down!");
        }
        if (other.CompareTag("AnimalAttack"))
        {
            hudControls.HealthDecreaseManager();       // Post() fires inside
            hudControls.VillageProgressDecrease();
            GameInfoUI.Post("Zuri was attacked by an animal!", GameInfoUI.MsgType.Obstacle);
            Debug.Log("Player health decreased by animal!");
        }
        if (other.CompareTag("Obstacle"))
        {
            hudControls.HealthDecreaseManager();       // Post() fires inside
            hudControls.VillageProgressDecrease();
            GameInfoUI.Post("Zuri hit an obstacle and lost health!", GameInfoUI.MsgType.Obstacle);
            Debug.Log("Player health decreased by obstacle!");
        }
        if (other.CompareTag("Materials"))
        {
            hudControls.SystemBuild();                 // Post() fires inside
            Debug.Log("Player material increased!");
        }
        if (other.CompareTag("FruitPickup"))
        {
            hudControls.HealthIncreaseManager();       // Post() fires inside
            GameInfoUI.Post("Zuri picked up fruit and feels better!", GameInfoUI.MsgType.Pickup);
            Debug.Log("Player health increased!");
        }
        if (other.CompareTag("Herbs"))
        {
            hudControls.HealthIncreaseManager();       // Post() fires inside
            GameInfoUI.Post("Zuri collected herbs and restored health!", GameInfoUI.MsgType.Pickup);
            Debug.Log("Player health increased!");
        }
        if (other.CompareTag("EndLevel1"))
        {
            hudControls.LevelProgress();               // Post() fires inside
            hudControls.SceneChange(2f);
        }
        if (other.CompareTag("EndLevel2"))
        {
            hudControls.LevelProgress();               // Post() fires inside
            hudControls.SceneChange(4f);
        }
    }
}
