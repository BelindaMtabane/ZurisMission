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
    [SerializeField] private float[] lanePositions = { -8.25f, -2.75f, 2.75f, 8.25f };
    [SerializeField] private float laneLerpSpeed = 12f;
    [SerializeField] private int startingLane = 2;
    [SerializeField] private bool freeLateralMovement = true;
    [SerializeField] private float lateralMoveSpeed = 17f;

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
    private Animator bodyAnimator;
    private const string RunAnimState = "RunningCarrying";
    private const string IdleAnimState = "idleCarrying";
    private const string WalkAnimState = "WalkingCarrying";
    private const string TenderAnimState = "Tender Placement";
    private bool showingStartIdle;
    private int currentLane;
    private float lateralInput;
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
    private bool playingFinishSequence;
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
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            defaultCapsuleCenter = capsule.center;
            defaultCapsuleHeight = capsule.height;
        }
        bodyAnimator = GetComponentInChildren<Animator>();
        ConfigureCharacterAnimator();
        ConfigureCharacterMaterials();
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
            SnapToGroundSurface();
        }

        if (RunnerLevelPacing.SupportsScene(sceneName))
        {
            RunnerLevelPacing.Apply(sceneName);
        }
        else
        {
            currentSpeed = baseSpeed;
        }

        if (seedTempTargetsIfMissing)
        {
            SeedTemporaryGrappleTargets();
        }

        if (bodyAnimator != null)
        {
            bool waiting = RunStateManager.Instance == null
                           || RunStateManager.Instance.CurrentState == RunStateManager.RunState.WaitingToStart;
            bodyAnimator.Play(waiting ? IdleAnimState : RunAnimState, 0, 0f);
            bodyAnimator.Update(0f);
            showingStartIdle = waiting;
        }

        Debug.Log($"[PlayerController] Ready lanes={lanePositions.Length} speed={currentSpeed}");
    }

    void Update()
    {
        if (playingFinishSequence) return;
        if (IsWaitingToStart())
        {
            HoldOnGroundWhileWaiting();
            return;
        }

        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        RestorePlayGravity();

        if (showingStartIdle && bodyAnimator != null)
        {
            bodyAnimator.CrossFadeInFixedTime(RunAnimState, 0.16f, 0, 0f);
            showingStartIdle = false;
        }
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
        if (playingFinishSequence) return;
        if (IsWaitingToStart())
        {
            HoldOnGroundWhileWaiting();
            return;
        }

        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        RestorePlayGravity();
        if (isGrappling) return;

        ApplyExtraGravity();
        MovePlayer();
        RescueIfFallenThrough();
    }

    bool IsWaitingToStart()
    {
        return RunStateManager.Instance != null
               && RunStateManager.Instance.CurrentState == RunStateManager.RunState.WaitingToStart;
    }

    void ConfigureCharacterAnimator()
    {
        if (bodyAnimator == null) return;

        bodyAnimator.applyRootMotion = false;
        // Animate every rendered frame; the interpolated Rigidbody smooths the player root.
        bodyAnimator.updateMode = AnimatorUpdateMode.Normal;
        bodyAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        // Mecanim Foot IK collapses this imported Mixamo leg rig. Avatar retargeting
        // already supplies the correct foot pose, so do not apply a second correction.
        bodyAnimator.stabilizeFeet = false;
        bodyAnimator.speed = 0.82f;
    }

    void ConfigureCharacterMaterials()
    {
        if (bodyAnimator == null) return;

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        SkinnedMeshRenderer[] renderers =
            bodyAnimator.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        for (int r = 0; r < renderers.Length; r++)
        {
            SkinnedMeshRenderer skinned = renderers[r];
            Material[] materials = skinned.materials;

            for (int m = 0; m < materials.Length; m++)
            {
                Material mat = materials[m];
                if (mat == null) continue;

                Texture albedo = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap")
                    : mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex")
                    : null;

                if (urpLit != null && mat.shader != urpLit)
                {
                    Color previousColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                        : mat.HasProperty("_Color") ? mat.GetColor("_Color")
                        : Color.white;
                    mat.shader = urpLit;
                    if (mat.HasProperty("_BaseMap") && albedo != null)
                        mat.SetTexture("_BaseMap", albedo);
                    if (mat.HasProperty("_BaseColor"))
                        mat.SetColor("_BaseColor", previousColor);
                }

                if (albedo == null && mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", ZuriFallbackColor(skinned.name, mat.name));

                if (mat.HasProperty("_Smoothness"))
                    mat.SetFloat("_Smoothness", 0.2f);
            }

            skinned.materials = materials;
            skinned.updateWhenOffscreen = true;
        }
    }

    public void RefreshCharacterAnimator()
    {
        bodyAnimator = GetComponentInChildren<Animator>();
        ConfigureCharacterAnimator();
        ConfigureCharacterMaterials();
        if (bodyAnimator != null)
        {
            bool waiting = RunStateManager.Instance != null
                           && RunStateManager.Instance.CurrentState == RunStateManager.RunState.WaitingToStart;
            bodyAnimator.Play(waiting ? IdleAnimState : RunAnimState, 0, 0f);
            bodyAnimator.Update(0f);
            showingStartIdle = waiting;
        }
    }

    static Color ZuriFallbackColor(string rendererName, string materialName)
    {
        string key = ((rendererName ?? "") + " " + (materialName ?? "")).ToLowerInvariant();
        if (key.Contains("hair")) return new Color(0.12f, 0.045f, 0.025f);
        if (key.Contains("body") || key.Contains("skin")) return new Color(0.42f, 0.18f, 0.10f);
        if (key.Contains("cloth")) return new Color(0.78f, 0.24f, 0.62f);
        if (key.Contains("sneaker") || key.Contains("shoe")) return new Color(0.12f, 0.30f, 0.85f);
        if (key.Contains("sock")) return new Color(0.95f, 0.95f, 0.98f);
        if (key.Contains("lash") || key.Contains("eye")) return new Color(0.04f, 0.025f, 0.02f);
        return Color.white;
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

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
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
        bool leftHeld = IsHeld(laneLeftAction)
                        || (kb != null && (kb.aKey.isPressed || kb.leftArrowKey.isPressed))
                        || Input.GetKey(KeyCode.A)
                        || Input.GetKey(KeyCode.LeftArrow);
        bool rightHeld = IsHeld(laneRightAction)
                         || (kb != null && (kb.dKey.isPressed || kb.rightArrowKey.isPressed))
                         || Input.GetKey(KeyCode.D)
                         || Input.GetKey(KeyCode.RightArrow);
        lateralInput = (rightHeld ? 1f : 0f) - (leftHeld ? 1f : 0f);

        if (freeLateralMovement) return;

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

        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }

        bool wantsJump = jumpPressed || jumpBufferCounter > 0f;
        bool canJump = (isGrounded || coyoteCounter > 0f) && jumpLockTimer <= 0f;
        if (!wantsJump || !canJump) return;
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

    public void SnapToGroundSurface()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (capsule == null) capsule = GetComponent<CapsuleCollider>();

        Vector3 p = rb != null ? rb.position : transform.position;
        p.y = RunnerPlayerSetup.StandingRootY(transform);
        if (rb != null)
        {
            rb.position = p;
            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(v.x, Mathf.Max(0f, v.y), v.z);
        }
        else
        {
            transform.position = p;
        }
    }

    void HoldOnGroundWhileWaiting()
    {
        if (rb == null) return;
        rb.useGravity = false;
        SnapToGroundSurface();
        rb.linearVelocity = Vector3.zero;
    }

    void RestorePlayGravity()
    {
        if (rb != null && !isGrappling) rb.useGravity = true;
    }

    void RescueIfFallenThrough()
    {
        if (rb == null) return;
        float surfaceY = RunnerPlayerSetup.SurfaceYForActiveScene();
        float bottom = capsule != null ? capsule.bounds.min.y : rb.position.y;
        if (bottom >= surfaceY - 0.45f) return;

        SnapToGroundSurface();
        Vector3 v = rb.linearVelocity;
        if (v.y < 0f) rb.linearVelocity = new Vector3(v.x, 0f, v.z);
    }

    void SnapLaneImmediately()
    {
        if (lanePositions == null || lanePositions.Length == 0) return;
        Vector3 p = rb.position;
        p.x = freeLateralMovement ? LevelLanes.PathCenterX : lanePositions[currentLane];
        rb.position = p;
        currentLane = NearestLaneIndex(p.x);
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
        if (freeLateralMovement) return ClampToPath(x);
        return lanePositions[NearestLaneIndex(x)];
    }

    float ClampToPath(float x)
    {
        if (lanePositions == null || lanePositions.Length == 0) return x;
        float playerInset = capsule != null ? Mathf.Max(0.5f, capsule.radius) : 0.75f;
        float minX = lanePositions[0] - LevelLanes.LaneHalfWidth + playerInset;
        float maxX = lanePositions[lanePositions.Length - 1] + LevelLanes.LaneHalfWidth - playerInset;
        return Mathf.Clamp(x, minX, maxX);
    }

    void ApplyExtraGravity()
    {
        if (isGrounded) return;
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    void MovePlayer()
    {
        float nextX;
        if (freeLateralMovement)
        {
            nextX = ClampToPath(rb.position.x + lateralInput * lateralMoveSpeed * Time.fixedDeltaTime);
            currentLane = NearestLaneIndex(nextX);
        }
        else
        {
            float targetX = lanePositions[currentLane];
            nextX = Mathf.Lerp(rb.position.x, targetX, laneLerpSpeed * Time.fixedDeltaTime);
        }
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
        if (locked) lateralInput = 0f;
    }

    public IEnumerator PlayFinishSequence()
    {
        playingFinishSequence = true;
        showingStartIdle = false;
        SetInputLocked(true);
        EndSlide();
        isGrappling = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;

        PlayFinishAnim(WalkAnimState, "WalkingCarrying", 1f);

        const float walkDuration = 1.15f;
        const float walkSpeed = 2.6f;
        float elapsed = 0f;
        while (elapsed < walkDuration)
        {
            float dt = Time.deltaTime;
            Vector3 position = rb.position;
            position.z += walkSpeed * dt;
            rb.position = position;
            elapsed += dt;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        PlayFinishAnim(TenderAnimState, "TenderPlacement", 1f);
        yield return WaitForAnim(TenderAnimState, 9.5f);
        HoldFinishPose();
        yield return new WaitForSeconds(0.35f);
    }

    void PlayFinishAnim(string stateName, string triggerName, float speed)
    {
        if (bodyAnimator == null) return;

        bodyAnimator.speed = speed;
        bodyAnimator.ResetTrigger("RunningCarrying");
        bodyAnimator.ResetTrigger("TenderPlacement");
        bodyAnimator.ResetTrigger("IdleCarrying");
        bodyAnimator.ResetTrigger("WalkingCarrying");
        if (!string.IsNullOrEmpty(triggerName))
            bodyAnimator.SetTrigger(triggerName);
        bodyAnimator.Play(stateName, 0, 0f);
        bodyAnimator.Update(0f);
    }

    IEnumerator WaitForAnim(string stateName, float fallbackSeconds)
    {
        if (bodyAnimator == null)
        {
            yield return new WaitForSeconds(fallbackSeconds);
            yield break;
        }

        float entered = 0f;
        while (entered < 0.6f && !bodyAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            entered += Time.deltaTime;
            yield return null;
        }

        float clipLength = fallbackSeconds;
        AnimatorClipInfo[] clips = bodyAnimator.GetCurrentAnimatorClipInfo(0);
        if (clips != null && clips.Length > 0 && clips[0].clip != null)
            clipLength = clips[0].clip.length;

        float maxWait = (clipLength / Mathf.Max(0.05f, bodyAnimator.speed)) + 0.4f;
        float waited = 0f;
        while (waited < maxWait)
        {
            AnimatorStateInfo info = bodyAnimator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName) && info.normalizedTime >= 0.98f)
                yield break;
            waited += Time.deltaTime;
            yield return null;
        }
    }

    void HoldFinishPose()
    {
        if (bodyAnimator == null) return;
        bodyAnimator.speed = 0f;
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
