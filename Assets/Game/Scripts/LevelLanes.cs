using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLanes
{
    public const int Count = 4;
    public const float LaneSpacing = 5.5f;
    public const float Level2LaneSpacing = 6.5f;
    public const float Level3LaneSpacing = 5.5f;
    /// <summary>Half-width from lane center to lane edge (visual / clearance).</summary>
    public const float LaneHalfWidth = 2.75f;
    /// <summary>Gap between outer lane edge and roadside scenery inner band.</summary>
    public const float SceneryMarginBeyondLane = 8.25f;
    /// <summary>Extra inset for wide green props (grass, bushes, trees) so canopies stay off lanes.</summary>
    public const float GreenFoliageExtraClearance = 6.75f;
    /// <summary>Green props only use the outer portion of the foliage band (keeps lane shoulders open).</summary>
    public const float GreenBandInnerBias = 0.68f;
    /// <summary>|X| inside this is kept clear of rocks/boulders (includes mesh overhang).</summary>
    public const float PathRockClearHalfWidth = 7.25f;

    static readonly float[] MainGameXs = { -8.25f, -2.75f, 2.75f, 8.25f };
    static float[] activeXs = { -8.25f, -2.75f, 2.75f, 8.25f };

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
        if (SceneCatalog.IsRunnerScene(scene))
        {
            float spacing = LaneSpacing;
            if (SceneCatalog.IsLevel2(scene)) spacing = Level2LaneSpacing;
            else if (SceneCatalog.IsLevel3(scene)) spacing = Level3LaneSpacing;
            ConfigureCentered(FindGroundCenterX(), spacing);
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

    public static float OutermostLaneExtent()
    {
        ConfigureForActiveScene();
        float centerEdge = Mathf.Max(Mathf.Abs(X(0)), Mathf.Abs(X(Count - 1)));
        return centerEdge + LaneHalfWidth;
    }

    public static void GetRoadsideSceneryBand(out float inner, out float outer)
    {
        float innerEdge = OutermostLaneExtent() + SceneryMarginBeyondLane;
        inner = innerEdge;
        outer = innerEdge + 10.5f;
    }

    public static float PathRockClearAbsX()
    {
        return OutermostLaneExtent() + PathRockClearHalfWidth;
    }

    public static float MinAbsXScenery()
    {
        return OutermostLaneExtent() + SceneryMarginBeyondLane;
    }

    public static float MinAbsXGreenFoliage()
    {
        return OutermostLaneExtent() + SceneryMarginBeyondLane + GreenFoliageExtraClearance;
    }

    /// <summary>Band for grass, bushes, and trees — inset from the inner scenery edge.</summary>
    public static void GetGreenFoliageBand(out float inner, out float outer)
    {
        GetRoadsideSceneryBand(out inner, out outer);
        inner = Mathf.Max(inner + 4.25f, MinAbsXGreenFoliage());
    }

    /// <summary>Desert village houses beside the run — outside lanes, behind the grey boulder strip.</summary>
    public static void GetVillageHouseBand(out float inner, out float outer)
    {
        GetRoadsideSceneryBand(out float sceneryInner, out float sceneryOuter);
        GetBoulderWallBand(out _, out float boulderOuter);
        inner = boulderOuter + 3.35f;
        outer = boulderOuter + 9.25f;
    }

    /// <summary>Far shoulder strip for grey Rock_A walls only.</summary>
    public static void GetBoulderWallBand(out float inner, out float outer)
    {
        GetRoadsideSceneryBand(out float sceneryInner, out float sceneryOuter);
        inner = sceneryInner + 6.75f;
        outer = sceneryOuter - 0.35f;
        inner = Mathf.Max(inner, OutermostLaneExtent() + 12.5f);
        if (outer <= inner + 1.5f)
            outer = inner + 2.5f;
    }

    /// <summary>Minimum |X| for tall grey boulders so meshes do not hang over lane pickups.</summary>
    public static float MinAbsXBoulderWall()
    {
        GetBoulderWallBand(out float inner, out _);
        return inner;
    }

    public static float EnforceRoadsideX(float x, bool greenFoliage)
    {
        float min = greenFoliage ? MinAbsXGreenFoliage() : MinAbsXScenery();
        return x >= 0f ? Mathf.Max(x, min) : -Mathf.Max(Mathf.Abs(x), min);
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
