using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake cameraShake;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeStrength = 0.08f;

    [Header("Tilt")]
    [SerializeField] private float tiltDuration = 0.18f;
    [SerializeField] private float tiltStrength = 4f;

    private Coroutine shakeRoutine;
    private Coroutine tiltRoutine;

    private void Awake()
    {
        cameraShake = this;
    }

    public void ShakeCam()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        if (tiltRoutine != null)
            StopCoroutine(tiltRoutine);

        shakeRoutine = StartCoroutine(Shake());
        tiltRoutine = StartCoroutine(Tilt());
    }

    private IEnumerator Shake()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            float progress = timer / shakeDuration;
            float strength = Mathf.Lerp(shakeStrength, 0f, progress);

            Vector3 randomOffset = Random.insideUnitSphere * strength;
            randomOffset.z = 0f;

            if (CameraController3D.cameraController != null)
                CameraController3D.cameraController.SetShakeOffset(randomOffset);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (CameraController3D.cameraController != null)
            CameraController3D.cameraController.SetShakeOffset(Vector3.zero);

        shakeRoutine = null;
    }

    private IEnumerator Tilt()
    {
        float timer = 0f;
        float randomDirection = Random.value > 0.5f ? 1f : -1f;

        while (timer < tiltDuration)
        {
            float progress = timer / tiltDuration;
            float tiltAmount = Mathf.Lerp(tiltStrength * randomDirection, 0f, progress);

            Vector3 tilt = new Vector3(0f, 0f, tiltAmount);

            if (CameraController3D.cameraController != null)
                CameraController3D.cameraController.SetTiltOffset(tilt);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (CameraController3D.cameraController != null)
            CameraController3D.cameraController.SetTiltOffset(Vector3.zero);

        tiltRoutine = null;
    }
}