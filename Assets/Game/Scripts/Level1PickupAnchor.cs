using UnityEngine;

/// <summary>
/// Keeps pickup visuals sitting above the scrolling ground tiles.
/// </summary>
public class Level1PickupAnchor : MonoBehaviour
{
    [SerializeField] private float surfaceLift = Level1Ground.SurfaceY;

    void LateUpdate()
    {
        Vector3 p = transform.position;
        if (p.y < surfaceLift)
        {
            p.y = surfaceLift;
            transform.position = p;
        }
    }
}
