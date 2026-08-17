using UnityEngine;

public enum Level2MaterialKind
{
    Pipe,
    Nails,
    Hammer
}

public class Level2PickupBob : MonoBehaviour
{
    Vector3 basePos;
    float spin;

    void Start()
    {
        basePos = transform.position;
        spin = Random.Range(28f, 48f);
    }

    void Update()
    {
        transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * 3.2f) * 0.16f);
        transform.Rotate(0f, spin * Time.deltaTime, 0f);
    }
}

public class Level2WaterDropletPickup : MonoBehaviour
{
    [SerializeField] private float playerWaterAmount = 15f;
    [SerializeField] private float bucketAmount = 15f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.CollectLevel2WaterDroplet(playerWaterAmount, bucketAmount);
        gameObject.SetActive(false);
    }
}

public class Level2BaobabPickup : MonoBehaviour
{
    [SerializeField] private float playerWaterAmount = 20f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.CollectBaobabWater(playerWaterAmount);
        gameObject.SetActive(false);
    }
}

public class Level2MaterialPickup : MonoBehaviour
{
    [SerializeField] private Level2MaterialKind kind = Level2MaterialKind.Hammer;
    [SerializeField] private int amount = 10;

    bool collected;

    public void Setup(Level2MaterialKind materialKind, int value)
    {
        kind = materialKind;
        amount = value;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.CollectLevel2Material(kind, amount);
        gameObject.SetActive(false);
    }
}

public class Level2BubbleShieldPickup : MonoBehaviour
{
    [SerializeField] private float shieldDuration = 9f;
    [SerializeField] private bool consumeOnBlock = true;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        Level2BubbleShield.Activate(shieldDuration, consumeOnBlock);
        gameObject.SetActive(false);
    }
}

public class Level2SpeedFruitPickup : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 1.32f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float bucketCost = 5f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        PlayerController controller = other.GetComponent<PlayerController>();
        controller?.ApplySpeedFruit(speedMultiplier, duration);

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.CollectLevel2SpeedFruit(bucketCost);
        gameObject.SetActive(false);
    }
}

public class Level2JumpBoostPickup : MonoBehaviour
{
    [SerializeField] private float jumpMultiplier = 1.32f;
    [SerializeField] private float duration = 6f;

    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;
        PlayerController controller = other.GetComponent<PlayerController>();
        controller?.ApplyJumpBoost(jumpMultiplier, duration);
        Level2FeedbackUI.Show("JUMP BOOST!", new Color(0.62f, 0.45f, 1f), 1.4f);
        gameObject.SetActive(false);
    }
}
