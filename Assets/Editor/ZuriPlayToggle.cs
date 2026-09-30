#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Play Mode always starts on StartScreen unless Skip is ticked.
/// Also provides shortcuts to open StartScreen and Level 1 (MainGame).
/// </summary>
[InitializeOnLoad]
public static class ZuriPlayToggle
{
    const string SkipMenu = "Tools/Zuri/Skip Start Screen When Testing";
    const string Key = "Zuri.SkipStartScreen";
    const string StartScreenPath = "Assets/Game/Scenes/Scene Levels/StartScreen.unity";
    const string MainGamePath = "Assets/Game/Scenes/Scene Levels/MainGame.unity";
    const string Level2Path = "Assets/Game/Scenes/Scene Levels/Level2.unity";
    const string Level3Path = "Assets/Game/Scenes/Scene Levels/Level3.unity";

    static ZuriPlayToggle()
    {
        ApplyPlayModeStartScene();
    }

    [MenuItem(SkipMenu)]
    static void Toggle()
    {
        EditorPrefs.SetBool(Key, !EditorPrefs.GetBool(Key, false));
        ApplyPlayModeStartScene();
    }

    [MenuItem(SkipMenu, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(SkipMenu, EditorPrefs.GetBool(Key, false));
        return true;
    }

    [MenuItem("Tools/Zuri/Open Start Screen")]
    static void OpenStartScreen() => OpenScene(StartScreenPath);

    [MenuItem("Tools/Zuri/Open Level 1 (MainGame)")]
    static void OpenMainGame() => OpenScene(MainGamePath);

    [MenuItem("Tools/Zuri/Open Level 2")]
    static void OpenLevel2() => OpenScene(Level2Path);

    [MenuItem("Tools/Zuri/Open Level 3")]
    static void OpenLevel3() => OpenScene(Level3Path);

    static void ApplyPlayModeStartScene()
    {
        bool skip = EditorPrefs.GetBool(Key, false);
        if (skip)
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        SceneAsset start = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScreenPath);
        EditorSceneManager.playModeStartScene = start;
    }

    static void OpenScene(string path)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(path);
    }
}
#endif
