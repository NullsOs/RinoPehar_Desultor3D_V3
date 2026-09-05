using UnityEngine;

public class CannonEnemySpawner3D : MonoBehaviour
{
    [Header("Projectile Enemy")]
    [SerializeField] private CannonProjectileEnemy3D projectilePrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 shootDirection = Vector3.forward;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private int damageAmount = 1;

    private float nextSpawnTime;

    private void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnProjectileEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnProjectileEnemy()
    {
        if (projectilePrefab == null)
            return;

        Vector3 direction = shootDirection.normalized;

        if (direction == Vector3.zero)
            direction = transform.forward;

        CannonProjectileEnemy3D projectile = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.LookRotation(direction)
        );

        projectile.Initialize(direction, projectileSpeed, projectileLifetime, damageAmount);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position;

        Vector3 direction = shootDirection.normalized;

        if (direction == Vector3.zero)
            direction = transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + direction * 3f);
        Gizmos.DrawWireSphere(origin, 0.2f);
    }
}