using UnityEngine;

/// <summary>
/// Tunable Level 1 pacing, water, and phase boundaries for MainGame.
/// </summary>
public static class Level1Config
{
    public const float VillageMaxPercent = 35f;

    // Phase progress (0–1 along the run)
    public const float Phase1End = 0.25f;
    public const float Phase2End = 0.50f;
    public const float Phase3End = 0.75f;

    public const float HeatWaveStartProgress = 0.25f;
    public const float SnakeIntroProgress = 0.28f;
    public const float RollingLogIntroProgress = 0.52f;

    // Cactus water (primary supply)
    public const float CactusPlayerWater = 18f;
    public const float CactusBucketWater = 10f;
    public const float CactusCollectInterval = 1.0f;

    // Water springs (secondary, stronger refill while standing)
    public const float SpringPlayerWater = 20f;
    public const float SpringBucketWater = 12f;
    public const float SpringCollectInterval = 0.9f;

    // Heat wave
    public const float HeatWaterLossPerSecond = 20f;
    public const float HeatBurstCooldown = 7f;

    // Layout safety: minimum progress gap when reusing the same lane (~22m on a 1015m run)
    public const float MinSameLaneProgressGap = 0.022f;

    public static bool IsPhase1(float progress) => progress < Phase1End;
    public static bool IsPhase2(float progress) => progress >= Phase1End && progress < Phase2End;
    public static bool IsPhase3(float progress) => progress >= Phase2End && progress < Phase3End;
    public static bool IsPhase4(float progress) => progress >= Phase3End;
}
