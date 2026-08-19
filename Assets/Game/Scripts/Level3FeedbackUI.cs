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
        BuildTankHud();
        // Show starting state immediately so the player can see all three tanks at 0%
        SetTankText(0, 0, 0);
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
        // Tank texts in the scene might be renamed (e.g. "tank1 (1)") and/or created
        // after this UI component's Awake(). So we re-attempt wiring whenever we
        // don't have all 3 references yet.
        string Line(string label, int pct)
        {
            // Full green when 100%, yellow while in progress, white at 0%
            string col = pct >= 100 ? "#4AFF6A" : pct > 0 ? "#FFE040" : "#FFFFFF";
            return $"<color={col}>{label}: {pct}% / 100%</color>";
        }

        string combined = Line("Tank 1", tank1) + "\n" +
                          Line("Tank 2", tank2) + "\n" +
                          Line("Tank 3", tank3);

        // Update every matching scene HUD text, not just the first one Unity returns.
        UpdateAllSceneTankTexts("tank1", Line("Tank 1", tank1));
        UpdateAllSceneTankTexts("tank2", Line("Tank 2", tank2));
        UpdateAllSceneTankTexts("tank3", Line("Tank 3", tank3));

        // Always also update runtime HUD as a fallback (in case scene texts are missing).
        if (tankHudText == null) BuildTankHud();
        if (tankHudText == null) return;
        tankHudText.text = combined;
    }

    void BuildTankHud()
    {
        if (tankHudReady) return;

        TMP_Text t1 = FindSceneTankText("tank1");
        TMP_Text t2 = FindSceneTankText("tank2");
        TMP_Text t3 = FindSceneTankText("tank3");
        if (t1 != null && t2 != null && t3 != null)
        {
            tankHudReady = true;
            tankHudText = null;
            sceneTankTexts = new[] { t1, t2, t3 };
            RemoveRuntimeTankHud();
            // Ensure rich text so the colored <color=...> tags render.
            for (int i = 0; i < sceneTankTexts.Length; i++)
                if (sceneTankTexts[i] != null) sceneTankTexts[i].richText = true;
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        tankHudReady = true;

        GameObject root = new GameObject("Level3TankHud");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-18f, 40f);
        rt.sizeDelta = new Vector2(300f, 130f);

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
        tankHudText.fontSize = 26f;
        tankHudText.fontStyle = FontStyles.Bold;
        tankHudText.alignment = TextAlignmentOptions.MidlineRight;
        tankHudText.raycastTarget = false;
        tankHudText.richText = true;
        tankHudText.textWrappingMode = TextWrappingModes.NoWrap;
        tankHudText.text = "<color=#FFFFFF>Tank 1: 0% / 100%</color>\n<color=#FFFFFF>Tank 2: 0% / 100%</color>\n<color=#FFFFFF>Tank 3: 0% / 100%</color>";
    }

    static void RemoveRuntimeTankHud()
    {
        GameObject runtimeHud = GameObject.Find("Level3TankHud");
        if (runtimeHud != null) Destroy(runtimeHud);
    }

    static TMP_Text FindSceneTankText(string objectName)
    {
        string Normalize(string s)
        {
            if (s == null) return "";
            // keep only letters/digits so names like "tank 1 (1)" still match "tank1"
            System.Text.StringBuilder sb = new System.Text.StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        string normNeedle = Normalize(objectName);

        TMP_Text[] all = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            // Scene objects might be renamed by Unity (e.g. "tank1 (1)").
            // Match by prefix so we still find them reliably.
            string n = all[i].gameObject.name;
            if (n == objectName) return all[i];
            if (n != null && n.StartsWith(objectName)) return all[i];
            // Some scenes might have different casing.
            if (n != null && n.ToLower().StartsWith(objectName.ToLower())) return all[i];

            string normHay = Normalize(n);
            if (!string.IsNullOrEmpty(normNeedle) && normHay.StartsWith(normNeedle)) return all[i];
            if (!string.IsNullOrEmpty(normNeedle) && normHay.Contains(normNeedle)) return all[i];
        }
        return null;
    }

    static void UpdateAllSceneTankTexts(string objectName, string text)
    {
        string Normalize(string s)
        {
            if (s == null) return "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        string normNeedle = Normalize(objectName);
        TMP_Text[] all = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            string n = all[i].gameObject.name;
            string normHay = Normalize(n);
            bool match =
                n == objectName ||
                (n != null && n.StartsWith(objectName)) ||
                (n != null && n.ToLower().StartsWith(objectName.ToLower())) ||
                (!string.IsNullOrEmpty(normNeedle) && normHay.StartsWith(normNeedle)) ||
                (!string.IsNullOrEmpty(normNeedle) && normHay.Contains(normNeedle));

            if (!match) continue;
            all[i].richText = true;
            all[i].text = text;
        }
    }
}
