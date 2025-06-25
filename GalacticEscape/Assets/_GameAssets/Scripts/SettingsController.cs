using UnityEngine;

/// <summary>
/// Manages initial game settings, specifically focusing on screen resolution and camera background setup.
/// This script typically runs once at the start of the game or scene.
/// </summary>
public class SettingsController : MonoBehaviour
{
    [Tooltip("Reference to the main camera in the scene whose clear flags and background will be set.")]
    [SerializeField] private Camera _mainCamera;

    [Tooltip("The Skybox material to be applied as the camera's background.")]
    [SerializeField] private Material skyboxMaterial;

    /// <summary>
    /// Called when the script instance is being loaded, before the Start method.
    /// Sets the game's screen resolution to the current monitor's resolution in full-screen windowed mode.
    /// Configures the main camera to use a skybox for its background and assigns the specified skybox material.
    /// </summary>
    private void Awake()
    {
        // --- Resolution Size Setting ---
        // Sets the game to run in full-screen windowed mode using the current display's resolution.
        // FullScreenMode.FullScreenWindow often provides better compatibility and alt-tab behavior than exclusive full screen.
        if (Screen.currentResolution.width > 0 && Screen.currentResolution.height > 0) // Basic check for valid resolution
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            Debug.Log($"Screen resolution set to: {Screen.currentResolution.width}x{Screen.currentResolution.height} (FullScreenWindow)");
        }
        else
        {
            Debug.LogWarning("SettingsController: Could not retrieve valid current screen resolution. Resolution not set.");
        }


        // --- Camera and Background Setting ---
        if (_mainCamera != null)
        {
            // Set the camera's clear flags to Skybox, so it renders the skybox material instead of a solid color or depth only.
            _mainCamera.clearFlags = CameraClearFlags.Skybox;

            if (skyboxMaterial != null)
            {
                // Assign the specified skybox material to the scene's render settings.
                // This makes it the default skybox for all cameras that use skybox clearing.
                RenderSettings.skybox = skyboxMaterial;
                Debug.Log($"Skybox '{skyboxMaterial.name}' assigned to RenderSettings.");
            }
            else
            {
                Debug.LogWarning("SettingsController: Skybox Material is not assigned. Camera background will use default or existing skybox.", this.gameObject);
            }
        }
        else
        {
            Debug.LogError("SettingsController: Main Camera reference is not assigned. Camera background settings cannot be applied.", this.gameObject);
        }
    }
}