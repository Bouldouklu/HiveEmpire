using UnityEngine;

/// <summary>
/// ScriptableObject that defines global bee fleet purchase configuration.
/// Players purchase bees through tiers, adding them to the global bee pool.
/// </summary>
[CreateAssetMenu(fileName = "BeeFleetUpgradeData", menuName = "Game/Bee Fleet Upgrade Data", order = 2)]
public class BeeFleetUpgradeData : ScriptableObject
{
    [Header("Bee Purchase Tiers")]
    [Tooltip("Cost for each bee purchase tier (e.g., [50, 100, 200, 400])")]
    public float[] beePurchaseCosts = new float[] { 50f, 100f, 200f, 400f };

    [Tooltip("Number of bees added to global pool per purchase tier")]
    public int[] beesPerPurchase = new int[] { 2, 3, 5, 8 };

    [Header("Configuration")]
    [Tooltip("Maximum number of bee purchase tiers available")]
    public int maxPurchaseTier = 4;

    /// <summary>
    /// Gets the cost for a specific purchase tier.
    /// </summary>
    /// <param name="tier">The purchase tier (0-based index)</param>
    /// <returns>Cost for that tier, or 0 if invalid</returns>
    public float GetPurchaseCost(int tier)
    {
        if (tier < 0 || tier >= beePurchaseCosts.Length)
        {
            Debug.LogWarning($"[BeeFleetUpgradeData] Invalid tier {tier}. Valid range: 0-{beePurchaseCosts.Length - 1}");
            return 0f;
        }
        return beePurchaseCosts[tier];
    }

    /// <summary>
    /// Gets the number of bees granted for a specific purchase tier.
    /// </summary>
    /// <param name="tier">The purchase tier (0-based index)</param>
    /// <returns>Number of bees for that tier, or 0 if invalid</returns>
    public int GetBeesForTier(int tier)
    {
        if (tier < 0 || tier >= beesPerPurchase.Length)
        {
            Debug.LogWarning($"[BeeFleetUpgradeData] Invalid tier {tier}. Valid range: 0-{beesPerPurchase.Length - 1}");
            return 0;
        }
        return beesPerPurchase[tier];
    }

    /// <summary>
    /// Validates configuration in the Unity Inspector.
    /// Called when the asset is loaded or values are changed in the Inspector.
    /// </summary>
    private void OnValidate()
    {
        // Ensure arrays are not null
        if (beePurchaseCosts == null || beePurchaseCosts.Length == 0)
        {
            Debug.LogWarning($"[{name}] Bee purchase costs array is empty. Resetting to defaults.", this);
            beePurchaseCosts = new float[] { 50f, 100f, 200f, 400f };
        }

        if (beesPerPurchase == null || beesPerPurchase.Length == 0)
        {
            Debug.LogWarning($"[{name}] Bees per purchase array is empty. Resetting to defaults.", this);
            beesPerPurchase = new int[] { 2, 3, 5, 8 };
        }

        // Ensure arrays have matching lengths
        if (beePurchaseCosts.Length != beesPerPurchase.Length)
        {
            Debug.LogWarning($"[{name}] Bee purchase costs and bees per purchase arrays must have the same length. Resizing to match.", this);
            int minLength = Mathf.Min(beePurchaseCosts.Length, beesPerPurchase.Length);
            System.Array.Resize(ref beePurchaseCosts, minLength);
            System.Array.Resize(ref beesPerPurchase, minLength);
        }

        // Ensure all costs are non-negative
        for (int i = 0; i < beePurchaseCosts.Length; i++)
        {
            if (beePurchaseCosts[i] < 0f)
            {
                Debug.LogWarning($"[{name}] Bee purchase cost at tier {i} cannot be negative. Setting to 0.", this);
                beePurchaseCosts[i] = 0f;
            }
        }

        // Ensure all bee counts are positive
        for (int i = 0; i < beesPerPurchase.Length; i++)
        {
            if (beesPerPurchase[i] < 1)
            {
                Debug.LogWarning($"[{name}] Bees per purchase at tier {i} must be at least 1. Setting to 1.", this);
                beesPerPurchase[i] = 1;
            }
        }

        // Ensure max tier matches array lengths
        if (maxPurchaseTier != beePurchaseCosts.Length)
        {
            Debug.LogWarning($"[{name}] Max purchase tier should match array lengths. Updating to {beePurchaseCosts.Length}.", this);
            maxPurchaseTier = beePurchaseCosts.Length;
        }

        // Ensure max tier is positive
        if (maxPurchaseTier < 1)
        {
            Debug.LogWarning($"[{name}] Max purchase tier must be at least 1. Setting to 1.", this);
            maxPurchaseTier = 1;
        }
    }
}
