using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Heat waves drain water; screen redness follows player water level in three bands.
/// </summary>
public class Level1HeatWave : MonoBehaviour
{
    const float StartProgress = 0.20f;
    const float WaveInterval = 10f;
    const float WaveDuration = 6f;
    const float WaterDrainPerSecond = 10f;
    const float FadeInSeconds = 1.5f;
    const float FadeOutSeconds = 1.2f;
    const float EndChallengeProgress = 0.78f;

    Image overlay;
    Light sun;
    Color sunOriginal = Color.white;
    ParticleSystem heatParticles;
    HUDControls hud;

    bool started;
    bool waveActive;
    float wavePulse;
    float pauseUntil;
    float drainAccumulator;
    Transform player;

    public bool IsPaused => Time.time < pauseUntil;
    public bool IsWaveActive => waveActive && !IsPaused;

    public void BindProgress(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void PauseHeatWave(float seconds)
    {
        pauseUntil = Time.time + seconds;
        Debug.Log($"[Level1] Heat wave paused for {seconds:0}s");
    }

    void Start()
    {
        CachePlayer();
        hud = FindFirstObjectByType<HUDControls>();
        CreateOverlay();
        CacheSun();
        CreateHeatParticles();
        StartCoroutine(WatchAndRunWaves());
    }

    IEnumerator WatchAndRunWaves()
    {
        while (!started)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            if (Level1Progress.Normalized(PlayerZ()) >= StartProgress)
            {
                started = true;
                Debug.Log("[Level1] Heat wave system started");
            }

            yield return null;
        }

        float intervalTimer = WaveInterval;
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

            intervalTimer -= Time.deltaTime;
            if (intervalTimer <= 0f)
            {
                yield return StartCoroutine(RunHeatWave());
                float progress = Level1Progress.Normalized(PlayerZ());
                intervalTimer = progress >= EndChallengeProgress ? WaveInterval * 0.75f : WaveInterval;
            }

            yield return null;
        }
    }

    IEnumerator RunHeatWave()
    {
        waveActive = true;
        drainAccumulator = 0f;
        Debug.Log("[Level1] Heat wave — collect water!");

        float elapsed = 0f;
        while (elapsed < WaveDuration)
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

            elapsed += Time.deltaTime;
            wavePulse = Mathf.Clamp01(elapsed / FadeInSeconds);

            drainAccumulator += Time.deltaTime;
            while (drainAccumulator >= 1f)
            {
                drainAccumulator -= 1f;
                if (hud == null) hud = FindFirstObjectByType<HUDControls>();
                hud?.DrainPlayerWater(WaterDrainPerSecond);
                Debug.Log("[Level1] Heat wave -10 water");
            }

            yield return null;
        }

        waveActive = false;

        if (hud == null) hud = FindFirstObjectByType<HUDControls>();
        if (hud != null && hud.PlayerWater <= 0f)
        {
            hud.LoseHeatWave("The heat wave dried you out. Collect water during heat waves to survive.");
            yield break;
        }

        float fadeT = 0f;
        float startPulse = wavePulse;
        while (fadeT < FadeOutSeconds)
        {
            fadeT += Time.deltaTime;
            wavePulse = Mathf.Lerp(startPulse, 0f, fadeT / FadeOutSeconds);
            yield return null;
        }

        wavePulse = 0f;
        Debug.Log("[Level1] Heat wave ended");
    }

    void LateUpdate()
    {
        if (!started) return;

        if (IsPaused)
        {
            if (heatParticles != null && heatParticles.isPlaying) heatParticles.Stop();
            if (sun != null) sun.color = sunOriginal;
        }
        else if (waveActive || Level1Progress.Normalized(PlayerZ()) >= StartProgress)
        {
            SetHeatVisual(waveActive);
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
            return Mathf.Lerp(0.48f, 0.28f, waterPercent / 20f);
        }

        if (waterPercent <= 60f)
        {
            return Mathf.Lerp(0.24f, 0.10f, (waterPercent - 20f) / 40f);
        }

        return Mathf.Lerp(0.10f, 0.04f, (waterPercent - 60f) / 40f);
    }

    void UpdateOverlayColor()
    {
        if (overlay == null) return;

        bool showHeatTint = waveActive || wavePulse > 0.01f;
        if (!showHeatTint)
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

        float alpha = GetWaterHeatAlpha(waterPercent) * Mathf.Max(wavePulse, waveActive ? 1f : wavePulse);

        if (waveActive)
        {
            float waveBoost = wavePulse * 0.14f;
            if (waterPercent <= 20f)
            {
                waveBoost += wavePulse * 0.10f;
            }
            else if (waterPercent <= 60f)
            {
                waveBoost += wavePulse * 0.06f;
            }
            else
            {
                waveBoost += wavePulse * 0.03f;
            }

            alpha = Mathf.Min(0.58f, alpha + waveBoost);
        }

        Color tint = waterPercent <= 20f
            ? new Color(0.95f, 0.08f, 0.05f, alpha)
            : waterPercent <= 60f
                ? new Color(0.92f, 0.16f, 0.08f, alpha)
                : new Color(0.90f, 0.28f, 0.12f, alpha);

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
            sun.color = on && !IsPaused ? new Color(1f, 0.58f, 0.25f) : sunOriginal;
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
        main.startColor = new Color(1f, 0.45f, 0.08f, 0.55f);
        main.startSize = 1.4f;
        main.startLifetime = 1.6f;
        main.startSpeed = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 40;
        var emission = heatParticles.emission;
        emission.rateOverTime = 12f;
        var shape = heatParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 4f;
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
