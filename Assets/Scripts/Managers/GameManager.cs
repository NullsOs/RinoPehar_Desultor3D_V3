using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager
    {
        get;
        private set;
    }

    [Header("Win Condition")]
    [SerializeField]
    private string requiredItemId = "Sword";

    [Tooltip(
        "When enabled, the required amount is calculated from " +
        "all matching ItemBehaviour objects in the scene."
    )]
    [SerializeField]
    private bool countRequiredItemsInScene = true;

    [Min(1)]
    [SerializeField]
    private int maxItemToWin = 1;

    private readonly HashSet<string> collectedPickupIds =
        new HashSet<string>();

    private int currentRequiredItems;
    private bool victoryTriggered;

    public int MaxItemsToWin => maxItemToWin;
    public int CurrentRequiredItems => currentRequiredItems;

    private void Awake()
    {
        if (gameManager != null &&
            gameManager != this)
        {
            Destroy(gameObject);
            return;
        }

        gameManager = this;
    }

    private void Start()
    {
        InitializeItemProgress();
        RefreshUI();
    }

    private void InitializeItemProgress()
    {
        currentRequiredItems = 0;
        victoryTriggered = false;
        collectedPickupIds.Clear();

        if (!countRequiredItemsInScene)
        {
            maxItemToWin = Mathf.Max(
                1,
                maxItemToWin
            );

            return;
        }

        ItemBehaviour[] pickups =
            FindObjectsByType<ItemBehaviour>(
                FindObjectsInactive.Include
            );

        int requiredInScene = 0;
        int previouslyCollected = 0;

        foreach (ItemBehaviour pickup in pickups)
        {
            if (pickup == null)
            {
                continue;
            }

            if (!pickup.CountsForWin(requiredItemId))
            {
                continue;
            }

            requiredInScene++;

            if (SaveManager.IsLoadingSave() &&
                pickup.WasPreviouslyCollected())
            {
                previouslyCollected++;
            }
        }

        if (requiredInScene > 0)
        {
            maxItemToWin = requiredInScene;
        }
        else
        {
            maxItemToWin = 1;

            Debug.LogWarning(
                "No collectibles with itemId '" +
                requiredItemId +
                "' were found in this scene."
            );
        }

        currentRequiredItems = Mathf.Clamp(
            previouslyCollected,
            0,
            maxItemToWin
        );
    }

    public void AddItem(
        Item item,
        string pickupSaveId)
    {
        if (victoryTriggered)
        {
            return;
        }

        if (item == null)
        {
            Debug.LogWarning(
                "Picked-up object has no Item " +
                "ScriptableObject assigned."
            );

            return;
        }

        if (!string.IsNullOrEmpty(pickupSaveId))
        {
            bool successfullyAdded =
                collectedPickupIds.Add(
                    pickupSaveId
                );

            if (!successfullyAdded)
            {
                return;
            }
        }

        if (item.itemId != requiredItemId)
        {
            return;
        }

        currentRequiredItems = Mathf.Min(
            currentRequiredItems + 1,
            maxItemToWin
        );

        RefreshUI();

        if (currentRequiredItems >= maxItemToWin)
        {
            WinGame();
        }
    }

    private void RefreshUI()
    {
        if (UIManager.uiManager != null)
        {
            UIManager.uiManager.UpdateText(
                currentRequiredItems,
                maxItemToWin
            );
        }
    }

    private void WinGame()
    {
        if (victoryTriggered)
        {
            return;
        }

        victoryTriggered = true;

        if (SFXManager.sfxManager != null)
        {
            SFXManager.sfxManager
                .PlayPlayerVictory();
        }

        if (UIManager.uiManager != null)
        {
            UIManager.uiManager
                .ShowVictoryPanel();
        }

        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        if (gameManager == this)
        {
            gameManager = null;
        }
    }
}