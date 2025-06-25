using UnityEngine;

/// <summary>
/// Controls the player's spaceship, handling movement, energy consumption, health,
/// collision damage, and fuel collection. Interacts with the GameManager and AudioManager.
/// </summary>
public class SpaceshipController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Base speed for forward and backward thrust.")]
    [SerializeField] private float moveSpeed = 15f;

    [Tooltip("Speed for yaw (left/right) rotation, in degrees per second.")]
    [SerializeField] private float turnSpeed = 100f;

    [Tooltip("Modifier for horizontal strafe speed, relative to forward moveSpeed. (e.g., 0.85 means 85% of moveSpeed).")]
    [SerializeField] private float strafeSpeedModifier = 0.85f;

    [Tooltip("Modifier for vertical strafe speed, relative to forward moveSpeed.")]
    [SerializeField] private float verticalStrafeSpeedModifier = 0.75f;

    [Header("Energy & Health")]
    [Tooltip("Maximum energy capacity of the spaceship.")]
    public float maxEnergy = 100f;

    /// <summary>
    /// Current energy level of the spaceship. Public for GameManager to access.
    /// </summary>
    public float currentEnergy;

    [Tooltip("Rate at which energy is consumed per second when basic movement inputs (thrust, strafe) are active.")]
    [SerializeField] private float energyConsumptionRate = 2.5f;

    [Tooltip("Additional flat energy cost applied per FixedUpdate frame when turning inputs are active.")]
    [SerializeField] private float turnEnergyCost = 0.05f;

    [Tooltip("Maximum health (hit points) of the spaceship.")]
    public float maxHealth = 100f;

    /// <summary>
    /// Current health level of the spaceship. Public for GameManager to access.
    /// </summary>
    public float currentHealth;

    [Header("Collision Damage")]
    [Tooltip("Multiplier to convert impact magnitude (force) into health damage.")]
    [SerializeField] private float collisionDamageMultiplier = 0.5f;

    [Tooltip("Minimum impact magnitude required to inflict damage upon collision.")]
    [SerializeField] private float minImpactForceForDamage = 5f;

    [Tooltip("Threshold (as a percentage of maxEnergy, 0.0 to 1.0) below which the low energy warning sound will activate.")]
    [SerializeField] private float lowEnergyThresholdPercentage = 0.25f;


    /// <summary>
    /// Cached reference to the FuelCellSpawner in the scene.
    /// </summary>
    private FuelCellSpawner fuelCellSpawner;

    /// <summary>
    /// Cached reference to the spaceship's Rigidbody component.
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// Velocity of the Rigidbody in the previous physics frame, used to calculate impact force.
    /// </summary>
    private Vector3 lastVelocity;

    /// <summary>
    /// Flag indicating if the low energy warning sound is currently active.
    /// </summary>
    private bool lowEnergyWarningActive = false;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the Rigidbody component and sets its properties for space flight.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("SpaceshipController: Rigidbody not found! Disabling script.", this.gameObject);
            enabled = false;
            return;
        }

        rb.useGravity = false; // Spaceships don't typically use Earth-like gravity in space.
        rb.interpolation = RigidbodyInterpolation.Extrapolate; // For smoother visual movement with physics.
    }

    /// <summary>
    /// Called before the first frame update.
    /// Initializes current energy and health, updates the UI via GameManager, and finds the FuelCellSpawner.
    /// </summary>
    void Start()
    {
        currentEnergy = maxEnergy;
        currentHealth = maxHealth;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateEnergyUI(currentEnergy, maxEnergy);
            GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }

        fuelCellSpawner = FindFirstObjectByType<FuelCellSpawner>();
        if (fuelCellSpawner == null)
        {
            Debug.LogWarning("SpaceshipController: FuelCellSpawner not found in the scene. Fuel collection reporting might be affected.", this.gameObject);
        }
    }

    /// <summary>
    /// Called every fixed framerate frame, if the Behaviour is enabled.
    /// Handles physics-based movement and rotation if the game is not paused or over.
    /// Consumes energy based on actions and updates the last known velocity.
    /// </summary>
    void FixedUpdate()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isPaused || GameManager.Instance.isGameOver))
        {
            rb.linearVelocity = Vector3.zero;   // Stop movement when paused/game over.
            rb.angularVelocity = Vector3.zero; // Stop rotation when paused/game over.
            return;
        }

        bool isActivelyMoving = HandleMovementAndStrafe();
        bool isActivelyTurning = HandleRotation();

        ConsumeEnergy(isActivelyMoving, isActivelyTurning);

        lastVelocity = rb.linearVelocity; // Store velocity for next frame's impact calculation.
    }

    /// <summary>
    /// Handles player input for forward/backward thrust and horizontal/vertical strafing.
    /// Applies forces to the Rigidbody based on input.
    /// </summary>
    /// <returns>True if any movement input was detected, false otherwise.</returns>
    bool HandleMovementAndStrafe()
    {
        float thrustInput = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow
        float horizontalStrafeInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float verticalStrafeInput = 0f;

        if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Space)) verticalStrafeInput = 1f; // Up
        else if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.LeftAlt)) verticalStrafeInput = -1f; // Down

        Vector3 combinedMovementForce = Vector3.zero;
        combinedMovementForce += Vector3.forward * thrustInput * moveSpeed;
        combinedMovementForce += Vector3.right * horizontalStrafeInput * moveSpeed * strafeSpeedModifier;
        combinedMovementForce += Vector3.up * verticalStrafeInput * moveSpeed * verticalStrafeSpeedModifier;

        if (combinedMovementForce.sqrMagnitude > 0.01f) // Check if there's any significant input
        {
            rb.AddRelativeForce(combinedMovementForce);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles player input for yaw (left/right) rotation.
    /// Rotates the spaceship's transform directly.
    /// </summary>
    /// <returns>True if any rotation input was detected, false otherwise.</returns>
    bool HandleRotation()
    {
        float yawInput = 0f;
        if (Input.GetKey(KeyCode.E)) yawInput = 1f;  // Turn Right
        else if (Input.GetKey(KeyCode.Q)) yawInput = -1f; // Turn Left

        if (Mathf.Abs(yawInput) > 0.01f) // Check if there's any significant input
        {
            transform.Rotate(Vector3.up, yawInput * turnSpeed * Time.fixedDeltaTime, Space.Self);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Consumes spaceship energy based on whether movement or turning actions were performed.
    /// Updates the energy UI and triggers game over if energy is depleted.
    /// Manages the low energy warning sound.
    /// </summary>
    /// <param name="isMoving">True if movement inputs were active in the current frame.</param>
    /// <param name="isTurning">True if turning inputs were active in the current frame.</param>
    void ConsumeEnergy(bool isMoving, bool isTurning)
    {
        if (currentEnergy <= 0 && !(isMoving || isTurning)) return; // No energy to consume unless an action is taken

        float totalConsumptionThisFrame = 0f;

        if (isMoving) totalConsumptionThisFrame += energyConsumptionRate * Time.fixedDeltaTime;
        if (isTurning) totalConsumptionThisFrame += turnEnergyCost; // Flat cost per turning frame

        if (totalConsumptionThisFrame > 0)
        {
            currentEnergy -= totalConsumptionThisFrame;
            currentEnergy = Mathf.Max(0, currentEnergy); // Clamp energy to a minimum of 0.

            GameManager.Instance?.UpdateEnergyUI(currentEnergy, maxEnergy);

            // Low energy warning logic
            bool shouldWarningBeActive = (currentEnergy <= maxEnergy * lowEnergyThresholdPercentage && currentEnergy > 0);
            if (shouldWarningBeActive && !lowEnergyWarningActive)
            {
                AudioManager.Instance?.StartLowEnergyWarning();
                lowEnergyWarningActive = true;
            }
            else if (!shouldWarningBeActive && lowEnergyWarningActive)
            {
                AudioManager.Instance?.StopLowEnergyWarning();
                lowEnergyWarningActive = false;
            }

            if (currentEnergy <= 0)
            {
                Debug.Log("Spaceship energy depleted!", this.gameObject);
                if (lowEnergyWarningActive) // Ensure warning is stopped if energy hits zero
                {
                    AudioManager.Instance?.StopLowEnergyWarning();
                    lowEnergyWarningActive = false;
                }
                GameManager.Instance?.GameOver(false, "Energy Depleted!");
            }
        }
    }

    /// <summary>
    /// Applies damage to the spaceship based on the magnitude of an impact.
    /// Updates health UI and triggers game over if health is depleted.
    /// </summary>
    /// <param name="impactMagnitude">The calculated force or magnitude of the collision impact.</param>
    public void ApplyCollisionDamage(float impactMagnitude)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return; // No damage if game is already over.
        if (impactMagnitude < minImpactForceForDamage) return; // Ignore minor impacts.

        float damageToApply = impactMagnitude * collisionDamageMultiplier;
        currentHealth -= damageToApply;
        currentHealth = Mathf.Max(0, currentHealth); // Clamp health to a minimum of 0.

        AudioManager.Instance?.PlayAsteroidImpact(); // Play impact sound.
        Debug.Log($"Spaceship took {damageToApply:F2} collision damage from impact. Health: {currentHealth:F2}", this.gameObject);

        GameManager.Instance?.UpdateHealthUI(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            GameManager.Instance?.GameOver(false, "Ship Destroyed by Collision!");
        }
    }

    /// <summary>
    /// Called when the spaceship collects a fuel cell.
    /// Restores energy, triggers score processing, and updates UI.
    /// </summary>
    /// <param name="energyAmount">The amount of energy to restore.</param>
    /// <param name="baseScoreAmount">The base score value of the fuel cell.</param>
    public void CollectFuel(float energyAmount, int baseScoreAmount)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        currentEnergy += energyAmount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy); // Ensure energy does not exceed maximum.

        AudioManager.Instance?.PlayFuelPickup(); // Play fuel pickup sound.

        // If collecting fuel brings energy above the warning threshold, stop the warning.
        if (currentEnergy > maxEnergy * lowEnergyThresholdPercentage && lowEnergyWarningActive)
        {
            AudioManager.Instance?.StopLowEnergyWarning();
            lowEnergyWarningActive = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessFuelCollection(baseScoreAmount);
            GameManager.Instance.UpdateEnergyUI(currentEnergy, maxEnergy);
        }
    }

    /// <summary>
    /// Called by Unity's physics engine when this GameObject's collider makes contact with another Rigidbody/Collider.
    /// Handles collision with asteroids.
    /// </summary>
    /// <param name="collision">Detailed information about the collision.</param>
    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (collision.gameObject.CompareTag("Asteroid"))
        {
            // Calculate impact magnitude based on the change in velocity during the collision.
            // This provides a measure of the collision's force.
            float impactMagnitude = (rb.linearVelocity - lastVelocity).magnitude / Time.fixedDeltaTime;

            Debug.Log($"Collided with Asteroid! Impact Magnitude: {impactMagnitude:F2}", this.gameObject);
            ApplyCollisionDamage(impactMagnitude);

            Destroy(collision.gameObject); // Destroy the asteroid on impact.
        }
    }

    /// <summary>
    /// Called by Unity's physics engine when this GameObject's collider enters another collider marked as a Trigger.
    /// Handles collection of FuelCells.
    /// </summary>
    /// <param name="other">The other Collider involved in this trigger event.</param>
    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (other.gameObject.CompareTag("FuelCell"))
        {
            FuelCell fuelCellComponent = other.GetComponent<FuelCell>();
            if (fuelCellComponent != null)
            {
                Debug.Log($"Spaceship triggered FuelCell: {other.gameObject.name}", this.gameObject);
                CollectFuel(fuelCellComponent.energyValue, fuelCellComponent.baseScoreValue);

                // Report to spawner before destroying the fuel cell.
                if (fuelCellSpawner != null)
                {
                    fuelCellSpawner.ReportFuelCellCollected(other.gameObject);
                }
                else
                {
                    Debug.LogWarning("SpaceshipController: FuelCellSpawner reference is null. Cannot report fuel cell collection.", this.gameObject);
                }
                Destroy(other.gameObject); // Destroy the collected fuel cell.
            }
        }
    }
}