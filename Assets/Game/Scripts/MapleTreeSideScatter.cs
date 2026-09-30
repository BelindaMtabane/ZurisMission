using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HQ Autumn Dry Maple Trees along the roadside band to fill visual gaps (Resources/MapleTrees).
/// </summary>
public static class MapleTreeSideScatter
{
    const string ResourcesFolder = "MapleTrees/";
    const float TreeFootprint = 4.2f;

    static readonly string[] EditorSourceRoots =
    {
        "Assets/klen/Prefab",
        "Assets/klen",
        "Assets/HQ Autumn Dry Maple Trees",
        "Assets/HQ Autumn Dry Maple Trees/Prefabs"
    };

    static readonly Dictionary<string, GameObject> PrefabCache = new();
    static string[] _variantNames;

    public static bool HasAnyPrefabs => GetVariantNames().Length > 0;

    public static void FillRoadsideGaps(Transform parent, float startZ, float endZ, float groundY, int count)
    {
        if (parent == null || count <= 0 || endZ <= startZ) return;
        string[] variants = GetVariantNames();
        if (variants.Length == 0) return;

        LevelLanes.GetRoadsideSceneryBand(out float inner, out float outer);
        float greenMin = LevelLanes.MinAbsXGreenFoliage() + 1.2f;
        inner = Mathf.Max(inner, greenMin);
        float mid = inner + 3.8f;

        Transform root = parent.Find("MapleSideTrees");
        if (root == null)
        {
            var host = new GameObject("MapleSideTrees");
            host.transform.SetParent(parent, false);
            root = host.transform;
        }

        var placed = new List<Vector2>(count);
        float span = endZ - startZ;
        int spawned = 0;

        for (int slot = 0; slot < count && spawned < count; slot++)
        {
            float t = (slot + 0.5f) / count;
            float baseZ = startZ + t * span;
            bool left = slot % 2 == 0;

            for (int attempt = 0; attempt < 14; attempt++)
            {
                float zJitter = (Hash01(slot * 41 + attempt * 13) - 0.5f) * (span / count) * 0.55f;
                float z = baseZ + zJitter;

                float bandRoll = Hash01(slot * 29 + attempt);
                float xBand = bandRoll < 0.55f
                    ? Mathf.Lerp(inner, mid, Hash01(slot * 19 + attempt))
                    : Mathf.Lerp(mid + 0.35f, outer - 0.8f, Hash01(slot * 23 + attempt + 2));
                xBand = Mathf.Max(xBand, greenMin);

                float x = LevelLanes.EnforceRoadsideX(left ? -xBand : xBand, greenFoliage: true);
                if (!IsSpotFree(placed, x, z)) continue;

                string variant = variants[(slot + attempt) % variants.Length];
                GameObject prefab = LoadPrefab(variant);
                if (prefab == null) break;

                GameObject tree = Object.Instantiate(prefab, root);
                tree.name = $"Maple_{variant}_{spawned}";
                NaturePackVisuals.EnsureSceneObjectMaterials(tree);
                tree.transform.position = new Vector3(x, groundY, z);
                tree.transform.rotation = Quaternion.Euler(0f, Hash01(slot * 37 + attempt) * 360f, 0f);

                float scale = 0.92f + Hash01(slot * 11 + attempt) * 0.38f;
                tree.transform.localScale = Vector3.one * scale;
                FitHeightAndGround(tree.transform, groundY, 7.5f + Hash01(slot * 7 + attempt) * 5.5f);

                StripColliders(tree);
                EnableWindAnimation(tree, slot + attempt);
                placed.Add(new Vector2(x, z));
                spawned++;
                break;
            }
        }
    }

    static string[] GetVariantNames()
    {
        if (_variantNames != null && _variantNames.Length > 0) return _variantNames;

        Object[] all = Resources.LoadAll(ResourcesFolder.TrimEnd('/'), typeof(GameObject));
        if (all != null && all.Length > 0)
        {
            var names = new List<string>(all.Length);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] is GameObject go && go != null)
                    names.Add(go.name);
            }

            _variantNames = names.ToArray();
            if (_variantNames.Length > 0) return _variantNames;
        }

#if UNITY_EDITOR
        _variantNames = DiscoverEditorPrefabNames();
#endif
        return _variantNames ?? System.Array.Empty<string>();
    }

#if UNITY_EDITOR
    static string[] DiscoverEditorPrefabNames()
    {
        var names = new HashSet<string>();
        for (int r = 0; r < EditorSourceRoots.Length; r++)
        {
            string folder = EditorSourceRoots[r];
            if (!UnityEditor.AssetDatabase.IsValidFolder(folder)) continue;
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;
                names.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }

        if (names.Count == 0)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("Maple t:Prefab");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.IndexOf("maple", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                names.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }

        var arr = new string[names.Count];
        names.CopyTo(arr);
        return arr;
    }
#endif

    static GameObject LoadPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (PrefabCache.TryGetValue(name, out GameObject cached) && cached != null) return cached;

        GameObject prefab = Resources.Load<GameObject>(ResourcesFolder + name);
#if UNITY_EDITOR
        if (prefab == null)
        {
            for (int r = 0; r < EditorSourceRoots.Length; r++)
            {
                string path = $"{EditorSourceRoots[r]}/{name}.prefab";
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) break;
            }
        }
#endif
        PrefabCache[name] = prefab;
        return prefab;
    }

    static bool IsSpotFree(List<Vector2> placed, float x, float z)
    {
        for (int i = 0; i < placed.Count; i++)
        {
            float dx = placed[i].x - x;
            float dz = placed[i].y - z;
            float min = TreeFootprint * 1.35f;
            if (dx * dx + dz * dz < min * min) return false;
        }

        return true;
    }

    static void FitHeightAndGround(Transform t, float groundY, float targetHeight)
    {
        Bounds b = WorldBounds(t.gameObject);
        float h = b.size.y;
        if (h > 0.02f && targetHeight > 0f)
            t.localScale *= targetHeight / h;

        b = WorldBounds(t.gameObject);
        t.position += Vector3.up * (groundY - b.min.y);
    }

    static Bounds WorldBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);
        return b;
    }

    static void StripColliders(GameObject go)
    {
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] != null) Object.Destroy(cols[i]);
        }
    }

    /// <summary>HQ maples use an Animator "Wind" clip on the skinned foliage — keep it running.</summary>
    static void EnableWindAnimation(GameObject tree, int seed)
    {
        Animator[] animators = tree.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null) continue;
            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.speed = 0.62f + Hash01(seed * 29 + 3) * 0.45f;
            animator.Play("Wind", 0, Hash01(seed * 41 + 7));
            animator.Update(0f);
        }

        SkinnedMeshRenderer[] skinned = tree.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skinned.Length; i++)
        {
            if (skinned[i] != null)
                skinned[i].updateWhenOffscreen = true;
        }
    }

    static float Hash01(int value)
    {
        uint x = (uint)value;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        return (x & 0xFFFF) / 65535f;
    }
}
