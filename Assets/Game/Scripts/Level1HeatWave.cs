using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// After 25% progress, recurring heat bursts last 3–5 seconds and drain player water by 20/sec.
/// Cactus and water springs restore body water and bucket.
/// </summary>
public class Level1HeatWave : MonoBehaviour
{
    [SerializeField] private float heatWaveStartProgress = Level1Config.HeatWaveStartProgress;
    [SerializeField] private float waterLossPerSecond = Level1Config.HeatWaterLossPerSecond;
    [SerializeField] private float betweenBurstCooldown = Level1Config.HeatBurstCooldown;

    static readonly float[] BurstDurations = { 3f, 4f, 5f };

    Image overlay;
    RawImage heatFogA;
    RawImage heatFogB;
    Light sun;
    Color sunOriginal = Color.white;
    HUDControls hud;

    bool started;
    bool heatActive;
    float pauseUntil;
    float fogAlpha;
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
                Debug.Log("[Level1] Heat wave system started at 25% progress");
            }

            yield return null;
        }

        while (true)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            if (IsPaused)
            {
                heatActive = false;
                yield return null;
                continue;
            }

            float burstDuration = BurstDurations[Random.Range(0, BurstDurations.Length)];
            heatActive = true;
            ObstacleGuideHUD.NotifyHit("heat_wave");
            Level1FeedbackUI.Show("HEAT WAVE! Drink from cactus or springs!", new Color(1f, 0.55f, 0.2f), burstDuration);
            Debug.Log($"[Level1] Heat burst started for {burstDuration:0}s (-{waterLossPerSecond:0}/sec player water)");

            float elapsed = 0f;
            while (elapsed < burstDuration)
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

                float dt = Time.deltaTime;
                ApplyHeatDrain(dt);
                elapsed += dt;
                yield return null;
            }

            heatActive = false;
            Debug.Log("[Level1] Heat burst ended");

            float cooldown = betweenBurstCooldown;
            while (cooldown > 0f)
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

                cooldown -= Time.deltaTime;
                yield return null;
            }
        }
    }

    void ApplyHeatDrain(float deltaTime)
    {
        if (hud == null) hud = FindFirstObjectByType<HUDControls>();
        hud?.DrainPlayerWater(waterLossPerSecond * deltaTime);
    }

    void LateUpdate()
    {
        if (!started) return;

        if (IsPaused)
        {
            if (sun != null) sun.color = sunOriginal;
        }
        else if (heatActive)
        {
            SetHeatVisual(true);
        }

        UpdateOverlayColor();
        UpdateHeatFog();
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
            fogAlpha = 0f;
            overlay.color = Color.clear;
            return;
        }

        if (hud == null) hud = FindFirstObjectByType<HUDControls>();

        float waterPercent = 100f;
        if (hud != null && hud.MaxPlayerWater > 0f)
        {
            waterPercent = (hud.PlayerWater / hud.MaxPlayerWater) * 100f;
        }

        fogAlpha = Mathf.Max(0.22f, GetWaterHeatAlpha(waterPercent));
        overlay.color = new Color(0.92f, 0.18f, 0.05f, fogAlpha * 0.5f);
    }

    void SetHeatVisual(bool on)
    {
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
        go.transform.SetAsFirstSibling();
        overlay = go.AddComponent<Image>();
        overlay.color = new Color(0.92f, 0.18f, 0.05f, 0f);
        overlay.raycastTarget = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Texture2D fogTexture = BuildFogTexture();
        heatFogA = CreateFogLayer(go.transform, "OrangeHeatFog", fogTexture);
        heatFogB = CreateFogLayer(go.transform, "RedHeatFog", fogTexture);
    }

    static RawImage CreateFogLayer(Transform parent, string name, Texture texture)
    {
        GameObject layer = new GameObject(name);
        layer.transform.SetParent(parent, false);
        RawImage image = layer.AddComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        image.color = new Color(1f, 1f, 1f, 0f);
        RectTransform rt = layer.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return image;
    }

    static Texture2D BuildFogTexture()
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "HeatFogNoise";
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)size;
                float ny = y / (float)size;
                float large = Mathf.PerlinNoise(nx * 3.1f + 4.2f, ny * 3.1f + 8.7f);
                float detail = Mathf.PerlinNoise(nx * 8.4f + 1.6f, ny * 8.4f + 3.3f);
                float alpha = Mathf.SmoothStep(0.28f, 0.82f, large * 0.72f + detail * 0.28f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * 0.82f));
            }
        }

        texture.Apply(false, true);
        return texture;
    }

    void UpdateHeatFog()
    {
        if (heatFogA == null || heatFogB == null) return;

        float alpha = heatActive && !IsPaused ? fogAlpha : 0f;
        heatFogA.color = new Color(1f, 0.40f, 0.08f, alpha * 0.62f);
        heatFogB.color = new Color(0.95f, 0.12f, 0.04f, alpha * 0.34f);
        heatFogA.uvRect = new Rect(Time.time * 0.015f, Time.time * 0.008f, 1.45f, 1.1f);
        heatFogB.uvRect = new Rect(-Time.time * 0.010f, Time.time * 0.005f, 1.2f, 1.35f);
    }

    void CacheSun()
    {
        sun = FindFirstObjectByType<Light>();
        if (sun != null) sunOriginal = sun.color;
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
