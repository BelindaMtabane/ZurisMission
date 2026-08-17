using UnityEngine;

public class Level3BossDirector : MonoBehaviour
{
    public static bool BossDefeated { get; private set; }

    const float TriggerDistance = 70f;

    [SerializeField] Transform layoutRoot;
    [SerializeField] float spawnProgress = 0.985f;

    enum Phase { Wait, MudSweep, Puddles, Acid, FinalPipe, Victory }
    Phase phase = Phase.Wait;
    float timer;
    Transform player;
    bool spawnedSweep;
    bool spawnedPuddles;
    bool spawnedAcid;
    GameObject finalPipe;

    public void Setup(Transform root, float progress)
    {
        layoutRoot = root;
        spawnProgress = progress;
        BossDefeated = false;
        transform.position = new Vector3(LevelLanes.PathCenterX, Level3Ground.SurfaceY, Level3Progress.WorldZ(progress));
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        switch (phase)
        {
            case Phase.Wait:
                if (player.position.z > transform.position.z - TriggerDistance)
                {
                    phase = Phase.MudSweep;
                    timer = 6f;
                    Level3FeedbackUI.Show("FINAL BOSS — MUD SWEEPS!", new Color(0.7f, 0.4f, 0.15f), 2.2f);
                }
                break;
            case Phase.MudSweep:
                if (!spawnedSweep)
                {
                    spawnedSweep = true;
                    Level3Primitives.MakeMudSweep(layoutRoot, transform.position.z + 8f, true, Level3EnemyPace.Medium);
                    Level3Primitives.MakeMudSweep(layoutRoot, transform.position.z + 18f, false, Level3EnemyPace.Fast);
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Puddles;
                    timer = 5f;
                    Level3FeedbackUI.Show("BOSS — MUD PUDDLES!", new Color(0.62f, 0.42f, 0.18f), 1.8f);
                }
                break;
            case Phase.Puddles:
                if (!spawnedPuddles)
                {
                    spawnedPuddles = true;
                    Level3Primitives.MakeMudPuddle(layoutRoot, 1, transform.position.z + 26f);
                    Level3Primitives.MakeMudPuddle(layoutRoot, 2, transform.position.z + 32f);
                    Level3Primitives.MakeRock(layoutRoot, 0, transform.position.z + 32f);
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.Acid;
                    timer = 6f;
                    Level3FeedbackUI.Show("BOSS — ACID RAIN! Grab leaves!", new Color(0.4f, 0.9f, 0.25f), 2f);
                }
                break;
            case Phase.Acid:
                if (!spawnedAcid)
                {
                    spawnedAcid = true;
                    Level3Primitives.MakeLeaf(layoutRoot, 3, transform.position.z + 36f);
                    Level3Primitives.MakeAcidRain(layoutRoot, transform.position.z + 44f, new[] { 0, 1 }, spawnProgress);
                }
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    phase = Phase.FinalPipe;
                    finalPipe = Level3Primitives.MakeBossRepair(layoutRoot, 2, transform.position.z + 54f);
                    Level3FeedbackUI.Show("RESTORE THE WATER — HIT THE YELLOW PIPE!", new Color(1f, 0.9f, 0.2f), 2.4f);
                }
                break;
        }
    }

    public static void NotifyBossPipeRepaired()
    {
        BossDefeated = true;
        Level3FeedbackUI.Show("WATER RESTORED! BOSS DEFEATED!", new Color(0.35f, 0.9f, 1f), 2.8f);
        FindFirstObjectByType<HUDControls>()?.LevelProgress();
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}

public class Level3BossRepairPoint : MonoBehaviour
{
    bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (!Level3PipeRepair.AllTanksRepaired)
        {
            Level3FeedbackUI.Show("REPAIR ALL THREE TANKS FIRST", new Color(1f, 0.5f, 0.2f), 1.6f);
            return;
        }

        used = true;
        Level3BossDirector.NotifyBossPipeRepaired();
    }
}
