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

    [Tooltip("Capacity upgrade tier (0-5, each tier adds bonus capacity)")]
    [SerializeField] private int capacityTier = 0;

    [Tooltip("Bonus capacity added per capacity upgrade tier")]
    [SerializeField] private int bonusCapacityPerUpgrade = 5;

    [Tooltip("Cost for each capacity upgrade tier [Tier 1, Tier 2, Tier 3, Tier 4, Tier 5]")]
    [SerializeField] private float[] capacityUpgradeCosts = new float[] { 50f, 150f, 400f, 900f, 2000f };

    [Tooltip("Maximum number of capacity upgrade tiers available")]
    [SerializeField] private int maxCapacityTier = 5;

    [Header("Events")]

    [Tooltip("Fired when capacity is upgraded")]
    public UnityEvent OnCapacityUpgraded = new UnityEvent();

    // Public properties
    /// <summary>
    /// Calculates maximum bee capacity: base capacity + capacity bonus from upgrades
    /// </summary>
    public int MaxBeeCapacity => baseCapacity + capacityBonus;

    /// <summary>
    /// Public accessor for the FlowerPatchData ScriptableObject
    /// </summary>
    public FlowerPatchData FlowerPatchData => flowerPatchData;

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

        // Initialize capacity settings from data
        baseCapacity = data.baseCapacity;
        bonusCapacityPerUpgrade = data.bonusCapacityPerUpgrade;
        maxCapacityTier = data.maxCapacityTier;

        // Initialize capacity upgrade costs array from data
        capacityUpgradeCosts = new float[data.capacityUpgradeCosts.Length];
        System.Array.Copy(data.capacityUpgradeCosts, capacityUpgradeCosts, data.capacityUpgradeCosts.Length);

        Debug.Log($"FlowerPatchController initialized from {data.name}: Biome={biomeType}, BaseCapacity={baseCapacity}, CapacityUpgradeCosts=[{string.Join(", ", capacityUpgradeCosts)}]");
    }

    // ============================================
    // CAPACITY UPGRADE SYSTEM
    // ============================================

    /// <summary>
    /// Checks if this flower patch's capacity can be upgraded
    /// </summary>
    public bool CanUpgradeCapacity()
    {
        return capacityTier < maxCapacityTier;
    }

    /// <summary>
    /// Gets the cost to upgrade capacity for the current tier
    /// </summary>
    /// <returns>Capacity upgrade cost, or -1 if already at max capacity</returns>
    public float GetCapacityUpgradeCost()
    {
        if (!CanUpgradeCapacity())
        {
            return -1f; // Already at max capacity
        }

        // Validate tier index
        if (capacityTier < 0 || capacityTier >= capacityUpgradeCosts.Length)
        {
            Debug.LogError($"Invalid capacity tier {capacityTier} for {gameObject.name}");
            return -1f;
        }

        return capacityUpgradeCosts[capacityTier];
    }

    /// <summary>
    /// Gets the capacity value after the next capacity upgrade
    /// </summary>
    /// <returns>New capacity value after adding bonus, or -1 if already at max</returns>
    public int GetNextCapacity()
    {
        if (!CanUpgradeCapacity())
        {
            return -1;
        }
        // Calculate what capacity will be after adding the bonus
        return baseCapacity + capacityBonus + bonusCapacityPerUpgrade;
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
            Debug.LogWarning($"Flower patch {gameObject.name} is already at max capacity tier ({maxCapacityTier})");
            return false;
        }

        // Get upgrade cost for current tier
        float upgradeCost = GetCapacityUpgradeCost();
        if (upgradeCost < 0f)
        {
            Debug.LogError($"Invalid capacity upgrade cost for {gameObject.name}");
            return false;
        }

        // Check if player can afford it
        if (!EconomyManager.Instance.CanAfford(upgradeCost))
        {
            Debug.Log($"Cannot afford capacity upgrade for {gameObject.name}. Cost: ${upgradeCost}");
            return false;
        }

        // Spend money
        EconomyManager.Instance.SpendMoney(upgradeCost);

        // Upgrade capacity by incrementing tier and adding bonus
        capacityTier++;
        capacityBonus += bonusCapacityPerUpgrade;
        Debug.Log($"Flower patch {gameObject.name} capacity upgraded to tier {capacityTier}/{maxCapacityTier}! Added +{bonusCapacityPerUpgrade} bonus capacity. New total capacity: {MaxBeeCapacity} bees");

        // Fire event
        OnCapacityUpgraded?.Invoke();

        return true;
    }

    /// <summary>
    /// Gets the current capacity tier
    /// </summary>
    public int GetCapacityTier()
    {
        return capacityTier;
    }

    /// <summary>
    /// Gets the maximum capacity tier available for this flower patch
    /// </summary>
    public int GetMaxCapacityTier()
    {
        return maxCapacityTier;
    }
}
