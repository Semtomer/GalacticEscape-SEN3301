using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for Linq operations like Count(predicate).

/// <summary>
/// Responsible for spawning a specified number of FuelCell GameObjects within a defined area.
/// It uses a procedural FuelCell prefab that generates its own mesh and material.
/// This spawner also tracks the active fuel cells and triggers an event when all spawned cells are collected.
/// </summary>
public class FuelCellSpawner : MonoBehaviour
{
    [Tooltip("The prefab to instantiate for each fuel cell. This prefab should have FuelCellGenerator and FuelCell scripts attached.")]
    [SerializeField] private GameObject proceduralFuelCellPrefab;

    [Tooltip("The total number of fuel cells to attempt to spawn in the level.")]
    [SerializeField] private int numberOfFuelCellsToSpawn = 15;

    [Tooltip("The center of the rectangular area where fuel cells will be spawned.")]
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;

    [Tooltip("The size (dimensions X, Y, Z) of the rectangular spawn area, centered around spawnAreaCenter.")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(50, 20, 50);

    [Tooltip("The minimum height (Y-coordinate) at which fuel cells can be spawned.")]
    [SerializeField] private float minSpawnHeight = 1f;

    [Tooltip("The minimum distance required between the centers of any two spawned fuel cells to prevent overlapping.")]
    [SerializeField] private float minDistanceBetweenCells = 3f;

    [Tooltip("Approximate radius used for checking against obstacles (e.g., asteroids, terrain) when finding a spawn position. Should be related to the fuel cell's visual size.")]
    [SerializeField] private float spawnObstacleCheckRadius = 1.0f;

    /// <summary>
    /// Static event triggered when all fuel cells initially spawned by this spawner have been collected.
    /// Other systems (like GameManager) can subscribe to this event to react to this game state.
    /// </summary>
    public static event System.Action OnAllFuelCellsCollected;

    /// <summary>
    /// List of currently active (spawned and not yet collected) fuel cell GameObject instances.
    /// </summary>
    private List<GameObject> activeFuelCellInstances = new List<GameObject>();

    /// <summary>
    /// The number of fuel cells that were successfully spawned and are being tracked at the start of the level.
    /// </summary>
    private int initialSpawnCount = 0;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Validates that the proceduralFuelCellPrefab is assigned and has the necessary components.
    /// If validation passes, it calls <see cref="SpawnFuelCells"/>.
    /// </summary>
    void Start()
    {
        if (proceduralFuelCellPrefab == null)
        {
            Debug.LogError("FuelCellSpawner: ProceduralFuelCell Prefab not assigned! Disabling spawner.", this.gameObject);
            enabled = false;
            return;
        }
        // Validate that the prefab has the required generator and behavior scripts.
        if (proceduralFuelCellPrefab.GetComponent<FuelCellGenerator>() == null ||
            proceduralFuelCellPrefab.GetComponent<FuelCell>() == null)
        {
            Debug.LogError("FuelCellSpawner: ProceduralFuelCell Prefab is missing FuelCellGenerator or FuelCell script! Disabling spawner.", this.gameObject);
            enabled = false;
            return;
        }
        SpawnFuelCells();
    }

    /// <summary>
    /// Spawns the configured number of fuel cells within the defined spawn area.
    /// It attempts to find valid positions for each fuel cell, avoiding obstacles and other spawned cells.
    /// After spawning, it performs an initial check to see if all cells (if any) might have been "collected" (e.g., if 0 were set to spawn).
    /// </summary>
    void SpawnFuelCells()
    {
        activeFuelCellInstances.Clear(); // Clear any instances from a previous run (e.g., if level restarts).
        initialSpawnCount = 0;

        for (int i = 0; i < numberOfFuelCellsToSpawn; i++)
        {
            Vector3 spawnPosition;
            int attempts = 0; // To prevent infinite loops if no valid position can be found.
            bool positionFound = false;

            do
            {
                // Generate a random position within the spawn volume.
                float x = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
                float y = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2);
                float z = Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2);
                y = Mathf.Max(y, minSpawnHeight); // Ensure it's not below the minimum height.
                spawnPosition = transform.TransformPoint(new Vector3(x, y, z)); // Convert local spawn area coords to world coords if spawner is moved/rotated.
                attempts++;

                // Check if the proposed spawn position is too close to an existing obstacle.
                bool tooCloseToObstacle = Physics.CheckSphere(spawnPosition, spawnObstacleCheckRadius);
                bool tooCloseToOtherSpawned = false;

                if (!tooCloseToObstacle)
                {
                    // Check if too close to already spawned fuel cells in this batch.
                    foreach (GameObject spawnedCell in activeFuelCellInstances)
                    {
                        if (Vector3.Distance(spawnPosition, spawnedCell.transform.position) < minDistanceBetweenCells)
                        {
                            tooCloseToOtherSpawned = true;
                            break;
                        }
                    }
                }

                if (!tooCloseToObstacle && !tooCloseToOtherSpawned)
                {
                    positionFound = true; // Valid position found.
                }

            } while (!positionFound && attempts < 50); // Limit attempts to find a position.

            if (positionFound)
            {
                GameObject fuelCellInstance = Instantiate(proceduralFuelCellPrefab, spawnPosition, Quaternion.identity);
                activeFuelCellInstances.Add(fuelCellInstance);
                initialSpawnCount++;
            }
            else
            {
                Debug.LogWarning($"FuelCellSpawner: Could not find a clear spot for fuel cell {i + 1} after {attempts} attempts.", this.gameObject);
            }
        }
        Debug.Log($"FuelCellSpawner: {initialSpawnCount} procedural fuel cells spawned and are being tracked.", this.gameObject);

