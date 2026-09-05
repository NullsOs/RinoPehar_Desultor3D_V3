using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private static readonly int IsRunningParameter =
        Animator.StringToHash("isRunning");

    private static readonly int IsJumpingParameter =
        Animator.StringToHash("isJumping");

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 6f;

    [SerializeField]
    private float rotationSpeed = 12f;

    [Header("Jump / Gravity")]
    [SerializeField]
    private float jumpHeight = 2f;

    [SerializeField]
    private float gravity = -25f;

    [SerializeField]
    private float groundedGravity = -2f;

    [Tooltip(
        "Small amount of time during which the player " +
        "can still jump after walking off a platform."
    )]
    [SerializeField]
    private float groundedGraceTime = 0.12f;

    [Tooltip(
        "Prevents the ground check from immediately cancelling " +
        "a jump while the player is still touching the floor."
    )]
    [SerializeField]
    private float minimumJumpAirTime = 0.1f;

    [Header("Ground Detection")]
    [Tooltip(
        "Include normal ground and moving-platform layers."
    )]
    [SerializeField]
    private LayerMask groundMask = ~0;

    [Min(0.01f)]
    [SerializeField]
    private float groundCheckRadius = 0.32f;

    [Min(0f)]
    [SerializeField]
    private float groundCheckOffset = 0.08f;

    [Min(0.01f)]
    [SerializeField]
    private float groundCheckDistance = 0.18f;

    [Header("Wall Jump - Side View Only")]
    [SerializeField]
    private LayerMask wallJumpMask;

    [SerializeField]
    private float wallCheckDistance = 0.55f;

    [SerializeField]
    private float wallCheckHeight = 0.8f;

    [SerializeField]
    private float wallJumpHeight = 2.2f;

    [SerializeField]
    private float wallJumpPushForce = 7f;

    [SerializeField]
    private float wallJumpControlLockTime = 0.18f;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Transform visualModel;

    [SerializeField]
    private float runningInputThreshold = 0.1f;

    [Tooltip(
        "Minimum actual horizontal movement required " +
        "before the running animation starts."
    )]
    [SerializeField]
    private float runningVelocityThreshold = 0.05f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs;

    private CharacterController controller;

    private Vector3 moveDirection;
    private Vector3 velocity;
    private Vector3 wallJumpHorizontalVelocity;

    private float horizontalInput;
    private float verticalInput;
    private float wallJumpControlLockTimer;

    private float lastGroundedTime =
        float.NegativeInfinity;

    private float lastJumpTime =
        float.NegativeInfinity;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool jumpAnimationActive;

    private Vector3 wallNormal;

    public bool IsFalling =>
        velocity.y < -0.1f;

    public float VerticalVelocity =>
        velocity.y;

    public bool IsGrounded =>
        isGrounded;

    private void Awake()
    {
        controller =
            GetComponent<CharacterController>();

        if (visualModel == null)
        {
            visualModel = transform;
        }

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    private void Start()
    {
        RefreshGroundedState();

        if (animator != null)
        {
            animator.SetBool(
                IsRunningParameter,
                false
            );

            animator.SetBool(
                IsJumpingParameter,
                false
            );
        }
    }

    private void Update()
    {
        RefreshGroundedState();

        ReadInput();
        CheckWallJump();
        HandleJumpInput();
        Move();

     
        RefreshGroundedState();

        RotateVisual();
        UpdateAnimator();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.uiManager != null)
            {
                UIManager.uiManager
                    .ShowContinuePanel();
            }
        }
    }

    private void ReadInput()
    {
        horizontalInput =
            Input.GetAxisRaw("Horizontal");

        verticalInput =
            Input.GetAxisRaw("Vertical");

        CameraMode currentCameraMode =
            GetCurrentCameraMode();

        if (currentCameraMode ==
            CameraMode.SideView)
        {
            verticalInput = 0f;

            moveDirection =
                GetSideViewRightDirection() *
                horizontalInput;
        }
        else
        {
            moveDirection =
                new Vector3(
                    horizontalInput,
                    0f,
                    verticalInput
                );
        }

        moveDirection =
            Vector3.ClampMagnitude(
                moveDirection,
                1f
            );
    }

    private void RefreshGroundedState()
    {
        bool controllerGrounded =
            controller != null &&
            controller.isGrounded;

        bool probeGrounded =
            IsGroundBelowPlayer();

        bool currentlyGrounded =
            controllerGrounded ||
            probeGrounded;

     
        bool insideJumpIgnoreTime =
            Time.time - lastJumpTime <
            minimumJumpAirTime;

        if (insideJumpIgnoreTime &&
            velocity.y > 0f)
        {
            currentlyGrounded = false;
        }

        if (currentlyGrounded)
        {
            lastGroundedTime =
                Time.time;
        }

        isGrounded =
            currentlyGrounded ||
            (
                !jumpAnimationActive &&
                Time.time - lastGroundedTime <=
                groundedGraceTime
            );

      
        if (currentlyGrounded &&
            velocity.y <= 0f)
        {
            Land();
        }
    }

    private bool IsGroundBelowPlayer()
    {
        if (controller == null)
        {
            return false;
        }

        Bounds controllerBounds =
            controller.bounds;

        Vector3 sphereOrigin =
            new Vector3(
                controllerBounds.center.x,
                controllerBounds.min.y +
                groundCheckOffset +
                groundCheckRadius,
                controllerBounds.center.z
            );

        float castDistance =
            groundCheckDistance +
            groundCheckOffset;

        return Physics.SphereCast(
            sphereOrigin,
            groundCheckRadius,
            Vector3.down,
            out _,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void Land()
    {
        bool wasJumping =
            jumpAnimationActive;

        jumpAnimationActive = false;

        if (velocity.y < groundedGravity)
        {
            velocity.y =
                groundedGravity;
        }

        wallJumpHorizontalVelocity =
            Vector3.zero;

        wallJumpControlLockTimer = 0f;

        if (wasJumping &&
            showDebugLogs)
        {
            Debug.Log(
                "Player landed. Jump animation disabled.",
                this
            );
        }
    }

    private void CheckWallJump()
    {
        isTouchingWall = false;
        wallNormal = Vector3.zero;

        if (GetCurrentCameraMode() !=
            CameraMode.SideView)
        {
            return;
        }

        if (isGrounded)
        {
            return;
        }

        Vector3 origin =
            transform.position +
            Vector3.up *
            wallCheckHeight;

        Vector3 rightDirection =
            GetSideViewRightDirection();

        Vector3 leftDirection =
            -rightDirection;

        if (Physics.Raycast(
                origin,
                rightDirection,
                out RaycastHit rightHit,
                wallCheckDistance,
                wallJumpMask,
                QueryTriggerInteraction.Ignore))
        {
            isTouchingWall = true;
            wallNormal = rightHit.normal;
            return;
        }

        if (Physics.Raycast(
                origin,
                leftDirection,
                out RaycastHit leftHit,
                wallCheckDistance,
                wallJumpMask,
                QueryTriggerInteraction.Ignore))
        {
            isTouchingWall = true;
            wallNormal = leftHit.normal;
        }
    }

    private void HandleJumpInput()
    {
        if (!Input.GetButtonDown("Jump"))
        {
            return;
        }

        if (isGrounded)
        {
            Jump();
            return;
        }

        if (GetCurrentCameraMode() ==
                CameraMode.SideView &&
            isTouchingWall)
        {
            WallJump();
        }
    }

    private void Move()
    {
        Vector3 horizontalMove;

        if (wallJumpControlLockTimer > 0f)
        {
            wallJumpControlLockTimer -=
                Time.deltaTime;

            horizontalMove =
                wallJumpHorizontalVelocity;
        }
        else
        {
            horizontalMove =
                moveDirection *
                moveSpeed;

            wallJumpHorizontalVelocity =
                Vector3.MoveTowards(
                    wallJumpHorizontalVelocity,
                    Vector3.zero,
                    moveSpeed *
                    4f *
                    Time.deltaTime
                );
        }

        velocity.y +=
            gravity *
            Time.deltaTime;

        Vector3 finalMove =
            horizontalMove +
            Vector3.up *
            velocity.y;

        controller.Move(
            finalMove *
            Time.deltaTime
        );
    }

    private void Jump()
    {
        velocity.y =
            Mathf.Sqrt(
                jumpHeight *
                -2f *
                gravity
            );

        BeginJump();

        if (SFXManager.sfxManager != null)
        {
            SFXManager.sfxManager
                .PlayPlayerJump();
        }
    }

    private void WallJump()
    {
        velocity.y =
            Mathf.Sqrt(
                wallJumpHeight *
                -2f *
                gravity
            );

        Vector3 pushDirection =
            wallNormal;

        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude <
            0.001f)
        {
            pushDirection =
                -moveDirection;
        }

        pushDirection.Normalize();

        wallJumpHorizontalVelocity =
            pushDirection *
            wallJumpPushForce;

        wallJumpControlLockTimer =
            wallJumpControlLockTime;

        BeginJump();

        if (SFXManager.sfxManager != null)
        {
            SFXManager.sfxManager
                .PlayPlayerJump();
        }
    }

    private void BeginJump()
    {
        lastJumpTime =
            Time.time;

        lastGroundedTime =
            float.NegativeInfinity;

        isGrounded = false;
        jumpAnimationActive = true;

        if (animator != null)
        {
            animator.SetBool(
                IsRunningParameter,
                false
            );

            animator.SetBool(
                IsJumpingParameter,
                true
            );
        }
    }

    public void Bounce(float bounceHeight)
    {
        velocity.y =
            Mathf.Sqrt(
                bounceHeight *
                -2f *
                gravity
            );

        BeginJump();
    }

  
    public void RefreshGroundedAfterExternalMove()
    {
        if (controller == null)
        {
            return;
        }

        RefreshGroundedState();
        UpdateAnimator();
    }

    private void RotateVisual()
    {
        if (visualModel == null)
        {
            return;
        }

        Vector3 lookDirection =
            moveDirection.sqrMagnitude >
            0.001f
                ? moveDirection
                : wallJumpHorizontalVelocity;

        if (lookDirection.sqrMagnitude <
            0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                lookDirection.normalized
            );

        visualModel.rotation =
            Quaternion.Slerp(
                visualModel.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    private void UpdateAnimator()
    {
        if (animator == null ||
            controller == null)
        {
            return;
        }

        bool movementInputPressed =
            Mathf.Abs(horizontalInput) >
                runningInputThreshold ||
            Mathf.Abs(verticalInput) >
                runningInputThreshold;

        Vector3 actualHorizontalVelocity =
            controller.velocity;

        actualHorizontalVelocity.y = 0f;

        bool actuallyMoving =
            actualHorizontalVelocity.magnitude >
            runningVelocityThreshold;

        bool shouldJump =
            jumpAnimationActive &&
            !isGrounded;

        bool shouldRun =
            movementInputPressed &&
            actuallyMoving &&
            isGrounded &&
            !shouldJump;

        animator.SetBool(
            IsJumpingParameter,
            shouldJump
        );

        animator.SetBool(
            IsRunningParameter,
            shouldRun
        );
    }

    private Vector3 GetSideViewRightDirection()
    {
        Transform cameraTransform =
            Camera.main != null
                ? Camera.main.transform
                : null;

        if (cameraTransform == null)
        {
            return Vector3.right;
        }

        Vector3 right =
            cameraTransform.right;

        right.y = 0f;

        if (right.sqrMagnitude <
            0.001f)
        {
            return Vector3.right;
        }

        return right.normalized;
    }

    private CameraMode GetCurrentCameraMode()
    {
        if (CameraController3D.cameraController !=
            null)
        {
            return CameraController3D
                .cameraController
                .CurrentMode;
        }

        return CameraMode.ThirdPerson;
    }

    private void OnDrawGizmosSelected()
    {
        CharacterController currentController =
            GetComponent<CharacterController>();

        if (currentController != null)
        {
            Bounds controllerBounds =
                currentController.bounds;

            Vector3 groundSphereOrigin =
                new Vector3(
                    controllerBounds.center.x,
                    controllerBounds.min.y +
                    groundCheckOffset +
                    groundCheckRadius,
                    controllerBounds.center.z
                );

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                groundSphereOrigin +
                Vector3.down *
                (
                    groundCheckDistance +
                    groundCheckOffset
                ),
                groundCheckRadius
            );
        }

        Vector3 wallOrigin =
            transform.position +
            Vector3.up *
            wallCheckHeight;

        Vector3 rightDirection =
            Application.isPlaying
                ? GetSideViewRightDirection()
                : Vector3.right;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            wallOrigin,
            wallOrigin +
            rightDirection *
            wallCheckDistance
        );

        Gizmos.DrawLine(
            wallOrigin,
            wallOrigin -
            rightDirection *
            wallCheckDistance
        );
    }
}