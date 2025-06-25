using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene load events

/// <summary>
/// Manages all audio playback in the game, including music, sound effects (SFX), and jingles.
/// Implements a Singleton pattern to ensure only one instance exists and is easily accessible.
/// Handles scene transitions to manage music playback appropriately.
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Static instance of the AudioManager to allow access from other scripts.
    /// </summary>
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource component dedicated to playing background music.")]
    [SerializeField] private AudioSource musicSource;

    [Tooltip("AudioSource component dedicated to playing one-shot sound effects.")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("AudioSource component dedicated to playing jingles (win/lose).")]
    [SerializeField] private AudioSource jingleSource;

    [Tooltip("AudioSource component dedicated to playing the looping low energy warning sound.")]
    [SerializeField] private AudioSource lowEnergyWarningSource;

    [Header("Music Clips")]
    [Tooltip("The primary background music for gameplay.")]
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Jingle Clips")]
    [Tooltip("Short audio clip played when the player wins.")]
    [SerializeField] private AudioClip winJingle;

    [Tooltip("Short audio clip played when the player loses or the game ends unfavorably.")]
    [SerializeField] private AudioClip loseJingle;

    [Header("SFX Clips")]
    [Tooltip("Sound effect for an asteroid impacting the spaceship.")]
    [SerializeField] private AudioClip asteroidImpactSound;

    [Tooltip("Looping beep sound effect for low energy warning.")]
    [SerializeField] private AudioClip lowEnergyBeep;

    [Tooltip("Sound effect for collecting a fuel cell.")]
    [SerializeField] private AudioClip fuelPickupSound;

    [Tooltip("Sound effect for UI button clicks.")]
    [SerializeField] private AudioClip buttonClickSound;

    /// <summary>
    /// Flag to track if the low energy warning sound is currently playing.
    /// </summary>
    private bool isLowEnergyWarningPlaying = false;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Implements the Singleton pattern and initializes AudioSource components.
    /// Subscribes to the sceneLoaded event.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist AudioManager across scene loads.
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load event.
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances.
            return;
        }

        // Ensure AudioSource components exist, adding them if necessary.
        // It's generally preferred to assign these in the Inspector.
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (jingleSource == null) jingleSource = gameObject.AddComponent<AudioSource>();
        if (lowEnergyWarningSource == null) lowEnergyWarningSource = gameObject.AddComponent<AudioSource>();

        // Configure AudioSource properties.
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        jingleSource.loop = false;
        jingleSource.playOnAwake = false;

        lowEnergyWarningSource.loop = true;
        lowEnergyWarningSource.playOnAwake = false;
        if (lowEnergyBeep != null) lowEnergyWarningSource.clip = lowEnergyBeep; // Pre-assign clip

        sfxSource.playOnAwake = false;
    }

    /// <summary>
    /// Called when the MonoBehaviour will be destroyed.
    /// Unsubscribes from the sceneLoaded event to prevent memory leaks.
    /// </summary>
    void OnDestroy()
    {
        if (Instance == this) // Ensure this is the original instance before unsubscribing.
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// Callback method executed when a new scene is loaded.
    /// Manages music playback based on the loaded scene (e.g., starts gameplay music for "GameScene").
    /// Stops any playing jingles when the game scene loads.
    /// </summary>
    /// <param name="scene">The <see cref="Scene"/> that was loaded.</param>
    /// <param name="mode">The <see cref="LoadSceneMode"/> used to load the scene.</param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Assuming your main game scene is named "GameScene". Adjust if different.
        if (scene.name == "GameScene")
        {
            if (jingleSource != null && jingleSource.isPlaying)
            {
                jingleSource.Stop(); // Stop any jingle if game scene reloads (e.g., after restart)
            }
            PlayGameplayMusic(); // Start/resume gameplay music
        }
        else // For other scenes (e.g., Main Menu)
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop(); // Stop gameplay music
            }
            if (jingleSource != null && jingleSource.isPlaying)
            {
                jingleSource.Stop(); // Stop any jingles
            }
        }
    }

    /// <summary>
    /// Plays the main gameplay background music.
    /// If the music is already playing with the correct clip, it does nothing.
    /// </summary>
    public void PlayGameplayMusic()
    {
        if (gameplayMusic != null && musicSource != null)
        {
            // Play only if it's not already playing this clip.
            if (musicSource.clip != gameplayMusic || !musicSource.isPlaying)
            {
                musicSource.clip = gameplayMusic;
                musicSource.Play();
                Debug.Log("Gameplay Music Started/Resumed.");
            }
        }
    }

    /// <summary>
    /// Explicitly stops the main gameplay background music.
    /// </summary>
    public void StopGameplayMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("Gameplay Music Stopped explicitly.");
        }
    }

    /// <summary>
    /// Plays the win jingle. Stops the main music first.
    /// Uses the dedicated jingleSource.
    /// </summary>
    public void PlayWinJingle()
    {
        if (winJingle != null && jingleSource != null)
        {
            // musicSource.Stop(); // Game Manager already calls StopGameplayMusic
            jingleSource.clip = winJingle;
            jingleSource.Play();
        }
    }

    /// <summary>
    /// Plays the lose jingle. Stops the main music first.
    /// Uses the dedicated jingleSource.
    /// </summary>
    public void PlayLoseJingle()
    {
        if (loseJingle != null && jingleSource != null)
        {
            // musicSource.Stop(); // Game Manager already calls StopGameplayMusic
            jingleSource.clip = loseJingle;
            jingleSource.Play();
        }
    }

    /// <summary>
    /// Plays the asteroid impact sound effect as a one-shot.
    /// </summary>
    public void PlayAsteroidImpact()
    {
        if (asteroidImpactSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(asteroidImpactSound);
        }
    }

    /// <summary>
    /// Starts playing the looping low energy warning sound if it's not already playing.
    /// </summary>
    public void StartLowEnergyWarning()
    {
        if (lowEnergyBeep != null && lowEnergyWarningSource != null && !isLowEnergyWarningPlaying)
        {
            if (lowEnergyWarningSource.clip != lowEnergyBeep) // Ensure correct clip is set
                lowEnergyWarningSource.clip = lowEnergyBeep;
            lowEnergyWarningSource.Play();
            isLowEnergyWarningPlaying = true;
        }
    }

    /// <summary>
    /// Stops the looping low energy warning sound if it is currently playing.
    /// </summary>
    public void StopLowEnergyWarning()
    {
        if (lowEnergyWarningSource != null && isLowEnergyWarningPlaying)
        {
            lowEnergyWarningSource.Stop();
            isLowEnergyWarningPlaying = false;
        }
    }

    /// <summary>
    /// Plays the fuel pickup sound effect as a one-shot.
    /// </summary>
    public void PlayFuelPickup()
    {
        if (fuelPickupSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(fuelPickupSound);
        }
    }

    /// <summary>
    /// Plays the UI button click sound effect as a one-shot.
    /// </summary>
    public void PlayButtonClick()
    {
        if (buttonClickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    /// <summary>
    /// Sets the volume for the background music.
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0).</param>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Sets the volume for jingles.
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0).</param>
    public void SetJingleVolume(float volume)
    {
        if (jingleSource != null) jingleSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Sets the volume for general sound effects and the low energy warning.
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0).</param>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(volume);
        if (lowEnergyWarningSource != null) lowEnergyWarningSource.volume = Mathf.Clamp01(volume);
    }
}