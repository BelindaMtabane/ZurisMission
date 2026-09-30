using UnityEditor;
using UnityEngine;

/// <summary>Copies a Big Water Tower prefab into Resources so Level 3 houses can spawn it at runtime.</summary>
public static class WaterTowerSetup
{
    const string DestFolder = "Assets/Game/Resources/Village";
    const string DestPrefab = DestFolder + "/BigWaterTower.prefab";

    [MenuItem("Tools/Village/Copy Big Water Tower To Resources")]
    public static void CopyToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder(DestFolder))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "Village");

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        string bestPath = null;
        int bestScore = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(path) || path.Contains("/Resources/Village/")) continue;
            string n = path.Replace("\\", "/").ToLowerInvariant();
            int score = 0;
            if (n.Contains("big water tower") || n.Contains("bigwatertower")) score += 8;
            if (n.Contains("water tower") || n.Contains("watertower")) score += 5;
            if (n.Contains("tower") && n.Contains("water")) score += 3;
            if (score > bestScore)
            {
                bestScore = score;
                bestPath = path;
            }
        }

        if (string.IsNullOrEmpty(bestPath) || bestScore < 3)
        {
            Debug.LogWarning("[WaterTowerSetup] Big Water Tower prefab not found. Import the pack, then run this menu again.");
            return;
        }

        AssetDatabase.DeleteAsset(DestPrefab);
        if (AssetDatabase.CopyAsset(bestPath, DestPrefab))
            Debug.Log($"[WaterTowerSetup] Copied {bestPath} -> {DestPrefab}");
        else
            Debug.LogWarning($"[WaterTowerSetup] Failed to copy {bestPath}");

        AssetDatabase.Refresh();
    }
}
