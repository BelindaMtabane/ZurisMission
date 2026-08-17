using UnityEngine;

public enum Level2ObstacleKind
{
    Rock,
    MudPuddle
}

public class Level2Obstacle : MonoBehaviour
{
    [SerializeField] private Level2ObstacleKind kind = Level2ObstacleKind.Rock;
    [SerializeField] private bool jumpable;
    [SerializeField] private float rockHealthDamage = 10f;
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

        if (jumpable && controller != null && !controller.IsGrounded)
        {
            if (kind == Level2ObstacleKind.MudPuddle)
            {
                Level2FeedbackUI.Show("CLEARED MUD!", new Color(0.35f, 0.95f, 0.45f), 0.9f);
            }
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
        hud?.ChangeHealth(-rockHealthDamage, "A rock hit you!");
        Level2FeedbackUI.Show("ROCK!", new Color(0.55f, 0.35f, 0.22f), 0.9f);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            appliedThisPass = false;
        }
    }
}
