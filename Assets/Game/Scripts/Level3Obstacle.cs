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
            return;
        }

        if (kind == Level3ObstacleKind.MudPuddle)
        {
            if (applied) return;
            applied = true;
            FindFirstObjectByType<HUDControls>()?.BreakMaterials(Level3Config.MudMaterialLoss);
            controller?.ApplySpeedModifier(controller.CurrentSpeed * 0.45f, 2.2f);
            return;
        }

        // Tree: health damage + flat 10 material loss
        if (kind == Level3ObstacleKind.Tree)
        {
            if (applied) return;
            applied = true;
            HUDControls hud = FindFirstObjectByType<HUDControls>();
            // Trees no longer damage health on contact; they only break materials.
            hud?.BreakMaterials(Level3Config.TreeMaterialLoss);
            return;
        }

        // Rock: slow only, no health damage
        if (applied) return;
        applied = true;
        controller?.ApplySpeedModifier(controller.CurrentSpeed * 0.5f, 1.8f);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) applied = false;
    }
}
