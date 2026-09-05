using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyHeadHitbox : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private EnemyDeath enemyDeath;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = true;

    private Collider headCollider;

    private void Awake()
    {
        headCollider =
            GetComponent<Collider>();

        headCollider.isTrigger = true;

        if (enemyDeath == null)
        {
            enemyDeath =
                GetComponentInParent<EnemyDeath>();
        }

        if (enemyDeath == null)
        {
            Debug.LogError(
                $"[{name}] EnemyDeath was not found. " +
                "Assign it in the Inspector or place this " +
                "object under the enemy root.",
                this
            );
        }
    }

    private void OnTriggerEnter(
        Collider other)
    {
        TryStompFromCollider(other);
    }

    private void OnTriggerStay(
        Collider other)
    {
        TryStompFromCollider(other);
    }

    private void TryStompFromCollider(
        Collider other)
    {
        if (enemyDeath == null ||
            enemyDeath.IsDead)
        {
            return;
        }

        PlayerMovement playerMovement =
            other.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            playerMovement =
                other.GetComponentInParent
                    <PlayerMovement>();
        }

        if (playerMovement == null)
        {
            playerMovement =
                other.GetComponentInChildren
                    <PlayerMovement>(true);
        }

        if (playerMovement == null)
        {
            return;
        }

        if (!playerMovement.IsFalling)
        {
            if (showDebugLogs)
            {
                Debug.Log(
                    $"[{name}] Player entered the head " +
                    "trigger but was not falling.",
                    this
                );
            }

            return;
        }

        TryKillEnemy(playerMovement);
    }

    public bool TryKillEnemy(
        PlayerMovement playerMovement)
    {
        if (enemyDeath == null)
        {
            Debug.LogError(
                $"[{name}] Stomp failed because " +
                "EnemyDeath is not assigned.",
                this
            );

            return false;
        }

        if (enemyDeath.IsDead)
        {
            return false;
        }

        if (playerMovement == null)
        {
            Debug.LogWarning(
                $"[{name}] Stomp failed because " +
                "PlayerMovement is null.",
                this
            );

            return false;
        }

        if (!playerMovement.IsFalling)
        {
            if (showDebugLogs)
            {
                Debug.Log(
                    $"[{name}] Stomp rejected because " +
                    "the player is not falling.",
                    this
                );
            }

            return false;
        }

        if (showDebugLogs)
        {
            Debug.Log(
                $"[{name}] Valid stomp. Killing enemy.",
                this
            );
        }

        enemyDeath.KillEnemy(
            playerMovement
        );

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Collider currentCollider =
            GetComponent<Collider>();

        if (currentCollider == null)
        {
            return;
        }

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            currentCollider.bounds.center,
            currentCollider.bounds.size
        );
    }
}