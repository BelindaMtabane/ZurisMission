using UnityEngine;

/// <summary>
/// Central temporary slow when the player runs through a Level 2 mud puddle.
/// Does not stack infinitely — a new puddle refreshes duration.
/// </summary>
public static class Level2MudSlowEffect
{
    public const float DefaultMultiplier = 0.65f;
    public const float DefaultDuration = 2f;

    public static float Multiplier { get; set; } = DefaultMultiplier;
    public static float Duration { get; set; } = DefaultDuration;

    public static void Apply(PlayerController controller, float multiplier = -1f, float duration = -1f)
    {
        if (controller == null) return;

        float mul = multiplier > 0f ? multiplier : Multiplier;
        float seconds = duration > 0f ? duration : Duration;
        controller.ApplyMudSlow(mul, seconds);
    }
}
