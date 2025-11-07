using UnityEngine;

/// <summary>
/// Manages global game state including airplane count tracking.
/// Provides singleton access for game-wide systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Statistics")]
    [Tooltip("Total number of airplanes currently active in the scene")]
    [SerializeField] private int totalAirplaneCount = 0;

    [Tooltip("Elapsed game time in seconds")]
    [SerializeField] private float elapsedTime = 0f;

    /// <summary>
    /// Gets the current total number of airplanes in the scene.
    /// </summary>
    public int TotalAirplaneCount => totalAirplaneCount;

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
        // Initialize starting demand for Wood (1 per minute)
        if (DemandManager.Instance != null)
        {
            DemandManager.Instance.AddDemand(ResourceType.Wood, 1f);
            Debug.Log("GameManager: Initialized starting demand - Wood: 1/min");
        }
        else
        {
            Debug.LogWarning("GameManager: DemandManager not found. Cannot initialize starting demand.");
        }
    }

    private void Update()
    {
        // Track elapsed game time
        elapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Registers a newly spawned airplane. Call this when an airplane is created.
    /// </summary>
    public void RegisterAirplane()
    {
        totalAirplaneCount++;
        Debug.Log($"Airplane registered. Total count: {totalAirplaneCount}");
    }

    /// <summary>
    /// Unregisters an airplane that has been destroyed. Call this when an airplane is destroyed.
    /// </summary>
    public void UnregisterAirplane()
    {
        totalAirplaneCount--;
        if (totalAirplaneCount < 0)
        {
            Debug.LogWarning("Airplane count went negative! Resetting to 0.");
            totalAirplaneCount = 0;
        }
        Debug.Log($"Airplane unregistered. Total count: {totalAirplaneCount}");
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
