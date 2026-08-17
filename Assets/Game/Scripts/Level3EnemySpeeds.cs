using UnityEngine;

public static class Level3EnemySpeeds
{
    public static float SnakeSlow = 4f;
    public static float SnakeMedium = 6f;
    public static float SnakeFast = 8f;
    public static float WarthogSlow = 5f;
    public static float WarthogMedium = 7f;
    public static float WarthogFast = 9f;

    public static float Snake(Level3EnemyPace pace)
    {
        switch (pace)
        {
            case Level3EnemyPace.Fast: return SnakeFast;
            case Level3EnemyPace.Medium: return SnakeMedium;
            default: return SnakeSlow;
        }
    }

    public static float Warthog(Level3EnemyPace pace)
    {
        switch (pace)
        {
            case Level3EnemyPace.Fast: return WarthogFast;
            case Level3EnemyPace.Medium: return WarthogMedium;
            default: return WarthogSlow;
        }
    }
}

public enum Level3EnemyPace
{
    Slow,
    Medium,
    Fast
}

public static class Level3MudSlowEffect
{
    public const float DefaultMultiplier = 0.6f;
    public const float DefaultDuration = 2f;

    public static float Multiplier { get; set; } = DefaultMultiplier;
    public static float Duration { get; set; } = DefaultDuration;

    public static void Apply(PlayerController controller)
    {
        if (controller == null) return;
        controller.ApplyMudSlow(Multiplier, Duration);
        Level3FeedbackUI.Show("SLOWED BY MUD!", new Color(0.62f, 0.42f, 0.18f), 1.3f);
    }
}
