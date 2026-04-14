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
    void WaterManager()
    {
        float waterLeveldecrease = UnityEngine.Random.Range(2f, 10f);
        //Check if player is fast or slow or normal
        if (playerMovement.playerSpeed == playerMovement.playerSpeedBoost)
        {
            //Decrease water level and increase player speed
            waterLevel -= waterLeveldecrease;
            Debug.Log("FAST, water level decreased by " + waterLeveldecrease);

        }
        else if (playerMovement.playerSpeed == playerMovement.playerSlowDown)
        {
            waterLevel += waterLeveldecrease;
            Debug.Log("SLOW, water level decreased by " + waterLeveldecrease);
        }
        WaterSystem();
        WaterLimit();
    }

    void WaterSystem()
    {
        //Check if agility is lower than 40 and decrease water by 1 if the 40 increases but if it decreases increase water level    }
        if (systemLevel < 40f)
        {
            Debug.Log("Agility is low to carry the water, water level decreases gradually");
        }
        else if (systemLevel > 40f)
        {
            Debug.Log("Agility is increasing, water level increases gradually");
        }
    }
    void WaterLimit()
    {
        //Check if water level is lower than 50 and increases agility  by 2 
        if (waterLevel < +50f)
        {
            playerMovement.playerSpeed = 4f;
            Debug.Log("Water level is low, agility level decreases gradually");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            waterLevel -= 5f;
            health -= 5f;
        }
        else if (other.gameObject.CompareTag("Dam"))
        {
            waterLevel += 8f;
        }
        else if (other.gameObject.CompareTag("Animal"))
        {
            waterLevel -= 12f;
            health -= 10f;
        }
    }
}
