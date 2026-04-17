using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    //Create variables
    public GameObject pauseMenuUI;

    //Set methods for buttons
    public void Pause()
    {
        //Display menu
        pauseMenuUI.SetActive(true);
        //Stop time
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        //Hide menu
        pauseMenuUI.SetActive(false);
        //Resume time
        Time.timeScale = 1f;
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
}
