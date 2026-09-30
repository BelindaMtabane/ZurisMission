using UnityEditor;
using UnityEngine;

/// <summary>
/// Copies selected Pandazole prefabs into Resources so builds can load them.
/// </summary>
public static class NaturePackSetup
{
    const string Source = "Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs";
    const string Dest = "Assets/Game/Resources/NaturePack";

    static readonly string[] Names =
    {
        "Cactus_01_A", "Cactus_04_A", "Cactus_08_A", "Cactus_12_A", "Cactus_15_A",
        "Cactus_19_A", "Cactus_20_A", "Cactus_24_A", "Cactus_27_A", "Cactus_31_A",
        "Cactus_01_B", "Cactus_08_B", "Cactus_12_B", "Cactus_19_B", "Cactus_24_B",
        "Log_01", "Log_02", "Log_03", "Log_04", "Log_05", "Log_06", "Log_07", "Log_08",
        "HardRock_01", "HardRock_02", "HardRock_06", "HardRock_08", "HardRock_12",
        "HardRock_15", "HardRock_18", "SoftRock_03", "SoftRock_06", "SoftRock_13",
        "Tree_01_Fall", "Tree_04_Fall", "Tree_08_Fall", "Tree_12_Fall", "Tree_16_Fall", "Tree_20_Fall",
        "Bush_02", "Bush_07", "Bush_10", "Bush_13", "Thorns_02", "Thorns_05", "Thorns_14",
        "Grass_01", "Grass_03", "Grass_07", "Grass_11", "Grass_14", "Grass_19",
        "Grass_22", "Grass_26", "Grass_29", "Grass_35", "Grass_08", "Grass_16"
    };

    [MenuItem("Tools/Nature Pack/Copy Prefabs To Resources")]
    public static void CopyPrefabsToResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Resources"))
            AssetDatabase.CreateFolder("Assets/Game", "Resources");
        if (!AssetDatabase.IsValidFolder(Dest))
            AssetDatabase.CreateFolder("Assets/Game/Resources", "NaturePack");

        int copied = 0;
        for (int i = 0; i < Names.Length; i++)
        {
            string src = $"{Source}/{Names[i]}.prefab";
            string dst = $"{Dest}/{Names[i]}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(src) == null)
            {
                Debug.LogWarning($"[NaturePackSetup] Missing {src}");
                continue;
            }

            AssetDatabase.DeleteAsset(dst);
            if (AssetDatabase.CopyAsset(src, dst)) copied++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[NaturePackSetup] Copied {copied} Pandazole prefabs into {Dest}");
    }

    [MenuItem("Tools/Nature Pack/Convert Materials To URP")]
    public static void ConvertMaterialsToUrp()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null)
        {
            Debug.LogError("[NaturePackSetup] URP Lit shader not found.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[]
        {
            "Assets/Pandazole_Ultimate_Pack",
            "Assets/Game/Resources/NaturePack"
        });

        int converted = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null || mat.shader == lit) continue;

            Texture albedo = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            if (albedo == null && mat.HasProperty("_BaseMap")) albedo = mat.GetTexture("_BaseMap");
            Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;

            mat.shader = lit;
            if (mat.HasProperty("_BaseMap") && albedo != null) mat.SetTexture("_BaseMap", albedo);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.18f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(mat);
            converted++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[NaturePackSetup] Converted {converted} Pandazole materials to URP Lit.");
    }
}
