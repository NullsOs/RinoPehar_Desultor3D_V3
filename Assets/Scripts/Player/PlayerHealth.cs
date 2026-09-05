using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    private void Start()
    {
        if (UIManager.uiManager != null)
            UIManager.uiManager.SetHealthIcons(currentHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (UIManager.uiManager != null)
            UIManager.uiManager.SetHealthIcons(currentHealth);

        PlayDamageFeedback();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        SaveManager.SaveGame();
    }

    public void Heal(int healAmount)
    {
        if (isDead)
            return;

        if (currentHealth >= maxHealth)
            return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (UIManager.uiManager != null)
            UIManager.uiManager.SetHealthIcons(currentHealth);

        SaveManager.SaveGame();
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 1, maxHealth);
        isDead = false;

        if (UIManager.uiManager != null)
            UIManager.uiManager.SetHealthIcons(currentHealth);

        gameObject.SetActive(true);
    }

    private void PlayDamageFeedback()
    {
        if (SFXManager.sfxManager != null)
            SFXManager.sfxManager.PlayPlayerTakeDamage();

        if (DamageFlash.damageFlash != null)
            DamageFlash.damageFlash.Flash();

        if (CameraShake.cameraShake != null)
            CameraShake.cameraShake.ShakeCam();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (SFXManager.sfxManager != null)
            SFXManager.sfxManager.PlayPlayerDefeat();

        gameObject.SetActive(false);

        if (UIManager.uiManager != null)
            UIManager.uiManager.ShowLoosePanel();

        Time.timeScale = 0f;
    }
}