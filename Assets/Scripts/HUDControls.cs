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
    /*public TMP_Text waterLevelText;
    public TMP_Text waterSystem;
    public TMP_Text healthText;*/
    public TMP_Text villageProgressText;

    //Bucket variables
    //private float waterMax = 100f;
    private float waterLevel = 0f;
    private float waterIncreaseRate;

    //Player variables
    private float waterLvlPLY = 100f;
    private float health = 100f;
    PlayerMovement playerMovement;
    bool isDead = false;

    //Village variables
    private float villageLevel;
    private float waterSystemLevel;

    //UI bars
    [SerializeField] private Slider healthbar;
    [SerializeField] private Slider bucketbar;
    [SerializeField] private Slider materialBar;
    [SerializeField] private Slider playerWaterLevelBar;

    void Start()
    {
        // Assign the playerscript to the variable3
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>(); // Updated to use the recommended method

        SetMax();
    }
    void Update()
    {
        //Display the HUD to the player 
        UpdateUI();
        BarFilling();
        //Check if the player has died
        if (!isDead && (waterLevel <= 99f || health <= 0f || waterSystemLevel <= 20f || waterLvlPLY <= 0f))
        {
            isDead = true;
            DeathCheck();
        }
    }

    void UpdateUI()
    {
        //Update the UI text to display the current water level and agility level
        /*waterLevelText.text = $"Water LVL: {waterLevel:F0}";
        waterSystem.text = $"Water System: {waterSystemLevel:F0}";
        healthText.text = $"Health:  {health:F0}";*/
        villageProgressText.text = $"Village Progress:  {villageLevel:F0}";

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

        WaterMoveManager();
    }
    // all good  ++
    void WaterMoveManager()
    {
        float waterDecreaseFAST = UnityEngine.Random.Range(5f, 10f);
        float waterDecreaseNORM = 3f;
        //Check if player is fast or slow or normal
        if (playerMovement.playerSpeed >= 40f)
        {
            //Decrease water level based on the player speed
            waterLevel -= waterDecreaseFAST;
            Debug.Log("FAST, water level decreased by " + waterDecreaseFAST);
        }
        if (playerMovement.playerSpeed <= 20f)
        {
            //Decrease water level based on the player speed
            waterLevel -= waterDecreaseNORM;
            waterLvlPLY -= 2f;
            Debug.Log("Water level decreased on NORM by " + waterDecreaseNORM);
        }
    }
    // all good ++
    public void WaterIncreaseManager()
    {
        //Check which scene the player is in
        if (GetActiveScene().name == "MainGame")
        {
            //Set the water Increase based on the scene
            waterIncreaseRate = 50f;
            waterLevel += waterIncreaseRate;
            Debug.Log("Water bucket level increased by " + waterIncreaseRate);
        }
        if (GetActiveScene().name == "Level2")
        {
            //Set the water Increase based on the scene
            waterIncreaseRate = 40f;
            waterLevel += waterIncreaseRate;
            Debug.Log("Water bucket level increased by " + waterIncreaseRate);
        }
        if (GetActiveScene().name == "Level3")
        {
            //Set the water Increase based on the scene
            waterIncreaseRate = 20f;
            waterLevel += waterIncreaseRate;
            Debug.Log("Water bucket level increased by " + waterIncreaseRate);
        }
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
    void BarFilling()
    {
        //Set the player water level bar fill amount to what the player water level is
        playerWaterLevelBar.value = waterLvlPLY;
        //Set the material progress bar fill amount to what the village progress is
        materialBar.value = waterSystemLevel;
        //Set the bucketbar fill amount to what the bucket water level is
        bucketbar.value = waterLevel;
        //Set the healthbar fill amount to what the player's health is
        healthbar.value = health;
    }
    void SetMax()
    {
        //Set the max value of the healthbar to 100
        healthbar.maxValue = 100f;
        healthbar.value = health;
        //Set the max value of the bucketbar to 100
        bucketbar.maxValue = 100f;
        bucketbar.value = waterLevel;

        //Set the max value of the material progress bar to 100
        materialBar.maxValue = 100f;
        materialBar.value = waterSystemLevel;

        //Set the max value of the player water level bar to 100
        playerWaterLevelBar.maxValue = 100f;
        playerWaterLevelBar.value = waterLvlPLY;
    }
    //Create a village progress based on the water system
    void VillageProgress()
    {
        // Check which scene the player is in
        if (GetActiveScene().name == "MainGame")
        {
            //Set the community progress
            villageLevel = 33.5f;
        }
        if (GetActiveScene().name == "Level2")
        {
            //Set the community progress
            villageLevel = 67f;
        }
        if (GetActiveScene().name == "Level3")
        {
            //Set the community progress
            villageLevel = 100f;
        }
    }
    public void SystemBuild()
    {
        // Check which scene the player is in
        if (GetActiveScene().name == "MainGame")
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
    public void PlayerWaterINC()
    {
        waterLvlPLY += 5f;
    }
    public void PlayerWaterDEC()
    {
        waterLvlPLY -= 5f;
    }
    public void LevelProgress()
    {

        //Set win active

        //Set the village progress
        VillageProgress();

    }
    void DeathCheck()
    {
        
            //Set death active
        
    }
}
