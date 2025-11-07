using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a route between an airport and the city, controlling airplane spawning
/// with distance-based spacing. The number of airplanes on the route is fixed,
/// and they are naturally spaced based on the distance between airport and city.
/// </summary>
public class RouteController : MonoBehaviour
{
    [Header("Route Configuration")]
    [Tooltip("Maximum number of airplanes allowed on this route")]
    [SerializeField] private int maxAirplanesOnRoute = 3;

    [Tooltip("Airplane prefab to spawn on this route")]
    [SerializeField] private GameObject airplanePrefab;

    [Tooltip("Speed of airplanes on this route (must match AirplaneController speed)")]
    [SerializeField] private float airplaneSpeed = 10f;

    [Header("References")]
    [Tooltip("The home airport transform (usually this GameObject's transform)")]
    [SerializeField] private Transform homeAirport;

    [Tooltip("The city destination transform")]
    [SerializeField] private Transform cityDestination;

    [Header("Debug Info")]
    [SerializeField] private int currentAirplaneCount = 0;
    [SerializeField] private float calculatedSpawnInterval = 0f;
    [SerializeField] private float routeDistance = 0f;

    // Internal state
    private List<GameObject> spawnedAirplanes = new List<GameObject>();
    private float nextSpawnTime;
    private bool hasCompletedInitialSpawning = false;

    private void Awake()
    {
        // Default to this GameObject's transform if not set
        if (homeAirport == null)
        {
            homeAirport = transform;
        }

        // Find city if not set
        if (cityDestination == null && CityController.Instance != null)
        {
            cityDestination = CityController.Instance.transform;
        }
    }

    private void Start()
    {
        // Validate configuration
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

        // Calculate spawn interval based on distance and max airplanes
        CalculateSpawnInterval();

        // Schedule first spawn immediately
        nextSpawnTime = Time.time;
    }

    private void Update()
    {
        // Only spawn if we haven't reached max count
        if (currentAirplaneCount < maxAirplanesOnRoute && Time.time >= nextSpawnTime)
        {
            SpawnAirplane();
            nextSpawnTime = Time.time + calculatedSpawnInterval;

            // Mark as complete when we've spawned all airplanes
            if (currentAirplaneCount >= maxAirplanesOnRoute)
            {
                hasCompletedInitialSpawning = true;
            }
        }
    }

    /// <summary>
    /// Validates that all required configuration is present
    /// </summary>
    private bool ValidateConfiguration()
    {
        if (airplanePrefab == null)
        {
            Debug.LogError($"RouteController on {gameObject.name}: airplanePrefab is not assigned!", this);
            return false;
        }

        if (homeAirport == null)
        {
            Debug.LogError($"RouteController on {gameObject.name}: homeAirport is not assigned!", this);
            return false;
        }

        if (cityDestination == null)
        {
            Debug.LogError($"RouteController on {gameObject.name}: cityDestination is not assigned! Make sure CityController exists in scene.", this);
            return false;
        }

        if (maxAirplanesOnRoute <= 0)
        {
            Debug.LogError($"RouteController on {gameObject.name}: maxAirplanesOnRoute must be greater than 0!", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates the spawn interval based on route distance, airplane speed, and max count.
    /// This ensures airplanes are evenly spaced along the route.
    /// </summary>
    private void CalculateSpawnInterval()
    {
        // Calculate straight-line distance between airport and city landing position
        Vector3 cityLanding = cityDestination.position;
        if (CityController.Instance != null)
        {
            cityLanding = CityController.Instance.LandingPosition;
        }

        routeDistance = Vector3.Distance(homeAirport.position, cityLanding);

        // Calculate one-way trip duration
        float tripDuration = routeDistance / airplaneSpeed;

        // Divide trip duration by number of airplanes to get even spacing
        // This creates natural spacing: further airports = more distance between planes
        calculatedSpawnInterval = tripDuration / maxAirplanesOnRoute;

        Debug.Log($"RouteController on {gameObject.name}: " +
                  $"Distance={routeDistance:F1}u, TripDuration={tripDuration:F1}s, " +
                  $"SpawnInterval={calculatedSpawnInterval:F1}s for {maxAirplanesOnRoute} airplanes");
    }

    /// <summary>
    /// Spawns a new airplane and initializes it with route information
    /// </summary>
    private void SpawnAirplane()
    {
        // Calculate spawn position (airport position with slight vertical offset)
        Vector3 spawnPosition = homeAirport.position + new Vector3(0f, 0.5f, 0f);

        // Instantiate airplane
        GameObject airplaneObject = Instantiate(airplanePrefab, spawnPosition, Quaternion.identity);
        airplaneObject.name = $"Airplane_{gameObject.name}_{currentAirplaneCount + 1}";

        // Get airplane controller and initialize
        AirplaneController airplane = airplaneObject.GetComponent<AirplaneController>();
        if (airplane == null)
        {
            Debug.LogError($"Spawned airplane prefab does not have AirplaneController component!", airplaneObject);
            Destroy(airplaneObject);
            return;
        }

        // Initialize airplane with route information
        airplane.Initialize(homeAirport, cityDestination);

        // Track this airplane
        spawnedAirplanes.Add(airplaneObject);
        currentAirplaneCount = spawnedAirplanes.Count;

        Debug.Log($"Spawned airplane {currentAirplaneCount}/{maxAirplanesOnRoute} on route {gameObject.name}");
    }

    /// <summary>
    /// Unregisters an airplane from this route (called if airplane is destroyed)
    /// </summary>
    public void UnregisterAirplane(GameObject airplane)
    {
        if (spawnedAirplanes.Contains(airplane))
        {
            spawnedAirplanes.Remove(airplane);
            currentAirplaneCount = spawnedAirplanes.Count;
        }
    }

    /// <summary>
    /// Cleans up null references from destroyed airplanes
    /// </summary>
    private void LateUpdate()
    {
        // Remove any null references (destroyed airplanes)
        spawnedAirplanes.RemoveAll(plane => plane == null);
        currentAirplaneCount = spawnedAirplanes.Count;

        // If we've completed initial spawning but lost airplanes, allow respawning
        // This handles cases where airplanes are destroyed externally
        if (hasCompletedInitialSpawning && currentAirplaneCount < maxAirplanesOnRoute)
        {
            hasCompletedInitialSpawning = false;
        }
    }

    // private void OnDrawGizmos()
    // {
    //     // Visualize route in Scene view
    //     if (homeAirport != null && cityDestination != null)
    //     {
    //         Gizmos.color = new Color(255f/255f, 187f/255f, 0f/255f, 0.3f); // #ffbb00 semi-transparent
    //
    //         Vector3 cityLanding = cityDestination.position;
    //         if (CityController.Instance != null)
    //         {
    //             cityLanding = CityController.Instance.LandingPosition;
    //         }
    //
    //         Gizmos.DrawLine(homeAirport.position + Vector3.up * 0.5f, cityLanding);
    //     }
    // }
}
