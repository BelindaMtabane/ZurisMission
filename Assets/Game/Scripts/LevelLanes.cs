using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLanes
{
    public const int Count = 4;
    public const float LaneSpacing = 4f;
    public const float Level2LaneSpacing = 5.5f;

    static readonly float[] MainGameXs = { -6f, -2f, 2f, 6f };
    static float[] activeXs = { -6f, -2f, 2f, 6f };

    public static float PathCenterX { get; private set; }

    public static float[] Xs => activeXs;

    public static float X(int laneIndex)
    {
        int i = Mathf.Clamp(laneIndex, 0, Count - 1);
        return activeXs[i];
    }

    public static int DisplayNumber(int laneIndex)
    {
        return Mathf.Clamp(laneIndex, 0, Count - 1) + 1;
    }

    public static void ConfigureForActiveScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Level2" || scene == "Level3")
        {
            ConfigureCentered(FindGroundCenterX(), Level2LaneSpacing);
            return;
        }

        ResetToMainGame();
    }

    public static void ConfigureCentered(float pathCenterX, float spacing)
    {
        PathCenterX = pathCenterX;
        activeXs = new float[Count];
        float halfSpan = (Count - 1) * 0.5f * spacing;
        for (int i = 0; i < Count; i++)
        {
            activeXs[i] = pathCenterX - halfSpan + i * spacing;
        }
    }

    public static void ResetToMainGame()
    {
        PathCenterX = 0f;
        activeXs = (float[])MainGameXs.Clone();
    }

    public static float FindGroundCenterX()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            return ground.transform.position.x;
        }

        GameObject tagged = GameObject.FindGameObjectWithTag("Ground");
        if (tagged != null)
        {
            return tagged.transform.position.x;
        }

        return 0f;
    }
}
