using UnityEngine;

public enum BushlandHazardType
{
    DustDevil,
    CactusWall,
    HeatWave,
    SandPit,
    Rock,
    Pipe,
    Glass
}

/// <summary>
/// Level 1 hazard: telegraph, then a live collider with consequence + feedback.
/// Uses existing HUD tags so PlayerHUDBase still applies the numbers.
/// </summary>
public class BushlandHazard : MonoBehaviour
{
    [SerializeField] private BushlandHazardType hazardType = BushlandHazardType.CactusWall;
    [SerializeField] private float telegraphSeconds = 0.8f;
    [SerializeField] private GameObject telegraph;
    [SerializeField] private Collider liveCollider;

    private float timer;
    private bool live;
    private bool applied;

    public BushlandHazardType HazardType => hazardType;

    public void Setup(BushlandHazardType type, GameObject telegraphObject, Collider damageCollider)
    {
        hazardType = type;
        telegraph = telegraphObject;
        liveCollider = damageCollider;
        if (liveCollider != null)
        {
            liveCollider.enabled = false;
            liveCollider.isTrigger = true;
        }
        if (telegraph != null)
        {
            telegraph.SetActive(true);
        }
        timer = 0f;
        live = false;
        applied = false;
        ApplyTag();
    }

    void Start()
    {
        if (liveCollider == null)
        {
            liveCollider = GetComponent<Collider>();
        }
        ApplyTag();
        if (liveCollider != null)
        {
            liveCollider.isTrigger = true;
            liveCollider.enabled = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (!live)
        {
            PulseTelegraph();
            if (timer >= telegraphSeconds)
            {
                GoLive();
            }
        }
    }

    void PulseTelegraph()
    {
        if (telegraph == null) return;
        float s = 1f + Mathf.Sin(Time.time * 10f) * 0.15f;
        telegraph.transform.localScale = Vector3.one * s;
    }

    void GoLive()
    {
        live = true;
        if (telegraph != null)
        {
            telegraph.SetActive(false);
        }
        if (liveCollider != null)
        {
            liveCollider.enabled = true;
        }
        Debug.Log($"[Hazard] {hazardType} live on {name}");
    }

    void ApplyTag()
    {
        switch (hazardType)
        {
            case BushlandHazardType.DustDevil:
                gameObject.tag = "SlowDown";
                break;
            case BushlandHazardType.CactusWall:
                gameObject.tag = "Obstacle";
                break;
            case BushlandHazardType.HeatWave:
                gameObject.tag = "Heat&Disease";
                break;
            case BushlandHazardType.SandPit:
                gameObject.tag = "SlowDown";
                break;
            case BushlandHazardType.Rock:
                gameObject.tag = "Obstacle";
                break;
            case BushlandHazardType.Pipe:
                gameObject.tag = "Obstacle";
                break;
            case BushlandHazardType.Glass:
                gameObject.tag = "SlowDown";
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!live || applied) return;
        if (!other.CompareTag("Player")) return;
        applied = true;
        ApplyConsequence(other);
    }

    void ApplyConsequence(Collider player)
    {
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        PlayerController controller = player.GetComponent<PlayerController>();

        switch (hazardType)
        {
            case BushlandHazardType.CactusWall:
                hud?.ChangeBucket(10f);
                Debug.Log("[Hazard] Cactus: bucket +10");
                break;
            case BushlandHazardType.DustDevil:
                if (controller != null) controller.ApplySpeedModifier(12f, 4f);
                hud?.ChangeHealth(-12f, "A dust devil knocked you out.");
                hud?.ChangeBucket(-10f);
                Debug.Log("[Hazard] Dust Devil: slow, health -12, bucket -10");
                break;
            case BushlandHazardType.HeatWave:
                hud?.ApplyHeatWave();
                Debug.Log("[Hazard] Heat Wave volume hit");
                break;
            case BushlandHazardType.SandPit:
                if (controller != null) controller.ApplySpeedModifier(10f, 2.5f);
                hud?.ChangeBucket(-8f);
                Debug.Log("[Hazard] Sand Pit: slow, bucket spill -8");
                break;
            case BushlandHazardType.Rock:
                hud?.ChangeHealth(-10f, "A rock took your last health.");
                hud?.ChangeBucket(-12f);
                Debug.Log("[Hazard] Rock: health -10, bucket -12");
                break;
            case BushlandHazardType.Pipe:
                if (controller != null) controller.ApplySpeedModifier(14f, 3f);
                hud?.ChangeHealth(-8f, "A pipe strike dropped your health to 0.");
                hud?.ChangeBucket(-10f);
                Debug.Log("[Hazard] Pipe: slow, health -8, bucket -10");
                break;
            case BushlandHazardType.Glass:
                if (controller != null) controller.ApplySpeedModifier(11f, 3f);
                hud?.ChangeHealth(-14f, "Broken glass dropped your health to 0.");
                hud?.ChangeBucket(-6f);
                Debug.Log("[Hazard] Glass: slow, health -14, bucket -6");
                break;
        }
    }
}
