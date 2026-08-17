using UnityEngine;

public static class Level2Progress
{
    public static float StartZ = 1.04f;
    public static float EndZ = 1415f;

    public static float Length => Mathf.Max(1f, EndZ - StartZ);

    public static float Normalized(float playerZ)
    {
        return Mathf.Clamp01((playerZ - StartZ) / Length);
    }

    public static float WorldZ(float progress01)
    {
        return StartZ + Mathf.Clamp01(progress01) * Length;
    }

    public static void BindFromScene(Transform player)
    {
        if (player != null)
        {
            StartZ = player.position.z;
        }

        GameObject ender = GameObject.Find("Ender2");
        if (ender != null)
        {
            EndZ = ender.transform.position.z;
        }
    }
}
