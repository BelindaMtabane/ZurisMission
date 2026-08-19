using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a Humanoid Animator to the player's Female.prefab, retargeting
/// DenysAlmaral CityPeople's "City F Animator" clips (locom_f_jogging_30f etc.)
/// onto the Distant Lands Free Characters rig. Both are Humanoid-rigged, so
/// Mecanim retargets automatically via each Animator's own Avatar.
/// </summary>
public static class PlayerAnimationSetup
{
    const string FemalePrefabPath = "Assets/Distant Lands/Free Characters/Contents/Mesh/Female.prefab";
    const string ControllerPath = "Assets/DenysAlmaral/CityPeople/Animations/City F Animator.controller";

    [MenuItem("Tools/Setup Player Run Animation")]
    public static void SetupPlayerAnimation()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(FemalePrefabPath);
        try
        {
            Animator anim = root.GetComponent<Animator>();
            if (anim == null) anim = root.AddComponent<Animator>();

            Avatar avatar = FindAvatar(FemalePrefabPath);
            if (avatar == null)
            {
                // Avatar sub-asset usually lives on the source FBX, not the prefab.
                avatar = FindAvatar("Assets/Distant Lands/Free Characters/Contents/Mesh/Female.fbx");
            }
            if (avatar == null)
            {
                Debug.LogError("[PlayerAnimationSetup] No Humanoid Avatar found for Female model.");
                return;
            }

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[PlayerAnimationSetup] Controller not found at {ControllerPath}");
                return;
            }

            anim.avatar = avatar;
            anim.runtimeAnimatorController = controller;
            anim.applyRootMotion = false;

            PrefabUtility.SaveAsPrefabAsset(root, FemalePrefabPath);
            Debug.Log($"[PlayerAnimationSetup] Animator added to {FemalePrefabPath} with avatar '{avatar.name}' and controller '{controller.name}'.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static Avatar FindAvatar(string path)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (o is Avatar a) return a;
        }
        return null;
    }
}
