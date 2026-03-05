using UnityEngine;

public class Pickups : MonoBehaviour
{
    //Create the variables for the pickups
    public GameObject fruitPickup;
    public GameObject brickPickup;

    void Start()
    {
        //Spawn multiple pickups at the start and respawn
        SpawnPickups();
    }

    void SpawnPickups()
    {
        //Create a for loop to spawn mulitple
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
            Instantiate(fruitPickup,randomPosition,Quaternion.identity);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Check if the collided object is a fruit
            if (gameObject.CompareTag("FruitPickup"))
            {
                Debug.Log("Fruit collected!");
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("BrickPickup"))
            {
                Debug.Log("Brick collected!");
                Destroy(gameObject);
            }
        }
        //Check if all 10 are destroyed to respawn
        if (GameObject.FindGameObjectsWithTag("FruitPickup").Length == 0)
        {
            //Respawn Pickups
            SpawnPickups();
        }
    }
}
