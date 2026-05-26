using UnityEngine;
using TMPro;
using System.Collections;
using static UnityEngine.SceneManagement.SceneManager;
using UnityEngine.SceneManagement;

public class PipeControlslevel3 : MonoBehaviour
{
    public TMP_Text material;
    public TMP_Text healthText;
    public TMP_Text villageProgressText;
    public TMP_Text tank1;
    public TMP_Text tank2;
    public TMP_Text tank3;

    //Player variables
    private float health = 100f;
    PlayerMovementOG playerMovement;
    bool isDead = false;
    private float healthIncreaseRate;
    private float tankIncreaseRate;
    private int tank1Amount;
    private int tank2Amount;
    private int tank3Amount;
    private int counter;
    private bool hasHit = false;

    //Village variables
    private float villageLevel;
    private float villageLevelTarget = 96f;
    public int materialLevel = 100;

    //Tanks variables
    private int tank1Progress;
    private int tank2Progress;
    private int tank3Progress;

    //UI bars

    void Start()
    {
        // Assign the playerscript to the variable3
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovementOG>(); // Updated to use the recommended method

        VillageProgress();
    }
    void Update()
    {
        //Display the HUD to the player 
        UpdateUI();
        //Check if the player has died
    }

    void UpdateUI()
    {
        material.text = ("Material:" + materialLevel);
        healthText.text = ("Health:" + health);
        tank1.text = ("Tank 1: " + tank1Amount + "%");
        tank2.text = ("Tank 2: " + tank2Amount + "%");
        tank3.text = ("Tank 3: " + tank3Amount + "%");
        villageProgressText.text = $"Village Progress: {villageLevel:F0}%";
    }
    //all good ++
    public void SpeedControls(float playerMove)
    {
        playerMovement.playerSpeed = playerMove;
        Debug.Log("Player speed set to " + playerMovement.playerSpeed);
        StopCoroutine("ResetSpeed");
        StartCoroutine(ResetSpeed());
    }

