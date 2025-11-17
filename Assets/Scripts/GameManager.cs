using UnityEngine;

/// <summary>
/// Manages global game state including bee count tracking.
/// Provides singleton access for game-wide systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Statistics")]
    [Tooltip("Total number of bees currently active in the scene")]
    [SerializeField] private int totalBeeCount = 0;

    [Tooltip("Elapsed game time in seconds")]
    [SerializeField] private float elapsedTime = 0f;

    [Header("Time Control")]
    [Tooltip("Current game speed multiplier (1x = normal, 2x = double speed, 5x = fast testing)")]
    [SerializeField] private float currentGameSpeed = 1f;

    /// <summary>
    /// Gets the current total number of bees in the scene.
    /// </summary>
    public int TotalBeeCount => totalBeeCount;

    /// <summary>
    /// Gets the elapsed game time in seconds.
    /// </summary>
    public float ElapsedTime => elapsedTime;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple GameManager instances detected. Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }

        Instance = this;

        // Initialize game speed to normal (always reset on game start)
        SetGameSpeed(1f);
    }

    private void Start()
    {
        // Game initialization
    }

    private void Update()
    {
        // Track elapsed game time
        elapsedTime += Time.deltaTime;

        // Keyboard shortcuts for game speed control (testing purposes)
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SetGameSpeed(1f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SetGameSpeed(3f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SetGameSpeed(10f);
        }
    }

    /// <summary>
    /// Registers a newly spawned bee. Call this when an bee is created.
    /// </summary>
    public void RegisterBee()
    {
        totalBeeCount++;
        Debug.Log($"Bee registered. Total count: {totalBeeCount}");
    }

    /// <summary>
    /// Unregisters an bee that has been destroyed. Call this when an bee is destroyed.
    /// </summary>
    public void UnregisterBee()
    {
        totalBeeCount--;
        if (totalBeeCount < 0)
        {
            Debug.LogWarning("Bee count went negative! Resetting to 0.");
            totalBeeCount = 0;
        }
        Debug.Log($"Bee unregistered. Total count: {totalBeeCount}");
    }

    /// <summary>
    /// Sets the game speed multiplier for testing purposes.
    /// Affects all time-dependent systems via Time.timeScale.
    /// </summary>
    /// <param name="speed">Speed multiplier (e.g., 1f = normal, 2f = double speed, 5f = fast)</param>
    public void SetGameSpeed(float speed)
    {
        // Clamp to reasonable range for safety
        currentGameSpeed = Mathf.Clamp(speed, 0.25f, 10f);
        Time.timeScale = currentGameSpeed;
        Debug.Log($"[GameManager] Game speed set to {currentGameSpeed}x");
    }

    /// <summary>
    /// Resets the game state for a new year playthrough.
    /// Resets all managers, destroys flower patches, and restarts the season cycle.
    /// </summary>
    public void ResetYear()
    {
        Debug.Log("[GameManager] Resetting year for new playthrough...");

        // Reset elapsed time
        elapsedTime = 0f;

        // Reset game speed to normal
        SetGameSpeed(1f);

        // Reset economy manager
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ResetToInitialState();
        }
        else
        {
            Debug.LogWarning("[GameManager] EconomyManager not found during reset");
        }

        // Reset bee fleet manager
        if (BeeFleetManager.Instance != null)
        {
            BeeFleetManager.Instance.ResetToInitialState();
        }
        else
        {
            Debug.LogWarning("[GameManager] BeeFleetManager not found during reset");
        }

        // Reset hive controller (clear inventory)
        if (HiveController.Instance != null)
        {
            HiveController.Instance.ResetInventory();
        }
        else
        {
            Debug.LogWarning("[GameManager] HiveController not found during reset");
        }

        // Destroy all bee GameObjects
        BeeController[] allBees = FindObjectsByType<BeeController>(FindObjectsSortMode.None);
        foreach (var bee in allBees)
        {
            if (bee != null)
            {
                Destroy(bee.gameObject);
            }
        }
        Debug.Log($"[GameManager] Destroyed {allBees.Length} bees");

        // Destroy all flower patch GameObjects
        FlowerPatchController[] allFlowerPatches = FindObjectsByType<FlowerPatchController>(FindObjectsSortMode.None);
        foreach (var flowerPatch in allFlowerPatches)
        {
            if (flowerPatch != null)
            {
                Destroy(flowerPatch.gameObject);
            }
        }
        Debug.Log($"[GameManager] Destroyed {allFlowerPatches.Length} flower patches");

        // Reset year stats tracker
        if (YearStatsTracker.Instance != null)
        {
            YearStatsTracker.Instance.ResetStats();
        }
        else
        {
            Debug.LogWarning("[GameManager] YearStatsTracker not found during reset");
        }

        // Reset recipe progression (unlock/upgrade state)
        if (RecipeProgressionManager.Instance != null)
        {
            RecipeProgressionManager.Instance.ResetToInitialState();
        }
        else
        {
            Debug.LogWarning("[GameManager] RecipeProgressionManager not found during reset");
        }

        // Reset season manager to start new year
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.StartNewYear();
        }
        else
        {
            Debug.LogWarning("[GameManager] SeasonManager not found during reset");
        }

        Debug.Log("[GameManager] Year reset complete - ready for new playthrough!");
    }

    private void OnDestroy()
    {
        // Clean up singleton reference
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
