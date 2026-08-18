using UnityEngine.SceneManagement;

/// <summary>
/// Canonical scene names for Drop by Drop: Flow of Hope.
/// Use these constants instead of typing scene names as string literals.
/// </summary>
public static class SceneCatalog
{
    public const string StartScreen = "StartScreen";
    public const string MainGame = "MainGame";
    public const string Level2 = "Level2";
    public const string Level3 = "Level3";

    // Other scenes still referenced in the project.
    public const string StarterInfor = "StarterInfor";
    public const string Level3End = "Level3End";

    public static string ActiveName => SceneManager.GetActiveScene().name;

    public static bool IsActive(string sceneName) => ActiveName == sceneName;

    public static bool IsRunnerScene(string sceneName) =>
        sceneName == MainGame || sceneName == Level2 || sceneName == Level3;

    public static bool IsLevel1(string sceneName) => sceneName == MainGame;

    public static bool IsLevel2(string sceneName) => sceneName == Level2;

    public static bool IsLevel3(string sceneName) => sceneName == Level3;

    public static bool UsesObjectiveVillageProgress(string sceneName) =>
        sceneName == MainGame || sceneName == Level2;

    public static bool ShouldIgnoreBootstrap(string sceneName) =>
        sceneName == StartScreen || sceneName == StarterInfor;

    /// <summary>
    /// Returns the next gameplay scene, or empty string when there is no next scene.
    /// </summary>
    public static string GetNextScene(string currentScene)
    {
        if (currentScene == MainGame) return Level2;
        if (currentScene == Level2) return Level3;
        if (currentScene == Level3 || currentScene == Level3End) return "";
        return "";
    }
}
