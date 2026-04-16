using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

public class HUDControls : MonoBehaviour
{
    public TMP_Text waterLevelText;
    public TMP_Text waterSystem;
    public TMP_Text healthText;

    public float waterLevel = 100f;
    public float health = 100f;
    public float villageWaterLevel = 0f;
    public float systemLevel = 100f;
    PlayerMovement playerMovement;

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
    }

    void UpdateUI()
    {
        //Update the UI text to display the current water level and agility level
        waterLevelText.text = "Water LVL: " + waterLevel;
        waterSystem.text = "Water System: " + systemLevel;
        healthText.text = "Health: " + health;
    }
    public void WaterControls()
    {
        float waterLeveldecrease = UnityEngine.Random.Range(2f, 10f);
        //Check if player is fast or slow or normal
        if (playerMovement.playerSpeed == playerMovement.playerSpeedBoost)
        {
            //Decrease water level based on the player speed
            waterLevel -= waterLeveldecrease;
            Debug.Log("FAST, water level decreased by " + waterLeveldecrease);

        }
        if (playerMovement.playerSpeed == playerMovement.playerSlowDown)
        {
            //agilityLevels
            Debug.Log("SLOW, water level stagnent, agility decrease");
        }
        WaterManager();
    }

    void WaterManager()
    {
        //Check if agility is lower than 40 and decrease water by 1 if the 40 increases but if it decreases increase water level    }
        if (systemLevel < 40f)
        {
            Debug.Log("Agility is low to carry the water, water level decreases gradually");
        }
        if (systemLevel > 40f)
        {
            Debug.Log("Agility is increasing, water level increases gradually");
        }
        //Check if water level is lower than 50 and increases agility  by 2 
        if (waterLevel < 50f)
        {
            playerMovement.playerSpeed = 4f;
            Debug.Log("Water level is low, agility level decreases gradually");
        }
    }
    void PlayerLose()
    {
        
    }
    void PlayerWins()
    {

    }
    void MoveControls(float playerMove)
    {
        //Based on the parameter attch it to the player speed
        playerMovement.playerSpeed = playerMove;
    }
}
