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
        return SceneCatalog.IsRunnerScene(sceneName);
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
            RunnerEnvironmentStreamer.Ensure()?.SpawnChunk(seedCenterZ, tileLength);
        }
        else
        {
            spawnX = transform.position.x;
            spawnY = 0f;
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
        float endZ = GetSceneEndZ();

        while (frontEdge < playerZ + lookAheadDistance)
        {
            if (nextSpawnZ > endZ + tileLength * 0.25f) break;

            SpawnGround();
            frontEdge = GetTerrainFrontEdgeZ();
        }
    }

    static float GetSceneEndZ()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == SceneCatalog.Level2) return Level2Progress.EndZ;
        if (sceneName == SceneCatalog.Level3) return Level3Progress.EndZ;
        if (sceneName == SceneCatalog.MainGame) return Level1Progress.EndZ;
        return float.PositiveInfinity;
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
        RunnerEnvironmentStreamer.Ensure()?.SpawnChunk(spawnPosition.z, tileLength);

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

        if (SceneCatalog.IsLevel1(SceneManager.GetActiveScene().name))
        {
            Level1Ground.ApplyGroundSurface(tile, groundSurfaceMaterial, scale);
            return;
        }

        if (SceneCatalog.IsLevel2(SceneManager.GetActiveScene().name))
        {
            Level2Ground.ApplyGroundSurface(tile, groundSurfaceMaterial, scale);
            return;
        }

        if (SceneCatalog.IsLevel3(SceneManager.GetActiveScene().name))
        {
            Level3Ground.ApplyGroundSurface(tile, groundSurfaceMaterial, scale);
            return;
        }

        ApplySceneGroundMaterial(tile);
    }

    static bool UsesLevelStreamingConfig(string sceneName)
    {
        return SceneCatalog.IsLevel2(sceneName) || SceneCatalog.IsLevel3(sceneName);
    }

    void ApplyStreamingGroundConfig()
    {
        if (!timerMode && !UsesStreamingTiles(SceneManager.GetActiveScene().name)) return;

        string sceneName = SceneManager.GetActiveScene().name;
        if (SceneCatalog.IsLevel1(sceneName) || SceneCatalog.IsLevel2(sceneName) || SceneCatalog.IsLevel3(sceneName))
        {
            spawnedTileScaleY = 1f;
            if (seedTile == null)
            {
                seedTile = FindSceneGround();
            }

            if (seedTile != null)
            {
                Vector3 seedPos = seedTile.transform.position;
                seedTile.transform.position = new Vector3(seedPos.x, 0f, seedPos.z);
                spawnX = seedTile.transform.position.x;
            }

            spawnY = 0f;
        }

        if (SceneCatalog.IsLevel3(sceneName))
        {
            Level3Ground.ConfigureStreamingTile(ref spawnedTileScaleX, ref spawnedTileScaleY, ref spawnedTileScaleZ, ref spawnX, ref spawnY);
            tileLength = Level3Ground.TileLength;

            if (seedTile == null)
            {
                seedTile = FindSceneGround();
            }

            Level3Ground.AlignSeedTile(seedTile, spawnX, spawnY);
        }

        if (SceneCatalog.IsLevel2(sceneName) || SceneCatalog.IsLevel3(sceneName))
        {
            destroyStartDelay = 60f;
            destroyInterval = 60f;
        }
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

/// <summary>
/// Streams lightweight roadside scenery with active ground tiles instead of
/// loading the scene's large manually placed Environment hierarchy upfront.
/// </summary>
public sealed class RunnerEnvironmentStreamer : MonoBehaviour
{
    static RunnerEnvironmentStreamer instance;
    readonly Dictionary<int, GameObject> chunks = new Dictionary<int, GameObject>();
    Transform player;
    float cleanupTimer;

    public static RunnerEnvironmentStreamer Ensure()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!SceneCatalog.IsLevel2(scene) && !SceneCatalog.IsLevel3(scene)) return null;
        if (instance != null) return instance;

        instance = FindFirstObjectByType<RunnerEnvironmentStreamer>();
        if (instance == null)
            instance = new GameObject("RunnerEnvironmentStreamer").AddComponent<RunnerEnvironmentStreamer>();
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DisableManualEnvironment();
        GroundSceneNpcs();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void SpawnChunk(float centerZ, float length)
    {
        int key = Mathf.RoundToInt(centerZ);
        if (chunks.ContainsKey(key)) return;

        float half = Mathf.Max(35f, length * 0.5f);
        float startZ = centerZ - half + 8f;
        float endZ = centerZ + half - 8f;
        float levelEnd = SceneCatalog.IsLevel3(SceneCatalog.ActiveName)
            ? Level3Progress.EndZ
            : Level2Progress.EndZ;
        startZ = Mathf.Max(0f, startZ);
        endZ = Mathf.Min(levelEnd, endZ);
        if (endZ <= startZ) return;

        GameObject chunk = new GameObject($"EnvironmentChunk_{key}");
        chunks[key] = chunk;

        bool level3 = SceneCatalog.IsLevel3(SceneCatalog.ActiveName);
        float groundY = level3 ? Level3Ground.SurfaceY : Level2Ground.SurfaceY;
        BuildRoadsideLand(chunk.transform, centerZ, length, groundY, level3);
        NaturePackVisuals.ScatterRoadside(chunk.transform, startZ, endZ, groundY, level3 ? 18 : 14);
        NaturePackVisuals.ScatterSideTreesAndGrass(
            chunk.transform, startZ, endZ, groundY,
            level3 ? 12 : 10,
            level3 ? 32 : 26);
        if (level3)
        {
            NaturePackVisuals.ScatterFlowers(chunk.transform, startZ, endZ, groundY, 30);
            NaturePackVisuals.ScatterMudMonsterScenery(chunk.transform, startZ, endZ, groundY, 6);
        }
        VillageSideHouses.Scatter(chunk.transform, startZ, endZ, groundY, level3 ? 12 : 5);
        NaturePackVisuals.ClearRunnerSceneryColliders();
    }

    static void BuildRoadsideLand(Transform parent, float centerZ, float length, float groundY, bool level3)
    {
        LevelLanes.ConfigureForActiveScene();
        float pathCenter = LevelLanes.PathCenterX;
        const float roadHalfWidth = 25.2f;
        const float landOuterExtent = 140f;
        float sideWidth = landOuterExtent - roadHalfWidth;
        float sideCenterOffset = roadHalfWidth + sideWidth * 0.5f;

        Color level2Land = new Color(0.43f, 0.55f, 0.30f);
        Color level3Land = new Color(0.16f, 0.34f, 0.18f);
        Color landColor = level3 ? level3Land : level2Land;

        BuildLandStrip(parent, "RoadsideLand_Left",
            new Vector3(pathCenter - sideCenterOffset, groundY - 0.5f, centerZ),
            new Vector3(sideWidth, 1f, length + 2f), landColor);
        BuildLandStrip(parent, "RoadsideLand_Right",
            new Vector3(pathCenter + sideCenterOffset, groundY - 0.5f, centerZ),
            new Vector3(sideWidth, 1f, length + 2f), landColor);

        for (int side = 0; side < 2; side++)
        {
            float sign = side == 0 ? -1f : 1f;
            for (int i = 0; i < 2; i++)
            {
                GameObject hill = new GameObject($"DistantRockHill_{side}_{i}");
                hill.transform.SetParent(parent, false);
                hill.transform.position = new Vector3(
                    pathCenter + sign * (57f + i * 14f),
                    groundY,
                    centerZ - length * 0.28f + i * length * 0.53f);
                NaturePackVisuals.AttachRock(hill.transform, (level3 ? 8f : 9.5f) + i * 2f);
            }
        }

        if (level3)
        {
            BuildRoadsideFog(parent, pathCenter, centerZ, length, groundY);
            BuildRoadFog(parent, pathCenter, centerZ, length, groundY);
        }
    }

    static void BuildRoadsideFog(Transform parent, float pathCenter, float centerZ, float length, float groundY)
    {
        Material fogSource = Resources.Load<Material>("Weather/FogWhite");
        if (fogSource == null) return;

        for (int side = 0; side < 2; side++)
        {
            float sign = side == 0 ? -1f : 1f;
            GameObject fogObject = new GameObject(side == 0 ? "RoadsideFog_Left" : "RoadsideFog_Right");
            fogObject.transform.SetParent(parent, false);
            fogObject.transform.position = new Vector3(pathCenter + sign * 42f, groundY + 1.6f, centerZ);

            ParticleSystem fog = fogObject.AddComponent<ParticleSystem>();
            var main = fog.main;
            main.loop = true;
            main.prewarm = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(9f, 14f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(8f, 16f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.68f, 0.78f, 0.72f, 0.08f),
                new Color(0.82f, 0.88f, 0.84f, 0.18f));
            main.maxParticles = 180;

            var emission = fog.emission;
            emission.rateOverTime = 9f;

            var shape = fog.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(22f, 2.5f, length * 0.94f);

            var velocity = fog.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f); // all axes must share a mode

            var noise = fog.noise;
            noise.enabled = true;
            noise.strength = 0.65f;
            noise.frequency = 0.12f;
            noise.scrollSpeed = 0.08f;

            ParticleSystemRenderer renderer = fogObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(fogSource);
            renderer.sortingFudge = -1f;
            fog.Play(true);
        }
    }

    static void BuildRoadFog(Transform parent, float pathCenter, float centerZ, float length, float groundY)
    {
        GameObject packFog = LoadWhitishFog();
        int patches = 3;
        for (int i = 0; i < patches; i++)
        {
            float z = centerZ - length * 0.32f + i * (length * 0.32f);
            if (packFog != null)
            {
                GameObject fog = Object.Instantiate(packFog, parent);
                fog.name = $"RoadFog_{i}";
                fog.transform.position = new Vector3(pathCenter + (i - 1) * 3.5f, groundY + 0.8f, z);
                fog.transform.localScale = Vector3.one * 6.5f;
                continue;
            }

            Material fogSource = Resources.Load<Material>("Weather/FogWhite");
            if (fogSource == null) return;
            GameObject fogObject = new GameObject($"RoadFog_{i}");
            fogObject.transform.SetParent(parent, false);
            fogObject.transform.position = new Vector3(pathCenter + (i - 1) * 3.2f, groundY + 1.1f, z);
            ParticleSystem fogPs = fogObject.AddComponent<ParticleSystem>();
            var main = fogPs.main;
            main.loop = true;
            main.prewarm = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.28f);
            main.startSize = new ParticleSystem.MinMaxCurve(4.5f, 8.5f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.72f, 0.82f, 0.76f, 0.10f),
                new Color(0.86f, 0.92f, 0.88f, 0.20f));
            main.maxParticles = 90;
            var emission = fogPs.emission;
            emission.rateOverTime = 6f;
            var shape = fogPs.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 1.8f, 18f);
            ParticleSystemRenderer renderer = fogObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(fogSource);
            fogPs.Play(true);
        }
    }

    static GameObject LoadWhitishFog()
    {
        GameObject prefab = Resources.Load<GameObject>("Weather/WhitishFog");
        if (prefab != null) return prefab;
#if UNITY_EDITOR
        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Fog Particles/Prefabs/Whitish Fog.prefab");
#endif
        return prefab;
    }

    static void BuildLandStrip(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = name;
        strip.transform.SetParent(parent, false);
        strip.transform.position = position;
        strip.transform.localScale = scale;

        Collider col = strip.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Renderer rend = strip.GetComponent<Renderer>();
        if (rend == null) return;
        Material material = rend.material;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        material.color = color;
    }

    void Update()
    {
        cleanupTimer -= Time.deltaTime;
        if (cleanupTimer > 0f) return;
        cleanupTimer = 1.5f;

        if (player == null)
        {
            PlayerController controller = FindFirstObjectByType<PlayerController>();
            if (controller != null) player = controller.transform;
        }
        if (player == null) return;

        float cutoff = player.position.z - 280f;
        List<int> remove = null;
        foreach (KeyValuePair<int, GameObject> entry in chunks)
        {
            if (entry.Value == null || entry.Key >= cutoff) continue;
            Destroy(entry.Value);
            remove ??= new List<int>();
            remove.Add(entry.Key);
        }

        if (remove == null) return;
        for (int i = 0; i < remove.Count; i++) chunks.Remove(remove[i]);
    }

    static void DisableManualEnvironment()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null) continue;
            string n = root.name;
            if (n == "Environment" || n == "Scene1" || n == "Road" ||
                n.StartsWith("Road (") || n == "Terrain")
            {
                root.SetActive(false);
            }
        }
    }

    static void GroundSceneNpcs()
    {
        bool level3 = SceneCatalog.IsLevel3(SceneCatalog.ActiveName);
        float groundY = level3 ? Level3Ground.SurfaceY : Level2Ground.SurfaceY;
        Transform[] sceneTransforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        List<Vector3> occupied = new List<Vector3>();

        for (int i = 0; i < sceneTransforms.Length; i++)
        {
            Transform t = sceneTransforms[i];
            if (t == null || !t.name.StartsWith("NPC_")) continue;
            if (t.parent != null && t.parent.name.StartsWith("NPC_")) continue;

            bool duplicate = false;
            for (int p = 0; p < occupied.Count; p++)
            {
                Vector3 delta = occupied[p] - t.position;
                if (delta.x * delta.x + delta.z * delta.z < 0.25f)
                {
                    duplicate = true;
                    break;
                }
            }
            if (duplicate)
            {
                t.gameObject.SetActive(false);
                continue;
            }
            occupied.Add(t.position);

            RoadsideNpcGrounding grounding = t.GetComponent<RoadsideNpcGrounding>();
            if (grounding == null) grounding = t.gameObject.AddComponent<RoadsideNpcGrounding>();
            grounding.Setup(groundY);
        }
    }
}

