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
/// Level 1 hazard: bucket damage, optional slow/material break.
/// Only log, rock, and cactus reduce player health.
/// </summary>
public class Level1Obstacle : MonoBehaviour
{
    const float SlowSpeed = 12f;
    const float SlowDuration = 2.5f;
    const int MaterialBreakAmount = 10;
    const float HealthDamage = 10f;

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
                return;
            }
        }

        applied = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeBucket(-bucketDamage);
        Debug.Log($"[Level1] Bucket Damage: {bucketDamage:0}");

        if (DamagesHealth())
        {
            hud?.ChangeHealth(-HealthDamage, GetHealthLossReason());
            Debug.Log($"[Level1] Health -{HealthDamage:0} from {obstacleKind}");
        }

        if (ShouldSlowAndBreakMaterials())
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            pc?.ApplySpeedModifier(SlowSpeed, SlowDuration);
            hud?.BreakMaterials(MaterialBreakAmount);
            Debug.Log("[Level1] Obstacle hit — slowed, materials may break");
        }
    }

    bool DamagesHealth()
    {
        return obstacleKind == Level1ObstacleKind.Rock
            || obstacleKind == Level1ObstacleKind.Cactus
            || obstacleKind == Level1ObstacleKind.Log;
    }

    string GetHealthLossReason()
    {
        if (obstacleKind == Level1ObstacleKind.Log) return "A log crushed you.";
        if (obstacleKind == Level1ObstacleKind.Rock) return "You hit the rocks too hard.";
        if (obstacleKind == Level1ObstacleKind.Cactus) return "The cactus spikes tore into you.";
        return "Your health reached 0.";
    }

    bool ShouldSlowAndBreakMaterials()
    {
        return obstacleKind == Level1ObstacleKind.SandPit
            || obstacleKind == Level1ObstacleKind.Rock
            || obstacleKind == Level1ObstacleKind.Cactus
            || obstacleKind == Level1ObstacleKind.Log;
    }
}
