using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lanemanager2 : MonoBehaviour
{
    public Transform[] laneSpawnsPositions;
    HUDControls hudControls;
    public Transform player;
    public float spawnDistance = 30f;
    public GameObject[] mediumObstacles;

    [Header("UI Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    private GameObject[] currentObstacleSet;


    private void Start()
    {
        if (hudControls == null)
            hudControls = FindFirstObjectByType<HUDControls>();
        LevelDifficulty();
        StartCoroutine(SpawnObstacleEvery5Seconds());
    }

    private System.Collections.IEnumerator SpawnObstacleEvery5Seconds()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            int randomLane = Random.Range(0, laneSpawnsPositions.Length);
            SpawnObstacle(randomLane);
            Debug.Log("Auto spawned obstacle in lane " + randomLane);
            //Decrease the water levels
            hudControls.WaterMoveManager();
        }
    }
    void LevelDifficulty()
    {
        
            currentObstacleSet = mediumObstacles;

            //Make sure 3 buttons are visible in tthe scene
            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);
            button3.gameObject.SetActive(true);

            //Hidwe the remaining buttons to deduce the difficulty
            button4.gameObject.SetActive(false);

            Debug.Log("Level 2 Loaded");
        
        
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