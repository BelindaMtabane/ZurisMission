using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Calculates and represents village restoration progress for each level.
/// </summary>
public class VillageProgressService : MonoBehaviour
{
    public static VillageProgressService Instance { get; private set; }

    public const float Level1MaxPercent = 35f;
    public const float Level2StartPercent = 35f;
    public const float Level2EndPercent = 65f;
    public const float Level3StartPercent = 65f;
    public const float Level3GainPercent = 35f;

    [SerializeField] private float level1MaxPercent = Level1MaxPercent;

    float currentPercent;

    public float CurrentPercent => currentPercent;

    public event Action OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        InitializeForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void InitializeForScene(string sceneName)
    {
        if (sceneName == SceneCatalog.Level3)
        {
            currentPercent = Level3StartPercent;
        }
        else if (sceneName == SceneCatalog.Level2)
        {
            currentPercent = Level2StartPercent;
        }
        else
        {
            currentPercent = 0f;
        }

        NotifyChanged();
    }

    public static float CalculateLevel1(PlayerResources resources, float maxPercent = Level1MaxPercent)
    {
        if (resources == null) return 0f;

        float materialNorm = Mathf.Clamp01(resources.Materials / (float)Mathf.Max(1, resources.MaxMaterials));
        float bucketRange = Mathf.Max(1f, resources.MaxBucketWater - resources.MainGameStartingBucket);
        float bucketNorm = Mathf.Clamp01((resources.BucketWater - resources.MainGameStartingBucket) / bucketRange);
        float progress = ((materialNorm + bucketNorm) * 0.5f) * maxPercent;
        return Mathf.Clamp(progress, 0f, maxPercent);
    }

    public static float CalculateLevel2(PlayerResources resources)
    {
        if (resources == null) return Level2StartPercent;

        float materialNorm = Mathf.Clamp01(resources.Materials / (float)Mathf.Max(1, resources.MaxMaterials));
        float bucketNorm = Mathf.Clamp01(resources.BucketWater / Mathf.Max(1f, resources.MaxBucketWater));
        float objectiveT = (materialNorm + bucketNorm) * 0.5f;
        return Mathf.Lerp(Level2StartPercent, Level2EndPercent, objectiveT);
    }

    public static float CalculateFromTanks(int tank1, int tank2, int tank3)
    {
        float tankAverage = (tank1 + tank2 + tank3) / 3f;
        return Mathf.Clamp(
            Level3StartPercent + tankAverage * (Level3GainPercent / 100f),
            Level3StartPercent,
            100f);
    }

    public void RecalculateFromResources(PlayerResources resources)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == SceneCatalog.MainGame)
        {
            currentPercent = CalculateLevel1(resources, level1MaxPercent);
        }
        else if (sceneName == SceneCatalog.Level2)
        {
            currentPercent = CalculateLevel2(resources);
        }

        NotifyChanged();
    }

    public void ApplyTankProgress(int tank1, int tank2, int tank3)
    {
        if (!SceneCatalog.IsLevel3(SceneManager.GetActiveScene().name)) return;

        currentPercent = CalculateFromTanks(tank1, tank2, tank3);
        NotifyChanged();
    }

    public void SetProgress(float percent)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (SceneCatalog.UsesObjectiveVillageProgress(sceneName))
        {
            return;
        }

        currentPercent = Mathf.Clamp(percent, 0f, 100f);
        NotifyChanged();
    }

    public void SetOrRecalculate(PlayerResources resources, float explicitPercent)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (SceneCatalog.UsesObjectiveVillageProgress(sceneName))
        {
            RecalculateFromResources(resources);
        }
        else
        {
            SetProgress(explicitPercent);
        }
    }

    void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}
