using System.Collections.Generic;
using UnityEngine;

public enum YoyoTankColor
{
    Green,
    Blue,
    Red
}

/// <summary>
/// Shared yoyo wrap materials plus a cylinder mesh whose UVs keep the wrap image
/// at its true aspect, so the black lettering reads cleanly on the tank's local -Z side.
/// </summary>
public static class YoyoTankWrap
{
    // Image widths wrapped once around the tank; below 1 enlarges the lettering.
    const float ImageWidthsAroundTank = 0.85f;
    // Vertical position of the "yoyo" lettering inside the source image (0 = bottom, 1 = top).
    const float LetteringV = 0.745f;
    // Height on the tank (0 = bottom, 1 = top) where the lettering is centred.
    const float LetteringHeight = 0.6f;
    const int Segments = 48;

    static readonly Dictionary<YoyoTankColor, Material> Materials = new();
    static readonly Dictionary<YoyoTankColor, Texture2D> Textures = new();
    static readonly Dictionary<int, Mesh> Meshes = new();

    public static Material Get(YoyoTankColor color)
    {
        if (Materials.TryGetValue(color, out Material cached) && cached != null) return cached;
        Material mat = Make($"YoyoTankWrap_{color}", LoadTexture(color));
        Materials[color] = mat;
        return mat;
    }

    /// <summary>Applies the wrap to a Unity cylinder primitive, keeping its current transform scale.</summary>
    public static void Apply(Renderer rend, YoyoTankColor color)
    {
        Material mat = Get(color);
        if (rend == null || mat == null) return;

        MeshFilter filter = rend.GetComponent<MeshFilter>();
        if (filter != null)
        {
            Vector3 scale = rend.transform.lossyScale;
            Texture tex = mat.mainTexture;
            float aspect = tex != null && tex.height > 0 ? (float)tex.width / tex.height : 16f / 9f;
            filter.sharedMesh = GetMesh(Mathf.Abs(scale.x), Mathf.Abs(scale.y) * 2f, aspect);
        }

        rend.sharedMaterial = mat;
    }

    static Texture2D LoadTexture(YoyoTankColor color)
    {
        if (Textures.TryGetValue(color, out Texture2D cached) && cached != null) return cached;

        string resource;
        string editorPath;
        switch (color)
        {
            case YoyoTankColor.Green:
                resource = "Village/yoyo_tank";
                editorPath = "Assets/Game/Resources/Village/yoyo_tank.png";
                break;
            case YoyoTankColor.Red:
                resource = "Village/yoyo_tank_red";
                editorPath = "Assets/Game/Resources/Village/yoyo_tank_red.jpg";
                break;
            default:
                resource = "Village/yoyo_tank_blue";
                editorPath = "Assets/Game/Resources/Village/yoyo_tank_blue.jpg";
                break;
        }

        Texture2D tex = Resources.Load<Texture2D>(resource);
#if UNITY_EDITOR
        if (tex == null) tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath);
#endif
        if (tex != null)
        {
            tex.wrapModeU = TextureWrapMode.Repeat;
            tex.wrapModeV = TextureWrapMode.Clamp;
        }

        Textures[color] = tex;
        return tex;
    }

    static Material Make(string name, Texture2D tex)
    {
        if (tex == null) return null;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) return null;

        Material mat = new Material(shader) { name = name };
        mat.mainTexture = tex;
        mat.mainTextureScale = Vector2.one;
        mat.mainTextureOffset = Vector2.zero;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
        if (mat.HasProperty("_BaseMap_ST")) mat.SetVector("_BaseMap_ST", new Vector4(1f, 1f, 0f, 0f));
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
        return mat;
    }

    /// <summary>
    /// Unit cylinder matching Unity's primitive (radius 0.5, y -1..1) with UVs sized for the world scale.
    /// </summary>
    static Mesh GetMesh(float worldDiameter, float worldHeight, float imageAspect)
    {
        worldDiameter = Mathf.Max(0.01f, worldDiameter);
        worldHeight = Mathf.Max(0.01f, worldHeight);
        int key = Mathf.RoundToInt(worldDiameter * 100f) * 100003
                  + Mathf.RoundToInt(worldHeight * 100f) * 101
                  + Mathf.RoundToInt(imageAspect * 100f);
        if (Meshes.TryGetValue(key, out Mesh cached) && cached != null) return cached;

        float circumference = Mathf.PI * worldDiameter;
        float imageWorldWidth = circumference / ImageWidthsAroundTank;
        float imageWorldHeight = imageWorldWidth / Mathf.Max(0.1f, imageAspect);

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        // Side: angle 0 faces local -Z; increasing angle moves toward +X so text reads left to right.
        for (int ring = 0; ring < 2; ring++)
        {
            float y = ring == 0 ? -1f : 1f;
            float worldY = ring == 0 ? 0f : worldHeight;
            float v = LetteringV + (worldY - LetteringHeight * worldHeight) / imageWorldHeight;
            for (int i = 0; i <= Segments; i++)
            {
                float a = Mathf.Lerp(-Mathf.PI, Mathf.PI, (float)i / Segments);
                Vector3 dir = new Vector3(Mathf.Sin(a), 0f, -Mathf.Cos(a));
                vertices.Add(dir * 0.5f + Vector3.up * y);
                normals.Add(dir);
                uvs.Add(new Vector2(0.5f + a / (2f * Mathf.PI) * ImageWidthsAroundTank, v));
            }
        }

        int row = Segments + 1;
        for (int i = 0; i < Segments; i++)
        {
            int b0 = i;
            int b1 = i + 1;
            int t0 = row + i;
            int t1 = row + i + 1;
            triangles.Add(t0); triangles.Add(t1); triangles.Add(b1);
            triangles.Add(t0); triangles.Add(b1); triangles.Add(b0);
        }

        // Caps sample a plain strip near the image's bottom edge.
        AddCap(vertices, normals, uvs, triangles, 1f, true);
        AddCap(vertices, normals, uvs, triangles, -1f, false);

        var mesh = new Mesh { name = "YoyoTankCylinder" };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        Meshes[key] = mesh;
        return mesh;
    }

    static void AddCap(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, float y, bool top)
    {
        Vector3 normal = top ? Vector3.up : Vector3.down;
        Vector2 plainUv = new Vector2(0.1f, 0.03f);
        int center = vertices.Count;
        vertices.Add(new Vector3(0f, y, 0f));
        normals.Add(normal);
        uvs.Add(plainUv);

        for (int i = 0; i <= Segments; i++)
        {
            float a = Mathf.Lerp(-Mathf.PI, Mathf.PI, (float)i / Segments);
            vertices.Add(new Vector3(Mathf.Sin(a) * 0.5f, y, -Mathf.Cos(a) * 0.5f));
            normals.Add(normal);
            uvs.Add(plainUv);
        }

        for (int i = 0; i < Segments; i++)
        {
            int r0 = center + 1 + i;
            int r1 = center + 2 + i;
            if (top)
            {
                triangles.Add(center); triangles.Add(r1); triangles.Add(r0);
            }
            else
            {
                triangles.Add(center); triangles.Add(r0); triangles.Add(r1);
            }
        }
    }
}
