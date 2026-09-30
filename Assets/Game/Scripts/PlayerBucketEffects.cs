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
        if (sceneName != "MainGame" && sceneName != "Level2" && sceneName != "Level3") return;
        if (FindFirstObjectByType<PlayerBucketEffects>() != null) return;
        new GameObject("PlayerBucketEffects").AddComponent<PlayerBucketEffects>();
    }

    HUDControls _hud;
    Animator _animator;
    Transform _leftHand;
    Transform _rightHand;
    Transform _bucket;
    Renderer[] _bucketRenderers;
    GameObject _waterDisc;
    ParticleSystem _dripParticles;
    float _emptyLocalY, _fullLocalY;
    bool _isMaintenanceTool;

    static readonly Color WaterColor = new Color(0.12f, 0.62f, 1f);

    void Start()
    {
        StartCoroutine(SetupWhenReady());
    }

    IEnumerator SetupWhenReady()
    {
        // The character may spawn a frame or two after this component does.
        for (int i = 0; i < 120 && _bucket == null; i++)
        {
            _hud = FindFirstObjectByType<HUDControls>();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Animator[] animators = player.GetComponentsInChildren<Animator>(false);
                for (int a = 0; a < animators.Length; a++)
                {
                    if (animators[a].isHuman)
                    {
                        _animator = animators[a];
                        break;
                    }
                }

                if (_animator != null)
                {
                    _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
                    if (_leftHand != null && _rightHand != null)
                    {
                        CreateOrRecoverBucket(player.transform);
                    }
                }
            }

            if (_bucket != null && _hud != null) break;
            yield return null;
        }
        if (_bucket == null) yield break;

        // Let the Animator apply its first pose before sizing the prop.
        yield return null;
        FitBucketToCharacter();
        FollowHands();
        if (_isMaintenanceTool) yield break;
        BuildWaterDisc();
        BuildDripParticles();
    }

    void CreateOrRecoverBucket(Transform player)
    {
        Transform[] children = player.GetComponentsInChildren<Transform>(true);
        _isMaintenanceTool = SceneCatalog.IsLevel3(SceneManager.GetActiveScene().name);
        if (_isMaintenanceTool)
        {
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == "Bucket" || children[i].name == "PlayerBucket")
                    children[i].gameObject.SetActive(false);
            }

            _bucket = BuildMaintenanceHammer(player);
            _bucketRenderers = _bucket.GetComponentsInChildren<Renderer>(true);
            return;
        }

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == "Bucket" || children[i].name == "PlayerBucket")
            {
                _bucket = children[i];
                break;
            }
        }

        if (_bucket == null)
        {
            GameObject prefab = Resources.Load<GameObject>("PlayerBucket");
            if (prefab != null)
            {
                _bucket = Instantiate(prefab, player).transform;
            }
            else
            {
                _bucket = BuildFallbackBucket(player);
                Debug.LogWarning("[PlayerBucketEffects] PlayerBucket resource was unavailable; using visible fallback.");
            }
        }
        else
        {
            // Recover a bucket left below the disabled legacy character.
            _bucket.SetParent(player, true);
        }

        _bucket.name = "PlayerBucket";
        _bucket.gameObject.SetActive(true);
        foreach (Collider col in _bucket.GetComponentsInChildren<Collider>(true)) col.enabled = false;
        foreach (Rigidbody body in _bucket.GetComponentsInChildren<Rigidbody>(true)) body.isKinematic = true;
        _bucketRenderers = _bucket.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in _bucketRenderers) rend.enabled = true;
    }

    static Transform BuildFallbackBucket(Transform player)
    {
        GameObject root = new GameObject("PlayerBucket");
        root.transform.SetParent(player);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "BucketBody";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.65f, 0.55f, 0.65f);
        Collider bodyCollider = body.GetComponent<Collider>();
        if (bodyCollider != null) Destroy(bodyCollider);

        Renderer renderer = body.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(renderer.sharedMaterial);
            Color metal = new Color(0.48f, 0.52f, 0.56f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", metal);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", metal);
            renderer.sharedMaterial = mat;
        }
        return root.transform;
    }

    static Transform BuildMaintenanceHammer(Transform player)
    {
        GameObject root = new GameObject("PlayerMaintenanceHammer");
        root.transform.SetParent(player);

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "HammerHandle";
        handle.transform.SetParent(root.transform, false);
        handle.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        handle.transform.localScale = new Vector3(0.10f, 0.58f, 0.10f);
        Destroy(handle.GetComponent<Collider>());
        TintPrimitive(handle, new Color(0.42f, 0.22f, 0.08f));

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "HammerHead";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.16f, 0f);
        head.transform.localScale = new Vector3(0.72f, 0.28f, 0.32f);
        Destroy(head.GetComponent<Collider>());
        TintPrimitive(head, new Color(0.42f, 0.46f, 0.50f));
        return root.transform;
    }

    void FitBucketToCharacter()
    {
        if (!TryGetBucketBounds(out Bounds bucketBounds)) return;

        SkinnedMeshRenderer[] bodyRenderers =
            _animator.GetComponentsInChildren<SkinnedMeshRenderer>(false);
        float characterHeight = 5f;
        if (bodyRenderers.Length > 0)
        {
            Bounds bodyBounds = bodyRenderers[0].bounds;
            for (int i = 1; i < bodyRenderers.Length; i++) bodyBounds.Encapsulate(bodyRenderers[i].bounds);
            characterHeight = bodyBounds.size.y;
        }

        // Keep the bucket large enough to read, but proportioned to the
        // carrying pose so its rim sits naturally between both hands.
        float desiredHeight = characterHeight * (_isMaintenanceTool ? 0.28f : 0.13f);
        if (bucketBounds.size.y > 0.001f)
        {
            float scale = Mathf.Clamp(desiredHeight / bucketBounds.size.y, 0.2f, 3f);
            _bucket.localScale *= scale;
        }
    }

    void LateUpdate()
    {
        FollowHands();
    }

    void FollowHands()
    {
        if (_bucket == null || _leftHand == null || _rightHand == null) return;

        if (_isMaintenanceTool)
        {
            _bucket.position = _rightHand.position;
            _bucket.rotation = _animator.transform.rotation * Quaternion.Euler(68f, 5f, 18f);
            return;
        }

        Vector3 handMidpoint = (_leftHand.position + _rightHand.position) * 0.5f;
        _bucket.rotation = Quaternion.Euler(0f, _animator.transform.eulerAngles.y, 0f);

        if (!TryGetBucketBounds(out Bounds bounds))
        {
            _bucket.position = handMidpoint;
            return;
        }

        // The rim sits just below both hands, so both hands visibly carry it.
        Vector3 rimCentre = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        // Carry the bucket in front of the torso, rather than around Zuri's
        // waist. Its rim remains directly below the two gripping hands.
        Vector3 targetRim = handMidpoint
                            + _animator.transform.forward * (bounds.size.z * 0.48f)
                            + Vector3.down * (bounds.size.y * 0.10f);
        _bucket.position += targetRim - rimCentre;
    }

    bool TryGetBucketBounds(out Bounds bounds)
    {
        bounds = default;
        if (_bucketRenderers == null || _bucketRenderers.Length == 0) return false;

        bool found = false;
        for (int i = 0; i < _bucketRenderers.Length; i++)
        {
            Renderer rend = _bucketRenderers[i];
            if (rend == null || !rend.enabled) continue;
            if (!found) { bounds = rend.bounds; found = true; }
            else bounds.Encapsulate(rend.bounds);
        }
        return found;
    }

    void BuildWaterDisc()
    {
        Bounds b = TryGetBucketBounds(out Bounds bucketBounds)
            ? bucketBounds
            : new Bounds(_bucket.position, Vector3.one * 0.5f);

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

    void BuildDripParticles()
    {
        Bounds b = TryGetBucketBounds(out Bounds bucketBounds)
            ? bucketBounds
            : new Bounds(_bucket.position, Vector3.one * 0.5f);

        GameObject fx = new GameObject("BucketWaterDroplets");
        fx.transform.SetParent(_bucket, true);
        fx.transform.position = new Vector3(b.center.x, b.max.y, b.center.z);

        _dripParticles = fx.AddComponent<ParticleSystem>();
        var main = _dripParticles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.055f, 0.11f);
        main.startColor = WaterColor;
        main.gravityModifier = 1.15f;
        main.maxParticles = 80;

        var emission = _dripParticles.emission;
        emission.rateOverTime = 8f;

        var shape = _dripParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = Mathf.Max(0.08f, Mathf.Min(b.extents.x, b.extents.z) * 0.7f);
        shape.rotation = new Vector3(180f, 0f, 0f);

        ParticleSystemRenderer particleRenderer = fx.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", WaterColor);
            if (material.HasProperty("_Color")) material.SetColor("_Color", WaterColor);
            particleRenderer.sharedMaterial = material;
        }
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

        if (_dripParticles != null)
        {
            bool shouldDrip = t > 0.02f;
            if (shouldDrip && !_dripParticles.isPlaying) _dripParticles.Play();
            else if (!shouldDrip && _dripParticles.isPlaying)
                _dripParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Bucket water dropping (obstacle bump spilling it) triggers a
        // burst of droplets spilling over the rim.
        if (_lastBucketWater >= 0f && bucketWater < _lastBucketWater - 0.01f)
        {
            SpawnDropletBurst();
            if (_dripParticles != null) _dripParticles.Emit(18);
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
        if (!TryGetBucketBounds(out Bounds b)) return;

        Vector2 rand = Random.insideUnitCircle;
        Vector3 spawnPos = new Vector3(
            b.center.x + rand.x * b.size.x * 0.4f,
            b.min.y + b.size.y * 0.75f,
            b.center.z + rand.y * b.size.z * 0.4f);

        var drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        drop.name = "BucketDroplet";
        var col = drop.GetComponent<Collider>();
        if (col != null) Destroy(col);
        float size = Random.Range(0.06f, 0.11f);
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
