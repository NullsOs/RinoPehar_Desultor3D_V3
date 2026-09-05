using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.75f;
    [SerializeField] private float blackHoldTime = 0.25f;

    private bool isTransitioning;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup == null)
            CreateFadeCanvas();

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    public void LoadSceneWithTransition(int sceneIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneIndex));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        Time.timeScale = 1f;

        yield return Fade(1f);
        yield return new WaitForSecondsRealtime(blackHoldTime);

        SceneManager.LoadScene(sceneName);

        yield return null;
        yield return null;

        yield return PlayCutsceneIfExists();

        Time.timeScale = 1f;
        isTransitioning = false;
    }

    private IEnumerator TransitionRoutine(int sceneIndex)
    {
        isTransitioning = true;
        Time.timeScale = 1f;

        yield return Fade(1f);
        yield return new WaitForSecondsRealtime(blackHoldTime);

        SceneManager.LoadScene(sceneIndex);

        yield return null;
        yield return null;

        yield return PlayCutsceneIfExists();

        Time.timeScale = 1f;
        isTransitioning = false;
    }

    private IEnumerator PlayCutsceneIfExists()
    {
        LevelCutscenePlayer cutscene = FindAnyObjectByType<LevelCutscenePlayer>();

        if (cutscene != null)
        {
            yield return cutscene.PlayCutsceneWithFade(this);
        }
        else
        {
            yield return Fade(0f);
        }
    }

    public IEnumerator FadeToBlack()
    {
        yield return Fade(1f);
    }

    public IEnumerator FadeFromBlack()
    {
        yield return Fade(0f);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
    }

    private void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("Fade Canvas");
        canvasObj.transform.SetParent(transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("Fade Image");
        imageObj.transform.SetParent(canvasObj.transform, false);

        Image image = imageObj.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rect = image.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeCanvasGroup = imageObj.AddComponent<CanvasGroup>();
    }
}