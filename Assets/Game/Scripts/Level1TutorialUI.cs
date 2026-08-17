using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// MainGame mini-tutorial: edge hints only — does not cover the play area.
/// </summary>
public class Level1TutorialUI : MonoBehaviour
{
    const float FirstLogProgress = 0.12f;
    const float JumpHintDistance = 28f;

    GameObject leftHintRoot;
    GameObject rightHintRoot;
    GameObject jumpHintRoot;
    TMP_Text leftHint;
    TMP_Text rightHint;
    TMP_Text jumpHint;
    Transform player;
    float firstLogZ;
    bool laneDone;
    bool jumpDone;
    bool jumpShown;

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
        firstLogZ = Level1Progress.WorldZ(FirstLogProgress);
        BuildUi();
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        CachePlayer();
        if (player == null) return;

        if (!laneDone)
        {
            if (PressedLaneLeft() && leftHintRoot != null)
            {
                leftHintRoot.SetActive(false);
            }

            if (PressedLaneRight() && rightHintRoot != null)
            {
                rightHintRoot.SetActive(false);
            }

            bool leftHidden = leftHintRoot == null || !leftHintRoot.activeSelf;
            bool rightHidden = rightHintRoot == null || !rightHintRoot.activeSelf;
            if (leftHidden && rightHidden)
            {
                laneDone = true;
            }
        }

        if (!jumpDone && laneDone)
        {
            if (!jumpShown && player.position.z >= firstLogZ - JumpHintDistance)
            {
                jumpShown = true;
                if (jumpHintRoot != null) jumpHintRoot.SetActive(true);
            }

            if (jumpShown && PressedJump())
            {
                jumpDone = true;
                if (jumpHintRoot != null) jumpHintRoot.SetActive(false);
            }
        }

        if (laneDone && jumpDone)
        {
            enabled = false;
        }
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

        jumpHintRoot = CreateEdgeHint(
            canvas.transform,
            "Tutorial_Jump",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -24f),
            TextAlignmentOptions.Top,
            "▲  JUMP  Space");

        leftHint = leftHintRoot.GetComponentInChildren<TMP_Text>();
        rightHint = rightHintRoot.GetComponentInChildren<TMP_Text>();
        jumpHint = jumpHintRoot.GetComponentInChildren<TMP_Text>();
        jumpHintRoot.SetActive(false);
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

    static bool PressedJump()
    {
        Keyboard kb = Keyboard.current;
        return Input.GetKeyDown(KeyCode.Space)
            || (kb != null && kb.spaceKey.wasPressedThisFrame);
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
