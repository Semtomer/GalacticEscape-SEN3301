// GameManager.cs
using UnityEngine;
using UnityEngine.UI; // Required for Button and potentially older UI Text if not using TextMeshPro
using UnityEngine.SceneManagement; // Required for scene management (e.g., restarting the game)
using TMPro; // Required for TextMeshPro UI elements

/// <summary>
/// Manages the overall game state, including score, time, player stats (via SpaceshipController),
/// UI updates, pause functionality, and game over conditions.
/// Implements a Singleton pattern for easy global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Static instance of the GameManager, allowing access from any script.
    /// </summary>
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [Tooltip("Initial time limit for the game session, in seconds.")]
    [SerializeField] private float initialTimeLimit = 120f;

    [Tooltip("Multiplier used in score calculation based on time. A higher value might mean time has less impact, or it's a base for a bonus calculation.")]
    [SerializeField] private float timeScoreMultiplier = 50f;

    [Header("UI Elements - Gameplay")]
    [Tooltip("TextMeshProUGUI element to display the player's score.")]
    [SerializeField] private TMP_Text scoreText;

    [Tooltip("TextMeshProUGUI element to display the remaining time.")]
    [SerializeField] private TMP_Text timeText;

    [Tooltip("TextMeshProUGUI element to display the player's current and max energy.")]
    [SerializeField] private TMP_Text energyText;

    [Tooltip("TextMeshProUGUI element to display the player's current and max health.")]
    [SerializeField] private TMP_Text healthText;

    [Header("UI Elements - Game Over")]
    [Tooltip("Panel GameObject that is shown when the game ends.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("TextMeshProUGUI element on the GameOverPanel to display win/loss status.")]
    [SerializeField] private TMP_Text gameOverStatusText;

    [Tooltip("TextMeshProUGUI element on the GameOverPanel to display the reason for game over and final score.")]
    [SerializeField] private TMP_Text gameOverReasonText;

    [Tooltip("Button on the GameOverPanel to restart the game.")]
    [SerializeField] private Button restartButtonGameOver;

    [Tooltip("Button on the GameOverPanel to quit the game.")]
    [SerializeField] private Button quitButtonGameOver; // Added as per previous setup context

    [Header("UI Elements - Pause Menu")]
    [Tooltip("Panel GameObject for the pause menu.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Button on the PauseMenuPanel to resume the game.")]
    [SerializeField] private Button resumeButton;

    [Tooltip("Button on the PauseMenuPanel to restart the game.")]
    [SerializeField] private Button restartButtonPauseMenu;

    [Tooltip("Button on the PauseMenuPanel to quit the game.")]
    [SerializeField] private Button quitButton; // This refers to the quit button on the Pause Menu

    /// <summary>
    /// Current player score.
    /// </summary>
    private int score = 0;

    /// <summary>
    /// Time remaining in the current game session.
    /// </summary>
    private float timeLeft;

    /// <summary>
    /// Timestamp (Time.time) when the game session started. Used for score calculations based on speed.
    /// </summary>
    private float gameStartTime;

    /// <summary>
    /// Flag indicating whether the game is currently over.
    /// Public for <see cref="SpaceshipController"/> to check before applying damage/collecting fuel post-game over.
    /// </summary>
    public bool isGameOver = false;

    /// <summary>
    /// Flag indicating whether the game is currently paused.
    /// Public for <see cref="SpaceshipController"/> to check before processing movement/actions.
    /// </summary>
    public bool isPaused = false;

    /// <summary>
    /// Cached reference to the player's SpaceshipController.
    /// </summary>
    private SpaceshipController spaceshipController;
    // private bool allCellsWinConditionMet = false; // This flag seems redundant if HandleAllFuelCellsCollected directly calls GameOver

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Sets up the Singleton instance and subscribes to the OnAllFuelCellsCollected event.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Consider if AudioManager already handles persistence
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        FuelCellSpawner.OnAllFuelCellsCollected += HandleAllFuelCellsCollected;
    }

    /// <summary>
    /// Called when the MonoBehaviour will be destroyed.
    /// Unsubscribes from the OnAllFuelCellsCollected event to prevent memory leaks.
    /// </summary>
    void OnDestroy()
    {
        if (Instance == this) // Only unsubscribe if this is the active Singleton instance
        {
            FuelCellSpawner.OnAllFuelCellsCollected -= HandleAllFuelCellsCollected;
        }
    }

    /// <summary>
    /// Called before the first frame update.
    /// Initializes game state, UI, and button listeners.
    /// </summary>
    void Start()
    {
        spaceshipController = FindFirstObjectByType<SpaceshipController>();
        if (spaceshipController == null) Debug.LogError("GameManager: SpaceshipController not found in scene!", this.gameObject);

        timeLeft = initialTimeLimit;
        gameStartTime = Time.time;
        UpdateScoreUI();
        UpdateTimeUI();
        if (spaceshipController != null)
        {
            UpdateEnergyUI(spaceshipController.currentEnergy, spaceshipController.maxEnergy);
            UpdateHealthUI(spaceshipController.currentHealth, spaceshipController.maxHealth);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Setup button listeners
        if (restartButtonGameOver != null) restartButtonGameOver.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); RestartGame(); });
        if (quitButtonGameOver != null) quitButtonGameOver.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); QuitGame(); });
        if (resumeButton != null) resumeButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); ResumeGame(); });
        if (restartButtonPauseMenu != null) restartButtonPauseMenu.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); RestartGame(); });
        if (quitButton != null) quitButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); QuitGame(); });

        Time.timeScale = 1f; // Ensure game time runs normally at start.
        isGameOver = false;
        isPaused = false;
        // allCellsWinConditionMet = false; // Redundant, handled by event directly calling GameOver
    }

    /// <summary>
    /// Called every frame.
    /// Handles ESC key input for pausing/resuming and updates the game timer.
    /// Checks for time-based game over condition.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGameOver) return; // Don't allow pausing if game is already over.
            TogglePause();
        }

        if (isGameOver || isPaused) return; // Skip game logic updates if paused or game over.

        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0, timeLeft); // Prevent time from going negative.
        UpdateTimeUI();

        if (!isGameOver && timeLeft <= 0) // Check if game is not already over from another condition.
        {
            GameOver(false, $"Time's Up!\nFinal Score: {score}");
        }
    }

    /// <summary>
    /// Event handler called when <see cref="FuelCellSpawner.OnAllFuelCellsCollected"/> is invoked.
    /// Ends the game with a win condition if not already over.
    /// </summary>
    private void HandleAllFuelCellsCollected()
    {
        if (isGameOver) return; // Do nothing if game has already ended.
        Debug.Log("GameManager: Win condition met - All fuel cells collected.", this.gameObject);
        GameOver(true, $"All Fuel Cells Collected!\nFinal Score: {score}");
    }

    /// <summary>
    /// Triggers the game over sequence.
    /// Stops game time, displays the game over panel with appropriate messages and sounds.
    /// </summary>
    /// <param name="playerWon">True if the player won, false otherwise.</param>
    /// <param name="reason">The string message explaining why the game ended and displaying the final score.</param>
    public void GameOver(bool playerWon, string reason)
    {
        if (isGameOver) return; // Prevent multiple calls.
        isGameOver = true;
        isPaused = false; // Game over overrides pause.
        Time.timeScale = 0f; // Stop game time.

        // Play appropriate sounds and stop ongoing game sounds.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGameplayMusic();
            AudioManager.Instance.StopLowEnergyWarning();
            if (playerWon) AudioManager.Instance.PlayWinJingle();
            else AudioManager.Instance.PlayLoseJingle();
        }

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false); // Hide pause menu if it was open.

        string statusMessage = playerWon ? "YOU WIN!" : "GAME OVER"; // Default status
        string finalReasonMessage = reason; // Use reason as is, assuming it contains score.

        // Refine messages based on specific loss conditions if needed (already handled by how `reason` is constructed)
        if (!playerWon)
        {
            if (reason.Contains("Energy Depleted") || reason.Contains("Ship Destroyed by Collision"))
            {
                statusMessage = "YOU LOST!";
                // finalReasonMessage = $"{reason}\nFinal Score: {score}"; // Score is already in reason
            }
            else if (reason.Contains("Time's Up"))
            {
                statusMessage = "GAME ENDED";
            }
        }


        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverStatusText != null) gameOverStatusText.text = statusMessage;
            if (gameOverReasonText != null) gameOverReasonText.text = finalReasonMessage;
        }
        else
        {
            Debug.LogWarning("GameManager: GameOverPanel is not assigned in Inspector.", this.gameObject);
        }
        Debug.Log($"Game Over. Player Won: {playerWon}. Reason: {finalReasonMessage}", this.gameObject);
    }

    /// <summary>
    /// Toggles the paused state of the game.
    /// Adjusts Time.timeScale and shows/hides the pause menu panel.
    /// </summary>
    public void TogglePause()
    {
        if (isGameOver) return; // Cannot pause if game is over.
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f; // Pause or resume game time.
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(isPaused);
        Debug.Log(isPaused ? "Game Paused" : "Game Resumed", this.gameObject);
    }

    /// <summary>
    /// Resumes the game if it is currently paused. Called by the Resume button.
    /// </summary>
    public void ResumeGame()
    {
        if (isPaused) TogglePause();
    }

    /// <summary>
    /// Quits the application. Called by Quit buttons.
    /// Includes conditional compilation for editor behavior.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game...", this.gameObject);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor.
