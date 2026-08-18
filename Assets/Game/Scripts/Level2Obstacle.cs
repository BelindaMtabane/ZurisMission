using UnityEngine;

public enum Level2ObstacleKind
{
    Rock,
    MudPuddle,
    Cactus
}

public class Level2Obstacle : MonoBehaviour
{
    [SerializeField] private Level2ObstacleKind kind = Level2ObstacleKind.Rock;
    [SerializeField] private bool jumpable;
    [SerializeField] private float rockHealthDamage = 10f;
    [SerializeField] private float cactusHealthDamage = Level2Config.CactusHealthDamage;
    [SerializeField] private float mudSlowMultiplier = Level2MudSlowEffect.DefaultMultiplier;
    [SerializeField] private float mudSlowDuration = Level2MudSlowEffect.DefaultDuration;

    bool appliedThisPass;

    public void Setup(Level2ObstacleKind obstacleKind, bool canJumpOver)
    {
        kind = obstacleKind;
        jumpable = canJumpOver;
        mudSlowMultiplier = Level2MudSlowEffect.Multiplier;
        mudSlowDuration = Level2MudSlowEffect.Duration;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null && controller.IsInputLocked) return;

        if (jumpable && IsJumpingOver(controller, other))
        {
            return;
        }

        if (appliedThisPass) return;
        appliedThisPass = true;

        if (kind == Level2ObstacleKind.MudPuddle)
        {
            Level2MudSlowEffect.Apply(controller, mudSlowMultiplier, mudSlowDuration);
            return;
        }

        HUDControls hud = FindFirstObjectByType<HUDControls>();

        if (kind == Level2ObstacleKind.Cactus)
        {
            hud?.ChangeHealth(-cactusHealthDamage, "You ran into a cactus!");
            hud?.ChangePlayerWater(-Level2Config.CactusWaterDamage, "The cactus drained your water.");
            return;
        }

        hud?.ChangeHealth(-rockHealthDamage, "A rock hit you!");
    }

    static bool IsJumpingOver(PlayerController controller, Collider other)
    {
        if (controller != null && !controller.IsGrounded) return true;
        if (other != null && other.transform.position.y > Level2Ground.SurfaceY + 1.15f) return true;
        return false;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            appliedThisPass = false;
        }
    }
}
