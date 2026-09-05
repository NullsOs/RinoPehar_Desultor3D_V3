using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    public static DamageFlash damageFlash;

    [SerializeField] private Image flashImage;
    [SerializeField] private float maxAlpha = 0.45f;
    [SerializeField] private float fadeSpeed = 4f;

    private Coroutine flashRoutine;

    private void Awake()
    {
        damageFlash = this;

        if (flashImage == null)
            flashImage = GetComponent<Image>();

        SetAlpha(0f);
    }

    public void Flash()
    {
        if (flashImage == null)
        {
            Debug.LogWarning("DamageFlash missing Image reference.");
            return;
        }

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetAlpha(maxAlpha);

        while (flashImage.color.a > 0f)
        {
            float newAlpha = Mathf.MoveTowards(
                flashImage.color.a,
                0f,
                fadeSpeed * Time.unscaledDeltaTime
            );

            SetAlpha(newAlpha);

            yield return null;
        }

        flashRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (flashImage == null) return;

        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }
}