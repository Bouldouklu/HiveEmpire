using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a route between an airport and the city, controlling airplane spawning
/// with distance-based spacing. The number of airplanes on the route is dynamically
/// determined by the airport's upgrade tier.
/// </summary>
public class RouteController : MonoBehaviour
{
    [Header("Route Configuration")]
    [Tooltip("Reference to the AirportController (automatically set in Awake if on same GameObject)")]
    [SerializeField] private AirportController airportController;

    [Tooltip("Airplane prefab to spawn on this route")]
    [SerializeField] private GameObject airplanePrefab;

    [Tooltip("Speed of airplanes on this route (must match AirplaneController speed)")]
    [SerializeField] private float airplaneSpeed = 10f;

    [Tooltip("Cruising altitude of airplanes (must match AirplaneController cruisingAltitude)")]
    [SerializeField] private float cruisingAltitude = 12f;

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
        // Get AirportController reference if not set
        if (airportController == null)
        {
            airportController = GetComponent<AirportController>();
            if (airportController == null)
            {
                Debug.LogError($"RouteController on {gameObject.name}: No AirportController found! RouteController requires AirportController on same GameObject.", this);
            }
        }

        // Subscribe to airport events (for capacity upgrades that may affect spacing)
        if (airportController != null)
        {
            airportController.OnAirportUpgraded.AddListener(OnAirportUpgraded);
            airportController.OnCapacityUpgraded.AddListener(OnCapacityUpgraded);
        }

        // Subscribe to fleet manager drone allocation events
        if (DroneFleetManager.Instance != null)
        {
            DroneFleetManager.Instance.OnDroneAllocationChanged.AddListener(OnDroneAllocationChanged);
        }

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

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (airportController != null)
        {
            airportController.OnAirportUpgraded.RemoveListener(OnAirportUpgraded);
            airportController.OnCapacityUpgraded.RemoveListener(OnCapacityUpgraded);
        }

        if (DroneFleetManager.Instance != null)
        {
            DroneFleetManager.Instance.OnDroneAllocationChanged.RemoveListener(OnDroneAllocationChanged);
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
        int allocatedDrones = GetAllocatedDrones();

        // Only spawn if we haven't reached allocated count
        if (currentAirplaneCount < allocatedDrones && Time.time >= nextSpawnTime)
        {
            SpawnAirplane();
            nextSpawnTime = Time.time + calculatedSpawnInterval;

            // Mark as complete when we've spawned all drones
            if (currentAirplaneCount >= allocatedDrones)
            {
                hasCompletedInitialSpawning = true;
            }
        }
    }

