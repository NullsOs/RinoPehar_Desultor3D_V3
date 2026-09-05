using System.Collections;
using UnityEngine;

public class EnemyIgnorePlayerCollision : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private Rigidbody enemyRigidbody;
    private Coroutine setupRoutine;

    private void Awake()
    {
        enemyRigidbody =
            GetComponentInParent<Rigidbody>();

        if (enemyRigidbody == null)
        {
            Debug.LogError(
                "EnemyIgnorePlayerCollision could not " +
                "find the enemy Rigidbody.",
                this
            );
        }
    }

    private void OnEnable()
    {
        setupRoutine = StartCoroutine(
            IgnorePlayerWhenAvailable()
        );
    }

    private void OnDisable()
    {
        if (setupRoutine == null)
            return;

        StopCoroutine(setupRoutine);
        setupRoutine = null;
    }

    private IEnumerator IgnorePlayerWhenAvailable()
    {
        GameObject player = null;

        while (player == null)
        {
            player =
                GameObject.FindGameObjectWithTag(
                    playerTag
                );

            if (player == null)
                yield return null;
        }

        if (enemyRigidbody == null)
        {
            setupRoutine = null;
            yield break;
        }

        Collider[] enemyColliders =
            enemyRigidbody.GetComponentsInChildren
                <Collider>(true);

        Collider[] playerColliders =
            player.GetComponentsInChildren
                <Collider>(true);

        foreach (Collider enemyCollider
                 in enemyColliders)
        {
            if (enemyCollider == null)
                continue;

            
            if (enemyCollider.isTrigger)
                continue;

            foreach (Collider playerCollider
                     in playerColliders)
            {
                if (playerCollider == null)
                    continue;

                if (playerCollider.isTrigger)
                    continue;

                Physics.IgnoreCollision(
                    enemyCollider,
                    playerCollider,
                    true
                );
            }
        }

        setupRoutine = null;
    }
}