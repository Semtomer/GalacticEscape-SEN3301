using UnityEngine;

/// <summary>
/// Manages the game camera's behavior, allowing it to follow a target (e.g., the player's spaceship),
/// be rotated by mouse input, and zoomed using the mouse scroll wheel.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Tooltip("The Transform of the GameObject the camera should follow and orbit around.")]
    [SerializeField] private Transform target;

    [Tooltip("Initial distance the camera will be from the target.")]
    [SerializeField] private float distance = 15.0f;

    [Tooltip("Minimum allowed distance from the target when zooming in.")]
    [SerializeField] private float minDistance = 5f;

    [Tooltip("Maximum allowed distance from the target when zooming out.")]
    [SerializeField] private float maxDistance = 30f;

    [Tooltip("Sensitivity of camera rotation based on mouse movement.")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Tooltip("Sensitivity of camera zooming based on mouse scroll wheel movement.")]
    [SerializeField] private float scrollSensitivity = 5.0f;

    [Tooltip("Minimum vertical (pitch) angle the camera can rotate to, in degrees.")]
    [SerializeField] private float yMinLimit = -20f;

    [Tooltip("Maximum vertical (pitch) angle the camera can rotate to, in degrees.")]
    [SerializeField] private float yMaxLimit = 80f;

    [Tooltip("Time taken for the camera to smoothly follow the target's position. Lower values mean faster, less smooth following.")]
    [SerializeField] private float smoothTime = 0.12f;

    /// <summary>
    /// Current horizontal rotation angle of the camera around the target (yaw).
    /// </summary>
    private float rotationX = 0.0f;

    /// <summary>
    /// Current vertical rotation angle of the camera around the target (pitch).
    /// </summary>
    private float rotationY = 0.0f;

    /// <summary>
    /// Velocity reference used by <see cref="Vector3.SmoothDamp(Vector3, Vector3, ref Vector3, float)"/> for smooth camera movement.
    /// </summary>
    private Vector3 cameraFollowVelocity = Vector3.zero; // Renamed for clarity

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes camera rotation angles based on its initial orientation if a target is set.
    /// Disables the script if no target is assigned.
    /// </summary>
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraController: Target Transform not assigned! Disabling script.", this.gameObject);
            enabled = false; // Disable the script if no target is set.
            return;
        }

        // Initialize rotation angles from the camera's current Euler angles.
        Vector3 initialAngles = transform.eulerAngles;
        rotationX = initialAngles.y; // Yaw
        rotationY = initialAngles.x; // Pitch
    }

    /// <summary>
    /// Called every frame, if the Behaviour is enabled.
    /// Handles mouse input for camera rotation and zooming.
    /// Updates the camera's position and orientation to follow and look at the target.
    /// It's generally recommended to update camera movement in LateUpdate to ensure
    /// the target has completed its movement for the current frame.
    /// </summary>
    void LateUpdate() // LateUpdate is ideal for camera following logic.
    {
        if (target) // Ensure the target still exists.
        {
            // Handle mouse input for camera rotation when the right mouse button is held down.
            if (Input.GetMouseButton(1)) // 1 corresponds to the right mouse button.
            {
                rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
                rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity; // Inverted for typical camera controls.
                rotationY = Mathf.Clamp(rotationY, yMinLimit, yMaxLimit); // Clamp vertical rotation.
            }

            // Calculate the desired camera rotation based on accumulated mouse input.
            Quaternion desiredRotation = Quaternion.Euler(rotationY, rotationX, 0);

            // Handle mouse scroll wheel input for zooming.
            distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
            distance = Mathf.Clamp(distance, minDistance, maxDistance); // Clamp zoom distance.

            // Calculate the desired camera position based on target's position, rotation, and distance.
            // The camera is positioned 'distance' units behind the target along the Z-axis of the 'desiredRotation'.
            Vector3 desiredPosition = target.position - (desiredRotation * Vector3.forward * distance);

            // Smoothly move the camera towards the desired position.
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref cameraFollowVelocity, smoothTime);

            // Ensure the camera is always looking at the target.
            transform.LookAt(target.position);
        }
    }
}