    private IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(5f);
        playerMovement.playerSpeed = 20f;
    }
    // all good ++
    public void HealthDecreaseManager()
    {
        float playerDAMGE = UnityEngine.Random.Range(3f, 15f);
        //Calculate the player Health and the water levels
        health -= playerDAMGE;
        Debug.Log("Player health decreased by " + playerDAMGE);
        if (health <= 0f)
        {
            health = 0f;
        }
    }
    // all good ++
    public void HealthIncreaseManager()
    {
        float healthIncrease = UnityEngine.Random.Range(3f, 15f);
        //Check if the player has no health
        if (health < 100f)
        {
            //Calculate the player Health
            health += healthIncrease;
            Debug.Log("Player health increased by " + healthIncrease);
        }
        else
        {
            Debug.Log("Health is maxed");
        }
    }
    void VillageProgress()
    {
        villageLevel = 89f;
    }

    public void VillageProgressIncrease()
    {
        float increase = UnityEngine.Random.Range(2f, 4f);
        villageLevel = Mathf.Min(villageLevel + increase, villageLevelTarget);
        Debug.Log($"Village progress +{increase:F1} → {villageLevel:F1}%");
    }

    public void VillageProgressDecrease()
    {
        float decrease = UnityEngine.Random.Range(1f, 2f);
        villageLevel = Mathf.Max(villageLevel - decrease, 0f);
        Debug.Log($"Village progress -{decrease:F1} → {villageLevel:F1}%");
    }
    //++All good
    public void TankMaterialDEC()
    {
        materialLevel -= 6;
    }
    //++All good
    public void TankMaterialINC()
    {
        if (materialLevel < 100)
        {
            //Create a random material increase between 5 and 15
            int materialIncrease = UnityEngine.Random.Range(2, 7);

            materialLevel += materialIncrease;
            //Increase Material Collection
            if (materialLevel >= 100)
            {
                materialLevel = 100;
                Debug.Log("Material collection is maxed");
            }
        }

    }
    public void LevelProgress()
    {
        if (villageLevel >= villageLevelTarget)
            Debug.Log("Level 3 Completed — Village at " + villageLevel.ToString("F0") + "%");
        else
            Debug.Log("Level 3 ended — Village at " + villageLevel.ToString("F0") + "% (target " + villageLevelTarget + "%)");
    }
    //++All good
    public void TankProgressINC1()
    {
        if (tank1Amount < 100)
        {
            tank1Amount += 25;
            Debug.Log("Tank 1 progress increased by 25");
        }
        else
        {
            tank1Amount = 100;
            Debug.Log("Tank 1 is fully repaired");
        }
    }
    //++All good
    public void TankProgressINC2()
    {
        if (tank2Amount < 100)
        {
            tank2Amount += 17;
            Debug.Log("Tank 2 progress increased by 17");
        }
        else
        {
            tank2Amount = 100;
            Debug.Log("Tank 2 is fully repaired");
        }
    }
    //++All good
    public void TankProgressINC3()
    {
        if (tank3Amount < 100)
        {
            tank3Amount += 13;
            Debug.Log("Tank 3 progress increased by 13");
        }
        else
        {
            tank3Amount = 100;
            Debug.Log("Tank 3 is fully repaired");
        }
    }
    /*void DeathCheck()
    {
        //if (!isDead && ( health <= 0f || materialLevel <= 0f )
        {
            isDead = true;
            //SetDeathMenu
            //deathMenuUI.SetActive(true);
            Time.timeScale = 0f;//Stop time
        }
    }*/
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("PipeFix1"))
        {
            hasHit = true;
            TankProgressINC1();
            VillageProgressIncrease();
            GameInfoUI.Post("Zuri repaired Pipe 1! Tank up +25%", GameInfoUI.MsgType.Win);
        }
        if (other.CompareTag("PipeFix2"))
        {
            hasHit = true;
            TankProgressINC2();
            VillageProgressIncrease();
            GameInfoUI.Post("Zuri repaired Pipe 2! Tank up +17%", GameInfoUI.MsgType.Win);
        }
        if (other.CompareTag("PipeFix3"))
        {
            hasHit = true;
            TankProgressINC3();
            VillageProgressIncrease();
            GameInfoUI.Post("Zuri repaired Pipe 3! Tank up +13%", GameInfoUI.MsgType.Win);
        }
        if (other.CompareTag("Materials"))
        {
            hasHit = true;
            TankMaterialINC();
            VillageProgressIncrease();
            GameInfoUI.Post("Zuri collected materials for the village!", GameInfoUI.MsgType.Pickup);
            Debug.Log("Player material increased!");
        }
        if (other.CompareTag("FruitPickup"))
        {
            hasHit = true;
            HealthIncreaseManager();
            GameInfoUI.Post("Zuri picked up fruit and feels better!", GameInfoUI.MsgType.Pickup);
            Debug.Log("Player health increased!");
        }
        if (other.CompareTag("SpeedBoast"))
        {
            hasHit = true;
            SpeedControls(40f);
            GameInfoUI.Post("Zuri is moving fast! Speed boosted.", GameInfoUI.MsgType.Interaction);
            Debug.Log("Player speed boosted!");
        }
        if (other.CompareTag("SlowDown"))
        {
            hasHit = true;
            SpeedControls(15f);
            GameInfoUI.Post("Zuri has been slowed down!", GameInfoUI.MsgType.Obstacle);
            Debug.Log("Player slowed down!");
        }
        if (other.CompareTag("Heat&Disease"))
        {
            hasHit = true;
            HealthDecreaseManager();
            VillageProgressDecrease();
            GameInfoUI.Post("Zuri is suffering from heat and disease!", GameInfoUI.MsgType.Obstacle);
            Debug.Log("Player health decreased");
        }
        if (other.CompareTag("EndLvl3End"))
        {
            hasHit = true;
            LevelProgress();
            GameInfoUI.Post("Zuri completed the mission! The village is saved!", GameInfoUI.MsgType.Win);
        }
        if (other.CompareTag("AnimalAttack"))
        {
            hasHit = true;
            HealthDecreaseManager();
            VillageProgressDecrease();
            GameInfoUI.Post("Zuri was attacked by an animal!", GameInfoUI.MsgType.Obstacle);
            Debug.Log("Player health decreased by animal!");
        }
        if (other.CompareTag("Obstacle"))
        {
            hasHit = true;
            HealthDecreaseManager();
            VillageProgressDecrease();
            GameInfoUI.Post("Zuri hit an obstacle and lost health!", GameInfoUI.MsgType.Obstacle);
            Debug.Log("Player health decreased by obstacle!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        hasHit = false;
    }
}

