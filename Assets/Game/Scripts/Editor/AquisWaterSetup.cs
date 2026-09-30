using UnityEditor;
using UnityEngine;

public static class AquisWaterSetup
{
    const string SourceRoot = "Assets/AureDevGames/Water Stylized Shader Orto & Perspective Camera/PresetMaterials";
    const string Dest = "Assets/Game/Resources/Water";

    [MenuItem("Tools/Water/Copy AQUIS Materials To Resources")]
    public static void CopyMaterialsToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder(Dest))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "Water");

        CopyMat($"{SourceRoot}/M_StylizedColdWater.mat", $"{Dest}/PoolWater.mat");
        CopyMat($"{SourceRoot}/M_RiverWater.mat", $"{Dest}/RiverWater.mat");

        AssetDatabase.Refresh();
        Debug.Log("[AquisWaterSetup] Copied pool and river materials into Resources/Water.");
    }

    [MenuItem("Tools/Water/Apply AQUIS To Scene Water")]
    public static void ApplyToOpenScene()
    {
        CopyMaterialsToResources();
        StylizedWaterFxSetup.CopyToResources();
        AquisWaterVisuals.DressSceneWater();
        Debug.Log("[AquisWaterSetup] Applied AQUIS materials to WellWaterTop, River, and pool meshes in the open scene.");
    }

    [MenuItem("Tools/Water/Setup All Water Assets (AQUIS + Namu FX)")]
    public static void SetupAllWaterAssets()
    {
        CopyMaterialsToResources();
        StylizedWaterFxSetup.CopyToResources();
        Debug.Log("[AquisWaterSetup] Copied AQUIS materials and NamuFX prefabs into Resources.");
    }

    static void CopyMat(string src, string dst)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(src) == null)
        {
            Debug.LogWarning($"[AquisWaterSetup] Missing {src}");
            return;
        }

        AssetDatabase.DeleteAsset(dst);
        if (!AssetDatabase.CopyAsset(src, dst))
            Debug.LogWarning($"[AquisWaterSetup] Failed to copy {src} -> {dst}");
    }
}
