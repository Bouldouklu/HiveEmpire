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
    }

    private void Start()
    {
        // Game initialization
    }

    private void Update()
    {
        // Track elapsed game time
        elapsedTime += Time.deltaTime;
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

    private void OnDestroy()
    {
        // Clean up singleton reference
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
