using UnityEngine;

public static class Level3Ground
{
    public const float SurfaceY = 0.55f;
    public const float TileScaleX = 50.837f;
    public const float TileScaleY = 1f;
    public const float TileScaleZ = 200f;

    // Medium dark green — flat colour, no texture.
    public static readonly Color MediumDarkGreen = new Color(0.18f, 0.36f, 0.20f);

    static Material cachedSource;

    public static Vector3 LanePosition(int lane, float z, float lift = 0f)
    {
        return new Vector3(LevelLanes.X(lane), SurfaceY + lift, z);
    }

    public static float TileLength => TileScaleZ;

    public static void ConfigureStreamingTile(ref float scaleX, ref float scaleY, ref float scaleZ, ref float spawnX, ref float spawnY)
    {
        scaleX = TileScaleX;
        scaleY = TileScaleY;
        scaleZ = TileScaleZ;
        spawnX = LevelLanes.FindGroundCenterX();
        spawnY = 0f;
    }

    public static void AlignSeedTile(GameObject seed, float spawnX, float spawnY)
    {
        if (seed == null) return;

        float half = TileScaleZ * 0.5f;
        seed.transform.position = new Vector3(spawnX, spawnY, half);
        seed.transform.localScale = new Vector3(TileScaleX, TileScaleY, TileScaleZ);
    }

    public static void ApplyGroundSurface(GameObject tile, Material materialSource, Vector3 tileScale)
    {
        if (tile == null) return;

        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer == null) return;

        Material source = materialSource != null ? materialSource : cachedSource;
        if (source == null) return;

        cachedSource = source;

        Material instance = new Material(source);
        ClearTextures(instance);

        if (instance.HasProperty("_BaseColor"))
        {
            instance.SetColor("_BaseColor", MediumDarkGreen);
        }

        if (instance.HasProperty("_Color"))
        {
            instance.SetColor("_Color", MediumDarkGreen);
        }

        instance.color = MediumDarkGreen;
        renderer.sharedMaterial = instance;
    }

    static void ClearTextures(Material instance)
    {
        if (instance.HasProperty("_BaseMap"))
        {
            instance.SetTexture("_BaseMap", null);
        }

        if (instance.HasProperty("_MainTex"))
        {
            instance.SetTexture("_MainTex", null);
        }

        instance.mainTexture = null;
    }
}
