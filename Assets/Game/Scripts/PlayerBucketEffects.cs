using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Live visual feedback on the player's carried Bucket prop: a water-level
/// disc that rises inside the bucket as BucketWater fills, plus a burst of
/// droplets spilling over the rim whenever an obstacle knocks water out of
/// the bucket. Auto-creates itself.
/// </summary>
public class PlayerBucketEffects : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "MainGame" && sceneName != "Level2") return;
        if (FindFirstObjectByType<PlayerBucketEffects>() != null) return;
        new GameObject("PlayerBucketEffects").AddComponent<PlayerBucketEffects>();
    }

    HUDControls _hud;
    Transform   _bucket;
    GameObject  _waterDisc;
    float       _emptyLocalY, _fullLocalY;

    static readonly Color WaterColor = new Color(0.20f, 0.55f, 0.90f);

    void Start()
    {
        StartCoroutine(SetupWhenReady());
    }

    IEnumerator SetupWhenReady()
    {
        // Player/bucket may spawn a frame or two after this component does.
        for (int i = 0; i < 60 && _bucket == null; i++)
        {
            GameObject bucketGO = GameObject.Find("Bucket");
            if (bucketGO != null) _bucket = bucketGO.transform;
            _hud = FindFirstObjectByType<HUDControls>();
            if (_bucket != null && _hud != null) break;
            yield return null;
        }
        if (_bucket == null) yield break;

        BuildWaterDisc();
    }

    void BuildWaterDisc()
    {
        Renderer rend = _bucket.GetComponentInChildren<Renderer>();
        Bounds b = rend != null ? rend.bounds : new Bounds(_bucket.position, Vector3.one * 0.5f);

        float diameter  = Mathf.Min(b.size.x, b.size.z) * 0.72f;
        Vector3 emptyPos = new Vector3(b.center.x, b.min.y + b.size.y * 0.18f, b.center.z);
        Vector3 fullPos  = new Vector3(b.center.x, b.min.y + b.size.y * 0.78f, b.center.z);

        _waterDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _waterDisc.name = "BucketWaterLevel";
        var col = _waterDisc.GetComponent<Collider>();
        if (col != null) Destroy(col);
        _waterDisc.transform.localScale = new Vector3(diameter, 0.015f, diameter);
        _waterDisc.transform.position   = emptyPos;
        _waterDisc.transform.SetParent(_bucket, true);
        TintPrimitive(_waterDisc, WaterColor);

        _emptyLocalY = _bucket.InverseTransformPoint(emptyPos).y;
        _fullLocalY  = _bucket.InverseTransformPoint(fullPos).y;
        _waterDisc.SetActive(false);
    }

    float _lastBucketWater = -1f;

    void Update()
    {
        if (_bucket == null) return;
        if (_hud == null) { _hud = FindFirstObjectByType<HUDControls>(); return; }

        float bucketWater = _hud.BucketWater;
        float t = Mathf.Clamp01(bucketWater / 100f);

        if (_waterDisc != null)
        {
            _waterDisc.SetActive(t > 0.02f);
            if (t > 0.02f)
            {
                var lp = _waterDisc.transform.localPosition;
                lp.y = Mathf.Lerp(_emptyLocalY, _fullLocalY, t);
                _waterDisc.transform.localPosition = lp;
            }
        }

        // Bucket water dropping (obstacle bump spilling it) triggers a
        // burst of droplets spilling over the rim.
        if (_lastBucketWater >= 0f && bucketWater < _lastBucketWater - 0.01f)
        {
            SpawnDropletBurst();
        }
        _lastBucketWater = bucketWater;
    }

    void SpawnDropletBurst()
    {
        int count = Random.Range(4, 7);
        for (int i = 0; i < count; i++)
        {
            SpawnDroplet();
        }
    }

    void SpawnDroplet()
    {
        if (_bucket == null) return;
        Renderer rend = _bucket.GetComponentInChildren<Renderer>();
        if (rend == null) return;
        Bounds b = rend.bounds;

        Vector2 rand = Random.insideUnitCircle;
        Vector3 spawnPos = new Vector3(
            b.center.x + rand.x * b.size.x * 0.4f,
            b.min.y + b.size.y * 0.75f,
            b.center.z + rand.y * b.size.z * 0.4f);

        var drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        drop.name = "BucketDroplet";
        var col = drop.GetComponent<Collider>();
        if (col != null) Destroy(col);
        float size = Random.Range(0.03f, 0.06f);
        drop.transform.localScale = Vector3.one * size;
        drop.transform.position   = spawnPos;
        TintPrimitive(drop, WaterColor);

        Vector3 outward = new Vector3(rand.x, 0.5f, rand.y).normalized * Random.Range(0.6f, 1.2f);
        StartCoroutine(AnimateDroplet(drop, outward));
    }

    static IEnumerator AnimateDroplet(GameObject drop, Vector3 initialVelocity)
    {
        Vector3 pos = drop.transform.position;
        Vector3 vel = initialVelocity;
        float life = 0f;
        const float maxLife = 0.6f;

        while (drop != null && life < maxLife)
        {
            float dt = Time.deltaTime;
            vel.y -= 2.2f * dt;
            pos += vel * dt;
            drop.transform.position = pos;

            float shrink = 1f - (life / maxLife);
            drop.transform.localScale = Vector3.one * (shrink * shrink);

            life += dt;
            yield return null;
        }

        if (drop != null) Destroy(drop);
    }

    // One material per distinct colour, shared across every droplet/disc —
    // avoids a fresh Material instance (and shader-variant compile) per spawn.
    static readonly Dictionary<Color, Material> materialCache = new();
    static void TintPrimitive(GameObject go, Color c)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null) return;
        if (!materialCache.TryGetValue(c, out Material mat) || mat == null)
        {
            mat = new Material(rend.sharedMaterial);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            materialCache[c] = mat;
        }
        rend.sharedMaterial = mat;
    }
}
