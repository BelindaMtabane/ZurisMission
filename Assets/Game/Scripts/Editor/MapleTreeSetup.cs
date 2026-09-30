using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MapleTreeSetup
{
    const string Dest = "Assets/Game/Resources/MapleTrees";

    static readonly string[] SearchFolderHints =
    {
        "Assets/klen/Prefab",
        "Assets/klen",
        "HQ Autumn Dry Maple Trees",
        "Autumn Dry Maple",
        "Maple"
    };

    [MenuItem("Tools/Maple Trees/Find And Copy Prefabs To Resources")]
    public static void CopyPrefabsToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder(Dest))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "MapleTrees");

        List<string> paths = FindMaplePrefabPaths();
        if (paths.Count == 0)
        {
            Debug.LogWarning("[MapleTreeSetup] No maple prefabs found. Expected Assets/klen/Prefab/Maple_*.prefab");
            return;
        }

        paths.Sort(System.StringComparer.OrdinalIgnoreCase);

        int copied = 0;
        for (int i = 0; i < paths.Count; i++)
        {
            string fileName = Path.GetFileName(paths[i]);
            string dst = $"{Dest}/{fileName}";
            AssetDatabase.DeleteAsset(dst);
            if (AssetDatabase.CopyAsset(paths[i], dst)) copied++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[MapleTreeSetup] Copied {copied} maple prefab(s) to {Dest}.");
    }

    static List<string> FindMaplePrefabPaths()
    {
        var results = new HashSet<string>();

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab");
        for (int i = 0; i < allGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
            if (!LooksLikeMapleTreePrefab(path)) continue;
            results.Add(path);
        }

        return new List<string>(results);
    }

    static bool LooksLikeMapleTreePrefab(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".prefab")) return false;

        for (int i = 0; i < SearchFolderHints.Length; i++)
        {
            if (assetPath.IndexOf(SearchFolderHints[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        string name = Path.GetFileNameWithoutExtension(assetPath);
        return name.IndexOf("Maple", System.StringComparison.OrdinalIgnoreCase) >= 0
               && name.IndexOf("Tree", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
