using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [Header("Enemy References")]
    [SerializeField]
    private EnemyFSM3D enemyFSM;

    [SerializeField]
    private EnemyData3D enemyData;

    [Tooltip(
        "The complete enemy object that should disappear. " +
        "Normally this is the object containing EnemyFSM3D."
    )]
    [SerializeField]
    private GameObject enemyRoot;

    [Header("Save")]
    [Tooltip(
        "Every enemy instance must have a unique save ID."
    )]
    [SerializeField]
    private string enemySaveId;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = true;

    private bool dead;

    public bool IsDead => dead;

    private void Awake()
    {
        ResolveReferences();

        if (string.IsNullOrWhiteSpace(enemySaveId))
        {
            Debug.LogError(
                $"{name} has no Enemy Save Id. " +
                "Every enemy needs a unique ID.",
                this
            );
        }

        if (enemyFSM == null)
        {
            Debug.LogError(
                $"{name}: EnemyFSM3D could not be found.",
                this
            );
        }

        if (enemyData == null)
        {
            Debug.LogError(
                $"{name}: EnemyData3D could not be found.",
                this
            );
        }

        if (enemyRoot == null)
        {
            Debug.LogError(
                $"{name}: Enemy Root could not be resolved.",
                this
            );
        }
    }

    private void ResolveReferences()
    {
        if (enemyFSM == null)
        {
            enemyFSM =
                GetComponent<EnemyFSM3D>();
        }

        if (enemyFSM == null)
        {
            enemyFSM =
                GetComponentInParent<EnemyFSM3D>();
        }

        if (enemyData == null &&
            enemyFSM != null)
        {
            enemyData =
                enemyFSM.Data;
        }

        
        if (enemyRoot == null &&
            enemyFSM != null)
        {
            enemyRoot =
                enemyFSM.gameObject;
        }


        if (enemyRoot == null)
        {
            enemyRoot =
                gameObject;
        }
    }

    private void Start()
    {
        if (SaveManager.IsLoadingSave() &&
            SaveManager.IsEnemyKilled(enemySaveId))
        {
            dead = true;

            if (enemyRoot != null)
            {
                enemyRoot.SetActive(false);
            }
        }
    }

    public void KillEnemy(
        PlayerMovement playerMovement)
    {
        if (dead)
        {
            return;
        }

        dead = true;

        ResolveReferences();

        if (showDebugLogs)
        {
            Debug.Log(
                $"Enemy killed: {name}. " +
                $"Disabling root: " +
                $"{(enemyRoot != null ? enemyRoot.name : "NULL")}",
                this
            );
        }

        if (enemyFSM != null)
        {
            enemyFSM.MarkAsDead();
        }

        if (playerMovement != null &&
            enemyData != null)
        {
            playerMovement.Bounce(
                enemyData.playerBounceHeight
            );
        }

        if (SFXManager.sfxManager != null)
        {
            SFXManager.sfxManager
                .PlayEnemyDeath();
        }

        if (!string.IsNullOrWhiteSpace(enemySaveId))
        {
            SaveManager.SaveKilledEnemy(
                enemySaveId
            );
        }

        if (EnemyKillManager.enemyKillManager != null)
        {
            EnemyKillManager.enemyKillManager
                .AddEnemyKill();
        }
        else
        {
            Debug.LogWarning(
                "EnemyKillManager was not found.",
                this
            );
        }

       
        TryDropHealingItem();

        SaveManager.SaveGame();

        if (enemyRoot != null)
        {
            enemyRoot.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "Enemy could not disappear because " +
                "Enemy Root is null.",
                this
            );
        }
    }

    private void TryDropHealingItem()
    {
        if (enemyData == null ||
            enemyData.healingItemPrefab == null)
        {
            return;
        }

        if (Random.value >
            enemyData.healingDropChance)
        {
            return;
        }

        Vector2 randomOffset =
            Random.insideUnitCircle *
            enemyData.dropRadius;

        Transform dropOrigin =
            enemyRoot != null
                ? enemyRoot.transform
                : transform;

        Vector3 spawnPosition =
            dropOrigin.position +
            new Vector3(
                randomOffset.x,
                enemyData.dropHeight,
                randomOffset.y
            );

        Instantiate(
            enemyData.healingItemPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}