using UnityEngine;


[RequireComponent(typeof(Collider))]
public class EnemyAttackTrigger : MonoBehaviour
{
    private void Awake()
    {
        Collider triggerCollider =
            GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}