using UnityEngine;

/// <summary>
/// A horizontal log that rolls toward the player. Jump over it while airborne to avoid damage.
/// </summary>
public class Level1RollingLog : MonoBehaviour
{
    const float RollSpeed = 12f;
    const float BucketDamage = 1f;
    const float DespawnBehind = 18f;

    Transform player;
    bool hit;

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        transform.position += Vector3.back * (RollSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right, RollSpeed * 18f * Time.deltaTime, Space.Self);

        CachePlayer();
        if (player != null && transform.position.z < player.position.z - DespawnBehind)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hit) return;
        if (!other.CompareTag("Player")) return;

        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null && !controller.IsGrounded)
        {
            return;
        }

        hit = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeBucket(-BucketDamage);
        Debug.Log("[Level1] Rolling log hit — bucket damage");
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
