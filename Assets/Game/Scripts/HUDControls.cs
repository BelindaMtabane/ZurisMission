using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDControls : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxBucketWater = 100f;
    [SerializeField] private float maxPlayerWater = 100f;
    [SerializeField] private int maxMaterial = 100;
    [SerializeField] private float waterIncreaseRate = 15f;
    [SerializeField, Range(0f, 100f)] private float villageProgressPercent;
    [SerializeField] private float mainGameVillageMaxPercent = 34f;

    [Header("UI")]
    [SerializeField] private TMP_Text material;
    [SerializeField] private TMP_Text villageProgressText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text bucketWaterText;
    [SerializeField] private TMP_Text playerWaterText;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private Slider healthbar;
    [SerializeField] private Slider bucketbar;
    [SerializeField] private Slider playerWaterLevelBar;

    private float waterLevel;
    private float playerWater = 100f;
    private float health = 100f;
    private int materialLevel;
    private bool uiDirty = true;
    private PlayerController playerController;
    private float mainGameStartingBucket = 15f;
    private bool lowWaterWarningActive;
    private float lowWaterGraceRemaining;

    public float Health => health;
    public float PlayerWater => playerWater;
    public float MaxPlayerWater => maxPlayerWater;
    public float BucketWater => waterLevel;
    public int MaterialLevel => materialLevel;
    public int MaxMaterial => maxMaterial;
    public float MaxBucketWater => maxBucketWater;

    public float VillageProgressPercent => villageProgressPercent;

    public void ChangeBucket(float delta)
    {
        waterLevel = Mathf.Clamp(waterLevel + delta, 0f, maxBucketWater);
        MarkDirty();
        Debug.Log($"[HUDControls] Bucket {delta:+0;-0} => {waterLevel:F0}");
    }

    public void CollectCactusWater(float amount = 20f)
    {
        playerWater = Mathf.Min(maxPlayerWater, playerWater + amount);
        if (playerWater > 0f)
        {
            EndLowWaterWarning();
        }

        MarkDirty();
        Level1FeedbackUI.Show($"+{amount:0} WATER", new Color(0.35f, 0.85f, 1f));
        Debug.Log("[Level1] Cactus collected — Player Water +20");
    }

    public void CollectWaterPool(float bucketAmount = 20f, float playerWaterAmount = 0f)
    {
        waterLevel = Mathf.Min(maxBucketWater, waterLevel + bucketAmount);
        if (playerWaterAmount > 0f)
        {
            playerWater = Mathf.Min(maxPlayerWater, playerWater + playerWaterAmount);
        }

        MarkDirty();
        Level1FeedbackUI.Show($"+{bucketAmount:0} BUCKET", new Color(0.2f, 0.55f, 0.95f));
        Debug.Log("[Level1] Water pool collected — Bucket +20");
    }

    public void BreakMaterials(int amount = 10)
    {
        if (materialLevel <= 0) return;
        materialLevel = Mathf.Max(0, materialLevel - amount);
        MarkDirty();
        Debug.Log($"[Level1] Materials broken -{amount}");
    }

    public void LoseMaterialPercent(float percent)
    {
        if (percent <= 0f || materialLevel <= 0) return;
        int loss = Mathf.Max(1, Mathf.RoundToInt(maxMaterial * percent));
        materialLevel = Mathf.Max(0, materialLevel - loss);
        MarkDirty();
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level3")
        {
            Level3FeedbackUI.Show($"-{loss} MATERIALS", new Color(0.9f, 0.35f, 0.2f), 1.1f);
        }
    }

    public void LoseBucketPercent(float percent)
    {
        if (percent <= 0f || waterLevel <= 0f) return;
        float loss = Mathf.Max(1f, maxBucketWater * percent);
        waterLevel = Mathf.Max(0f, waterLevel - loss);
        MarkDirty();
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level3")
        {
            Level3FeedbackUI.Show($"-{loss:0} BUCKET WATER", new Color(0.2f, 0.55f, 0.9f), 1.1f);
        }
    }

    public const float Level3VillageStartPercent = 65f;
    public const float Level3VillageGainPercent = 35f;

    public static float Level3VillageFromTanks(int tank1, int tank2, int tank3)
    {
        float tankAverage = (tank1 + tank2 + tank3) / 3f;
        return Mathf.Clamp(Level3VillageStartPercent + tankAverage * (Level3VillageGainPercent / 100f), Level3VillageStartPercent, 100f);
    }

    public void ApplyLevel3TankProgress(int tank1, int tank2, int tank3)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Level3") return;
        villageProgressPercent = Level3VillageFromTanks(tank1, tank2, tank3);
        if (villageProgressText == null) AutoWireUiTextFields();
        BindLevel3VillageText();
        uiDirty = true;
        RefreshAllUi();
        Level3FeedbackUI.UpdateTanks(tank1, tank2, tank3);
    }

    public void DrainPlayerWater(float amount)
    {
        playerWater = Mathf.Clamp(playerWater - amount, 0f, maxPlayerWater);
        MarkDirty();
        if (playerWater <= 0f && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainGame")
        {
            BeginLowWaterWarning(18f);
            Level1FeedbackUI.Show("LOW WATER! Find cactus now!", new Color(1f, 0.35f, 0.15f), 2.5f);
        }
    }

    public void BeginLowWaterWarning(float seconds)
    {
        if (lowWaterWarningActive) return;
        lowWaterWarningActive = true;
        lowWaterGraceRemaining = seconds;
    }

    public void EndLowWaterWarning()
    {
        lowWaterWarningActive = false;
        lowWaterGraceRemaining = 0f;
    }

    public void LoseHeatWave(string reason)
    {
        Lose(reason);
    }

    public void ApplyHeatWaveTick(float delta = -10f)
    {
        DrainPlayerWater(Mathf.Abs(delta));
        Debug.Log("[Level1] Heat Wave Tick: -10");
    }

    public void AddMaterials(int amount)
    {
        materialLevel = Mathf.Clamp(materialLevel + amount, 0, maxMaterial);
        MarkDirty();
    }

    public void CollectHealthPickup(float amount = 10f)
    {
        health = Mathf.Min(maxHealth, health + amount);
        MarkDirty();
        Debug.Log("[Level1] Health collected");
        Debug.Log("[Level1] Health +10");
    }

    public void CollectMaterialPickup(int amount = 10)
    {
        materialLevel = Mathf.Clamp(materialLevel + amount, 0, maxMaterial);
        MarkDirty();
        Debug.Log("[HUDControls] Material collected");
    }

    public void CollectLevel2WaterDroplet(float playerAmount = 15f, float bucketAmount = 15f)
    {
        playerWater = Mathf.Min(maxPlayerWater, playerWater + playerAmount);
        waterLevel = Mathf.Min(maxBucketWater, waterLevel + bucketAmount);

        MarkDirty();
        Level2FeedbackUI.Show($"+{playerAmount:0} WATER", new Color(0.35f, 0.85f, 1f));
    }

    public void CollectLevel2WaterPool(float playerAmount = 15f, float bucketAmount = 15f)
    {
        CollectLevel2WaterDroplet(playerAmount, bucketAmount);
    }

    public void CollectBaobabWater(float amount = 20f)
    {
        playerWater = Mathf.Min(maxPlayerWater, playerWater + amount);
        MarkDirty();
        Level2FeedbackUI.Show($"+{amount:0} BAOBAB WATER", new Color(0.45f, 0.92f, 0.55f));
    }

    public void CollectLevel2Material(Level2MaterialKind kind, int amount = 10)
    {
        materialLevel = Mathf.Clamp(materialLevel + amount, 0, maxMaterial);
        MarkDirty();
        Level2FeedbackUI.Show($"+{amount} {kind.ToString().ToUpper()}", new Color(0.85f, 0.72f, 0.35f));
    }

    public void CollectLevel2SpeedFruit(float bucketCost = 5f)
    {
        waterLevel = Mathf.Clamp(waterLevel - Mathf.Abs(bucketCost), 0f, maxBucketWater);
        MarkDirty();
        Level2FeedbackUI.Show("SPEED!", new Color(1f, 0.45f, 0.12f), 1.3f);
    }

    public void ChangePlayerWater(float delta, string reason = null)
    {
        playerWater = Mathf.Clamp(playerWater + delta, 0f, maxPlayerWater);
        MarkDirty();
        Debug.Log($"[HUDControls] Player water {delta:+0;-0} => {playerWater:F0} {reason}");
        if (playerWater <= 0f && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainGame")
        {
            BeginLowWaterWarning(18f);
        }
        else if (playerWater > 0f)
        {
            EndLowWaterWarning();
        }
    }

    public void ChangeHealth(float delta, string reason = null)
    {
        health = Mathf.Clamp(health + delta, 0f, maxHealth);
        MarkDirty();
        Debug.Log($"[HUDControls] Health {delta:+0;-0} => {health:F0}");
        if (health <= 0f)
        {
            Lose(string.IsNullOrEmpty(reason)
                ? "Your health reached 0."
                : reason);
        }
    }

    public void ApplyHeatWave()
    {
        ApplyHeatWaveSession();
    }

    public void ApplyHeatWaveSession()
    {
        DrainPlayerWater(10f);
        Debug.Log("[HUDControls] Heat wave session: water -10");
    }

    public void DrinkBottle()
    {
        ChangePlayerWater(10f);
        Debug.Log("[HUDControls] Water bottle +10 player water.");
    }

    public void Lose(string reason)
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        RunStateManager.Instance?.NotifyDeath(reason);
    }

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        health = maxHealth;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        playerWater = maxPlayerWater;
        if (sceneName == "MainGame")
        {
            waterLevel = mainGameStartingBucket;
        }
        else if (sceneName == "Level3")
        {
            waterLevel = 25f;
        }
        else
        {
            waterLevel = 0f;
        }
        materialLevel = 0;
        villageProgressPercent = sceneName == "Level3" ? Level3VillageStartPercent : 0f;

        AutoWireUiTextFields();
        BindLevel3VillageText();

        SetMaxValues();
        if (sceneName == "MainGame")
        {
            RecalculateVillageProgress();
        }
        RefreshAllUi();
    }

    void Update()
    {
        if (uiDirty)
        {
            RefreshAllUi();
        }

        if (lowWaterWarningActive)
        {
            if (playerWater > 0f)
            {
                EndLowWaterWarning();
            }
            else
            {
                lowWaterGraceRemaining -= Time.deltaTime;
                if (lowWaterGraceRemaining <= 0f)
                {
                    lowWaterWarningActive = false;
                    Lose("Out of Water! Collect cactus to stay hydrated.");
                }
            }
        }

        if (staminaText != null)
        {
            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
                staminaText.text = $"Stamina : {playerController.Stamina:F0}/{playerController.MaxStamina:F0}";
        }
    }

    void SetMaxValues()
    {
        if (healthbar != null)
        {
            healthbar.maxValue = maxHealth;
        }

        if (bucketbar != null)
        {
            bucketbar.maxValue = maxBucketWater;
        }

        if (playerWaterLevelBar != null)
        {
            playerWaterLevelBar.maxValue = maxPlayerWater;
        }
    }

    void RefreshAllUi()
    {
        if (material != null)
            material.text = $"Material: {materialLevel}/{maxMaterial}";

        if (villageProgressText != null)
        {
            // Match the existing edit-time label formatting in the scene:
            // "Village Progress : 0%"
            villageProgressText.text = $"Village Progress : {villageProgressPercent:F0}%";
        }

        if (healthText != null)
            healthText.text = $"Health : {health:F0}/{maxHealth:F0}";
        if (bucketWaterText != null)
            bucketWaterText.text = $"Bucket : {waterLevel:F0}/{maxBucketWater:F0}";
        if (playerWaterText != null)
            playerWaterText.text = $"Water : {playerWater:F0}/{maxPlayerWater:F0}";
        if (staminaText != null && playerController != null)
            staminaText.text = $"Stamina : {playerController.Stamina:F0}/{playerController.MaxStamina:F0}";

        if (healthbar != null)
        {
            healthbar.value = health;
        }

        if (bucketbar != null)
        {
            bucketbar.value = waterLevel;
        }

        if (playerWaterLevelBar != null)
        {
            playerWaterLevelBar.value = playerWater;
        }

        // Debug trace so you can confirm HUD updates are happening.
        Debug.Log(
            $"[HUDControls] HUD update | Material={materialLevel} Village={villageProgressPercent:F0}% Health={health:F0} Water={playerWater:F0} Bucket={waterLevel:F0}");

        uiDirty = false;
    }

    void MarkDirty()
    {
        RecalculateVillageProgress();
        uiDirty = true;
    }

    void RecalculateVillageProgress()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainGame")
        {
            return;
        }

        float materialNorm = materialLevel / (float)maxMaterial;
        float bucketRange = Mathf.Max(1f, maxBucketWater - mainGameStartingBucket);
        float bucketNorm = Mathf.Clamp01((waterLevel - mainGameStartingBucket) / bucketRange);
        villageProgressPercent = ((materialNorm + bucketNorm) * 0.5f) * mainGameVillageMaxPercent;
        villageProgressPercent = Mathf.Clamp(villageProgressPercent, 0f, mainGameVillageMaxPercent);
    }

    public void SetVillageProgress(float percent)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainGame")
        {
            RecalculateVillageProgress();
        }
        else
        {
            villageProgressPercent = Mathf.Clamp(percent, 0f, 100f);
        }

        MarkDirty();
        Debug.Log($"[HUDControls] Village progress {villageProgressPercent:F0}%");
    }

    void CheckDeath()
    {
        if (health <= 0f)
        {
            Lose("Your health reached 0.");
        }
        else if (playerWater <= 0f)
        {
            BeginLowWaterWarning(18f);
        }
    }

    public void ShowActionFeedback(string message, Color color)
    {
        Level1FeedbackUI.Show(message, color);
    }

    public void SpeedControls(float newSpeed)
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        if (playerController == null) return;

        playerController.ApplySpeedModifier(newSpeed, 5f);
        WaterMoveManager();

        Debug.Log($"[HUDControls] SpeedControls speed={newSpeed}");
    }

    public void WaterMoveManager()
    {
        float bucketDrain = playerController != null && playerController.CurrentSpeed >= 40f
            ? Random.Range(5f, 10f)
            : 3f;

        waterLevel = Mathf.Clamp(waterLevel - bucketDrain, 0f, maxBucketWater);

        if (playerController != null && playerController.CurrentSpeed <= 20f)
        {
            playerWater = Mathf.Clamp(playerWater - 2f, 0f, maxPlayerWater);
            CheckDeath();
        }

        MarkDirty();

        Debug.Log($"[HUDControls] WaterMoveManager bucketDrain={bucketDrain:F1} bucketWater={waterLevel:F1} playerWater={playerWater:F1}");
    }

    public void WaterIncreaseManager()
    {
        waterLevel = Mathf.Clamp(waterLevel + waterIncreaseRate, 0f, maxBucketWater);
        MarkDirty();

        Debug.Log($"[HUDControls] WaterIncreaseManager +{waterIncreaseRate:F1} => bucketWater={waterLevel:F1}");
    }

    public void HealthDecreaseManager()
    {
        health -= Random.Range(3f, 15f);
        health = Mathf.Clamp(health, 0f, maxHealth);
        MarkDirty();
        CheckDeath();

        Debug.Log($"[HUDControls] HealthDecreaseManager health={health:F1}");
    }

    public void HealthIncreaseManager()
    {
        health += Random.Range(3f, 15f);
        health = Mathf.Clamp(health, 0f, maxHealth);
        MarkDirty();

        Debug.Log($"[HUDControls] HealthIncreaseManager health={health:F1}");
    }

    public void SystemBuild()
    {
        if (materialLevel >= maxMaterial) return;

        materialLevel += Random.Range(10, 25);
        materialLevel = Mathf.Clamp(materialLevel, 0, maxMaterial);
        MarkDirty();

        Debug.Log($"[HUDControls] SystemBuild materialLevel={materialLevel}");
    }

    public void PlayerWaterINC()
    {
        ChangePlayerWater(10f);
    }

    public void PlayerWaterDEC()
    {
        ChangePlayerWater(-5f);
    }

    public void SceneChange(float sceneNumber)
    {
        if (sceneNumber == 2f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
        }

        if (sceneNumber == 4f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level3");
        }
    }

    public void CollectLevel3Material(string kind, int amount = 10)
    {
        materialLevel = Mathf.Clamp(materialLevel + amount, 0, maxMaterial);
        MarkDirty();
    }

    public void CollectLevel3Bucket(float amount = 15f)
    {
        waterLevel = Mathf.Min(maxBucketWater, waterLevel + amount);
        MarkDirty();
    }

    public void CollectLevel3Health(float amount = 15f)
    {
        health = Mathf.Min(maxHealth, health + amount);
        MarkDirty();
    }

    public void LevelProgress()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level3")
        {
            if (health <= 0f)
            {
                Lose("Your health reached 0.");
                return;
            }

            if (Level3PipeRepair.AllTanksRepaired)
            {
                RunStateManager.Instance?.NotifyVictory();
                Debug.Log("[HUDControls] Level3 complete — all tanks repaired");
                return;
            }

            Lose("Repair all three water tanks to 100% before the end.");
            return;
        }

        if (health <= 0f)
        {
            Lose("Your health reached 0.");
            return;
        }

        if (playerWater <= 0f)
        {
            Lose("You ran out of water.");
            return;
        }

        bool missingMaterials = materialLevel < maxMaterial;
        bool bucketNotFull = waterLevel < maxBucketWater;

        if (missingMaterials || bucketNotFull)
        {
            string reason;
            if (missingMaterials && bucketNotFull)
            {
                reason = $"You reached the end without enough materials ({materialLevel}/{maxMaterial}) and your bucket was not full ({waterLevel:F0}/{maxBucketWater:F0}).";
            }
            else if (missingMaterials)
            {
                reason = $"You reached the end without enough materials ({materialLevel}/{maxMaterial}).";
            }
            else
            {
                reason = $"You reached the end but your water bucket was not full ({waterLevel:F0}/{maxBucketWater:F0}). You needed {maxBucketWater:F0}.";
            }

            Lose(reason);
            return;
        }

        RunStateManager.Instance?.NotifyVictory();
        Debug.Log("[HUDControls] LevelProgress complete=true");
    }

    void BindLevel3VillageText()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Level3") return;
        GameObject go = GameObject.Find("VILLAGEtext");
        if (go == null) return;
        TMP_Text t = go.GetComponent<TMP_Text>();
        if (t != null) villageProgressText = t;
    }

    void AutoWireUiTextFields()
    {
        // Scene should already have these wired. This is a fallback in case
        // Inspector references were lost after a script rewrite.
        TMP_Text[] all = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (all == null || all.Length == 0) return;

        foreach (TMP_Text t in all)
        {
            if (t == null) continue;

            string txt = t.text != null ? t.text.Trim() : "";
            string goName = t.gameObject != null ? t.gameObject.name : "";

            if (material == null &&
                (goName.Contains("Material") || txt.StartsWith("Material") || txt == "MaterialLVL"))
            {
                material = t;
            }
            else if (villageProgressText == null &&
                     (txt.StartsWith("Village Progress") || goName.Contains("Village") || goName == "VILLAGEtext"))
            {
                villageProgressText = t;
            }
            else if (healthText == null &&
                     (txt.StartsWith("Health") || txt == "HealthLVL"))
            {
                healthText = t;
            }
            else if (playerWaterText == null &&
                     (txt.StartsWith("Water") || txt == "WaterLVL"))
            {
                playerWaterText = t;
            }
            else if (bucketWaterText == null &&
                     (txt.StartsWith("Bucket") || txt == "BucketLVL"))
            {
                bucketWaterText = t;
            }
        }

        Debug.Log(
            $"[HUDControls] AutoWireText => material={(material != null)}, village={(villageProgressText != null)}, health={(healthText != null)}, water={(playerWaterText != null)}, bucket={(bucketWaterText != null)}");
    }
}
