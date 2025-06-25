using UnityEngine;

/// <summary>
/// Represents a collectible fuel cell in the game.
/// Manages its score and energy values, and handles its visual animation (rotation and floating).
/// This script is typically attached to a FuelCell prefab.
/// </summary>
public class FuelCell : MonoBehaviour
{
    [Tooltip("The base score value awarded to the player when this fuel cell is collected.")]
    public int baseScoreValue = 100;

    [Tooltip("The amount of energy restored to the spaceship when this fuel cell is collected.")]
    public float energyValue = 15f; // Changed from 10f in a previous version, matches current provided code.

    [Tooltip("Speed at which the fuel cell rotates around its local Y-axis (degrees per second).")]
    [SerializeField] private float rotationSpeed = 100f;

    [Tooltip("Maximum distance the fuel cell will move up or down from its starting position during its floating animation.")]
    [SerializeField] private float floatAmplitude = 0.1f;

    [Tooltip("Speed of the up-and-down floating animation cycle.")]
    [SerializeField] private float floatSpeed = 1f;

    /// <summary>
    /// The initial position of the fuel cell when it was spawned or started. Used as a reference for the floating animation.
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Stores the initial position of the fuel cell for the floating animation.
    /// </summary>
    void Start()
    {
        startPosition = transform.position;
    }

    /// <summary>
    /// Called every frame, if the Behaviour is enabled.
    /// Updates the fuel cell's rotation and floating animation.
    /// </summary>
    void Update()
    {
        // Rotate the fuel cell around its world Y-axis.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Animate the fuel cell floating up and down using a sine wave.
        // The Y position oscillates around its 'startPosition.y'.
        float newYPosition = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);
    }
}