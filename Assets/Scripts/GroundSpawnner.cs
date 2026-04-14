using UnityEngine;

public class GroundSpawnner : MonoBehaviour
{
    public GameObject groundPrefabTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            SpawnGround();
        }
        if (other.CompareTag("Destroy"))
        {
            Destroyground();
        }
    }
    void SpawnGround()
    {
        // Instantiate a new ground piece at the desired position
        Vector3 spawnPosition = transform.position + new Vector3(0, 0, 70); // Adjust the spawn position as needed
        Instantiate(groundPrefabTrigger, spawnPosition, Quaternion.identity);

    }
    void Destroyground()
    {
        //Destroy the ground after 20 seconds each
        Destroy(groundPrefabTrigger);
    }
}
