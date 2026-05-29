using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Static utility — call via Unity MCP editor_invoke_method.
/// Usage: className="LevelFixer", methodName="FixAll"
/// </summary>
public static class LevelFixer
{
    // ── Entry point ────────────────────────────────────────────────────────
    public static void FixAll()
    {
        FixGroundTiles();
        FixSnakes();
        FixCars();
        FixPickupsAndObstacles();
        FixPlayerSpheres();
        MarkSceneDirty();
        Debug.Log("LevelFixer: all fixes applied.");
    }

    // ── Ground retiling ────────────────────────────────────────────────────
    public static void FixGroundTiles()
    {
        GameObject original = GameObject.Find("Ground");
        if (original == null) { Debug.LogError("LevelFixer: 'Ground' not found!"); return; }

        // Reset original tile to its proper single-tile dimensions
        const float tileSizeZ = 56.58871f;
        const float startZ    = 25.3f;
        const float posX      = 1.4f;
        const float scaleX    = 50.8371429f;

        original.transform.position   = new Vector3(posX, 0f, startZ);
        original.transform.localScale = new Vector3(scaleX, 1f, tileSizeZ);

        // Remove any previously created fix tiles
        for (int i = 1; i <= 25; i++)
        {
            GameObject old = GameObject.Find("Ground_Tile_" + i);
            if (old != null) Object.DestroyImmediate(old);
        }

        // Tile forward: original ends at ~Z=53.6; Ender1 is at Z=1016
        int count = Mathf.CeilToInt((1030f - startZ) / tileSizeZ); // ≈ 18

        for (int i = 1; i <= count; i++)
        {
            float newZ  = startZ + tileSizeZ * i;
            GameObject tile = Object.Instantiate(original);
            tile.name = "Ground_Tile_" + i;
            tile.transform.SetParent(null);
            tile.transform.position   = new Vector3(posX, 0f, newZ);
            tile.transform.localScale = new Vector3(scaleX, 1f, tileSizeZ);

            // Strip the trigger child – only the original tile needs it
            Transform triggerChild = tile.transform.Find("GameObject");
            if (triggerChild != null) Object.DestroyImmediate(triggerChild.gameObject);
        }

        Debug.Log($"LevelFixer: Ground reset + {count} tiles added. Coverage Z≈-3 → Z≈{startZ + tileSizeZ * (count + 0.5f):F0}");
    }

    // ── Snake fixes ────────────────────────────────────────────────────────
    public static void FixSnakes()
    {
        FixSnake("Snakes",    71f,  199f, 0.3f);   // keep original X/Z, land on ground
        FixSnake("Snakes (1)", 5f,  700f, 0.3f);   // move inside level bounds
    }

