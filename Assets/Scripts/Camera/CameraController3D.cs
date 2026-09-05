using UnityEngine;

public class CameraController3D : MonoBehaviour
{
    public static CameraController3D cameraController;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Third Person")]
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 3f, -7f);
    [SerializeField] private Vector3 thirdPersonRotation = new Vector3(20f, 0f, 0f);

    [Header("Side View")]
    [SerializeField] private Vector3 sideViewOffset = new Vector3(0f, 2.5f, -8f);
    [SerializeField] private Vector3 sideViewRotation = new Vector3(15f, 0f, 0f);

    [Header("Top Down")]
    [SerializeField] private Vector3 topDownOffset = new Vector3(0f, 12f, 0f);
    [SerializeField] private Vector3 topDownRotation = new Vector3(90f, 0f, 0f);

    private CameraMode currentMode = CameraMode.ThirdPerson;

    private Vector3 shakeOffset;
    private Vector3 tiltOffset;

    public CameraMode CurrentMode => currentMode;

    private void Awake()
    {
        cameraController = this;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player != null)
        {
            transform.position = player.position + GetOffset();
            transform.rotation = Quaternion.Euler(GetRotation());
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + GetOffset() + shakeOffset;
        Quaternion targetRotation = Quaternion.Euler(GetRotation() + tiltOffset);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.unscaledDeltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.unscaledDeltaTime
        );
    }

    public void SetCameraMode(CameraMode newMode)
    {
        currentMode = newMode;
    }

    public void SetThirdPerson()
    {
        currentMode = CameraMode.ThirdPerson;
    }

    public void SetSideView()
    {
        currentMode = CameraMode.SideView;
    }

    public void SetTopDown()
    {
        currentMode = CameraMode.TopDown;
    }

    public void SetShakeOffset(Vector3 newShakeOffset)
    {
        shakeOffset = newShakeOffset;
    }

    public void SetTiltOffset(Vector3 newTiltOffset)
    {
        tiltOffset = newTiltOffset;
    }

    private Vector3 GetOffset()
    {
        switch (currentMode)
        {
            case CameraMode.SideView:
                return sideViewOffset;

            case CameraMode.TopDown:
                return topDownOffset;

            default:
                return thirdPersonOffset;
        }
    }

    private Vector3 GetRotation()
    {
        switch (currentMode)
        {
            case CameraMode.SideView:
                return sideViewRotation;

            case CameraMode.TopDown:
                return topDownRotation;

            default:
                return thirdPersonRotation;
        }
    }
}