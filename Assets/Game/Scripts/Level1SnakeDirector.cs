using UnityEngine;

/// <summary>
/// Ensures only one Level 1 snake moves toward the player at a time.
/// </summary>
public static class Level1SnakeDirector
{
    static Level1Snake activeSnake;

    public static bool TryStartSnake(Level1Snake snake)
    {
        if (activeSnake != null && activeSnake != snake)
        {
            return false;
        }

        activeSnake = snake;
        return true;
    }

    public static void ReleaseSnake(Level1Snake snake)
    {
        if (activeSnake == snake)
        {
            activeSnake = null;
        }
    }
}
