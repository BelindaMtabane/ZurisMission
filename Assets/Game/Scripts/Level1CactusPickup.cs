using UnityEngine;

public class Level1CactusPickup : MonoBehaviour
{
    [SerializeField] private float playerWaterAmount = 20f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.CollectCactusWater(playerWaterAmount);

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }

        gameObject.SetActive(false);
    }
}
