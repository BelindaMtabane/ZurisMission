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

public class HUDControls : MonoBehaviour
{
    //public TMP_Text waterLevelText;
    public TMP_Text material;
    //public TMP_Text healthText;
    public TMP_Text villageProgressText;

    //Bucket variables
    //private float waterMax = 100f;
    private float waterLevel = 0f;
    public float waterIncreaseRate;
    public int bucket;

    //Player variables
    private float waterLvlPLY = 100f;
    private float health = 100f;
    PlayerMovement playerMovement;
    PlayerMovementOG playerMovementOG;

    bool isDead = false;

    //Village variables
    private float villageLevel;
    private float villageLevelTarget;
    public int materialLevel;

    //UI bars
    [SerializeField] private Slider healthbar;
    [SerializeField] private Slider bucketbar;
    //[SerializeField] private Slider materialBar;
    [SerializeField] private Slider playerWaterLevelBar;
    [SerializeField] private GameObject failPanel;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerMovementOG = FindFirstObjectByType<PlayerMovementOG>();
        SetMax();
        VillageProgress();
    }
    void Update()
    {
        UpdateUI();
        BarFilling();
        DeathCheck();
    }

    void UpdateUI()
    {
        //Update the UI text to display the current water level and agility level
        //waterLevelText.text = $"Water LVL: {waterLevel:F0}";
        material.text = ("Material:" + materialLevel);
        //healthText.text = $"Health:  {health:F0}";
        villageProgressText.text = $"Village Progress:  {villageLevel:F0}%";

    }
    float ActiveSpeed()
    {
        if (playerMovement != null) return playerMovement.playerSpeed;
        if (playerMovementOG != null) return playerMovementOG.playerSpeed;
        return 20f;
    }

    void SetActiveSpeed(float speed)
    {
        if (playerMovement != null) playerMovement.playerSpeed = speed;
        if (playerMovementOG != null) playerMovementOG.playerSpeed = speed;
    }

    public void SpeedControls(float playerMove)
    {
        SetActiveSpeed(playerMove);
        Debug.Log("Player speed set to " + playerMove);
        StopCoroutine("ResetSpeed");
        StartCoroutine(ResetSpeed());
        WaterMoveManager();

        if (playerMove >= 40f)
            GameInfoUI.Post("Zuri is moving fast! Speed boosted.", GameInfoUI.MsgType.Interaction);
        else
            GameInfoUI.Post("Zuri has been slowed down!", GameInfoUI.MsgType.Obstacle);
    }

    private IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(5f);
        SetActiveSpeed(20f);
    }

    public void WaterMoveManager()
    {
        float speed = ActiveSpeed();
        float waterDecreaseFAST = UnityEngine.Random.Range(5f, 10f);
        float waterDecreaseNORM = 3f;
        if (speed >= 40f)
        {
            waterLevel -= waterDecreaseFAST;
            Debug.Log("FAST, water level decreased by " + waterDecreaseFAST);
        }
        if (speed <= 20f)
        {
            waterLevel -= waterDecreaseNORM;
            waterLvlPLY -= 2f;
            Debug.Log("Water level decreased on NORM by " + waterDecreaseNORM);
        }
    }
    // all good ++
    public void WaterIncreaseManager()
    {
        waterLevel += waterIncreaseRate;
        Debug.Log("Water bucket level increased by " + waterIncreaseRate);
        GameInfoUI.Post("Zuri filled the bucket with water!", GameInfoUI.MsgType.Pickup);
    }
    // all good ++
    public void HealthDecreaseManager()
    {
        float playerDAMGE = UnityEngine.Random.Range(3f, 15f);
        health -= playerDAMGE;
        Debug.Log("Player health decreased by " + playerDAMGE);
        GameInfoUI.Post($"Zuri took damage! -{playerDAMGE:F0} health", GameInfoUI.MsgType.Loss);
        if (health <= 0f) health = 0f;
    }
    // all good ++
    public void HealthIncreaseManager()
    {
        float healthIncrease = UnityEngine.Random.Range(3f, 15f);
        if (health < 100f)
        {
            health += healthIncrease;
            Debug.Log("Player health increased by " + healthIncrease);
            GameInfoUI.Post($"Zuri is healing! +{healthIncrease:F0} health", GameInfoUI.MsgType.Pickup);
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
        //materialBar.value = waterSystemLevel;
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
        //materialBar.maxValue = 100f;
        //materialBar.value = waterSystemLevel;

        //Set the max value of the player water level bar to 100
        playerWaterLevelBar.maxValue = 100f;
        playerWaterLevelBar.value = waterLvlPLY;
    }
    void VillageProgress()
    {
        if (GetActiveScene().name == "MainGame")
        {
            villageLevel = 0f;
            villageLevelTarget = 66f;
        }
        else if (GetActiveScene().name == "Level2")
        {
            villageLevel = 67f;
            villageLevelTarget = 88f;
        }
        else if (GetActiveScene().name == "Level3End")
        {
            villageLevel = 89f;
            villageLevelTarget = 96f;
        }
    }

    public void VillageProgressIncrease()
    {
        float increase = UnityEngine.Random.Range(3f, 6f);
        villageLevel = Mathf.Min(villageLevel + increase, villageLevelTarget);
        Debug.Log($"Village progress +{increase:F1} → {villageLevel:F1}%");
    }

    public void VillageProgressDecrease()
    {
        float decrease = UnityEngine.Random.Range(1f, 3f);
        villageLevel = Mathf.Max(villageLevel - decrease, 0f);
        Debug.Log($"Village progress -{decrease:F1} → {villageLevel:F1}%");
    }
    public void SystemBuild()
    {
        if (materialLevel < 100)
        {
            int materialIncrease = UnityEngine.Random.Range(10, 25);
            materialLevel += materialIncrease;
            if (materialLevel >= 100)
            {
                materialLevel = 100;
                Debug.Log("Material collection is maxed");
                GameInfoUI.Post("Zuri has maxed out all materials!", GameInfoUI.MsgType.Win);
            }
            else
            {
                GameInfoUI.Post($"Zuri collected materials! +{materialIncrease}", GameInfoUI.MsgType.Pickup);
            }
        }
    }
    public void PitDamage()
    {
        float pitHealthDamage   = UnityEngine.Random.Range(25f, 40f);
        float pitWaterDamage    = UnityEngine.Random.Range(20f, 35f);
        int   pitMaterialDamage = UnityEngine.Random.Range(15, 25);

        health        = Mathf.Max(health - pitHealthDamage, 0f);
        waterLvlPLY   = Mathf.Max(waterLvlPLY - pitWaterDamage, 0f);
        materialLevel = Mathf.Max(materialLevel - pitMaterialDamage, 0);

        VillageProgressDecrease();
        GameInfoUI.Post($"Zuri fell into a pit! Health -{pitHealthDamage:F0}, Water -{pitWaterDamage:F0}", GameInfoUI.MsgType.Loss);
        Debug.Log($"Pit! Health -{pitHealthDamage:F0}, Water -{pitWaterDamage:F0}, Materials -{pitMaterialDamage}");
        DeathCheck();
    }

    public void PlayerWaterINC()
    {
        waterLvlPLY += 5f;
        GameInfoUI.Post("Zuri collected a water drop!", GameInfoUI.MsgType.Pickup);
    }
    public void PlayerWaterDEC()
    {
        waterLvlPLY -= 5f;
        GameInfoUI.Post("Zuri is affected by heat and disease! Water drained.", GameInfoUI.MsgType.Obstacle);
    }
    public void SceneChange(float scenenumber)
    {
        if (scenenumber == 2f)
        {
            SceneManager.LoadScene("Level2");
        }
        if (scenenumber == 4f)
        {
            SceneManager.LoadScene("Level3End");
        }
    }
    public void LevelProgress()
    {
        if (GetActiveScene().name == "MainGame")
        {
            if (villageLevel >= villageLevelTarget)
            {
                Debug.Log("Level 1 Completed — Village at " + villageLevel.ToString("F0") + "%");
                GameInfoUI.Post("Zuri completed Level 1! Village target reached.", GameInfoUI.MsgType.Win);
            }
            else
            {
                Debug.Log("Level 1 ended — Village at " + villageLevel.ToString("F0") + "%");
                GameInfoUI.Post($"Zuri ended Level 1 with {villageLevel:F0}% village progress.", GameInfoUI.MsgType.Interaction);
            }
        }
        else if (GetActiveScene().name == "Level2")
        {
            if (villageLevel >= villageLevelTarget)
            {
                Debug.Log("Level 2 Completed — Village at " + villageLevel.ToString("F0") + "%");
                GameInfoUI.Post("Zuri completed Level 2! Village target reached.", GameInfoUI.MsgType.Win);
            }
            else
            {
                Debug.Log("Level 2 ended — Village at " + villageLevel.ToString("F0") + "%");
                GameInfoUI.Post($"Zuri ended Level 2 with {villageLevel:F0}% village progress.", GameInfoUI.MsgType.Interaction);
            }
        }
    }
    void DeathCheck()
    {
        if (isDead) return;
        if (health <= 0f || waterLvlPLY <= 0f)
            GameOver();
    }

    void GameOver()
    {
        isDead = true;
        Time.timeScale = 0f;
        if (failPanel != null)
            failPanel.SetActive(true);
        GameInfoUI.Post("Zuri has fallen! Game Over.", GameInfoUI.MsgType.Loss);
        Debug.Log("Game Over!");
    }
}
