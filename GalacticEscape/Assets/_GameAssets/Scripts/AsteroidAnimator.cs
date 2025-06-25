using UnityEngine;

/// <summary>
/// Handles the continuous movement and rotation animation for a single asteroid GameObject.
/// Requires a Rigidbody component on the same GameObject for physics-based movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AsteroidAnimator : MonoBehaviour
{
    #region Variables

    [Header("Movement Settings")]
    [Tooltip("Minimum speed the asteroid will move at.")]
    [SerializeField] private float minSpeed = 2.5f;

    [Tooltip("Maximum speed the asteroid will move at.")]
    [SerializeField] private float maxSpeed = 5f;


    [Header("Rotation Settings")]
    [Tooltip("Minimum rotation speed (degrees per second).")]
    [SerializeField] private float minRotationSpeed = 5.0f;

    [Tooltip("Maximum rotation speed (degrees per second).")]
    [SerializeField] private float maxRotationSpeed = 30.0f;


    // --- Private Variables ---
    /// <summary>
    /// Cached Rigidbody component reference.
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// The constant velocity vector determining the asteroid's linear movement.
    /// </summary>
    private Vector3 movementVelocity;

    /// <summary>
    /// The world-space axis around which the asteroid rotates.
    /// </summary>
    private Vector3 rotationAxis;

    /// <summary>
    /// The speed at which the asteroid rotates around its <see cref="rotationAxis"/> (degrees per second).
    /// </summary>
    private float rotationSpeed;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Called when the script instance is first loaded.
    /// Caches the Rigidbody component and initializes random movement and rotation parameters for the asteroid.
    /// Configures Rigidbody properties for space movement (no gravity, some drag).
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0.1f;  // Provides some resistance to linear motion changes.
        rb.angularDamping = 0.1f; // Provides some resistance to angular motion changes.

        // --- Initialize Movement ---
        Vector3 randomDirection = Random.onUnitSphere; // A random direction in 3D space.
        float randomSpeedValue = Random.Range(minSpeed, maxSpeed); // A random speed within the specified range.
        movementVelocity = randomDirection * randomSpeedValue; // Calculate the final constant velocity vector.
        rb.linearVelocity = movementVelocity; // Apply the initial velocity to the Rigidbody.

        // --- Initialize Rotation ---
        rotationAxis = Random.onUnitSphere; // A random world-space axis to rotate around.
        this.rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed); // A random rotation speed.
        // Apply a small initial random angular velocity to make the start less uniform.
        rb.angularVelocity = rotationAxis * this.rotationSpeed * Mathf.Deg2Rad * 0.1f;
    }

    /// <summary>
    /// Called once per physics frame (fixed time interval).
    /// Applies continuous rotation to the asteroid's transform.
    /// </summary>
    void FixedUpdate()
    {
        // Apply continuous rotation around the random axis at the random speed in world space.
        transform.Rotate(rotationAxis, this.rotationSpeed * Time.fixedDeltaTime, Space.World);
    }

    #endregion
}