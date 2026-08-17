using UnityEngine;

/// <summary>
/// Marker for points the player can grapple to.
/// Empty GameObjects with this component are valid temporary targets.
/// </summary>
public interface IGrappleTarget
{
    Transform GrapplePoint { get; }
    bool CanGrapple { get; }
}

public class GrappleTarget : MonoBehaviour, IGrappleTarget
{
    [SerializeField] private bool available = true;

    public Transform GrapplePoint => transform;
    public bool CanGrapple => available && isActiveAndEnabled;

    public void SetAvailable(bool value)
    {
        available = value;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = available ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}
