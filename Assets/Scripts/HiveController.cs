using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Serializable class for storing pollen inventory data.
/// Uses FlowerPatchData ScriptableObject references as keys.
/// </summary>
[System.Serializable]
public class PollenInventorySlot
{
    [Tooltip("The flower patch data that defines this pollen type")]
    public FlowerPatchData pollenType;

    [Tooltip("Amount of this pollen type in inventory")]
    public int quantity;

    public PollenInventorySlot(FlowerPatchData pollenType, int quantity)
    {
        this.pollenType = pollenType;
        this.quantity = quantity;
    }
}

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
    [Tooltip("Current pollen inventory slots (serializable)")]
    [SerializeField] private List<PollenInventorySlot> pollenInventory = new List<PollenInventorySlot>();

    [Header("Storage Settings")]
    [Tooltip("Base storage capacity per pollen type (before seasonal modifiers)")]
    [SerializeField] private int baseStorageCapacity = 100;

    [Tooltip("Current default storage capacity (base * seasonal modifier)")]
    [SerializeField] private int defaultStorageCapacity = 100;

    private List<PollenInventorySlot> storageCapacities = new List<PollenInventorySlot>();

    /// <summary>
    /// Event fired when resources are delivered to the hive
    /// </summary>
    public UnityEvent OnResourcesChanged = new UnityEvent();

    /// <summary>
    /// Event fired when pollen is discarded due to full storage (passes flower patch data and amount discarded)
    /// </summary>
    public UnityEvent<FlowerPatchData, int> OnPollenDiscarded = new UnityEvent<FlowerPatchData, int>();

    /// <summary>
    /// The position where bees should aim to arrive
    /// </summary>
    public Vector3 LandingPosition => transform.position + landingOffset;

    /// <summary>
    /// Receives pollen delivered by a bee.
    /// Adds to inventory up to storage capacity. Overflow is discarded.
    /// </summary>
    public void ReceiveResources(List<FlowerPatchData> resources)
    {
        if (resources == null || resources.Count == 0)
        {
            return;
        }

        int totalReceived = 0;
        int totalDiscarded = 0;

        // Add each resource to inventory, respecting storage caps
        foreach (FlowerPatchData patchData in resources)
        {
            if (patchData == null)
            {
                Debug.LogWarning("Received null FlowerPatchData in ReceiveResources - skipping");
                continue;
            }

            // Find or create inventory slot
            PollenInventorySlot slot = pollenInventory.FirstOrDefault(s => s.pollenType == patchData);
            if (slot == null)
            {
                slot = new PollenInventorySlot(patchData, 0);
                pollenInventory.Add(slot);
            }

            int currentAmount = slot.quantity;
            int capacity = GetStorageCapacity(patchData);

            // Check if we can add this pollen
            if (currentAmount < capacity)
            {
                slot.quantity++;
                totalReceived++;
            }
            else
            {
                // Storage full - discard overflow
                totalDiscarded++;
                OnPollenDiscarded?.Invoke(patchData, 1);
                Debug.LogWarning($"Storage full for {patchData.pollenDisplayName}! Discarded 1 pollen. Current: {currentAmount}/{capacity}");
            }
        }

        if (totalReceived > 0)
        {
            // Fire event to notify UI
            OnResourcesChanged?.Invoke();

            // Track resources for year stats
            if (YearStatsTracker.Instance != null)
            {
                YearStatsTracker.Instance.RecordResourcesCollected(resources);
            }
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

        return TryConsumeResources(recipe.ingredients);
    }

    /// <summary>
    /// Try to consume a specific list of ingredients. Returns true if successful.
    /// Used by RecipeProductionManager to support tier-adjusted recipes.
    /// </summary>
    public bool TryConsumeResources(List<HoneyRecipe.Ingredient> ingredients)
    {
        if (ingredients == null)
            return false;

        // Check if we have enough of each ingredient
        foreach (var ingredient in ingredients)
        {
            if (ingredient.pollenType == null)
            {
                Debug.LogWarning("Ingredient has null pollenType - cannot consume");
                return false;
            }

            if (GetResourceCount(ingredient.pollenType) < ingredient.quantity)
            {
                return false;
            }
        }

        // Consume the resources
        foreach (var ingredient in ingredients)
        {
            PollenInventorySlot slot = pollenInventory.FirstOrDefault(s => s.pollenType == ingredient.pollenType);
            if (slot != null)
            {
                slot.quantity -= ingredient.quantity;
            }
        }

        Debug.Log($"Consumed resources from hive inventory");
        OnResourcesChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Get a read-only copy of the current pollen inventory.
    /// Used by RecipeProductionManager and UI systems.
    /// </summary>
    public List<PollenInventorySlot> GetPollenInventory()
    {
        // Return a shallow copy (slots are reference types, so changes to quantity will reflect)
        return new List<PollenInventorySlot>(pollenInventory);
    }

    /// <summary>
    /// Get a dictionary representation of pollen inventory for easier lookup.
    /// Maps FlowerPatchData to quantity.
    /// </summary>
    public Dictionary<FlowerPatchData, int> GetPollenInventoryDictionary()
    {
        return pollenInventory.ToDictionary(slot => slot.pollenType, slot => slot.quantity);
    }

    /// <summary>
    /// Get the storage capacity for a specific pollen type (FlowerPatchData).
    /// Can be upgraded in the future.
    /// </summary>
    public int GetStorageCapacity(FlowerPatchData pollenType)
    {
        if (pollenType == null)
            return defaultStorageCapacity;

        // Check if this pollen type has a custom capacity
        PollenInventorySlot capacitySlot = storageCapacities.FirstOrDefault(s => s.pollenType == pollenType);
        if (capacitySlot != null)
        {
            return capacitySlot.quantity;
        }

        // Return default capacity
        return defaultStorageCapacity;
    }

    /// <summary>
    /// Set the storage capacity for a specific pollen type.
    /// Used for upgrades.
    /// </summary>
    public void SetStorageCapacity(FlowerPatchData pollenType, int capacity)
    {
        if (pollenType == null)
        {
            Debug.LogWarning("Cannot set storage capacity for null pollenType");
            return;
        }

        PollenInventorySlot capacitySlot = storageCapacities.FirstOrDefault(s => s.pollenType == pollenType);
        if (capacitySlot == null)
        {
            capacitySlot = new PollenInventorySlot(pollenType, 0);
            storageCapacities.Add(capacitySlot);
        }

        capacitySlot.quantity = Mathf.Max(1, capacity);
        Debug.Log($"Updated storage capacity for {pollenType.pollenDisplayName}: {capacity}");
    }

    /// <summary>
    /// Upgrade storage capacity for a pollen type by a specific amount.
    /// </summary>
    public void UpgradeStorageCapacity(FlowerPatchData pollenType, int additionalCapacity)
    {
        int currentCapacity = GetStorageCapacity(pollenType);
        SetStorageCapacity(pollenType, currentCapacity + additionalCapacity);
    }

    /// <summary>
    /// Gets the count of a specific pollen type in inventory.
    /// </summary>
    public int GetResourceCount(FlowerPatchData pollenType)
    {
        if (pollenType == null)
            return 0;

        PollenInventorySlot slot = pollenInventory.FirstOrDefault(s => s.pollenType == pollenType);
        return slot != null ? slot.quantity : 0;
    }

    /// <summary>
    /// Gets a formatted string of all resources (for debugging).
    /// </summary>
    private string GetResourceSummary()
    {
        string summary = "";
        foreach (var slot in pollenInventory)
        {
            if (slot.pollenType != null)
            {
                summary += $"{slot.pollenType.pollenDisplayName}: {slot.quantity}, ";
            }
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

        // Subscribe to season changes for storage capacity modifiers
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.AddListener(OnSeasonChanged);
            // Apply initial seasonal modifier
            ApplySeasonalStorageModifier();
        }
    }

    /// <summary>
    /// Called when the season changes to update storage capacity
    /// </summary>
    private void OnSeasonChanged(Season newSeason)
    {
        ApplySeasonalStorageModifier();
    }

    /// <summary>
    /// Applies the current seasonal storage capacity modifier
    /// </summary>
    private void ApplySeasonalStorageModifier()
    {
        if (SeasonManager.Instance == null)
        {
            defaultStorageCapacity = baseStorageCapacity;
            return;
        }

        SeasonData currentSeason = SeasonManager.Instance.GetCurrentSeasonData();
        if (currentSeason == null)
        {
            defaultStorageCapacity = baseStorageCapacity;
            return;
        }

        // Apply seasonal storage capacity modifier
        defaultStorageCapacity = Mathf.RoundToInt(baseStorageCapacity * currentSeason.storageCapacityModifier);

        Debug.Log($"[HiveController] Applied seasonal storage modifier: {baseStorageCapacity} * {currentSeason.storageCapacityModifier} = {defaultStorageCapacity}");
    }

    private void OnDestroy()
    {
        // Clean up singleton reference
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from season changes
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.RemoveListener(OnSeasonChanged);
        }
    }

    /// <summary>
    /// Reset inventory to initial state for new year playthrough.
    /// Clears all resources and resets storage capacities.
    /// </summary>
    public void ResetInventory()
    {
        pollenInventory.Clear();
        storageCapacities.Clear();
        defaultStorageCapacity = baseStorageCapacity;

        // Reapply seasonal modifiers if needed
        ApplySeasonalStorageModifier();

        OnResourcesChanged?.Invoke();

        Debug.Log("[HiveController] Inventory reset - all resources cleared");
    }

    private void OnDrawGizmos()
    {
        // Visualize landing position in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LandingPosition, 1f);
    }
}
