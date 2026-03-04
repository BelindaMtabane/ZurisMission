using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    //Create variables
    public GameObject[] obstacleSpawnObjects;
    private float timer;


    void Update()
    {
        HandleSpawnObstacle();
    }
    private void HandleSpawnObstacle()
    {
        int obstacleToSpawn = 10; // Number of pickups to spawn
        
        for (int i = 0; i < obstacleToSpawn; i++)
        {
            int randomSpawnIndex = UnityEngine.Random.Range(0, obstacleSpawnObjects.Length); // Randomly select a pickup prefab from the array
            GameObject obstacle = Instantiate(obstacleSpawnObjects[randomSpawnIndex]);

            Vector3 spawnPos = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
            obstacle.transform.position = spawnPos;
            Destroy(obstacle, 60); // Destroy after 60 seconds
        }
    }

}
