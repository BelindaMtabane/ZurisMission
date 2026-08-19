using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public enum SlideMode
    {
        Hold,
        Toggle
    }

    [Header("Lane Settings")]
    [SerializeField] private float[] lanePositions = { -6f, -2f, 2f, 6f };
    [SerializeField] private float laneLerpSpeed = 12f;
    [SerializeField] private int startingLane = 2;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 25f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float extraGravity = 0f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.45f;
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayerMask = ~0;

    [Header("Accessibility")]
    [SerializeField] private float coyoteTime = 0f;
    [SerializeField] private float jumpBufferTime = 0f;
    [SerializeField] private SlideMode slideMode = SlideMode.Hold;

    [Header("Slide")]
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private float slideHeightScale = 0.5f;
    [SerializeField] private float slideStaminaPerSecond = 18f;

    [Header("Grapple")]
    [SerializeField] private float grappleRange = 22f;
    [SerializeField] private float grappleDuration = 0.35f;
    [SerializeField] private float grappleStaminaCost = 22f;
    [SerializeField] private float grappleMaxAngle = 55f;
    [SerializeField] private bool seedTempTargetsIfMissing = false;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenPerSecond = 14f;
    [SerializeField] private float jumpStaminaCost = 0f;
    [SerializeField] private float staminaRegenDelay = 0.35f;

    [Header("Input (optional overrides)")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private int currentLane;
    private bool isGrounded;
    private float currentSpeed;
    private Coroutine speedModRoutine;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float jumpLockTimer;
    readonly Collider[] groundHits = new Collider[12];
    private bool isSliding;
    private float slideTimer;
    private Vector3 defaultCapsuleCenter;
    private float defaultCapsuleHeight;

    private bool isGrappling;
    private float stamina;
    private float staminaRegenLock;

    private InputAction jumpAction;
    private InputAction slideAction;
    private InputAction grappleAction;
    private InputAction laneLeftAction;
    private InputAction laneRightAction;
    private readonly System.Collections.Generic.List<InputAction> localActions = new System.Collections.Generic.List<InputAction>();

    private bool inputLocked;
    private float speedFruitMultiplier = 1f;
    private float speedFruitTimer;
    private float mudSlowMultiplier = 1f;
    private float mudSlowTimer;
    private float jumpBoostMultiplier = 1f;
    private float jumpBoostTimer;

    public float CurrentSpeed => currentSpeed;
    public bool IsGrounded => isGrounded;
    public bool IsSliding => isSliding;
    public bool IsGrappling => isGrappling;
    public bool IsInputLocked => inputLocked;
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
    public int CurrentLane => currentLane;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;
        capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            defaultCapsuleCenter = capsule.center;
            defaultCapsuleHeight = capsule.height;
        }
    }

    void OnEnable()
    {
        BindActions();
        for (int i = 0; i < localActions.Count; i++)
        {
            localActions[i].Enable();
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < localActions.Count; i++)
        {
            localActions[i].Disable();
            localActions[i].Dispose();
        }
        localActions.Clear();
    }

    void Start()
    {
        currentLane = Mathf.Clamp(startingLane, 0, lanePositions.Length - 1);
        stamina = maxStamina;
        LevelLanes.ConfigureForActiveScene();
        EnsureFourLanes();
        SnapLaneImmediately();

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (RunnerPlayerSetup.IsRunnerScene(sceneName))
        {
            ApplyRunnerMovementFeel();
        }

        if (RunnerLevelPacing.SupportsScene(sceneName))
        {
            RunnerLevelPacing.Apply(sceneName);
        }
        else
        {
            currentSpeed = baseSpeed;
        }

        Debug.Log($"[PlayerController] Ready lanes={lanePositions.Length} speed={currentSpeed}");
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (isGrappling) return;
        if (inputLocked) return;

        if (jumpLockTimer > 0f)
        {
            jumpLockTimer -= Time.deltaTime;
        }

        CheckGrounded();
        UpdateCoyoteAndBuffer();
        HandleLaneInput();
        HandleJumpInput();
        TickStamina(Time.deltaTime);
        TickRuntimeModifiers(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (isGrappling) return;

        ApplyExtraGravity();
        MovePlayer();
    }

    void BindActions()
    {
        InputActionAsset asset = inputActions;
        if (asset == null)
        {
            asset = InputSystem.actions;
        }
        InputActionMap map = asset != null ? asset.FindActionMap(actionMapName, false) : null;

        if (map != null)
        {
            jumpAction = map.FindAction("Jump", false);
            slideAction = map.FindAction("Crouch", false);
            grappleAction = map.FindAction("Grapple", false);
            laneLeftAction = map.FindAction("LaneLeft", false);
            laneRightAction = map.FindAction("LaneRight", false);
        }

        if (jumpAction == null || laneLeftAction == null || laneRightAction == null)
        {
            CreateFallbackActions();
        }

        jumpAction?.Enable();
        laneLeftAction?.Enable();
        laneRightAction?.Enable();
    }

    InputAction CreateLocal(string name, params string[] bindings)
    {
        InputAction action = new InputAction(name, InputActionType.Button);
        for (int i = 0; i < bindings.Length; i++)
        {
            action.AddBinding(bindings[i]);
        }
        localActions.Add(action);
        return action;
    }

    void CreateFallbackActions()
    {
        if (jumpAction == null)
            jumpAction = CreateLocal("Jump", "<Keyboard>/space");
        if (laneLeftAction == null)
            laneLeftAction = CreateLocal("LaneLeft", "<Keyboard>/a", "<Keyboard>/leftArrow");
        if (laneRightAction == null)
            laneRightAction = CreateLocal("LaneRight", "<Keyboard>/d", "<Keyboard>/rightArrow");
    }

    bool WasPressed(InputAction action)
    {
        return action != null && action.WasPressedThisFrame();
    }

    bool IsHeld(InputAction action)
    {
        return action != null && action.IsPressed();
    }

    void CheckGrounded()
    {
        if (jumpLockTimer > 0f)
        {
            isGrounded = false;
            return;
        }

        float radius = groundCheckRadius;
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        if (capsule != null)
        {
            radius = Mathf.Max(0.2f, capsule.radius * 0.85f);
            float bottom = capsule.bounds.min.y;
            origin = new Vector3(transform.position.x, bottom + radius + 0.08f, transform.position.z);
        }

        isGrounded = HasGroundHit(origin, radius, 0f)
                     || HasGroundHit(origin, radius, groundCheckDistance);
    }

    bool HasGroundHit(Vector3 origin, float radius, float downDistance)
    {
        if (downDistance <= 0.001f)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, radius, groundHits, groundLayerMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (IsExternalGround(groundHits[i])) return true;
            }

            return false;
        }

        if (!Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, downDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return IsExternalGround(hit.collider);
    }

    bool IsExternalGround(Collider col)
    {
        if (col == null || col.isTrigger) return false;
        Transform t = col.transform;
        if (t == transform || t.IsChildOf(transform)) return false;
        return true;
    }

    void UpdateCoyoteAndBuffer()
    {
        if (!isGrounded)
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void HandleLaneInput()
    {
        Keyboard kb = Keyboard.current;
        bool left = WasPressed(laneLeftAction)
                    || (kb != null && (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame))
                    || Input.GetKeyDown(KeyCode.A)
                    || Input.GetKeyDown(KeyCode.LeftArrow);
        bool right = WasPressed(laneRightAction)
                     || (kb != null && (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame))
                     || Input.GetKeyDown(KeyCode.D)
                     || Input.GetKeyDown(KeyCode.RightArrow);

        if (left)
        {
            ShiftLane(-1);
        }
        else if (right)
        {
            ShiftLane(1);
        }
    }

    void HandleJumpInput()
    {
        Keyboard kb = Keyboard.current;
        bool jumpPressed = WasPressed(jumpAction)
                           || (kb != null && kb.spaceKey.wasPressedThisFrame)
                           || Input.GetKeyDown(KeyCode.Space);

        if (!jumpPressed) return;
        if (!isGrounded || jumpLockTimer > 0f) return;
        if (rb.linearVelocity.y > 0.4f) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * (jumpForce * jumpBoostMultiplier), ForceMode.Impulse);
        isGrounded = false;
        coyoteCounter = 0f;
        jumpBufferCounter = 0f;
        jumpLockTimer = 0.18f;
        if (jumpStaminaCost > 0f)
        {
            SpendStamina(jumpStaminaCost);
        }
    }

    void EnsureFourLanes()
    {
        lanePositions = LevelLanes.Xs;
        startingLane = 2;
        currentLane = 2;
    }

    void HandleSlideInput()
    {
        if (!isGrounded && !isSliding) return;

        if (slideMode == SlideMode.Toggle)
        {
            if (WasPressed(slideAction))
            {
                if (isSliding)
                {
                    EndSlide();
                }
                else if (stamina > 0f)
                {
                    BeginSlide();
                }
            }
        }
        else
        {
            bool wantSlide = IsHeld(slideAction);
            if (wantSlide && !isSliding && stamina > 0f && isGrounded)
            {
                BeginSlide();
            }
            else if (!wantSlide && isSliding)
            {
                EndSlide();
            }
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            SpendStamina(slideStaminaPerSecond * Time.deltaTime);
            if (slideTimer <= 0f || stamina <= 0f)
            {
                EndSlide();
            }
        }
    }

    void BeginSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        if (capsule != null)
        {
            capsule.height = defaultCapsuleHeight * slideHeightScale;
            capsule.center = new Vector3(
                defaultCapsuleCenter.x,
                defaultCapsuleCenter.y * slideHeightScale,
                defaultCapsuleCenter.z);
        }
    }

    void EndSlide()
    {
        if (!isSliding) return;
        isSliding = false;
        if (capsule != null)
        {
            capsule.height = defaultCapsuleHeight;
            capsule.center = defaultCapsuleCenter;
        }
    }

    void HandleGrappleInput()
    {
        if (!WasPressed(grappleAction) || isSliding) return;
        if (stamina < grappleStaminaCost) return;

        GrappleTarget target = FindBestGrappleTarget();
        if (target == null || !target.CanGrapple)
        {
            Debug.Log("[PlayerController] Grapple: no target in range.");
            return;
        }

        StartCoroutine(GrappleRoutine(target));
    }

    GrappleTarget FindBestGrappleTarget()
    {
        GrappleTarget[] targets = FindObjectsByType<GrappleTarget>(FindObjectsSortMode.None);
        GrappleTarget best = null;
        float bestScore = float.MaxValue;

        Vector3 origin = transform.position;
        Vector3 forward = Vector3.forward;

        for (int i = 0; i < targets.Length; i++)
        {
            GrappleTarget t = targets[i];
            if (t == null || !t.CanGrapple) continue;

            Vector3 to = t.GrapplePoint.position - origin;
            float dist = to.magnitude;
            if (dist > grappleRange || dist < 1.5f) continue;
            if (to.z < 0.5f) continue;

            float angle = Vector3.Angle(forward, to);
            if (angle > grappleMaxAngle) continue;

            float score = dist + angle;
            if (score < bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }

    IEnumerator GrappleRoutine(GrappleTarget target)
    {
        isGrappling = true;
        EndSlide();
        SpendStamina(grappleStaminaCost);

        Vector3 start = rb.position;
        Vector3 end = target.GrapplePoint.position;
        end.x = GetNearestLaneX(end.x);

        float t = 0f;
        rb.useGravity = false;
        Debug.Log($"[PlayerController] Grapple start -> {target.name}");

        while (t < 1f)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                break;
            }

            t += Time.deltaTime / Mathf.Max(0.05f, grappleDuration);
            Vector3 p = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            rb.MovePosition(p);
            yield return null;
        }

        SnapToNearestLane();
        rb.useGravity = true;
        rb.linearVelocity = new Vector3(0f, 0f, 0f);
        isGrappling = false;
        Debug.Log("[PlayerController] Grapple complete.");
    }

    void TickStamina(float dt)
    {
        staminaRegenLock -= dt;
        if (isSliding || isGrappling) return;
        if (staminaRegenLock > 0f) return;

        stamina = Mathf.Min(maxStamina, stamina + staminaRegenPerSecond * dt);
    }

    void SpendStamina(float amount)
    {
        stamina = Mathf.Max(0f, stamina - amount);
        staminaRegenLock = staminaRegenDelay;
    }

    void ShiftLane(int direction)
    {
        currentLane = Mathf.Clamp(currentLane + direction, 0, lanePositions.Length - 1);
    }

    void SnapLaneImmediately()
    {
        if (lanePositions == null || lanePositions.Length == 0) return;
        Vector3 p = rb.position;
        p.x = lanePositions[currentLane];
        rb.position = p;
    }

    void SnapToNearestLane()
    {
        currentLane = NearestLaneIndex(rb.position.x);
    }

    int NearestLaneIndex(float x)
    {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < lanePositions.Length; i++)
        {
            float d = Mathf.Abs(lanePositions[i] - x);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    float GetNearestLaneX(float x)
    {
        return lanePositions[NearestLaneIndex(x)];
    }

    void ApplyExtraGravity()
    {
        if (isGrounded) return;
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    void MovePlayer()
    {
        float targetX = lanePositions[currentLane];
        float nextX = Mathf.Lerp(rb.position.x, targetX, laneLerpSpeed * Time.fixedDeltaTime);
        float nextZ = rb.position.z + (currentSpeed * Time.fixedDeltaTime);
        float yVel = rb.linearVelocity.y;
        rb.MovePosition(new Vector3(nextX, rb.position.y, nextZ));
        rb.linearVelocity = new Vector3(0f, yVel, 0f);
    }

    public void ApplySpeedModifier(float newSpeed, float duration)
    {
        if (speedModRoutine != null)
        {
            StopCoroutine(speedModRoutine);
        }

        speedModRoutine = StartCoroutine(SpeedModifierRoutine(newSpeed, duration));
    }

    public void ConfigureForwardSpeed(float speed)
    {
        baseSpeed = Mathf.Max(0.1f, speed);
        RefreshRuntimeSpeed();
    }

    public void ApplyRunnerMovementFeel()
    {
        laneLerpSpeed = 16f;
        jumpForce = 15f;
        extraGravity = 16f;
        coyoteTime = 0f;
        jumpBufferTime = 0f;
        LevelLanes.ConfigureForActiveScene();
        EnsureFourLanes();
        SnapLaneImmediately();
    }

    public void ApplyLevel1MovementFeel()
    {
        ApplyRunnerMovementFeel();
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    public void ApplySpeedFruit(float multiplier, float duration)
    {
        if (speedModRoutine != null)
        {
            StopCoroutine(speedModRoutine);
            speedModRoutine = null;
        }

        speedFruitMultiplier = Mathf.Max(1f, multiplier);
        speedFruitTimer = Mathf.Max(0.1f, duration);
        RefreshRuntimeSpeed();
    }

    public void ApplyMudSlow(float multiplier, float duration)
    {
        mudSlowMultiplier = Mathf.Clamp(multiplier, 0.2f, 1f);
        mudSlowTimer = Mathf.Max(0.1f, duration);
        RefreshRuntimeSpeed();
    }

    public void ApplyJumpBoost(float multiplier, float duration)
    {
        jumpBoostMultiplier = Mathf.Max(1f, multiplier);
        jumpBoostTimer = Mathf.Max(0.1f, duration);
    }

    void TickRuntimeModifiers(float dt)
    {
        bool speedDirty = false;

        if (speedFruitTimer > 0f)
        {
            speedFruitTimer -= dt;
            if (speedFruitTimer <= 0f)
            {
                speedFruitTimer = 0f;
                speedFruitMultiplier = 1f;
                speedDirty = true;
            }
        }

        if (mudSlowTimer > 0f)
        {
            mudSlowTimer -= dt;
            if (mudSlowTimer <= 0f)
            {
                mudSlowTimer = 0f;
                mudSlowMultiplier = 1f;
                speedDirty = true;
            }
        }

        if (jumpBoostTimer > 0f)
        {
            jumpBoostTimer -= dt;
            if (jumpBoostTimer <= 0f)
            {
                jumpBoostTimer = 0f;
                jumpBoostMultiplier = 1f;
            }
        }

        if (speedDirty)
        {
            RefreshRuntimeSpeed();
        }
    }

    void RefreshRuntimeSpeed()
    {
        if (speedModRoutine != null) return;
        currentSpeed = baseSpeed * speedFruitMultiplier * mudSlowMultiplier;
    }

    IEnumerator SpeedModifierRoutine(float newSpeed, float duration)
    {
        currentSpeed = newSpeed;
        yield return new WaitForSeconds(duration);
        speedModRoutine = null;
        RefreshRuntimeSpeed();
    }

    void SeedTemporaryGrappleTargets()
    {
        if (FindFirstObjectByType<GrappleTarget>() != null) return;

        for (int i = 1; i <= 4; i++)
        {
            GameObject go = new GameObject($"TempGrappleTarget_{i}");
            go.transform.position = new Vector3(
                lanePositions[i % lanePositions.Length],
                rb.position.y + 2.5f,
                rb.position.z + 18f * i);
            go.AddComponent<GrappleTarget>();
        }

        Debug.Log("[PlayerController] Seeded temporary GrappleTarget objects.");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;
        Debug.Log($"[PlayerController] Collision with {collision.gameObject.name} tag={collision.gameObject.tag}");
    }
}
