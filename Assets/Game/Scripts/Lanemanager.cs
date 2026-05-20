using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lanemanager : MonoBehaviour
{
    /*[Header("Lane Spawn Points")]
    public Transform[] laneSpawns;

    [Header("Obstacle Prefabs")]
    public GameObject[] easyObstacles;
    public GameObject[] mediumObstacles;
    public GameObject[] hardObstacles;

    [Header("UI Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    private GameObject[] currentObstacleSet;

    void Start()
    {
        SetupLevelDifficulty();
    }

    void SetupLevelDifficulty()
    {
        string currentScene =
            SceneManager.GetActiveScene().name;

        // LEVEL 1
        if (currentScene == "MainGame")
        {
            currentObstacleSet = easyObstacles;

            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);

            button3.gameObject.SetActive(false);
            button4.gameObject.SetActive(false);
        }

        // LEVEL 2
        /*else if (currentScene == "Level2")
        {
            currentObstacleSet = mediumObstacles;

            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);
            button3.gameObject.SetActive(true);

            button4.gameObject.SetActive(false);
        }

        // LEVEL 3
        else if (currentScene == "Level3")
        {
            currentObstacleSet = hardObstacles;

            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);
            button3.gameObject.SetActive(true);
            button4.gameObject.SetActive(true);
        }
    }

    public void SpawnObstacle(int laneIndex)
    {
        // SAFETY CHECK
        if (currentObstacleSet.Length == 0)
        {
            Debug.Log("No obstacles assigned!");
            return;
        }

        // RANDOM OBSTACLE
        int randomObstacle =
            Random.Range(0, currentObstacleSet.Length);

        // SPAWN POSITION
        Vector3 spawnPosition =
            laneSpawns[laneIndex].position;

        // SPAWN OBSTACLE
        Instantiate(
            currentObstacleSet[randomObstacle],
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log("Obstacle spawned in lane " + laneIndex);
    }*/

    [Header("Lane Positions (ONLY X matters here)")]
    public Transform[] laneSpawns;

    [Header("Player Reference")]
    public Transform player;

    [Header("Spawn Settings")]
    public float spawnDistance = 25f;

    [Header("Level 1 Obstacles")]
    public GameObject[] easyObstacles;

    [Header("UI Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    // Current obstacle list being used
    private GameObject[] currentObstacleSet;

    void Start()
    {
        SetupLevelDifficulty();

    }

    void SetupLevelDifficulty()
    {

        string currentScene =
            SceneManager.GetActiveScene().name;

        // LEVEL 1 ONLY
        if (currentScene == "MainGame")
        {
            currentObstacleSet = easyObstacles;

            // SHOW ONLY 2 BUTTONS
            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);

            // HIDE HARDER BUTTONS
            button3.gameObject.SetActive(false);
            button4.gameObject.SetActive(false);

            Debug.Log("Level 1 Loaded");
        }
    }

    public void SpawnObstacle(int laneIndex)
    {
        Debug.Log("BUTTON PRESSED");
        // SAFETY CHECK
        if (currentObstacleSet == null ||
            currentObstacleSet.Length == 0)
        {
            Debug.Log("No obstacles assigned!");
            return;
        }

        if (laneSpawns == null || laneIndex >= laneSpawns.Length)
        {
            Debug.Log("Lane spawn missing or invalid index!");
            return;
        }

        if (player == null)
        {
            Debug.Log("Player not assigned!");
            return;
        }

        // PICK RANDOM OBSTACLE
        int randomObstacle = Random.Range(0, currentObstacleSet.Length);

        // BUILD SPAWN POSITION (THIS IS THE FIX)
        Vector3 spawnPosition = new Vector3(
            laneSpawns[laneIndex].position.x,   // lane (X)
            laneSpawns[laneIndex].position.y,   // height (Y)
            player.position.z + spawnDistance   // forward (Z FIX)
        );

        // SPAWN
        Instantiate(
            currentObstacleSet[randomObstacle],
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log("Spawned obstacle in lane " + laneIndex);
    }
}