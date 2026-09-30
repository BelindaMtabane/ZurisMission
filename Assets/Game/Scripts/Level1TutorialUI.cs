using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Level 1 opening guide. Objects stay where they are.
/// Buttons and a short line teach press A or D, Space to jump and dodge, then collect versus avoid.
/// </summary>
public class Level1TutorialUI : MonoBehaviour
{
    const float GuideEndProgress = 0.36f;

    GameObject centerBannerRoot;
    TMP_Text centerBannerText;
    RectTransform buttonRow;
    Transform player;
    int lastTipIndex = -1;

    static readonly TutorialTip[] Tips =
    {
        new TutorialTip(0.00f, "Press A or D to move", "btn_a,btn_d"),
        new TutorialTip(0.02f, "Collect materials", "btn_collect"),
        new TutorialTip(0.05f, "Collect cactus for water", "btn_collect"),
        new TutorialTip(0.12f, "Press SPACE to jump and dodge", "btn_space,btn_jump,btn_dodge"),
        new TutorialTip(Level1Config.HeatWaveStartProgress, "Drink cactus in the heat", "btn_collect"),
        new TutorialTip(Level1Config.SnakeIntroProgress, "Avoid the snake. Press A or D", "btn_avoid,btn_a,btn_d"),
    };

    struct TutorialTip
    {
        public readonly float Progress;
        public readonly string Message;
        public readonly string Buttons;

        public TutorialTip(float progress, string message, string buttons)
        {
            Progress = progress;
            Message = message;
            Buttons = buttons;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneCatalog.MainGame) return;
        if (FindFirstObjectByType<Level1TutorialUI>() != null) return;
        new GameObject("Level1TutorialUI").AddComponent<Level1TutorialUI>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.MainGame) return;
        if (FindFirstObjectByType<Level1TutorialUI>() != null) return;
        new GameObject("Level1TutorialUI").AddComponent<Level1TutorialUI>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != SceneCatalog.MainGame)
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
        if (progress >= GuideEndProgress)
        {
            HideAll();
            enabled = false;
            return;
        }

        UpdateCenterTip(progress);
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
            ShowButtons(Tips[tipIndex].Buttons);
        }

        centerBannerRoot.SetActive(true);
    }

    void ShowButtons(string names)
    {
        if (buttonRow == null) return;

        for (int i = buttonRow.childCount - 1; i >= 0; i--)
            Destroy(buttonRow.GetChild(i).gameObject);

        if (string.IsNullOrEmpty(names)) return;

        string[] parts = names.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            Sprite sprite = LoadGuide(parts[i].Trim());
            if (sprite == null) continue;

            GameObject go = new GameObject(parts[i], typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(buttonRow, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            float width = sprite.rect.width / sprite.rect.height * 78f;
            rt.sizeDelta = new Vector2(Mathf.Clamp(width, 78f, 170f), 78f);
        }
    }

    static Sprite LoadGuide(string name)
    {
        Sprite sprite = Resources.Load<Sprite>("UI/GuideButtons/" + name);
        if (sprite != null) return sprite;

        Texture2D tex = Resources.Load<Texture2D>("UI/GuideButtons/" + name);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    void HideAll()
    {
        if (centerBannerRoot != null) centerBannerRoot.SetActive(false);
    }

    void BuildUi()
    {
        Canvas canvas = null;
        GameObject canvasGo = GameObject.Find("Canvas");
        if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        centerBannerRoot = new GameObject("Tutorial_CenterBanner");
        centerBannerRoot.transform.SetParent(canvas.transform, false);
        RectTransform bannerRt = centerBannerRoot.AddComponent<RectTransform>();
        bannerRt.anchorMin = new Vector2(0f, 1f);
        bannerRt.anchorMax = new Vector2(0f, 1f);
        bannerRt.pivot = new Vector2(0f, 1f);
        bannerRt.anchoredPosition = new Vector2(4f, -240f);
        bannerRt.sizeDelta = new Vector2(720f, 210f);

        GameObject row = new GameObject("GuideRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(centerBannerRoot.transform, false);
        buttonRow = row.GetComponent<RectTransform>();
        buttonRow.anchorMin = new Vector2(0f, 1f);
        buttonRow.anchorMax = new Vector2(1f, 1f);
        buttonRow.pivot = new Vector2(0.5f, 1f);
        buttonRow.anchoredPosition = new Vector2(0f, -4f);
        buttonRow.sizeDelta = new Vector2(0f, 86f);
        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        GameObject sign = new GameObject("Sign", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        sign.transform.SetParent(centerBannerRoot.transform, false);
        RectTransform signRt = sign.GetComponent<RectTransform>();
        signRt.anchorMin = new Vector2(0f, 0f);
        signRt.anchorMax = new Vector2(1f, 0f);
        signRt.pivot = new Vector2(0.5f, 0f);
        signRt.anchoredPosition = new Vector2(0f, 0f);
        signRt.sizeDelta = new Vector2(0f, 110f);
        Image signImage = sign.GetComponent<Image>();
        signImage.sprite = LoadGuide("sign");
        signImage.preserveAspect = false;
        signImage.raycastTarget = false;
        if (signImage.sprite == null)
            signImage.color = new Color(0.93f, 0.86f, 0.72f, 0.95f);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(sign.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(36f, 18f);
        textRt.offsetMax = new Vector2(-36f, -16f);

        centerBannerText = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            centerBannerText.font = TMP_Settings.defaultFontAsset;
        centerBannerText.fontSize = 26f;
        centerBannerText.fontStyle = FontStyles.Bold;
        centerBannerText.alignment = TextAlignmentOptions.Center;
        centerBannerText.color = new Color(0.28f, 0.16f, 0.08f);
        centerBannerText.raycastTarget = false;
        centerBannerText.text = Tips[0].Message;
        ShowButtons(Tips[0].Buttons);

        HudLayout.LayoutMissionPanel(HudLayout.FindMissionPanel());
        HudLayout.LayoutTutorialBanner();
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
