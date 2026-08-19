using UnityEngine;

public static class Level1Ground
{
    // Unity ground cube center y=0, scale y=1 => top surface at 0.5
    public const float SurfaceY = 0.55f;
    public static readonly Color LightBrown = new Color(0.86f, 0.74f, 0.54f);

    static Material cachedSource;

    public static Vector3 LanePosition(int lane, float z, float lift = 0f)
    {
        return new Vector3(LevelLanes.X(lane), SurfaceY + lift, z);
    }

    public static void ApplyGroundSurface(GameObject tile, Material materialSource, Vector3 tileScale)
    {
        if (tile == null) return;

        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer == null) return;

        Material source = materialSource != null ? materialSource : cachedSource;
        if (source == null)
        {
            return;
        }

        cachedSource = source;

        Material instance = new Material(source);
        if (instance.HasProperty("_BaseMap"))
        {
            instance.SetTexture("_BaseMap", null);
        }

        if (instance.HasProperty("_MainTex"))
        {
            instance.SetTexture("_MainTex", null);
        }

        instance.mainTexture = null;

        if (instance.HasProperty("_BaseColor"))
        {
            instance.SetColor("_BaseColor", LightBrown);
        }

        if (instance.HasProperty("_Color"))
        {
            instance.SetColor("_Color", LightBrown);
        }

        instance.color = LightBrown;
        renderer.sharedMaterial = instance;
    }
}
