using UnityEditor;
using UnityEngine;

public static class RoadsideBoulderSetup
{
    const string Dest = "Assets/Game/Resources/RoadsideBoulders";

    static readonly (string src, string name)[] Files =
    {
        ("Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Rock/Rock_A_01.prefab", "Rock_A_01"),
        ("Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Rock/Rock_A_02.prefab", "Rock_A_02"),
        ("Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Rock/Rock_A_03.prefab", "Rock_A_03"),
        ("Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Rock/Rock_A_04.prefab", "Rock_A_04"),
        ("Assets/AssetHunts!/GameDev Starter Kit - Farming/Asset/Rock/Rock_A_05.prefab", "Rock_A_05"),
        ("Assets/AGWYN's Low Poly Cliffs/Models/Cliff_Large_1.prefab", "Cliff_Large_1"),
        ("Assets/AGWYN's Low Poly Cliffs/Models/Cliff_Large_2.prefab", "Cliff_Large_2"),
        ("Assets/AGWYN's Low Poly Cliffs/Models/Cliff_Small_1.prefab", "Cliff_Small_1"),
        ("Assets/AGWYN's Low Poly Cliffs/Models/Cliff_Small_2.prefab", "Cliff_Small_2")
    };

    [MenuItem("Tools/Roadside/Copy Boulder Prefabs To Resources")]
    public static void CopyToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder(Dest))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "RoadsideBoulders");

        int copied = 0;
        for (int i = 0; i < Files.Length; i++)
        {
            string src = Files[i].src;
            string dst = $"{Dest}/{Files[i].name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(src) == null)
            {
                Debug.LogWarning($"[RoadsideBoulderSetup] Missing {src}");
                continue;
            }

            AssetDatabase.DeleteAsset(dst);
            if (AssetDatabase.CopyAsset(src, dst)) copied++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[RoadsideBoulderSetup] Copied {copied} boulder prefabs to Resources/RoadsideBoulders.");
    }
}
