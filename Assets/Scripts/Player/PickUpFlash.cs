using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PickUpFlash : MonoBehaviour
{
    public static PickUpFlash pickUpFlash { get; private set; }

    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float maxAlpha = 0.25f;

    [Header("Pickup Color")]
    [SerializeField] private Color flashColor = Color.green;

    private Coroutine flashRoutine;

    private void Awake()
    {
        if (pickUpFlash == null)
            pickUpFlash = this;

        Color color = flashImage.color;
        color.a = 0f;
        flashImage.color = color;
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Color color = flashColor;
        color.a = maxAlpha;

        flashImage.color = color;

        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.unscaledDeltaTime;

            color.a = Mathf.Lerp(maxAlpha, 0f, timer / flashDuration);
            flashImage.color = color;

            yield return null;
        }

        color.a = 0f;
        flashImage.color = color;
    }
}