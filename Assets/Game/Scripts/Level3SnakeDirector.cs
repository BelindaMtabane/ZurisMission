using UnityEngine;

public static class Level3SnakeDirector
{
    const int MaxConcurrentSnakes = 8;

    static readonly Level3Snake[] active = new Level3Snake[MaxConcurrentSnakes];

    public static bool TryStartSnake(Level3Snake snake)
    {
        for (int i = 0; i < active.Length; i++)
        {
            if (active[i] == snake) return true;
        }

        for (int i = 0; i < active.Length; i++)
        {
            if (active[i] == null)
            {
                active[i] = snake;
                return true;
            }
        }

        return false;
    }

    public static void ReleaseSnake(Level3Snake snake)
    {
        for (int i = 0; i < active.Length; i++)
        {
            if (active[i] == snake) active[i] = null;
        }
    }
}
