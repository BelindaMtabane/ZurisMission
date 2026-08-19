using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies MainGame-style forward pacing to runner scenes (MainGame, Level2, Level3).
/// </summary>
public static class RunnerLevelPacing
{
    public const float TargetPlaytimeSeconds = Level1Pacing.TargetPlaytimeSeconds;
    public const float Level2TargetPlaytimeSeconds = Level2Config.TargetPlaytimeSeconds;
    public const float Level3TargetPlaytimeSeconds = 180f;
    public const float RunnerForwardSpeed = Level1Pacing.MainGameForwardSpeed;
    public const float MinSpeed = Level1Pacing.MinSpeed;
    public const float MaxSpeed = Level1Pacing.MaxSpeed;

    static readonly Dictionary<string, string> EndMarkerNames = new Dictionary<string, string>
    {
        { SceneCatalog.MainGame, "Ender1" },
        { SceneCatalog.Level2, "Ender2" },
        { SceneCatalog.Level3, "Ender3" },
    };

    public static bool SupportsScene(string sceneName)
    {
        return EndMarkerNames.ContainsKey(sceneName);
    }

    public static void Apply(string sceneName)
    {
        if (sceneName == SceneCatalog.MainGame)
        {
            Level1Pacing.Apply();
            return;
        }

        if (!EndMarkerNames.TryGetValue(sceneName, out string endName))
        {
            return;
        }

        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc == null)
        {
            return;
        }

        Transform player = pc.transform;
        float startZ = player.position.z;

        GameObject ender = GameObject.Find(endName);
        float endZ = ender != null ? ender.transform.position.z : startZ + 1000f;
        float length = Mathf.Max(1f, endZ - startZ);

        float playtimeTarget = TargetPlaytimeSeconds;
        if (sceneName == SceneCatalog.Level2) playtimeTarget = Level2TargetPlaytimeSeconds;
        else if (sceneName == SceneCatalog.Level3) playtimeTarget = Level3TargetPlaytimeSeconds;
        float pacedSpeed = length / playtimeTarget;
        float targetSpeed = Mathf.Clamp(Mathf.Max(pacedSpeed, RunnerForwardSpeed), MinSpeed, MaxSpeed);
        pc.ConfigureForwardSpeed(targetSpeed);

        Debug.Log($"[RunnerPacing] scene={sceneName} speed={targetSpeed:F1} length={length:F0}");
    }
}
