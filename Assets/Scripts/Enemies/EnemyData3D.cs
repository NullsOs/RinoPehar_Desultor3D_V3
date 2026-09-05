using UnityEngine;

[CreateAssetMenu(
    fileName = "New Enemy Data",
    menuName = "Enemies/Enemy Data 3D"
)]
public class EnemyData3D : ScriptableObject
{
    [Header("Identification")]
    public string enemyTypeName = "Enemy";

    [Header("Movement")]
    [Min(0f)]
    public float walkSpeed = 2f;

    [Min(0f)]
    public float runSpeed = 4f;

    [Min(0f)]
    public float rotationSpeed = 10f;

    [Header("Player Detection")]
    [Min(0f)]
    public float detectionRange = 6f;

    [Min(0f)]
    public float detectionExitBuffer = 1f;

    [Header("Attack")]
    [Min(0f)]
    public float attackRange = 1.5f;

    [Min(0f)]
    public float attackExitBuffer = 0.25f;

    [Min(1)]
    public int damage = 1;

    [Min(0.01f)]
    public float attackCooldown = 1f;

    [Header("Animator State Names")]
    [Tooltip(
        "Enter the complete state path, for example " +
        "'Base Layer.Mutant Swiping'."
    )]
    public string attackStateName = "Base Layer.Attack";

    [Range(0f, 1f)]
    public float attackTransitionDuration = 0.1f;

    [Header("Platform Detection")]
    public LayerMask groundMask;

    [Min(0f)]
    public float ledgeCheckForwardDistance = 0.5f;

    [Min(0.01f)]
    public float ledgeCheckDownDistance = 2f;

    [Min(0f)]
    public float ledgeCheckUpOffset = 0.15f;

    [Min(0f)]
    public float wallCheckDistance = 0.4f;

    [Min(0f)]
    public float wallCheckHeight = 0.5f;

    [Header("Animation Speed")]
    [Min(0f)]
    public float walkAnimationSpeed = 1f;

    [Min(0f)]
    public float runAnimationSpeed = 1f;

    [Min(0f)]
    public float attackAnimationSpeed = 1f;

    [Header("Player Stomp")]
    [Min(0f)]
    public float playerBounceHeight = 1.5f;

    [Header("Healing Item Drop")]
    public GameObject healingItemPrefab;

    [Range(0f, 1f)]
    public float healingDropChance = 0.35f;

    [Min(0f)]
    public float dropRadius = 0.75f;

    public float dropHeight = 0.5f;

    private void OnValidate()
    {
        walkSpeed =
            Mathf.Max(0f, walkSpeed);

        runSpeed =
            Mathf.Max(runSpeed, walkSpeed);

        rotationSpeed =
            Mathf.Max(0f, rotationSpeed);

        attackRange =
            Mathf.Max(0f, attackRange);

        detectionRange =
            Mathf.Max(
                detectionRange,
                attackRange
            );

        detectionExitBuffer =
            Mathf.Max(
                0f,
                detectionExitBuffer
            );

        attackExitBuffer =
            Mathf.Max(
                0f,
                attackExitBuffer
            );

        damage =
            Mathf.Max(1, damage);

        attackCooldown =
            Mathf.Max(
                0.01f,
                attackCooldown
            );

        attackTransitionDuration =
            Mathf.Clamp01(
                attackTransitionDuration
            );

        ledgeCheckForwardDistance =
            Mathf.Max(
                0f,
                ledgeCheckForwardDistance
            );

        ledgeCheckDownDistance =
            Mathf.Max(
                0.01f,
                ledgeCheckDownDistance
            );

        ledgeCheckUpOffset =
            Mathf.Max(
                0f,
                ledgeCheckUpOffset
            );

        wallCheckDistance =
            Mathf.Max(
                0f,
                wallCheckDistance
            );

        wallCheckHeight =
            Mathf.Max(
                0f,
                wallCheckHeight
            );

        walkAnimationSpeed =
            Mathf.Max(
                0f,
                walkAnimationSpeed
            );

        runAnimationSpeed =
            Mathf.Max(
                0f,
                runAnimationSpeed
            );

        attackAnimationSpeed =
            Mathf.Max(
                0f,
                attackAnimationSpeed
            );

        playerBounceHeight =
            Mathf.Max(
                0f,
                playerBounceHeight
            );

        dropRadius =
            Mathf.Max(
                0f,
                dropRadius
            );
    }
}