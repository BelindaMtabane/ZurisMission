using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    static float LaneX(int laneIndex)
    {
        return LevelLanes.X(laneIndex);
    }

    public GameObject[] spawnObjects;
    public int spawnCount = 4;

    public void SpawnGameObjects(GameObject ground)
    {
        if (SceneCatalog.IsRunnerScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
        {
            return;
        }

        if (spawnObjects == null || spawnObjects.Length == 0 || ground == null) return;

        int count = Mathf.Clamp(spawnCount, 1, 8);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = spawnObjects[Random.Range(0, spawnObjects.Length)];
            if (prefab == null) continue;

            float laneX = LaneX(Random.Range(0, LevelLanes.Count));
            float zJitter = Random.Range(-28f, 28f);
            Vector3 pos = ground.transform.position + new Vector3(laneX, 1.6f, zJitter);

            GameObject spawned = Instantiate(prefab, pos, Quaternion.identity);
            if (spawned.GetComponent<PickupCollectable>() == null
                && IsPickupTag(spawned.tag))
            {
                spawned.AddComponent<PickupCollectable>();
            }
        }
    }

    static bool IsPickupTag(string tag)
    {
        return tag == "WaterDROP"
            || tag == "DamWaterBUCK"
            || tag == "Materials"
            || tag == "Herbs"
            || tag == "FruitPickup"
            || tag == "SpeedBoast";
    }
}
