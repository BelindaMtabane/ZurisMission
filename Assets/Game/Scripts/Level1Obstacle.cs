using UnityEngine;

public enum Level1ObstacleKind
{
    SandPit,
    Rock,
    Cactus,
    Log,
    DustDevil,
    Barrier,
    BlackPit
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
    bool appliedThisPass;

    public void Setup(Level1ObstacleKind kind, float damage, bool canJumpOver)
    {
        obstacleKind = kind;
        bucketDamage = damage;
        jumpable = canJumpOver;
    }

    void OnTriggerEnter(Collider other)
    {
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

        if (obstacleKind == Level1ObstacleKind.Cactus)
        {
            if (appliedThisPass) return;
            appliedThisPass = true;

            HUDControls hudControls = FindFirstObjectByType<HUDControls>();
            hudControls?.ChangeHealth(-Level1Primitives.CactusHealthDamage, "You ran into a cactus!");
            Debug.Log($"[Level1] Health -{Level1Primitives.CactusHealthDamage:0} from {obstacleKind}");
            return;
        }

        if (obstacleKind == Level1ObstacleKind.BlackPit)
        {
            if (appliedThisPass) return;
            appliedThisPass = true;

            HUDControls hud = FindFirstObjectByType<HUDControls>();
            hud?.ChangeHealth(-Level1Primitives.BlackPitHealthDamage, "You fell into a black pit!");
            PlayerController pitPc = other.GetComponent<PlayerController>();
            pitPc?.ApplySpeedModifier(Level1Primitives.BlackPitSlowSpeed, Level1Primitives.BlackPitSlowDuration);
            Debug.Log($"[Level1] Health -{Level1Primitives.BlackPitHealthDamage:0} and slowed from BlackPit");
            return;
        }

        if (applied) return;
        applied = true;

        HUDControls hudControlsBucket = FindFirstObjectByType<HUDControls>();
        hudControlsBucket?.ChangeBucket(-bucketDamage);
        Debug.Log($"[Level1] Bucket -{bucketDamage:0} from {obstacleKind}");

        if (ShouldSlowAndBreakMaterials())
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            pc?.ApplySpeedModifier(SlowSpeed, SlowDuration);
            hudControlsBucket?.BreakMaterials(MaterialBreakAmount);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (obstacleKind == Level1ObstacleKind.Cactus && other.CompareTag("Player"))
        {
            appliedThisPass = false;
        }

        if (obstacleKind == Level1ObstacleKind.BlackPit && other.CompareTag("Player"))
        {
            appliedThisPass = false;
        }
    }

    bool ShouldSlowAndBreakMaterials()
    {
        return obstacleKind == Level1ObstacleKind.SandPit
            || obstacleKind == Level1ObstacleKind.Rock
            || obstacleKind == Level1ObstacleKind.Log;
    }
}