/// <summary>Keeps roadside village characters standing on the streamed flat ground.</summary>
public sealed class RoadsideNpcGrounding : MonoBehaviour
{
    float targetGroundY;
    float groundedRootY;
    float patrolCenterZ;
    float patrolDirection;
    bool grounded;

    public void Setup(float groundY)
    {
        targetGroundY = groundY;
    }

    void Start()
    {
        patrolCenterZ = transform.position.z;
        patrolDirection = (GetInstanceID() & 1) == 0 ? 1f : -1f;
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] == null) continue;
            animators[i].applyRootMotion = false;
            animators[i].speed = 0.9f;
            string walkState = name.Contains("Female")
                ? "locom_f_basicWalk_30f"
                : "locom_m_basicWalk_30f";
            animators[i].Play(walkState, 0, Mathf.Abs(GetInstanceID() % 100) / 100f);
        }

        Invoke(nameof(GroundOnce), 0.08f);
    }

    void Update()
    {
        if (!grounded) return;

        float nextZ = transform.position.z + patrolDirection * 0.85f * Time.deltaTime;
        if (Mathf.Abs(nextZ - patrolCenterZ) > 7f)
        {
            patrolDirection *= -1f;
            nextZ = Mathf.Clamp(nextZ, patrolCenterZ - 7f, patrolCenterZ + 7f);
        }

        Vector3 p = transform.position;
        p.z = nextZ;
        p.y = groundedRootY;
        transform.position = p;
        transform.rotation = Quaternion.Euler(0f, patrolDirection > 0f ? 0f : 180f, 0f);
    }

    void GroundOnce()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);

        transform.position += Vector3.up * (targetGroundY - bounds.min.y + 0.025f);
        groundedRootY = transform.position.y;
        grounded = true;
    }

    void LateUpdate()
    {
        if (!grounded) return;
        Vector3 p = transform.position;
        p.y = groundedRootY;
        transform.position = p;
    }
}
