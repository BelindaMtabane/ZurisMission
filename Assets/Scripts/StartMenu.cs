using UnityEngine;

public class StartMenu : MonoBehaviour
{
    //Create variables
    public GameObject startMenuUI;
    //public GameObject controlsUI;

    //Set methods for buttons
    public void StartGame()
    {
        //Start the game
        startMenuUI.SetActive(false);
        //controlsUI.SetActive(false);
    }
    public void ExitGame()
    {
        //Exit the game
        Application.Quit();
    }
    /*public void Controls()
    {
        //Display controls
        startMenuUI.SetActive(false);
        //controlsUI.SetActive(true);
    }*/
}
