using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level3FeedbackUI : MonoBehaviour
{
    static Level3FeedbackUI instance;
    TMP_Text toastText;
    Image toastBackground;
    Coroutine hideRoutine;
    TMP_Text tankHudText;
    bool tankHudReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneCatalog.Level3) return;
        instance = null;
        Ensure();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.Level3) return;
        Ensure();
    }

    static void Ensure()
    {
        if (instance != null) return;
        Level3FeedbackUI existing = FindFirstObjectByType<Level3FeedbackUI>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject host = new GameObject("Level3FeedbackUI");
        instance = host.AddComponent<Level3FeedbackUI>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildToast();
        HideSceneTankLabels();
        BuildTankHud();
        SetTankText(0, 0, 0);
    }

    void Start()
    {
        HideSceneTankLabels();
        if (tankHudText == null) BuildTankHud();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static void Show(string message, Color color, float duration = 1.6f)
    {
        if (instance == null) Ensure();
        instance?.Display(message, color, duration);
    }

    public static void UpdateTanks(int tank1, int tank2, int tank3)
    {
        if (instance == null) Ensure();
        instance?.HideSceneTankLabels();
        instance?.SetTankText(tank1, tank2, tank3);
    }

    void Display(string message, Color color, float duration)
    {
        if (toastText == null) BuildToast();
        if (toastText == null) return;

        toastText.text = message;
        toastText.color = color;
        if (toastBackground != null)
        {
            if (toastBackground.sprite != null) toastBackground.color = Color.white;
            toastBackground.gameObject.SetActive(true);
        }

        toastText.gameObject.SetActive(true);
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter(duration));
    }

    IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (toastText != null) toastText.gameObject.SetActive(false);
        if (toastBackground != null) toastBackground.gameObject.SetActive(false);
        hideRoutine = null;
    }

    void BuildToast()
    {
        GameObject canvasObject = new GameObject("Level3FeedbackCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 145;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject root = new GameObject("Level3FeedbackToast");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.70f, 0.36f);
        rt.anchorMax = new Vector2(0.985f, 0.54f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        toastBackground = bgGo.AddComponent<Image>();
        AdventureUI.ApplyPanel(toastBackground);
        if (toastBackground.sprite == null)
            toastBackground.color = new Color(0.10f, 0.16f, 0.12f, 0.94f);
        else
            toastBackground.color = Color.white;
        toastBackground.raycastTarget = false;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(root.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(18f, 12f);
        textRt.offsetMax = new Vector2(-18f, -12f);
        toastText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) toastText.font = TMP_Settings.defaultFontAsset;
        toastText.fontSize = 22f;
        toastText.fontStyle = FontStyles.Bold;
        toastText.alignment = TextAlignmentOptions.Center;
        toastText.textWrappingMode = TextWrappingModes.Normal;
        toastText.overflowMode = TextOverflowModes.Ellipsis;
        toastText.color = HudTextStyle.Body;
        toastText.outlineWidth = 0.22f;
        toastText.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        toastText.raycastTarget = false;
        toastText.gameObject.SetActive(false);
        toastBackground.gameObject.SetActive(false);
    }

    void HideSceneTankLabels()
    {
        TMP_Text[] all = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            Transform parent = all[i].transform.parent;
            if (parent != null && parent.name == "Level3TankHud") continue;
            string n = all[i].gameObject.name;
            if (string.IsNullOrEmpty(n)) continue;
            string compact = n.Replace(" ", "").ToLowerInvariant();
            if (compact.StartsWith("tank1") || compact.StartsWith("tank2") || compact.StartsWith("tank3"))
                all[i].gameObject.SetActive(false);
        }
    }

    void SetTankText(int tank1, int tank2, int tank3)
    {
        if (tankHudText == null) BuildTankHud();
        if (tankHudText == null) return;

        string Line(string label, int pct)
        {
            string col = pct >= 100 ? "#4AFF6A" : pct > 0 ? "#FFE040" : "#FFFFFF";
            return $"<color={col}>{label}: {pct}% / 100%</color>";
        }

        tankHudText.text =
            Line("Tank 1", tank1) + "\n" +
            Line("Tank 2", tank2) + "\n" +
            Line("Tank 3", tank3);
        tankHudText.gameObject.SetActive(true);
        if (tankHudText.transform.parent != null)
            tankHudText.transform.parent.gameObject.SetActive(true);
    }

    void BuildTankHud()
    {
        if (tankHudReady && tankHudText != null) return;

        GameObject existing = GameObject.Find("Level3TankHud");
        if (existing != null) Destroy(existing);

        tankHudReady = true;
        Canvas canvas = null;
        Transform toastCanvas = toastText != null ? toastText.canvas.transform : null;
        if (toastCanvas != null) canvas = toastCanvas.GetComponent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Level3TankCanvas");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 144;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        GameObject root = new GameObject("Level3TankHud");
        root.layer = 5;
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.805f, 0.68f);
        rt.anchorMax = new Vector2(0.988f, 0.97f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        GameObject bgGo = new GameObject("Background");
        bgGo.layer = 5;
        bgGo.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bg = bgGo.AddComponent<Image>();
        AdventureUI.ApplyPanel(bg);
        if (bg.sprite == null)
            bg.color = new Color(0.10f, 0.16f, 0.12f, 0.94f);
        else
            bg.color = Color.white;
        bg.raycastTarget = false;

        GameObject titleGo = new GameObject("Title");
        titleGo.layer = 5;
        titleGo.transform.SetParent(root.transform, false);
        RectTransform titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.06f, 0.78f);
        titleRt.anchorMax = new Vector2(0.94f, 0.96f);
        titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;
        TextMeshProUGUI title = titleGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) title.font = TMP_Settings.defaultFontAsset;
        title.text = "TANKS";
        title.fontSize = 22f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(1f, 0.92f, 0.55f);
        title.outlineWidth = 0.22f;
        title.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        title.raycastTarget = false;

        GameObject textGo = new GameObject("Text");
        textGo.layer = 5;
        textGo.transform.SetParent(root.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.08f, 0.08f);
        textRt.anchorMax = new Vector2(0.94f, 0.76f);
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
        tankHudText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) tankHudText.font = TMP_Settings.defaultFontAsset;
        tankHudText.fontSize = 22f;
        tankHudText.fontStyle = FontStyles.Bold;
        tankHudText.alignment = TextAlignmentOptions.MidlineLeft;
        tankHudText.outlineWidth = 0.22f;
        tankHudText.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        tankHudText.raycastTarget = false;
        tankHudText.richText = true;
        tankHudText.textWrappingMode = TextWrappingModes.NoWrap;
        tankHudText.lineSpacing = 8f;
    }
}
