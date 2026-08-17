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
        float tileU = Mathf.Max(2f, tileScale.x * 0.12f);
        float tileV = Mathf.Max(2f, tileScale.z * 0.04f);
        Vector2 tiling = new Vector2(tileU, tileV);

        Texture baseTex = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.mainTexture;
        if (baseTex == null && source.HasProperty("_MainTex"))
        {
            baseTex = source.GetTexture("_MainTex");
        }

        if (instance.HasProperty("_BaseMap") && baseTex != null)
        {
            instance.SetTexture("_BaseMap", baseTex);
            instance.SetTextureScale("_BaseMap", tiling);
        }

        if (instance.HasProperty("_MainTex") && baseTex != null)
        {
            instance.SetTexture("_MainTex", baseTex);
            instance.SetTextureScale("_MainTex", tiling);
        }

        instance.mainTexture = baseTex;
        instance.mainTextureScale = tiling;

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
