using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Plays the clips imported under Assets/sound, named for each gameplay use.
/// </summary>
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    AudioSource music;
    AudioSource sfx;
    AudioSource ambience;
    AudioSource storm;

    AudioClip buttonClick;
    AudioClip gameMusic;
    AudioClip level3Music;
    AudioClip maintenance;
    AudioClip nextLevel;
    AudioClip pickup;
    AudioClip waterPickup;
    AudioClip warthogAttack;
    AudioClip wolfAttack;
    AudioClip waterSplash;
    AudioClip mudMonster;
    AudioClip springWater;
    AudioClip thunderstorm;
    AudioClip windLeaves;

    const float StormVolume = 0.8f;
    const float StormHoldSeconds = 4.5f;
    const float StormFadeSeconds = 1.6f;

    float waterReadyAt;
    float splashReadyAt;
    float mudReadyAt;
    float stormStopAt;
    Coroutine buttonScan;
    float musicSceneVolume = 0.42f;

    const string PrefMuted = "Zuri.SoundMuted";
    const string PrefVolume = "Zuri.SoundVolume";

    public static event System.Action OnChanged;

    public static bool Muted { get; private set; }
    public static float Volume { get; private set; } = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Ensure().ApplySceneMusic(SceneManager.GetActiveScene().name);
        Ensure().ScanButtonsSoon();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Ensure().ApplySceneMusic(scene.name);
        Ensure().ScanButtonsSoon();
    }

    public static GameAudio Ensure()
    {
        if (Instance != null) return Instance;
        GameObject go = new GameObject("GameAudio");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<GameAudio>();
        Instance.Build();
        return Instance;
    }

    void Build()
    {
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        music.spatialBlend = 0f;
        music.volume = 0.42f;

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.loop = false;
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;
        sfx.volume = 1f;

        ambience = gameObject.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.spatialBlend = 0f;

        storm = gameObject.AddComponent<AudioSource>();
        storm.loop = false;
        storm.playOnAwake = false;
        storm.spatialBlend = 0f;

        buttonClick = LoadClip("buttonClick");
        gameMusic = LoadClip("game sound");
        level3Music = LoadClip("third level sound");
        maintenance = LoadClip("maintainance sound");
        nextLevel = LoadClip("Nextlevelbutton");
        pickup = LoadClip("pickup");
        waterPickup = LoadClip("watercollection pickup");
        warthogAttack = LoadClip("wathog attack sound");
        wolfAttack = LoadClip("wolf attack sound");
        waterSplash = LoadClip("bouncing-on-the-water");
        mudMonster = LoadClip("mud monster sound");
        springWater = LoadClip("spring water sound");
        thunderstorm = LoadClip("thunderstorm_pouring-rain");
        windLeaves = LoadClip("wind-through-the-leaves");

        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolume, 1f));
        Muted = PlayerPrefs.GetInt(PrefMuted, 0) == 1;
        ApplyOutput();
        OnChanged?.Invoke();
    }

    static AudioClip LoadClip(string fileName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Sound/" + fileName);
        if (clip != null) return clip;

#if UNITY_EDITOR
        clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sound/" + fileName + ".mp3");
        if (clip != null) return clip;
        clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/" + fileName + ".mp3");
        if (clip != null) return clip;
#endif

        Debug.LogWarning("[GameAudio] Missing clip: " + fileName);
        return null;
    }

    void ApplySceneMusic(string sceneName)
    {
        ApplySceneAmbience(sceneName);

        AudioClip clip = null;
        float volume = 0.42f;

        if (sceneName == SceneCatalog.Level3)
        {
            clip = level3Music;
            volume = 0.46f;
        }
        else if (sceneName == SceneCatalog.MainGame || sceneName == SceneCatalog.Level2 || sceneName == SceneCatalog.StartScreen)
        {
            clip = gameMusic;
            volume = sceneName == SceneCatalog.StartScreen ? 0.32f : 0.42f;
        }

        if (clip == null)
        {
            music.Stop();
            music.clip = null;
            return;
        }

        musicSceneVolume = volume;
        if (music.clip == clip && music.isPlaying)
        {
            music.volume = volume;
            return;
        }

        music.clip = clip;
        music.volume = volume;
        music.Play();
    }

    /// <summary>Level 3 fog is present along the whole run, so its wind bed loops for the entire level.</summary>
    void ApplySceneAmbience(string sceneName)
    {
        if (storm != null) storm.Stop();
        stormStopAt = 0f;

        if (sceneName == SceneCatalog.Level3 && windLeaves != null)
        {
            if (ambience.clip == windLeaves && ambience.isPlaying) return;
            ambience.clip = windLeaves;
            ambience.volume = 0.34f;
            ambience.Play();
            return;
        }

        ambience.Stop();
        ambience.clip = null;
    }

    void Update()
    {
        if (storm == null || !storm.isPlaying || stormStopAt <= 0f) return;

        float over = Time.unscaledTime - stormStopAt;
        if (over <= 0f) return;

        storm.volume = StormVolume * Mathf.Clamp01(1f - over / StormFadeSeconds);
        if (over >= StormFadeSeconds)
        {
            storm.Stop();
            stormStopAt = 0f;
        }
    }

    public static void ToggleMuted()
    {
        SetMuted(!Muted);
    }

    public static void SetMuted(bool muted)
    {
        Ensure();
        if (Muted == muted) return;
        Muted = muted;
        PlayerPrefs.SetInt(PrefMuted, muted ? 1 : 0);
        PlayerPrefs.Save();
        Ensure().ApplyOutput();
        OnChanged?.Invoke();
    }

    public static void SetVolume(float volume)
    {
        Ensure();
        volume = Mathf.Clamp01(volume);
        Volume = volume;
        PlayerPrefs.SetFloat(PrefVolume, volume);
        if (volume <= 0.001f)
        {
            Muted = true;
            PlayerPrefs.SetInt(PrefMuted, 1);
        }
        else if (Muted)
        {
            Muted = false;
            PlayerPrefs.SetInt(PrefMuted, 0);
        }

        PlayerPrefs.Save();
        Ensure().ApplyOutput();
        OnChanged?.Invoke();
    }

    void ApplyOutput()
    {
        AudioListener.volume = Muted ? 0f : Volume;
        if (music != null) music.volume = musicSceneVolume;
    }

    void ScanButtonsSoon()
    {
        if (buttonScan != null) StopCoroutine(buttonScan);
        buttonScan = StartCoroutine(ScanButtonsRoutine());
    }

    IEnumerator ScanButtonsRoutine()
    {
        yield return null;
        yield return null;
        HookAllButtons();
        yield return null;
        HookAllButtons();
    }

    public static void HookAllButtons()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            if (buttons[i].GetComponent<GameAudioButtonHook>() != null) continue;
            buttons[i].gameObject.AddComponent<GameAudioButtonHook>();
        }
    }

    public static void PlayClick() => Ensure().PlaySfx(Ensure().buttonClick, 0.85f);

    public static void PlayNextLevel() => Ensure().PlaySfx(Ensure().nextLevel, 0.9f);

    public static void PlayPickup() => Ensure().PlaySfx(Ensure().pickup, 0.85f);

    public static void PlayWater()
    {
        GameAudio audio = Ensure();
        if (Time.unscaledTime < audio.waterReadyAt) return;
        audio.waterReadyAt = Time.unscaledTime + 0.4f;
        audio.PlaySfx(audio.waterPickup, 0.8f);
    }

    public static void PlayMaintenance() => Ensure().PlaySfx(Ensure().maintenance, 0.9f);

    public static void PlayWarthog() => Ensure().PlaySfx(Ensure().warthogAttack, 0.95f);

    public static void PlayWolf() => Ensure().PlaySfx(Ensure().wolfAttack, 0.95f);

    /// <summary>Player splashing into a water source or drinking from a cactus.</summary>
    public static void PlayWaterSplash()
    {
        GameAudio audio = Ensure();
        if (Time.unscaledTime < audio.splashReadyAt) return;
        audio.splashReadyAt = Time.unscaledTime + 0.35f;
        audio.PlaySfx(audio.waterSplash, 0.9f);
    }

    /// <summary>Mud puddles, mud balls, and mud monsters.</summary>
    public static void PlayMud()
    {
        GameAudio audio = Ensure();
        if (Time.unscaledTime < audio.mudReadyAt) return;
        audio.mudReadyAt = Time.unscaledTime + 0.7f;
        audio.PlaySfx(audio.mudMonster, 0.95f);
    }

    /// <summary>Thunder and rain burst for a lightning strike; repeated strikes extend it before it fades out.</summary>
    public static void PlayThunder()
    {
        GameAudio audio = Ensure();
        if (audio.thunderstorm == null || audio.storm == null) return;

        audio.stormStopAt = Time.unscaledTime + StormHoldSeconds;
        audio.storm.volume = StormVolume;
        if (audio.storm.isPlaying) return;

        audio.storm.clip = audio.thunderstorm;
        audio.storm.time = 0f;
        audio.storm.Play();
    }

    /// <summary>Looping positional spring sound that follows a water source object.</summary>
    public static void AttachSpringLoop(GameObject host, float volume = 0.6f)
    {
        if (host == null) return;
        AudioClip clip = Ensure().springWater;
        if (clip == null || host.GetComponent<GameAudioSpringLoop>() != null) return;

        AudioSource source = host.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 3f;
        source.maxDistance = 26f;
        source.dopplerLevel = 0f;
        source.volume = volume;
        host.AddComponent<GameAudioSpringLoop>().Bind(source);
    }

    void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null || sfx == null) return;
        sfx.PlayOneShot(clip, volume);
    }
}

public class GameAudioSpringLoop : MonoBehaviour
{
    AudioSource source;

    public void Bind(AudioSource loopSource)
    {
        source = loopSource;
        if (isActiveAndEnabled) StartLoop();
    }

    void OnEnable() => StartLoop();

    void StartLoop()
    {
        if (source == null || source.clip == null || source.isPlaying) return;
        // Offset each spring so neighbouring sources do not play in lockstep.
        source.time = Random.Range(0f, Mathf.Max(0f, source.clip.length - 0.1f));
        source.Play();
    }
}

public class GameAudioButtonHook : MonoBehaviour
{
    Button button;
    bool nextLevel;

    void Awake() => Bind();

    void OnEnable() => Bind();

    void Bind()
    {
        if (button == null) button = GetComponent<Button>();
        if (button == null) return;

        string n = gameObject.name;
        nextLevel = n.IndexOf("NextLevel", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("Continue", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (n.IndexOf("UseBtn", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return;
        if (n.IndexOf("HudSoundToggle", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return;
        if (n.IndexOf("SP_SoundBtn", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return;

        button.onClick.RemoveListener(OnClicked);
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (nextLevel) GameAudio.PlayNextLevel();
        else GameAudio.PlayClick();
    }
}
