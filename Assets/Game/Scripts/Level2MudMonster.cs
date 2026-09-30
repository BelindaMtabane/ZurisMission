using System.Collections;
using UnityEngine;

/// <summary>
/// Mud Monster pops up in a lane and rolls a mud ball toward the player.
/// </summary>
public class Level2MudMonster : MonoBehaviour
{
    const float TriggerDistance = 52f;

    [SerializeField] private int laneIndex;
    [SerializeField] private float spawnProgress;
    [SerializeField] private GameObject warningRoot;
    [SerializeField] private GameObject visualRoot;

    enum Phase { Wait, Emerging, Throw, Done }

    Phase phase = Phase.Wait;
    float ballDamage;
    Transform player;
    ParticleSystem mudSplash;
    float splashTimer;

    public void Setup(int lane, float progress, GameObject warning, GameObject visuals)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        spawnProgress = progress;
        warningRoot = warning;
        visualRoot = visuals;

        if (spawnProgress <= 0.20f)
        {
            ballDamage = 14f;
        }
        else if (spawnProgress <= 0.65f)
        {
            ballDamage = 20f;
        }
        else if (spawnProgress <= 0.90f)
        {
            ballDamage = 26f;
        }
        else
        {
            ballDamage = 38f;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
            visualRoot.transform.localScale = Vector3.one;
            visualRoot.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        }

        if (warningRoot != null) warningRoot.SetActive(true);
        GetComponent<Level2MudPothole>()?.Setup(ballDamage);
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        CachePlayer();
        if (player == null) return;

        Vector3 p = transform.position;
        p.x = LevelLanes.X(laneIndex);
        p.y = Level2Ground.SurfaceY;
        transform.position = p;

        switch (phase)
        {
            case Phase.Wait:
                if (player.position.z > transform.position.z - TriggerDistance)
                {
                    PopUpNow();
                }
                break;

            case Phase.Throw:
                if (visualRoot != null)
                {
                    float bounce = Mathf.Max(0f, Mathf.Sin(Time.time * 9f));
                    visualRoot.transform.localScale = new Vector3(
                        1f + bounce * 0.05f,
                        1f + bounce * 0.13f,
                        1f + bounce * 0.05f);
                }

                splashTimer -= Time.deltaTime;
                if (splashTimer <= 0f)
                {
                    phase = Phase.Done;
                    Level2MudMonsterDirector.ReleaseMonster(this);
                    Destroy(gameObject, 0.8f);
                }
                break;

            case Phase.Emerging:
                break;
        }
    }

    void PopUpNow()
    {
        // Every visible pothole represents a real monster spawn. Do not hold
        // later monsters behind a global one-at-a-time queue.
        Level2MudMonsterDirector.TryStartMonster(this);
        phase = Phase.Emerging;
        GameAudio.PlayMud();

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
            visualRoot.transform.localScale = Vector3.one;
            visualRoot.transform.localPosition = new Vector3(0f, -3.2f, 0f);
        }

        StartCoroutine(EmergeAndThrow());
    }

    IEnumerator EmergeAndThrow()
    {
        float elapsed = 0f;
        const float duration = 0.65f;
        while (elapsed < duration)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            if (visualRoot != null)
                visualRoot.transform.localPosition = Vector3.Lerp(
                    new Vector3(0f, -3.2f, 0f), new Vector3(0f, 0.2f, 0f), t);
            yield return null;
        }

        phase = Phase.Throw;
        StartMudSplash();
    }

    void StartMudSplash()
    {
        splashTimer = 2.1f;

        if (visualRoot != null)
        {
            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            if (animator != null) animator.CrossFadeInFixedTime("Attack01", 0.12f, 0, 0f);
        }

        GameObject fx = new GameObject("TinyMudSplash");
        fx.transform.SetParent(transform, false);
        fx.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        fx.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        mudSplash = fx.AddComponent<ParticleSystem>();
        var main = mudSplash.main;
        main.loop = false;
        main.duration = 1.8f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.14f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.32f, 0.15f, 0.035f),
            new Color(0.62f, 0.34f, 0.09f));
        main.gravityModifier = 1.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 90;

        var emission = mudSplash.emission;
        emission.rateOverTime = 10f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 18),
            new ParticleSystem.Burst(0.48f, 12),
            new ParticleSystem.Burst(0.96f, 12)
        });

        var shape = mudSplash.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 55f;
        shape.radius = 1.15f;

        ParticleSystemRenderer particleRenderer = fx.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material material = new Material(shader);
            // Keep the renderer neutral so the particle system's brown
            // gradient is displayed without being multiplied too dark.
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            particleRenderer.sharedMaterial = material;
        }

        mudSplash.Play();
    }

    void OnDestroy()
    {
        Level2MudMonsterDirector.ReleaseMonster(this);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}

public class Level2MudBall : MonoBehaviour
{
    [SerializeField] private int laneIndex;
    [SerializeField] private float speed = 16f;
    [SerializeField] private float damage = 12f;

    Transform player;
    bool hit;
    bool passed;

    public bool HasPassed => passed;

    public void Launch(int lane, float rollSpeed, float healthDamage, Transform target)
    {
        laneIndex = Mathf.Clamp(lane, 0, LevelLanes.Count - 1);
        speed = rollSpeed;
        damage = healthDamage;
        player = target;
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
            if (player == null) return;
        }

        Vector3 p = transform.position;
        p.x = LevelLanes.X(laneIndex);
        p.y = Level2Ground.SurfaceY + 0.7f;
        p.z -= speed * Time.deltaTime;
        transform.position = p;
        transform.Rotate(Vector3.right, speed * 40f * Time.deltaTime, Space.World);

        if (!hit && player.position.z >= transform.position.z - 1.6f)
        {
            TryHit();
        }

        if (transform.position.z < player.position.z - 14f)
        {
            passed = true;
            Destroy(gameObject);
        }
    }

    void TryHit()
    {
        hit = true;

        if (Level2BubbleShield.TryBlockMudBall())
        {
            passed = true;
            Destroy(gameObject);
            return;
        }

        PlayerController controller = player.GetComponent<PlayerController>();
        float laneDelta = Mathf.Abs(player.position.x - LevelLanes.X(laneIndex));
        if (laneDelta > 3.2f || (controller != null && !controller.IsGrounded && player.position.y > Level2Ground.SurfaceY + 1.4f))
        {
            passed = true;
            return;
        }

        GameAudio.PlayMud();
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-damage, "A mud monster hit you!");
        controller?.ApplySpeedModifier(
            controller.CurrentSpeed * Level2Config.MudBallSlowMultiplier,
            Level2Config.MudBallSlowDuration);
        passed = true;
        Destroy(gameObject);
    }
}

public class Level2MudPothole : MonoBehaviour
{
    float damage = 14f;
    bool hitPlayer;

    public void Setup(float healthDamage)
    {
        damage = Mathf.Max(14f, healthDamage);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hitPlayer || !other.CompareTag("Player")) return;
        hitPlayer = true;

        GameAudio.PlayMud();
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-damage, "The mud monster's pothole hurt you!");
        PlayerController player = other.GetComponent<PlayerController>();
        player?.ApplyMudSlow(Level2Config.MudBallSlowMultiplier, Level2Config.MudBallSlowDuration);
    }
}
