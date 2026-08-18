using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hides old lane "Button" UI so gameplay toasts remain readable.
/// </summary>
public static class LegacyLaneUi
{
    static readonly string[] LaneButtonNames =
    {
        "Button1", "Button2", "Button3", "Button4",
        "Button (2)", "Button (4)"
    };

    public static void Hide()
    {
        for (int i = 0; i < LaneButtonNames.Length; i++)
        {
            GameObject go = GameObject.Find(LaneButtonNames[i]);
            if (go != null) go.SetActive(false);
        }

        TMP_Text[] labels = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (labels == null) return;

        for (int i = 0; i < labels.Length; i++)
        {
            TMP_Text label = labels[i];
            if (label == null || string.IsNullOrEmpty(label.text)) continue;
            if (label.text.Trim() != "Button") continue;

            Transform node = label.transform;
            while (node != null && node.GetComponent<Button>() == null)
            {
                node = node.parent;
            }

            if (node != null)
            {
                node.gameObject.SetActive(false);
            }
        }
    }
}
