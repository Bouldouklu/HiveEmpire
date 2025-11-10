using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls the central hive where bees deliver pollen.
/// Tracks resource inventory and fires events when resources are received.
/// Provides a singleton instance for easy access by bees and flower patches.
/// </summary>
public class HiveController : MonoBehaviour
{
    public static HiveController Instance { get; private set; }

    [Header("Hive Settings")]
    [Tooltip("Position where bees should arrive")]
    [SerializeField] private Vector3 landingOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Resource Tracking")]
    [Tooltip("Current resource inventory (for debugging)")]
    [SerializeField] private Dictionary<ResourceType, int> resourceInventory = new Dictionary<ResourceType, int>();

    [Header("Storage Settings")]
    [Tooltip("Storage capacity per pollen type (upgradeable)")]
    [SerializeField] private int defaultStorageCapacity = 100;

    private Dictionary<ResourceType, int> storageCapacities = new Dictionary<ResourceType, int>();

    /// <summary>
    /// Event fired when resources are delivered to the hive
    /// </summary>
    public UnityEvent OnResourcesChanged = new UnityEvent();

    /// <summary>
    /// Event fired when pollen is discarded due to full storage (passes resource type and amount discarded)
    /// </summary>
    public UnityEvent<ResourceType, int> OnPollenDiscarded = new UnityEvent<ResourceType, int>();

    /// <summary>
    /// The position where bees should aim to arrive
    /// </summary>
    public Vector3 LandingPosition => transform.position + landingOffset;

    /// <summary>
    /// Receives pollen delivered by a bee.
    /// Adds to inventory up to storage capacity. Overflow is discarded.
    /// </summary>
    public void ReceiveResources(List<ResourceType> resources)
    {
        if (resources == null || resources.Count == 0)
        {
            return;
        }

        int totalReceived = 0;
        int totalDiscarded = 0;

        // Add each resource to inventory, respecting storage caps
        foreach (ResourceType resource in resources)
        {
            // Initialize inventory and capacity if needed
            if (!resourceInventory.ContainsKey(resource))
            {
                resourceInventory[resource] = 0;
            }

            int currentAmount = resourceInventory[resource];
            int capacity = GetStorageCapacity(resource);

            // Check if we can add this pollen
            if (currentAmount < capacity)
            {
                resourceInventory[resource]++;
                totalReceived++;
            }
            else
            {
                // Storage full - discard overflow
                totalDiscarded++;
                OnPollenDiscarded?.Invoke(resource, 1);
                Debug.LogWarning($"Storage full for {resource}! Discarded 1 pollen. Current: {currentAmount}/{capacity}");
            }
        }

        if (totalReceived > 0)
        {
            // Debug.Log($"Hive received {totalReceived} pollen ({totalDiscarded} discarded). New totals: " + GetResourceSummary());
            // Fire event to notify UI
            OnResourcesChanged?.Invoke();
        }
    }

    /// <summary>
    /// Try to consume resources for a recipe. Returns true if successful.
    /// Used by RecipeProductionManager to start recipe production.
    /// </summary>
    public bool TryConsumeResources(HoneyRecipe recipe)
    {
        if (recipe == null)
            return false;

        // Check if we have enough of each ingredient
        foreach (var ingredient in recipe.ingredients)
        {
            if (GetResourceCount(ingredient.pollenType) < ingredient.quantity)
            {
                return false;
            }
        }

        // Consume the resources
        foreach (var ingredient in recipe.ingredients)
        {
            resourceInventory[ingredient.pollenType] -= ingredient.quantity;
        }

        Debug.Log($"Consumed resources for recipe: {recipe.recipeName}");
        OnResourcesChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Get a copy of the current pollen inventory.
    /// Used by RecipeProductionManager for allocation checks.
    /// </summary>
    public Dictionary<ResourceType, int> GetPollenInventory()
    {
        return new Dictionary<ResourceType, int>(resourceInventory);
    }

    /// <summary>
    /// Get the storage capacity for a specific resource type.
    /// Can be upgraded in the future.
    /// </summary>
    public int GetStorageCapacity(ResourceType resourceType)
    {
        // Check if this resource has a custom capacity
        if (storageCapacities.ContainsKey(resourceType))
        {
            return storageCapacities[resourceType];
        }

        // Return default capacity
        return defaultStorageCapacity;
    }

    /// <summary>
    /// Set the storage capacity for a specific resource type.
    /// Used for upgrades.
    /// </summary>
    public void SetStorageCapacity(ResourceType resourceType, int capacity)
    {
        storageCapacities[resourceType] = Mathf.Max(1, capacity);
        Debug.Log($"Updated storage capacity for {resourceType}: {capacity}");
    }

    /// <summary>
    /// Upgrade storage capacity for a resource type by a specific amount.
    /// </summary>
    public void UpgradeStorageCapacity(ResourceType resourceType, int additionalCapacity)
    {
        int currentCapacity = GetStorageCapacity(resourceType);
        SetStorageCapacity(resourceType, currentCapacity + additionalCapacity);
    }

    /// <summary>
    /// Gets the count of a specific resource type in inventory.
    /// </summary>
    public int GetResourceCount(ResourceType resourceType)
    {
        return resourceInventory.ContainsKey(resourceType) ? resourceInventory[resourceType] : 0;
    }

    /// <summary>
    /// Gets a formatted string of all resources (for debugging).
    /// </summary>
    private string GetResourceSummary()
    {
        string summary = "";
        foreach (var kvp in resourceInventory)
        {
            summary += $"{kvp.Key}: {kvp.Value}, ";
        }
        return summary.TrimEnd(',', ' ');
    }

    private void Awake()
    {
        // Singleton pattern - only one hive should exist
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple HiveController instances detected. Destroying duplicate on {gameObject.name}");
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
