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
    public BiomeType biomeType = BiomeType.Forest;

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

    [Header("Upgrade Configuration - Nectar Flow Tiers")]
    [Tooltip("Cost for each tier upgrade [Tier 1, Tier 2, Tier 3]")]
    public float[] upgradeCosts = new float[] { 50f, 150f, 400f };

    [Tooltip("Number of bees added to global pool per tier upgrade")]
    public int beesPerUpgrade = 2;

    [Header("Capacity Upgrade Configuration")]
    [Tooltip("Cost to upgrade bee capacity (adds bonus capacity)")]
    public float capacityUpgradeCost = 100f;

    [Tooltip("Bonus capacity added per capacity upgrade")]
    public int bonusCapacityPerUpgrade = 5;

    [Header("Base Properties")]
    [Tooltip("Base bee capacity before any upgrades")]
    public int baseCapacity = 5;

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

        // Ensure we have exactly 3 upgrade costs
        if (upgradeCosts == null || upgradeCosts.Length != 3)
        {
            Debug.LogWarning($"[{name}] Upgrade costs must have exactly 3 values (Tier 1, 2, 3). Resetting to defaults.", this);
            upgradeCosts = new float[] { 50f, 150f, 400f };
        }

        // Ensure all upgrade costs are non-negative
        for (int i = 0; i < upgradeCosts.Length; i++)
        {
            if (upgradeCosts[i] < 0f)
            {
                Debug.LogWarning($"[{name}] Upgrade cost at tier {i + 1} cannot be negative. Setting to 0.", this);
                upgradeCosts[i] = 0f;
            }
        }

        // Ensure bees per upgrade is positive
        if (beesPerUpgrade < 1)
        {
            Debug.LogWarning($"[{name}] Bees per upgrade must be at least 1. Setting to 2.", this);
            beesPerUpgrade = 2;
        }

        // Ensure capacity upgrade cost is non-negative
        if (capacityUpgradeCost < 0f)
        {
            Debug.LogWarning($"[{name}] Capacity upgrade cost cannot be negative. Setting to 0.", this);
            capacityUpgradeCost = 0f;
        }

        // Ensure bonus capacity is positive
        if (bonusCapacityPerUpgrade < 1)
        {
            Debug.LogWarning($"[{name}] Bonus capacity per upgrade must be at least 1. Setting to 5.", this);
            bonusCapacityPerUpgrade = 5;
        }

        // Ensure base capacity is positive
        if (baseCapacity < 1)
        {
            Debug.LogWarning($"[{name}] Base capacity must be at least 1. Setting to 5.", this);
            baseCapacity = 5;
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
