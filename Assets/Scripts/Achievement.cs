using UnityEngine;

/// <summary>
/// Defines a single achievement milestone that can be unlocked based on year statistics.
/// </summary>
[CreateAssetMenu(fileName = "Achievement", menuName = "Game/Achievement", order = 1)]
public class Achievement : ScriptableObject
{
    [Header("Achievement Info")]
    [Tooltip("Unique identifier for this achievement")]
    public string achievementId;

    [Tooltip("Display name shown to player")]
    public string achievementName;

    [Tooltip("Description of what was accomplished")]
    [TextArea(2, 4)]
    public string description;

    [Header("Unlock Conditions")]
    [Tooltip("Type of stat this achievement tracks")]
    public AchievementType achievementType;

    [Tooltip("Threshold value required to unlock (e.g., 1000 for $1000 earned)")]
    public float unlockThreshold;

    /// <summary>
    /// Check if this achievement is unlocked based on year summary stats.
    /// </summary>
    public bool IsUnlocked(YearSummary summary)
    {
        if (summary == null)
            return false;

        return achievementType switch
        {
            AchievementType.TotalMoneyEarned => summary.totalMoneyEarned >= unlockThreshold,
            AchievementType.EndingMoney => summary.endingMoney >= unlockThreshold,
            AchievementType.HighestTransaction => summary.highestTransaction >= unlockThreshold,
            AchievementType.TotalRecipes => summary.totalRecipesCompleted >= unlockThreshold,
            AchievementType.FlowerPatches => summary.flowerPatchesPlaced >= unlockThreshold,
            AchievementType.PeakBeeFleet => summary.peakBeeFleetSize >= unlockThreshold,
            AchievementType.TotalResources => GetTotalResourcesCount(summary) >= unlockThreshold,
            _ => false
        };
    }

    /// <summary>
    /// Get total resources collected across all types.
    /// </summary>
    private float GetTotalResourcesCount(YearSummary summary)
    {
        float total = 0f;
        foreach (var kvp in summary.totalResourcesCollected)
        {
            total += kvp.Value;
        }
        return total;
    }
}

/// <summary>
/// Types of achievements based on different stats categories.
/// </summary>
public enum AchievementType
{
    TotalMoneyEarned,
    EndingMoney,
    HighestTransaction,
    TotalRecipes,
    FlowerPatches,
    PeakBeeFleet,
    TotalResources
}
