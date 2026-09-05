using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager musicManager { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (musicManager != null && musicManager != this)
        {
            Destroy(gameObject);
            return;
        }

        musicManager = this;
        

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource == null) return;

        if (musicSource.clip == musicClip && musicSource.isPlaying)
            return;

        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
    }
}