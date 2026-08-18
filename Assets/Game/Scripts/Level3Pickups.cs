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

public class Level3WaterDropletPickup : MonoBehaviour
{
    [SerializeField] float amount = 15f;
    bool collected;

    public void Setup(float bucketAmount)
    {
        amount = bucketAmount;
    }

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

    public void Setup(float healthAmount)
    {
        amount = healthAmount;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        collected = true;
        FindFirstObjectByType<HUDControls>()?.CollectLevel3Health(amount);
        gameObject.SetActive(false);
    }
}

public class Level3SpeedFruitPickup : MonoBehaviour
{
    [SerializeField] float boostSpeed = Level3Config.SpeedFruitBoostSpeed;
    [SerializeField] float boostSeconds = Level3Config.SpeedFruitDurationSeconds;
    [SerializeField] int particleCount = 8;

    bool collected;
    Transform[] particles;

    void Start()
    {
        // Build a lightweight "particles coming out" effect (small glowing spheres).
        if (particleCount <= 0) return;
        particles = new Transform[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.name = $"SpeedParticle_{i}";
            p.transform.SetParent(transform, false);
            p.transform.localScale = Vector3.one * 0.14f;
            p.transform.localPosition = Random.onUnitSphere * 0.18f + Vector3.up * 0.18f;
            Renderer r = p.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.35f, 0.95f, 0.22f, 0.95f);
            Collider c = p.GetComponent<Collider>();
            if (c != null) Destroy(c);
            particles[i] = p.transform;
        }
    }

    void Update()
    {
        if (particles == null) return;
        float t = Time.time;
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] == null) continue;
            float s = 1f + Mathf.Sin(t * 10f + i) * 0.15f;
            particles[i].localScale = Vector3.one * (0.14f * s);
            particles[i].localPosition = particles[i].localPosition + Vector3.up * (Mathf.Sin(t * 4f + i) * 0.0005f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        collected = true;

        PlayerController controller = other.GetComponent<PlayerController>();
        controller?.ApplySpeedModifier(boostSpeed, boostSeconds);

        Level3FeedbackUI.Show("SUPER FRUIT — SPEED BOOST!", new Color(0.2f, 0.95f, 0.25f), 1.2f);
        gameObject.SetActive(false);
    }
}
