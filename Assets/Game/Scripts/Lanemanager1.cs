using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lanemanager1 : MonoBehaviour
{
    public Transform[] laneSpawnsPositions;

    public Transform player;
    public float spawnDistance = 25f;
    public GameObject[] easyObstacles;

    [Header("UI Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    private GameObject[] currentObstacleSet;

    void Start()
    {
        LevelDifficulty();
    }

    void LevelDifficulty()
    {
        if (SceneManager.GetActiveScene().name == "MainScreen")
        { 
            currentObstacleSet = easyObstacles;

            //Make sure 2 buttons are visible in tthe scene
            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);

            //Hidwe the remaining buttons to deduce the difficulty
            button3.gameObject.SetActive(false);
            button4.gameObject.SetActive(false);

            Debug.Log("Level 1 Loaded");
        }
        
    }

    public void SpawnObstacle(int laneIndex)
    {
        Debug.Log("BUTTON PRESSED");
        //Check if the obstacle set is assigned and not empty
        if (currentObstacleSet == null || currentObstacleSet.Length == 0)
        {
            Debug.Log("No obstacles assigned!");
            return;
        }
        //Check if the lane spawn positions are assigned and the index is valid
        if (laneSpawnsPositions == null || laneIndex >= laneSpawnsPositions.Length)
        {
            Debug.Log("Lane spawn missing or not the correct index!");
            return;
        }
        //Check if the player is assigned
        if (player == null)
        {
            Debug.Log("Player not assigned!");
            return;
        }

        //Pick a random obstacle from the current set
        int randomObstacle = Random.Range(0, currentObstacleSet.Length);

        //Instatiate the spawnning positions
        Vector3 spawnPosition = new Vector3(
            laneSpawnsPositions[laneIndex].position.x,   //Correct the x axis of the obstacles based on the buttons
            laneSpawnsPositions[laneIndex].position.y,  //Set the height of the obstacles
            player.position.z + spawnDistance //Correct the z axis of the spawnned obstacles
        );

        //Spawn the obstacles
        Instantiate(currentObstacleSet[randomObstacle],spawnPosition,Quaternion.identity);

        Debug.Log("Spawned obstacle in lane " + laneIndex);
    }

}