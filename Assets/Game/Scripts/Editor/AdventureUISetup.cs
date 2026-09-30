using UnityEditor;
using UnityEngine;

/// <summary>
/// Copies Adventure UI Kit sprites into Resources and configures 9-slice borders.
/// </summary>
public static class AdventureUISetup
{
    const string SourceRoot = "Assets/Adventure UI Kit/Sprites";
    const string DestFolder = "Assets/Game/Resources/UI/Adventure";

    static readonly (string src, string dest, Vector4 border)[] Copies =
    {
        ("GameUI/popup_bg.png", "panel_popup.png", new Vector4(32, 32, 32, 32)),
        ("GameUI/progressbar_bg_.png", "progressbar_bg.png", new Vector4(20, 20, 20, 20)),
        ("Slider/slider_fill_green.png", "fill_green.png", new Vector4(12, 12, 12, 12)),
        ("Slider/slider_fill_blue.png", "fill_blue.png", new Vector4(12, 12, 12, 12)),
        ("Slider/slider_fill_orange.png", "fill_orange.png", new Vector4(12, 12, 12, 12)),
        ("GameUI/btn_retry.png", "btn_retry.png", Vector4.zero),
        ("Button/btn_newGame.png", "btn_new_game.png", Vector4.zero),
        ("Button/btn_resume.png", "btn_resume.png", Vector4.zero),
        ("Button/btn_quit.png", "btn_quit.png", Vector4.zero),
        ("Button/btn_setting.png", "btn_setting.png", Vector4.zero),
        ("GameUI/btn_bg_medium.png", "btn_medium.png", new Vector4(24, 24, 24, 24)),
        ("GameUI/btn_play_bg.png", "btn_play.png", Vector4.zero),
        ("GameUI/coin_bg.png", "coin_bg.png", new Vector4(24, 24, 24, 24)),
        ("Icons/icon_pause.png", "icon_pause.png", Vector4.zero),
        ("Button/btn_mission.png", "btn_mission.png", Vector4.zero),
        ("GameUI/btn_small.png", "btn_small.png", new Vector4(16, 16, 16, 16)),
    };

    [MenuItem("Tools/Adventure UI/Copy Sprites To Resources")]
    public static void CopySpritesToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources/UI"))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "UI");
        if (!AssetDatabase.IsValidFolder(DestFolder))
            AssetDatabase.CreateFolder("Assets/Game/Resources/UI", "Adventure");

        int copied = 0;
        for (int i = 0; i < Copies.Length; i++)
        {
            string src = $"{SourceRoot}/{Copies[i].src}";
            string dst = $"{DestFolder}/{Copies[i].dest}";
            if (AssetDatabase.LoadAssetAtPath<Object>(src) == null)
            {
                Debug.LogWarning($"[AdventureUISetup] Missing {src}");
                continue;
            }

            AssetDatabase.DeleteAsset(dst);
            if (AssetDatabase.CopyAsset(src, dst))
            {
                ApplyBorder(dst, Copies[i].border);
                copied++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AdventureUISetup] Copied {copied} Adventure UI sprites into {DestFolder}.");
    }

    static void ApplyBorder(string assetPath, Vector4 border)
    {
        if (border == Vector4.zero) return;
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }
}
