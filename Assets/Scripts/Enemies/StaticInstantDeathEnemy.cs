using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StaticInstantDeathEnemy3D : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            if (debugLogs)
                Debug.LogWarning("[StaticInstantDeathEnemy3D] PlayerHealth not found.");

            return;
        }

        if (debugLogs)
            Debug.Log("[StaticInstantDeathEnemy3D] Player touched instant death enemy.");

        playerHealth.TakeDamage(playerHealth.CurrentHealth);
    }
}