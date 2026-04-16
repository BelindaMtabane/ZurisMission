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

public class HUDControls : MonoBehaviour
{
    public TMP_Text waterLevelText;
    public TMP_Text waterSystem;
    public TMP_Text healthText;
    public TMP_Text villageProgressText;

    //Bucket variables
    public float waterMax = 100f;
    public float waterLevel = 100f;
    private float waterIncreaseRate;

    //Player variables
    public float waterLvlPLY = 100f;
    public float health = 100f;
    PlayerMovement playerMovement;

    //Village variables
    public float villageLevel;
    public float waterSystemLevel;

    void Start()
    {
        // Assign the playerscript to the variable3
        if (playerMovement == null) return;
        playerMovement = FindFirstObjectByType<PlayerMovement>(); // Updated to use the recommended method
    }
    void Update()
    {
        //Display the HUD to the player 
        UpdateUI();

        //Set the village progress
        VillageProgress();

        //Decrease Player water levels by the second
        waterLvlPLY -= Time.deltaTime * 2f;

        //Check if the player has died
        if (waterLevel <= 0f || health <= 0f || waterSystemLevel <= 20f || waterLvlPLY <= 0f)
        {
            DeathCheck();
        }
    }

    void UpdateUI()
    {
        //Update the UI text to display the current water level and agility level
        waterLevelText.text = "Water LVL: " + waterLevel;
        waterSystem.text = "Water System: " + waterSystemLevel;
        healthText.text = "Health: " + health;
        villageProgressText.text = "Village Progress: " + villageLevel;

    }
    //all good ++
    public void SpeedControls(float playerMove)
    {
        //Based on the parameter attach it to the player speed
        playerMovement.playerSpeed = playerMove;
        WaterMoveManager();
    }
    // all good  ++
    void WaterMoveManager()
    {
        float waterDecreaseFAST = UnityEngine.Random.Range(5f, 10f);
        float waterDecreaseNORM = 3f;
        //Check if player is fast or slow or normal
        if (playerMovement.playerSpeed == 40f)
        {
            //Decrease water level based on the player speed
            waterLevel -= waterDecreaseFAST;
            Debug.Log("FAST, water level decreased by " + waterDecreaseFAST);
        }
        if (playerMovement.playerSpeed == 20f)
        {
            //Decrease water level based on the player speed
            waterLevel -= waterDecreaseNORM;
            Debug.Log("Water level decreased on NORM by " + waterDecreaseNORM);
        }
    }
    // all good ++
    //Create a method which the player's water level will decrease and they will decide to drink their well water to get at the end
    public void WaterConsumeManager()
    {
        float waterConsumption = 0f;
        if (GetActiveScene().name == "Level1")
        {
            waterConsumption = 10f;
            waterLevel -= waterConsumption;
            waterLvlPLY += waterConsumption;
        }
        if (GetActiveScene().name == "Level2")
        {
            waterConsumption = 20f;
            waterLevel -= waterConsumption;
            waterLvlPLY += waterConsumption;
        }
    }
    // all good ++
    public void WaterIncreaseManager()
    {
        //Check which scene the player is in
        if (GetActiveScene().name == "Level1")
        {
            //Set the water Increase based on the scene
            waterIncreaseRate = 50f;
            waterLevel += waterIncreaseRate;
        }
        if (GetActiveScene().name == "Level2")
        {
            //Set the water Increase based on the scene
            waterIncreaseRate = 40f;
            waterLevel += waterIncreaseRate;
        }
        if (GetActiveScene().name == "Level3")
        {
            //Set the water Increase based on the scene
            waterIncreaseRate = 20f;
            waterLevel += waterIncreaseRate;
        }
    }
    // all good ++
    public void HealthDecreaseManager()
    {
        float playerDAMGE = UnityEngine.Random.Range(3f, 15f);
        float waterDecreaseFAST = UnityEngine.Random.Range(5f, 10f);
        //Calculate the player Health and the water levels
        health -= playerDAMGE;
        waterLevel -= waterDecreaseFAST;
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
        }
        else
        {
            Debug.Log("Health is maxed");
        }
    }

    //Create a village progress based on the water system
    void VillageProgress()
    {
        // Check which scene the player is in
        if (GetActiveScene().name == "Level1")
        {
            //Set the community progress
            villageLevel = 33.5f;
        }
        if (GetActiveScene().name == "Level2")
        {
            //Set the community progress
            villageLevel += 33.5f;
        }
        if (GetActiveScene().name == "Level3")
        {
            //Set the community progress
            villageLevel += 33.5f;
        }
    }
    public void SystemBuild()
    {
        // Check which scene the player is in
        if (GetActiveScene().name == "Level1")
        {
            //Increase Material Collection
            waterSystemLevel += 25f;
        }
        if (GetActiveScene().name == "Level2")
        {
            //Increase Material Collection
            waterSystemLevel += 20f;
        }
        if (GetActiveScene().name == "Level3")
        {
            //Increase Material Collection
            waterSystemLevel += 15f;
        }
    }

    public void LevelProgress()
    {
        
            //Set win active
        
    }
    void DeathCheck()
    {
        
            //Set death active
        
    }
}
