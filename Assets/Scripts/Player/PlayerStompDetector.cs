using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerStompDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private Transform feetCheck;

    [Header("Detection")]
    [Tooltip(
        "This should contain only the EnemyHead layer."
    )]
    [SerializeField]
    private LayerMask enemyHeadMask;

    [Min(0.05f)]
    [SerializeField]
    private float stompRadius = 0.5f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = true;

    private readonly Collider[] stompResults =
        new Collider[16];

    private CharacterController characterController;

    private void Awake()
    {
        characterController =
            GetComponent<CharacterController>();

        if (playerMovement == null)
        {
            playerMovement =
                GetComponent<PlayerMovement>();
        }

        if (playerMovement == null)
        {
            Debug.LogError(
                "PlayerStompDetector could not find " +
                "PlayerMovement on the Player.",
                this
            );
        }
    }

    private void Update()
    {
        if (playerMovement == null)
        {
            return;
        }

        if (!playerMovement.IsFalling)
        {
            return;
        }

        DetectEnemyHead();
    }

    private void DetectEnemyHead()
    {
        Vector3 feetPosition =
            GetFeetPosition();

        int hitCount =
            Physics.OverlapSphereNonAlloc(
                feetPosition,
                stompRadius,
                stompResults,
                enemyHeadMask,
                QueryTriggerInteraction.Collide
            );

        if (showDebugLogs &&
            hitCount > 0)
        {
            Debug.Log(
                $"Player stomp check found " +
                $"{hitCount} collider(s).",
                this
            );
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit =
                stompResults[i];

            if (hit == null)
            {
                continue;
            }

            if (showDebugLogs)
            {
                Debug.Log(
                    $"Stomp detector found collider: " +
                    $"{hit.name}, layer: " +
                    $"{LayerMask.LayerToName(hit.gameObject.layer)}",
                    hit
                );
            }

            EnemyHeadHitbox headHitbox =
                FindHeadHitbox(hit);

            if (headHitbox != null)
            {
                if (showDebugLogs)
                {
                    Debug.Log(
                        $"EnemyHeadHitbox found on " +
                        $"{headHitbox.name}.",
                        headHitbox
                    );
                }

                bool killed =
                    headHitbox.TryKillEnemy(
                        playerMovement
                    );

                if (killed)
                {
                    ClearResults(hitCount);
                    return;
                }

                continue;
            }

            
            EnemyDeath enemyDeath =
                hit.GetComponentInParent
                    <EnemyDeath>();

            if (enemyDeath != null &&
                !enemyDeath.IsDead)
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning(
                        $"The collider {hit.name} is on " +
                        "EnemyHead, but it has no " +
                        "EnemyHeadHitbox. Using EnemyDeath " +
                        "from the parent as a fallback.",
                        hit
                    );
                }

                enemyDeath.KillEnemy(
                    playerMovement
                );

                ClearResults(hitCount);
                return;
            }

            Debug.LogError(
                $"Collider {hit.name} was detected by the " +
                "stomp check, but neither EnemyHeadHitbox " +
                "nor EnemyDeath could be found. Make " +
                "HeadHitbox a child of the enemy root.",
                hit
            );
        }

        ClearResults(hitCount);
    }

    private EnemyHeadHitbox FindHeadHitbox(
        Collider hit)
    {
        EnemyHeadHitbox headHitbox =
            hit.GetComponent<EnemyHeadHitbox>();

        if (headHitbox == null)
        {
            headHitbox =
                hit.GetComponentInParent
                    <EnemyHeadHitbox>();
        }

        if (headHitbox == null)
        {
            headHitbox =
                hit.GetComponentInChildren
                    <EnemyHeadHitbox>(true);
        }

        return headHitbox;
    }

    private Vector3 GetFeetPosition()
    {
        if (feetCheck != null)
        {
            return feetCheck.position;
        }

        if (characterController != null)
        {
            return new Vector3(
                characterController.bounds.center.x,
                characterController.bounds.min.y,
                characterController.bounds.center.z
            );
        }

        return transform.position;
    }

    private void ClearResults(int count)
    {
        for (int i = 0; i < count; i++)
        {
            stompResults[i] = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        CharacterController controller =
            GetComponent<CharacterController>();

        Vector3 feetPosition;

        if (feetCheck != null)
        {
            feetPosition =
                feetCheck.position;
        }
        else if (controller != null)
        {
            feetPosition =
                new Vector3(
                    controller.bounds.center.x,
                    controller.bounds.min.y,
                    controller.bounds.center.z
                );
        }
        else
        {
            feetPosition =
                transform.position;
        }

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            feetPosition,
            stompRadius
        );
    }
}