    static void FixSnake(string goName, float x, float z, float animSpeed)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"LevelFixer: '{goName}' not found."); return; }

        go.transform.position = new Vector3(x, 2.1f, z);

        Animator anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            anim.applyRootMotion = false;   // prevent animation from overriding world position
            anim.speed           = animSpeed;
        }

        Debug.Log($"LevelFixer: {goName} → Y=2.1, Z={z}, rootMotion=OFF, Animator.speed={animSpeed}");
    }

    // ── Cars ───────────────────────────────────────────────────────────────
    public static void FixCars()
    {
        // CarLeft / CarRight – correct Y, keep their Z (within level)
        SetY("CarLeft",  2.1f);
        SetY("CarRight", 2.1f);

        // CarLeft(1) / CarRight(1) – correct Y AND move into level bounds
        // (they were at Z≈1704/1720 which is past Ender1 at Z=1016)
        SetPos("CarLeft (1)",  109.8f, 2.1f, 450f);
        SetPos("CarRight (1)", -101.4f, 2.1f, 600f);
    }

    // ── Pickups & trigger volumes ──────────────────────────────────────────
    public static void FixPickupsAndObstacles()
    {
        SetY("Pick1 - speed",   2.1f);   // was 3.702 (floating)
        SetY("Pick4 - herbs",   2.1f);   // was 1.61  (below ground)
        SetY("Pick5 - Materials", 2.1f); // was 1.55  (below ground)
        SetY("Heat&Disease",    3.0f);   // was 1.1   (below player collider; raise to ~player centre)

        // Duplicate material pickups — were at Y≈1.15/1.56 (below ground)
        // AND at Z=-24/-30 (before level starts); move them into the playable area
        SetPos("Pick5 - Materials (1)", 12.18f, 2.1f, 120f);
        SetPos("Pick5 - Materials (2)", 15.37f, 2.1f, 280f);
    }

    static void SetPos(string goName, float x, float y, float z)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"LevelFixer: '{goName}' not found."); return; }
        go.transform.position = new Vector3(x, y, z);
        Debug.Log($"LevelFixer: {goName} → ({x}, {y}, {z})");
    }

    static void SetY(string goName, float y)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"LevelFixer: '{goName}' not found."); return; }
        Vector3 p = go.transform.position;
        go.transform.position = new Vector3(p.x, y, p.z);
        Debug.Log($"LevelFixer: {goName} Y → {y}");
    }

    // ── Level 2 fixes ──────────────────────────────────────────────────────
    /// <summary>
    /// Run this while Level2.unity is open.
    /// Fixes obstacle2-pits floating + corrects all LaneSpawn Y values so
    /// dynamically spawned obstacles land on the ground surface (Y=0.5).
    /// </summary>
    public static void FixLevel2()
    {
        // ── obstacle2-pits ─────────────────────────────────────────────────
        // Mesh half-height in world space = 0.18 u.
        // Ground surface Y = 0.5 (top of Ground mesh, scale=1).
        // Center must be at 0.5 + 0.18 = 0.68 so the BOTTOM is flush with ground.
        // The flat/bumpy pit texture then straddles the surface (natural look).
        FixObstacle2Pits();

        // ── LaneSpawn markers ──────────────────────────────────────────────
        // Lanemanager2 uses laneSpawnsPositions[i].position.y as the spawn
        // center-Y for every dynamically instantiated obstacle.
        // Correct ground surface Y = 0.5; obstacle2-pits half-height = 0.18.
        // Setting spawn Y = 0.68 places the obstacle bottom exactly on the surface.
        string[] laneNames = { "LaneSpawn1", "LaneSpawn2", "LaneSpawn3", "LaneSpawn4" };
        const float spawnY = 0.68f;
        foreach (string ln in laneNames)
        {
            GameObject ls = GameObject.Find(ln);
            if (ls == null) continue;
            Vector3 p = ls.transform.position;
            ls.transform.position = new Vector3(p.x, spawnY, p.z);
            Debug.Log($"LevelFixer: {ln} Y → {spawnY}");
        }

        MarkSceneDirty();
        Debug.Log("LevelFixer.FixLevel2: done.");
    }

    public static void FixObstacle2Pits()
    {
        GameObject go = GameObject.Find("obstacle2 - pits");
        if (go == null) { Debug.LogWarning("LevelFixer: 'obstacle2 - pits' not found."); return; }

        // Ground surface = Y 0.5; obstacle half-height = 0.18 → center at 0.68
        const float groundSurface  = 0.5f;
        const float meshHalfHeight = 0.18f;
        float targetY = groundSurface + meshHalfHeight;   // 0.68

        Vector3 p = go.transform.position;
        go.transform.position = new Vector3(p.x, targetY, p.z);
        Debug.Log($"LevelFixer: obstacle2 - pits Y → {targetY}  (bottom flush at Y={groundSurface})");
    }

    // ── Player Sphere fix — hide renderer on unnamed default spheres ───────
    public static void FixPlayerSpheres()
    {
        // Spheres named "Sphere", "Sphere (1)" … under Player/Female have no
        // material assigned so they render as grey blobs above the character.
        // If they are pure-collider helpers (triggers / hit-boxes) we just
        // disable the renderer; the SphereCollider (if any) is left intact.
        string[] sphereNames = { "Sphere", "Sphere (1)", "Sphere (2)", "Sphere (3)" };

        GameObject playerGO = GameObject.Find("Player");
        if (playerGO == null) { Debug.LogWarning("LevelFixer: 'Player' not found."); return; }

        int hidden = 0;
        foreach (string sName in sphereNames)
        {
            // Search recursively through Player hierarchy
            Transform found = playerGO.transform.Find("Female/" + sName);
            if (found == null)
            {
                // Try a broad search under the whole player
                foreach (Transform t in playerGO.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == sName) { found = t; break; }
                }
            }
            if (found == null) { Debug.Log($"LevelFixer: '{sName}' not found under Player."); continue; }

            MeshRenderer mr = found.GetComponent<MeshRenderer>();
            if (mr == null) { Debug.Log($"LevelFixer: '{sName}' has no MeshRenderer — skipping."); continue; }

            // Check whether a non-default material is assigned
            bool hasMaterial = mr.sharedMaterial != null &&
                               mr.sharedMaterial.name != "Default-Material" &&
                               mr.sharedMaterial.name != "Default Material";

            if (!hasMaterial)
            {
                mr.enabled = false;
                hidden++;
                Debug.Log($"LevelFixer: Hidden grey renderer on Player/{found.name} (no real material).");
            }
            else
            {
                Debug.Log($"LevelFixer: '{sName}' already has material '{mr.sharedMaterial.name}' — left as-is.");
            }
        }
        Debug.Log($"LevelFixer: FixPlayerSpheres complete — {hidden} renderer(s) hidden.");
    }

    // ── Diagnostic: list all objects floating above Y threshold ───────────
    public static void FindFloating()
    {
        // Scan every renderer in scene — catches both root and child objects
        Renderer[] renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int found = 0;
        foreach (var r in renderers)
        {
            float y = r.transform.position.y;
            if (y > 3.5f)
            {
                string path = GetPath(r.transform);
                Debug.Log($"[FLOATING] {path}  Y={y:F2}  Z={r.transform.position.z:F0}");
                found++;
            }
        }
        Debug.Log($"FindFloating: {found} renderer(s) above Y=3.5");
    }

    static string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }

    // ── Mark scene dirty so Unity offers Save ─────────────────────────────
    static void MarkSceneDirty()
    {
#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
    }
}
