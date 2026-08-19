using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level3LeafProtection : MonoBehaviour
{
    public static Level3LeafProtection Instance { get; private set; }

    const float DefaultDuration = 10f;

    GameObject visualRoot;
    GameObject overlayRoot;
    TMP_Text timerText;
    float timer;
    bool active;

    public static bool IsActive => Instance != null && Instance.active;

    public static void Activate(float seconds = DefaultDuration)
    {
        EnsureInstance();
        Instance?.Begin(seconds);
    }

    public static bool TryBlockAcid()
    {
        return IsActive;
    }

    static void EnsureInstance()
    {
        if (Instance != null) return;
        GameObject player = GameObject.Find("Player");
        if (player == null) return;
        Instance = player.GetComponent<Level3LeafProtection>();
        if (Instance == null) Instance = player.AddComponent<Level3LeafProtection>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        BuildVisual();
        BuildOverlay();
        End();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (overlayRoot != null) Destroy(overlayRoot);
    }

    void Update()
    {
        if (!active) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Level3FeedbackUI.Show("LEAF PROTECTION DISABLED", new Color(0.45f, 0.75f, 0.35f), 1.2f);
            End();
            return;
        }

        if (visualRoot != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.05f;
            visualRoot.transform.localScale = Vector3.one * (1.45f * pulse);
        }

        if (timerText != null) timerText.text = $"LEAF {Mathf.CeilToInt(timer)}";
    }

    void Begin(float seconds)
    {
        active = true;
        timer = seconds;
        if (visualRoot != null) visualRoot.SetActive(true);
        if (overlayRoot != null) overlayRoot.SetActive(true);
        Level3FeedbackUI.Show("LEAF PROTECTION 10s", new Color(0.4f, 0.9f, 0.35f), 1.5f);
    }

    void End()
    {
        active = false;
        timer = 0f;
        if (visualRoot != null) visualRoot.SetActive(false);
        if (overlayRoot != null) overlayRoot.SetActive(false);
    }

    void BuildVisual()
    {
        visualRoot = new GameObject("LeafProtectionVisual");
        visualRoot.transform.SetParent(transform, false);
        visualRoot.transform.localPosition = new Vector3(0f, 1.1f, 0f);

        GameObject leafA = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leafA.transform.SetParent(visualRoot.transform, false);
        leafA.transform.localScale = new Vector3(1.4f, 0.35f, 0.9f);
        leafA.transform.localPosition = new Vector3(-0.35f, 0.2f, 0f);
        DisableCol(leafA);
        Level3Primitives.MakeTransparent(leafA.GetComponent<Renderer>(), new Color(0.35f, 0.85f, 0.28f, 0.32f));

        GameObject leafB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leafB.transform.SetParent(visualRoot.transform, false);
        leafB.transform.localScale = new Vector3(1.3f, 0.32f, 0.85f);
        leafB.transform.localPosition = new Vector3(0.4f, -0.1f, 0.1f);
        DisableCol(leafB);
        Level3Primitives.MakeTransparent(leafB.GetComponent<Renderer>(), new Color(0.28f, 0.72f, 0.22f, 0.3f));
    }

    void BuildOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        overlayRoot = new GameObject("LeafProtectionHud");
        overlayRoot.transform.SetParent(canvas.transform, false);
        RectTransform rt = overlayRoot.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.9f);
        rt.anchorMax = new Vector2(0.5f, 0.9f);
        rt.sizeDelta = new Vector2(240f, 36f);
        timerText = overlayRoot.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) timerText.font = TMP_Settings.defaultFontAsset;
        timerText.fontSize = 22f;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = new Color(0.45f, 0.95f, 0.4f);
        timerText.raycastTarget = false;
    }

    static void DisableCol(GameObject go)
    {
        Collider col = go.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}