#endif
    }

    /// <summary>
    /// Called by <see cref="SpaceshipController"/> when a fuel cell is collected.
    /// Calculates score based on base value and a time bonus, then updates the score UI.
    /// </summary>
    /// <param name="baseScoreValue">The base score value of the collected fuel cell.</param>
    public void ProcessFuelCollection(int baseScoreValue)
    {
        if (isGameOver || isPaused) return;

        // Score bonus based on remaining time, ensuring a minimum bonus.
        int timeBonus = Mathf.Max(0, Mathf.CeilToInt(timeScoreMultiplier * (timeLeft / initialTimeLimit)));
        int calculatedScore = baseScoreValue + timeBonus;
        score += calculatedScore;

        Debug.Log($"Fuel collected. Base: {baseScoreValue}, TimeBonus: {timeBonus}, Total Added: {calculatedScore}, New Score: {score}", this.gameObject);
        UpdateScoreUI();
    }

    /// <summary>
    /// Updates the energy UI text element.
    /// </summary>
    /// <param name="currentEnergy">The spaceship's current energy.</param>
    /// <param name="maxEnergy">The spaceship's maximum energy.</param>
    public void UpdateEnergyUI(float currentEnergy, float maxEnergy)
    {
        if (energyText != null)
            energyText.text = $"Energy: {Mathf.CeilToInt(currentEnergy)} / {Mathf.CeilToInt(maxEnergy)}";
    }

    /// <summary>
    /// Updates the health UI text element.
    /// </summary>
    /// <param name="currentHealth">The spaceship's current health.</param>
    /// <param name="maxHealth">The spaceship's maximum health.</param>
    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthText != null)
            healthText.text = $"Health: {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    /// <summary>
    /// Updates the score UI text element.
    /// </summary>
    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    /// <summary>
    /// Updates the time UI text element.
    /// </summary>
    void UpdateTimeUI()
    {
        if (timeText != null)
            timeText.text = $"Time: {Mathf.CeilToInt(timeLeft)}s";
    }

    /// <summary>
    /// Restarts the current game scene.
    /// Resets Time.timeScale to normal before loading the scene.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure time is running normally before scene reload.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}