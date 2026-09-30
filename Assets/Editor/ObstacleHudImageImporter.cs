using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Imports the obstacle artwork supplied for the HUD into its runtime Resources
/// folder and configures every file as a UI Sprite.
/// </summary>
[InitializeOnLoad]
public static class ObstacleHudImageImporter
{
    const string TargetFolder = "Assets/Game/Resources/UI/Obstacles";
    const string SourceFolder =
        @"C:\Users\TUF\.cursor\projects\c-Users-TUF-source-repos-WILSummativePortfolio-ZurisMission\assets";

    static readonly Dictionary<string, string> Images = new()
    {
        { "log.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-a54bc94f-3100-4bae-8bd3-e4bae3672e0f.png" },
        { "snake.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-647f8f9d-08f0-4649-9507-4652c6e942f9.png" },
        { "wolf.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-04661b51-837d-4550-b612-1fbbf6c1606e.png" },
        { "warthog.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-63f571f1-6b37-4b55-9359-e2289c506018.png" },
        { "mud.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-a4ba67ff-be41-48c3-b167-90e41a300fc9.png" },
        { "heat_wave.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-9585d911-fc82-4bda-88bf-a7690cf7ca96.png" },
        { "acid.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-8eaddbaa-495b-4deb-958d-06dd54b6ce5a.png" },
        { "sand_pit.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-3e3fc533-5097-487e-9c02-32f9cd6782c2.png" },
        { "lightning.png", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-a10e201b-9b91-4d33-981a-4348f9c309b8.png" }
    };

    static ObstacleHudImageImporter()
    {
        EditorApplication.delayCall += ImportImages;
    }

    [MenuItem("Tools/Zuri/Import Obstacle HUD Images")]
    public static void ImportImages()
    {
        Directory.CreateDirectory(TargetFolder);
        bool copiedAny = false;

        foreach (KeyValuePair<string, string> pair in Images)
        {
            string source = Path.Combine(SourceFolder, pair.Value);
            string target = Path.Combine(TargetFolder, pair.Key);
            if (!File.Exists(source)) continue;

            if (!File.Exists(target))
            {
                File.Copy(source, target);
                copiedAny = true;
            }
        }

        if (copiedAny) AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        foreach (string fileName in Images.Keys)
        {
            string assetPath = TargetFolder + "/" + fileName;
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;

            bool changed = importer.textureType != TextureImporterType.Sprite
                           || importer.spriteImportMode != SpriteImportMode.Single
                           || importer.mipmapEnabled;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            if (changed) importer.SaveAndReimport();
        }

        if (copiedAny)
            Debug.Log("[ObstacleHudImageImporter] Imported supplied obstacle HUD images.");
    }
}
