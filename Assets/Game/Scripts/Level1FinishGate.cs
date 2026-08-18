using UnityEngine;

/// <summary>
/// End-of-Level-1 trigger. Completes the run when health, player water,
/// bucket, and materials all meet the win checks.
/// </summary>
public class Level1FinishGate : MonoBehaviour
{
    bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!IsPlayer(other)) return;

        used = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud == null) return;

        hud.LevelProgress();
    }

    static bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        return other.GetComponentInParent<PlayerController>() != null;
    }
}
