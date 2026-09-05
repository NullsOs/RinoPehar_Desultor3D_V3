using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraAngleTrigger3D : MonoBehaviour
{
    [Header("Camera Mode")]
    [SerializeField] private CameraMode cameraModeOnEnter = CameraMode.SideView;
    [SerializeField] private CameraMode cameraModeOnExit = CameraMode.ThirdPerson;
    [SerializeField] private bool changeBackOnExit = true;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CameraController3D.cameraController != null)
            CameraController3D.cameraController.SetCameraMode(cameraModeOnEnter);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!changeBackOnExit)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (CameraController3D.cameraController != null)
            CameraController3D.cameraController.SetCameraMode(cameraModeOnExit);
    }
}