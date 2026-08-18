using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lanemanager3 : MonoBehaviour
{
    public Transform[] laneSpawnsPositions;
    HUDControls hudControls;
    public Transform player;
    public float spawnDistance = 25f;
    public GameObject[] hardObstacles;

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
        if (!enabled) return;
        LevelDifficulty();
        StartCoroutine(SpawnObstacleEvery5Seconds());
    }

    private System.Collections.IEnumerator SpawnObstacleEvery5Seconds()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (!enabled) yield break;
            if (laneSpawnsPositions == null || laneSpawnsPositions.Length == 0) continue;
            int randomLane = Random.Range(0, laneSpawnsPositions.Length);
            SpawnObstacle(randomLane);
            Debug.Log("Auto spawned obstacle in lane " + randomLane);
            if (hudControls != null)
                hudControls.WaterMoveManager();
        }
    }

    void LevelDifficulty()
    {
        
            currentObstacleSet = hardObstacles;

            if (button1 != null) button1.gameObject.SetActive(true);
            if (button2 != null) button2.gameObject.SetActive(true);
            if (button3 != null) button3.gameObject.SetActive(true);
            if (button4 != null) button4.gameObject.SetActive(true);

            Debug.Log("Level 3 Loaded");
        
        
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