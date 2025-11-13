using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls a flower patch that instantly provides pollen based on its biome type.
/// Flower patches act as infinite pollen dispensers - bees are the limiting factor.
/// Bee spawning is now handled by RouteController component.
/// Supports Nectar Flow upgrades that increase bee count per route.
/// </summary>
public class FlowerPatchController : MonoBehaviour
{
    [Header("Flower Patch Settings")]
    [Tooltip("Biome type determines what pollen this flower patch produces")]
    [SerializeField] private BiomeType biomeType = BiomeType.Marsh;

    [Tooltip("FlowerPatchData used to initialize this flower patch (for reference)")]
    [SerializeField] private FlowerPatchData flowerPatchData;

    [Header("Bee Capacity System")]
    [Tooltip("Base bee capacity (before tier and bonus upgrades)")]
    [SerializeField] private int baseCapacity = 5;

    [Tooltip("Bonus capacity from capacity upgrades")]
    [SerializeField] private int capacityBonus = 0;

    [Tooltip("Capacity upgrade tier (0=no bonus, 1=+5 bonus)")]
    [SerializeField] private int capacityTier = 0;

    [Tooltip("Bonus capacity added per capacity upgrade")]
    [SerializeField] private int bonusCapacityPerUpgrade = 5;

    [Tooltip("Cost to upgrade capacity (adds bonus capacity on top of base + tier)")]
    [SerializeField] private float capacityUpgradeCost = 100f;

    [Header("Upgrade System - Nectar Flow")]
    [Tooltip("Current upgrade tier (0=Base, 1-3=Upgraded)")]
    [SerializeField] private int currentTier = 0;

    [Tooltip("Upgrade cost for each tier [Tier1, Tier2, Tier3]")]
    [SerializeField] private float[] upgradeCosts = new float[] { 50f, 150f, 400f };

    [Tooltip("Number of bees added to global pool per tier upgrade")]
    [SerializeField] private int beesPerUpgrade = 2;

    [Header("Events")]
    [Tooltip("Fired when flower patch is upgraded")]
    public UnityEvent<int> OnFlowerPatchUpgraded = new UnityEvent<int>(); // Passes new tier

    [Tooltip("Fired when capacity is upgraded")]
    public UnityEvent OnCapacityUpgraded = new UnityEvent();

    // Public properties
    /// <summary>
    /// Calculates maximum bee capacity: base (5) + tier increases (tier × beesPerUpgrade) + capacity bonus
    /// </summary>
    public int MaxBeeCapacity => baseCapacity + (currentTier * beesPerUpgrade) + capacityBonus;

