using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager for the global bee fleet system.
/// Manages a global pool of bees that can be allocated to individual flower patch pollen routes.
/// Handles allocation, deallocation, and capacity constraints.
/// </summary>
public class BeeFleetManager : MonoBehaviour
{
    public static BeeFleetManager Instance { get; private set; }

    [Header("Fleet State")]
    [SerializeField]
    [Tooltip("Total number of bees owned by the player")]
    private int totalBeesOwned = 0;

    [SerializeField]
    [Tooltip("Current bee purchase tier (0-based index)")]
    private int currentPurchaseTier = 0;

    [Header("Configuration")]
    [SerializeField]
    [Tooltip("Bee fleet upgrade data defining purchase costs and bee amounts")]
    private BeeFleetUpgradeData beeFleetUpgradeData;

    // Track bee allocation per flower patch (FlowerPatch -> allocated bee count)
    private Dictionary<FlowerPatchController, int> beeAllocations = new Dictionary<FlowerPatchController, int>();

    [Header("Events")]
    [Tooltip("Fired when bee allocation changes. Passes (flowerPatch, newBeeCount).")]
    public UnityEvent<FlowerPatchController, int> OnBeeAllocationChanged;

    [Tooltip("Fired when total bees owned changes. Passes new total.")]
    public UnityEvent<int> OnTotalBeesChanged;

    public int TotalBeesOwned => totalBeesOwned;

    private void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.parent = null; // Detach from parent to make root GameObject
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Gets the number of bees available for allocation (not currently assigned to any route).
    /// </summary>
    /// <returns>Number of unassigned bees in the global pool</returns>
    public int GetAvailableBees()
    {
        int totalAllocated = 0;
        foreach (var allocation in beeAllocations.Values)
        {
            totalAllocated += allocation;
        }

        return totalBeesOwned - totalAllocated;
    }

    /// <summary>
    /// Gets the number of bees currently allocated to a specific flower patch.
    /// </summary>
    /// <param name="flowerPatch">Flower patch to query</param>
    /// <returns>Number of bees assigned to this flower patch's pollen route</returns>
    public int GetAllocatedBees(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null)
        {
            Debug.LogWarning("BeeFleetManager: Attempted to get allocation for null flower patch");
            return 0;
        }

