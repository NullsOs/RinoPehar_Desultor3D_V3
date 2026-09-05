using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager uiManager { get; private set; }

    [Header("Gameplay UI")]
    [SerializeField] private List<Image> healthIcons = new List<Image>();
    [SerializeField] private TextMeshProUGUI itemCounter;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI enemyCounterText;

    [Header("Message Text")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageTime = 1.5f;

    [Header("Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject loosePanel;
    [SerializeField] private GameObject continuePanel;

    private bool victoryActive;
    private Coroutine messageRoutine;

    private void Awake()
    {
        if (uiManager != null && uiManager != this)
        {
            Destroy(gameObject);
            return;
        }

        uiManager = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        HideAllPanels();
        ShowGameplayUI();

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        if (GameManager.gameManager != null)
            UpdateText(GameManager.gameManager.CurrentRequiredItems, GameManager.gameManager.MaxItemsToWin);
        else
            UpdateText(0, 0);
    }

    public void UpdateText(int currentItems, int maxItems)
    {
        if (itemCounter != null)
            itemCounter.text = currentItems + " / " + maxItems;
    }

    public void UpdateEnemyCounter(int killedEnemies)
    {
        if (enemyCounterText != null)
            enemyCounterText.text = "Enemies killed: " + killedEnemies;
    }

    public void SetHealthIcons(int currentHealth)
    {
        for (int i = 0; i < healthIcons.Count; i++)
        {
            if (healthIcons[i] != null)
                healthIcons[i].gameObject.SetActive(i < currentHealth);
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(MessageRoutine(message));
    }

    private IEnumerator MessageRoutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(messageTime);

        messageText.gameObject.SetActive(false);
        messageRoutine = null;
    }

    private void HideGameplayUI()
    {
        foreach (Image heart in healthIcons)
        {
            if (heart != null)
                heart.gameObject.SetActive(false);
        }

        if (itemCounter != null)
            itemCounter.gameObject.SetActive(false);

        if (itemIcon != null)
            itemIcon.gameObject.SetActive(false);

        if (enemyCounterText != null)
            enemyCounterText.gameObject.SetActive(false);

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    private void ShowGameplayUI()
    {
        foreach (Image heart in healthIcons)
        {
            if (heart != null)
                heart.gameObject.SetActive(true);
        }

        if (itemCounter != null)
            itemCounter.gameObject.SetActive(true);

        if (itemIcon != null)
            itemIcon.gameObject.SetActive(true);

        if (enemyCounterText != null)
            enemyCounterText.gameObject.SetActive(true);
    }

    public void ShowVictoryPanel()
    {
        victoryActive = true;

        HideGameplayUI();
        HideAllPanels();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ShowLoosePanel()
    {
        if (victoryActive)
            return;

        HideGameplayUI();
        HideAllPanels();

        if (loosePanel != null)
            loosePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ShowContinuePanel()
    {
        if (victoryActive)
            return;

        HideAllPanels();

        if (continuePanel != null)
            continuePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        HideAllPanels();
        ShowGameplayUI();

        Time.timeScale = 1f;
    }

    public void HideAllPanels()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (loosePanel != null)
            loosePanel.SetActive(false);

        if (continuePanel != null)
            continuePanel.SetActive(false);
    }
}