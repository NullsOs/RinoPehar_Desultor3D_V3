using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MovingPlatform3D : MonoBehaviour
{
    public enum MoveAxis
    {
        X,
        Y,
        Z
    }

    [Header("Optional ScriptableObject")]
    [Tooltip(
        "When assigned, speed, distance and axis come " +
        "from this ScriptableObject."
    )]
    [SerializeField]
    private PlatformMovementData movementData;

    [Header("Movement")]
    [Tooltip(
        "Used when Movement Data is not assigned."
    )]
    [SerializeField]
    private MoveAxis moveAxis = MoveAxis.Y;

    [Tooltip(
        "Used when Movement Data is not assigned."
    )]
    [Min(0f)]
    [SerializeField]
    private float moveDistance = 4f;

    [Tooltip(
        "Used when Movement Data is not assigned."
    )]
    [Min(0f)]
    [SerializeField]
    private float moveSpeed = 2f;

    [Min(0f)]
    [SerializeField]
    private float waitTimeAtEnds = 0.2f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private Vector3 platformDelta;

    private float waitTimer;

    public Vector3 PlatformDelta =>
        platformDelta;

    private float CurrentMoveSpeed
    {
        get
        {
            return movementData != null
                ? movementData.moveSpeed
                : moveSpeed;
        }
    }

    private float CurrentMoveDistance
    {
        get
        {
            return movementData != null
                ? movementData.moveDistance
                : moveDistance;
        }
    }

    private void Awake()
    {
        startPosition =
            transform.position;

        endPosition =
            startPosition +
            GetMoveDirection() *
            CurrentMoveDistance;

        targetPosition =
            endPosition;

        previousPosition =
            transform.position;
    }

    private void Update()
    {
        Vector3 positionBeforeMovement =
            transform.position;

        if (waitTimer > 0f)
        {
            waitTimer -=
                Time.deltaTime;

            platformDelta =
                Vector3.zero;

            previousPosition =
                transform.position;

            return;
        }

        Vector3 newPosition =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                CurrentMoveSpeed *
                Time.deltaTime
            );

        transform.position =
            newPosition;

        platformDelta =
            transform.position -
            positionBeforeMovement;

        previousPosition =
            transform.position;

        if (Vector3.Distance(
                transform.position,
                targetPosition) <= 0.01f)
        {
            bool reachedEnd =
                Vector3.Distance(
                    targetPosition,
                    endPosition) <= 0.01f;

            targetPosition =
                reachedEnd
                    ? startPosition
                    : endPosition;

            waitTimer =
                waitTimeAtEnds;
        }
    }

    private Vector3 GetMoveDirection()
    {
        if (movementData != null)
        {
            switch (movementData.moveDirection)
            {
                case PlatformMoveDirection3D.XAxis:
                    return Vector3.right;

                case PlatformMoveDirection3D.YAxis:
                    return Vector3.up;

                case PlatformMoveDirection3D.ZAxis:
                    return Vector3.forward;
            }
        }

        switch (moveAxis)
        {
            case MoveAxis.X:
                return Vector3.right;

            case MoveAxis.Y:
                return Vector3.up;

            case MoveAxis.Z:
                return Vector3.forward;

            default:
                return Vector3.up;
        }
    }

    private void OnValidate()
    {
        moveDistance =
            Mathf.Max(
                0f,
                moveDistance
            );

        moveSpeed =
            Mathf.Max(
                0f,
                moveSpeed
            );

        waitTimeAtEnds =
            Mathf.Max(
                0f,
                waitTimeAtEnds
            );
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 direction =
            GetMoveDirection();

        float distance =
            CurrentMoveDistance;

        Vector3 start =
            Application.isPlaying
                ? startPosition
                : transform.position;

        Vector3 end =
            start +
            direction *
            distance;

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(
            start,
            end
        );

        Gizmos.DrawWireSphere(
            start,
            0.15f
        );

        Gizmos.DrawWireSphere(
            end,
            0.15f
        );
    }
}