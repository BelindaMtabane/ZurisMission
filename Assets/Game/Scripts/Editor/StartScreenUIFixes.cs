using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-off fix for the StartScreen "CreditNAME" panel, which used a
/// non-uniform RectTransform.localScale to reach its size. That scale
/// multiplies into every child (including the credits text), stretching
/// the text glyphs horizontally. Converts the scale into sizeDelta instead,
/// which resizes the rect without distorting children.
/// </summary>
public static class StartScreenUIFixes
{
    [MenuItem("Tools/Log SSB Button Rects")]
    public static void LogButtonRects()
    {
        foreach (string n in new[] { "SSB_StartBtn", "SSB_ExitBtn", "SSB_Card", "SSB_Sub" })
        {
            GameObject go = GameObject.Find(n);
            if (go == null) { Debug.LogWarning($"[StartScreenUIFixes] {n} not found"); continue; }
            var rt = go.GetComponent<RectTransform>();
            var img = go.GetComponent<UnityEngine.UI.Image>();
            string spriteInfo = img != null && img.sprite != null
                ? $"{img.sprite.name} rect={img.sprite.rect} texRect={img.sprite.textureRect}"
                : "none";
            Debug.Log($"[StartScreenUIFixes] {n}: active={go.activeInHierarchy} rect={rt.rect} anchoredPos={rt.anchoredPosition} offsetMin={rt.offsetMin} offsetMax={rt.offsetMax} sprite={spriteInfo}");
        }
    }

    [MenuItem("Tools/Click SSB_StartBtn")]
    public static void ClickStart() => ClickByName("SSB_StartBtn");

    [MenuItem("Tools/Click SSB_SettingsBtn")]
    public static void ClickSettings() => ClickByName("SSB_SettingsBtn");

    [MenuItem("Tools/Click SP_CreditsBtn")]
    public static void ClickCredits() => ClickByName("SP_CreditsBtn");

    [MenuItem("Tools/Click SP_SoundBtn")]
    public static void ClickSound() => ClickByName("SP_SoundBtn");

    [MenuItem("Tools/Click SP_CloseBtn")]
    public static void ClickClose() => ClickByName("SP_CloseBtn");

    [MenuItem("Tools/Click CP_BackBtn")]
    public static void ClickBack() => ClickByName("CP_BackBtn");

    public static void ClickByName(string n)
    {
        GameObject go = GameObject.Find(n);
        if (go == null) { Debug.LogError($"[StartScreenUIFixes] {n} not found"); return; }
        var btn = go.GetComponent<UnityEngine.UI.Button>();
        if (btn == null) { Debug.LogError($"[StartScreenUIFixes] {n} has no Button"); return; }
        btn.onClick.Invoke();
        Debug.Log($"[StartScreenUIFixes] Clicked {n}");
    }

    [MenuItem("Tools/Test Lose Screen")]
    public static void TestLoseScreen()
    {
        if (RunStateManager.Instance == null) { Debug.LogError("[Test] RunStateManager.Instance is null — enter Play mode first."); return; }
        RunStateManager.Instance.NotifyDeath("You ran out of water.");
    }

    [MenuItem("Tools/Test Victory Screen")]
    public static void TestVictoryScreen()
    {
        if (RunStateManager.Instance == null) { Debug.LogError("[Test] RunStateManager.Instance is null — enter Play mode first."); return; }
        RunStateManager.Instance.NotifyVictory("LEVEL 1 COMPLETE",
            "You collected enough materials, kept your health and water up, and filled the bucket.\n\nStart Level 2 when you are ready.");
    }

    [MenuItem("Tools/Test End Level Dialogue")]
    public static void TestEndLevelDialogue()
    {
        if (EndLevelDialogue.Instance == null) { Debug.LogError("[Test] EndLevelDialogue.Instance is null — enter Play mode first."); return; }
        EndLevelDialogue.Instance.ShowForLevel(1);
    }

    [MenuItem("Tools/Test Pause Screen")]
    public static void TestPauseScreen()
    {
        if (RunStateManager.Instance == null) { Debug.LogError("[Test] RunStateManager.Instance is null — enter Play mode first."); return; }
        RunStateManager.Instance.Pause();
    }

