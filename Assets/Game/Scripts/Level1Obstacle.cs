using UnityEngine;

public enum Level1ObstacleKind
{
    SandPit,
    Rock,
    Cactus,
    Log,
    DustDevil,
    Barrier
}

/// <summary>
/// Level 1 hazard: bucket damage only. Jumpable obstacles can be cleared while airborne.
/// </summary>
public class Level1Obstacle : MonoBehaviour
{
    const float SlowSpeed = 12f;
    const float SlowDuration = 2.5f;
    const int MaterialBreakAmount = 10;

    [SerializeField] private Level1ObstacleKind obstacleKind = Level1ObstacleKind.Rock;
    [SerializeField] private float bucketDamage = 2f;
    [SerializeField] private bool jumpable;

    bool applied;

    public void Setup(Level1ObstacleKind kind, float damage, bool canJumpOver)
    {
        obstacleKind = kind;
        bucketDamage = damage;
        jumpable = canJumpOver;
    }

    void OnTriggerEnter(Collider other)
    {
        if (applied) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        if (jumpable)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null && !controller.IsGrounded)
            {
                HUDControls hud = FindFirstObjectByType<HUDControls>();
                hud?.ShowActionFeedback("NICE!", new Color(0.35f, 0.95f, 0.45f));
                return;
            }
        }

        applied = true;
        HUDControls hudControls = FindFirstObjectByType<HUDControls>();
        hudControls?.ChangeBucket(-bucketDamage);
        Debug.Log($"[Level1] Bucket -{bucketDamage:0} from {obstacleKind}");

        if (ShouldSlowAndBreakMaterials())
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            pc?.ApplySpeedModifier(SlowSpeed, SlowDuration);
            hudControls?.BreakMaterials(MaterialBreakAmount);
        }
    }

    bool ShouldSlowAndBreakMaterials()
    {
        return obstacleKind == Level1ObstacleKind.SandPit
            || obstacleKind == Level1ObstacleKind.Rock
            || obstacleKind == Level1ObstacleKind.Log;
    }
}
