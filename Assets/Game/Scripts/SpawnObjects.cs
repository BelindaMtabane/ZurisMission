using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    //Create the variables for the objects
    public GameObject[] spawnObjects;
    //public GameObject brickPickup;
    public void SpawnGameObjects(GameObject ground)
    {
        int spawnCount = Random.Range(1, 5);
        //Create a for loop to spawn mulitple
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawnPickup = spawnObjects[Random.Range(0, spawnObjects.Length)];
            Vector3 randomPosition = ground.transform.position + new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-50f, 50f));
            Instantiate(spawnPickup, randomPosition,Quaternion.identity);
            
        }
    }
    
}
