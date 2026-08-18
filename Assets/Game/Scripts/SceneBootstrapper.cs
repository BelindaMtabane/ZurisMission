using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SceneBootstrapper : MonoBehaviour
{
    private static readonly HashSet<string> IgnoredScenes = new HashSet<string>
    {
        SceneCatalog.StartScreen,
        SceneCatalog.StarterInfor
    };

    private static readonly Dictionary<string, float> VillageProgressMap = new Dictionary<string, float>
    {
        { SceneCatalog.MainGame, 35f },
        { SceneCatalog.Level2, VillageProgressService.Level2StartPercent },
        { SceneCatalog.Level3, 80f },
        { SceneCatalog.Level3End, 80f }
    };

    private static readonly Dictionary<string, string> NextSceneMap = new Dictionary<string, string>
    {
        { SceneCatalog.MainGame, SceneCatalog.Level2 },
        { SceneCatalog.Level2, SceneCatalog.Level3 },
        { SceneCatalog.Level3, "" },
        { SceneCatalog.Level3End, "" }
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
        if (RunnerPlayerSetup.IsRunnerScene(sceneName))
        {
            RunnerPlayerSetup.Apply(sceneName);
        }
        EnsureHudControls(sceneName);
        EnsurePlayerHudBase(playerController);
        EnsureGroundSpawner(sceneName);
        EnsureLevelSystems(sceneName);
        LegacyLaneUi.Hide();

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
        runState.SetupScenes(SceneCatalog.StartScreen, NextSceneMap.TryGetValue(sceneName, out string nextScene) ? nextScene : "");

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
            GameObject host = GameObject.Find("HUDControls");
            if (host == null)
            {
                host = gameObject;
            }

            hud = host.GetComponent<HUDControls>();
            if (hud == null)
            {
                hud = host.AddComponent<HUDControls>();
            }
        }

        if (hud == null)
        {
            Debug.LogWarning("[SceneBootstrapper] HUDControls not found.");
            return;
        }

        if (VillageProgressMap.TryGetValue(sceneName, out float value)
            && !SceneCatalog.UsesObjectiveVillageProgress(sceneName))
        {
            hud.SetVillageProgress(SceneCatalog.IsLevel3(sceneName)
                ? VillageProgressService.Level3StartPercent
                : value);
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
        if (!GroundSpawnner.UsesStreamingTiles(sceneName)) return;

        GroundSpawnner spawner = FindFirstObjectByType<GroundSpawnner>();
        if (spawner == null)
        {
            Debug.LogWarning($"[SceneBootstrapper] GroundSpawnner not found in '{sceneName}'.");
            return;
        }

        spawner.EnsureGroundLinked();
    }

    void EnsureLevelSystems(string sceneName)
    {
        if (SceneCatalog.IsLevel1(sceneName))
        {
            if (FindFirstObjectByType<Level1LayoutDirector>() == null)
            {
                gameObject.AddComponent<Level1LayoutDirector>();
            }

            if (FindFirstObjectByType<Level1TutorialUI>() == null)
            {
                new GameObject("Level1TutorialUI").AddComponent<Level1TutorialUI>();
            }

            if (FindFirstObjectByType<Level1FeedbackUI>() == null)
            {
                new GameObject("Level1FeedbackUI").AddComponent<Level1FeedbackUI>();
            }

            if (FindFirstObjectByType<Level1LowWaterMonitor>() == null)
            {
                new GameObject("Level1LowWaterMonitor").AddComponent<Level1LowWaterMonitor>();
            }
        }
        else if (SceneCatalog.IsLevel2(sceneName))
        {
            if (FindFirstObjectByType<Level2LayoutDirector>() == null)
            {
                gameObject.AddComponent<Level2LayoutDirector>();
            }

            if (FindFirstObjectByType<Level2FeedbackUI>() == null)
            {
                new GameObject("Level2FeedbackUI").AddComponent<Level2FeedbackUI>();
            }
        }
        else if (SceneCatalog.IsLevel3(sceneName))
        {
            if (FindFirstObjectByType<Level3LayoutDirector>() == null)
            {
                gameObject.AddComponent<Level3LayoutDirector>();
            }

            if (FindFirstObjectByType<Level3FeedbackUI>() == null)
            {
                new GameObject("Level3FeedbackUI").AddComponent<Level3FeedbackUI>();
            }
        }
    }
}
