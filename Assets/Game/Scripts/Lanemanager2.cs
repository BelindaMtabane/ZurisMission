using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("UI – Bucket Bar")]
    public RectTransform bucketBarRoot;   // drag the BucketBar parent object here

    private GameObject[] currentObstacleSet;

    private void Start()
    {
        if (hudControls == null)
            hudControls = FindFirstObjectByType<HUDControls>();

        LevelDifficulty();
        StartCoroutine(AutoSpawnLoop());
    }

    void LevelDifficulty()
    {
        currentObstacleSet = mediumObstacles;

        // Hide the 3 individual lane buttons
        if (button2 != null) button2.gameObject.SetActive(false);
        if (button3 != null) button3.gameObject.SetActive(false);
        if (button4 != null) button4.gameObject.SetActive(false);

        // Repurpose button1 as the single Release button
        if (button1 != null)
        {
            button1.gameObject.SetActive(true);
            button1.onClick.RemoveAllListeners();
            button1.onClick.AddListener(SpawnRandomObstacle);

            // Red background, white text
            var btnImage = button1.GetComponent<UnityEngine.UI.Image>();
            if (btnImage != null) btnImage.color = Color.red;

            var label = button1.GetComponentInChildren<TMPro.TMP_Text>();
            if (label != null)
            {
                label.text = "Release Obstacle";
                label.color = Color.white;
            }

            // TOP-RIGHT of screen, placed below the pause panel (~90 px gap)
            RectTransform rt = button1.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin        = new Vector2(1f, 1f);
                rt.anchorMax        = new Vector2(1f, 1f);
                rt.pivot            = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-20f, -110f);  // -110 clears the pause panel above
                rt.sizeDelta        = new Vector2(200f, 60f);
            }
        }

        // MIDDLE-RIGHT of screen
        if (bucketBarRoot != null)
        {
            bucketBarRoot.anchorMin        = new Vector2(1f, 0.5f);
            bucketBarRoot.anchorMax        = new Vector2(1f, 0.5f);
            bucketBarRoot.pivot            = new Vector2(1f, 0.5f);
            bucketBarRoot.anchoredPosition = new Vector2(-20f, 0f);
            bucketBarRoot.sizeDelta        = new Vector2(110f, 40f);
        }
        // Info feed is anchored to BOTTOM-RIGHT — handled inside GameInfoUI.Bootstrap()

        Debug.Log("Level 2 Loaded — single release button active");
    }

    // ── Auto-spawn every 5 seconds — NO warning effect ────────────────────
    private IEnumerator AutoSpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (laneSpawnsPositions != null && laneSpawnsPositions.Length > 0)
            {
                int lane = Random.Range(0, laneSpawnsPositions.Length);
                SpawnObstacle(lane, false);   // silent auto-spawn
            }
            if (hudControls != null) hudControls.WaterMoveManager();
        }
    }

    // ── Button entry point — shows warning effect ──────────────────────────
    public void SpawnRandomObstacle()
    {
        if (laneSpawnsPositions == null || laneSpawnsPositions.Length == 0) return;
        int lane = Random.Range(0, laneSpawnsPositions.Length);
        SpawnObstacle(lane, true);   // warning only on manual button press
    }

    // ── Core spawn ─────────────────────────────────────────────────────────
    public void SpawnObstacle(int laneIndex, bool showWarning = false)
    {
        if (currentObstacleSet == null || currentObstacleSet.Length == 0)
        {
            Debug.Log("No obstacles assigned!");
            return;
        }
        if (laneSpawnsPositions == null || laneIndex >= laneSpawnsPositions.Length)
        {
            Debug.Log("Lane spawn missing or invalid index!");
            return;
        }
        if (player == null)
        {
            Debug.Log("Player not assigned!");
            return;
        }

        float laneX  = laneSpawnsPositions[laneIndex].position.x;
        float laneY  = laneSpawnsPositions[laneIndex].position.y;
        float spawnZ = player.position.z + spawnDistance;

        // Only show the warning tile when the Release Obstacle button is pressed
        if (showWarning)
        {
            float damageLevel = Random.Range(0f, 1f);
            LaneWarningEffect.Spawn(laneX, laneY, spawnZ, damageLevel);
            Debug.Log($"Warning tile spawned for lane {laneIndex} (damage {damageLevel:F2})");
        }

        // Spawn the actual obstacle after a brief delay
        int obstacleIndex = Random.Range(0, currentObstacleSet.Length);
        Vector3 spawnPos  = new Vector3(laneX, laneY, spawnZ);
        StartCoroutine(SpawnAfterDelay(currentObstacleSet[obstacleIndex], spawnPos, 0.5f));

        Debug.Log($"Obstacle queued for lane {laneIndex}");
    }

    private IEnumerator SpawnAfterDelay(GameObject prefab, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(prefab, position, Quaternion.identity);
    }
}
