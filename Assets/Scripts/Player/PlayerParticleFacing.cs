using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerParticleFacing : MonoBehaviour
{
    [Header("References")]
    [Tooltip(
        "Assign the Player's rotating visual model."
    )]
    [SerializeField]
    private Transform visualModel;

    [Tooltip(
        "Assign the PlayerParticles container. " +
        "Only this Transform will be rotated."
    )]
    [SerializeField]
    private Transform particleRoot;

    [Header("Rotation")]
    [Tooltip(
        "Copies only Y rotation so particles remain upright."
    )]
    [SerializeField]
    private bool copyOnlyYRotation = true;

    [Tooltip(
        "Use Y = 180 if directional particles emit backward."
    )]
    [SerializeField]
    private Vector3 rotationOffset =
        Vector3.zero;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs;

    private void Awake()
    {
        ValidateReferences();
    }

    private void LateUpdate()
    {
        if (visualModel == null ||
            particleRoot == null)
        {
            return;
        }

        /*
         * Position is deliberately never changed.
         * Player's original scene position remains untouched.
         */
        if (copyOnlyYRotation)
        {
            float modelYRotation =
                visualModel.eulerAngles.y;

            particleRoot.rotation =
                Quaternion.Euler(
                    rotationOffset.x,
                    modelYRotation +
                    rotationOffset.y,
                    rotationOffset.z
                );
        }
        else
        {
            particleRoot.rotation =
                visualModel.rotation *
                Quaternion.Euler(
                    rotationOffset
                );
        }
    }

    private void ValidateReferences()
    {
        if (visualModel == null)
        {
            Debug.LogError(
                "PlayerParticleFacing: Visual Model is not assigned.",
                this
            );
        }

        if (particleRoot == null)
        {
            Debug.LogError(
                "PlayerParticleFacing: Particle Root is not assigned.",
                this
            );
        }

        if (particleRoot == transform &&
            GetComponent<PlayerMovement>() != null)
        {
            Debug.LogError(
                "PlayerParticleFacing: Particle Root is set to " +
                "the Player root. Assign the PlayerParticles " +
                "child instead.",
                this
            );
        }

        if (showDebugLogs &&
            visualModel != null &&
            particleRoot != null)
        {
            Debug.Log(
                $"Particle rotation setup: " +
                $"Model = {visualModel.name}, " +
                $"Particle Root = {particleRoot.name}",
                this
            );
        }
    }

    public void SetVisualModel(
        Transform newVisualModel)
    {
        visualModel =
            newVisualModel;
    }

    public void SetParticleRoot(
        Transform newParticleRoot)
    {
        particleRoot =
            newParticleRoot;
    }

    public void SetRotationOffset(
        Vector3 newRotationOffset)
    {
        rotationOffset =
            newRotationOffset;
    }
}