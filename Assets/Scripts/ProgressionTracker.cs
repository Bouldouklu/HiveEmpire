using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Tracks progression metrics throughout the campaign to validate game balance.
/// Monitors decision density, completion percentage, and spending patterns.
/// Provides data for end-of-year summary and balance validation.
/// </summary>
public class ProgressionTracker : MonoBehaviour
{
    public static ProgressionTracker Instance { get; private set; }

    [Header("Decision Tracking")]
    [SerializeField]
    [Tooltip("Decisions made in the current week")]
    private int decisionsThisWeek = 0;

    [SerializeField]
    [Tooltip("Total decisions made this campaign")]
    private int totalDecisionsMade = 0;

    [SerializeField]
    [Tooltip("Decision count per week (for wave pattern analysis)")]
    private List<int> decisionsPerWeek = new List<int>();

    [Header("Completion Tracking")]
    [SerializeField]
    [Tooltip("Number of flower patches unlocked")]
    private int patchesUnlocked = 0;

    [SerializeField]
    [Tooltip("Number of recipes unlocked")]
    private int recipesUnlocked = 0;

    [SerializeField]
    [Tooltip("Current bee purchase tier")]
    private int beePurchaseTier = 0;

    [SerializeField]
    [Tooltip("Total capacity upgrades purchased across all patches")]
    private int totalCapacityUpgrades = 0;

    [SerializeField]
    [Tooltip("Total recipe upgrades purchased across all recipes")]
    private int totalRecipeUpgrades = 0;

    [Header("Spending Tracking")]
    [SerializeField]
    [Tooltip("Total money spent on flower patch unlocks")]
    private float moneySpentOnPatches = 0f;

    [SerializeField]
    [Tooltip("Total money spent on recipe unlocks")]
    private float moneySpentOnRecipes = 0f;

    [SerializeField]
    [Tooltip("Total money spent on bee purchases")]
    private float moneySpentOnBees = 0f;

    [SerializeField]
    [Tooltip("Total money spent on capacity upgrades")]
    private float moneySpentOnCapacity = 0f;

    [SerializeField]
    [Tooltip("Total money spent on recipe upgrades")]
    private float moneySpentOnRecipeUpgrades = 0f;

    [Header("Balance Validation")]
    [Tooltip("Maximum possible decisions (6 patches + 12 recipes + 10 bee tiers + 30 capacity + 60 recipe upgrades)")]
    private const int MAX_POSSIBLE_DECISIONS = 118;

    [Tooltip("Target completion percentage (60-80%)")]
    [SerializeField]
    private float targetCompletionMin = 60f;

    [SerializeField]
    private float targetCompletionMax = 80f;

    [Header("Events")]
    public UnityEvent<int> OnDecisionMade; // Passes total decisions
    public UnityEvent<float> OnCompletionPercentageChanged; // Passes completion %

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to game events
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    #region Event Subscription