        if (initialSpawnCount == 0 && numberOfFuelCellsToSpawn > 0)
        {
            Debug.LogWarning("FuelCellSpawner: No fuel cells were spawned (e.g., spawn area too small or obstructed). 'All Cells Collected' win condition might not trigger as expected.", this.gameObject);
        }
        // Initial check, important if numberOfFuelCellsToSpawn is 0.
        CheckForAllCellsCollected();
    }

    /// <summary>
    /// Called by other scripts (e.g., <see cref="SpaceshipController"/>) when a fuel cell is collected.
    /// Removes the collected cell from the tracking list and checks if all cells have now been collected.
    /// </summary>
    /// <param name="collectedCell">The GameObject of the fuel cell that was collected.</param>
    public void ReportFuelCellCollected(GameObject collectedCell)
    {
        if (collectedCell == null) return;

        // Attempt to remove the cell from the list of active instances.
        bool removed = activeFuelCellInstances.Remove(collectedCell);
        // if (removed) Debug.Log($"FuelCellSpawner: Reported collection of {collectedCell.name}. Remaining: {activeFuelCellInstances.Count}");
        // else Debug.LogWarning($"FuelCellSpawner: {collectedCell.name} reported collected but not found in active list.");

        CheckForAllCellsCollected(); // Check if this collection was the last one.
    }

    /// <summary>
    /// Checks if all initially spawned fuel cells have been collected.
    /// If so, invokes the <see cref="OnAllFuelCellsCollected"/> event.
    /// This handles the win condition where the player collects all items.
    /// </summary>
    private void CheckForAllCellsCollected()
    {
        // Count active cells (ensuring to not count any that might have become null unexpectedly, though Remove should handle this).
        int activeCount = activeFuelCellInstances.Count(cell => cell != null);

        // Debug.Log($"FuelCellSpawner: Checking all cells collected. Initial: {initialSpawnCount}, Active: {activeCount}");

        // Condition 1: If cells were intended to be spawned, and all of them are now gone.
        if (initialSpawnCount > 0 && activeCount == 0)
        {
            Debug.Log("FuelCellSpawner: All fuel cells have been collected! Invoking event.", this.gameObject);
            OnAllFuelCellsCollected?.Invoke();
        }
        // Condition 2: If no cells were set to be spawned from the start, this is also considered "all collected".
        else if (initialSpawnCount == 0 && numberOfFuelCellsToSpawn == 0)
        {
            Debug.Log("FuelCellSpawner: No fuel cells were set to spawn. Invoking event for immediate 'all collected' state.", this.gameObject);
            OnAllFuelCellsCollected?.Invoke();
        }
    }
}