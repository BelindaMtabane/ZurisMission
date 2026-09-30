using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loads sprites from the Adventure UI Kit (Resources/UI/Adventure at runtime,
/// pack folder in the Editor) and applies them to panels, sliders, and buttons.
/// </summary>
public static class AdventureUI
{
    const string ResourcesFolder = "UI/Adventure/";
#if UNITY_EDITOR
    const string PackRoot = "Assets/Adventure UI Kit/Sprites/";
#endif

    static readonly Dictionary<string, Sprite> Cache = new();

    static readonly Dictionary<string, string> ResourceKeys = new()
    {
        { "panel_popup", "panel_popup" },
        { "progressbar_bg", "progressbar_bg" },
        { "fill_green", "fill_green" },
        { "fill_blue", "fill_blue" },
        { "fill_orange", "fill_orange" },
        { "btn_retry", "btn_retry" },
        { "btn_new_game", "btn_new_game" },
        { "btn_resume", "btn_resume" },
        { "btn_quit", "btn_quit" },
        { "btn_setting", "btn_setting" },
        { "btn_medium", "btn_medium" },
        { "btn_play", "btn_play" },
        { "coin_bg", "coin_bg" },
        { "icon_pause", "icon_pause" },
        { "btn_mission", "btn_mission" },
        { "btn_small", "btn_small" },
    };

#if UNITY_EDITOR
    static readonly Dictionary<string, string> EditorPaths = new()
    {
        { "panel_popup", "GameUI/popup_bg.png" },
        { "progressbar_bg", "GameUI/progressbar_bg_.png" },
        { "fill_green", "Slider/slider_fill_green.png" },
        { "fill_blue", "Slider/slider_fill_blue.png" },
        { "fill_orange", "Slider/slider_fill_orange.png" },
        { "btn_retry", "GameUI/btn_retry.png" },
        { "btn_new_game", "Button/btn_newGame.png" },
        { "btn_resume", "Button/btn_resume.png" },
        { "btn_quit", "Button/btn_quit.png" },
        { "btn_setting", "Button/btn_setting.png" },
        { "btn_medium", "GameUI/btn_bg_medium.png" },
        { "btn_play", "GameUI/btn_play_bg.png" },
        { "coin_bg", "GameUI/coin_bg.png" },
        { "icon_pause", "Icons/icon_pause.png" },
        { "btn_mission", "Button/btn_mission.png" },
        { "btn_small", "GameUI/btn_small.png" },
    };
#endif

    public static bool Available => PanelPopup != null;

    public static Sprite PanelPopup => Load("panel_popup");
    public static Sprite ProgressBarBg => Load("progressbar_bg");
    public static Sprite FillGreen => Load("fill_green");
    public static Sprite FillBlue => Load("fill_blue");
    public static Sprite FillOrange => Load("fill_orange");
    public static Sprite BtnRetry => Load("btn_retry");
    public static Sprite BtnNewGame => Load("btn_new_game");
    public static Sprite BtnResume => Load("btn_resume");
    public static Sprite BtnQuit => Load("btn_quit");
    public static Sprite BtnSetting => Load("btn_setting");
    public static Sprite BtnMedium => Load("btn_medium");
    public static Sprite BtnPlay => Load("btn_play");
    public static Sprite CoinBg => Load("coin_bg");
    public static Sprite IconPause => Load("icon_pause");
    public static Sprite BtnMission => Load("btn_mission");
    public static Sprite BtnSmall => Load("btn_small");

    public static Sprite Load(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (Cache.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        if (ResourceKeys.TryGetValue(key, out string resName))
        {
            Sprite s = Resources.Load<Sprite>(ResourcesFolder + resName);
            if (s != null)
            {
                Cache[key] = s;
                return s;
            }
        }

#if UNITY_EDITOR
        if (EditorPaths.TryGetValue(key, out string rel))
        {
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(PackRoot + rel);
            Cache[key] = s;
            return s;
        }
#endif
        return null;
    }

    public static void ApplyPanel(Image img, Sprite fallback = null)
    {
        if (img == null) return;
        Sprite s = PanelPopup ?? fallback;
        if (s != null)
        {
            img.sprite = s;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
    }

    public static void ApplyBarTrack(Image img)
    {
        if (img == null) return;
        Sprite s = ProgressBarBg;
        if (s != null)
        {
            img.sprite = s;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            return;
        }
        img.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
    }

    public static void ApplyBarFill(Image img, Sprite fillSprite)
    {
        if (img == null) return;
        Sprite s = fillSprite ?? FillGreen;
        if (s != null)
        {
            img.sprite = s;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.color = new Color(0.2f, 0.75f, 0.2f);
        }
    }

    public static Slider BuildHorizontalBar(Transform parent, string name, Sprite fillSprite,
        Vector2 anchorMin, Vector2 anchorMax, out Image fillImage)
    {
        var sliderGo = new GameObject(name);
        sliderGo.layer = 5;
        sliderGo.transform.SetParent(parent, false);

        var rt = sliderGo.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var bg = sliderGo.AddComponent<Image>();
        ApplyBarTrack(bg);
        bg.raycastTarget = false;

        var fillArea = new GameObject("Fill Area");
        fillArea.layer = 5;
        fillArea.transform.SetParent(sliderGo.transform, false);
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(6f, 6f);
        fillAreaRt.offsetMax = new Vector2(-6f, -6f);

        var fill = new GameObject("Fill");
        fill.layer = 5;
        fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        fillImage = fill.AddComponent<Image>();
        ApplyBarFill(fillImage, fillSprite);
        fillImage.raycastTarget = false;

        var slider = sliderGo.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = null;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.wholeNumbers = false;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.targetGraphic = null;
        return slider;
    }

    public static void StyleExistingSlider(Slider slider, Sprite fillSprite)
    {
        if (slider == null) return;

        Image bg = slider.GetComponent<Image>();
        ApplyBarTrack(bg);

        if (slider.fillRect != null)
        {
            Image fill = slider.fillRect.GetComponent<Image>();
            ApplyBarFill(fill, fillSprite);
        }
    }

    /// <summary>Re-skins scene-placed HUD images (mission, village banner, pause).</summary>
    public static void SkinSceneCanvas(Canvas canvas)
    {
        if (canvas == null) return;

        SkinNamedPanel("MissionPanel", PanelPopup);
        SkinNamedPanel("VillageProcessplanel", CoinBg ?? PanelPopup);

        GameObject pause = GameObject.Find("Pause");
        if (pause != null)
        {
            Image img = pause.GetComponent<Image>();
            if (img != null && IconPause != null)
            {
                img.sprite = IconPause;
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
                img.color = Color.white;
            }
        }

        Slider[] sliders = canvas.GetComponentsInChildren<Slider>(true);
        for (int i = 0; i < sliders.Length; i++)
        {
            Slider s = sliders[i];
            if (s == null || !s.gameObject.activeInHierarchy) continue;
            string n = s.gameObject.name.ToUpperInvariant();
            Sprite fill = FillGreen;
            if (n.Contains("WATER") || n.Contains("BUCKET")) fill = FillBlue;
            else if (n.Contains("MATERIAL")) fill = FillOrange;
            StyleExistingSlider(s, fill);
        }
    }

    static void SkinNamedPanel(string objectName, Sprite sprite)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null || sprite == null) return;

        Image rootImg = go.GetComponent<Image>();
        if (rootImg != null)
        {
            rootImg.sprite = sprite;
            rootImg.type = Image.Type.Sliced;
            rootImg.color = Color.white;
        }

        Image[] children = go.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == rootImg) continue;
            if (children[i].transform.name == "MissionPanelTitle") continue;
            if (children[i].sprite == null || children[i].sprite.name.Contains("PANEL1"))
            {
                children[i].sprite = sprite;
                children[i].type = Image.Type.Sliced;
                children[i].color = Color.white;
            }
        }
    }
}
