using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerMovingPlatformHandler : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip(
        "Include only the layer used by moving platforms."
    )]
    [SerializeField]
    private LayerMask platformMask;

    [Min(0.01f)]
    [SerializeField]
    private float checkRadius = 0.35f;

    [Min(0f)]
    [SerializeField]
    private float checkYOffset = 0.08f;

    [Min(0.01f)]
    [SerializeField]
    private float checkDistance = 0.2f;

    [Header("Debug")]
    [SerializeField]
    private bool debugLogs;

    private CharacterController controller;
    private PlayerMovement playerMovement;

    private MovingPlatform3D currentPlatform;

    private void Awake()
    {
        controller =
            GetComponent<CharacterController>();

        playerMovement =
            GetComponent<PlayerMovement>();
    }

    private void LateUpdate()
    {
        DetectPlatform();

        if (currentPlatform == null)
        {
            return;
        }

      
        if (!playerMovement.IsGrounded)
        {
            LeaveCurrentPlatform();
            return;
        }

        Vector3 platformDelta =
            currentPlatform.PlatformDelta;

        if (platformDelta.sqrMagnitude <=
            0.000001f)
        {
            playerMovement
                .RefreshGroundedAfterExternalMove();

            return;
        }

        controller.Move(
            platformDelta
        );


        playerMovement
            .RefreshGroundedAfterExternalMove();
    }

    private void DetectPlatform()
    {
        RaycastHit[] hits =
            Physics.SphereCastAll(
                GetSphereOrigin(),
                checkRadius,
                Vector3.down,
                checkDistance,
                platformMask,
                QueryTriggerInteraction.Ignore
            );

        MovingPlatform3D foundPlatform =
            null;

        float closestDistance =
            float.PositiveInfinity;

        foreach (RaycastHit hit in hits)
        {
            MovingPlatform3D candidate =
                hit.collider.GetComponentInParent
                    <MovingPlatform3D>();

            if (candidate == null)
            {
                continue;
            }

            
            
            if (Vector3.Dot(
                    hit.normal,
                    Vector3.up) < 0.5f)
            {
                continue;
            }

            if (hit.distance <
                closestDistance)
            {
                closestDistance =
                    hit.distance;

                foundPlatform =
                    candidate;
            }
        }

        if (foundPlatform ==
            currentPlatform)
        {
            return;
        }

        if (debugLogs &&
            currentPlatform != null)
        {
            Debug.Log(
                "[PlatformHandler] Left platform: " +
                currentPlatform.name,
                this
            );
        }

        currentPlatform =
            foundPlatform;

        if (debugLogs &&
            currentPlatform != null)
        {
            Debug.Log(
                "[PlatformHandler] Standing on: " +
                currentPlatform.name,
                this
            );
        }
    }

    private Vector3 GetSphereOrigin()
    {
        Bounds controllerBounds =
            controller.bounds;

        return new Vector3(
            controllerBounds.center.x,
            controllerBounds.min.y +
            checkYOffset +
            checkRadius,
            controllerBounds.center.z
        );
    }

    private void LeaveCurrentPlatform()
    {
        if (debugLogs &&
            currentPlatform != null)
        {
            Debug.Log(
                "[PlatformHandler] Player jumped off: " +
                currentPlatform.name,
                this
            );
        }

        currentPlatform = null;
    }

    private void OnDrawGizmosSelected()
    {
        CharacterController currentController =
            GetComponent<CharacterController>();

        if (currentController == null)
        {
            return;
        }

        Bounds controllerBounds =
            currentController.bounds;

        Vector3 sphereOrigin =
            new Vector3(
                controllerBounds.center.x,
                controllerBounds.min.y +
                checkYOffset +
                checkRadius,
                controllerBounds.center.z
            );

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            sphereOrigin +
            Vector3.down *
            checkDistance,
            checkRadius
        );
    }
}