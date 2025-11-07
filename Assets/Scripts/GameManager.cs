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

    /// <summary>
    /// Gets the current total number of airplanes in the scene.
    /// </summary>
    public int TotalAirplaneCount => totalAirplaneCount;

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
