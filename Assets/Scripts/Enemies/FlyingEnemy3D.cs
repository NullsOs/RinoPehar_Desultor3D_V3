using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FlyingEnemy3D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveDistance = 5f;

    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Visual")]
    [SerializeField] private Transform visualModel;
    [SerializeField] private float rotationSpeed = 10f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 targetPosition;

    private float nextDamageTime;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (visualModel == null)
            visualModel = transform;
    }

    private void Start()
    {
        moveDirection = moveDirection.normalized;

        if (moveDirection == Vector3.zero)
            moveDirection = Vector3.right;

        startPosition = transform.position;
        endPosition = startPosition + moveDirection * moveDistance;
        targetPosition = endPosition;
    }

    private void Update()
    {
        MoveEnemy();
        RotateVisual();
    }

    private void MoveEnemy()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) <= 0.05f)
        {
            targetPosition = targetPosition == endPosition
                ? startPosition
                : endPosition;
        }
    }

    private void RotateVisual()
    {
        Vector3 directionToLook = targetPosition - transform.position;
        directionToLook.y = 0f;

        if (directionToLook.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(directionToLook.normalized);

        visualModel.rotation = Quaternion.Slerp(
            visualModel.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerDamageReceiver damageReceiver = other.GetComponent<PlayerDamageReceiver>();

        if (damageReceiver == null)
            return;

        damageReceiver.TakeDamage(damageAmount);
        nextDamageTime = Time.time + damageCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 direction = moveDirection.normalized;

        if (direction == Vector3.zero)
            direction = Vector3.right;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + direction * moveDistance);
        Gizmos.DrawWireSphere(transform.position + direction * moveDistance, 0.2f);
    }
}