        return beeAllocations.TryGetValue(flowerPatch, out int count) ? count : 0;
    }

    /// <summary>
    /// Gets the total number of registered flower patches (flower patches with bee allocations).
    /// </summary>
    /// <returns>Count of flower patches currently registered with the fleet manager</returns>
    public int GetRegisteredFlowerPatchCount()
    {
        return beeAllocations.Count;
    }

    /// <summary>
    /// Adds bees to the global pool (called when purchasing flower patches or upgrading).
    /// </summary>
    /// <param name="count">Number of bees to add (must be positive)</param>
    public void AddBeesToPool(int count)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"BeeFleetManager: Attempted to add non-positive bee count: {count}");
            return;
        }

        totalBeesOwned += count;
        OnTotalBeesChanged?.Invoke(totalBeesOwned);

        Debug.Log($"BeeFleetManager: Added {count} bees. Total owned: {totalBeesOwned}, Available: {GetAvailableBees()}");
    }

    // ============================================
    // GLOBAL BEE PURCHASE SYSTEM
    // ============================================

    /// <summary>
    /// Checks if bees can be purchased (not at max tier)
    /// </summary>
    public bool CanPurchaseBees()
    {
        if (beeFleetUpgradeData == null)
        {
            Debug.LogError("BeeFleetManager: BeeFleetUpgradeData is not assigned!");
            return false;
        }

        return currentPurchaseTier < beeFleetUpgradeData.maxPurchaseTier;
    }

    /// <summary>
    /// Gets the cost for the next bee purchase
    /// </summary>
    /// <returns>Purchase cost, or -1 if cannot purchase</returns>
    public float GetBeePurchaseCost()
    {
        if (!CanPurchaseBees())
        {
            return -1f;
        }

        return beeFleetUpgradeData.GetPurchaseCost(currentPurchaseTier);
    }

    /// <summary>
    /// Gets the number of bees that will be added for the next purchase
    /// </summary>
    /// <returns>Number of bees, or -1 if cannot purchase</returns>
    public int GetBeePurchaseAmount()
    {
        if (!CanPurchaseBees())
        {
            return -1;
        }

        return beeFleetUpgradeData.GetBeesForTier(currentPurchaseTier);
    }

    /// <summary>
    /// Gets the current bee purchase tier
    /// </summary>
    public int GetCurrentPurchaseTier()
    {
        return currentPurchaseTier;
    }

    /// <summary>
    /// Gets the maximum bee purchase tier
    /// </summary>
    public int GetMaxPurchaseTier()
    {
        return beeFleetUpgradeData != null ? beeFleetUpgradeData.maxPurchaseTier : 0;
    }

    /// <summary>
    /// Purchases bees from the global bee pool upgrade system.
    /// Adds bees to the pool WITHOUT auto-allocation - player manually allocates via Fleet Management Panel.
    /// </summary>
    /// <returns>True if purchase succeeded, false otherwise</returns>
    public bool PurchaseBees()
    {
        // Validate bee fleet upgrade data
        if (beeFleetUpgradeData == null)
        {
            Debug.LogError("BeeFleetManager: Cannot purchase bees - BeeFleetUpgradeData is not assigned!");
            return false;
        }

        // Check if purchase is possible
        if (!CanPurchaseBees())
        {
            Debug.LogWarning($"BeeFleetManager: Already at max bee purchase tier ({beeFleetUpgradeData.maxPurchaseTier})");
            return false;
        }

        // Get purchase cost and bee amount for current tier
        float purchaseCost = GetBeePurchaseCost();
        int beeAmount = GetBeePurchaseAmount();

        if (purchaseCost < 0f || beeAmount < 0)
        {
            Debug.LogError("BeeFleetManager: Invalid purchase cost or bee amount");
            return false;
        }

        // Check if player can afford it
        if (!EconomyManager.Instance.CanAfford(purchaseCost))
        {
            Debug.Log($"BeeFleetManager: Cannot afford bee purchase. Cost: ${purchaseCost}");
            return false;
        }

        // Spend money
        EconomyManager.Instance.SpendMoney(purchaseCost);

        // Add bees to global pool (NO auto-allocation - player manually allocates)
        AddBeesToPool(beeAmount);

        // Increment purchase tier
        currentPurchaseTier++;
        Debug.Log($"BeeFleetManager: Purchased {beeAmount} bees for ${purchaseCost}. Purchase tier: {currentPurchaseTier}/{beeFleetUpgradeData.maxPurchaseTier}");

        return true;
    }

    /// <summary>
    /// Allocates one bee to a flower patch's pollen route.
    /// Validates capacity constraints and bee availability.
    /// </summary>
    /// <param name="flowerPatch">Flower patch to allocate bee to</param>
    /// <returns>True if allocation succeeded, false if constraints violated</returns>
    public bool AllocateBee(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null)
        {
            Debug.LogWarning("BeeFleetManager: Attempted to allocate bee to null flower patch");
            return false;
        }

        // Check if flower patch is at capacity
        int currentAllocation = GetAllocatedBees(flowerPatch);
        if (currentAllocation >= flowerPatch.MaxBeeCapacity)
        {
            Debug.LogWarning($"BeeFleetManager: Flower patch '{flowerPatch.name}' is at capacity ({flowerPatch.MaxBeeCapacity})");
            return false;
        }

        // Check if bees are available
        if (GetAvailableBees() <= 0)
        {
            Debug.LogWarning("BeeFleetManager: No bees available for allocation");
            return false;
        }

        // Allocate bee
        if (!beeAllocations.ContainsKey(flowerPatch))
        {
            beeAllocations[flowerPatch] = 0;
        }

        beeAllocations[flowerPatch]++;
        OnBeeAllocationChanged?.Invoke(flowerPatch, beeAllocations[flowerPatch]);

        Debug.Log($"BeeFleetManager: Allocated bee to '{flowerPatch.name}'. Now has {beeAllocations[flowerPatch]}/{flowerPatch.MaxBeeCapacity}. Available: {GetAvailableBees()}");
        return true;
    }

    /// <summary>
    /// Deallocates one bee from a flower patch's pollen route, returning it to the global pool.
    /// </summary>
    /// <param name="flowerPatch">Flower patch to deallocate bee from</param>
    /// <returns>True if deallocation succeeded, false if constraints violated</returns>
    public bool DeallocateBee(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null)
        {
            Debug.LogWarning("BeeFleetManager: Attempted to deallocate bee from null flower patch");
            return false;
        }

        // Check if flower patch has any bees allocated
        int currentAllocation = GetAllocatedBees(flowerPatch);
        if (currentAllocation <= 0)
        {
            Debug.LogWarning($"BeeFleetManager: Flower patch '{flowerPatch.name}' has no bees to deallocate");
            return false;
        }

        // Deallocate bee
        beeAllocations[flowerPatch]--;

        // Clean up dictionary entry if allocation reaches 0
        if (beeAllocations[flowerPatch] == 0)
        {
            beeAllocations.Remove(flowerPatch);
        }

        OnBeeAllocationChanged?.Invoke(flowerPatch, GetAllocatedBees(flowerPatch));

        Debug.Log($"BeeFleetManager: Deallocated bee from '{flowerPatch.name}'. Now has {GetAllocatedBees(flowerPatch)}/{flowerPatch.MaxBeeCapacity}. Available: {GetAvailableBees()}");
        return true;
    }

    /// <summary>
    /// Unregisters a flower patch from the fleet system (called when flower patch is destroyed).
    /// Returns allocated bees to the global pool.
    /// </summary>
    /// <param name="flowerPatch">Flower patch being destroyed</param>
    public void UnregisterFlowerPatch(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null) return;

        if (beeAllocations.ContainsKey(flowerPatch))
        {
            int freedBees = beeAllocations[flowerPatch];
            beeAllocations.Remove(flowerPatch);

            Debug.Log($"BeeFleetManager: Unregistered flower patch '{flowerPatch.name}'. Freed {freedBees} bees. Available: {GetAvailableBees()}");
        }
    }

    /// <summary>
    /// Gets all flower patches that currently have bee allocations.
    /// Useful for UI display and iteration.
    /// </summary>
    /// <returns>List of flower patches with allocated bees</returns>
    public List<FlowerPatchController> GetAllAllocatedFlowerPatches()
    {
        return new List<FlowerPatchController>(beeAllocations.Keys);
    }

    /// <summary>
    /// Sets total bees owned (useful for testing/save loading).
    /// WARNING: Does not adjust allocations. Only use for initialization.
    /// </summary>
    /// <param name="count">New total bee count</param>
    public void SetTotalBees(int count)
    {
        totalBeesOwned = Mathf.Max(0, count);
        OnTotalBeesChanged?.Invoke(totalBeesOwned);

        Debug.Log($"BeeFleetManager: Total bees set to: {totalBeesOwned}");
    }

    /// <summary>
    /// Reset bee fleet to initial state for new year playthrough.
    /// Clears all allocations and resets bee count to zero.
    /// </summary>
    public void ResetToInitialState()
    {
        totalBeesOwned = 0;
        currentPurchaseTier = 0;
        beeAllocations.Clear();
        OnTotalBeesChanged?.Invoke(totalBeesOwned);

        Debug.Log("[BeeFleetManager] Reset to initial state - Total bees: 0, Purchase tier: 0, Allocations cleared");
    }
}
