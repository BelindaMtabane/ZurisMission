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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Level3") return;
        Ensure();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != "Level3") return;
        Ensure();
    }

    static void Ensure()
    {
        if (instance != null) return;
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
    }

    public static void Show(string message, Color color, float duration = 1.6f)
    {
        if (instance == null) Ensure();
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
            Color bg = color;
            bg.a = 0.22f;
            toastBackground.color = bg;
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
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("Level3FeedbackToast");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.72f);
        rt.anchorMax = new Vector2(0.5f, 0.72f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(560f, 72f);

        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        toastBackground = bgGo.AddComponent<Image>();
        toastBackground.raycastTarget = false;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(root.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(12f, 6f);
        textRt.offsetMax = new Vector2(-12f, -6f);
        toastText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) toastText.font = TMP_Settings.defaultFontAsset;
        toastText.fontSize = 28f;
        toastText.fontStyle = FontStyles.Bold;
        toastText.alignment = TextAlignmentOptions.Center;
        toastText.raycastTarget = false;
        toastText.gameObject.SetActive(false);
        toastBackground.gameObject.SetActive(false);
    }
}
