using UnityEngine;

public static class Level3Progress
{
    public static float StartZ = 0f;
    public static float EndZ = 7182f;

    public static float Length => Mathf.Max(1f, EndZ - StartZ);

    public static float ContentStartZ => StartZ + Level3Config.InitialSpawnDistance;

    public static float Normalized(float playerZ)
    {
        return Mathf.Clamp01((playerZ - StartZ) / Length);
    }

    public static float WorldZ(float progress01)
    {
        float origin = ContentStartZ;
        float span = Mathf.Max(1f, EndZ - origin);
        return origin + Mathf.Clamp01(progress01) * span;
    }

    public static void BindFromScene(Transform player)
    {
        if (player != null) StartZ = player.position.z;

        GameObject ender = GameObject.Find("Ender3");
        if (ender != null) EndZ = ender.transform.position.z;
    }
}
