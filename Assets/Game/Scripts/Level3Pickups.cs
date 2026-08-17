using UnityEngine;

public class Level3PickupBob : MonoBehaviour
{
    Vector3 basePos;
    float spin;

    void Start()
    {
        basePos = transform.position;
        spin = Random.Range(24f, 42f);
    }

    void Update()
    {
        transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * 3.1f) * 0.14f);
        transform.Rotate(0f, spin * Time.deltaTime, 0f);
    }
}

public class Level3MaterialPickup : MonoBehaviour
{
    [SerializeField] string kind = "Pipe";
    [SerializeField] int amount = 10;
    bool collected;

    public void Setup(string materialKind, int value)
    {
        kind = materialKind;
        amount = value;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        collected = true;
        FindFirstObjectByType<HUDControls>()?.CollectLevel3Material(kind, amount);
        gameObject.SetActive(false);
    }
}

public class Level3BucketPickup : MonoBehaviour
{
    [SerializeField] float amount = 15f;
    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        collected = true;
        FindFirstObjectByType<HUDControls>()?.CollectLevel3Bucket(amount);
        gameObject.SetActive(false);
    }
}

public class Level3HealthPickup : MonoBehaviour
{
    [SerializeField] float amount = 15f;
    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        collected = true;
        FindFirstObjectByType<HUDControls>()?.CollectLevel3Health(amount);
        gameObject.SetActive(false);
    }
}

public class Level3LeafPickup : MonoBehaviour
{
    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        collected = true;
        Level3LeafProtection.Activate(10f);
        gameObject.SetActive(false);
    }
}
