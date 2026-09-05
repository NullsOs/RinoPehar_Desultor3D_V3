using UnityEngine;

public class EnemyProximitySFX : MonoBehaviour
{
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private string playerTag = "Player";

    private Transform player;
    private float timer;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange && timer <= 0f)
        {
            if (SFXManager.sfxManager != null)
                SFXManager.sfxManager.PlayEnemyProximity();

            timer = cooldown;
        }
    }
}
