using UnityEditor;
using UnityEngine;

/// <summary>Copies PolyRonin desert village house prefabs into Resources for runtime scatter.</summary>
public static class VillageHouseSetup
{
    const string Source = "Assets/PolyRonin/Desert Village/Prefabs";
    const string Dest = "Assets/Game/Resources/Village";

    static readonly string[] Houses = { "House1", "House2", "House3", "House4", "House5", "House6" };

    [MenuItem("Tools/Village/Copy House Prefabs To Resources")]
    public static void CopyToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder(Dest))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "Village");

        int copied = 0;
        for (int i = 0; i < Houses.Length; i++)
        {
            string src = $"{Source}/{Houses[i]}.prefab";
            string dst = $"{Dest}/{Houses[i]}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(src) == null)
            {
                Debug.LogWarning($"[VillageHouseSetup] Missing {src}");
                continue;
            }

            AssetDatabase.DeleteAsset(dst);
            if (AssetDatabase.CopyAsset(src, dst)) copied++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[VillageHouseSetup] Copied {copied} house prefabs to {Dest}");
    }
}
