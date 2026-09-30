using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Compact bottom-left visual guide showing the main hazards in each level.
/// </summary>
public class ObstacleGuideHUD : MonoBehaviour
{
    const string SuppliedImageFolder =
        @"C:\Users\TUF\.cursor\projects\c-Users-TUF-source-repos-WILSummativePortfolio-ZurisMission\assets";

    static readonly Dictionary<string, string> SuppliedImages = new()
    {
        { "log", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-a54bc94f-3100-4bae-8bd3-e4bae3672e0f.png" },
        { "snake", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-647f8f9d-08f0-4649-9507-4652c6e942f9.png" },
        { "wolf", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-04661b51-837d-4550-b612-1fbbf6c1606e.png" },
        { "warthog", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-63f571f1-6b37-4b55-9359-e2289c506018.png" },
        { "mud", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-a4ba67ff-be41-48c3-b167-90e41a300fc9.png" },
        { "heat_wave", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-9585d911-fc82-4bda-88bf-a7690cf7ca96.png" },
        { "acid", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-8eaddbaa-495b-4deb-958d-06dd54b6ce5a.png" },
        { "sand_pit", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-3e3fc533-5097-487e-9c02-32f9cd6782c2.png" },
        { "lightning", "c__Users_TUF_AppData_Roaming_Cursor_User_workspaceStorage_e0747c6623e04786d4e6c3de8a7dc948_images_image-a10e201b-9b91-4d33-981a-4348f9c309b8.png" }
    };

    static readonly Dictionary<string, Sprite> RuntimeSprites = new();
    static ObstacleGuideHUD instance;
    readonly Dictionary<string, Image> hazardImages = new();
    readonly Dictionary<string, Coroutine> hitFlashes = new();

    readonly struct HazardIcon
    {
        public readonly string Label;
        public readonly string ImageName;
        public readonly Color Colour;

        public HazardIcon(string label, string imageName, Color colour)
        {
            Label = label;
            ImageName = imageName;
            Colour = colour;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        TryCreate(SceneManager.GetActiveScene().name);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TryCreate(scene.name);

    static void TryCreate(string sceneName)
    {
        if (!SceneCatalog.IsRunnerScene(sceneName)) return;
        if (FindFirstObjectByType<ObstacleGuideHUD>() != null) return;
        GameObject host = new GameObject("AvoidGuideHUD");
        host.layer = 5;
        host.AddComponent<ObstacleGuideHUD>();
    }

    public static void EnsureForScene(string sceneName) => TryCreate(sceneName);

    void Awake() => instance = this;

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static void NotifyHit(string imageName)
    {
        instance?.FlashHazard(imageName);
    }

    void Start()
    {
        Canvas canvas = BuildDedicatedCanvas();
        BuildGuide(canvas.transform, HazardsForScene(SceneCatalog.ActiveName));
    }

    void FlashHazard(string imageName)
    {
        if (!hazardImages.TryGetValue(imageName, out Image artwork) || artwork == null) return;
        if (hitFlashes.TryGetValue(imageName, out Coroutine running) && running != null)
            StopCoroutine(running);
        hitFlashes[imageName] = StartCoroutine(FlashRoutine(imageName, artwork));
    }

    IEnumerator FlashRoutine(string imageName, Image artwork)
    {
        Color original = artwork.sprite != null ? Color.white : artwork.color;
        RectTransform rect = artwork.rectTransform;
        rect.localScale = Vector3.one * 1.12f;
        artwork.color = new Color(1f, 0.12f, 0.08f, 1f);
        yield return new WaitForSecondsRealtime(0.85f);
        if (artwork != null)
        {
            artwork.color = original;
            rect.localScale = Vector3.one;
        }
        hitFlashes.Remove(imageName);
    }

    Canvas BuildDedicatedCanvas()
    {
        GameObject canvasObject = new GameObject("ObstacleInventoryCanvas");
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 155;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static HazardIcon[] HazardsForScene(string scene)
    {
        if (SceneCatalog.IsLevel2(scene))
        {
            return new[]
            {
                new HazardIcon("MUD", "mud", new Color(0.58f, 0.32f, 0.14f)),
                new HazardIcon("WARTHOG", "warthog", new Color(0.72f, 0.48f, 0.25f)),
                new HazardIcon("LOG", "log", new Color(0.72f, 0.40f, 0.16f)),
                new HazardIcon("WOLF", "wolf", new Color(0.76f, 0.78f, 0.82f))
            };
        }

        if (SceneCatalog.IsLevel3(scene))
        {
            return new[]
            {
                new HazardIcon("MUD", "mud", new Color(0.58f, 0.32f, 0.14f)),
                new HazardIcon("SNAKE", "snake", new Color(0.92f, 0.75f, 0.12f)),
                new HazardIcon("ACID", "acid", new Color(0.42f, 1f, 0.25f)),
                new HazardIcon("LIGHTNING", "lightning", new Color(1f, 0.90f, 0.20f)),
                new HazardIcon("WARTHOG", "warthog", new Color(0.72f, 0.48f, 0.25f)),
                new HazardIcon("WOLF", "wolf", new Color(0.76f, 0.78f, 0.82f))
            };
        }

        return new[]
        {
            new HazardIcon("SNAKE", "snake", new Color(0.92f, 0.75f, 0.12f)),
            new HazardIcon("LOG", "log", new Color(0.72f, 0.40f, 0.16f)),
            new HazardIcon("HEAT", "heat_wave", new Color(1f, 0.42f, 0.14f)),
            new HazardIcon("WOLF", "wolf", new Color(0.76f, 0.78f, 0.82f))
        };
    }

    void BuildGuide(Transform canvas, HazardIcon[] hazards)
    {
        GameObject panel = new GameObject("ObstacleGuidePanel");
        panel.layer = 5;
        panel.transform.SetParent(canvas, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;
        panelRect.pivot = Vector2.zero;
        panelRect.anchoredPosition = new Vector2(18f, 108f);
        int columns = Mathf.Min(6, hazards.Length);
        int rows = Mathf.CeilToInt(hazards.Length / (float)columns);
        panelRect.sizeDelta = new Vector2(620f, rows > 1 ? 246f : 154f);

        Image panelImage = panel.AddComponent<Image>();
        AdventureUI.ApplyPanel(panelImage);
        if (panelImage.sprite == null)
            panelImage.color = new Color(0.025f, 0.045f, 0.08f, 0.92f);
        else
            panelImage.color = new Color(1f, 1f, 1f, 0.96f);
        panelImage.raycastTarget = false;

        float cardsTop = rows > 1 ? 0.84f : 0.75f;
        CreateText(panel.transform, "Title", "OBSTACLES TO AVOID", 22f, FontStyles.Bold,
            Color.white, new Vector2(0.04f, cardsTop), new Vector2(0.96f, 0.98f));

        float gap = 0.012f;
        float start = 0.04f;
        float cardWidth = (0.92f - gap * (columns - 1)) / columns;
        float usableHeight = cardsTop - 0.06f;
        float rowGap = rows > 1 ? 0.025f : 0f;
        float cardHeight = (usableHeight - rowGap * (rows - 1)) / rows;
        for (int i = 0; i < hazards.Length; i++)
        {
            int column = i % columns;
            int row = i / columns;
            float xMin = start + column * (cardWidth + gap);
            float xMax = xMin + cardWidth;
            float yMax = cardsTop - row * (cardHeight + rowGap);
            float yMin = yMax - cardHeight;
            CreateHazardCard(panel.transform, hazards[i],
                new Vector2(xMin, yMin), new Vector2(xMax, yMax));
        }
    }

    void CreateHazardCard(Transform parent, HazardIcon hazard, Vector2 min, Vector2 max)
    {
        GameObject card = new GameObject("Hazard_" + hazard.Label);
        card.layer = 5;
        card.transform.SetParent(parent, false);
        RectTransform rect = card.AddComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        Image background = card.AddComponent<Image>();
        background.color = new Color(0.02f, 0.025f, 0.04f, 0.86f);
        background.raycastTarget = false;

        GameObject iconPlate = new GameObject("IconPlate");
        iconPlate.layer = 5;
        iconPlate.transform.SetParent(card.transform, false);
        RectTransform plateRect = iconPlate.AddComponent<RectTransform>();
        plateRect.anchorMin = new Vector2(0.19f, 0.35f);
        plateRect.anchorMax = new Vector2(0.81f, 0.94f);
        plateRect.offsetMin = plateRect.offsetMax = Vector2.zero;
        Image artwork = iconPlate.AddComponent<Image>();
        artwork.sprite = LoadObstacleSprite(hazard.ImageName);
        artwork.preserveAspect = true;
        artwork.raycastTarget = false;
        hazardImages[hazard.ImageName] = artwork;
        if (artwork.sprite != null)
        {
            artwork.color = Color.white;
        }
        else
        {
            artwork.color = new Color(hazard.Colour.r, hazard.Colour.g, hazard.Colour.b, 0.28f);
            CreateText(iconPlate.transform, "MissingImage", "IMAGE", 13f, FontStyles.Bold,
                Color.white, new Vector2(0f, 0f), new Vector2(1f, 1f));
        }

        CreateText(card.transform, "Label", hazard.Label, 13f, FontStyles.Bold,
            Color.white, new Vector2(0.02f, 0.03f), new Vector2(0.98f, 0.31f));
    }

    static TMP_Text CreateText(
        Transform parent, string name, string value, float size, FontStyles style,
        Color colour, Vector2 min, Vector2 max)
    {
        GameObject textObject = new GameObject(name);
        textObject.layer = 5;
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.color = colour;
        text.outlineWidth = 0.22f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.9f);
        text.raycastTarget = false;
        return text;
    }

    static Sprite LoadObstacleSprite(string imageName)
    {
        Sprite resourceSprite = Resources.Load<Sprite>("UI/Obstacles/" + imageName);
        if (resourceSprite != null) return resourceSprite;
        if (RuntimeSprites.TryGetValue(imageName, out Sprite cached) && cached != null) return cached;
        if (!SuppliedImages.TryGetValue(imageName, out string suppliedFile)) return null;

        string path = Path.Combine(SuppliedImageFolder, suppliedFile);
        if (!File.Exists(path)) return null;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            Destroy(texture);
            return null;
        }

        texture.name = imageName + "_ObstacleHud";
        texture.wrapMode = TextureWrapMode.Clamp;
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        sprite.name = imageName;
        RuntimeSprites[imageName] = sprite;
        return sprite;
    }
}
