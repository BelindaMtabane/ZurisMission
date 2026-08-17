using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// After 20% progress, heat drains player water every 6 seconds.
/// Screen warmth scales with hydration level.
/// </summary>
public class Level1HeatWave : MonoBehaviour
{
    [SerializeField] private float heatWaveStartProgress = 0.20f;
    [SerializeField] private float heatWaveWaterLoss = 5f;
    [SerializeField] private float heatWaveInterval = 6f;

    Image overlay;
    Light sun;
    Color sunOriginal = Color.white;
    ParticleSystem heatParticles;
    HUDControls hud;

    bool started;
    bool heatActive;
    float pauseUntil;
    float tickTimer;
    Transform player;

    public bool IsPaused => Time.time < pauseUntil;
    public bool IsHeatActive => heatActive && !IsPaused;

    public void BindProgress(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void PauseHeatWave(float seconds)
    {
        pauseUntil = Time.time + seconds;
        Debug.Log($"[Level1] Heat paused for {seconds:0}s");
    }

    void Start()
    {
        CachePlayer();
        hud = FindFirstObjectByType<HUDControls>();
        CreateOverlay();
        CacheSun();
        CreateHeatParticles();
        StartCoroutine(HeatLoop());
    }

    IEnumerator HeatLoop()
    {
        while (!started)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            if (Level1Progress.Normalized(PlayerZ()) >= heatWaveStartProgress)
            {
                started = true;
                heatActive = true;
                Level1FeedbackUI.Show("HEAT WAVE! Collect cactus for water!", new Color(1f, 0.55f, 0.2f), 2.4f);
                Debug.Log("[Level1] Heat wave started at 20% progress");
            }

            yield return null;
        }

        tickTimer = heatWaveInterval;
        while (true)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            if (IsPaused)
            {
                yield return null;
                continue;
            }

            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = heatWaveInterval;
                ApplyHeatTick();
            }

            yield return null;
        }
    }

    void ApplyHeatTick()
    {
        if (hud == null) hud = FindFirstObjectByType<HUDControls>();
        hud?.ApplyHeatWaveTick(heatWaveWaterLoss);
    }

    void LateUpdate()
    {
        if (!started) return;

        if (IsPaused)
        {
            if (heatParticles != null && heatParticles.isPlaying) heatParticles.Stop();
            if (sun != null) sun.color = sunOriginal;
        }
        else if (heatActive)
        {
            SetHeatVisual(true);
            if (heatParticles != null && player != null)
            {
                heatParticles.transform.position = player.position + Vector3.up * 2f;
            }
        }

        UpdateOverlayColor();
    }

    static float GetWaterHeatAlpha(float waterPercent)
    {
        waterPercent = Mathf.Clamp(waterPercent, 0f, 100f);

        if (waterPercent <= 20f)
        {
            return Mathf.Lerp(0.42f, 0.24f, waterPercent / 20f);
        }

        if (waterPercent <= 60f)
        {
            return Mathf.Lerp(0.22f, 0.08f, (waterPercent - 20f) / 40f);
        }

        return Mathf.Lerp(0.08f, 0.03f, (waterPercent - 60f) / 40f);
    }

    void UpdateOverlayColor()
    {
        if (overlay == null) return;
        if (!heatActive)
        {
            overlay.color = new Color(1f, 1f, 1f, 0f);
            return;
        }

        if (hud == null) hud = FindFirstObjectByType<HUDControls>();

        float waterPercent = 100f;
        if (hud != null && hud.MaxPlayerWater > 0f)
        {
            waterPercent = (hud.PlayerWater / hud.MaxPlayerWater) * 100f;
        }

        float alpha = GetWaterHeatAlpha(waterPercent);
        Color tint = waterPercent <= 20f
            ? new Color(0.95f, 0.12f, 0.08f, alpha)
            : waterPercent <= 60f
                ? new Color(0.92f, 0.22f, 0.10f, alpha)
                : new Color(0.90f, 0.35f, 0.14f, alpha);

        overlay.color = tint;
    }

    void SetHeatVisual(bool on)
    {
        if (heatParticles != null)
        {
            if (on && !IsPaused) heatParticles.Play();
            else heatParticles.Stop();
        }

        if (sun != null)
        {
            sun.color = on && !IsPaused ? new Color(1f, 0.62f, 0.32f) : sunOriginal;
        }
    }

    void CreateOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject go = new GameObject("Level1HeatWaveOverlay");
        go.transform.SetParent(canvas.transform, false);
        overlay = go.AddComponent<Image>();
        overlay.color = new Color(0.92f, 0.12f, 0.08f, 0f);
        overlay.raycastTarget = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void CacheSun()
    {
        sun = FindFirstObjectByType<Light>();
        if (sun != null) sunOriginal = sun.color;
    }

    void CreateHeatParticles()
    {
        GameObject go = new GameObject("HeatWaveParticles");
        go.transform.SetParent(transform, false);
        heatParticles = go.AddComponent<ParticleSystem>();
        var main = heatParticles.main;
        main.startColor = new Color(1f, 0.5f, 0.12f, 0.45f);
        main.startSize = 1.2f;
        main.startLifetime = 1.4f;
        main.startSpeed = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 30;
        var emission = heatParticles.emission;
        emission.rateOverTime = 10f;
        var shape = heatParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 3.5f;
        heatParticles.Stop();
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }

    float PlayerZ()
    {
        CachePlayer();
        return player != null ? player.position.z : Level1Progress.StartZ;
    }
}
