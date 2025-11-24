using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a route between a flower patch/region and the hive, controlling bee spawning
/// with distance-based spacing. Supports both legacy single-patch and new multi-tile region systems.
/// </summary>
public class RouteController : MonoBehaviour
{
    [Header("Route Configuration")]
    [Tooltip("Reference to the BiomeRegion (required for region-based system)")]
    [SerializeField] private BiomeRegion biomeRegion;

    [Tooltip("Bee prefab to spawn on this route")]
    [SerializeField] private GameObject beePrefab;

    [Header("References")]
    [Tooltip("The home flowerPatch transform (usually this GameObject's transform, fallback for legacy mode)")]
    [SerializeField] private Transform homeFlowerPatch;

    [Tooltip("The hive destination transform")]
    [SerializeField] private Transform hiveDestination;

    [Header("Debug Info")]
    [SerializeField] private int currentBeeCount = 0;
    [SerializeField] private float calculatedSpawnInterval = 0f;
    [SerializeField] private float routeDistance = 0f;

    // Internal state
    private List<GameObject> spawnedBees = new List<GameObject>();
    private float nextSpawnTime;
    private bool hasCompletedInitialSpawning = false;

    private void Awake()
    {
        // Get BiomeRegion reference if not set
        if (biomeRegion == null)
        {
            biomeRegion = GetComponent<BiomeRegion>();
            if (biomeRegion == null)
            {
                Debug.LogError($"RouteController on {gameObject.name}: No BiomeRegion found! RouteController requires BiomeRegion component.", this);
            }
        }

        // Subscribe to biome region events (for capacity upgrades that may affect spacing)
        if (biomeRegion != null)
        {
            biomeRegion.OnCapacityUpgraded.AddListener(OnCapacityUpgraded);
            biomeRegion.OnRegionUnlocked.AddListener(OnFlowerPatchUnlocked);
        }

        // Subscribe to fleet manager bee allocation events
        if (BeeFleetManager.Instance != null)
        {
            BeeFleetManager.Instance.OnBeeAllocationChanged.AddListener(OnBeeAllocationChanged);
        }

        // Default to this GameObject's transform if not set
        if (homeFlowerPatch == null)
        {
            homeFlowerPatch = transform;
        }

        // Find hive if not set
        if (hiveDestination == null && HiveController.Instance != null)
        {
            hiveDestination = HiveController.Instance.transform;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (biomeRegion != null)
        {
            biomeRegion.OnCapacityUpgraded.RemoveListener(OnCapacityUpgraded);
            biomeRegion.OnRegionUnlocked.RemoveListener(OnFlowerPatchUnlocked);
        }

        if (BeeFleetManager.Instance != null)
        {
            BeeFleetManager.Instance.OnBeeAllocationChanged.RemoveListener(OnBeeAllocationChanged);
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

        // Check if region is locked
        bool isLocked = biomeRegion != null && biomeRegion.IsLocked;

        if (isLocked)
        {
            // Disable route until unlocked
            Debug.Log($"RouteController on {gameObject.name}: Region is locked, disabling route until unlocked");
            enabled = false;
            return;
        }

        // Initialize route for unlocked regions
        InitializeRoute();
    }

    /// <summary>
    /// Initializes the route for bee spawning.
    /// Called automatically for unlocked patches/regions, or via OnFlowerPatchUnlocked event.
    /// </summary>
    private void InitializeRoute()
    {
        Debug.Log($"RouteController on {gameObject.name}: Initializing route");

        // Calculate spawn interval based on distance and max bees
        CalculateSpawnInterval();

        // Schedule first spawn immediately
        nextSpawnTime = Time.time;

        // Enable route updates
        enabled = true;
    }

    private void Update()
    {
        // Don't spawn bees if region is locked
        bool isLocked = biomeRegion != null && biomeRegion.IsLocked;

        if (isLocked)
        {
            return;
        }

        int allocatedBees = GetAllocatedBees();

        // Only spawn if we haven't reached allocated count
        if (currentBeeCount < allocatedBees && Time.time >= nextSpawnTime)
        {
            SpawnBee();
            nextSpawnTime = Time.time + calculatedSpawnInterval;

            // Mark as complete when we've spawned all bees
            if (currentBeeCount >= allocatedBees)
            {
                hasCompletedInitialSpawning = true;
            }
        }
    }

    /// <summary>
    /// Gets the number of bees allocated to this route from the global fleet
    /// </summary>
    private int GetAllocatedBees()
    {
        if (biomeRegion == null)
        {
            Debug.LogWarning($"RouteController on {gameObject.name}: biomeRegion is null, defaulting to 0 bees");
            return 0;
        }

        if (BeeFleetManager.Instance == null)
        {
            Debug.LogWarning($"RouteController on {gameObject.name}: BeeFleetManager.Instance is null, defaulting to 0 bees");
            return 0;
        }

        return BeeFleetManager.Instance.GetAllocatedBees(biomeRegion);
    }

    /// <summary>
    /// Validates that all required configuration is present
    /// </summary>
    private bool ValidateConfiguration()
    {
        if (beePrefab == null)
        {
            Debug.LogError($"RouteController on {gameObject.name}: beePrefab is not assigned!", this);
            return false;
        }

        if (hiveDestination == null)
        {
            Debug.LogError($"RouteController on {gameObject.name}: hiveDestination is not assigned! Make sure HiveController exists in scene.", this);
            return false;
        }

        if (biomeRegion == null)
        {
            Debug.LogError($"RouteController on {gameObject.name}: biomeRegion is not assigned!", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates the spawn interval based on route distance, bee speed, and max count.
    /// This ensures bees are evenly spaced along the route.
    /// Uses actual Bezier curve arc length for accurate spacing calculations.
    /// </summary>
    private void CalculateSpawnInterval()
    {
        // Get flower patch/region position (use region center if available, else homeFlowerPatch)
        Vector3 flowerPatchPosition;
        if (biomeRegion != null)
        {
            flowerPatchPosition = biomeRegion.GetRegionCenter() + new Vector3(0f, 0.5f, 0f);
        }
        else
        {
            flowerPatchPosition = homeFlowerPatch.position + new Vector3(0f, 0.5f, 0f);
        }

        Vector3 hiveLanding = hiveDestination.position;
        if (HiveController.Instance != null)
        {
            hiveLanding = HiveController.Instance.LandingPosition;
        }

        // Get flight altitude and speed from bee prefab to ensure accuracy
        float flightAltitude = 12f; // Default fallback
        float beeSpeed = 10f; // Default fallback
        if (beePrefab != null)
        {
            BeeController beeController = beePrefab.GetComponent<BeeController>();
            if (beeController != null)
            {
                flightAltitude = beeController.FlightAltitude;
                beeSpeed = beeController.BaseSpeed;
            }
        }

        // Calculate actual Bezier arc length (not straight-line distance)
        // This matches the actual flight path that bees will follow
        float oneWayArcLength = BeeController.CalculateBezierArcLength(flowerPatchPosition, hiveLanding, flightAltitude);

        // Store for debugging (one-way arc length)
        routeDistance = oneWayArcLength;

        // Calculate round-trip duration (flowerPatch → hive → flowerPatch)
        // Bees complete a full loop, so we need to account for both directions
        float oneWayDuration = oneWayArcLength / beeSpeed;
        float roundTripDuration = oneWayDuration * 2f;

        // Divide round-trip duration by number of bees to get even spacing around the full loop
        // This creates natural spacing: further flowerPatchs = more distance between planes
        int allocatedBees = GetAllocatedBees();

        // Avoid division by zero if no bees are allocated
        if (allocatedBees <= 0)
        {
            calculatedSpawnInterval = 0f;
            Debug.Log($"RouteController on {gameObject.name}: No bees allocated, spawn interval = 0");
            return;
        }

        calculatedSpawnInterval = roundTripDuration / allocatedBees;

        Debug.Log($"RouteController on {gameObject.name}: " +
                  $"ArcLength={oneWayArcLength:F1}u, RoundTripDuration={roundTripDuration:F1}s, " +
                  $"SpawnInterval={calculatedSpawnInterval:F1}s for {allocatedBees} bees");
    }

    /// <summary>
    /// Spawns a new bee and initializes it with route information.
    /// For regions, distributes bees across different hex tiles.
    /// </summary>
    private void SpawnBee()
    {
        // Determine spawn position and home transform
        Vector3 spawnPosition;
        Transform homeTransform;

        if (biomeRegion != null && biomeRegion.HexTileCount > 0)
        {
            // Region-based spawning: distribute across hex tiles
            HexTile hexTile = biomeRegion.GetNextBeeSpawnTile();
            if (hexTile != null)
            {
                spawnPosition = hexTile.BeeGatherPosition;
                homeTransform = hexTile.transform;
            }
            else
            {
                // Fallback to region center if no hex tiles
                Debug.LogWarning($"RouteController on {gameObject.name}: BiomeRegion has no hex tiles, using region center");
                spawnPosition = biomeRegion.GetRegionCenter() + new Vector3(0f, 0.5f, 0f);
                homeTransform = biomeRegion.transform;
            }
        }
        else
        {
            // Legacy single-patch spawning
            spawnPosition = homeFlowerPatch.position + new Vector3(0f, 0.5f, 0f);
            homeTransform = homeFlowerPatch;
        }

        // Instantiate bee
        GameObject beeObject = Instantiate(beePrefab, spawnPosition, Quaternion.identity);
        beeObject.name = $"Bee_{gameObject.name}_{currentBeeCount + 1}";

        // Get bee controller and initialize
        BeeController bee = beeObject.GetComponent<BeeController>();
        if (bee == null)
        {
            Debug.LogError($"Spawned bee prefab does not have BeeController component!", beeObject);
            Destroy(beeObject);
            return;
        }

        // Initialize bee with route information
        bee.Initialize(homeTransform, hiveDestination);

        // Track this bee
        spawnedBees.Add(beeObject);
        currentBeeCount = spawnedBees.Count;

        Debug.Log($"Spawned bee {currentBeeCount}/{GetAllocatedBees()} on route {gameObject.name}");
    }

    /// <summary>
    /// Unregisters a bee from this route (called if bee is destroyed)
    /// </summary>
    public void UnregisterBee(GameObject bee)
    {
        if (spawnedBees.Contains(bee))
        {
            spawnedBees.Remove(bee);
            currentBeeCount = spawnedBees.Count;
        }
    }

    /// <summary>
    /// Cleans up null references from destroyed bees and handles deallocation
    /// </summary>
    private void LateUpdate()
    {
        // Remove any null references (destroyed bees)
        spawnedBees.RemoveAll(plane => plane == null);
        currentBeeCount = spawnedBees.Count;

        int allocatedBees = GetAllocatedBees();

        // If we've completed initial spawning but lost bees, allow respawning
        // This handles cases where bees are destroyed externally
        if (hasCompletedInitialSpawning && currentBeeCount < allocatedBees)
        {
            hasCompletedInitialSpawning = false;
        }

        // If we have more bees than allocated bees, destroy excess
        // This handles bee deallocation
        while (currentBeeCount > allocatedBees && spawnedBees.Count > 0)
        {
            int lastIndex = spawnedBees.Count - 1;
            GameObject excessBee = spawnedBees[lastIndex];
            spawnedBees.RemoveAt(lastIndex);

            if (excessBee != null)
            {
                Destroy(excessBee);
                Debug.Log($"Destroyed excess bee on route {gameObject.name}. Now {spawnedBees.Count}/{allocatedBees}");
            }

            currentBeeCount = spawnedBees.Count;
        }
    }

    /// <summary>
    /// Called when flower patch capacity is upgraded. Recalculates spacing.
    /// </summary>
    private void OnCapacityUpgraded()
    {
        Debug.Log($"RouteController on {gameObject.name}: Capacity upgraded");

        // Recalculate spawn interval (capacity changed, may affect allocation)
        CalculateSpawnInterval();
    }

    /// <summary>
    /// Called when the flower patch/region is unlocked.
    /// Activates the route and begins bee spawning.
    /// </summary>
    private void OnFlowerPatchUnlocked()
    {
        Debug.Log($"RouteController on {gameObject.name}: Unlocked, activating route");
        InitializeRoute();
    }

    /// <summary>
    /// Called when bee allocation changes for this region.
    /// Handles spawning or despawning bees based on new allocation.
    /// </summary>
    /// <param name="region">The region whose allocation changed</param>
    /// <param name="newAllocation">The new bee allocation count</param>
    private void OnBeeAllocationChanged(BiomeRegion region, int newAllocation)
    {
        // Only respond if this is our region
        if (region != biomeRegion)
        {
            return;
        }

        Debug.Log($"RouteController on {gameObject.name}: Bee allocation changed to {newAllocation}");

        // Recalculate spawn interval for new bee count
        CalculateSpawnInterval();

        // If allocation increased, allow spawning more bees
        if (newAllocation > currentBeeCount)
        {
            hasCompletedInitialSpawning = false;
            nextSpawnTime = Time.time; // Spawn immediately
        }

        // If allocation decreased, excess bees will be destroyed in LateUpdate()
    }
}
