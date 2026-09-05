using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float scaleSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            scaleSpeed * Time.unscaledDeltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SFXManager.sfxManager != null)
            SFXManager.sfxManager.PlayButtonHovered();

        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
