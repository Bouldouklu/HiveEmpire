using UnityEngine;

/// <summary>
/// Simple placeholder component for manually placed flower patch spawn points.
/// Shows visual feedback (green=affordable, red=unaffordable) on hover.
/// Spawns actual flower patch and destroys itself when clicked.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FlowerPatchPlaceholder : MonoBehaviour
{
    [Header("Flower Patch Configuration")]
    [Tooltip("ScriptableObject containing all configuration for this flower patch")]
    [SerializeField] private FlowerPatchData flowerPatchData;

    [Tooltip("Bee prefab for the spawned flower patch's pollen route")]
    [SerializeField] private GameObject beePrefab;

    [Header("Visual Feedback Materials")]
    [Tooltip("Material when player can afford to build")]
    [SerializeField] private Material affordableMaterial;

    [Tooltip("Material when player cannot afford to build")]
    [SerializeField] private Material unaffordableMaterial;

    [Tooltip("Default material when not hovering")]
    [SerializeField] private Material defaultMaterial;

    [Header("Settings")]
    [Tooltip("Offset for spawned flower patch position (optional)")]
    [SerializeField] private Vector3 flowerPatchSpawnOffset = Vector3.zero;

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
            Debug.LogError($"FlowerPatchPlaceholder on {gameObject.name}: No MeshRenderer found! Add a MeshRenderer component.", this);
        }

        // Ensure collider exists for mouse events
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"FlowerPatchPlaceholder on {gameObject.name}: No Collider found! Mouse events won't work. Adding BoxCollider.", this);
            gameObject.AddComponent<BoxCollider>();
        }
    }

    private void Start()
    {
        // Access external references in Start (after all Awake() calls complete)
        economyManager = EconomyManager.Instance;
        if (economyManager == null)
        {
            Debug.LogError($"FlowerPatchPlaceholder on {gameObject.name}: EconomyManager not found in scene!", this);
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
    /// Attempts to spawn flower patch if affordable.
    /// </summary>
    private void OnMouseDown()
    {
        TryBuildFlowerPatch();
    }

    /// <summary>
    /// Updates visual feedback based on whether player can afford the flower patch.
    /// </summary>
    private void UpdateVisualFeedback()
    {
        if (meshRenderer == null || economyManager == null || flowerPatchData == null) return;

        float cost = flowerPatchData.placementCost;
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
    /// Attempts to build a flower patch at this location.
    /// Checks affordability, spawns flower patch, and destroys placeholder.
    /// </summary>
    private void TryBuildFlowerPatch()
    {
        // Validate references
        if (economyManager == null)
        {
            Debug.LogError($"FlowerPatchPlaceholder on {gameObject.name}: EconomyManager is null!", this);
            return;
        }

        if (flowerPatchData == null)
        {
            Debug.LogError($"FlowerPatchPlaceholder on {gameObject.name}: Flower patch data is not assigned!", this);
            return;
        }

        if (flowerPatchData.flowerPatchPrefab == null)
        {
            Debug.LogError($"FlowerPatchPlaceholder on {gameObject.name}: Flower patch prefab is not assigned in FlowerPatchData!", this);
            return;
        }

        // Get cost from FlowerPatchData
        float cost = flowerPatchData.placementCost;

        // Check affordability
        if (!economyManager.CanAfford(cost))
        {
            Debug.Log($"Cannot afford {flowerPatchData.displayName}! Cost: ${cost:F0}, Available: ${economyManager.GetCurrentMoney():F0}");
            return;
        }

        // Deduct money
        if (!economyManager.SpendMoney(cost))
        {
            Debug.LogError($"Failed to spend money for flower patch (this shouldn't happen)");
            return;
        }

        // Spawn flower patch
        Vector3 spawnPosition = transform.position + flowerPatchSpawnOffset;
        GameObject flowerPatch = Instantiate(flowerPatchData.flowerPatchPrefab, spawnPosition, transform.rotation);
        flowerPatch.name = $"FlowerPatch_{flowerPatchData.biomeType}_{gameObject.name}";

        // Configure FlowerPatchController with FlowerPatchData
        FlowerPatchController flowerPatchController = flowerPatch.GetComponent<FlowerPatchController>();
        if (flowerPatchController != null)
        {
            flowerPatchController.InitializeFromData(flowerPatchData);
        }
        else
        {
            Debug.LogWarning($"Spawned flower patch prefab does not have FlowerPatchController component!");
        }

        // Ensure FlowerPatchClickHandler exists and has a collider for mouse events
        FlowerPatchClickHandler clickHandler = flowerPatch.GetComponent<FlowerPatchClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = flowerPatch.AddComponent<FlowerPatchClickHandler>();
            Debug.Log($"Added FlowerPatchClickHandler to spawned flower patch {flowerPatch.name}");
        }

        // Ensure collider exists for mouse events (required for OnMouseEnter/Exit/Down)
        Collider flowerPatchCollider = flowerPatch.GetComponent<Collider>();
        if (flowerPatchCollider == null)
        {
            // Add BoxCollider if no collider exists
            flowerPatchCollider = flowerPatch.AddComponent<BoxCollider>();
            Debug.Log($"Added BoxCollider to spawned flower patch {flowerPatch.name} for click detection");
        }

        // Configure RouteController
        RouteController routeController = flowerPatch.GetComponent<RouteController>();
        if (routeController == null)
        {
            routeController = flowerPatch.AddComponent<RouteController>();
        }

        // Set bee prefab using reflection (if field is private)
        if (beePrefab != null)
        {
            var field = typeof(RouteController).GetField("beePrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(routeController, beePrefab);
            }
        }

        // Register with AudioManager for sound events
        if (AudioManager.Instance != null && flowerPatchController != null)
        {
            AudioManager.Instance.RegisterFlowerPatch(flowerPatchController);
        }

        // Play unlock sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFlowerPatchUnlockSound();
        }

        Debug.Log($"Built {flowerPatchData.displayName} at {gameObject.name} for ${cost:F0}");

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
        return flowerPatchData != null ? flowerPatchData.biomeType : BiomeType.WildMeadow;
    }

    /// <summary>
    /// Gets the cost to build this flower patch (for debugging/UI).
    /// </summary>
    public float GetCost()
    {
        return flowerPatchData != null ? flowerPatchData.placementCost : 0f;
    }

    /// <summary>
    /// Gets the FlowerPatchData for this placeholder (for debugging/UI).
    /// </summary>
    public FlowerPatchData GetFlowerPatchData()
    {
        return flowerPatchData;
    }
}
