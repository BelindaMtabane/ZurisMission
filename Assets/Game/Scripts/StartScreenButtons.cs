using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Auto-creates a styled Start / Exit button panel in the StartScreen scene.
/// Uses SceneManager.sceneLoaded so it fires on every scene change, not just
/// the first scene of the session.
/// </summary>
public class StartScreenButtons : MonoBehaviour
{
    // Static flag — resets every Play session (domain reload resets statics).
    static bool _startScreenShown = false;

    // ── Session bootstrap — redirect + register for every scene load ───────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        // Register callback now so it fires on every future scene change.
        SceneManager.sceneLoaded += (scene, _) => TryCreate(scene.name);

        string current = SceneManager.GetActiveScene().name;

        // If Play was pressed while a gameplay scene was open, redirect to
        // StartScreen first so the opening screen is always shown.
        if (!_startScreenShown && current != "StartScreen")
        {
            _startScreenShown = true;   // prevent redirect loop
            SceneManager.LoadScene("StartScreen");
            return;                     // sceneLoaded callback handles the rest
        }

        TryCreate(current);
    }

    static void TryCreate(string sceneName)
    {
        if (sceneName != "StartScreen") return;
        _startScreenShown = true;
        if (FindFirstObjectByType<StartScreenButtons>() != null) return;
        new GameObject("StartScreenButtons").AddComponent<StartScreenButtons>();
    }

    // ── Colour helpers ────────────────────────────────────────────────────
    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    static readonly Color ColOverlay  = C(0.04f, 0.08f, 0.04f, 0.55f);
    static readonly Color ColTitle    = C(1.00f, 0.82f, 0.20f);
    static readonly Color ColSubtitle = C(0.55f, 0.90f, 0.55f);
    static readonly Color ColBtnStart = C(0.10f, 0.55f, 0.10f, 0.90f);
    static readonly Color ColBtnExit  = C(0.55f, 0.10f, 0.10f, 0.85f);
    static readonly Color ColBtnText  = C(1.00f, 1.00f, 1.00f);

    // ══════════════════════════════════════════════════════════════════════
    void Awake() => BuildUI();

    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        // ── Canvas ─────────────────────────────────────────────────────────
        var cvGO = new GameObject("SSB_Canvas");
        cvGO.transform.SetParent(transform);
        var cv = cvGO.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 200;
        var cs = cvGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGO.AddComponent<GraphicRaycaster>();

        // ── Central card  (35 %→65 % wide, 15 %→75 % tall) ────────────────
        var card   = MakeRect(cvGO, "SSB_Card");
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.35f, 0.15f);
        cardRT.anchorMax = new Vector2(0.65f, 0.75f);
        cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;
        card.AddComponent<Image>().color = ColOverlay;

        // ── Title ──────────────────────────────────────────────────────────
        MakeTxt(card, "SSB_Title",
                "ZURI'S MISSION",
                36, FontStyles.Bold, ColTitle,
                new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.96f));

        // ── Subtitle ───────────────────────────────────────────────────────
        MakeTxt(card, "SSB_Sub",
                "Help Zuri bring water to her village",
                14, FontStyles.Italic, ColSubtitle,
                new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.74f));

        // ── START GAME ─────────────────────────────────────────────────────
        var btnStart = MakeButton(card, "SSB_StartBtn",
                                  "START GAME", ColBtnStart,
                                  new Vector2(0.10f, 0.36f), new Vector2(0.90f, 0.56f));
        btnStart.onClick.AddListener(() => SceneManager.LoadScene("MainGame"));

        // ── EXIT ───────────────────────────────────────────────────────────
        var btnExit = MakeButton(card, "SSB_ExitBtn",
                                 "EXIT", ColBtnExit,
                                 new Vector2(0.25f, 0.10f), new Vector2(0.75f, 0.28f));
        btnExit.onClick.AddListener(() => Application.Quit());
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static GameObject MakeRect(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static TMP_Text MakeTxt(GameObject parent, string name, string text,
                             float size, FontStyles style, Color col,
                             Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text             = text;
        t.fontSize         = size;
        t.fontStyle        = style;
        t.color            = col;
        t.alignment        = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.Normal;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }

    static Button MakeButton(GameObject parent, string name, string label,
                              Color bgCol, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = bgCol;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = bgCol;
        colors.highlightedColor = Color.Lerp(bgCol, Color.white, 0.25f);
        colors.pressedColor     = Color.Lerp(bgCol, Color.black, 0.25f);
        colors.selectedColor    = bgCol;
        btn.colors              = colors;
        btn.targetGraphic       = img;

        MakeTxt(go, name + "_Lbl", label, 20, FontStyles.Bold, ColBtnText,
                new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f));
        return btn;
    }
}
