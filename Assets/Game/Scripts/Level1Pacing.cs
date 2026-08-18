using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tunes MainGame forward speed so Level 1 playtime is roughly 3:30.
/// </summary>
public static class Level1Pacing
{
    public const float TargetPlaytimeSeconds = 210f;
    public const float MainGameForwardSpeed = 20f;
    public const float MinSpeed = 18f;
    public const float MaxSpeed = 24f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.MainGame) return;
        Apply();
    }

    public static void Apply()
    {
        Transform player = null;
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;

        Level1Progress.BindFromScene(player);

        float pacedSpeed = Level1Progress.Length / TargetPlaytimeSeconds;
        float targetSpeed = Mathf.Clamp(Mathf.Max(pacedSpeed, MainGameForwardSpeed), MinSpeed, MaxSpeed);

        if (pc != null)
        {
            pc.ConfigureForwardSpeed(targetSpeed);
            Debug.Log($"[Level1] Forward speed={targetSpeed:F1} length={Level1Progress.Length:F0}");
        }
    }
}
