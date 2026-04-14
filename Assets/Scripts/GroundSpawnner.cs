using UnityEngine;

public class GroundSpawnner : MonoBehaviour
{
    public GameObject groundPrefabTrigger;
    public Pickups pickups;

    void Start()
    {
        // Assign the pickup script to the variable
        if (pickups == null) return;
        pickups = Object.FindFirstObjectByType<Pickups>(); // Updated to use the recommended method
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            SpawnGround();
        }
    }
    void SpawnGround()
    {
        // Instantiate a new ground piece at the desired position
        Vector3 spawnPosition = new Vector3(transform.position.x,0f, transform.position.z + 71); // Adjust the spawn position as needed
        GameObject newGround = Instantiate(groundPrefabTrigger, spawnPosition, Quaternion.identity);
        // Spawn the pickups on the new grounds
        pickups.SpawnPickups(newGround);
        // Destroy the ground after 10 seconds each
        Destroy(newGround, 10f);
    }
}
