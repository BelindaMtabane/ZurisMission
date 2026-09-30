using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Re-anchors the scene HUD so Mission, Village Progress, and Pause no longer
/// collide with the runtime status panel, timer, or tutorial banner.
/// </summary>
public class HudLayout : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        SceneManager.sceneLoaded += (_, __) => TryCreate();
        TryCreate();
    }

    static void TryCreate()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!SceneCatalog.IsRunnerScene(scene) && scene != SceneCatalog.Level3End) return;
        if (FindFirstObjectByType<HudLayout>() != null) return;
        new GameObject("HudLayout").AddComponent<HudLayout>();
    }

    void Awake()
    {
        Apply();
    }

    void Start()
    {
        // Runtime HUD canvases spawn in other AfterSceneLoad callbacks; re-apply
        // once so Pause stays above them and the scene scaler is consistent.
        Apply();
        StartCoroutine(ApplyTextStyleAfterHudSpawn());
    }

    static IEnumerator ApplyTextStyleAfterHudSpawn()
    {
        yield return null;
        yield return null;
        LayoutMissionPanel(FindRect("MissionPanel"));
        LayoutVillageProgressPanel(FindRect("VillageProcessplanel"));
        HudTextStyle.ApplyRunnerSceneHud();
        LayoutMissionPanel(FindRect("MissionPanel"));
        LayoutVillageProgressPanel(FindRect("VillageProcessplanel"));
        LayoutTutorialBanner();
        yield return null;
        LayoutMissionPanel(FindRect("MissionPanel"));
        LayoutVillageProgressPanel(FindRect("VillageProcessplanel"));
        LayoutTutorialBanner();
        Canvas.ForceUpdateCanvases();
    }

    public static void LoadMainGame()
    {
        SceneManager.LoadScene(SceneCatalog.MainGame);
    }

    public static void Apply()
    {
        Canvas canvas = FindSceneCanvas();
        if (canvas == null) return;

        ConfigureScaler(canvas);

        RectTransform village = FindRect("VillageProcessplanel");
        PinCorner(
            village,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -72f),
            Vector3.one);
        LayoutVillageProgressPanel(village);
        RaiseSorting(village, 135);

        PinCorner(
            FindRect("MissionPanel"),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(4f, -24f),
            Vector3.one);

        RectTransform pause = FindRect("Pause");
        PinCorner(
            pause,
            new Vector2(0f, 1f),
            new Vector2(0.5f, 0.5f),
            new Vector2(108f, -40f),
            new Vector3(0.52f, 1.9f, 1f));
        RaiseSorting(pause, 210);
        EnsureSoundToggle(canvas);

        AdventureUI.SkinSceneCanvas(canvas);
        HudTextStyle.ApplyRunnerSceneHud();
        LayoutMissionPanel(FindRect("MissionPanel"));
        LayoutVillageProgressPanel(FindRect("VillageProcessplanel"));
        LayoutTutorialBanner();
    }

    static void EnsureSoundToggle(Canvas canvas)
    {
        if (canvas == null) return;

        GameObject go = GameObject.Find("HudSoundToggle");
        if (go == null)
        {
            go = new GameObject("HudSoundToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas.transform, false);
            go.layer = 5;

            Image img = go.GetComponent<Image>();
            Sprite art = AdventureUI.BtnSmall ?? AdventureUI.BtnMedium ?? AdventureUI.PanelPopup;
            if (art != null)
            {
                img.sprite = art;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.12f, 0.38f, 0.16f, 0.94f);
            }

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(OnHudSoundClicked);

            GameObject labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            TMP_Text label = labelGo.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.fontSize = 16f;
            label.color = Color.white;
            label.raycastTarget = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = 18f;
            RectTransform labelRt = label.rectTransform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(8f, 6f);
            labelRt.offsetMax = new Vector2(-8f, -6f);

            HudSoundToggle view = go.AddComponent<HudSoundToggle>();
            view.Bind(label, img);
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(210f, -40f);
        rt.sizeDelta = new Vector2(108f, 52f);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        RaiseSorting(rt, 211);
        go.transform.SetAsLastSibling();
    }

    static void OnHudSoundClicked()
    {
        bool turningOff = !GameAudio.Muted;
        if (turningOff) GameAudio.PlayClick();
        GameAudio.ToggleMuted();
        if (!turningOff) GameAudio.PlayClick();
    }

    /// <summary>Phase-1 tip banner under the mission panel, inset left from village progress.</summary>
    public static void LayoutTutorialBanner()
    {
        GameObject go = GameObject.Find("Tutorial_CenterBanner");
        if (go == null) return;

        RectTransform banner = go.GetComponent<RectTransform>();
        if (banner == null) return;

        const float gapBelowMission = 10f;
        const float bannerHeight = 64f;

        banner.localScale = Vector3.one;
        banner.anchorMin = new Vector2(0f, 1f);
        banner.anchorMax = new Vector2(0f, 1f);
        banner.pivot = new Vector2(0f, 1f);

        TMP_Text label = banner.GetComponentInChildren<TMP_Text>(true);

        RectTransform mission = FindRect("MissionPanel");
        if (mission != null)
        {
            float left = mission.anchoredPosition.x;
            float topOffset = -mission.anchoredPosition.y + mission.sizeDelta.y + gapBelowMission;
            banner.anchoredPosition = new Vector2(left, -topOffset);

            float width = mission.sizeDelta.x;
            if (label != null)
            {
                label.ForceMeshUpdate();
                width = Mathf.Min(width, label.preferredWidth + 28f);
            }

            banner.sizeDelta = new Vector2(width, bannerHeight);
            ApplyTutorialBannerTextInset(label);
            return;
        }

        banner.anchoredPosition = new Vector2(4f, -240f);
        banner.sizeDelta = new Vector2(620f, bannerHeight);
        ApplyTutorialBannerTextInset(label);
    }

    static void ApplyTutorialBannerTextInset(TMP_Text label)
    {
        if (label == null) return;
        RectTransform textRt = label.rectTransform;
        textRt.offsetMin = new Vector2(46f, 8f);
        textRt.offsetMax = new Vector2(-16f, -8f);
    }

    static void LayoutVillageProgressPanel(RectTransform panel)
    {
        if (panel == null) return;

        panel.localScale = Vector3.one;
        const float panelHeight = 128f;
        const float panelWidth = 545f;

        Image rootBg = panel.GetComponent<Image>();
        Sprite banner = AdventureUI.CoinBg;
        if (rootBg != null)
        {
            if (banner != null)
            {
                rootBg.sprite = banner;
                rootBg.color = Color.white;
            }
            rootBg.type = Image.Type.Sliced;
            rootBg.preserveAspect = false;
            rootBg.raycastTarget = false;
        }

        panel.sizeDelta = new Vector2(panelWidth, panelHeight);

        Image[] images = panel.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null || images[i] == rootBg) continue;
            images[i].enabled = false;
        }

        TMP_Text label = panel.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.transform.SetParent(panel, false);
            label.gameObject.SetActive(true);
            label.transform.SetAsLastSibling();
            HudTextStyle.ApplyVillageProgressText(label);
        }

        PositionVillagePanel(panel);
    }

    /// <summary>
    /// Sit village progress to the right of the mission card with a gap.
    /// Level 3 also keeps clear of the top-right TANKS HUD.
    /// </summary>
    static void PositionVillagePanel(RectTransform panel)
    {
        if (panel == null) return;

        bool level3 = SceneCatalog.IsLevel3(SceneCatalog.ActiveName);
        const float gap = 40f;
        const float defaultHeight = 128f;
        float preferredWidth = level3 ? 400f : 545f;
        float topY = level3 ? -24f : -72f;

        float canvasWidth = OverlayCanvasWidth();
        float rightLimit = canvasWidth - 16f;
        if (level3)
            rightLimit = canvasWidth * 0.805f - 12f;

        RectTransform mission = FindRect("MissionPanel");
        float missionRight = 4f;
        if (mission != null)
            missionRight = mission.anchoredPosition.x + mission.sizeDelta.x + 8f;

        float minLeft = missionRight + gap;
        float width = preferredWidth;
        float centeredLeft = canvasWidth * 0.5f - width * 0.5f;
        bool wouldOverlapCenter = centeredLeft < minLeft || centeredLeft + width > rightLimit;

        if (level3 || wouldOverlapCenter)
        {
            width = Mathf.Clamp(rightLimit - minLeft, 300f, preferredWidth);
            PinCorner(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(minLeft, topY), Vector3.one);
            panel.sizeDelta = new Vector2(width, defaultHeight);
            return;
        }

        PinCorner(
            panel,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, topY),
            Vector3.one);
        panel.sizeDelta = new Vector2(width, defaultHeight);
    }

    static float OverlayCanvasWidth()
    {
        Canvas canvas = FindSceneCanvas();
        if (canvas == null) return 1920f;

        RectTransform canvasRt = canvas.GetComponent<RectTransform>();
        if (canvasRt != null && canvasRt.rect.width > 8f)
            return canvasRt.rect.width;

        return canvas.pixelRect.width / Mathf.Max(canvas.scaleFactor, 0.01f);
    }

    static Canvas FindSceneCanvas()
    {
        GameObject go = GameObject.Find("Canvas");
        if (go != null)
        {
            Canvas named = go.GetComponent<Canvas>();
            if (named != null) return named;
        }

        Canvas[] all = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].renderMode == RenderMode.ScreenSpaceOverlay)
                return all[i];
        }

        return null;
    }

    static void ConfigureScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    public static RectTransform FindMissionPanel() => FindRect("MissionPanel");

    static RectTransform FindRect(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<RectTransform>() : null;
    }

    static void PinCorner(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector3 scale)
    {
        if (rt == null) return;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.localRotation = Quaternion.identity;
        rt.localScale = scale;
    }

    /// <summary>
    /// Enlarges the mission stone panel and stacks objective lines with spacing for readable TMP.
    /// </summary>
    public static void LayoutMissionPanel(RectTransform panel)
    {
        if (panel == null) return;

        panel.localScale = Vector3.one;
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);

        const float padX = 10f;
        const float missionTextShiftRight = 30f;
        const float textPadLeft = padX + missionTextShiftRight;
        const float textPadRight = padX;
        const float padBottom = 24f;
        const float lineGap = 6f;
        const float lineMaxHeight = 120f;
        float maxPanelWidth = SceneCatalog.IsLevel3(SceneCatalog.ActiveName) ? 400f : 520f;
        const float objectivesPadTop = 10f;

        List<TMP_Text> lines = CollectMissionObjectiveLines(panel);

        lines.Sort(CompareMissionLines);

        RectTransform objectives = EnsureMissionObjectives(panel);

        const float probeFont = HudTextStyle.MissionPanelLineFontSize;
        float widest = 0f;
        for (int i = 0; i < lines.Count; i++)
            widest = Mathf.Max(widest, HudTextStyle.MeasureMissionLineWidth(lines[i], probeFont));

        float panelWidth = Mathf.Min(widest + textPadLeft + textPadRight + 8f, maxPanelWidth);
        float textWidth = panelWidth - textPadLeft - textPadRight;

        RectTransform title = panel.Find("MissionPanelTitle") as RectTransform;
        if (title != null)
        {
            title.localScale = Vector3.one;
            title.anchorMin = new Vector2(0.5f, 1f);
            title.anchorMax = new Vector2(0.5f, 1f);
            title.pivot = new Vector2(0.5f, 1f);
            title.anchoredPosition = new Vector2(0f, 20f);
            title.sizeDelta = new Vector2(panelWidth + 12f, 96f);

            Image titleBg = title.GetComponent<Image>();
            if (titleBg != null)
            {
                titleBg.preserveAspect = false;
                titleBg.type = Image.Type.Simple;
            }

            TMP_Text titleText = title.GetComponentInChildren<TMP_Text>(true);
            if (titleText != null)
            {
                RectTransform titleTextRt = titleText.rectTransform;
                titleTextRt.localScale = Vector3.one;
                titleTextRt.anchorMin = Vector2.zero;
                titleTextRt.anchorMax = Vector2.one;
                titleTextRt.offsetMin = new Vector2(12f, 8f);
                titleTextRt.offsetMax = new Vector2(-12f, -8f);
                titleText.margin = Vector4.zero;
                titleText.enableVertexGradient = false;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.enableAutoSizing = true;
                titleText.fontSizeMin = HudTextStyle.MissionPanelFontMin;
                titleText.fontSizeMax = HudTextStyle.MissionPanelFontMax;
                titleText.fontSize = HudTextStyle.MissionPanelFontMax;
                titleText.overflowMode = TextOverflowModes.Overflow;
                titleText.color = HudTextStyle.Header;
                titleText.fontStyle = FontStyles.Bold;
                HudTextStyle.ApplyOutline(titleText);
            }
        }

        float headerReserve = MissionHeaderReserve(title);
        float contentHeight = MeasureMissionContentHeight(
            lines,
            objectivesPadTop,
            lineGap,
            textWidth,
            lineMaxHeight);

        float panelHeight = headerReserve + contentHeight + padBottom + 8f;
        panel.sizeDelta = new Vector2(panelWidth, panelHeight);

        ConfigureMissionObjectivesArea(objectives, headerReserve, padBottom);

        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].transform.SetParent(objectives, false);
            lines[i].gameObject.SetActive(true);
        }

        LayoutMissionLines(
            lines,
            objectivesPadTop,
            lineGap,
            textPadLeft,
            textWidth,
            lineMaxHeight);

        if (title != null)
            title.SetSiblingIndex(0);
        objectives.SetSiblingIndex(1);
        Canvas.ForceUpdateCanvases();

        Image bg = panel.GetComponent<Image>();
        AdventureUI.ApplyPanel(bg);
        if (bg != null)
        {
            bg.type = Image.Type.Sliced;
            bg.preserveAspect = false;
        }
    }

    static int CompareMissionLines(TMP_Text a, TMP_Text b)
    {
        return MissionLineSortKey(a).CompareTo(MissionLineSortKey(b));
    }

    static int MissionLineSortKey(TMP_Text t)
    {
        if (t == null || string.IsNullOrEmpty(t.text)) return 99;
        string s = t.text.TrimStart();
        if (s.Length > 0 && char.IsDigit(s[0])) return s[0] - '0';
        return 99;
    }

    static List<TMP_Text> CollectMissionObjectiveLines(RectTransform panel)
    {
        var lines = new List<TMP_Text>();
        for (int i = 0; i < panel.childCount; i++)
        {
            Transform child = panel.GetChild(i);
            if (child.name == "MissionPanelTitle" || child.name == "MissionObjectives") continue;
            TMP_Text tmp = child.GetComponent<TMP_Text>();
            if (tmp != null) lines.Add(tmp);
        }

        Transform objectives = panel.Find("MissionObjectives");
        if (objectives != null)
        {
            TMP_Text[] nested = objectives.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < nested.Length; i++)
            {
                if (nested[i] != null && !lines.Contains(nested[i]))
                    lines.Add(nested[i]);
            }
        }

        return lines;
    }

    static RectTransform EnsureMissionObjectives(RectTransform panel)
    {
        Transform existing = panel.Find("MissionObjectives");
        if (existing != null)
            return existing as RectTransform;

        var go = new GameObject("MissionObjectives", typeof(RectTransform));
        go.transform.SetParent(panel, false);
        return go.GetComponent<RectTransform>();
    }

    /// <summary>Pixels from panel top to the grey body (below wood Mission header).</summary>
    static float MissionHeaderReserve(RectTransform title)
    {
        const float fallback = 104f;
        if (title == null) return fallback;
        // Title pivot sits above the panel; body starts below the header sprite.
        float belowTop = title.sizeDelta.y - title.anchoredPosition.y;
        return Mathf.Max(belowTop + 16f, fallback);
    }

    static void ConfigureMissionObjectivesArea(RectTransform objectives, float headerReserve, float padBottom)
    {
        objectives.localScale = Vector3.one;
        objectives.anchorMin = Vector2.zero;
        objectives.anchorMax = Vector2.one;
        objectives.pivot = new Vector2(0.5f, 1f);
        objectives.offsetMin = new Vector2(0f, padBottom);
        objectives.offsetMax = new Vector2(0f, -headerReserve);
    }

    static float MeasureMissionContentHeight(
        List<TMP_Text> lines,
        float startY,
        float lineGap,
        float textWidth,
        float lineMaxHeight)
    {
        if (lines == null || lines.Count == 0) return startY + 4f;

        float y = startY;
        for (int i = 0; i < lines.Count; i++)
        {
            HudTextStyle.LayoutMissionLine(lines[i], textWidth, lineMaxHeight, out float rowHeight);
            rowHeight = Mathf.Max(rowHeight, HudTextStyle.MissionPanelLineFontSize + 12f);
            y += rowHeight + lineGap;
        }

        return y - lineGap + 4f;
    }

    static float LayoutMissionLines(
        List<TMP_Text> lines,
        float startY,
        float lineGap,
        float padLeft,
        float textWidth,
        float lineMaxHeight)
    {
        float y = startY;
        for (int i = 0; i < lines.Count; i++)
        {
            TMP_Text line = lines[i];
            RectTransform rt = line.rectTransform;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            HudTextStyle.LayoutMissionLine(line, textWidth, lineMaxHeight, out float rowHeight);
            rowHeight = Mathf.Max(rowHeight, HudTextStyle.MissionPanelLineFontSize + 12f);
            rt.sizeDelta = new Vector2(textWidth, rowHeight);
            rt.anchoredPosition = new Vector2(padLeft, -y);
            HudTextStyle.ApplyMissionLineStyle(line);
            line.ForceMeshUpdate(true);

            y += rowHeight + lineGap;
        }

        return y - lineGap + 4f;
    }

    static void RaiseSorting(RectTransform rt, int sortingOrder)
    {
        if (rt == null) return;

        Canvas overlay = rt.GetComponent<Canvas>();
        if (overlay == null) overlay = rt.gameObject.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = sortingOrder;

        if (rt.GetComponent<GraphicRaycaster>() == null)
            rt.gameObject.AddComponent<GraphicRaycaster>();
    }
}

public class HudSoundToggle : MonoBehaviour
{
    TMP_Text label;
    Image background;

    public void Bind(TMP_Text text, Image image)
    {
        label = text;
        background = image;
        GameAudio.OnChanged -= Refresh;
        GameAudio.OnChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        GameAudio.OnChanged -= Refresh;
    }

    void Refresh()
    {
        GameAudio.Ensure();
        bool on = !GameAudio.Muted && GameAudio.Volume > 0.001f;
        if (label != null) label.text = on ? "SOUND ON" : "SOUND OFF";
        if (background != null)
            background.color = on ? Color.white : new Color(0.62f, 0.62f, 0.62f, 1f);
    }
}
