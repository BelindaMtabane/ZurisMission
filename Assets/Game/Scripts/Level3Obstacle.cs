using UnityEngine;

public enum Level3ObstacleKind
{
    Rock,
    MudPuddle
}

public class Level3Obstacle : MonoBehaviour
{
    [SerializeField] Level3ObstacleKind kind = Level3ObstacleKind.Rock;
    [SerializeField] bool jumpable;
    [SerializeField] float rockDamage = 8f;
    bool applied;

    public void Setup(Level3ObstacleKind obstacleKind, bool canJumpOver)
    {
        kind = obstacleKind;
        jumpable = canJumpOver;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        PlayerController controller = other.GetComponent<PlayerController>();
        if (jumpable && controller != null && !controller.IsGrounded)
        {
            Level3FeedbackUI.Show(kind == Level3ObstacleKind.MudPuddle ? "CLEARED MUD!" : "JUMPED!", new Color(0.4f, 0.95f, 0.45f), 0.8f);
            return;
        }

        if (applied) return;
        applied = true;

        if (kind == Level3ObstacleKind.MudPuddle)
        {
            Level3MudSlowEffect.Apply(controller);
            return;
        }

        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-rockDamage, "A rock hit you!");
        Level3FeedbackUI.Show("ROCK!", new Color(0.55f, 0.35f, 0.22f), 0.8f);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) applied = false;
    }
}
