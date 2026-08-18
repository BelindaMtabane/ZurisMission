using UnityEngine;

public static class Level3Config
{
    // Level 3 pacing (time limit)
    public const float Level3TimeLimitSeconds = 180f;

    // Tank repair: required successful hits vs total pipe opportunities
    public const int Tank1Required = 5;
    public const int Tank1Opportunities = 8;
    public const int Tank2Required = 10;
    public const int Tank2Opportunities = 15;
    public const int Tank3Required = 2;
    public const int Tank3Opportunities = 5;

    public const int Tank1RepairMaterialCost = 4;
    public const int Tank2RepairMaterialCost = 5;
    public const int Tank3RepairMaterialCost = 6;

    // Lightning
    public const float LightningWarningSeconds = 2f;
    public const float LightningHealthDamage = 5f;
    public const float LightningMaterialLossPercent = 0.05f;

    // Enemies
    public const float SnakeHealthDamage = 3f;
    public const float WarthogHealthDamage = 5f;
    public const float SnakeApproachSpeed = 20f;
    public const float TreeBucketLossPercent = 0.05f;
    // Acid rain now deals flat damage on contact (see Level3AcidRainZone)
    public const float AcidHealthDamage = 10f;
    public const float AcidMaterialLossPercent = 0.05f;
    public const float RollingLogHealthDamage = 6f;

    // Acid rain warning cadence (like lightning)
    public const float AcidWarningSeconds = 2f;

    // Speed fruit (temporary partial run boost — not a full max-speed sprint)
    public const float SpeedFruitBoostSpeed = 35f;
    public const float SpeedFruitDurationSeconds = 6f;
    public const float SpeedFruitSpawnChance = 0.12f;

    // Mud — flat material deduction
    public const float MudMaterialLossPercent = 0.05f;   // kept for legacy paths
    public const int   MudMaterialLoss        = 5;

    // Tree — flat material deduction
    public const int   TreeMaterialLoss       = 10;

    // Rolling logs also damage materials (no health damage in Level 3)
    public const int   LogMaterialLoss        = 10;

    // Pickups (defaults — layout may override per pickup)
    public static readonly float[] DropletAmounts = { 10f, 15f, 20f };
    public static readonly float[] HealthAmounts = { 10f, 15f, 20f };
    public const int DefaultMaterialPickup = 10;

    // Spawn distances (world units). Layout director copies Inspector values here on Awake.
    public static float InitialSpawnDistance = 16f;
    public static float InitialSpawnBuffer = 8f;
    public static int InitialSpawnCount = 8;
    public static float VisibleSpawnDistance = 80f;
    public static float MinimumObjectSpacing = 14f;
    public static float MinimumHazardSpacing = 22f;
    public static float MinimumPipeSpacing = 80f;
    public static bool EnableSpawnDebug;

    public static int ProgressPerRepair(int tankIndex)
    {
        int required = tankIndex == 0 ? Tank1Required : tankIndex == 1 ? Tank2Required : Tank3Required;
        return Mathf.CeilToInt(100f / required);
    }

    public static int RepairMaterialCost(int tankIndex)
    {
        if (tankIndex == 1) return Tank2RepairMaterialCost;
        if (tankIndex == 2) return Tank3RepairMaterialCost;
        return Tank1RepairMaterialCost;
    }
}
