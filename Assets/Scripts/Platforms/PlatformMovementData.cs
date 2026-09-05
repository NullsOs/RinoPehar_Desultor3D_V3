using UnityEngine;

public enum PlatformMoveDirection3D
{
    XAxis,
    YAxis,
    ZAxis
}

[CreateAssetMenu(
    fileName = "PlatformMovementData",
    menuName = "Platform/Platform Movement Data"
)]
public class PlatformMovementData : ScriptableObject
{
    [Min(0f)]
    public float moveSpeed = 2f;

    [Min(0f)]
    public float moveDistance = 5f;

    public PlatformMoveDirection3D moveDirection =
        PlatformMoveDirection3D.XAxis;

    private void OnValidate()
    {
        moveSpeed =
            Mathf.Max(
                0f,
                moveSpeed
            );

        moveDistance =
            Mathf.Max(
                0f,
                moveDistance
            );
    }
}