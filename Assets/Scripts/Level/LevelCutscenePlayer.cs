using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class LevelCutscenePlayer : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector playableDirector;

    [Header("Cameras")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Camera cutsceneCamera;

    [Header("Player")]
    [SerializeField] private GameObject playerObject;

    [Header("Options")]
    [SerializeField] private bool playOnlyOnce = true;
    [SerializeField] private float fallbackDuration = 3f;

    private bool played;

    private PlayerMovement playerMovement;
    private PlayerStompDetector playerStompDetector;
    private PlayerDamageReceiver playerDamageReceiver;
    private CameraController3D cameraController;

    private AudioListener gameplayListener;
    private AudioListener cutsceneListener;

    private void Awake()
    {
        if (playableDirector == null)
            playableDirector = GetComponent<PlayableDirector>();

        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerMovement = playerObject.GetComponent<PlayerMovement>();
            playerStompDetector = playerObject.GetComponent<PlayerStompDetector>();
            playerDamageReceiver = playerObject.GetComponent<PlayerDamageReceiver>();
        }

        if (gameplayCamera != null)
        {
            cameraController = gameplayCamera.GetComponent<CameraController3D>();
            gameplayListener = gameplayCamera.GetComponent<AudioListener>();
        }

        if (cutsceneCamera != null)
        {
            cutsceneListener = cutsceneCamera.GetComponent<AudioListener>();

            cutsceneCamera.gameObject.SetActive(true);
            cutsceneCamera.enabled = false;

            if (cutsceneListener != null)
                cutsceneListener.enabled = false;
        }

        if (playableDirector != null)
        {
            playableDirector.playOnAwake = false;
            playableDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            playableDirector.extrapolationMode = DirectorWrapMode.None;
            playableDirector.Stop();
            playableDirector.time = 0;
        }
    }

    public IEnumerator PlayCutsceneWithFade(SceneTransitionManager transition)
    {
        if (playOnlyOnce && played)
        {
            yield return transition.FadeFromBlack();
            yield break;
        }

        played = true;

        if (playableDirector == null || playableDirector.playableAsset == null || cutsceneCamera == null)
        {
            yield return transition.FadeFromBlack();
            yield break;
        }

        Time.timeScale = 0f;

        SetGameplayEnabled(false);

        SwitchToCutsceneCamera();

        yield return null;

        playableDirector.Stop();
        playableDirector.time = 0;
        playableDirector.Evaluate();

        yield return transition.FadeFromBlack();

        double duration = playableDirector.duration;

        if (duration <= 0.05f)
            duration = fallbackDuration;

        playableDirector.Play();

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        playableDirector.Stop();
        playableDirector.time = 0;

        yield return transition.FadeToBlack();

        SwitchToGameplayCamera();

        Time.timeScale = 1f;

        SetGameplayEnabled(true);

        yield return null;

        yield return transition.FadeFromBlack();
    }

    private void SwitchToCutsceneCamera()
    {
        if (gameplayCamera != null)
            gameplayCamera.enabled = false;

        if (gameplayListener != null)
            gameplayListener.enabled = false;

        if (cutsceneCamera != null)
            cutsceneCamera.enabled = true;

        if (cutsceneListener != null)
            cutsceneListener.enabled = true;
    }

    private void SwitchToGameplayCamera()
    {
        if (cutsceneCamera != null)
            cutsceneCamera.enabled = false;

        if (cutsceneListener != null)
            cutsceneListener.enabled = false;

        if (gameplayCamera != null)
            gameplayCamera.enabled = true;

        if (gameplayListener != null)
            gameplayListener.enabled = true;
    }

    private void SetGameplayEnabled(bool enabled)
    {
        if (playerMovement != null)
            playerMovement.enabled = enabled;

        if (playerStompDetector != null)
            playerStompDetector.enabled = enabled;

        if (playerDamageReceiver != null)
            playerDamageReceiver.enabled = enabled;

        if (cameraController != null)
            cameraController.enabled = enabled;
    }
}