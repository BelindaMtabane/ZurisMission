using UnityEngine;

public enum Level1StatPickupType
{
    Health,
    Material
}

public class Level1StatPickup : MonoBehaviour
{
    [SerializeField] private Level1StatPickupType pickupType = Level1StatPickupType.Health;
    [SerializeField] private float amount = 10f;

    bool collected;

    public void Setup(Level1StatPickupType type, float value)
    {
        pickupType = type;
        amount = value;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud != null)
        {
            if (pickupType == Level1StatPickupType.Health)
            {
                hud.CollectHealthPickup(amount);
            }
            else
            {
                hud.CollectMaterialPickup(Mathf.RoundToInt(amount));
            }
        }

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }

        gameObject.SetActive(false);
    }
}
