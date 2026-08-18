using UnityEngine;

public enum Level3ObstacleKind
{
    Rock,
    MudPuddle,
    Tree
}

public class Level3Obstacle : MonoBehaviour
{
    [SerializeField] Level3ObstacleKind kind = Level3ObstacleKind.Rock;
    [SerializeField] bool jumpable;
    [SerializeField] float rockDamage = 6f;
    bool applied;

    public void Setup(Level3ObstacleKind obstacleKind, bool canJumpOver)
    {
        kind = obstacleKind;
        jumpable = canJumpOver;
        applied = false;
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

        if (kind == Level3ObstacleKind.MudPuddle)
        {
            if (applied) return;
            applied = true;
            FindFirstObjectByType<HUDControls>()?.LoseMaterialPercent(Level3Config.MudMaterialLossPercent);
            Level3FeedbackUI.Show("MUD — MATERIALS LOST!", new Color(0.62f, 0.42f, 0.18f), 1.2f);
            return;
        }

        if (kind == Level3ObstacleKind.Tree)
        {
            if (applied) return;
            applied = true;
            FindFirstObjectByType<HUDControls>()?.LoseBucketPercent(Level3Config.TreeBucketLossPercent);
            Level3FeedbackUI.Show("TREE HIT — BUCKET -5%!", new Color(0.2f, 0.55f, 0.25f), 1.2f);
            return;
        }

        if (applied) return;
        applied = true;
        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-rockDamage, "A rock hit you!");
        Level3FeedbackUI.Show("ROCK!", new Color(0.55f, 0.35f, 0.22f), 0.8f);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) applied = false;
    }
}
