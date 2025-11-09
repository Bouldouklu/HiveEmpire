using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls an airport that instantly provides resources based on its biome type.
/// Airports act as infinite resource dispensers - airplanes are the limiting factor.
/// Airplane spawning is now handled by RouteController component.
/// Supports Speed Hub upgrades that increase airplane count per route.
/// </summary>
public class AirportController : MonoBehaviour
{
    [Header("Airport Settings")]
    [Tooltip("Biome type determines what resource this airport produces")]
    [SerializeField] private BiomeType biomeType = BiomeType.Desert;

    [Header("Drone Capacity System")]
    [Tooltip("Maximum drones that can be assigned to this airport's route")]
    [SerializeField] private int maxDroneCapacity = 5;

    [Tooltip("Capacity upgrade tier (0=5 drones, 1=10 drones)")]
    [SerializeField] private int capacityTier = 0;

    [Tooltip("Cost to upgrade capacity from 5 to 10")]
    [SerializeField] private float capacityUpgradeCost = 100f;

    [Header("Upgrade System - Speed Hub")]
    [Tooltip("Current upgrade tier (0=Base, 1-3=Upgraded)")]
    [SerializeField] private int currentTier = 0;

    [Tooltip("Upgrade cost for each tier [Tier1, Tier2, Tier3]")]
    [SerializeField] private float[] upgradeCosts = new float[] { 50f, 150f, 400f };

    [Tooltip("Number of drones added to global pool per tier upgrade")]
    [SerializeField] private int dronesPerUpgrade = 2;

    [Header("Events")]
    [Tooltip("Fired when airport is upgraded")]
    public UnityEvent<int> OnAirportUpgraded = new UnityEvent<int>(); // Passes new tier

    [Tooltip("Fired when capacity is upgraded")]
    public UnityEvent OnCapacityUpgraded = new UnityEvent();

    // Public properties
    public int MaxDroneCapacity => maxDroneCapacity;

    private void OnDestroy()
    {
        // Unregister from fleet manager when destroyed
        if (DroneFleetManager.Instance != null)
        {
            DroneFleetManager.Instance.UnregisterAirport(this);
        }
    }

    /// <summary>
    /// Instantly provides a resource based on the airport's biome type.
    /// Called by airplanes when they need to pick up cargo.
    /// </summary>
    /// <returns>The resource type this airport produces</returns>
    public ResourceType GetResource()
    {
        // Map biome type to resource type
        return biomeType switch
        {
            BiomeType.Forest => ResourceType.Wood,
            BiomeType.Plains => ResourceType.Food,
            BiomeType.Mountain => ResourceType.Stone,
            BiomeType.Desert => ResourceType.Oil,
            BiomeType.Coastal => ResourceType.Fish,
            BiomeType.Tundra => ResourceType.Minerals,
            _ => ResourceType.Wood // Default fallback
        };
    }

    /// <summary>
    /// Gets the biome type of this airport (for external queries)
    /// </summary>
    public BiomeType GetBiomeType()
    {
        return biomeType;
    }

    /// <summary>
    /// Sets the biome type of this airport (for runtime configuration)
    /// </summary>
    /// <param name="newBiomeType">The biome type to set</param>
    public void SetBiomeType(BiomeType newBiomeType)
    {
        biomeType = newBiomeType;
    }

    // ============================================
    // UPGRADE SYSTEM
    // ============================================

    /// <summary>
    /// Gets the current upgrade tier (0-3)
    /// </summary>
    public int GetCurrentTier()
    {
        return currentTier;
    }

    /// <summary>
    /// Gets the number of drones added to global pool at current tier.
    /// Note: This is for UI display. Actual airplane count is determined by drone allocation.
    /// </summary>
    public int GetMaxAirplanesForCurrentTier()
    {
        // With the new drone system, upgrades add drones to the global pool
        // This method is kept for backward compatibility with UI
        // Returns total drones that have been added through upgrades
        return currentTier * dronesPerUpgrade;
    }

    /// <summary>
    /// Checks if this airport can be upgraded further
    /// </summary>
    public bool CanUpgrade()
    {
        return currentTier < 3; // Max tier is 3
    }

    /// <summary>
    /// Gets the cost to upgrade to the next tier
    /// </summary>
    /// <returns>Upgrade cost, or -1 if at max tier</returns>
    public float GetUpgradeCost()
    {
        if (!CanUpgrade())
        {
            return -1f; // Already at max tier
        }

        // upgradeCosts array is 0-indexed for tier upgrades [Tier1, Tier2, Tier3]
        // currentTier is the index into upgradeCosts
        if (currentTier < 0 || currentTier >= upgradeCosts.Length)
        {
            Debug.LogError($"Invalid tier {currentTier} for upgrade cost lookup");
            return -1f;
        }

        return upgradeCosts[currentTier];
    }

