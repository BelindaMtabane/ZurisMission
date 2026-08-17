using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SceneBootstrapper : MonoBehaviour
{
    private const string MainMenuScene = "StartScreen";
    private static readonly HashSet<string> IgnoredScenes = new HashSet<string> { "StartScreen", "StarterInfor" };

    private static readonly Dictionary<string, float> VillageProgressMap = new Dictionary<string, float>
    {
        { "MainGame", 33.5f },
        { "Level2", 67f },
        { "Level3", 80f },
        { "Level3End", 80f }
    };

    private static readonly Dictionary<string, string> NextSceneMap = new Dictionary<string, string>
    {
        { "MainGame", "Level2" },
        { "Level2", "Level3" },
        { "Level3", "" },
        { "Level3End", "" }
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterForSceneLoads()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IgnoredScenes.Contains(scene.name)) return;
        if (Object.FindFirstObjectByType<SceneBootstrapper>() != null) return;

        GameObject host = GameObject.Find("gameManager");
        if (host == null)
        {
            host = new GameObject("gameManager");
        }

        host.AddComponent<SceneBootstrapper>();
    }

    void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        RunStateManager runState = EnsureRunStateManager(sceneName);
        PlayerController playerController = EnsurePlayerController();
        EnsureHudControls(sceneName);
        EnsurePlayerHudBase(playerController);
        EnsureGroundSpawner(sceneName);

        Debug.Log($"[SceneBootstrapper] Phase 2 player ready in '{sceneName}' groundedCheck={playerController != null}.");
    }

    RunStateManager EnsureRunStateManager(string sceneName)
    {
        RunStateManager runState = FindFirstObjectByType<RunStateManager>();
        if (runState == null)
        {
            runState = gameObject.AddComponent<RunStateManager>();
        }

        GameObject pausePanel = GameObject.Find("PausePanel");
        GameObject victoryPanel = GameObject.Find("VictoryPanel2");

        runState.SetupPanels(null, victoryPanel, pausePanel);
        runState.SetupScenes(MainMenuScene, NextSceneMap.TryGetValue(sceneName, out string nextScene) ? nextScene : "");

        return runState;
    }

    PlayerController EnsurePlayerController()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("[SceneBootstrapper] Player GameObject not found.");
            return null;
        }

        PlayerMovement oldMovement = player.GetComponent<PlayerMovement>();
        if (oldMovement != null)
        {
            oldMovement.enabled = false;
        }

        PlayerMovementOG oldOg = player.GetComponent<PlayerMovementOG>();
        if (oldOg != null)
        {
            oldOg.enabled = false;
        }

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null)
        {
            controller = player.AddComponent<PlayerController>();
        }

        return controller;
    }

    void EnsureHudControls(string sceneName)
    {
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud == null)
        {
            Debug.LogWarning("[SceneBootstrapper] HUDControls not found.");
            return;
        }

        if (VillageProgressMap.TryGetValue(sceneName, out float value) && sceneName != "MainGame")
        {
            hud.SetVillageProgress(value);
        }
    }

    void EnsurePlayerHudBase(PlayerController playerController)
    {
        if (playerController == null) return;

        PlayerHUDBase hudBase = playerController.GetComponent<PlayerHUDBase>();
        if (hudBase == null)
        {
            hudBase = playerController.gameObject.AddComponent<PlayerHUDBase>();
        }
    }

    void EnsureGroundSpawner(string sceneName)
    {
        if (sceneName != "MainGame") return;

        GroundSpawnner spawner = FindFirstObjectByType<GroundSpawnner>();
        if (spawner == null)
        {
            Debug.LogWarning("[SceneBootstrapper] GroundSpawnner not found in MainGame.");
            return;
        }

        spawner.EnsureGroundLinked();
    }
}
