using UnityEngine;

/// <summary>
/// Simple placeholder component for manually placed airport spawn points.
/// Shows visual feedback (green=affordable, red=unaffordable) on hover.
/// Spawns actual airport and destroys itself when clicked.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AirportPlaceholder : MonoBehaviour
{
    [Header("Airport Configuration")]
    [Tooltip("What type of biome/resource this airport will produce")]
    [SerializeField] private BiomeType biomeType = BiomeType.Forest;

    [Header("Prefab References")]
    [Tooltip("Airport prefab to spawn when clicked")]
    [SerializeField] private GameObject airportPrefab;

    [Tooltip("Airplane prefab for the spawned airport's route")]
    [SerializeField] private GameObject airplanePrefab;

    [Header("Visual Feedback Materials")]
    [Tooltip("Material when player can afford to build")]
    [SerializeField] private Material affordableMaterial;

    [Tooltip("Material when player cannot afford to build")]
    [SerializeField] private Material unaffordableMaterial;

    [Tooltip("Default material when not hovering")]
    [SerializeField] private Material defaultMaterial;

    [Header("Settings")]
    [Tooltip("Offset for spawned airport position (optional)")]
    [SerializeField] private Vector3 airportSpawnOffset = Vector3.zero;

    // Components
    private MeshRenderer meshRenderer;
    private EconomyManager economyManager;
    private bool isHovering = false;

    private void Awake()
    {
        // Get required local components
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError($"AirportPlaceholder on {gameObject.name}: No MeshRenderer found! Add a MeshRenderer component.", this);
        }

        // Ensure collider exists for mouse events
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"AirportPlaceholder on {gameObject.name}: No Collider found! Mouse events won't work. Adding BoxCollider.", this);
            gameObject.AddComponent<BoxCollider>();
        }
    }

    private void Start()
    {
        // Access external references in Start (after all Awake() calls complete)
        economyManager = EconomyManager.Instance;
        if (economyManager == null)
        {
            Debug.LogError($"AirportPlaceholder on {gameObject.name}: EconomyManager not found in scene!", this);
        }

        // Set default material
        if (meshRenderer != null && defaultMaterial != null)
        {
            meshRenderer.material = defaultMaterial;
        }
    }

    /// <summary>
    /// Called when mouse enters the collider.
    /// Updates material based on affordability.
    /// </summary>
    private void OnMouseEnter()
    {
        isHovering = true;
        UpdateVisualFeedback();
    }

    /// <summary>
    /// Called when mouse exits the collider.
    /// Resets to default material.
    /// </summary>
    private void OnMouseExit()
    {
        isHovering = false;

        if (meshRenderer != null && defaultMaterial != null)
        {
            meshRenderer.material = defaultMaterial;
        }
    }

    /// <summary>
    /// Called when mouse clicks on the collider.
    /// Attempts to spawn airport if affordable.
    /// </summary>
    private void OnMouseDown()
    {
        TryBuildAirport();
    }

    /// <summary>
    /// Updates visual feedback based on whether player can afford the airport.
    /// </summary>
    private void UpdateVisualFeedback()
    {
        if (meshRenderer == null || economyManager == null) return;

        float cost = economyManager.GetAirportPlacementCost(biomeType);
        bool canAfford = economyManager.CanAfford(cost);

        if (canAfford && affordableMaterial != null)
        {
            meshRenderer.material = affordableMaterial;
        }
        else if (!canAfford && unaffordableMaterial != null)
        {
            meshRenderer.material = unaffordableMaterial;
        }
    }

    /// <summary>
    /// Attempts to build an airport at this location.
    /// Checks affordability, spawns airport, and destroys placeholder.
    /// </summary>
    private void TryBuildAirport()
    {
        // Validate references
        if (economyManager == null)
        {
            Debug.LogError($"AirportPlaceholder on {gameObject.name}: EconomyManager is null!", this);
            return;
        }

        if (airportPrefab == null)
        {
            Debug.LogError($"AirportPlaceholder on {gameObject.name}: Airport prefab is not assigned!", this);
            return;
        }

        // Get cost
        float cost = economyManager.GetAirportPlacementCost(biomeType);

        // Check affordability
        if (!economyManager.CanAfford(cost))
        {
            Debug.Log($"Cannot afford {biomeType} airport! Cost: ${cost:F0}, Available: ${economyManager.GetCurrentMoney():F0}");
            return;
        }

        // Deduct money
        if (!economyManager.SpendMoney(cost))
        {
            Debug.LogError($"Failed to spend money for airport (this shouldn't happen)");
            return;
        }

        // Spawn airport
        Vector3 spawnPosition = transform.position + airportSpawnOffset;
        GameObject airport = Instantiate(airportPrefab, spawnPosition, transform.rotation);
        airport.name = $"Airport_{biomeType}_{gameObject.name}";

        // Configure AirportController
        AirportController airportController = airport.GetComponent<AirportController>();
        if (airportController != null)
        {
            airportController.SetBiomeType(biomeType);
        }
        else
        {
            Debug.LogWarning($"Spawned airport prefab does not have AirportController component!");
        }

        // Ensure AirportClickHandler exists and has a collider for mouse events
        AirportClickHandler clickHandler = airport.GetComponent<AirportClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = airport.AddComponent<AirportClickHandler>();
            Debug.Log($"Added AirportClickHandler to spawned airport {airport.name}");
        }

        // Ensure collider exists for mouse events (required for OnMouseEnter/Exit/Down)
        Collider airportCollider = airport.GetComponent<Collider>();
        if (airportCollider == null)
        {
            // Add BoxCollider if no collider exists
            airportCollider = airport.AddComponent<BoxCollider>();
            Debug.Log($"Added BoxCollider to spawned airport {airport.name} for click detection");
        }

        // Configure RouteController
        RouteController routeController = airport.GetComponent<RouteController>();
        if (routeController == null)
        {
            routeController = airport.AddComponent<RouteController>();
        }

        // Set airplane prefab using reflection (if field is private)
        if (airplanePrefab != null)
        {
            var field = typeof(RouteController).GetField("airplanePrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(routeController, airplanePrefab);
            }
        }

        // Register placement with economy manager (for cost scaling)
        economyManager.RegisterAirportPlaced();

        // Add drones to global pool (+3 drones per airport)
        if (DroneFleetManager.Instance != null && airportController != null)
        {
            DroneFleetManager.Instance.AddDronesToPool(3);
            Debug.Log($"Added 3 drones to global pool for new {biomeType} airport");

            // Automatically allocate the 3 drones to this airport's route
            for (int i = 0; i < 3; i++)
            {
                bool allocated = DroneFleetManager.Instance.AllocateDrone(airportController);
                if (!allocated)
                {
                    Debug.LogWarning($"Failed to auto-allocate drone {i + 1}/3 to new airport {airport.name}");
                    break;
                }
            }
            Debug.Log($"Auto-allocated 3 drones to new {biomeType} airport route");
        }
        else
        {
            Debug.LogWarning($"DroneFleetManager or AirportController not found! Drones were not added to pool.");
        }

        Debug.Log($"Built {biomeType} airport at {gameObject.name} for ${cost:F0}");

        // Destroy this placeholder
        Destroy(gameObject);
    }

    /// <summary>
    /// Optional: Update feedback continuously while hovering (in case money changes).
    /// </summary>
    private void Update()
    {
        if (isHovering)
        {
            UpdateVisualFeedback();
        }
    }

    /// <summary>
    /// Gets the biome type for this placeholder (for debugging/UI).
    /// </summary>
    public BiomeType GetBiomeType()
    {
        return biomeType;
    }

    /// <summary>
    /// Gets the cost to build this airport (for debugging/UI).
    /// </summary>
    public float GetCost()
    {
        if (economyManager == null) return 0f;
        return economyManager.GetAirportPlacementCost(biomeType);
    }
}
