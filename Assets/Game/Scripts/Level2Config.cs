using UnityEngine;

public static class Level2Config
{
    public const float TargetPlaytimeSeconds = 180f;

    public const float LogHealthDamage = 5f;
    public const int LogMaterialLoss = 5;
    public const int LogJumpMaterialLoss = 10;

    public const float CactusHealthDamage = 10f;
    public const float CactusWaterDamage = 5f;

    public const float WarthogHealthDamage = 10f;

    public const float MudBallHealthDamage = 5f;
    public const float MudBallSlowMultiplier = 0.45f;
    public const float MudBallSlowDuration = 2.4f;

    public const float VisibleSpawnDistance = 70f;
    public const int DefaultMaterialPickup = 15;
    public const float MinimumObjectSpacing = 12f;
    public const float MinimumHazardSpacing = 22f;

    public static readonly float[] HealthAmounts = { 12f, 15f, 18f, 20f };
}
