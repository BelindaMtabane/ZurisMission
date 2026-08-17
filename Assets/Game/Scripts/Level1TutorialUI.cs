using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// First 20% of MainGame: movement tutorial plus short tips for cactus, logs, heat, and snakes.
/// </summary>
public class Level1TutorialUI : MonoBehaviour
{
    const float TutorialEndProgress = 0.20f;
    const float JumpHintProgress = 0.12f;
    const float LogHintProgress = 0.165f;
    const float HeatHintProgress = 0.185f;
    const float SnakeHintProgress = 0.195f;

    GameObject leftHintRoot;
    GameObject rightHintRoot;
    GameObject centerBannerRoot;
    TMP_Text centerBannerText;
    Image centerBannerBg;

    Transform player;
    bool laneLeftUsed;
    bool laneRightUsed;
    int lastTipIndex = -1;

    static readonly TutorialTip[] Tips =
    {
        new TutorialTip(0.00f, "Use  A  and  D  to move between lanes"),
        new TutorialTip(0.05f, "Collect cactus for +20 water"),
        new TutorialTip(JumpHintProgress, "Press  SPACE  to jump over obstacles"),
        new TutorialTip(LogHintProgress, "Rolling log ahead — jump with  SPACE"),
        new TutorialTip(HeatHintProgress, "Heat wave soon! Collect cactus to stay hydrated"),
        new TutorialTip(SnakeHintProgress, "Watch for snake warnings — dodge with  A  or  D"),
    };

    struct TutorialTip
    {
        public readonly float Progress;
        public readonly string Message;

        public TutorialTip(float progress, string message)
        {
            Progress = progress;
            Message = message;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != "MainGame") return;
        if (FindFirstObjectByType<Level1TutorialUI>() != null) return;

        GameObject host = new GameObject("Level1TutorialUI");
        host.AddComponent<Level1TutorialUI>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainGame")
        {
            Destroy(gameObject);
            return;
        }

        Level1Progress.BindFromScene(FindPlayer());
        BuildUi();
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        CachePlayer();
        if (player == null) return;

        float progress = Level1Progress.Normalized(player.position.z);

        if (progress >= TutorialEndProgress)
        {
            HideAll();
            enabled = false;
            return;
        }

        UpdateLaneHints(progress);
        UpdateCenterTip(progress);
    }

    void UpdateLaneHints(float progress)
    {
        if (leftHintRoot == null || rightHintRoot == null) return;

        bool showLanes = progress < 0.10f || progress >= SnakeHintProgress - 0.02f;
        leftHintRoot.SetActive(showLanes && !laneLeftUsed);
        rightHintRoot.SetActive(showLanes && !laneRightUsed);

        if (PressedLaneLeft()) laneLeftUsed = true;
        if (PressedLaneRight()) laneRightUsed = true;
    }

    void UpdateCenterTip(float progress)
    {
        if (centerBannerRoot == null || centerBannerText == null) return;

        int tipIndex = 0;
        for (int i = Tips.Length - 1; i >= 0; i--)
        {
            if (progress >= Tips[i].Progress)
            {
                tipIndex = i;
                break;
            }
        }

        if (tipIndex != lastTipIndex)
        {
            lastTipIndex = tipIndex;
            centerBannerText.text = Tips[tipIndex].Message;
        }

        centerBannerRoot.SetActive(true);
    }

    void HideAll()
    {
        if (leftHintRoot != null) leftHintRoot.SetActive(false);
        if (rightHintRoot != null) rightHintRoot.SetActive(false);
        if (centerBannerRoot != null) centerBannerRoot.SetActive(false);
    }

    void BuildUi()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        leftHintRoot = CreateEdgeHint(
            canvas.transform,
            "Tutorial_Left",
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(24f, 24f),
            TextAlignmentOptions.BottomLeft,
            "◀  A");

        rightHintRoot = CreateEdgeHint(
            canvas.transform,
            "Tutorial_Right",
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-24f, 24f),
            TextAlignmentOptions.BottomRight,
            "D  ▶");

        centerBannerRoot = new GameObject("Tutorial_CenterBanner");
        centerBannerRoot.transform.SetParent(canvas.transform, false);
        RectTransform bannerRt = centerBannerRoot.AddComponent<RectTransform>();
        bannerRt.anchorMin = new Vector2(0.5f, 0.82f);
        bannerRt.anchorMax = new Vector2(0.5f, 0.82f);
        bannerRt.pivot = new Vector2(0.5f, 0.5f);
        bannerRt.sizeDelta = new Vector2(680f, 72f);

        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(centerBannerRoot.transform, false);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        centerBannerBg = bgGo.AddComponent<Image>();
        centerBannerBg.color = new Color(0.05f, 0.08f, 0.12f, 0.72f);
        centerBannerBg.raycastTarget = false;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(centerBannerRoot.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(16f, 8f);
        textRt.offsetMax = new Vector2(-16f, -8f);

        centerBannerText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            centerBannerText.font = TMP_Settings.defaultFontAsset;
        }

        centerBannerText.fontSize = 26f;
        centerBannerText.fontStyle = FontStyles.Bold;
        centerBannerText.alignment = TextAlignmentOptions.Center;
        centerBannerText.color = new Color(1f, 0.96f, 0.88f, 0.98f);
        centerBannerText.outlineWidth = 0.2f;
        centerBannerText.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        centerBannerText.raycastTarget = false;
        centerBannerText.text = Tips[0].Message;
    }

    static GameObject CreateEdgeHint(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        TextAlignmentOptions alignment,
        string text)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = new Vector2(200f, 80f);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(root.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TMP_Text tmp = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        tmp.text = text;
        tmp.fontSize = 28f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = alignment;
        tmp.color = new Color(1f, 1f, 1f, 0.92f);
        tmp.outlineWidth = 0.22f;
        tmp.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        tmp.raycastTarget = false;

        return root;
    }

    static bool PressedLaneLeft()
    {
        Keyboard kb = Keyboard.current;
        return Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.LeftArrow)
            || (kb != null && (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame));
    }

    static bool PressedLaneRight()
    {
        Keyboard kb = Keyboard.current;
        return Input.GetKeyDown(KeyCode.D)
            || Input.GetKeyDown(KeyCode.RightArrow)
            || (kb != null && (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame));
    }

    void CachePlayer()
    {
        if (player != null) return;
        player = FindPlayer();
    }

    static Transform FindPlayer()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform;
        GameObject p = GameObject.Find("Player");
        return p != null ? p.transform : null;
    }
}
