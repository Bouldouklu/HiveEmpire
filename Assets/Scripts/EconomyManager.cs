using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager for the game's economy system.
/// Tracks player money, handles earning and spending, and notifies listeners of changes.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Economy State")]
    [SerializeField]
    [Tooltip("Current player money balance")]
    private float currentMoney = 0f;

    [SerializeField]
    [Tooltip("Total number of airports placed (for scaling placement costs)")]
    private int totalAirportsPlaced = 0;

    [Header("Airport Placement Costs")]
    [Tooltip("Base cost for Forest/Plains biomes")]
    [SerializeField] private float commonBiomeCost = 10f;

    [Tooltip("Base cost for Mountain/Coastal biomes")]
    [SerializeField] private float mediumBiomeCost = 20f;

    [Tooltip("Base cost for Desert/Tundra biomes")]
    [SerializeField] private float rareBiomeCost = 30f;

    [Tooltip("Cost scaling multiplier per airport placed (e.g., 0.5 = 50% increase per airport)")]
    [SerializeField] private float costScalingMultiplier = 0.5f;

    [Header("Events")]
    [Tooltip("Fired when money amount changes. Passes new total money amount.")]
    public UnityEvent<float> OnMoneyChanged;

    public float CurrentMoney => currentMoney;

    private void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Add money to the player's balance and trigger update event.
    /// </summary>
    /// <param name="amount">Amount of money to earn (must be positive)</param>
    public void EarnMoney(float amount)
    {
        if (amount <= 0f)
        {
            Debug.LogWarning($"EconomyManager: Attempted to earn non-positive amount: {amount}");
            return;
        }

        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);

        Debug.Log($"Earned ${amount}. Total: ${currentMoney:F2}");
    }

    /// <summary>
    /// Deduct money from the player's balance if sufficient funds exist.
    /// </summary>
    /// <param name="amount">Amount of money to spend (must be positive)</param>
    /// <returns>True if transaction succeeded, false if insufficient funds</returns>
    public bool SpendMoney(float amount)
    {
        if (amount <= 0f)
        {
            Debug.LogWarning($"EconomyManager: Attempted to spend non-positive amount: {amount}");
            return false;
        }

        if (!CanAfford(amount))
        {
            Debug.LogWarning($"EconomyManager: Insufficient funds. Need ${amount:F2}, have ${currentMoney:F2}");
            return false;
        }

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);

        Debug.Log($"Spent ${amount}. Remaining: ${currentMoney:F2}");
        return true;
    }

    /// <summary>
    /// Check if player has sufficient funds for a purchase.
    /// </summary>
    /// <param name="cost">Cost to check against current balance</param>
    /// <returns>True if player can afford the cost</returns>
    public bool CanAfford(float cost)
    {
        return currentMoney >= cost;
    }

    /// <summary>
    /// Set money to a specific amount (useful for testing/cheats/save loading).
    /// </summary>
    /// <param name="amount">New money amount</param>
    public void SetMoney(float amount)
    {
        currentMoney = Mathf.Max(0f, amount);
        OnMoneyChanged?.Invoke(currentMoney);

        Debug.Log($"Money set to: ${currentMoney:F2}");
    }

    /// <summary>
    /// Gets the current money balance.
    /// </summary>
    /// <returns>Current money amount</returns>
    public float GetCurrentMoney()
    {
        return currentMoney;
    }

    /// <summary>
    /// Calculates the cost to place an airport based on biome type and scaling.
    /// Cost = baseCost × (1 + scalingMultiplier × airportsPlaced)
    /// </summary>
    /// <param name="biome">Biome type where airport will be placed</param>
    /// <returns>Total cost for placing airport at this biome</returns>
    public float GetAirportPlacementCost(BiomeType biome)
    {
        // Get base cost by biome rarity
        float baseCost = biome switch
        {
            BiomeType.Forest => commonBiomeCost,
            BiomeType.Plains => commonBiomeCost,
            BiomeType.Mountain => mediumBiomeCost,
            BiomeType.Coastal => mediumBiomeCost,
            BiomeType.Desert => rareBiomeCost,
            BiomeType.Tundra => rareBiomeCost,
            _ => commonBiomeCost // Fallback
        };

        // Apply scaling based on total airports placed
        float scaledCost = baseCost * (1f + (costScalingMultiplier * totalAirportsPlaced));

        return Mathf.Round(scaledCost); // Round to whole number for cleaner UI
    }

    /// <summary>
    /// Increments the airport placement counter (called when an airport is successfully placed).
    /// Should be called by PlacementController after spending money.
    /// </summary>
    public void RegisterAirportPlaced()
    {
        totalAirportsPlaced++;
        Debug.Log($"EconomyManager: Total airports placed: {totalAirportsPlaced}");
    }

    /// <summary>
    /// Gets the total number of airports placed.
    /// </summary>
    public int GetTotalAirportsPlaced()
    {
        return totalAirportsPlaced;
    }
}