    private void SubscribeToEvents()
    {
        // Subscribe to SeasonManager week changes to track decisions per week
        if (SeasonManager.Instance != null)
        {
            // Note: SeasonManager would need to expose OnWeekChanged event
            // For now, we'll track manually via LogDecision calls
        }
    }

    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from events when destroyed
    }

    #endregion

    #region Decision Logging

    /// <summary>
    /// Logs a flower patch unlock decision.
    /// </summary>
    /// <param name="cost">Cost of the patch unlock</param>
    public void LogPatchUnlock(float cost)
    {
        patchesUnlocked++;
        moneySpentOnPatches += cost;
        LogDecision($"Patch Unlock (${cost})");
    }

    /// <summary>
    /// Logs a recipe unlock decision.
    /// </summary>
    /// <param name="cost">Cost of the recipe unlock</param>
    public void LogRecipeUnlock(float cost)
    {
        recipesUnlocked++;
        moneySpentOnRecipes += cost;
        LogDecision($"Recipe Unlock (${cost})");
    }

    /// <summary>
    /// Logs a bee purchase decision.
    /// </summary>
    /// <param name="cost">Cost of the bee purchase</param>
    /// <param name="beesAdded">Number of bees added</param>
    public void LogBeePurchase(float cost, int beesAdded)
    {
        beePurchaseTier++;
        moneySpentOnBees += cost;
        LogDecision($"Bee Purchase Tier {beePurchaseTier} (${cost}, +{beesAdded} bees)");
    }

    /// <summary>
    /// Logs a capacity upgrade decision.
    /// </summary>
    /// <param name="cost">Cost of the capacity upgrade</param>
    /// <param name="patchName">Name of the flower patch</param>
    public void LogCapacityUpgrade(float cost, string patchName)
    {
        totalCapacityUpgrades++;
        moneySpentOnCapacity += cost;
        LogDecision($"Capacity Upgrade on {patchName} (${cost})");
    }

    /// <summary>
    /// Logs a recipe upgrade decision.
    /// </summary>
    /// <param name="cost">Cost of the recipe upgrade</param>
    /// <param name="recipeName">Name of the recipe</param>
    /// <param name="newTier">New tier level</param>
    public void LogRecipeUpgrade(float cost, string recipeName, int newTier)
    {
        totalRecipeUpgrades++;
        moneySpentOnRecipeUpgrades += cost;
        LogDecision($"Recipe Upgrade: {recipeName} → Tier {newTier} (${cost})");
    }

    /// <summary>
    /// Generic decision logging (internal).
    /// </summary>
    private void LogDecision(string decisionDescription)
    {
        decisionsThisWeek++;
        totalDecisionsMade++;

        Debug.Log($"[ProgressionTracker] Decision #{totalDecisionsMade}: {decisionDescription}");

        OnDecisionMade?.Invoke(totalDecisionsMade);
        UpdateCompletionPercentage();
    }

    /// <summary>
    /// Called when a week advances - resets weekly decision counter and logs to history.
    /// Should be called by SeasonManager when week changes.
    /// </summary>
    public void OnWeekAdvanced()
    {
        decisionsPerWeek.Add(decisionsThisWeek);
        Debug.Log($"[ProgressionTracker] Week completed: {decisionsThisWeek} decisions made");
        decisionsThisWeek = 0;
    }

    #endregion

    #region Progression Metrics

    /// <summary>
    /// Calculates current completion percentage based on decisions made.
    /// </summary>
    public float GetCompletionPercentage()
    {
        return (totalDecisionsMade / (float)MAX_POSSIBLE_DECISIONS) * 100f;
    }

    /// <summary>
    /// Updates completion percentage and fires event.
    /// </summary>
    private void UpdateCompletionPercentage()
    {
        float completionPercentage = GetCompletionPercentage();
        OnCompletionPercentageChanged?.Invoke(completionPercentage);
    }

    /// <summary>
    /// Checks if player is within target completion range (60-80%).
    /// </summary>
    public bool IsWithinTargetCompletion()
    {
        float completion = GetCompletionPercentage();
        return completion >= targetCompletionMin && completion <= targetCompletionMax;
    }

    /// <summary>
    /// Gets average decisions per week.
    /// </summary>
    public float GetAverageDecisionsPerWeek()
    {
        if (decisionsPerWeek.Count == 0) return 0f;
        int total = 0;
        foreach (int count in decisionsPerWeek)
        {
            total += count;
        }
        return total / (float)decisionsPerWeek.Count;
    }

    /// <summary>
    /// Gets decision density for a specific week (0-indexed).
    /// </summary>
    public int GetDecisionsForWeek(int weekIndex)
    {
        if (weekIndex < 0 || weekIndex >= decisionsPerWeek.Count)
        {
            return 0;
        }
        return decisionsPerWeek[weekIndex];
    }

    /// <summary>
    /// Gets total decisions made this campaign.
    /// </summary>
    public int GetTotalDecisions()
    {
        return totalDecisionsMade;
    }

    /// <summary>
    /// Gets total money spent across all categories.
    /// </summary>
    public float GetTotalMoneySpent()
    {
        return moneySpentOnPatches + moneySpentOnRecipes + moneySpentOnBees + moneySpentOnCapacity + moneySpentOnRecipeUpgrades;
    }

    /// <summary>
    /// Gets spending breakdown as a dictionary.
    /// </summary>
    public Dictionary<string, float> GetSpendingBreakdown()
    {
        return new Dictionary<string, float>
        {
            { "Flower Patches", moneySpentOnPatches },
            { "Recipe Unlocks", moneySpentOnRecipes },
            { "Bee Purchases", moneySpentOnBees },
            { "Capacity Upgrades", moneySpentOnCapacity },
            { "Recipe Upgrades", moneySpentOnRecipeUpgrades }
        };
    }

    #endregion

    #region Debug & Validation

    /// <summary>
    /// Logs progression summary to console (useful for balance validation).
    /// </summary>
    [ContextMenu("Log Progression Summary")]
    public void LogProgressionSummary()
    {
        Debug.Log("=== PROGRESSION TRACKER SUMMARY ===");
        Debug.Log($"Total Decisions: {totalDecisionsMade}/{MAX_POSSIBLE_DECISIONS}");
        Debug.Log($"Completion: {GetCompletionPercentage():F1}% (Target: {targetCompletionMin}-{targetCompletionMax}%)");
        Debug.Log($"Within Target: {(IsWithinTargetCompletion() ? "YES" : "NO")}");
        Debug.Log($"Avg Decisions/Week: {GetAverageDecisionsPerWeek():F1}");
        Debug.Log($"\nUnlocks:");
        Debug.Log($"  - Patches: {patchesUnlocked}/6");
        Debug.Log($"  - Recipes: {recipesUnlocked}/12");
        Debug.Log($"  - Bee Tier: {beePurchaseTier}/10");
        Debug.Log($"\nUpgrades:");
        Debug.Log($"  - Capacity: {totalCapacityUpgrades}/30 (max: 6 patches × 5 tiers)");
        Debug.Log($"  - Recipe: {totalRecipeUpgrades}/60 (max: 12 recipes × 5 tiers)");
        Debug.Log($"\nSpending:");
        Debug.Log($"  - Patches: ${moneySpentOnPatches:F2}");
        Debug.Log($"  - Recipe Unlocks: ${moneySpentOnRecipes:F2}");
        Debug.Log($"  - Bees: ${moneySpentOnBees:F2}");
        Debug.Log($"  - Capacity: ${moneySpentOnCapacity:F2}");
        Debug.Log($"  - Recipe Upgrades: ${moneySpentOnRecipeUpgrades:F2}");
        Debug.Log($"  - TOTAL: ${GetTotalMoneySpent():F2}");
        Debug.Log($"\nDecisions Per Week:");
        for (int i = 0; i < decisionsPerWeek.Count; i++)
        {
            Debug.Log($"  Week {i + 1}: {decisionsPerWeek[i]} decisions");
        }
        Debug.Log("===================================");
    }

    /// <summary>
    /// Validates balance against design goals.
    /// </summary>
    [ContextMenu("Validate Balance")]
    public void ValidateBalance()
    {
        Debug.Log("=== BALANCE VALIDATION ===");

        // Check completion percentage
        float completion = GetCompletionPercentage();
        bool completionValid = IsWithinTargetCompletion();
        Debug.Log($"✓ Completion: {completion:F1}% {(completionValid ? "[PASS]" : "[FAIL]")} (Target: {targetCompletionMin}-{targetCompletionMax}%)");

        // Check decision density
        float avgDecisionsPerWeek = GetAverageDecisionsPerWeek();
        bool decisionDensityValid = avgDecisionsPerWeek >= 3f && avgDecisionsPerWeek <= 10f;
        Debug.Log($"✓ Avg Decisions/Week: {avgDecisionsPerWeek:F1} {(decisionDensityValid ? "[PASS]" : "[FAIL]")} (Target: 3-10)");

        // Check wave pattern (alternating busy/calm weeks)
        bool hasWavePattern = CheckWavePattern();
        Debug.Log($"✓ Wave Pattern: {(hasWavePattern ? "[PASS]" : "[FAIL]")} (Busy/Calm alternation)");

        // Overall validation
        bool overallValid = completionValid && decisionDensityValid && hasWavePattern;
        Debug.Log($"\n{(overallValid ? "✅ BALANCE VALIDATED" : "⚠️ BALANCE NEEDS ADJUSTMENT")}");
        Debug.Log("==========================");
    }

    /// <summary>
    /// Checks if decision pattern shows wave behavior (busy/calm alternation).
    /// </summary>
    private bool CheckWavePattern()
    {
        if (decisionsPerWeek.Count < 6) return false; // Need enough data

        int busyWeeks = 0;
        int calmWeeks = 0;

        foreach (int decisions in decisionsPerWeek)
        {
            if (decisions >= 6) busyWeeks++;
            else if (decisions <= 2) calmWeeks++;
        }

        // Wave pattern exists if we have both busy and calm weeks
        return busyWeeks >= 3 && calmWeeks >= 3;
    }

    #endregion

    #region Reset

    /// <summary>
    /// Resets progression tracker for a new campaign.
    /// </summary>
    public void ResetProgressionTracking()
    {
        decisionsThisWeek = 0;
        totalDecisionsMade = 0;
        decisionsPerWeek.Clear();

        patchesUnlocked = 0;
        recipesUnlocked = 0;
        beePurchaseTier = 0;
        totalCapacityUpgrades = 0;
        totalRecipeUpgrades = 0;

        moneySpentOnPatches = 0f;
        moneySpentOnRecipes = 0f;
        moneySpentOnBees = 0f;
        moneySpentOnCapacity = 0f;
        moneySpentOnRecipeUpgrades = 0f;

        Debug.Log("[ProgressionTracker] Reset for new campaign");
    }

    #endregion
}
