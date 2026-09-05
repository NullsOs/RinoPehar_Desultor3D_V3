using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayButtonSound);
    }

    private void PlayButtonSound()
    {
        Debug.Log("Button clicked: " + gameObject.name);

        if (SFXManager.sfxManager == null)
        {
            Debug.LogError("SFXManager is missing.");
            return;
        }

        SFXManager.sfxManager.PlayButtonPressed();
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayButtonSound);
    }
}