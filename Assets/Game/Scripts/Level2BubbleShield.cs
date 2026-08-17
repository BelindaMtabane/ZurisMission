using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Temporary water bubble that blocks Level 2 mud balls.
/// </summary>
public class Level2BubbleShield : MonoBehaviour
{
    public static Level2BubbleShield Instance { get; private set; }

    [SerializeField] private bool consumeOnBlock = true;
    [SerializeField] private float duration = 9f;

    GameObject visualRoot;
    GameObject overlayRoot;
    Image overlayImage;
    TMP_Text timerText;
    float timer;
    bool active;

    public static bool IsActive => Instance != null && Instance.active;

    public static void Activate(float seconds = 9f, bool consumeWhenBlocked = true)
    {
        EnsureInstance();
        if (Instance == null) return;

        Instance.consumeOnBlock = consumeWhenBlocked;
        Instance.duration = seconds;
        Instance.Begin(seconds);
    }

    public static bool TryBlockMudBall()
    {
        if (!IsActive) return false;

        Level2FeedbackUI.Show("BUBBLE BLOCKED!", new Color(0.35f, 0.85f, 1f), 1.4f);

        if (Instance.consumeOnBlock)
        {
            Instance.End();
        }

        return true;
    }

    static void EnsureInstance()
    {
        if (Instance != null) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        Instance = player.GetComponent<Level2BubbleShield>();
        if (Instance == null)
        {
            Instance = player.AddComponent<Level2BubbleShield>();
        }
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
            End();
            return;
        }

        if (visualRoot != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.05f;
            visualRoot.transform.localScale = Vector3.one * (1.55f * pulse);
        }

        if (overlayImage != null)
        {
            float glow = 0.12f + Mathf.Sin(Time.time * 2.4f) * 0.03f;
            overlayImage.color = new Color(0.28f, 0.68f, 1f, glow);
        }

        if (timerText != null)
        {
            timerText.text = $"SHIELD {Mathf.CeilToInt(timer)}s";
        }
    }

    void Begin(float seconds)
    {
        active = true;
        timer = seconds;
        if (visualRoot != null) visualRoot.SetActive(true);
        if (overlayRoot != null) overlayRoot.SetActive(true);
        Level2FeedbackUI.Show("BUBBLE SHIELD!", new Color(0.45f, 0.92f, 1f), 1.6f);
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
        visualRoot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualRoot.name = "BubbleShieldVisual";
        visualRoot.transform.SetParent(transform, false);
        visualRoot.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        visualRoot.transform.localScale = Vector3.one * 1.55f;

        Collider col = visualRoot.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Renderer r = visualRoot.GetComponent<Renderer>();
        Level2Primitives.MakeTransparent(r, new Color(0.45f, 0.82f, 1f, 0.28f));
    }

    void BuildOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        overlayRoot = new GameObject("BubbleShieldOverlay");
        overlayRoot.transform.SetParent(canvas.transform, false);

        RectTransform rt = overlayRoot.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        overlayImage = overlayRoot.AddComponent<Image>();
        overlayImage.color = new Color(0.28f, 0.68f, 1f, 0.12f);
        overlayImage.raycastTarget = false;

        GameObject textGo = new GameObject("ShieldTimer");
        textGo.transform.SetParent(overlayRoot.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.88f);
        textRt.anchorMax = new Vector2(0.5f, 0.88f);
        textRt.sizeDelta = new Vector2(280f, 40f);

        timerText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            timerText.font = TMP_Settings.defaultFontAsset;
        }

        timerText.fontSize = 22f;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = new Color(0.75f, 0.94f, 1f, 0.95f);
        timerText.raycastTarget = false;
    }
}
