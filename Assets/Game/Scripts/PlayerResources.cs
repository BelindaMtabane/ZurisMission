using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns player resource values (health, water, bucket, materials).
/// Gameplay systems change resources through the public methods on this component.
/// </summary>
public class PlayerResources : MonoBehaviour
{
    public static PlayerResources Instance { get; private set; }

    [Header("Maximums")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxPlayerWater = 100f;
    [SerializeField] private float maxBucketWater = 100f;
    [SerializeField] private int maxMaterials = 100;

    [Header("Scene Starting Values")]
    [SerializeField] private float mainGameStartingBucket = 15f;
    [SerializeField] private float level3StartingBucket = 25f;

    float health;
    float playerWater;
    float bucketWater;
    int materials;

    public float Health => health;
    public float PlayerWater => playerWater;
    public float BucketWater => bucketWater;
    public int Materials => materials;

    public float MaxHealth => maxHealth;
    public float MaxPlayerWater => maxPlayerWater;
    public float MaxBucketWater => maxBucketWater;
    public int MaxMaterials => maxMaterials;

    public float MainGameStartingBucket => mainGameStartingBucket;

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
        health = maxHealth;
        playerWater = maxPlayerWater;
        materials = 0;

        if (sceneName == SceneCatalog.MainGame)
        {
            bucketWater = mainGameStartingBucket;
        }
        else if (sceneName == SceneCatalog.Level3)
        {
            bucketWater = level3StartingBucket;
        }
        else
        {
            bucketWater = 0f;
        }

        NotifyChanged();
    }

    public void AddHealth(float amount)
    {
        if (amount <= 0f) return;
        health = Mathf.Min(maxHealth, health + amount);
        NotifyChanged();
    }

    public void RemoveHealth(float amount)
    {
        if (amount <= 0f) return;
        health = Mathf.Max(0f, health - amount);
        NotifyChanged();
    }

    public void AddPlayerWater(float amount)
    {
        if (amount <= 0f) return;
        playerWater = Mathf.Min(maxPlayerWater, playerWater + amount);
        NotifyChanged();
    }

    public void RemovePlayerWater(float amount)
    {
        if (amount <= 0f) return;
        playerWater = Mathf.Max(0f, playerWater - amount);
        NotifyChanged();
    }

    public void AddBucketWater(float amount)
    {
        if (amount <= 0f) return;
        bucketWater = Mathf.Min(maxBucketWater, bucketWater + amount);
        NotifyChanged();
    }

    public void RemoveBucketWater(float amount)
    {
        if (amount <= 0f) return;
        bucketWater = Mathf.Max(0f, bucketWater - amount);
        NotifyChanged();
    }

    public void AddMaterials(int amount)
    {
        if (amount <= 0) return;
        materials = Mathf.Clamp(materials + amount, 0, maxMaterials);
        NotifyChanged();
    }

    public void RemoveMaterials(int amount)
    {
        if (amount <= 0 || materials <= 0) return;
        materials = Mathf.Max(0, materials - amount);
        NotifyChanged();
    }

    public void ChangeHealth(float delta)
    {
        if (delta >= 0f) AddHealth(delta);
        else RemoveHealth(-delta);
    }

    public void ChangePlayerWater(float delta)
    {
        if (delta >= 0f) AddPlayerWater(delta);
        else RemovePlayerWater(-delta);
    }

    public void ChangeBucketWater(float delta)
    {
        if (delta >= 0f) AddBucketWater(delta);
        else RemoveBucketWater(-delta);
    }

    public void SetBucketWater(float value)
    {
        bucketWater = Mathf.Clamp(value, 0f, maxBucketWater);
        NotifyChanged();
    }

    public void SetPlayerWater(float value)
    {
        playerWater = Mathf.Clamp(value, 0f, maxPlayerWater);
        NotifyChanged();
    }

    public void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0f, maxHealth);
        NotifyChanged();
    }

    public void SetMaterials(int value)
    {
        materials = Mathf.Clamp(value, 0, maxMaterials);
        NotifyChanged();
    }

    public void RemoveMaterialPercent(float percent)
    {
        if (percent <= 0f || materials <= 0) return;
        int loss = Mathf.Max(1, Mathf.RoundToInt(maxMaterials * percent));
        materials = Mathf.Max(0, materials - loss);
        NotifyChanged();
    }

    public void RemoveBucketPercent(float percent)
    {
        if (percent <= 0f || bucketWater <= 0f) return;
        float loss = Mathf.Max(1f, maxBucketWater * percent);
        bucketWater = Mathf.Max(0f, bucketWater - loss);
        NotifyChanged();
    }

    public int GetMaterialLossForPercent(float percent)
    {
        if (percent <= 0f || materials <= 0) return 0;
        return Mathf.Max(1, Mathf.RoundToInt(maxMaterials * percent));
    }

    public float GetBucketLossForPercent(float percent)
    {
        if (percent <= 0f || bucketWater <= 0f) return 0f;
        return Mathf.Max(1f, maxBucketWater * percent);
    }

    void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}
