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
    /// Updates resource inventory, earns money, and fires OnResourcesChanged event.
    /// </summary>
    public void ReceiveResources(List<ResourceType> resources)
    {
        if (resources == null || resources.Count == 0)
        {
            return;
        }

        // Add each resource to inventory and record deliveries
        foreach (ResourceType resource in resources)
        {
            if (!resourceInventory.ContainsKey(resource))
            {
                resourceInventory[resource] = 0;
            }
            resourceInventory[resource]++;

            // Record delivery with DemandManager for tracking
            if (DemandManager.Instance != null)
            {
                DemandManager.Instance.RecordDelivery(resource);
            }
        }

        // Calculate and earn money from delivered resources
        // Payment amount depends on whether demand is met (1.0x if met, 0.5x if not)
        float totalValue = CalculateResourceValue(resources);
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.EarnMoney(totalValue);
        }
        else
        {
            Debug.LogWarning("HiveController: EconomyManager not found. Money not earned.");
        }

        Debug.Log($"Hive received {resources.Count} pollen. New totals: " + GetResourceSummary());

        // Fire event to notify UI
        OnResourcesChanged?.Invoke();
    }

    /// <summary>
    /// Calculates the monetary value of delivered resources based on demand fulfillment.
    /// Base value: $1 per resource
    /// Multiplier: 1.0x if demand met, 0.5x if demand not met
    /// Future: Will calculate synergy combo values.
    /// </summary>
    private float CalculateResourceValue(List<ResourceType> resources)
    {
        float totalValue = 0f;

        // Calculate value for each resource based on demand status
        foreach (ResourceType resource in resources)
        {
            float baseValue = 1f; // $1 per resource base
            float multiplier = 1f; // Default multiplier

            // Get demand-based payment multiplier if DemandManager exists
            if (DemandManager.Instance != null)
            {
                multiplier = DemandManager.Instance.GetPaymentMultiplier(resource);
            }

            totalValue += baseValue * multiplier;
        }

        // TODO: Future feature - Implement combo synergy calculations:
        // - Two-resource combos: $8-12
        // - Three-resource combos: $40-50
        // - Four-resource combos: $200+

        return totalValue;
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
