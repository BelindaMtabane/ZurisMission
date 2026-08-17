using UnityEngine;

public class Level1AloePickup : MonoBehaviour
{
    [SerializeField] private float pauseSeconds = 10f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        Level1HeatWave heat = FindFirstObjectByType<Level1HeatWave>();
        heat?.PauseHeatWave(pauseSeconds);

        Debug.Log("[Level1] Aloe collected — heat wave paused 10s");
        gameObject.SetActive(false);
    }
}
