using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    //Create variables
    public GameObject pauseMenuUI;

    //Set methods for buttons
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        SetSceneHUDVisible(false);
        LevelHUDStrip.Instance?.SetVisible(false);
        InventoryUI.Instance?.SetVisible(false);
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        SetSceneHUDVisible(true);
        LevelHUDStrip.Instance?.SetVisible(true);
        InventoryUI.Instance?.SetVisible(true);
    }

    // ── Public helper so EndLevelDialogue can call it for Level3End ───────
    public void HideSceneHUDForOverlay() => SetSceneHUDVisible(false);

    // ── Hide / show all scene HUD siblings of the pause panel ─────────────
    void SetSceneHUDVisible(bool visible)
    {
        if (pauseMenuUI == null) return;
        Transform canvasT = pauseMenuUI.transform.parent;
        if (canvasT == null) return;

        foreach (Transform child in canvasT)
        {
            if (child.gameObject == pauseMenuUI) continue;  // keep pause panel
            if (child.name == "FailPanel")       continue;  // keep fail panel
            if (child.name == "VictoryPanel2")   continue;  // keep victory panel
            child.gameObject.SetActive(visible);
        }

        GameInfoUI.Instance?.SetVisible(visible);
    }
    public void MainMenu()
    {
        //Return to main menu
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreen");
    }
    public void Restart()
    {
        //Restart the game
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainGame");
    }
    public void HUDInfor()
    {
        //Display HUD info
        Time.timeScale = 1f;
        SceneManager.LoadScene("StarterInfor");
    }
    public void Level2()
    {
        //Go to level 2
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }
    public void Level3()
    {
        //Go to level 3
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level3End");
    }

}
