using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Level2FeedbackUI : MonoBehaviour
{
    static Level2FeedbackUI instance;

    TMP_Text toastText;
    Image toastBackground;
    Coroutine hideRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneCatalog.Level2) return;
        if (instance != null) return;
        GameObject host = new GameObject("Level2FeedbackUI");
        instance = host.AddComponent<Level2FeedbackUI>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.Level2) return;
        if (instance != null) return;

        GameObject host = new GameObject("Level2FeedbackUI");
        instance = host.AddComponent<Level2FeedbackUI>();
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
    }

    public static void Show(string message, Color color, float duration = 1.6f)
    {
        if (instance == null) Boot();
        instance?.Display(message, color, duration);
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
        GameObject canvasObject = new GameObject("Level2FeedbackCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 145;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject root = new GameObject("Level2FeedbackToast");
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
        if (TMP_Settings.defaultFontAsset != null)
        {
            toastText.font = TMP_Settings.defaultFontAsset;
        }

        toastText.fontSize = 22f;
        toastText.color = HudTextStyle.Body;
        toastText.fontStyle = FontStyles.Bold;
        toastText.alignment = TextAlignmentOptions.Center;
        toastText.textWrappingMode = TextWrappingModes.Normal;
        toastText.overflowMode = TextOverflowModes.Ellipsis;
        toastText.raycastTarget = false;
        toastText.outlineWidth = 0.2f;
        toastText.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        toastText.gameObject.SetActive(false);
        toastBackground.gameObject.SetActive(false);
    }
}
