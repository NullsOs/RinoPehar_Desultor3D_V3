using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;

    private void Start()
    {
        if (MusicManager.musicManager != null)
        {
            MusicManager.musicManager.PlayMusic(sceneMusic);
        }
    }
}