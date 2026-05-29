using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    //Create variables
    public GameObject startMenuUI;
    //public GameObject controlsUI;

    //Set methods for buttons
    public void Update()
    {
        //Delay and start the game after 50 seconds
        /*Time.timeScale += 1f;
        Debug.Log("Game will start in 50 seconds...");

        if (Time.timeScale == 50f)
        {
            SceneManager.LoadScene(1);
        }*/

    }
    public void StartGame()
    {
        // Load the first level
        SceneManager.LoadScene("MainGame");
    }

    public void ExitGame()
    {
        //Exit the game
        Application.Quit();
    }
}
