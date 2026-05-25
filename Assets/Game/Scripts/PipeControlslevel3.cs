using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        tank1.text = ("Tank 1: " + tank1Amount);
        tank2.text = ("Tank 2: " + tank2Amount);
        tank3.text = ("Tank 3: " + tank3Amount);
    }
    //all good ++
    public void SpeedControls(float playerMove)
    {
        //Based on the parameter attach it to the player speed
        playerMovement.playerSpeed = playerMove;
        //After 5 seconds set the player speed back to normal
        float timer = 5f;
        timer -= Time.deltaTime;

        if (timer <= 0f)
            playerMovement.playerSpeed = 20f;
        Debug.Log("Player speed set to " + playerMovement.playerSpeed);

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
    public void SceneChange(float scenenumber)
    {
        if (scenenumber == 2f)
        {
            SceneManager.LoadScene("Lvl3Victory");
        }
        if (scenenumber == 3f)
        {
            SceneManager.LoadScene("DeathScene");
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
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PipeFix"))
        {
            
                TankProgressINC1();
            
        }
        if (other.CompareTag("PipeFix2"))
        {
            if (other.CompareTag("PipeHit"))
            {
                TankProgressINC2();
            }
        }
        if (other.CompareTag("PipeFix3"))
        {
            if (other.CompareTag("PipeHit"))
            {
                TankProgressINC3();
            }
        }
        if (!other.CompareTag("PipeFix1") || !other.CompareTag("PipeFix2") || !other.CompareTag("PipeFix3"))
        {
            if (other.CompareTag("PipeHit"))
            {
                Debug.Log("player hit pipe but not specific placement");
            }
        }

        if (other.CompareTag("Materials"))
        {
            TankMaterialINC();
            Debug.Log("Player material increased!");
        }
        if (other.CompareTag("FruitPickup"))
        {
            HealthIncreaseManager();
            Debug.Log("Player health increased!");
        }

        if (other.CompareTag("SpeedBoast"))
        {
            SpeedControls(40f);
            Debug.Log("Player speed boosted!");
        }
        if (other.CompareTag("SlowDown"))
        {
            SpeedControls(15f);
            Debug.Log("Player slowed down!");
        }

        if (other.CompareTag("Heat&Disease"))
        {
            HealthDecreaseManager();
            Debug.Log("Player health system level decreased!");
        }

        if (other.CompareTag("EndLevel3"))
        {
            //SceneChange(4f);
            Debug.Log("Ended level 3");
        }
    }
}

