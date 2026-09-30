using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One spring, two uses. Drink refills body water. Fill the bucket raises village progress.
/// Either choice lets the player continue.
/// </summary>
public class Level1WaterChoice : MonoBehaviour
{
    const float DrinkAmount = 28f;
    const float BucketAmount = 22f;

    bool used;
    GameObject prompt;

    void OnTriggerEnter(Collider other)
    {
        if (used || !IsPlayer(other)) return;
        ShowPrompt();
    }

    void OnTriggerExit(Collider other)
    {
        if (used || !IsPlayer(other)) return;
        HidePrompt();
    }

    void ChooseDrink()
    {
        if (used) return;
        used = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangePlayerWater(DrinkAmount, "drank from the spring");
        Level1FeedbackUI.Show("Drank. Body water up. Village unchanged.", new Color(0.35f, 0.85f, 1f), 1.8f);
        HidePrompt();
    }

    void ChooseBucket()
    {
        if (used) return;
        used = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud != null)
        {
            PlayerResources resources = PlayerResources.Instance;
            resources?.AddBucketWater(BucketAmount);
            hud.VillageProgress?.RecalculateFromResources(resources);
            Level1FeedbackUI.Show("Bucket filled. Village progress up. Body water unchanged.", new Color(0.45f, 0.75f, 1f), 1.8f);
        }

        HidePrompt();
    }

    void ShowPrompt()
    {
        if (prompt != null)
        {
            prompt.SetActive(true);
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        prompt = new GameObject("WaterChoicePrompt");
        prompt.transform.SetParent(canvas.transform, false);
        RectTransform root = prompt.AddComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.18f);
        root.anchorMax = new Vector2(0.5f, 0.18f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(520f, 150f);

        TMP_Text label = CreateText(prompt.transform, "Choose", "Drink, or fill the bucket?", 26f, new Vector2(0f, 42f), new Vector2(500f, 40f));
        label.alignment = TextAlignmentOptions.Center;

        Button drink = CreateButton(prompt.transform, "Drink", "DRINK", new Vector2(-120f, -28f));
        drink.onClick.AddListener(ChooseDrink);
        Button fill = CreateButton(prompt.transform, "Fill", "FILL BUCKET", new Vector2(120f, -28f));
        fill.onClick.AddListener(ChooseBucket);
    }

    void HidePrompt()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    static TMP_Text CreateText(Transform parent, string name, string message, float size, Vector2 position, Vector2 box)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.95f, 0.9f, 0.75f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        RectTransform rt = text.rectTransform;
        rt.anchoredPosition = position;
        rt.sizeDelta = box;
        return text;
    }

    static Button CreateButton(Transform parent, string name, string message, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = new Color(0.18f, 0.42f, 0.22f, 0.95f);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(200f, 56f);
        CreateText(go.transform, "Label", message, 20f, Vector2.zero, new Vector2(190f, 48f));
        return go.GetComponent<Button>();
    }

    static bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        return other.GetComponentInParent<PlayerController>() != null;
    }
}