    [MenuItem("Tools/Diagnose Lose Panel")]
    public static void DiagnoseLosePanel()
    {
        GameObject go = GameObject.Find("LosePanel");
        if (go == null) { Debug.LogError("[Diagnose] LosePanel not found."); return; }

        Canvas canvas = go.GetComponentInParent<Canvas>();
        var canvasRt = canvas.GetComponent<RectTransform>();
        Debug.Log($"[Diagnose] Canvas '{canvas.name}' renderMode={canvas.renderMode} sortingOrder={canvas.sortingOrder} worldCamera={canvas.worldCamera} enabled={canvas.enabled} rootActive={canvas.gameObject.activeInHierarchy} rect={canvasRt.rect} scale={canvasRt.lossyScale}");

        var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in allCanvases)
            Debug.Log($"[Diagnose] AllCanvas: {GetPath(c.transform)} renderMode={c.renderMode} sortingOrder={c.sortingOrder} enabled={c.enabled} active={c.gameObject.activeInHierarchy}");

        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponent<UnityEngine.UI.Image>();
        Debug.Log($"[Diagnose] LosePanel rect={rt.rect} anchoredPos={rt.anchoredPosition} anchorMin={rt.anchorMin} anchorMax={rt.anchorMax} scale={rt.lossyScale} imgColor={img?.color} imgEnabled={img?.enabled} active={go.activeInHierarchy}");

        var inner = go.transform.Find("Inner");
        if (inner != null)
        {
            var innerRt = inner.GetComponent<RectTransform>();
            var innerImg = inner.GetComponent<UnityEngine.UI.Image>();
            Debug.Log($"[Diagnose] Inner rect={innerRt.rect} anchoredPos={innerRt.anchoredPosition} sprite={innerImg?.sprite} imgColor={innerImg?.color} active={inner.gameObject.activeInHierarchy}");
            foreach (Transform child in inner)
                Debug.Log($"[Diagnose] Inner child: {child.name} active={child.gameObject.activeInHierarchy}");
        }
    }

    [MenuItem("Tools/Diagnose Broken UI Graphics In Scene")]
    public static void DiagnoseBrokenUiGraphics()
    {
        int count = 0;
        var images = Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None);
        foreach (var img in images)
        {
            if (img.sprite == null)
            {
                Debug.LogWarning($"[DiagnoseUI] Image {GetPath(img.transform)} has NULL sprite. color={img.color} type={img.type} enabled={img.enabled} activeInHierarchy={img.gameObject.activeInHierarchy}");
                count++;
            }
        }
        var raws = Object.FindObjectsByType<UnityEngine.UI.RawImage>(FindObjectsSortMode.None);
        foreach (var raw in raws)
        {
            if (raw.texture == null)
            {
                Debug.LogWarning($"[DiagnoseUI] RawImage {GetPath(raw.transform)} has NULL texture. color={raw.color}");
                count++;
            }
        }
        Debug.Log($"[DiagnoseUI] Scanned {images.Length} Images + {raws.Length} RawImages, found {count} with missing sprite/texture.");
    }

    [MenuItem("Tools/Diagnose Broken Shaders In Scene")]
    public static void DiagnoseBrokenShaders()
    {
        int count = 0;
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null)
                {
                    Debug.LogWarning($"[Diagnose] {GetPath(r.transform)} has a NULL material slot.");
                    count++;
                    continue;
                }
                if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                {
                    Debug.LogWarning($"[Diagnose] {GetPath(r.transform)} material '{mat.name}' shader='{(mat.shader == null ? "NULL" : mat.shader.name)}'");
                    count++;
                }
            }
        }
        Debug.Log($"[Diagnose] Scanned {renderers.Length} renderers, found {count} broken material slots.");
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    [MenuItem("Tools/Fix StartScreen Credit Panel Stretch")]
    public static void FixCreditPanelStretch()
    {
        GameObject go = GameObject.Find("CreditNAME");
        if (go == null)
        {
            Debug.LogError("[StartScreenUIFixes] CreditNAME not found — is StartScreen open?");
            return;
        }

        var rt = go.GetComponent<RectTransform>();
        Vector2 finalSize = new Vector2(rt.sizeDelta.x * rt.localScale.x, rt.sizeDelta.y * rt.localScale.y);
        rt.localScale = Vector3.one;
        rt.sizeDelta = finalSize;

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);

        Debug.Log($"[StartScreenUIFixes] CreditNAME fixed: sizeDelta={rt.sizeDelta}, scale={rt.localScale}");
    }
}
