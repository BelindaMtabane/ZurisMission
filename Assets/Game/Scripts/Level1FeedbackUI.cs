using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Lightweight floating feedback for Level 1 (cactus, dodge, jump, heat).
/// </summary>
public class Level1FeedbackUI : MonoBehaviour
{
    static Level1FeedbackUI instance;

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
        if (scene.name != SceneCatalog.MainGame) return;
        if (instance != null) return;
        GameObject host = new GameObject("Level1FeedbackUI");
        instance = host.AddComponent<Level1FeedbackUI>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.MainGame) return;
        if (instance != null) return;

        GameObject host = new GameObject("Level1FeedbackUI");
        instance = host.AddComponent<Level1FeedbackUI>();
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
        if (instance == null)
        {
            Boot();
        }

        if (instance != null)
        {
            instance.Display(message, color, duration);
        }
    }

    void Display(string message, Color color, float duration)
    {
        if (toastText == null) BuildToast();
        if (toastText == null) return;

        toastText.text = message;
        toastText.color = color;

        toastText.gameObject.SetActive(true);
        if (toastBackground != null) toastBackground.gameObject.SetActive(true);

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
        GameObject cvGo = new GameObject("Level1FeedbackCanvas");
        cvGo.transform.SetParent(transform, false);
        Canvas canvas = cvGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 145; // above the status/timer HUD (140), below end-screens (200)
        var cs = cvGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();

        // Compact toast directly under the STATUS panel.
        GameObject root = new GameObject("Level1FeedbackToast");
        root.transform.SetParent(cvGo.transform, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.805f, 0.345f);
        rt.anchorMax = new Vector2(0.988f, 0.458f);
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
        if (toastBackground.sprite == null) toastBackground.color = new Color(0.30f, 0.20f, 0.12f, 0.96f);
        toastBackground.raycastTarget = false;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(root.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(16f, 10f);
        textRt.offsetMax = new Vector2(-16f, -10f);

        toastText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            toastText.font = TMP_Settings.defaultFontAsset;
        }

        toastText.fontSize = 20f;
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
