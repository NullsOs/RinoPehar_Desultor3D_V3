using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealingItemBehaviour : MonoBehaviour
{
    [SerializeField] private HealingItemData healingItemData;

    private bool collected;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
        {
            if (UIManager.uiManager != null)
                UIManager.uiManager.ShowMessage("You already have max HP!");

            return;
        }

        collected = true;

        playerHealth.Heal(healingItemData.healAmount);

        if (SFXManager.sfxManager != null)
            SFXManager.sfxManager.PlayItemCollected();

        if (PickUpFlash.pickUpFlash != null)
            PickUpFlash.pickUpFlash.Flash();

        gameObject.SetActive(false);
    }
}