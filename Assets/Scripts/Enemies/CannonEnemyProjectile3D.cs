using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CannonProjectileEnemy3D : MonoBehaviour
{
    private Vector3 moveDirection;
    private float moveSpeed;
    private float lifetime;
    private int damageAmount;

    private bool initialized;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void Initialize(
        Vector3 direction,
        float speed,
        float lifeTime,
        int damage
    )
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifetime = lifeTime;
        damageAmount = damage;

        if (moveDirection == Vector3.zero)
            moveDirection = transform.forward;

        initialized = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized)
            return;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerDamageReceiver damageReceiver = other.GetComponent<PlayerDamageReceiver>();

        if (damageReceiver != null)
            damageReceiver.TakeDamage(damageAmount);

        Destroy(gameObject);
    }
}