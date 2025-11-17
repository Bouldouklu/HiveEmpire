using UnityEngine;

/// <summary>
/// ScriptableObject that defines all configuration for a flower patch type.
/// Each placeholder references a specific FlowerPatchData asset, allowing
/// multiple patches of the same biome type with different properties.
/// </summary>
[CreateAssetMenu(fileName = "NewFlowerPatchData", menuName = "Game/Flower Patch Data", order = 1)]
public class FlowerPatchData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The biome type this flower patch represents")]
    public BiomeType biomeType = BiomeType.WildMeadow;

    [Tooltip("Display name for UI (e.g., 'Forest Flower Patch')")]
    public string displayName = "Flower Patch";

    [TextArea(2, 4)]
    [Tooltip("Optional description for tooltips")]
    public string description = "";

    [Header("Placement Cost")]
    [Tooltip("Fixed cost to place this flower patch (no scaling)")]
    public float placementCost = 10f;

    [Header("Prefab Reference")]
    [Tooltip("The flower patch prefab to instantiate when placed")]
    public GameObject flowerPatchPrefab;

    [Header("Capacity Upgrade Configuration")]
    [Tooltip("Cost for each capacity upgrade tier (e.g., [50, 150, 400, 900, 2000])")]
    public float[] capacityUpgradeCosts = new float[] { 50f, 150f, 400f, 900f, 2000f };

    [Tooltip("Bonus capacity added per capacity upgrade tier")]
    public int bonusCapacityPerUpgrade = 5;

    [Tooltip("Maximum number of capacity upgrade tiers available")]
    public int maxCapacityTier = 5;

    [Header("Base Properties")]
    [Tooltip("Base bee capacity before any upgrades")]
    public int baseCapacity = 5;

    [Header("Gathering Behavior")]
    [Tooltip("Time (in seconds) bees spend hovering at this patch before returning to hive")]
    public float gatheringDuration = 2.5f;

    /// <summary>
    /// Validates configuration in the Unity Inspector.
    /// Called when the asset is loaded or values are changed in the Inspector.
    /// </summary>
    private void OnValidate()
    {
        // Ensure placement cost is non-negative
        if (placementCost < 0f)
        {
            Debug.LogWarning($"[{name}] Placement cost cannot be negative. Setting to 0.", this);
            placementCost = 0f;
        }

        // Ensure capacity upgrade costs array is valid
        if (capacityUpgradeCosts == null || capacityUpgradeCosts.Length == 0)
        {
            Debug.LogWarning($"[{name}] Capacity upgrade costs array is empty. Resetting to defaults.", this);
            capacityUpgradeCosts = new float[] { 50f, 150f, 400f, 900f, 2000f };
        }

        // Ensure all capacity upgrade costs are non-negative
        for (int i = 0; i < capacityUpgradeCosts.Length; i++)
        {
            if (capacityUpgradeCosts[i] < 0f)
            {
                Debug.LogWarning($"[{name}] Capacity upgrade cost at tier {i} cannot be negative. Setting to 0.", this);
                capacityUpgradeCosts[i] = 0f;
            }
        }

        // Ensure bonus capacity is positive
        if (bonusCapacityPerUpgrade < 1)
        {
            Debug.LogWarning($"[{name}] Bonus capacity per upgrade must be at least 1. Setting to 5.", this);
            bonusCapacityPerUpgrade = 5;
        }

        // Ensure max capacity tier matches array length
        if (maxCapacityTier != capacityUpgradeCosts.Length)
        {
            Debug.LogWarning($"[{name}] Max capacity tier should match capacity upgrade costs array length. Updating to {capacityUpgradeCosts.Length}.", this);
            maxCapacityTier = capacityUpgradeCosts.Length;
        }

        // Ensure max capacity tier is positive
        if (maxCapacityTier < 1)
        {
            Debug.LogWarning($"[{name}] Max capacity tier must be at least 1. Setting to 1.", this);
            maxCapacityTier = 1;
        }

        // Ensure base capacity is positive
        if (baseCapacity < 1)
        {
            Debug.LogWarning($"[{name}] Base capacity must be at least 1. Setting to 5.", this);
            baseCapacity = 5;
        }

        // Ensure gathering duration is positive
        if (gatheringDuration < 0.1f)
        {
            Debug.LogWarning($"[{name}] Gathering duration must be at least 0.1 seconds. Setting to 2.5.", this);
            gatheringDuration = 2.5f;
        }

        // Warn if prefab is missing
        if (flowerPatchPrefab == null)
        {
            Debug.LogWarning($"[{name}] Flower patch prefab is not assigned!", this);
        }
        else
        {
            // Verify prefab has FlowerPatchController component
            if (flowerPatchPrefab.GetComponent<FlowerPatchController>() == null)
            {
                Debug.LogError($"[{name}] Flower patch prefab '{flowerPatchPrefab.name}' is missing FlowerPatchController component!", this);
            }
        }

        // Auto-generate display name if empty
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = $"{biomeType} Flower Patch";
        }
    }
}
