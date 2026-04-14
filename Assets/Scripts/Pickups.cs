using UnityEngine;

public class Pickups : MonoBehaviour
{
    //Create the variables for the pickups
    public GameObject[] pickup;
    //public GameObject brickPickup;
    public void SpawnPickups(GameObject ground)
    {
        int pickupCount = Random.Range(2, 10);
        //Create a for loop to spawn mulitple
        for (int i = 0; i < pickupCount; i++)
        {
            GameObject spawnPickup = pickup[Random.Range(0, pickup.Length)];
            Vector3 randomPosition = ground.transform.position + new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-50f, 50f));
            Instantiate(spawnPickup, randomPosition,Quaternion.identity);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Check if the collided object is a fruit
        if (CompareTag("FruitPickup"))
        {
            Debug.Log("Fruit collected!");
            Destroy(gameObject);
        }
        if (CompareTag("BrickPickup"))
        {
            Debug.Log("Brick collected!");
            Destroy(gameObject);
        }
    }
}
