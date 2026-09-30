using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Readable gameplay HUD typography (light text + dark outline on adventure panels).
/// </summary>
public static class HudTextStyle
{
    public static readonly Color Body = new Color(1f, 0.98f, 0.94f);
    public static readonly Color Header = new Color(1f, 0.92f, 0.55f);

    const float MinBodySize = 20f;
    public const float MissionPanelFontMin = 28f;
    public const float MissionPanelFontMax = 40f;
    public const float MissionPanelLineFontSize = 28f;
    public const float VillageProgressFontSize = 25f;
    const float MinHeaderSize = 24f;
    const float MinTitleSize = 28f;
    const float OutlineWidth = 0.22f;

    public static void ApplyOutline(TMP_Text text)
    {
        if (text == null) return;
        text.outlineWidth = OutlineWidth;
        text.outlineColor = new Color(0f, 0f, 0f, 0.85f);
    }

    public static void ApplyBody(TMP_Text text, float minSize = MinBodySize, bool preserveColor = false)
    {
        if (text == null) return;
        if (!preserveColor && !HasRichTextColor(text)) text.color = Body;
        text.fontSize = Mathf.Max(text.fontSize, minSize);
        ApplyOutline(text);
    }

    public static void ApplyHeader(TMP_Text text, float minSize = MinHeaderSize)
    {
        if (text == null) return;
        if (!HasRichTextColor(text)) text.color = Header;
        text.fontSize = Mathf.Max(text.fontSize, minSize);
        if (text.fontStyle == FontStyles.Normal) text.fontStyle = FontStyles.Bold;
        ApplyOutline(text);
    }

    public static void ApplyTitle(TMP_Text text)
    {
        if (text == null) return;
        if (!HasRichTextColor(text)) text.color = Header;
        text.fontSize = Mathf.Max(text.fontSize, MinTitleSize);
        text.fontStyle = FontStyles.Bold;
        ApplyOutline(text);
    }

    public static void ApplyHierarchy(Transform root)
    {
        if (root == null) return;
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
            StyleOne(texts[i]);
    }

    public static void ApplyRunnerSceneHud()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!SceneCatalog.IsRunnerScene(scene) && scene != SceneCatalog.Level3End) return;

        ApplyHierarchy(GameObject.Find("MissionPanel")?.transform);
        ApplyHierarchy(GameObject.Find("VillageProcessplanel")?.transform);
        ApplyHierarchy(GameObject.Find("MissionPanelTitle")?.transform);

        TMP_Text[] all = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            TMP_Text t = all[i];
            if (t == null) continue;

            Canvas canvas = t.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            if (canvas.sortingOrder >= 250) continue;

            if (!IsGameplayHudText(t)) continue;
            StyleOne(t);
        }
    }

    static void StyleOne(TMP_Text t)
    {
        if (t == null) return;
        bool preserve = HasRichTextColor(t);
        if (IsTitleLike(t)) ApplyTitle(t);
        else if (IsHeaderLike(t))
        {
            if (!preserve) t.color = Header;
            t.fontSize = Mathf.Max(t.fontSize, MinHeaderSize);
            if (t.fontStyle == FontStyles.Normal) t.fontStyle = FontStyles.Bold;
            ApplyOutline(t);
        }
        else if (IsMissionListLine(t)) ApplyMissionLineStyle(t);
        else if (IsVillageProgressText(t)) ApplyVillageProgressText(t);
        else ApplyBody(t, preserveColor: preserve);
    }

    static bool IsMissionListLine(TMP_Text t)
    {
        Transform p = t != null ? t.transform.parent : null;
        if (p == null) return false;
        return p.name == "MissionPanel" || p.name == "MissionObjectives";
    }

    static bool IsVillageProgressText(TMP_Text t)
    {
        Transform x = t != null ? t.transform : null;
        while (x != null)
        {
            if (x.name == "VillageProcessplanel") return true;
            x = x.parent;
        }
        return false;
    }

    public static void ApplyVillageProgressText(TMP_Text t)
    {
        if (t == null) return;

        RectTransform rt = t.rectTransform;
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-24f, 10f);
        rt.offsetMax = new Vector2(-82f, -10f);

        t.enableVertexGradient = false;
        t.color = Body;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.MidlineGeoAligned;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Truncate;
        t.enableAutoSizing = false;
        t.fontSize = VillageProgressFontSize;
        ApplyOutline(t);
    }

    public static void ApplyMissionLineStyle(TMP_Text t)
    {
        if (t == null) return;
        t.enableVertexGradient = false;
        t.color = Body;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.overflowMode = TextOverflowModes.Overflow;
        t.enableAutoSizing = false;
        t.fontSize = MissionPanelLineFontSize;
        t.margin = Vector4.zero;
        t.rectTransform.localScale = Vector3.one;
        ApplyOutline(t);
    }

    /// <summary>Fixed objective size; wraps within boxWidth when needed.</summary>
    public static void LayoutMissionLine(TMP_Text t, float boxWidth, float maxRowHeight, out float rowHeight)
    {
        rowHeight = MissionPanelLineFontSize + 10f;
        if (t == null) return;

        ApplyMissionLineStyle(t);
        t.fontSize = MissionPanelLineFontSize;
        t.rectTransform.sizeDelta = new Vector2(boxWidth, maxRowHeight);
        t.ForceMeshUpdate(true);
        rowHeight = Mathf.Max(t.preferredHeight + 10f, MissionPanelLineFontSize + 12f);
        rowHeight = Mathf.Min(rowHeight, maxRowHeight);
        t.rectTransform.sizeDelta = new Vector2(boxWidth, rowHeight);
        t.ForceMeshUpdate(true);
    }

    public static float MeasureMissionLineWidth(TMP_Text t, float fontSize)
    {
        if (t == null) return 120f;
        ApplyMissionLineStyle(t);
        t.fontSize = fontSize;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.rectTransform.sizeDelta = new Vector2(4096f, 64f);
        t.ForceMeshUpdate();
        float width = t.preferredWidth;
        t.textWrappingMode = TextWrappingModes.Normal;
        return width;
    }

    static bool HasRichTextColor(TMP_Text t) =>
        t.richText && !string.IsNullOrEmpty(t.text) && t.text.Contains("<color");

    static bool IsTitleLike(TMP_Text t)
    {
        string n = t.gameObject.name;
        return n.Contains("Title") || n.Contains("MissionPanelTitle");
    }

    static bool IsHeaderLike(TMP_Text t)
    {
        string n = t.gameObject.name.ToUpperInvariant();
        if (n.Contains("LBL") || n.Contains("LABEL")) return false;
        if (n.Contains("STRIPTITLE") || n.Contains("TIME") && n == "LBL") return true;
        return t.fontSize >= 26f && t.fontStyle == FontStyles.Bold;
    }

    static bool IsGameplayHudText(TMP_Text t)
    {
        Transform x = t.transform;
        while (x != null)
        {
            string n = x.name;
            if (n == "PausePanel" || n == "FailPanel" || n == "VictoryPanel2" || n == "ELD_Canvas"
                || n == "LosePanel" || n == "VictoryPanel") return false;
            if (n == "MissionPanel" || n == "VillageProcessplanel" || n == "LvlStrip" || n == "TimerBadge"
                || n == "Lvl3Status" || n == "Level3StatusHUD"
                || n.StartsWith("Tutorial_") || n.Contains("Feedback") || n == "InfoFeedPanel"
                || n == "Level3TankHud" || n == "LeafProtectionHud" || n.Contains("ShieldTimer")
                || n == "HudSoundToggle" || n == "Canvas") return true;
            x = x.parent;
        }
        return false;
    }
}
