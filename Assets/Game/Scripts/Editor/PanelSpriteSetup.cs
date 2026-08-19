using UnityEditor;
using UnityEngine;

/// <summary>
/// One-off: gives PANEL1 (the wood-framed parchment panel) a 9-slice border
/// so UI code can set Image.type = Sliced and stretch it to any panel size
/// without distorting the wood frame or corner ornaments.
/// </summary>
public static class PanelSpriteSetup
{
    const string Panel1Path = "Assets/Game/Resources/UI/PANEL1.png";

    [MenuItem("Tools/Setup Panel1 9-Slice Border")]
    public static void SetupBorder()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(Panel1Path);
        if (importer == null) { Debug.LogError("[PanelSpriteSetup] PANEL1 importer not found"); return; }

#pragma warning disable CS0618 // spritesheet is deprecated but still the simplest reliable API here
        SpriteMetaData[] metas = importer.spritesheet;
        for (int i = 0; i < metas.Length; i++)
        {
            metas[i].border = new Vector4(30, 30, 30, 30);
        }
        importer.spritesheet = metas;
#pragma warning restore CS0618

        importer.SaveAndReimport();
        Debug.Log($"[PanelSpriteSetup] PANEL1 border set to (30,30,30,30) on {metas.Length} sprite(s).");
    }

    static readonly string[] IconPaths =
    {
        "Assets/Game/Resources/UI/leaf_icon.png",
        "Assets/Game/Resources/UI/droplet_icon.png",
    };

    [MenuItem("Tools/Setup HUD Icon Sprites")]
    public static void SetupIconSprites()
    {
        int count = 0;
        foreach (string path in IconPaths)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) { Debug.LogError($"[PanelSpriteSetup] importer not found: {path}"); continue; }

            importer.textureType      = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled    = false;
            importer.filterMode       = FilterMode.Bilinear;

            importer.SaveAndReimport();
            count++;
        }
        Debug.Log($"[PanelSpriteSetup] Configured {count} HUD icon sprite(s).");
    }
}
