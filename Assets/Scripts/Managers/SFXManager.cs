using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager sfxManager { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("UI")]
    [SerializeField] private AudioClip buttonPressedClip;
    [SerializeField] private AudioClip buttonHoveredClip;

    [Header("Player")]
    [SerializeField] private AudioClip playerJumpClip;
    [SerializeField] private AudioClip playerTakeDamageClip;
    [SerializeField] private AudioClip playerVictoryClip;
    [SerializeField] private AudioClip playerDefeatClip;

    [Header("Enemy")]
    [SerializeField] private AudioClip enemyDeathClip;

    [Header("Gameplay")]
    [SerializeField] private AudioClip enemyProximityClip;
    [SerializeField] private AudioClip itemCollectedClip;

    private void Awake()
    {
        if (sfxManager != null && sfxManager != this)
        {
            Destroy(gameObject);
            return;
        }

        sfxManager = this;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.ignoreListenerPause = true;
    }

    public void PlayButtonPressed() => Play(buttonPressedClip);
    public void PlayButtonHovered() => Play(buttonHoveredClip);
    public void PlayPlayerJump() => Play(playerJumpClip);
    public void PlayPlayerTakeDamage() => Play(playerTakeDamageClip);
    public void PlayPlayerVictory() => Play(playerVictoryClip);
    public void PlayPlayerDefeat() => Play(playerDefeatClip);
    public void PlayEnemyDeath() => Play(enemyDeathClip);
    public void PlayEnemyProximity() => Play(enemyProximityClip);
    public void PlayItemCollected() => Play(itemCollectedClip);

    private void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}