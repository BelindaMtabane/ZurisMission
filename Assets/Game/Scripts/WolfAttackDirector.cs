using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds avoidable wolf ambushes to every runner level. Wolves sometimes wait
/// ahead and charge a telegraphed line; when the countdown is critical, one
/// pressure wolf also chases from behind.
/// </summary>
public class WolfAttackDirector : MonoBehaviour
{
    readonly List<WolfEnemy> activeWolves = new();
    Transform player;
    GameObject wolfPrefab;
    float nextRandomAttack;
    bool criticalWolfSpawned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() => TryCreate(SceneManager.GetActiveScene().name);

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TryCreate(scene.name);

    static void TryCreate(string sceneName)
    {
        if (!SceneCatalog.IsRunnerScene(sceneName)) return;
        if (FindFirstObjectByType<WolfAttackDirector>() != null) return;
        new GameObject("WolfAttackDirector").AddComponent<WolfAttackDirector>();
    }

    void Start()
    {
        wolfPrefab = Resources.Load<GameObject>("Enemies/Wolf");
        nextRandomAttack = Random.Range(8f, 13f);
        CachePlayer();
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null || wolfPrefab == null) return;

        activeWolves.RemoveAll(wolf => wolf == null);
        nextRandomAttack -= Time.deltaTime;
        if (nextRandomAttack <= 0f && activeWolves.Count < 2)
        {
            SpawnWolf(false);
            nextRandomAttack = Random.Range(16f, 24f);
        }

        LevelTimerUI timer = LevelTimerUI.Instance;
        if (!criticalWolfSpawned && timer != null &&
            timer.RemainingSeconds > 0f && timer.RemainingSeconds <= 15f)
        {
            criticalWolfSpawned = true;
            SpawnWolf(true);
        }
    }

    void SpawnWolf(bool fromBehind)
    {
        float halfPath = LevelLanes.OutermostLaneExtent() - 1.1f;
        float x = fromBehind
            ? Mathf.Clamp(player.position.x + Random.Range(-3f, 3f), -halfPath, halfPath)
            : Random.Range(-halfPath, halfPath);
        float z = player.position.z + (fromBehind ? -18f : Random.Range(55f, 78f));

        GameObject wolfObject = Instantiate(wolfPrefab, new Vector3(x, GroundY(), z), Quaternion.identity, transform);
        wolfObject.name = fromBehind ? "Wolf_TimePressure" : "Wolf_Ambush";
        WolfEnemy wolf = wolfObject.AddComponent<WolfEnemy>();
        wolf.Configure(player, fromBehind);
        activeWolves.Add(wolf);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController controller = FindFirstObjectByType<PlayerController>();
        if (controller != null) player = controller.transform;
    }

    static float GroundY()
    {
        if (SceneCatalog.IsLevel2(SceneCatalog.ActiveName)) return Level2Ground.SurfaceY;
        if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName)) return Level3Ground.SurfaceY;
        return Level1Ground.SurfaceY;
    }
}

public class WolfEnemy : MonoBehaviour
{
    enum WolfPhase { Waiting, Telegraph, Charge, Attack }

    const float HealthDamage = 20f;
    const float BucketDamage = 8f;
    const float AmbushNoticeDistance = 48f;
    const float TelegraphSeconds = 1.05f;
    const float AmbushSpeed = 40f;
    const float BehindCatchSpeed = 11f;
    const float BehindLockDistance = 9f;

    Transform player;
    PlayerController playerController;
    Animator animator;
    WolfPhase phase;
    bool fromBehind;
    bool hit;
    bool behindTargetLocked;
    float telegraphTimer;
    float lockedTargetX;
    Vector3 chargeDirection;

    public void Configure(Transform target, bool attacksFromBehind)
    {
        player = target;
        playerController = target != null ? target.GetComponent<PlayerController>() : null;
        fromBehind = attacksFromBehind;
        animator = GetComponentInChildren<Animator>(true);
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        FitAndGroundWolf();
        BuildHitTrigger();

        if (fromBehind)
        {
            phase = WolfPhase.Charge;
            PlayAnimation("Running_New");
            ShowWarning("WOLF BEHIND! MOVE ASIDE!");
        }
        else
        {
            phase = WolfPhase.Waiting;
            PlayAnimation("Idle_New");
        }
    }

    void Update()
    {
        if (player == null || hit) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        if (fromBehind) UpdateBehindAttack();
        else UpdateAmbush();

        TryDistanceHit();
    }

