using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemBehaviour : MonoBehaviour
{
    [SerializeField] private Item item;

    [Header("Save")]
    [SerializeField] private string pickupSaveId;

    private bool collected;

    public bool CountsForWin(string requiredItemId)
    {
        return item != null &&
               item.itemId == requiredItemId;
    }

    public bool WasPreviouslyCollected()
    {
        return
            !string.IsNullOrEmpty(pickupSaveId) &&
            SaveManager.IsItemCollected(pickupSaveId);
    }

    private void Awake()
    {
        Collider itemCollider = GetComponent<Collider>();
        itemCollider.isTrigger = true;

        CreateSaveIdIfMissing();
    }

    private void Start()
    {
        collected = false;

        if (SaveManager.IsLoadingSave() &&
            SaveManager.IsItemCollected(pickupSaveId))
        {
            collected = true;
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
    }

    private void CreateSaveIdIfMissing()
    {
        if (!string.IsNullOrEmpty(pickupSaveId))
            return;

        pickupSaveId =
            gameObject.scene.name + "_" +
            Mathf.RoundToInt(transform.position.x * 100f) + "_" +
            Mathf.RoundToInt(transform.position.y * 100f) + "_" +
            Mathf.RoundToInt(transform.position.z * 100f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        PlayerDamageReceiver player =
            other.GetComponentInParent<PlayerDamageReceiver>();

        if (player == null)
            return;

        collected = true;

        if (GameManager.gameManager != null)
        {
            GameManager.gameManager.AddItem(
                item,
                pickupSaveId
            );
        }

        SaveManager.SaveCollectedItem(pickupSaveId);
        SaveManager.SaveGame();

        if (PickUpFlash.pickUpFlash != null)
            PickUpFlash.pickUpFlash.Flash();

        if (SFXManager.sfxManager != null)
            SFXManager.sfxManager.PlayItemCollected();

        gameObject.SetActive(false);
    }
}