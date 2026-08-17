using UnityEngine;

public static class Level3Ground
{
    public const float SurfaceY = 0.55f;

    public static Vector3 LanePosition(int lane, float z, float lift = 0f)
    {
        return new Vector3(LevelLanes.X(lane), SurfaceY + lift, z);
    }
}
