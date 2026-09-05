using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyFSM3D : MonoBehaviour
{
    public enum PatrolAxis
    {
        X,
        Z
    }

    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    private static readonly int MoveSpeedParameter =
        Animator.StringToHash("MoveSpeed");

    private static readonly int IsAttackingParameter =
        Animator.StringToHash("isAttacking");

    [Header("Enemy Configuration")]
    [SerializeField]
    private EnemyData3D enemyData;

    [Tooltip(
        "Patrolling uses this axis. Chasing can use both X and Z."
    )]
    [SerializeField]
    private PatrolAxis patrolAxis = PatrolAxis.X;

    [SerializeField]
    private bool startInNegativeDirection;

    [Header("References")]
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Transform ledgeCheckOrigin;

    [SerializeField]
    private Transform wallCheckOrigin;

    [SerializeField]
    private Animator animator;

    [Header("Chase")]
    [SerializeField]
    private bool freeMovementWhileChasing = true;

    [SerializeField]
    private bool stopAtPlatformEdgesWhileChasing = true;

    [Header("Debug")]
    [SerializeField]
    private bool drawDebugGizmos = true;

    [SerializeField]
    private bool showDebugLogs;

    private Rigidbody enemyRigidbody;

    private EnemyState currentState =
        EnemyState.Patrol;

    private int patrolDirection = 1;
    private float nextAttackTime;
    private bool dead;

    public EnemyData3D Data =>
        enemyData;

    public bool IsDead =>
        dead;

    private Vector3 SelectedPatrolAxis
    {
        get
        {
            return patrolAxis == PatrolAxis.X
                ? Vector3.right
                : Vector3.forward;
        }
    }

    private void Awake()
    {
        enemyRigidbody =
            GetComponent<Rigidbody>();

        enemyRigidbody.useGravity = true;
        enemyRigidbody.isKinematic = false;

        enemyRigidbody.constraints =
            RigidbodyConstraints.FreezeRotation;

        enemyRigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;

        enemyRigidbody.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        patrolDirection =
            startInNegativeDirection
                ? -1
                : 1;

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;

            animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;
        }

        if (ledgeCheckOrigin == null)
        {
            ledgeCheckOrigin =
                transform;
        }

        if (wallCheckOrigin == null)
        {
            wallCheckOrigin =
                transform;
        }
    }

    private void Start()
    {
        FindPlayer();
        ValidateSetup();

        ApplyAnimatorValues();

        FaceDirection(
            SelectedPatrolAxis *
            patrolDirection,
            true
        );
    }

    private void FixedUpdate()
    {
        if (dead ||
            enemyData == null)
        {
            StopHorizontalMovement();
            return;
        }

        if (player == null ||
            !player.gameObject.activeInHierarchy)
        {
            FindPlayer();
        }

        UpdateState();

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Attack:
                AttackPlayer();
                break;
        }

        ApplyAnimatorValues();
    }

    private void ValidateSetup()
    {
        if (enemyData == null)
        {
            Debug.LogError(
                $"{name} has no EnemyData3D assigned.",
                this
            );

            return;
        }

        if (animator == null)
        {
            Debug.LogError(
                $"{name} has no Animator assigned.",
                this
            );

            return;
        }

        if (enemyData.groundMask.value == 0)
        {
            Debug.LogWarning(
                $"{name}: Ground Mask is empty.",
                this
            );
        }

        if (!HasAnimatorParameter(
                MoveSpeedParameter,
                AnimatorControllerParameterType.Float))
        {
            Debug.LogError(
                $"{name}: Animator is missing the Float " +
                "parameter 'MoveSpeed'.",
                animator
            );
        }

        if (!HasAnimatorParameter(
                IsAttackingParameter,
                AnimatorControllerParameterType.Bool))
        {
            Debug.LogError(
                $"{name}: Animator is missing the Bool " +
                "parameter 'isAttacking'.",
                animator
            );
        }

        ValidateAttackState();
    }

    private bool HasAnimatorParameter(
        int parameterHash,
        AnimatorControllerParameterType type)
    {
        if (animator == null)
        {
            return false;
        }

        foreach (
            AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (parameter.nameHash ==
                    parameterHash &&
                parameter.type == type)
            {
                return true;
            }
        }

        return false;
    }

    private void ValidateAttackState()
    {
        if (animator == null ||
            enemyData == null ||
            string.IsNullOrWhiteSpace(
                enemyData.attackStateName))
        {
            return;
        }

        int attackStateHash =
            Animator.StringToHash(
                enemyData.attackStateName
            );

        if (!animator.HasState(
                0,
                attackStateHash))
        {
            Debug.LogError(
                $"{name}: Animator Layer 0 does not contain " +
                $"the state '{enemyData.attackStateName}'. " +
                "Enter its complete path in EnemyData3D, " +
                "for example 'Base Layer.Mutant Swiping'.",
                animator
            );
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        player =
            playerObject != null
                ? playerObject.transform
                : null;
    }

    private void UpdateState()
    {
        if (player == null)
        {
            ChangeState(
                EnemyState.Patrol
            );

            return;
        }

        float distance =
            GetHorizontalDistanceToPlayer();

        if (currentState ==
            EnemyState.Attack)
        {
            float attackExitDistance =
                enemyData.attackRange +
                enemyData.attackExitBuffer;

            if (distance <=
                attackExitDistance)
            {
                return;
            }
        }

        if (distance <=
            enemyData.attackRange)
        {
            ChangeState(
                EnemyState.Attack
            );

            return;
        }

        if (currentState ==
            EnemyState.Chase)
        {
            float chaseExitDistance =
                enemyData.detectionRange +
                enemyData.detectionExitBuffer;

            if (distance <=
                chaseExitDistance)
            {
                return;
            }
        }

        if (distance <=
            enemyData.detectionRange)
        {
            ChangeState(
                EnemyState.Chase
            );
        }
        else
        {
            ChangeState(
                EnemyState.Patrol
            );
        }
    }

    private void ChangeState(
        EnemyState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        EnemyState previousState =
            currentState;

        currentState =
            newState;

        if (showDebugLogs)
        {
            Debug.Log(
                $"{name}: {previousState} -> {newState}",
                this
            );
        }

        ApplyAnimatorValues();

        if (newState ==
            EnemyState.Attack)
        {
            EnterAttackAnimation();
        }
    }

    private void EnterAttackAnimation()
    {
        if (animator == null ||
            enemyData == null)
        {
            return;
        }

        animator.speed =
            enemyData.attackAnimationSpeed;

        animator.SetFloat(
            MoveSpeedParameter,
            0f
        );

        animator.SetBool(
            IsAttackingParameter,
            true
        );

        if (string.IsNullOrWhiteSpace(
                enemyData.attackStateName))
        {
            return;
        }

        int attackStateHash =
            Animator.StringToHash(
                enemyData.attackStateName
            );

        if (!animator.HasState(
                0,
                attackStateHash))
        {
            Debug.LogError(
                $"{name} cannot play attack state " +
                $"'{enemyData.attackStateName}'. " +
                "Check the exact state name in the Animator.",
                animator
            );

            return;
        }

        animator.CrossFade(
            attackStateHash,
            enemyData.attackTransitionDuration,
            0,
            0f
        );

        if (showDebugLogs)
        {
            Debug.Log(
                $"{name}: Playing attack animation " +
                $"'{enemyData.attackStateName}'.",
                animator
            );
        }
    }

    private void Patrol()
    {
        Vector3 direction =
            SelectedPatrolAxis *
            patrolDirection;

        if (!HasGroundAhead(direction) ||
            HasWallAhead(direction))
        {
            TurnAround();
            return;
        }

        MoveInDirection(
            direction,
            enemyData.walkSpeed
        );
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            ChangeState(
                EnemyState.Patrol
            );

            StopHorizontalMovement();
            return;
        }

        Vector3 directionToPlayer =
            player.position -
            transform.position;

        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <
            0.001f)
        {
            StopHorizontalMovement();
            return;
        }

        Vector3 chaseDirection;

        if (freeMovementWhileChasing)
        {
            chaseDirection =
                directionToPlayer.normalized;
        }
        else
        {
            float axisOffset =
                Vector3.Dot(
                    directionToPlayer,
                    SelectedPatrolAxis
                );

            if (Mathf.Abs(axisOffset) <
                0.01f)
            {
                StopHorizontalMovement();
                return;
            }

            chaseDirection =
                SelectedPatrolAxis *
                Mathf.Sign(axisOffset);
        }

        if (directionToPlayer.magnitude <=
            enemyData.attackRange)
        {
            StopHorizontalMovement();
            FaceDirection(chaseDirection);

            ChangeState(
                EnemyState.Attack
            );

            return;
        }

        if (stopAtPlatformEdgesWhileChasing &&
            !HasGroundAhead(chaseDirection))
        {
            StopHorizontalMovement();
            FaceDirection(chaseDirection);
            return;
        }

        if (HasWallAhead(chaseDirection))
        {
            StopHorizontalMovement();
            FaceDirection(chaseDirection);
            return;
        }

        MoveInDirection(
            chaseDirection,
            enemyData.runSpeed
        );
    }

    private void AttackPlayer()
    {
        StopHorizontalMovement();

        if (player == null)
        {
            ChangeState(
                EnemyState.Patrol
            );

            return;
        }

        Vector3 directionToPlayer =
            player.position -
            transform.position;

        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude >
            0.001f)
        {
            FaceDirection(
                directionToPlayer.normalized
            );
        }

        float distance =
            directionToPlayer.magnitude;

        float attackExitDistance =
            enemyData.attackRange +
            enemyData.attackExitBuffer;

        if (distance >
            attackExitDistance)
        {
            ChangeState(
                EnemyState.Chase
            );

            return;
        }

        if (Time.time <
            nextAttackTime)
        {
            return;
        }

        PlayerDamageReceiver damageReceiver =
            FindPlayerDamageReceiver();

        if (damageReceiver == null)
        {
            Debug.LogWarning(
                $"{name} cannot attack because Player " +
                "has no PlayerDamageReceiver.",
                this
            );

            nextAttackTime =
                Time.time +
                enemyData.attackCooldown;

            return;
        }

        damageReceiver.TakeDamage(
            enemyData.damage
        );

        nextAttackTime =
            Time.time +
            enemyData.attackCooldown;

        
        EnterAttackAnimation();
    }

    private PlayerDamageReceiver
        FindPlayerDamageReceiver()
    {
        if (player == null)
        {
            return null;
        }

        PlayerDamageReceiver receiver =
            player.GetComponent
                <PlayerDamageReceiver>();

        if (receiver == null)
        {
            receiver =
                player.GetComponentInChildren
                    <PlayerDamageReceiver>(true);
        }

        if (receiver == null)
        {
            receiver =
                player.GetComponentInParent
                    <PlayerDamageReceiver>();
        }

        return receiver;
    }

    private bool HasGroundAhead(
        Vector3 moveDirection)
    {
        if (ledgeCheckOrigin == null)
        {
            return true;
        }

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude <
            0.001f)
        {
            return true;
        }

        moveDirection.Normalize();

        Vector3 rayOrigin =
            ledgeCheckOrigin.position +
            moveDirection *
            enemyData.ledgeCheckForwardDistance +
            Vector3.up *
            enemyData.ledgeCheckUpOffset;

        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            enemyData.ledgeCheckDownDistance,
            enemyData.groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private bool HasWallAhead(
        Vector3 moveDirection)
    {
        if (wallCheckOrigin == null)
        {
            return false;
        }

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude <
            0.001f)
        {
            return false;
        }

        moveDirection.Normalize();

        Vector3 rayOrigin =
            wallCheckOrigin.position +
            Vector3.up *
            enemyData.wallCheckHeight;

        return Physics.Raycast(
            rayOrigin,
            moveDirection,
            enemyData.wallCheckDistance,
            enemyData.groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void MoveInDirection(
        Vector3 direction,
        float movementSpeed)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            StopHorizontalMovement();
            return;
        }

        direction.Normalize();

        Vector3 horizontalVelocity =
            direction *
            movementSpeed;

        enemyRigidbody.linearVelocity =
            new Vector3(
                horizontalVelocity.x,
                enemyRigidbody.linearVelocity.y,
                horizontalVelocity.z
            );

        FaceDirection(direction);
    }

    private void StopHorizontalMovement()
    {
        if (enemyRigidbody == null)
        {
            return;
        }

        enemyRigidbody.linearVelocity =
            new Vector3(
                0f,
                enemyRigidbody.linearVelocity.y,
                0f
            );
    }

    private void TurnAround()
    {
        patrolDirection *= -1;

        StopHorizontalMovement();

        FaceDirection(
            SelectedPatrolAxis *
            patrolDirection
        );
    }

    private void FaceDirection(
        Vector3 direction,
        bool immediately = false)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized
            );

        Quaternion newRotation;

        if (immediately ||
            enemyData == null)
        {
            newRotation =
                targetRotation;
        }
        else
        {
            newRotation =
                Quaternion.Slerp(
                    enemyRigidbody.rotation,
                    targetRotation,
                    enemyData.rotationSpeed *
                    Time.fixedDeltaTime
                );
        }

        enemyRigidbody.MoveRotation(
            newRotation
        );
    }

    private float
        GetHorizontalDistanceToPlayer()
    {
        if (player == null)
        {
            return float.PositiveInfinity;
        }

        Vector3 difference =
            player.position -
            transform.position;

        difference.y = 0f;

        return difference.magnitude;
    }

    private void ApplyAnimatorValues()
    {
        if (animator == null ||
            enemyData == null)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                animator.speed =
                    enemyData.walkAnimationSpeed;

                animator.SetFloat(
                    MoveSpeedParameter,
                    0.5f
                );

                animator.SetBool(
                    IsAttackingParameter,
                    false
                );
                break;

            case EnemyState.Chase:
                animator.speed =
                    enemyData.runAnimationSpeed;

                animator.SetFloat(
                    MoveSpeedParameter,
                    1f
                );

                animator.SetBool(
                    IsAttackingParameter,
                    false
                );
                break;

            case EnemyState.Attack:
                animator.speed =
                    enemyData.attackAnimationSpeed;

                animator.SetFloat(
                    MoveSpeedParameter,
                    0f
                );

                animator.SetBool(
                    IsAttackingParameter,
                    true
                );
                break;
        }
    }

    public void MarkAsDead()
    {
        if (dead)
        {
            return;
        }

        dead = true;

        StopHorizontalMovement();

        if (animator != null)
        {
            animator.SetFloat(
                MoveSpeedParameter,
                0f
            );

            animator.SetBool(
                IsAttackingParameter,
                false
            );
        }

        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos ||
            enemyData == null)
        {
            return;
        }

        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            enemyData.detectionRange
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            enemyData.attackRange
        );

        Vector3 direction =
            SelectedPatrolAxis *
            (
                startInNegativeDirection
                    ? -1f
                    : 1f
            );

        Transform ledgeOrigin =
            ledgeCheckOrigin != null
                ? ledgeCheckOrigin
                : transform;

        Vector3 ledgeRayOrigin =
            ledgeOrigin.position +
            direction *
            enemyData.ledgeCheckForwardDistance +
            Vector3.up *
            enemyData.ledgeCheckUpOffset;

        Gizmos.color =
            Color.green;

        Gizmos.DrawLine(
            ledgeRayOrigin,
            ledgeRayOrigin +
            Vector3.down *
            enemyData.ledgeCheckDownDistance
        );

        Transform wallOrigin =
            wallCheckOrigin != null
                ? wallCheckOrigin
                : transform;

        Vector3 wallRayOrigin =
            wallOrigin.position +
            Vector3.up *
            enemyData.wallCheckHeight;

        Gizmos.color =
            Color.blue;

        Gizmos.DrawLine(
            wallRayOrigin,
            wallRayOrigin +
            direction *
            enemyData.wallCheckDistance
        );
    }
}