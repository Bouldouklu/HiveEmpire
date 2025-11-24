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

    [Tooltip("Amount of this pollen type in inventory (supports decimals for early game)")]
    public float quantity;

    public PollenInventorySlot(FlowerPatchData pollenType, float quantity)
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

    /// <summary>
    /// Event fired when resources are delivered to the hive
    /// </summary>
    public UnityEvent OnResourcesChanged = new UnityEvent();

    /// <summary>
    /// The position where bees should aim to arrive
    /// </summary>
    public Vector3 LandingPosition => transform.position + landingOffset;

    /// <summary>
    /// Receives pollen delivered by a bee.
    /// Adds to inventory with unlimited capacity.
    /// </summary>
    public void ReceiveResources(List<FlowerPatchData> resources)
    {
        if (resources == null || resources.Count == 0)
        {
            return;
        }

        int totalReceived = 0;

        // Add each resource to inventory
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

            // Add pollen without capacity check
            slot.quantity++;
            totalReceived++;
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
    public Dictionary<FlowerPatchData, float> GetPollenInventoryDictionary()
    {
        return pollenInventory.ToDictionary(slot => slot.pollenType, slot => slot.quantity);
    }


    /// <summary>
    /// Gets the count of a specific pollen type in inventory.
    /// </summary>
    public float GetResourceCount(FlowerPatchData pollenType)
    {
        if (pollenType == null)
            return 0f;

        PollenInventorySlot slot = pollenInventory.FirstOrDefault(s => s.pollenType == pollenType);
        return slot != null ? slot.quantity : 0f;
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
    }

    private void OnDestroy()
    {
        // Clean up singleton reference
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Reset inventory to initial state for new year playthrough.
    /// Clears all resources.
    /// </summary>
    public void ResetInventory()
    {
        pollenInventory.Clear();

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
