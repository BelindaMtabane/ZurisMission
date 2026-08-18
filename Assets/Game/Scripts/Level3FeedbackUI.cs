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
    TMP_Text[] sceneTankTexts;
    bool tankHudReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Level3") return;
        instance = null;
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
        BuildTankHud();
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

    void SetTankText(int tank1, int tank2, int tank3)
    {
        if (!tankHudReady) BuildTankHud();
        if (sceneTankTexts != null && sceneTankTexts.Length >= 3)
        {
            if (sceneTankTexts[0] != null) sceneTankTexts[0].text = $"Tank 1: {tank1}% / 100%";
            if (sceneTankTexts[1] != null) sceneTankTexts[1].text = $"Tank 2: {tank2}% / 100%";
            if (sceneTankTexts[2] != null) sceneTankTexts[2].text = $"Tank 3: {tank3}% / 100%";
            return;
        }

        if (tankHudText == null) BuildTankHud();
        if (tankHudText == null) return;
        tankHudText.text = $"Tank 1: {tank1}% / 100%\nTank 2: {tank2}% / 100%\nTank 3: {tank3}% / 100%";
    }

    void BuildTankHud()
    {
        if (tankHudReady) return;
        tankHudReady = true;

        TMP_Text t1 = FindSceneTankText("tank1");
        TMP_Text t2 = FindSceneTankText("tank2");
        TMP_Text t3 = FindSceneTankText("tank3");
        if (t1 != null && t2 != null && t3 != null)
        {
            tankHudText = null;
            sceneTankTexts = new[] { t1, t2, t3 };
            RemoveRuntimeTankHud();
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("Level3TankHud");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-18f, 18f);
        rt.sizeDelta = new Vector2(260f, 110f);

        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bg = bgGo.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.45f);
        bg.raycastTarget = false;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(root.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(12f, 8f);
        textRt.offsetMax = new Vector2(-12f, -8f);
        tankHudText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) tankHudText.font = TMP_Settings.defaultFontAsset;
        tankHudText.fontSize = 22f;
        tankHudText.fontStyle = FontStyles.Bold;
        tankHudText.alignment = TextAlignmentOptions.MidlineRight;
        tankHudText.raycastTarget = false;
        tankHudText.text = "Tank 1: 0% / 100%\nTank 2: 0% / 100%\nTank 3: 0% / 100%";
    }

    static void RemoveRuntimeTankHud()
    {
        GameObject runtimeHud = GameObject.Find("Level3TankHud");
        if (runtimeHud != null) Destroy(runtimeHud);
    }

    static TMP_Text FindSceneTankText(string objectName)
    {
        TMP_Text[] all = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].gameObject.name == objectName) return all[i];
        }
        return null;
    }
}