    void UpdateAmbush()
    {
        float distanceAhead = transform.position.z - player.position.z;
        if (phase == WolfPhase.Waiting)
        {
            FacePoint(player.position);
            if (distanceAhead <= AmbushNoticeDistance)
            {
                phase = WolfPhase.Telegraph;
                telegraphTimer = TelegraphSeconds;
                lockedTargetX = player.position.x;
                ShowWarning("WOLF AHEAD! DODGE OR JUMP!");
            }
            return;
        }

        if (phase == WolfPhase.Telegraph)
        {
            FacePoint(player.position);
            lockedTargetX = player.position.x;
            telegraphTimer -= Time.deltaTime;
            if (telegraphTimer <= 0f)
            {
                phase = WolfPhase.Charge;
                Vector3 target = new Vector3(lockedTargetX, transform.position.y, player.position.z - 12f);
                chargeDirection = (target - transform.position).normalized;
                PlayAnimation("Running_New");
            }
            return;
        }

        if (phase == WolfPhase.Charge)
        {
            transform.position += chargeDirection * AmbushSpeed * Time.deltaTime;
            FaceDirection(chargeDirection);
            if (transform.position.z < player.position.z - 10f) Destroy(gameObject);
        }
    }

    void UpdateBehindAttack()
    {
        float gap = player.position.z - transform.position.z;
        float playerSpeed = playerController != null ? playerController.CurrentSpeed : 25f;

        if (!behindTargetLocked)
        {
            transform.position = new Vector3(
                Mathf.MoveTowards(transform.position.x, player.position.x, 7f * Time.deltaTime),
                transform.position.y,
                transform.position.z);
            if (gap <= BehindLockDistance)
            {
                behindTargetLocked = true;
                lockedTargetX = player.position.x;
                ShowWarning("WOLF LUNGING! DODGE NOW!");
            }
        }
        else
        {
            transform.position = new Vector3(
                Mathf.MoveTowards(transform.position.x, lockedTargetX, 15f * Time.deltaTime),
                transform.position.y,
                transform.position.z);
        }

        Vector3 p = transform.position;
        p.z += (playerSpeed + BehindCatchSpeed) * Time.deltaTime;
        transform.position = p;
        FaceDirection(Vector3.forward);
        if (transform.position.z > player.position.z + 9f) Destroy(gameObject);
    }

    void TryDistanceHit()
    {
        Vector3 delta = player.position - transform.position;
        delta.y = 0f;
        if (delta.sqrMagnitude > 5.3f) return;
        TryHit(player.gameObject);
    }

    void OnTriggerEnter(Collider other) => TryHit(other.gameObject);
    void OnTriggerStay(Collider other) => TryHit(other.gameObject);

    void TryHit(GameObject other)
    {
        if (hit || phase != WolfPhase.Charge) return;
        PlayerController controller = other.GetComponent<PlayerController>()
                                      ?? other.GetComponentInParent<PlayerController>();
        if (controller == null) return;
        if (!controller.IsGrounded) return;

        hit = true;
        phase = WolfPhase.Attack;
        ObstacleGuideHUD.NotifyHit("wolf");
        GameAudio.PlayWolf();
        PlayAnimation(Random.value > 0.5f ? "AttackR" : "AttackL");
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ChangeHealth(-HealthDamage, "A wolf attacked you!");
        hud?.ChangeBucket(-BucketDamage);
        GameInfoUI.Post("Wolf attack! Health and bucket water lost.", GameInfoUI.MsgType.Obstacle);
        Destroy(gameObject, 1.1f);
    }

    void FitAndGroundWolf()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

        if (bounds.size.y > 0.01f)
        {
            // Keep wolves large enough to read clearly from the runner camera.
            float scale = Mathf.Clamp(4f / bounds.size.y, 0.2f, 7f);
            transform.localScale *= scale;
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 p = transform.position;
        p.y += WolfAttackDirectorGroundY() - bounds.min.y;
        transform.position = p;
    }

    float WolfAttackDirectorGroundY()
    {
        if (SceneCatalog.IsLevel2(SceneCatalog.ActiveName)) return Level2Ground.SurfaceY;
        if (SceneCatalog.IsLevel3(SceneCatalog.ActiveName)) return Level3Ground.SurfaceY;
        return Level1Ground.SurfaceY;
    }

    void BuildHitTrigger()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = transform.InverseTransformPoint(bounds.center);
        Vector3 scale = transform.lossyScale;
        box.size = new Vector3(
            bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(scale.x)),
            bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(scale.y)),
            bounds.size.z / Mathf.Max(0.001f, Mathf.Abs(scale.z)));

        Rigidbody body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    void PlayAnimation(string state)
    {
        if (animator == null) return;
        animator.Play(state, 0, 0f);
    }

    void FacePoint(Vector3 point)
    {
        Vector3 direction = point - transform.position;
        direction.y = 0f;
        FaceDirection(direction);
    }

    void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    static void ShowWarning(string message)
    {
        Color warning = new Color(1f, 0.32f, 0.18f);
        if (SceneCatalog.IsLevel1(SceneCatalog.ActiveName))
            Level1FeedbackUI.Show(message, warning, 1.5f);
        else if (SceneCatalog.IsLevel2(SceneCatalog.ActiveName))
            Level2FeedbackUI.Show(message, warning, 1.5f);
        else
            Level3FeedbackUI.Show(message, warning, 1.5f);
    }
}
