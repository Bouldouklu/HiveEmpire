using UnityEngine;

/// <summary>
/// Controls the central city hub where airplanes deliver resources.
/// Provides a singleton instance for easy access by airplanes and airports.
/// </summary>
public class CityController : MonoBehaviour
{
    public static CityController Instance { get; private set; }

    [Header("City Settings")]
    [Tooltip("Position where airplanes should land")]
    [SerializeField] private Vector3 landingOffset = new Vector3(0f, 0.5f, 0f);

    /// <summary>
    /// The position where airplanes should aim to land
    /// </summary>
    public Vector3 LandingPosition => transform.position + landingOffset;

    private void Awake()
    {
        // Singleton pattern - only one city should exist
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple CityController instances detected. Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // Clean up singleton reference
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize landing position in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LandingPosition, 1f);
    }
}
