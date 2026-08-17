using UnityEngine;

public class Level3AcidRainZone : MonoBehaviour
{
    const float TriggerDistance = 48f;

    [SerializeField] float spawnProgress;
    [SerializeField] int[] lanes = { 0, 1 };
    [SerializeField] float damagePerTick = 5f;
    [SerializeField] GameObject warningRoot;
    [SerializeField] GameObject rainRoot;

    enum Phase { Idle, Warning, Rain, Heavy, Clear }
    Phase phase = Phase.Idle;
    float timer;
    float tick;
    Transform player;

    public void Setup(float progress, int[] rainLanes, GameObject warning, GameObject rain)
    {
        spawnProgress = progress;
        lanes = rainLanes;
        warningRoot = warning;
        rainRoot = rain;
        if (warningRoot != null) warningRoot.SetActive(false);
        if (rainRoot != null) rainRoot.SetActive(false);
        damagePerTick = progress < 0.7f ? 4f : 7f;
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        switch (phase)
        {
            case Phase.Idle:
                if (player.position.z > transform.position.z - TriggerDistance)
                {
                    phase = Phase.Warning;
                    timer = 2f;
                    if (warningRoot != null) warningRoot.SetActive(true);
                    Level3FeedbackUI.Show("ACID RAIN WARNING!", new Color(0.45f, 0.95f, 0.25f), 2f);
                }
                break;
            case Phase.Warning:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Rain;
                    timer = 2.2f;
                    if (warningRoot != null) warningRoot.SetActive(false);
                    if (rainRoot != null) rainRoot.SetActive(true);
                }
                break;
            case Phase.Rain:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Heavy;
                    timer = 1.6f;
                }
                break;
            case Phase.Heavy:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Clear;
                    if (rainRoot != null) rainRoot.SetActive(false);
                }
                break;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (phase != Phase.Rain && phase != Phase.Heavy) return;
        if (!other.CompareTag("Player")) return;
        if (Level3LeafProtection.TryBlockAcid()) return;

        tick += Time.deltaTime;
        if (tick < 0.4f) return;
        tick = 0f;
        float dmg = phase == Phase.Heavy ? damagePerTick * 1.4f : damagePerTick;
        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-dmg, "Acid rain burned you!");
        Level3FeedbackUI.Show("ACID RAIN!", new Color(0.4f, 0.9f, 0.2f), 0.6f);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
