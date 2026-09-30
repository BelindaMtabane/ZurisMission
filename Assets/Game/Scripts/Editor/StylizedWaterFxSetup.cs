using UnityEditor;
using UnityEngine;

public static class StylizedWaterFxSetup
{
    const string Source = "Assets/NamuFX/StylizedWaterEffects";
    const string Dest = "Assets/Game/Resources/WaterFx";

    static readonly string[] Prefabs =
    {
        "Bubbles_Vertical_Loop",
        "Bubbles_Burst",
        "Water_Beam",
        "Hit_02",
        "Water_Splash_Multiple",
        "Water_Splash_A",
        "Water_Impact",
        "Hit_01"
    };

    [MenuItem("Tools/Water/Copy Stylized Water FX To Resources")]
    public static void CopyToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder(Dest))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "WaterFx");

        int copied = 0;
        for (int i = 0; i < Prefabs.Length; i++)
        {
            string src = $"{Source}/Prefabs/{Prefabs[i]}.prefab";
            string dst = $"{Dest}/{Prefabs[i]}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(src) == null)
            {
                Debug.LogWarning($"[StylizedWaterFxSetup] Missing {src}");
                continue;
            }

            AssetDatabase.DeleteAsset(dst);
            if (AssetDatabase.CopyAsset(src, dst)) copied++;
        }

        CopyMat($"{Source}/Materials/M_WaterTrail.mat", $"{Dest}/WaterTrail.mat");
        CopyMat($"{Source}/Materials/M_Water_03.mat", $"{Dest}/GreenWater.mat");

        AssetDatabase.Refresh();
        Debug.Log($"[StylizedWaterFxSetup] Copied {copied} FX prefabs + trail material into Resources/WaterFx.");
    }

    static void CopyMat(string src, string dst)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(src) == null)
        {
            Debug.LogWarning($"[StylizedWaterFxSetup] Missing {src}");
            return;
        }

        AssetDatabase.DeleteAsset(dst);
        AssetDatabase.CopyAsset(src, dst);
    }
}
