using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Canonical scene names for Drop by Drop: Flow of Hope.
/// Use these constants instead of typing scene names as string literals.
/// </summary>
public static class SceneCatalog
{
    public const string Tutorial = "Tutorial";
    public const string StartScreen = "StartScreen";
    public const string MiniNarrative1 = "MiniNarrative1";
    public const string MiniNarrative2 = "MiniNarrative2";
    public const string MiniNarrative3 = "MiniNarrative3";
    public const string MiniNarrative4 = "MiniNarrative4";
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
        sceneName == StartScreen || sceneName == StarterInfor ||
               sceneName == MiniNarrative1 || sceneName == MiniNarrative2 ||
               sceneName == MiniNarrative3 || sceneName == MiniNarrative4;

    const string Level1WonKey = "DropByDrop.Level1Won";
    const string Level2WonKey = "DropByDrop.Level2Won";
    const string Level3WonKey = "DropByDrop.Level3Won";

    public static bool HasWonLevel1 => PlayerPrefs.GetInt(Level1WonKey, 0) == 1;
    public static bool HasWonLevel2 => PlayerPrefs.GetInt(Level2WonKey, 0) == 1;
    public static bool HasWonLevel3 => PlayerPrefs.GetInt(Level3WonKey, 0) == 1;

    public static bool CanOpenNarrative(string narrativeScene)
    {
        if (narrativeScene == MiniNarrative1) return true;
        if (narrativeScene == MiniNarrative2) return HasWonLevel1;
        if (narrativeScene == MiniNarrative3) return HasWonLevel2;
        if (narrativeScene == MiniNarrative4) return HasWonLevel3;
        return false;
    }

    public static void ResetAllProgress()
    {
        StoryNarrativeData.ResetAll();
        StoryNarrativeData2.ResetAll();
        StoryNarrativeData3.ResetAll();
        StoryNarrativeData4.ResetAll();
        PlayerPrefs.DeleteKey(Level1WonKey);
        PlayerPrefs.DeleteKey(Level2WonKey);
        PlayerPrefs.DeleteKey(Level3WonKey);
        PlayerPrefs.Save();
    }

    public static void RecordLevelVictory(string sceneName)
    {
        if (IsLevel1(sceneName))
            PlayerPrefs.SetInt(Level1WonKey, 1);
        else if (IsLevel2(sceneName))
            PlayerPrefs.SetInt(Level2WonKey, 1);
        else if (IsLevel3(sceneName))
            PlayerPrefs.SetInt(Level3WonKey, 1);
        else
            return;

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns the next scene after a level victory or after a narrative scene ends.
    /// Losing a level restarts that same level and never uses this path.
    /// </summary>
    public static string GetNextScene(string currentScene)
    {
        if (currentScene == MainGame) return MiniNarrative2;
        if (currentScene == Level2) return MiniNarrative3;
        if (currentScene == Level3 || currentScene == Level3End) return MiniNarrative4;
        if (currentScene == MiniNarrative1) return MainGame;
        if (currentScene == MiniNarrative2) return Level2;
        if (currentScene == MiniNarrative3) return Level3;
        if (currentScene == MiniNarrative4) return StartScreen;
        return "";
    }
}
