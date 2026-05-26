using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Right-side notification feed stacked below the bucket bar.
/// Self-bootstraps its own panel; call SetFeedPosition() to override placement.
///
/// GameInfoUI.Post("Water collected!",  GameInfoUI.MsgType.Pickup);
/// GameInfoUI.Post("Obstacle hit!",     GameInfoUI.MsgType.Obstacle);
/// GameInfoUI.Post("Level Complete!",   GameInfoUI.MsgType.Win);
/// GameInfoUI.Post("Game Over!",        GameInfoUI.MsgType.Loss);
/// GameInfoUI.Post("Speed boost!",      GameInfoUI.MsgType.Interaction);
/// </summary>
public class GameInfoUI : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static GameInfoUI Instance { get; private set; }

    // ── Inspector (optional — auto-created if not assigned) ───────────────
    [SerializeField] private TMP_Text feedText;
    [SerializeField] private int      maxMessages     = 5;
    [SerializeField] private float    messageDuration = 4f;

    // ── Message categories ─────────────────────────────────────────────────
    public enum MsgType { Pickup, Obstacle, Win, Loss, Interaction }

    // ── Internal ───────────────────────────────────────────────────────────
    private struct Entry
    {
        public string colorHex;
        public string text;
        public float  expireAt;   // unscaledTime — keeps ticking while paused
    }

    private readonly Queue<Entry> _queue   = new Queue<Entry>();
    private          RectTransform _panelRT;
    private          bool          _dirty;

    // ── Lifecycle ──────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (feedText == null)
            Bootstrap();
    }

    private void Update()
    {
        while (_queue.Count > 0 && _queue.Peek().expireAt <= Time.unscaledTime)
        {
            _queue.Dequeue();
            _dirty = true;
        }
        if (_dirty) { Rebuild(); _dirty = false; }
    }

    // ── Public API ─────────────────────────────────────────────────────────
    public static void Post(string message, MsgType type = MsgType.Interaction)
    {
        if (Instance != null) Instance.Add(message, type);
    }


    // ── Internal add ──────────────────────────────────────────────────────
    private void Add(string message, MsgType type)
    {
        string hex, prefix;
        switch (type)
        {
            case MsgType.Pickup:   hex = "#4DFF91"; prefix = "[ + ]"; break;
            case MsgType.Win:      hex = "#FFD700"; prefix = "[WIN]";  break;
            case MsgType.Loss:     hex = "#FF4444"; prefix = "[!!]";   break;
            case MsgType.Obstacle: hex = "#FF8C00"; prefix = "[ ! ]";  break;
            default:               hex = "#E0E0E0"; prefix = "[ ~ ]";  break;
        }

        _queue.Enqueue(new Entry
        {
            colorHex = hex,
            text     = $"{prefix} {message}",
            expireAt = Time.unscaledTime + messageDuration
        });

        while (_queue.Count > maxMessages)
            _queue.Dequeue();

        Rebuild();
        _dirty = false;
    }

    private void Rebuild()
    {
        if (feedText == null) return;
        StringBuilder sb = new StringBuilder();
        foreach (Entry e in _queue)
            sb.AppendLine($"<color={e.colorHex}>{e.text}</color>");
        feedText.text = sb.ToString().TrimEnd();
    }

    // ── Auto-build panel ──────────────────────────────────────────────────
    private void Bootstrap()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cGO = new GameObject("GameInfoCanvas");
            canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 99;
            cGO.AddComponent<CanvasScaler>();
            cGO.AddComponent<GraphicRaycaster>();
        }

        // Panel — BOTTOM-RIGHT of screen
        GameObject panel = new GameObject("InfoFeedPanel");
        panel.transform.SetParent(canvas.transform, false);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.06f, 0.22f, 0.50f);   // blue-black 50% opacity

        _panelRT                  = panel.GetComponent<RectTransform>();
        _panelRT.anchorMin        = new Vector2(1f, 0f);
        _panelRT.anchorMax        = new Vector2(1f, 0f);
        _panelRT.pivot            = new Vector2(1f, 0f);
        _panelRT.anchoredPosition = new Vector2(-20f, 20f);   // 20px from bottom-right edge
        _panelRT.sizeDelta        = new Vector2(200f, 150f);

        // Text inside panel
        GameObject textGO = new GameObject("InfoFeedText");
        textGO.transform.SetParent(panel.transform, false);

        TextMeshProUGUI tmp    = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize           = 13f;
        tmp.alignment          = TextAlignmentOptions.TopLeft;
        tmp.overflowMode       = TextOverflowModes.Truncate;
        tmp.enableWordWrapping = true;

        RectTransform tRT = textGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(8f,  6f);
        tRT.offsetMax = new Vector2(-8f, -6f);

        feedText = tmp;
    }
}
