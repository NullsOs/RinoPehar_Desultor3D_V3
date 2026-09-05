using UnityEngine;

public class EnemyKillManager : MonoBehaviour
{
    public static EnemyKillManager enemyKillManager
    {
        get;
        private set;
    }

    private int killedEnemies;

    public int KilledEnemies =>
        killedEnemies;

    private void Awake()
    {
        if (enemyKillManager != null &&
            enemyKillManager != this)
        {
            Destroy(gameObject);
            return;
        }

        enemyKillManager = this;
    }

    private void Start()
    {
        killedEnemies =
            SaveManager.IsLoadingSave()
                ? SaveManager.GetKilledEnemyCount()
                : 0;

        RefreshUI();
    }

    public void AddEnemyKill()
    {
        killedEnemies++;

        SaveManager.SaveKilledEnemyCount(
            killedEnemies
        );

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (UIManager.uiManager != null)
        {
            UIManager.uiManager
                .UpdateEnemyCounter(
                    killedEnemies
                );
        }
    }

    private void OnDestroy()
    {
        if (enemyKillManager == this)
        {
            enemyKillManager = null;
        }
    }
}