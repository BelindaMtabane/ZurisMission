using UnityEngine;

/// <summary>
/// Rolling log spanning two or three lanes — same behaviour as Level 2.
/// </summary>
public class Level1RollingLog : MonoBehaviour
{
    const float DespawnBehind = 18f;

    [SerializeField] float speed = 13f;
    bool jumpPenaltyApplied;
    bool collisionApplied;
    Transform player;

    float StartAhead => 48f;

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
                Level1FeedbackUI.Show("ROLLING LOG — JUMP!", new Color(0.72f, 0.48f, 0.22f), 1.2f);
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
        if (phase != Phase.Roll) return;
        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerController>() == null) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        PlayerController controller = other.GetComponentInParent<PlayerController>();
        HUDControls hud = FindFirstObjectByType<HUDControls>();

        if (IsJumpingOver(controller, other))
        {
            if (!jumpPenaltyApplied)
            {
                jumpPenaltyApplied = true;
                hud?.BreakMaterials(Level1Primitives.LogJumpMaterialLoss);
            }
            return;
        }

        if (collisionApplied) return;
        collisionApplied = true;

        hud?.ChangeHealth(-Level1Primitives.LogHealthDamage, "A rolling log hit you!");
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }

    static bool IsJumpingOver(PlayerController controller, Collider other)
    {
        if (controller != null && !controller.IsGrounded) return true;
        if (other != null && other.transform.position.y > Level1Ground.SurfaceY + 1.15f) return true;
        return false;
    }
}
