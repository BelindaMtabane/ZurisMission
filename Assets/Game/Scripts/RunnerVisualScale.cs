using UnityEngine;

/// <summary>
/// Scales runner pickups and hazards to child-friendly proportions (~78% of adult-sized props).
/// </summary>
public static class RunnerVisualScale
{
    public const float Factor = 0.78f;
    public const float Level3Boost = 1.42f;
    public const float PlantBoost = 2.45f;
    public const float TreeBoost = 3.05f;

    public static float F(float value) => value * Factor;

    public static Vector3 V(Vector3 value) => value * Factor;

    public static float L3(float value) => value * Factor * Level3Boost;

    public static Vector3 L3V(Vector3 value) => value * Factor * Level3Boost;

    public static float PlantF(float value) => value * Factor * PlantBoost;

    public static Vector3 PlantV(Vector3 value) => value * Factor * PlantBoost;
}
