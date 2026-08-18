using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDControls : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float waterIncreaseRate = 15f;

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
    [SerializeField] private Slider materialBar;

    PlayerResources playerResources;
    VillageProgressService villageProgress;
    bool uiDirty = true;
    PlayerController playerController;
    bool lowWaterWarningActive;
    float lowWaterGraceRemaining;

    public PlayerResources PlayerResources => playerResources;
    public VillageProgressService VillageProgress => villageProgress;

    public float Health => playerResources != null ? playerResources.Health : 0f;
    public float PlayerWater => playerResources != null ? playerResources.PlayerWater : 0f;
    public float MaxPlayerWater => playerResources != null ? playerResources.MaxPlayerWater : 100f;
    public float BucketWater => playerResources != null ? playerResources.BucketWater : 0f;
    public int MaterialLevel => playerResources != null ? playerResources.Materials : 0;
    public int MaxMaterial => playerResources != null ? playerResources.MaxMaterials : 100;
    public float MaxBucketWater => playerResources != null ? playerResources.MaxBucketWater : 100f;

    public float VillageProgressPercent => villageProgress != null ? villageProgress.CurrentPercent : 0f;

    void Awake()
    {
        EnsurePlayerResources();
        EnsureVillageProgress();
    }

    void EnsureVillageProgress()
    {
        if (villageProgress == null)
        {
            villageProgress = GetComponent<VillageProgressService>();
        }

        if (villageProgress == null)
        {
            villageProgress = gameObject.AddComponent<VillageProgressService>();
        }

        villageProgress.OnChanged -= HandleVillageProgressChanged;
        villageProgress.OnChanged += HandleVillageProgressChanged;
    }

    void HandleVillageProgressChanged()
    {
        uiDirty = true;
    }

    void EnsurePlayerResources()
    {
        if (playerResources == null)
        {
            playerResources = GetComponent<PlayerResources>();
        }

        if (playerResources == null)
        {
            playerResources = gameObject.AddComponent<PlayerResources>();
        }

        playerResources.OnChanged -= HandleResourcesChanged;
        playerResources.OnChanged += HandleResourcesChanged;
    }

    void HandleResourcesChanged()
    {
        villageProgress?.RecalculateFromResources(playerResources);
    }

    void OnDestroy()
    {
        if (playerResources != null)
        {
            playerResources.OnChanged -= HandleResourcesChanged;
        }

        if (villageProgress != null)
        {
            villageProgress.OnChanged -= HandleVillageProgressChanged;
        }
    }

    public void ChangeBucket(float delta)
    {
        if (playerResources == null) return;
        playerResources.ChangeBucketWater(delta);
        Debug.Log($"[HUDControls] Bucket {delta:+0;-0} => {playerResources.BucketWater:F0}");
        if (SceneCatalog.IsLevel1(SceneCatalog.ActiveName) && delta < 0f)
        {
            Level1FeedbackUI.Show(
                $"-{Mathf.Abs(delta):0} BUCKET WATER",
                new Color(0.3f, 0.6f, 0.95f),
                1.2f);
        }
    }

    public void CollectCactusWater(float playerAmount = 20f, float bucketAmount = 0f)
    {
        if (playerResources == null) return;
        playerResources.AddPlayerWater(playerAmount);
        if (bucketAmount > 0f)
        {
            playerResources.AddBucketWater(bucketAmount);
        }

        if (playerResources.PlayerWater > 0f)
        {
            EndLowWaterWarning();
        }

        Debug.Log($"[Level1] Cactus collected — Player Water +{playerAmount:0}, Bucket +{bucketAmount:0}");
    }

    public void CollectWaterPool(float bucketAmount = 10f, float playerWaterAmount = 20f)
    {
        if (playerResources == null) return;
        playerResources.AddBucketWater(bucketAmount);
        playerResources.AddPlayerWater(playerWaterAmount);
        if (playerResources.PlayerWater > 0f)
        {
            EndLowWaterWarning();
        }

        Debug.Log($"[Level1] Water spring collected — Player Water +{playerWaterAmount:0}, Bucket +{bucketAmount:0}");
    }

    public void BreakMaterials(int amount = 10)
    {
        if (playerResources == null) return;
        playerResources.RemoveMaterials(amount);
        Debug.Log($"[Level1] Materials broken -{amount}");
    }

    public void LoseMaterialPercent(float percent)
    {
        if (playerResources == null) return;
        int loss = playerResources.GetMaterialLossForPercent(percent);
        if (loss <= 0) return;

        playerResources.RemoveMaterialPercent(percent);
        if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName))
        {
            Level3FeedbackUI.Show($"-{loss} MATERIALS", new Color(0.9f, 0.35f, 0.2f), 1.1f);
        }
    }

    public void LoseBucketPercent(float percent)
    {
        if (playerResources == null) return;
        float loss = playerResources.GetBucketLossForPercent(percent);
        if (loss <= 0f) return;

        playerResources.RemoveBucketPercent(percent);
        if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName))
        {
            Level3FeedbackUI.Show($"-{loss:0} BUCKET WATER", new Color(0.2f, 0.55f, 0.9f), 1.1f);
        }
    }

    public void ApplyLevel3TankProgress(int tank1, int tank2, int tank3)
    {
        if (!SceneCatalog.IsLevel3(SceneCatalog.ActiveName)) return;
        villageProgress?.ApplyTankProgress(tank1, tank2, tank3);
        if (villageProgressText == null) AutoWireUiTextFields();
        BindLevel3VillageText();
        RefreshAllUi();
        Level3FeedbackUI.UpdateTanks(tank1, tank2, tank3);
    }

    public void DrainPlayerWater(float amount)
    {
        if (playerResources == null) return;
        playerResources.RemovePlayerWater(amount);
        if (playerResources.PlayerWater <= 0f && SceneCatalog.IsLevel1(SceneCatalog.ActiveName))
        {
            BeginLowWaterWarning(18f);
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
        playerResources?.AddMaterials(amount);
    }

    public void CollectHealthPickup(float amount = 10f)
    {
        playerResources?.AddHealth(amount);
        Debug.Log("[Level1] Health collected");
        Debug.Log("[Level1] Health +10");
    }

    public void CollectMaterialPickup(int amount = 10)
    {
        playerResources?.AddMaterials(amount);
        Debug.Log("[HUDControls] Material collected");
    }

    public void CollectLevel2WaterDroplet(float playerAmount = 15f, float bucketAmount = 15f)
    {
        if (playerResources == null) return;
        playerResources.AddPlayerWater(playerAmount);
        playerResources.AddBucketWater(bucketAmount);
    }

    public void CollectLevel2WaterPool(float playerAmount = 10f, float bucketAmount = 25f)
    {
        if (playerResources == null) return;
        playerResources.AddPlayerWater(playerAmount);
        playerResources.AddBucketWater(bucketAmount);
    }

    public void CollectBaobabWater(float amount = 20f)
    {
        playerResources?.AddPlayerWater(amount);
    }

    public void CollectLevel2Material(Level2MaterialKind kind, int amount = 10)
    {
        playerResources?.AddMaterials(amount);
    }

    public void CollectLevel2SpeedFruit(float bucketCost = 5f)
    {
        playerResources?.RemoveBucketWater(Mathf.Abs(bucketCost));
    }

    public void CollectLevel2Health(float amount = 15f)
    {
        playerResources?.AddHealth(amount);
    }

    public void ChangePlayerWater(float delta, string reason = null)
    {
        if (playerResources == null) return;
        playerResources.ChangePlayerWater(delta);
        Debug.Log($"[HUDControls] Player water {delta:+0;-0} => {playerResources.PlayerWater:F0} {reason}");
        if (playerResources.PlayerWater <= 0f && SceneCatalog.IsLevel1(SceneCatalog.ActiveName))
        {
            BeginLowWaterWarning(18f);
        }
        else if (playerResources.PlayerWater > 0f)
        {
            EndLowWaterWarning();
        }
    }

    public void ChangeHealth(float delta, string reason = null)
    {
        if (playerResources == null) return;
        playerResources.ChangeHealth(delta);
        Debug.Log($"[HUDControls] Health {delta:+0;-0} => {playerResources.Health:F0}");
        if (SceneCatalog.IsLevel1(SceneCatalog.ActiveName) && delta < 0f)
        {
            Level1FeedbackUI.Show(
                $"-{Mathf.Abs(delta):0} HEALTH",
                new Color(0.95f, 0.25f, 0.2f),
                1.4f);
        }

        if (playerResources.Health <= 0f)
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
        EnsurePlayerResources();
        EnsureVillageProgress();
        playerController = FindFirstObjectByType<PlayerController>();

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        AutoWireUiTextFields();
        BindLevel3VillageText();
        HideAllResourceSliders();

        SetMaxValues();
        if (SceneCatalog.UsesObjectiveVillageProgress(sceneName))
        {
            villageProgress?.RecalculateFromResources(playerResources);
        }
        RefreshAllUi();
    }

    void Update()
    {
        if (uiDirty)
        {
            RefreshAllUi();
        }

        if (lowWaterWarningActive && playerResources != null)
        {
            if (playerResources.PlayerWater > 0f)
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
        if (playerResources == null) return;

        if (healthbar != null)
        {
            healthbar.maxValue = playerResources.MaxHealth;
        }

        if (bucketbar != null)
        {
            bucketbar.maxValue = playerResources.MaxBucketWater;
        }

        if (playerWaterLevelBar != null)
        {
            playerWaterLevelBar.maxValue = playerResources.MaxPlayerWater;
        }

        if (materialBar != null)
        {
            materialBar.minValue = 0f;
            materialBar.maxValue = playerResources.MaxMaterials;
            materialBar.wholeNumbers = true;
        }
    }

    void RefreshAllUi()
    {
        if (playerResources == null)
        {
            uiDirty = false;
            return;
        }

        if (material != null)
            material.text = $"Material: {playerResources.Materials}/{playerResources.MaxMaterials}";

        if (villageProgressText != null && villageProgress != null)
        {
            villageProgressText.text = $"Village Progress : {villageProgress.CurrentPercent:F0}%";
        }

        if (healthText != null)
            healthText.text = $"Health : {playerResources.Health:F0}/{playerResources.MaxHealth:F0}";
        if (bucketWaterText != null)
            bucketWaterText.text = $"Bucket : {playerResources.BucketWater:F0}/{playerResources.MaxBucketWater:F0}";
        if (playerWaterText != null)
            playerWaterText.text = $"Water : {playerResources.PlayerWater:F0}/{playerResources.MaxPlayerWater:F0}";
        if (staminaText != null && playerController != null)
            staminaText.text = $"Stamina : {playerController.Stamina:F0}/{playerController.MaxStamina:F0}";

        if (healthbar != null)
        {
            healthbar.value = playerResources.Health;
        }

        if (bucketbar != null)
        {
            bucketbar.value = playerResources.BucketWater;
        }

        if (playerWaterLevelBar != null)
        {
            playerWaterLevelBar.value = playerResources.PlayerWater;
        }

        if (materialBar != null)
        {
            materialBar.value = playerResources.Materials;
        }

        float village = villageProgress != null ? villageProgress.CurrentPercent : 0f;
        Debug.Log(
            $"[HUDControls] HUD update | Material={playerResources.Materials} Village={village:F0}% Health={playerResources.Health:F0} Water={playerResources.PlayerWater:F0} Bucket={playerResources.BucketWater:F0}");

        uiDirty = false;
    }

    void MarkDirty()
    {
        uiDirty = true;
    }

    public void SetVillageProgress(float percent)
    {
        EnsureVillageProgress();
        villageProgress?.SetOrRecalculate(playerResources, percent);
        uiDirty = true;
        Debug.Log($"[HUDControls] Village progress {VillageProgressPercent:F0}%");
    }

    void CheckDeath()
    {
        if (playerResources == null) return;

        if (playerResources.Health <= 0f)
        {
            Lose("Your health reached 0.");
        }
        else if (playerResources.PlayerWater <= 0f)
        {
            BeginLowWaterWarning(18f);
        }
    }

    public void ShowActionFeedback(string message, Color color)
    {
        if (SceneCatalog.IsLevel1(SceneCatalog.ActiveName))
        {
            Level1FeedbackUI.Show(message, color);
        }
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
        if (playerResources == null) return;

        float bucketDrain = playerController != null && playerController.CurrentSpeed >= 40f
            ? Random.Range(5f, 10f)
            : 3f;

        playerResources.RemoveBucketWater(bucketDrain);

        if (playerController != null && playerController.CurrentSpeed <= 20f)
        {
            playerResources.RemovePlayerWater(2f);
            CheckDeath();
        }

        Debug.Log($"[HUDControls] WaterMoveManager bucketDrain={bucketDrain:F1} bucketWater={playerResources.BucketWater:F1} playerWater={playerResources.PlayerWater:F1}");
    }

    public void WaterIncreaseManager()
    {
        if (playerResources == null) return;
        playerResources.AddBucketWater(waterIncreaseRate);
        Debug.Log($"[HUDControls] WaterIncreaseManager +{waterIncreaseRate:F1} => bucketWater={playerResources.BucketWater:F1}");
    }

    public void HealthDecreaseManager()
    {
        if (playerResources == null) return;
        playerResources.RemoveHealth(Random.Range(3f, 15f));
        CheckDeath();
        Debug.Log($"[HUDControls] HealthDecreaseManager health={playerResources.Health:F1}");
    }

    public void HealthIncreaseManager()
    {
        if (playerResources == null) return;
        playerResources.AddHealth(Random.Range(3f, 15f));
        Debug.Log($"[HUDControls] HealthIncreaseManager health={playerResources.Health:F1}");
    }

    public void SystemBuild()
    {
        if (playerResources == null) return;
        if (playerResources.Materials >= playerResources.MaxMaterials) return;

        playerResources.AddMaterials(Random.Range(10, 25));
        Debug.Log($"[HUDControls] SystemBuild materialLevel={playerResources.Materials}");
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
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneCatalog.Level2);
        }

        if (sceneNumber == 4f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneCatalog.Level3);
        }
    }

    public void CollectLevel3Material(string kind, int amount = 10)
    {
        playerResources?.AddMaterials(amount);
    }

    public void CollectLevel3Bucket(float amount = 15f)
    {
        playerResources?.AddBucketWater(amount);
    }

    public void CollectLevel3Health(float amount = 15f)
    {
        playerResources?.AddHealth(amount);
    }

    public void LevelProgress()
    {
        if (playerResources == null) return;

        if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName))
        {
            if (playerResources.Health <= 0f)
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

        if (playerResources.Health <= 0f)
        {
            Lose("Your health reached 0.");
            return;
        }

        if (playerResources.PlayerWater <= 0f)
        {
            Lose("You ran out of water.");
            return;
        }

        bool missingMaterials = playerResources.Materials < playerResources.MaxMaterials;
        bool bucketNotFull = playerResources.BucketWater < playerResources.MaxBucketWater;

        if (missingMaterials || bucketNotFull)
        {
            string reason;
            if (missingMaterials && bucketNotFull)
            {
                reason = $"You reached the end without enough materials ({playerResources.Materials}/{playerResources.MaxMaterials}) and your bucket was not full ({playerResources.BucketWater:F0}/{playerResources.MaxBucketWater:F0}).";
            }
            else if (missingMaterials)
            {
                reason = $"You reached the end without enough materials ({playerResources.Materials}/{playerResources.MaxMaterials}).";
            }
            else
            {
                reason = $"You reached the end but your water bucket was not full ({playerResources.BucketWater:F0}/{playerResources.MaxBucketWater:F0}). You needed {playerResources.MaxBucketWater:F0}.";
            }

            Lose(reason);
            return;
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (SceneCatalog.IsLevel1(sceneName))
        {
            RunStateManager.Instance?.NotifyVictory(
                "LEVEL 1 COMPLETE",
                "You collected enough materials, kept your health and water up, and filled the bucket.\n\nStart Level 2 when you are ready.");
        }
        else if (SceneCatalog.IsLevel2(sceneName))
        {
            RunStateManager.Instance?.NotifyVictory(
                "LEVEL 2 COMPLETE",
                "You collected enough materials, kept your health and water up, and filled the bucket to create the bowl.\n\nStart Level 3 when you are ready.");
        }
        else
        {
            RunStateManager.Instance?.NotifyVictory();
        }

        Debug.Log("[HUDControls] LevelProgress complete=true");
    }

    void BindLevel3VillageText()
    {
        if (!SceneCatalog.IsLevel3(SceneCatalog.ActiveName)) return;
        GameObject go = GameObject.Find("VILLAGEtext");
        if (go == null) return;
        TMP_Text t = go.GetComponent<TMP_Text>();
        if (t != null) villageProgressText = t;
    }

    void AutoWireUiTextFields()
    {
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

    void HideAllResourceSliders()
    {
        healthbar = null;
        bucketbar = null;
        playerWaterLevelBar = null;
        materialBar = null;

        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (sliders == null || sliders.Length == 0) return;

        for (int i = 0; i < sliders.Length; i++)
        {
            Slider slider = sliders[i];
            if (slider == null || slider.gameObject == null) continue;
            if (!IsResourceSlider(slider.gameObject.name)) continue;

            slider.gameObject.SetActive(false);
        }
    }

    static bool IsResourceSlider(string goName)
    {
        if (string.IsNullOrEmpty(goName)) return false;

        string upper = goName.ToUpperInvariant();
        return upper.Contains("HEALTH")
            || upper.Contains("BUCKET")
            || upper.Contains("MATERIAL")
            || upper.Contains("WATERPLY")
            || goName == "SliderWATERPLY"
            || (upper.Contains("WATER") && !upper.Contains("BUCKET"));
    }

    Slider GetOrCreateResourceSlider(string sliderName, string[] parentNames, Color fillColor, Vector2 fallbackBarPosition, string label)
    {
        Transform parent = FindHudBar(parentNames);
        bool createdFrame = false;
        if (parent == null)
        {
            parent = CreateMatchingResourceBar(parentNames[0], fallbackBarPosition, label);
            createdFrame = parent != null;
        }

        if (parent == null) return null;

        Slider existing = parent.GetComponentInChildren<Slider>(true);
        if (existing != null) return existing;

        Slider created = CreateSceneStyleSlider(parent, sliderName, fillColor);
        if (createdFrame)
            BindMissingSliderLabel(label, parent);
        return created;
    }

    static Transform FindHudBar(string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            GameObject go = GameObject.Find(names[i]);
            if (go != null) return go.transform;
        }

        return null;
    }

    Transform CreateMatchingResourceBar(string name, Vector2 anchoredPosition, string label)
    {
        GameObject canvasGo = GameObject.Find("Canvas");
        Transform canvas = canvasGo != null ? canvasGo.transform : FindFirstObjectByType<Canvas>()?.transform;
        if (canvas == null) return null;

        GameObject template = GameObject.Find("HealthBAR");
        GameObject bar = new GameObject(name);
        bar.layer = 5;
        bar.transform.SetParent(canvas, false);

        RectTransform rt = bar.AddComponent<RectTransform>();
        Image img = bar.AddComponent<Image>();
        img.raycastTarget = false;

        if (template != null)
        {
            RectTransform src = template.GetComponent<RectTransform>();
            rt.anchorMin = src.anchorMin;
            rt.anchorMax = src.anchorMax;
            rt.pivot = src.pivot;
            rt.sizeDelta = src.sizeDelta;
            rt.localScale = src.localScale;

            Image srcImg = template.GetComponent<Image>();
            if (srcImg != null)
            {
                img.sprite = srcImg.sprite;
                img.color = srcImg.color;
                img.type = srcImg.type;
                img.preserveAspect = srcImg.preserveAspect;
            }
        }
        else
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(100f, 100f);
            rt.localScale = new Vector3(2.68013f, 0.77431f, 1f);
            img.color = new Color(1f, 1f, 1f, 0.9f);
        }

        rt.anchoredPosition = anchoredPosition;

        GameObject labelGo = new GameObject(label + "Label");
        labelGo.layer = 5;
        labelGo.transform.SetParent(bar.transform, false);
        RectTransform labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(8f, 4f);
        labelRt.offsetMax = new Vector2(-8f, -4f);
        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        tmp.text = label;
        return rt;
    }

    Slider CreateSceneStyleSlider(Transform parent, string name, Color fillColor)
    {
        GameObject sliderGo = new GameObject(name);
        sliderGo.layer = 5;
        sliderGo.transform.SetParent(parent, false);

        RectTransform rt = sliderGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 40f);
        rt.sizeDelta = new Vector2(160f, 20f);
        rt.localScale = new Vector3(0.59001f, 5.3625f, 1f);

        Image bg = sliderGo.AddComponent<Image>();
        bg.sprite = GetUiSprite();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
        bg.raycastTarget = false;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.layer = 5;
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(2f, 2f);
        fillAreaRt.offsetMax = new Vector2(-2f, -2f);

        GameObject fill = new GameObject("Fill");
        fill.layer = 5;
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.sprite = GetUiSprite();
        fillImg.type = Image.Type.Sliced;
        fillImg.color = fillColor;
        fillImg.raycastTarget = false;

        Slider slider = sliderGo.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = null;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.wholeNumbers = false;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.targetGraphic = null;
        sliderGo.transform.SetAsLastSibling();
        return slider;
    }

    void BindMissingSliderLabel(string label, Transform bar)
    {
        TMP_Text tmp = bar != null ? bar.GetComponentInChildren<TMP_Text>(true) : null;
        if (tmp == null) return;

        if (playerWaterText == null && label == "Water")
            playerWaterText = tmp;
        else if (bucketWaterText == null && label == "Bucket")
            bucketWaterText = tmp;
    }

    static Sprite uiSprite;

    static Sprite GetUiSprite()
    {
        if (uiSprite != null) return uiSprite;

        uiSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (uiSprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            uiSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        return uiSprite;
    }
}
