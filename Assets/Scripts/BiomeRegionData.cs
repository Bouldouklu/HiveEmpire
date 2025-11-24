using UnityEngine;

/// <summary>
/// ScriptableObject defining configuration data for a BiomeRegion.
/// Extends FlowerPatchData to maintain compatibility with existing systems.
/// </summary>
[CreateAssetMenu(fileName = "NewBiomeRegionData", menuName = "Game/Biome Region Data", order = 1)]
public class BiomeRegionData : ScriptableObject
{
    [Header("Region Identity")]
    [Tooltip("Type of biome this region represents")]
    public BiomeType biomeType;

    [Tooltip("Display name shown in UI")]
    public string displayName;

    [Tooltip("Description shown in unlock panel")]
    [TextArea(3, 5)]
    public string description;

    [Header("Visual Configuration")]
    [Tooltip("Number of hex tiles in this region")]
    [SerializeField] private int hexTileCount = 7;

    [Tooltip("Prefab for the complete region (includes all hex tiles)")]
    public GameObject regionPrefab;

    [Header("Pollen Properties")]
    [Tooltip("FlowerPatchData for pollen properties (used by inventory and recipes)")]
    public FlowerPatchData flowerPatchData;

    [Tooltip("Display name for pollen type")]
    public string pollenDisplayName;

    [Tooltip("Icon for pollen in recipe UI")]
    public Sprite pollenIcon;

    [Tooltip("Optional material override for pollen cubes")]
    public Material pollenMaterial;

    [Tooltip("Description of pollen type")]
    [TextArea(2, 3)]
    public string pollenDescription;

    [Header("Economy")]
    [Tooltip("Cost to unlock entire region")]
    [SerializeField] private float unlockCost = 100f;

    [Header("Capacity Configuration")]
    [Tooltip("Starting bee capacity for entire region")]
    [SerializeField] private int baseCapacity = 10;

    [Tooltip("Cost for each capacity upgrade tier")]
    [SerializeField] private float[] capacityUpgradeCosts = new float[] { 50f, 100f, 200f };

    [Tooltip("Capacity bonus added per upgrade")]
    [SerializeField] private int bonusCapacityPerUpgrade = 5;

    [Tooltip("Maximum number of capacity upgrade tiers")]
    [SerializeField] private int maxCapacityTier = 3;

    [Header("Gathering")]
    [Tooltip("Duration bees hover at tiles before returning to hive")]
    [SerializeField] private float gatheringDuration = 2f;

    /// <summary>
    /// Gets the number of hex tiles in this region
    /// </summary>
    public int HexTileCount => hexTileCount;

    /// <summary>
    /// Gets the cost to unlock this region
    /// </summary>
    public float UnlockCost => unlockCost;

    /// <summary>
    /// Gets the base capacity for this region
    /// </summary>
    public int BaseCapacity => baseCapacity;

    /// <summary>
    /// Gets the capacity upgrade costs array
    /// </summary>
    public float[] CapacityUpgradeCosts => capacityUpgradeCosts;

    /// <summary>
    /// Gets the capacity bonus per upgrade
    /// </summary>
    public int BonusCapacityPerUpgrade => bonusCapacityPerUpgrade;

    /// <summary>
    /// Gets the maximum capacity tier
    /// </summary>
    public int MaxCapacityTier => maxCapacityTier;

    /// <summary>
    /// Gets the gathering duration
    /// </summary>
    public float GatheringDuration => gatheringDuration;

    /// <summary>
    /// Gets the capacity upgrade cost for a specific tier
    /// </summary>
    public float GetCapacityUpgradeCost(int tier)
    {
        if (tier < 0 || tier >= capacityUpgradeCosts.Length)
        {
            Debug.LogWarning($"Invalid capacity tier {tier} for region {displayName}");
            return 0f;
        }
        return capacityUpgradeCosts[tier];
    }

    /// <summary>
    /// Checks if a capacity upgrade is available at the given tier
    /// </summary>
    public bool CanUpgradeCapacity(int currentTier)
    {
        return currentTier < maxCapacityTier && currentTier < capacityUpgradeCosts.Length;
    }

    private void OnValidate()
    {
        // Ensure unlock cost is positive
        if (unlockCost < 0f)
        {
            Debug.LogWarning($"Unlock cost cannot be negative for {name}. Setting to 0.");
            unlockCost = 0f;
        }

        // Ensure base capacity is positive
        if (baseCapacity < 1)
        {
            Debug.LogWarning($"Base capacity must be at least 1 for {name}. Setting to 1.");
            baseCapacity = 1;
        }

        // Ensure hex tile count is valid
        if (hexTileCount < 1)
        {
            Debug.LogWarning($"Hex tile count must be at least 1 for {name}. Setting to 1.");
            hexTileCount = 1;
        }

        // Validate capacity upgrade costs
        if (capacityUpgradeCosts == null || capacityUpgradeCosts.Length == 0)
        {
            Debug.LogWarning($"Capacity upgrade costs array is empty for {name}. Creating default array.");
            capacityUpgradeCosts = new float[] { 50f, 100f, 200f };
        }

        for (int i = 0; i < capacityUpgradeCosts.Length; i++)
        {
            if (capacityUpgradeCosts[i] < 0f)
            {
                Debug.LogWarning($"Capacity upgrade cost at tier {i} cannot be negative for {name}. Setting to 0.");
                capacityUpgradeCosts[i] = 0f;
            }
        }

        // Ensure bonus capacity is positive
        if (bonusCapacityPerUpgrade < 1)
        {
            Debug.LogWarning($"Bonus capacity per upgrade must be at least 1 for {name}. Setting to 1.");
            bonusCapacityPerUpgrade = 1;
        }

        // Ensure max capacity tier matches costs array
        if (maxCapacityTier != capacityUpgradeCosts.Length)
        {
            Debug.LogWarning($"Max capacity tier ({maxCapacityTier}) doesn't match costs array length ({capacityUpgradeCosts.Length}) for {name}. Adjusting.");
            maxCapacityTier = capacityUpgradeCosts.Length;
        }

        // Ensure gathering duration is positive
        if (gatheringDuration <= 0f)
        {
            Debug.LogWarning($"Gathering duration must be positive for {name}. Setting to 2.0.");
            gatheringDuration = 2f;
        }

        // Set display name from biome type if empty
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = biomeType.ToString();
        }
    }
}
