using UnityEngine;

/// <summary>
/// Ensures only a few Level 1 snakes charge at full speed at once.
/// Snakes still creep forward while waiting for a slot.
/// </summary>
public static class Level1SnakeDirector
{
    const int MaxEarlyActive = 1;
    const int MaxMidActive = 2;
    const float MidChallengeProgress = 0.45f;

    static readonly System.Collections.Generic.List<Level1Snake> activeSnakes =
        new System.Collections.Generic.List<Level1Snake>(2);

    public static bool TryStartSnake(Level1Snake snake)
    {
        if (snake == null) return false;

        CleanupInactive();
        if (activeSnakes.Contains(snake))
        {
            return true;
        }

        int limit = snake.SpawnProgress >= MidChallengeProgress ? MaxMidActive : MaxEarlyActive;
        if (activeSnakes.Count >= limit)
        {
            return false;
        }

        activeSnakes.Add(snake);
        return true;
    }

    public static void ReleaseSnake(Level1Snake snake)
    {
        if (snake == null) return;
        activeSnakes.Remove(snake);
    }

    static void CleanupInactive()
    {
        for (int i = activeSnakes.Count - 1; i >= 0; i--)
        {
            if (activeSnakes[i] == null)
            {
                activeSnakes.RemoveAt(i);
            }
        }
    }
}
