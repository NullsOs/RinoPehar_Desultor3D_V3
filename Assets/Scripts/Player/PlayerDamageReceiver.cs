using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth =
                GetComponent<PlayerHealth>();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (playerHealth == null ||
            damageAmount <= 0)
        {
            return;
        }

        playerHealth.TakeDamage(
            damageAmount
        );
    }
}