    /// <summary>
    /// Gets the number of drones allocated to this route from the global fleet
    /// </summary>
    private int GetAllocatedDrones()
    {
        if (airportController == null)
        {
            Debug.LogWarning($"RouteController on {gameObject.name}: airportController is null, defaulting to 0 drones");
            return 0;
        }

        if (DroneFleetManager.Instance == null)
        {
            Debug.LogWarning($"RouteController on {gameObject.name}: DroneFleetManager.Instance is null, defaulting to 0 drones");
            return 0;
        }

        return DroneFleetManager.Instance.GetAllocatedDrones(airportController);
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

        if (airportController == null)
        {
            Debug.LogError($"RouteController on {gameObject.name}: airportController is not assigned!", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates the spawn interval based on route distance, airplane speed, and max count.
    /// This ensures airplanes are evenly spaced along the route.
    /// Uses actual Bezier curve arc length for accurate spacing calculations.
    /// </summary>
    private void CalculateSpawnInterval()
    {
        // Get airport and city landing positions
        Vector3 airportPosition = homeAirport.position + new Vector3(0f, 0.5f, 0f);
        Vector3 cityLanding = cityDestination.position;
        if (CityController.Instance != null)
        {
            cityLanding = CityController.Instance.LandingPosition;
        }

        // Calculate actual Bezier arc length (not straight-line distance)
        // This matches the actual flight path that airplanes will follow
        float oneWayArcLength = AirplaneController.CalculateBezierArcLength(airportPosition, cityLanding, cruisingAltitude);

        // Store for debugging (one-way arc length)
        routeDistance = oneWayArcLength;

        // Calculate round-trip duration (airport → city → airport)
        // Airplanes complete a full loop, so we need to account for both directions
        float oneWayDuration = oneWayArcLength / airplaneSpeed;
        float roundTripDuration = oneWayDuration * 2f;

        // Divide round-trip duration by number of drones to get even spacing around the full loop
        // This creates natural spacing: further airports = more distance between planes
        int allocatedDrones = GetAllocatedDrones();

        // Avoid division by zero if no drones are allocated
        if (allocatedDrones <= 0)
        {
            calculatedSpawnInterval = 0f;
            Debug.Log($"RouteController on {gameObject.name}: No drones allocated, spawn interval = 0");
            return;
        }

        calculatedSpawnInterval = roundTripDuration / allocatedDrones;

        Debug.Log($"RouteController on {gameObject.name}: " +
                  $"ArcLength={oneWayArcLength:F1}u, RoundTripDuration={roundTripDuration:F1}s, " +
                  $"SpawnInterval={calculatedSpawnInterval:F1}s for {allocatedDrones} drones");
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

        Debug.Log($"Spawned drone {currentAirplaneCount}/{GetAllocatedDrones()} on route {gameObject.name}");
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
    /// Cleans up null references from destroyed airplanes and handles deallocation
    /// </summary>
    private void LateUpdate()
    {
        // Remove any null references (destroyed airplanes)
        spawnedAirplanes.RemoveAll(plane => plane == null);
        currentAirplaneCount = spawnedAirplanes.Count;

        int allocatedDrones = GetAllocatedDrones();

        // If we've completed initial spawning but lost airplanes, allow respawning
        // This handles cases where airplanes are destroyed externally
        if (hasCompletedInitialSpawning && currentAirplaneCount < allocatedDrones)
        {
            hasCompletedInitialSpawning = false;
        }

        // If we have more airplanes than allocated drones, destroy excess
        // This handles drone deallocation
        while (currentAirplaneCount > allocatedDrones && spawnedAirplanes.Count > 0)
        {
            int lastIndex = spawnedAirplanes.Count - 1;
            GameObject excessAirplane = spawnedAirplanes[lastIndex];
            spawnedAirplanes.RemoveAt(lastIndex);

            if (excessAirplane != null)
            {
                Destroy(excessAirplane);
                Debug.Log($"Destroyed excess drone on route {gameObject.name}. Now {spawnedAirplanes.Count}/{allocatedDrones}");
            }

            currentAirplaneCount = spawnedAirplanes.Count;
        }
    }

    /// <summary>
    /// Called when the airport is upgraded. Recalculates spacing.
    /// Note: Upgrades now add drones to global pool, not directly to this route.
    /// </summary>
    /// <param name="newTier">The new tier level (1-3)</param>
    private void OnAirportUpgraded(int newTier)
    {
        Debug.Log($"RouteController on {gameObject.name}: Airport upgraded to tier {newTier}");

        // Recalculate spawn interval (allocation may have changed)
        CalculateSpawnInterval();
    }

    /// <summary>
    /// Called when airport capacity is upgraded. Recalculates spacing.
    /// </summary>
    private void OnCapacityUpgraded()
    {
        Debug.Log($"RouteController on {gameObject.name}: Airport capacity upgraded");

        // Recalculate spawn interval (capacity changed, may affect allocation)
        CalculateSpawnInterval();
    }

    /// <summary>
    /// Called when drone allocation changes for this airport.
    /// Handles spawning or despawning drones based on new allocation.
    /// </summary>
    /// <param name="airport">The airport whose allocation changed</param>
    /// <param name="newAllocation">The new drone allocation count</param>
    private void OnDroneAllocationChanged(AirportController airport, int newAllocation)
    {
        // Only respond if this is our airport
        if (airport != airportController)
        {
            return;
        }

        Debug.Log($"RouteController on {gameObject.name}: Drone allocation changed to {newAllocation}");

        // Recalculate spawn interval for new drone count
        CalculateSpawnInterval();

        // If allocation increased, allow spawning more drones
        if (newAllocation > currentAirplaneCount)
        {
            hasCompletedInitialSpawning = false;
            nextSpawnTime = Time.time; // Spawn immediately
        }

        // If allocation decreased, excess drones will be destroyed in LateUpdate()
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