    private void OnDestroy()
    {
        // Unregister from fleet manager when destroyed
        if (BeeFleetManager.Instance != null)
        {
            BeeFleetManager.Instance.UnregisterFlowerPatch(this);
        }

        // Unregister from audio manager when destroyed
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterFlowerPatch(this);
        }
    }

    /// <summary>
    /// Instantly provides pollen based on the flower patch's biome type.
    /// Called by bees when they need to pick up pollen.
    /// </summary>
    /// <returns>The resource type this flower patch produces</returns>
    public ResourceType GetResource()
    {
        // Map biome type to resource type
        return biomeType switch
        {
            BiomeType.WildMeadow => ResourceType.WildMeadowPollen,
            BiomeType.Orchard => ResourceType.OrchardPollen,
            BiomeType.CultivatedGarden => ResourceType.CultivatedGardenPollen,
            BiomeType.Marsh => ResourceType.MarshPollen,
            BiomeType.ForestEdge => ResourceType.ForestEdgePollen,
            BiomeType.AgriculturalField => ResourceType.AgriculturalFieldPollen,
            _ => ResourceType.WildMeadowPollen // Default fallback
        };
    }

    /// <summary>
    /// Gets the biome type of this flower patch (for external queries)
    /// </summary>
    public BiomeType GetBiomeType()
    {
        return biomeType;
    }

    /// <summary>
    /// Sets the biome type of this flower patch (for runtime configuration)
    /// </summary>
    /// <param name="newBiomeType">The biome type to set</param>
    public void SetBiomeType(BiomeType newBiomeType)
    {
        biomeType = newBiomeType;
    }

    /// <summary>
    /// Initializes this flower patch from FlowerPatchData ScriptableObject.
    /// Called by FlowerPatchPlaceholder when spawning a new flower patch.
    /// </summary>
    /// <param name="data">The FlowerPatchData containing configuration</param>
    public void InitializeFromData(FlowerPatchData data)
    {
        if (data == null)
        {
            Debug.LogError($"FlowerPatchController on {gameObject.name}: Cannot initialize from null FlowerPatchData!", this);
            return;
        }

        // Store reference to data
        flowerPatchData = data;

        // Set biome type
        biomeType = data.biomeType;

        // Initialize upgrade costs from data
        upgradeCosts = new float[data.upgradeCosts.Length];
        System.Array.Copy(data.upgradeCosts, upgradeCosts, data.upgradeCosts.Length);

        // Initialize capacity settings from data
        baseCapacity = data.baseCapacity;
        beesPerUpgrade = data.beesPerUpgrade;
        capacityUpgradeCost = data.capacityUpgradeCost;
        bonusCapacityPerUpgrade = data.bonusCapacityPerUpgrade;

        Debug.Log($"FlowerPatchController initialized from {data.name}: Biome={biomeType}, BaseCapacity={baseCapacity}, UpgradeCosts=[{string.Join(", ", upgradeCosts)}]");
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
    /// Gets the number of bees added to global pool at current tier.
    /// Note: This is for UI display. Actual bee count is determined by bee allocation.
    /// </summary>
    public int GetMaxBeesForCurrentTier()
    {
        // With the new bee system, upgrades add bees to the global pool
        // This method is kept for backward compatibility with UI
        // Returns total bees that have been added through upgrades
        return currentTier * beesPerUpgrade;
    }

    /// <summary>
    /// Checks if this flower patch can be upgraded further
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
    /// Gets the number of bees that will be added to global pool at next tier.
    /// Note: This is for UI display. Actual bee count is determined by bee allocation.
    /// </summary>
    /// <returns>Bees to be added at next tier, or -1 if at max tier</returns>
    public int GetNextTierBeeCount()
    {
        if (!CanUpgrade())
        {
            return -1;
        }

        // Each upgrade adds beesPerUpgrade bees to the global pool
        // Return the total that will have been added after the next upgrade
        int nextTier = currentTier + 1;
        return nextTier * beesPerUpgrade;
    }

    /// <summary>
    /// Attempts to upgrade this flower patch to the next tier
    /// </summary>
    /// <returns>True if upgrade succeeded, false otherwise</returns>
    public bool UpgradeFlowerPatch()
    {
        // Check if upgrade is possible
        if (!CanUpgrade())
        {
            Debug.LogWarning($"Flower patch {gameObject.name} is already at max tier {currentTier}");
            return false;
        }

        float upgradeCost = GetUpgradeCost();
        if (upgradeCost < 0f)
        {
            Debug.LogError($"Invalid upgrade cost for flower patch {gameObject.name}");
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
        Debug.Log($"Flower patch {gameObject.name} upgraded to Tier {currentTier}");

        // Add bees to global pool AND automatically allocate them to this route
        if (BeeFleetManager.Instance != null)
        {
            BeeFleetManager.Instance.AddBeesToPool(beesPerUpgrade);
            Debug.Log($"Added {beesPerUpgrade} bees to global pool");

            // Auto-allocate the new bees to this flower patch
            // This ensures the bees are immediately assigned to this route
            // Capacity automatically increases via MaxBeeCapacity calculation (base + tier × beesPerUpgrade + bonus)
            for (int i = 0; i < beesPerUpgrade; i++)
            {
                bool allocated = BeeFleetManager.Instance.AllocateBee(this);
                if (!allocated)
                {
                    Debug.LogWarning($"Failed to allocate bee {i + 1}/{beesPerUpgrade} to {gameObject.name}");
                }
            }
            Debug.Log($"Auto-allocated {beesPerUpgrade} bees to {gameObject.name}. New capacity: {MaxBeeCapacity}");
        }

        // Fire event so RouteController can adjust spawning
        OnFlowerPatchUpgraded?.Invoke(currentTier);

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
            1 => "Nectar Flow I",
            2 => "Nectar Flow II",
            3 => "Nectar Flow III",
            _ => "Unknown"
        };
    }

    // ============================================
    // CAPACITY UPGRADE SYSTEM
    // ============================================

    /// <summary>
    /// Checks if this flower patch's capacity can be upgraded
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
    /// Gets the capacity value after bonus capacity upgrade
    /// </summary>
    /// <returns>New capacity value after adding bonus, or -1 if already at max</returns>
    public int GetNextCapacity()
    {
        if (!CanUpgradeCapacity())
        {
            return -1;
        }
        // Calculate what capacity will be after adding the bonus
        return baseCapacity + (currentTier * beesPerUpgrade) + capacityBonus + bonusCapacityPerUpgrade;
    }

    /// <summary>
    /// Attempts to upgrade this flower patch's bee capacity by adding bonus capacity
    /// </summary>
    /// <returns>True if upgrade succeeded, false otherwise</returns>
    public bool UpgradeCapacity()
    {
        // Check if upgrade is possible
        if (!CanUpgradeCapacity())
        {
            Debug.LogWarning($"Flower patch {gameObject.name} is already at max capacity tier");
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

        // Upgrade capacity by adding bonus
        capacityTier = 1;
        capacityBonus += bonusCapacityPerUpgrade;
        Debug.Log($"Flower patch {gameObject.name} capacity upgraded! Added +{bonusCapacityPerUpgrade} bonus capacity. New total capacity: {MaxBeeCapacity} bees");

        // Fire event
        OnCapacityUpgraded?.Invoke();

        return true;
    }

    /// <summary>
    /// Gets the current capacity tier (0=5 bees, 1=10 bees)
    /// </summary>
    public int GetCapacityTier()
    {
        return capacityTier;
    }
}
