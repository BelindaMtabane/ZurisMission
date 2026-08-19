using UnityEngine;

public enum Level1MaterialKind
{
    Hammer,
    Brick,
    CementBag
}

public class Level1MaterialPickup : MonoBehaviour
{
    [SerializeField] private Level1MaterialKind kind = Level1MaterialKind.Hammer;
    [SerializeField] private int amount = 10;

    bool collected;

    public void Setup(Level1MaterialKind materialKind, int value)
    {
        kind = materialKind;
        amount = value;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.CollectMaterialPickup(amount);

        Debug.Log($"[Level1] {kind} collected +{amount}");
        gameObject.SetActive(false);
    }
}
