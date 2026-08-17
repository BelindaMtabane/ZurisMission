using UnityEngine;

public class Level1WaterPoolPickup : MonoBehaviour
{
    [SerializeField] private float bucketAmount = 20f;
    [SerializeField] private float playerWaterAmount = 10f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.CollectWaterPool(bucketAmount, playerWaterAmount);

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }

        gameObject.SetActive(false);
    }
}
