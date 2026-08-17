using UnityEngine;

/// <summary>
/// A snake that slithers across the player's path and deals 5 health on contact.
/// </summary>
public class SnakePassHazard : MonoBehaviour
{
    public float speed = 18f;
    public float endX = 16f;
    public int direction = 1;

    bool hit;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true;
        gameObject.tag = "AnimalAttack";
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        transform.position += Vector3.right * (direction * speed * Time.deltaTime);

        if ((direction > 0 && transform.position.x > endX)
            || (direction < 0 && transform.position.x < endX))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hit || !other.CompareTag("Player")) return;
        hit = true;

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-5f, "A snake bite dropped your health to 0.");
        Debug.Log("[Snake] Hit player, health -5");
    }
}
