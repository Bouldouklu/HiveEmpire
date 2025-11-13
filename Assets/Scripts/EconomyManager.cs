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
        transform.parent = null; // Detach from parent to make root GameObject
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
    /// Reset economy to initial state for new year playthrough.
    /// </summary>
    public void ResetToInitialState()
    {
        currentMoney = 0f;
        OnMoneyChanged?.Invoke(currentMoney);

        Debug.Log("[EconomyManager] Reset to initial state - Money: $0");
    }
}
