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
        //Update the UI text to display the current water level and agility level
        material.text = ("Material:" + materialLevel);
        healthText.text = ("Health:" + health);
        tank1.text = ("Tank 1: " + tank1Amount + "%");
        tank2.text = ("Tank 2: " + tank2Amount + "%");
        tank3.text = ("Tank 3: " + tank3Amount + "%");
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
    //Create a village progress based on the water system
    void VillageProgress()
    {
        villageProgressText.text = "Village Progress: " + 90;
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
        //SetVictoryMenu
        if (tank1Amount >= 100 && tank2Amount >= 100 && tank3Amount >= 100)
        {
            Debug.Log("All tanks are fully repaired, level complete!");
            //SetVictoryMenu

        }
        else
        {
            Debug.Log("Not all tanks are fully repaired, keep working!");
            //Set death scene
        }
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
        }

        if (other.CompareTag("PipeFix2"))
        {
            hasHit = true;
            TankProgressINC2();
        }

        if (other.CompareTag("PipeFix3"))
        {
            hasHit = true;
            TankProgressINC3();
        }
        if (other.CompareTag("Materials"))
        {
            hasHit = true;
            TankMaterialINC();
            Debug.Log("Player material increased!");
        }
        if (other.CompareTag("FruitPickup"))
        {
            hasHit = true;
            HealthIncreaseManager();
            Debug.Log("Player health increased!");
        }

        if (other.CompareTag("SpeedBoast"))
        {
            hasHit = true;
            SpeedControls(40f);
            Debug.Log("Player speed boosted!");
        }
        if (other.CompareTag("SlowDown"))
        {
            hasHit = true;
            SpeedControls(15f);
            Debug.Log("Player slowed down!");
        }

        if (other.CompareTag("Heat&Disease"))
        {
            hasHit = true;
            HealthDecreaseManager();
            Debug.Log("Player health decreased");
        }

        if (other.CompareTag("EndLvl3End"))
        {
            hasHit = true;
            LevelProgress();
        }

        if (other.CompareTag("AnimalAttack") || other.CompareTag("Obstacle"))
        {
            hasHit = true;
            HealthDecreaseManager();
            Debug.Log("Player health decreased by animal or obstacle!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        hasHit = false;
    }
}

