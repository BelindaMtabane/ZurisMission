using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Random acid-rain sheets that sweep the path from left, right, front, or above.
/// The player can dodge by moving to a clear side of the road.
/// </summary>
public class Level3AcidStorm : MonoBehaviour
{
    enum Dir { Left, Right, Front, Above }

    const float WarningSeconds = 2.2f;
    const float StrikeSeconds = 4.2f;
    const float MinGap = 8f;
    const float MaxGap = 13f;

    Transform player;
    float nextBurst;
    bool busy;
    bool damaging;
    GameObject rainRoot;
    BoxCollider rainHit;
    ParticleSystem rain;
    Dir currentDir;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.Level3) return;
        if (FindFirstObjectByType<Level3AcidStorm>() != null) return;
        new GameObject("Level3AcidStorm").AddComponent<Level3AcidStorm>();
    }

    void Start()
    {
        nextBurst = 4.5f;
        BuildRain();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.Level3) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null || rainRoot == null) return;

        nextBurst -= Time.deltaTime;
        if (!busy && nextBurst <= 0f)
            StartCoroutine(Burst());
    }

    System.Collections.IEnumerator Burst()
    {
        busy = true;
        nextBurst = 999f;
        currentDir = (Dir)Random.Range(0, 4);
        damaging = false;
        PlaceBurst(false);
        if (rain != null)
        {
            rain.gameObject.SetActive(true);
            rain.Play(true);
        }
        if (rainHit != null) rainHit.enabled = false;

        string side = currentDir == Dir.Left ? "LEFT"
            : currentDir == Dir.Right ? "RIGHT"
            : currentDir == Dir.Front ? "AHEAD"
            : "ABOVE";
        Level3FeedbackUI.Show($"ACID RAIN FROM THE {side}! MOVE!", new Color(0.45f, 0.95f, 0.28f), 2.1f);

        yield return new WaitForSeconds(WarningSeconds);
        damaging = true;
        if (rainHit != null) rainHit.enabled = true;
        Level3FeedbackUI.Show("ACID RAIN!", new Color(0.4f, 0.9f, 0.2f), 0.8f);
        float elapsed = 0f;
        while (elapsed < StrikeSeconds)
        {
            PlaceBurst(true);
            elapsed += Time.deltaTime;
            yield return null;
        }

        damaging = false;
        if (rainHit != null) rainHit.enabled = false;
        if (rain != null)
        {
            rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            rain.gameObject.SetActive(false);
        }
        busy = false;
        nextBurst = Random.Range(MinGap, MaxGap);
    }

    void PlaceBurst(bool followPlayer)
    {
        if (player == null) return;
        Vector3 p = player.position;
        float groundY = Level3Ground.SurfaceY;
        float centerX = LevelLanes.PathCenterX;
        Vector3 pos;
        Vector3 size;
        Vector3 vel;

        switch (currentDir)
        {
            case Dir.Left:
                pos = new Vector3(centerX - 7.5f, groundY + 10f, p.z + 6f);
                size = new Vector3(12f, 22f, 16f);
                vel = new Vector3(10f, -24f, 2f);
                break;
            case Dir.Right:
                pos = new Vector3(centerX + 7.5f, groundY + 10f, p.z + 6f);
                size = new Vector3(12f, 22f, 16f);
                vel = new Vector3(-10f, -24f, 2f);
                break;
            case Dir.Front:
                pos = new Vector3(centerX + Random.Range(-6f, 6f), groundY + 12f, p.z + 14f);
                size = new Vector3(11f, 24f, 14f);
                vel = new Vector3(Random.Range(-4f, 4f), -26f, -8f);
                break;
            default:
                pos = new Vector3(centerX + Random.Range(-8f, 8f), groundY + 16f, p.z + 4f);
                size = new Vector3(10f, 28f, 12f);
                vel = new Vector3(Random.Range(-8f, 8f), -28f, Random.Range(-4f, 4f));
                break;
        }

        if (!followPlayer || rainRoot != null)
            transform.position = pos;

        if (rainHit != null)
        {
            rainHit.center = Vector3.zero;
            rainHit.size = size;
        }

        if (rain != null)
        {
            var velocity = rain.velocityOverLifetime;
            velocity.x = new ParticleSystem.MinMaxCurve(vel.x - 2f, vel.x + 2f);
            velocity.y = new ParticleSystem.MinMaxCurve(vel.y - 2f, vel.y + 2f);
            velocity.z = new ParticleSystem.MinMaxCurve(vel.z - 2f, vel.z + 2f);
            var shape = rain.shape;
            shape.scale = new Vector3(size.x * 0.45f, 0.4f, size.z * 0.45f);
        }
    }

    void OnTriggerStay(Collider other)
    {
        HandleHit(other);
    }

    public void HandleHit(Collider other)
    {
        if (!damaging) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (!IsPlayer(other)) return;
        if (Level3LeafProtection.TryBlockAcid()) return;

        damaging = false;
        ObstacleGuideHUD.NotifyHit("acid");
        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-Level3Config.AcidHealthDamage, "Acid rain burned you!");
        Level3FeedbackUI.Show("ACID RAIN! LOST 10 HEALTH!", new Color(0.3f, 0.85f, 0.15f), 0.7f);
    }

    void BuildRain()
    {
        rainRoot = gameObject;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rainHit = gameObject.GetComponent<BoxCollider>();
        if (rainHit == null) rainHit = gameObject.AddComponent<BoxCollider>();
        rainHit.isTrigger = true;
        rainHit.size = new Vector3(12f, 22f, 16f);

        GameObject psGo = new GameObject("Rain");
        psGo.transform.SetParent(rainRoot.transform, false);
        psGo.transform.localPosition = new Vector3(0f, 8f, 0f);
        rain = psGo.AddComponent<ParticleSystem>();
        var main = rain.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.5f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.38f, 0.95f, 0.48f, 0.82f),
            new Color(0.62f, 1f, 0.72f, 0.95f));
        main.maxParticles = 1800;
        var emission = rain.emission;
        emission.rateOverTime = 520f;
        var shape = rain.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(6f, 0.4f, 6f);
        var velocity = rain.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(-28f, -22f);
        ParticleSystemRenderer renderer = psGo.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 4.2f;
        Material waterMaterial = NamuWaterFxLibrary.LoadTrailMaterial();
        if (waterMaterial != null)
        {
            Material rainMaterial = new Material(waterMaterial);
            Color tint = new Color(0.40f, 1f, 0.52f, 0.88f);
            if (rainMaterial.HasProperty("_BaseColor")) rainMaterial.SetColor("_BaseColor", tint);
            if (rainMaterial.HasProperty("_Color")) rainMaterial.SetColor("_Color", tint);
            renderer.material = rainMaterial;
        }

        rain.gameObject.SetActive(false);
        if (rainHit != null) rainHit.enabled = false;
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }

    static bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        return other.GetComponentInParent<PlayerController>() != null;
    }
}
