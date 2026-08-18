using UnityEngine;

/// <summary>
/// Two-lane rolling log. Jump over it or move to a clear lane.
/// </summary>
public class Level3RollingLog : MonoBehaviour
{
    const float DespawnBehind = 18f;

    [SerializeField] float speed = 14f;
    bool hit;
    Transform player;

    float StartAhead => Mathf.Max(90f, Level3Config.VisibleSpawnDistance + 10f);

    enum Phase { Wait, Roll }
    Phase phase = Phase.Wait;

    public void Setup(float rollSpeed)
    {
        speed = rollSpeed;
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        if (phase == Phase.Wait)
        {
            if (player.position.z > transform.position.z - StartAhead)
            {
                phase = Phase.Roll;
                Level3FeedbackUI.Show("ROLLING LOG — JUMP OR SWITCH LANES!", new Color(0.72f, 0.48f, 0.22f), 1.3f);
            }
            return;
        }

        transform.position += Vector3.back * (speed * Time.deltaTime);
        transform.Rotate(Vector3.right, speed * 22f * Time.deltaTime, Space.Self);

        if (transform.position.z < player.position.z - DespawnBehind)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other) => TryHandleHit(other);
    void OnTriggerStay(Collider other) => TryHandleHit(other);

    void TryHandleHit(Collider other)
    {
        if (hit || phase != Phase.Roll) return;
        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerController>() == null) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        PlayerController controller = other.GetComponentInParent<PlayerController>();
        if (controller != null && !controller.IsGrounded)
        {
            hit = true;
            return;
        }

        hit = true;
        // Log: materials loss only (no health damage, no slowdown in Level 3)
        FindFirstObjectByType<HUDControls>()?.BreakMaterials(Level3Config.LogMaterialLoss);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
