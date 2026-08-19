using UnityEngine;

/// <summary>
/// Refines existing pickup objects: bobbing, lane snap, collect feedback.
/// Works with the current HUD tags — does not replace PlayerHUDBase.
/// </summary>
public class PickupCollectable : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.35f;
    [SerializeField] private float bobSpeed = 2.4f;
    [SerializeField] private float spinSpeed = 70f;

    private Vector3 startPos;
    private bool collected;

    void Start()
    {
        SnapToNearestLane();
        startPos = transform.position;
        if (GetComponent<DestroyObject>() == null)
        {
            gameObject.AddComponent<DestroyObject>();
        }
    }

    void Update()
    {
        if (collected) return;
        float y = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    void SnapToNearestLane()
    {
        float[] lanes = LevelLanes.Xs;
        float x = transform.position.x;
        float best = lanes[0];
        float bestD = Mathf.Abs(x - lanes[0]);
        for (int i = 1; i < lanes.Length; i++)
        {
            float d = Mathf.Abs(x - lanes[i]);
            if (d < bestD)
            {
                bestD = d;
                best = lanes[i];
            }
        }
        Vector3 p = transform.position;
        p.x = best;
        transform.position = p;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        Debug.Log($"[Pickup] Collected {name} tag={gameObject.tag}");
    }
}
