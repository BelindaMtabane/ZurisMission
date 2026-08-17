using UnityEngine;

public static class LevelLanes
{
    public const int Count = 4;
    public static readonly float[] Xs = { -6f, -2f, 2f, 6f };

    public static float X(int laneIndex)
    {
        int i = Mathf.Clamp(laneIndex, 0, Count - 1);
        return Xs[i];
    }

    public static int DisplayNumber(int laneIndex)
    {
        return Mathf.Clamp(laneIndex, 0, Count - 1) + 1;
    }
}
