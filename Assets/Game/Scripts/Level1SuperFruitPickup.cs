using UnityEngine;

public class Level1SuperFruitPickup : MonoBehaviour
{
    [SerializeField] private float boostSpeed = 42f;
    [SerializeField] private float boostSeconds = 2f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        PlayerController controller = other.GetComponent<PlayerController>();
        controller?.ApplySpeedModifier(boostSpeed, boostSeconds);

        Debug.Log("[Level1] Super fruit collected — speed boost 2s");
        gameObject.SetActive(false);
    }
}
