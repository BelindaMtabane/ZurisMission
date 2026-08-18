using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroundSpawnner : MonoBehaviour
{
    [Header("Ground")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject groundPrefabTrigger;
    [SerializeField] private Material groundSurfaceMaterial;

    public SpawnObjects spawnObjects;

    [Header("Tile Size (world units via scale)")]
    [SerializeField] private float spawnedTileScaleX = 50.837f;
    [SerializeField] private float spawnedTileScaleY = 1f;
    [SerializeField] private float spawnedTileScaleZ = 200f;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnCheckInterval = 0.25f;
    [SerializeField] private float lookAheadDistance = 280f;
    [SerializeField] private float destroyStartDelay = 120f;
    [SerializeField] private float destroyInterval = 3f;

    readonly Queue<GameObject> spawnedTiles = new Queue<GameObject>();
    GameObject spawnTemplate;
    GameObject seedTile;
    float tileLength;
    float nextSpawnZ;
    float spawnX;
    float spawnY;
    bool timerMode;

    public static bool UsesStreamingTiles(string sceneName)
    {
        return sceneName == "MainGame" || sceneName == "Level2" || sceneName == "Level3";
    }

    public void EnsureGroundLinked()
    {
        ResolveSpawnTemplate();
        ResolveGroundMaterial();
        ApplyStreamingGroundConfig();
        RefreshVisibleGroundTiles();
    }

    void Awake()
    {
        ResolveSpawnTemplate();
    }

    void Start()
    {
        if (spawnObjects == null)
        {
            spawnObjects = FindFirstObjectByType<SpawnObjects>();
        }

        timerMode = UsesStreamingTiles(SceneManager.GetActiveScene().name);

        if (!ResolveSpawnTemplate())
        {
            Debug.LogError("[GroundSpawnner] No ground tile prefab or scene Ground found.");
            return;
        }

        if (seedTile == null)
        {
            seedTile = FindSceneGround();
        }

        ApplyStreamingGroundConfig();

        if (groundSurfaceMaterial == null && seedTile != null)
        {
            Renderer seedRenderer = seedTile.GetComponent<Renderer>();
            if (seedRenderer != null && seedRenderer.sharedMaterial != null)
            {
                groundSurfaceMaterial = seedRenderer.sharedMaterial;
            }
        }

        if (seedTile != null)
        {
            if (!UsesLevelStreamingConfig(SceneManager.GetActiveScene().name))
            {
                CaptureTileDimensionsFromSeed(seedTile);
            }

            spawnX = seedTile.transform.position.x;
            spawnY = seedTile.transform.position.y;
            ApplyTileAppearance(seedTile);
            tileLength = GetConfiguredTileLength();
            float seedCenterZ = seedTile.transform.position.z;
            nextSpawnZ = seedCenterZ + tileLength;
            spawnedTiles.Enqueue(seedTile);
        }
        else
        {
            spawnX = transform.position.x;
            spawnY = transform.position.y;
            nextSpawnZ = transform.position.z;
            tileLength = spawnedTileScaleZ;
        }

        if (timerMode)
        {
            RefreshVisibleGroundTiles();
            StartCoroutine(SpawnLoop());
            StartCoroutine(DestroyLoop());
            Debug.Log($"[GroundSpawnner] Tile size X={spawnedTileScaleX} Y={spawnedTileScaleY} Z={spawnedTileScaleZ}; despawn after {destroyStartDelay}s.");
        }
    }

    void ResolveGroundMaterial()
    {
        if (groundSurfaceMaterial != null) return;

        if (spawnTemplate != null)
        {
            Renderer templateRenderer = spawnTemplate.GetComponent<Renderer>();
            if (templateRenderer != null && templateRenderer.sharedMaterial != null)
            {
                groundSurfaceMaterial = templateRenderer.sharedMaterial;
                return;
            }
        }

        if (seedTile != null)
        {
            Renderer seedRenderer = seedTile.GetComponent<Renderer>();
            if (seedRenderer != null && seedRenderer.sharedMaterial != null)
            {
                groundSurfaceMaterial = seedRenderer.sharedMaterial;
            }
        }
    }

    void RefreshVisibleGroundTiles()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("Ground");
        for (int i = 0; i < tagged.Length; i++)
        {
            ApplyTileAppearance(tagged[i]);
        }

        GameObject sceneGround = GameObject.Find("Ground");
        if (sceneGround != null)
        {
            ApplyTileAppearance(sceneGround);
        }

        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null)
            {
                ApplyTileAppearance(tile);
            }
        }
    }

    void CaptureTileDimensionsFromSeed(GameObject seed)
    {
        Vector3 scale = seed.transform.localScale;
        if (scale.x > 0.01f)
        {
            spawnedTileScaleX = scale.x;
        }

        if (scale.z > 0.01f)
        {
            spawnedTileScaleZ = scale.z;
        }
    }

    bool ResolveSpawnTemplate()
    {
        if (groundTilePrefab != null)
        {
            spawnTemplate = groundTilePrefab;
            return true;
        }

        if (groundPrefabTrigger != null)
        {
            if (IsSceneInstance(groundPrefabTrigger))
            {
                seedTile = groundPrefabTrigger;
            }

            spawnTemplate = groundPrefabTrigger;
            return true;
        }

        GameObject sceneGround = FindSceneGround();
        if (sceneGround == null)
        {
            return false;
        }

        seedTile = sceneGround;
        groundPrefabTrigger = sceneGround;
        spawnTemplate = sceneGround;
        Debug.Log("[GroundSpawnner] Auto-linked scene Ground as spawn template.");
        return true;
    }

    static bool IsSceneInstance(GameObject go)
    {
        return go != null && go.scene.IsValid();
    }

    static GameObject FindSceneGround()
    {
        GameObject tagged = GameObject.FindGameObjectWithTag("Ground");
        if (tagged != null)
        {
            return tagged;
        }

        return GameObject.Find("Ground");
    }

    void OnTriggerEnter(Collider other)
    {
        if (timerMode) return;
        if (other.CompareTag("Trigger"))
        {
            SpawnGround();
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (RunStateManager.Instance == null || RunStateManager.Instance.IsPlaying)
            {
                TrySpawnAheadOfPlayer();
            }

            yield return new WaitForSeconds(spawnCheckInterval);
        }
    }

    void TrySpawnAheadOfPlayer()
    {
        float playerZ = GetPlayerZ();
        float frontEdge = GetTerrainFrontEdgeZ();

        while (frontEdge < playerZ + lookAheadDistance)
        {
            SpawnGround();
            frontEdge = GetTerrainFrontEdgeZ();
        }
    }

    float GetTerrainFrontEdgeZ()
    {
        return nextSpawnZ - tileLength * 0.5f;
    }

    float GetPlayerZ()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform.position.z;

        GameObject player = GameObject.Find("Player");
        return player != null ? player.transform.position.z : nextSpawnZ;
    }

    IEnumerator DestroyLoop()
    {
        yield return new WaitForSeconds(destroyStartDelay);
        while (true)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            if (spawnedTiles.Count > 1)
            {
                GameObject oldest = spawnedTiles.Dequeue();
                if (oldest != null && oldest != seedTile)
                {
                    Debug.Log("[GroundSpawnner] Destroying old tile " + oldest.name);
                    Destroy(oldest);
                }
            }

            yield return new WaitForSeconds(destroyInterval);
        }
    }

    void SpawnGround()
    {
        if (spawnTemplate == null && !ResolveSpawnTemplate())
        {
            Debug.LogError("[GroundSpawnner] Ground prefab is NOT assigned!");
            return;
        }

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, nextSpawnZ);

        GameObject newGround = Instantiate(spawnTemplate, spawnPosition, spawnTemplate.transform.rotation);
        newGround.name = "Ground_" + Mathf.RoundToInt(nextSpawnZ);
        ApplyTileAppearance(newGround);
        nextSpawnZ += tileLength;
        spawnedTiles.Enqueue(newGround);

        if (spawnObjects != null)
        {
            spawnObjects.SpawnGameObjects(newGround);
        }

        Debug.Log("[GroundSpawnner] Spawned tile at " + spawnPosition + " size=(" + spawnedTileScaleX + "," + spawnedTileScaleY + "," + spawnedTileScaleZ + ") length=" + tileLength);
    }

    void ApplyTileAppearance(GameObject tile)
    {
        if (tile == null) return;
        Vector3 scale = new Vector3(spawnedTileScaleX, spawnedTileScaleY, spawnedTileScaleZ);
        tile.transform.localScale = scale;

        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            Level1Ground.ApplyGroundSurface(tile, groundSurfaceMaterial, scale);
            return;
        }

        if (SceneManager.GetActiveScene().name == "Level2")
        {
            Level2Ground.ApplyGroundSurface(tile, groundSurfaceMaterial, scale);
            return;
        }

        if (SceneManager.GetActiveScene().name == "Level3")
        {
            Level3Ground.ApplyGroundSurface(tile, groundSurfaceMaterial, scale);
            return;
        }

        ApplySceneGroundMaterial(tile);
    }

    static bool UsesLevelStreamingConfig(string sceneName)
    {
        return sceneName == "Level3";
    }

    void ApplyStreamingGroundConfig()
    {
        if (!timerMode && !UsesStreamingTiles(SceneManager.GetActiveScene().name)) return;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Level3") return;

        Level3Ground.ConfigureStreamingTile(ref spawnedTileScaleX, ref spawnedTileScaleY, ref spawnedTileScaleZ, ref spawnX, ref spawnY);
        tileLength = Level3Ground.TileLength;

        if (seedTile == null)
        {
            seedTile = FindSceneGround();
        }

        Level3Ground.AlignSeedTile(seedTile, spawnX, spawnY);
    }

    float GetConfiguredTileLength()
    {
        if (spawnedTileScaleZ > 0.01f)
        {
            return spawnedTileScaleZ;
        }

        return GetTileLength(MeasureTileLength(spawnTemplate != null ? spawnTemplate : seedTile));
    }

    void ApplySceneGroundMaterial(GameObject tile)
    {
        if (groundSurfaceMaterial == null) return;

        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = groundSurfaceMaterial;
        }
    }

    float MeasureTileLength(GameObject tile)
    {
        Collider col = tile.GetComponent<Collider>();
        if (col != null)
        {
            float z = col.bounds.size.z;
            if (z > 0.01f) return z;
        }

        Renderer rend = tile.GetComponent<Renderer>();
        if (rend != null)
        {
            float z = rend.bounds.size.z;
            if (z > 0.01f) return z;
        }

        return spawnedTileScaleZ;
    }

    float GetTileLength(float measured)
    {
        if (measured > 0.01f)
        {
            return measured;
        }

        return spawnedTileScaleZ;
    }
}
