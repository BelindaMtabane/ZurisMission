using UnityEngine;

public class PitObstacle : MonoBehaviour
{
    [SerializeField] private float pitDamage = 5f;

    private bool damageDone = false;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    // Fires when the collider is successfully a trigger
    void OnTriggerEnter(Collider other)
    {
        if (damageDone || !other.CompareTag("Player")) return;
        ApplyDamage();
    }

    // Fallback: fires if the collider wasn't a trigger yet
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        // Force the collider to trigger so the player passes through immediately
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        Physics.IgnoreCollision(col, collision.collider);
        ApplyDamage();
    }

    void ApplyDamage()
    {
        damageDone = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud != null)
            hud.ChangeHealth(-pitDamage, "You fell into a pit!");
    }
}
