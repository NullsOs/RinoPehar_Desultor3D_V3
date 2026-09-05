using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerParticleController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private CharacterController characterController;

    [Header("Movement Particles")]
    [SerializeField]
    private ParticleSystem runningParticles;

    [SerializeField]
    private ParticleSystem jumpParticles;

    [SerializeField]
    private ParticleSystem wallJumpParticles;

    [SerializeField]
    private ParticleSystem landingParticles;

    [SerializeField]
    private ParticleSystem stompParticles;

    [Header("Health Particles")]
    [SerializeField]
    private ParticleSystem damageParticles;

    [SerializeField]
    private ParticleSystem healingParticles;

    [Header("Death")]
    [Tooltip(
        "Use a ParticleSystem prefab. It is spawned outside " +
        "the Player so it remains visible after Player is disabled."
    )]
    [SerializeField]
    private ParticleSystem deathParticlesPrefab;

    [Header("Movement Detection")]
    [Tooltip(
        "Minimum horizontal CharacterController velocity " +
        "required to play running particles."
    )]
    [Min(0f)]
    [SerializeField]
    private float runningVelocityThreshold = 0.25f;

    [Tooltip(
        "Minimum upward velocity required to detect " +
        "a jump, wall jump, or stomp bounce."
    )]
    [Min(0f)]
    [SerializeField]
    private float upwardVelocityThreshold = 0.5f;

    [Tooltip(
        "How far to search for walls when deciding whether " +
        "an airborne jump was a wall jump."
    )]
    [Min(0f)]
    [SerializeField]
    private float wallDetectionDistance = 0.8f;

    [Tooltip(
        "Height of the wall raycast relative to Player."
    )]
    [Min(0f)]
    [SerializeField]
    private float wallDetectionHeight = 0.8f;

    [SerializeField]
    private LayerMask wallMask;

    [Header("Particle Cooldowns")]
    [Min(0f)]
    [SerializeField]
    private float jumpParticleCooldown = 0.1f;

    [Min(0f)]
    [SerializeField]
    private float landingParticleCooldown = 0.1f;

    [Min(0f)]
    [SerializeField]
    private float healthParticleCooldown = 0.05f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs;

    private bool initialized;
    private bool wasGrounded;
    private bool deathParticlesPlayed;

    private float previousVerticalVelocity;
    private float nextJumpParticleTime;
    private float nextLandingParticleTime;
    private float nextHealthParticleTime;

    private int previousHealth;

    private void Awake()
    {
        ResolveReferences();
        StopAllParticles();
    }

    private void Start()
    {
        ResolveReferences();

        if (playerMovement == null ||
            playerHealth == null ||
            characterController == null)
        {
            Debug.LogError(
                "PlayerParticleController is missing one or " +
                "more required Player components.",
                this
            );

            enabled = false;
            return;
        }

        wasGrounded =
            playerMovement.IsGrounded;

        previousVerticalVelocity =
            playerMovement.VerticalVelocity;

        previousHealth =
            playerHealth.CurrentHealth;

        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            return;
        }

        UpdateRunningParticles();
        DetectGroundedStateChanges();
        DetectUpwardMovement();
        DetectHealthChanges();

        wasGrounded =
            playerMovement.IsGrounded;

        previousVerticalVelocity =
            playerMovement.VerticalVelocity;

        previousHealth =
            playerHealth.CurrentHealth;
    }

    private void ResolveReferences()
    {
        if (playerMovement == null)
        {
            playerMovement =
                GetComponent<PlayerMovement>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                GetComponent<PlayerHealth>();
        }

        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }
    }

    private void UpdateRunningParticles()
    {
        if (runningParticles == null ||
            characterController == null ||
            playerMovement == null)
        {
            return;
        }

        Vector3 horizontalVelocity =
            characterController.velocity;

        horizontalVelocity.y = 0f;

        bool shouldPlay =
            playerMovement.IsGrounded &&
            horizontalVelocity.magnitude >=
            runningVelocityThreshold;

        if (shouldPlay)
        {
            if (!runningParticles.isPlaying)
            {
                runningParticles.Play();
            }
        }
        else
        {
            if (runningParticles.isPlaying)
            {
                runningParticles.Stop(
                    true,
                    ParticleSystemStopBehavior
                        .StopEmitting
                );
            }
        }
    }

    private void DetectGroundedStateChanges()
    {
        bool currentlyGrounded =
            playerMovement.IsGrounded;

        bool justLanded =
            !wasGrounded &&
            currentlyGrounded;

        if (!justLanded ||
            Time.time < nextLandingParticleTime)
        {
            return;
        }

        PlayParticle(
            landingParticles
        );

        nextLandingParticleTime =
            Time.time +
            landingParticleCooldown;

        if (showDebugLogs)
        {
            Debug.Log(
                "Player landing particles played.",
                this
            );
        }
    }

    private void DetectUpwardMovement()
    {
        float currentVerticalVelocity =
            playerMovement.VerticalVelocity;

        bool startedMovingUp =
            previousVerticalVelocity <=
            upwardVelocityThreshold &&
            currentVerticalVelocity >
            upwardVelocityThreshold;

        if (!startedMovingUp ||
            Time.time < nextJumpParticleTime)
        {
            return;
        }

       
        if (wasGrounded)
        {
            PlayParticle(
                jumpParticles
            );

            LogParticle(
                "Player jump particles played."
            );
        }
       
        else if (IsNextToWall())
        {
            PlayParticle(
                wallJumpParticles
            );

            LogParticle(
                "Player wall-jump particles played."
            );
        }
       
        else
        {
            PlayParticle(
                stompParticles
            );

            LogParticle(
                "Player stomp particles played."
            );
        }

        nextJumpParticleTime =
            Time.time +
            jumpParticleCooldown;
    }

    private void DetectHealthChanges()
    {
        if (playerHealth == null)
        {
            return;
        }

        int currentHealth =
            playerHealth.CurrentHealth;

        if (currentHealth ==
            previousHealth)
        {
            return;
        }

        if (Time.time <
            nextHealthParticleTime)
        {
            previousHealth =
                currentHealth;

            return;
        }

        if (currentHealth <
            previousHealth)
        {
            PlayParticle(
                damageParticles
            );

            LogParticle(
                "Player damage particles played."
            );
        }
        else
        {
            PlayParticle(
                healingParticles
            );

            LogParticle(
                "Player healing particles played."
            );
        }

        nextHealthParticleTime =
            Time.time +
            healthParticleCooldown;
    }

    private bool IsNextToWall()
    {
        Vector3 origin =
            transform.position +
            Vector3.up *
            wallDetectionHeight;

        Vector3 firstDirection =
            GetHorizontalCameraRight();

        Vector3 secondDirection =
            -firstDirection;

        bool wallOnFirstSide =
            Physics.Raycast(
                origin,
                firstDirection,
                wallDetectionDistance,
                wallMask,
                QueryTriggerInteraction.Ignore
            );

        bool wallOnSecondSide =
            Physics.Raycast(
                origin,
                secondDirection,
                wallDetectionDistance,
                wallMask,
                QueryTriggerInteraction.Ignore
            );

        return wallOnFirstSide ||
               wallOnSecondSide;
    }

    private Vector3 GetHorizontalCameraRight()
    {
        if (Camera.main == null)
        {
            return Vector3.right;
        }

        Vector3 cameraRight =
            Camera.main.transform.right;

        cameraRight.y = 0f;

        if (cameraRight.sqrMagnitude <
            0.001f)
        {
            return Vector3.right;
        }

        return cameraRight.normalized;
    }

    private void PlayParticle(
        ParticleSystem particleSystem)
    {
        if (particleSystem == null)
        {
            return;
        }

        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior
                .StopEmittingAndClear
        );

        particleSystem.Play();
    }

    private void StopAllParticles()
    {
        StopParticleImmediately(
            runningParticles
        );

        StopParticleImmediately(
            jumpParticles
        );

        StopParticleImmediately(
            wallJumpParticles
        );

        StopParticleImmediately(
            landingParticles
        );

        StopParticleImmediately(
            stompParticles
        );

        StopParticleImmediately(
            damageParticles
        );

        StopParticleImmediately(
            healingParticles
        );
    }

    private void StopParticleImmediately(
        ParticleSystem particleSystem)
    {
        if (particleSystem == null)
        {
            return;
        }

        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior
                .StopEmittingAndClear
        );
    }

    private void OnDisable()
    {
        StopAllParticles();

        if (!Application.isPlaying ||
            !initialized ||
            deathParticlesPlayed ||
            playerHealth == null ||
            playerHealth.CurrentHealth > 0)
        {
            return;
        }

        deathParticlesPlayed = true;

        if (deathParticlesPrefab != null)
        {
            ParticleSystem deathParticles =
                Instantiate(
                    deathParticlesPrefab,
                    transform.position,
                    Quaternion.identity
                );

            deathParticles.Play();

            Destroy(
                deathParticles.gameObject,
                GetParticleLifetime(
                    deathParticles
                )
            );
        }
    }

    private float GetParticleLifetime(
        ParticleSystem particleSystem)
    {
        ParticleSystem.MainModule main =
            particleSystem.main;

        float lifetime =
            main.startLifetime.constantMax;

        return main.duration +
               lifetime +
               0.5f;
    }

    private void LogParticle(
        string message)
    {
        if (!showDebugLogs)
        {
            return;
        }

        Debug.Log(
            message,
            this
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin =
            transform.position +
            Vector3.up *
            wallDetectionHeight;

        Vector3 direction =
            Application.isPlaying
                ? GetHorizontalCameraRight()
                : Vector3.right;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            origin,
            origin +
            direction *
            wallDetectionDistance
        );

        Gizmos.DrawLine(
            origin,
            origin -
            direction *
            wallDetectionDistance
        );
    }
}