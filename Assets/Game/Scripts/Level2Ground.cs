using UnityEngine;

public static class Level2Ground
{
    public const float SurfaceY = 0.55f;

    // Faint light green with warm brown mud undertone — no texture.
    public static readonly Color MudGreen = new Color(0.74f, 0.80f, 0.56f);
    public static readonly Color MudBrownAccent = new Color(0.72f, 0.68f, 0.46f);

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

        float stripe = Mathf.Repeat(tile.transform.position.z * 0.035f, 1f);
        Color tint = Color.Lerp(MudGreen, MudBrownAccent, stripe * 0.45f + 0.18f);

        Material instance = new Material(source);
        ClearTextures(instance);

        if (instance.HasProperty("_BaseColor"))
        {
            instance.SetColor("_BaseColor", tint);
        }

        if (instance.HasProperty("_Color"))
        {
            instance.SetColor("_Color", tint);
        }

        instance.color = tint;
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