    /// <summary>
    /// Gets the number of drones that will be added to global pool at next tier.
    /// Note: This is for UI display. Actual airplane count is determined by drone allocation.
    /// </summary>
    /// <returns>Drones to be added at next tier, or -1 if at max tier</returns>
    public int GetNextTierAirplaneCount()
    {
        if (!CanUpgrade())
        {
            return -1;
        }

        // Each upgrade adds dronesPerUpgrade drones to the global pool
        // Return the total that will have been added after the next upgrade
        int nextTier = currentTier + 1;
        return nextTier * dronesPerUpgrade;
    }

    /// <summary>
    /// Attempts to upgrade this airport to the next tier
    /// </summary>
    /// <returns>True if upgrade succeeded, false otherwise</returns>
    public bool UpgradeAirport()
    {
        // Check if upgrade is possible
        if (!CanUpgrade())
        {
            Debug.LogWarning($"Airport {gameObject.name} is already at max tier {currentTier}");
            return false;
        }

        float upgradeCost = GetUpgradeCost();
        if (upgradeCost < 0f)
        {
            Debug.LogError($"Invalid upgrade cost for airport {gameObject.name}");
            return false;
        }

        // Check if player can afford it
        if (!EconomyManager.Instance.CanAfford(upgradeCost))
        {
            Debug.Log($"Cannot afford upgrade for {gameObject.name}. Cost: ${upgradeCost}");
            return false;
        }

        // Spend money
        EconomyManager.Instance.SpendMoney(upgradeCost);

        // Increment tier
        currentTier++;
        Debug.Log($"Airport {gameObject.name} upgraded to Tier {currentTier}");

        // Add drones to global pool
        if (DroneFleetManager.Instance != null)
        {
            DroneFleetManager.Instance.AddDronesToPool(dronesPerUpgrade);
            Debug.Log($"Added {dronesPerUpgrade} drones to global pool");
        }

        // Fire event so RouteController can adjust spawning
        OnAirportUpgraded?.Invoke(currentTier);

        return true;
    }

    /// <summary>
    /// Gets a display name for the current tier
    /// </summary>
    public string GetTierDisplayName()
    {
        return currentTier switch
        {
            0 => "Base",
            1 => "Speed Hub I",
            2 => "Speed Hub II",
            3 => "Speed Hub III",
            _ => "Unknown"
        };
    }

    // ============================================
    // CAPACITY UPGRADE SYSTEM
    // ============================================

    /// <summary>
    /// Checks if this airport's capacity can be upgraded
    /// </summary>
    public bool CanUpgradeCapacity()
    {
        return capacityTier < 1; // Can only upgrade once (5 -> 10)
    }

    /// <summary>
    /// Gets the cost to upgrade capacity
    /// </summary>
    /// <returns>Capacity upgrade cost, or -1 if already at max capacity</returns>
    public float GetCapacityUpgradeCost()
    {
        if (!CanUpgradeCapacity())
        {
            return -1f; // Already at max capacity
        }
        return capacityUpgradeCost;
    }

    /// <summary>
    /// Gets the capacity value after upgrade
    /// </summary>
    /// <returns>New capacity value, or -1 if already at max</returns>
    public int GetNextCapacity()
    {
        if (!CanUpgradeCapacity())
        {
            return -1;
        }
        return 10; // Capacity upgrades from 5 to 10
    }

    /// <summary>
    /// Attempts to upgrade this airport's drone capacity from 5 to 10
    /// </summary>
    /// <returns>True if upgrade succeeded, false otherwise</returns>
    public bool UpgradeCapacity()
    {
        // Check if upgrade is possible
        if (!CanUpgradeCapacity())
        {
            Debug.LogWarning($"Airport {gameObject.name} is already at max capacity {maxDroneCapacity}");
            return false;
        }

        // Check if player can afford it
        if (!EconomyManager.Instance.CanAfford(capacityUpgradeCost))
        {
            Debug.Log($"Cannot afford capacity upgrade for {gameObject.name}. Cost: ${capacityUpgradeCost}");
            return false;
        }

        // Spend money
        EconomyManager.Instance.SpendMoney(capacityUpgradeCost);

        // Upgrade capacity
        capacityTier = 1;
        maxDroneCapacity = 10;
        Debug.Log($"Airport {gameObject.name} capacity upgraded to {maxDroneCapacity} drones");

        // Fire event
        OnCapacityUpgraded?.Invoke();

        return true;
    }

    /// <summary>
    /// Gets the current capacity tier (0=5 drones, 1=10 drones)
    /// </summary>
    public int GetCapacityTier()
    {
        return capacityTier;
    }
}
