using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public void Pause()
    {
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.Pause();
            return;
        }

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.Resume();
            return;
        }

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.GoToMainMenu();
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreen");
    }

    public void Restart()
    {
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.RestartRun();
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void HUDInfor()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StarterInfor");
    }

    public void Level2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }

    public void Level3()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level3");
    }
}
