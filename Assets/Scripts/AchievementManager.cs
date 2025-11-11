using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages all achievements and checks which ones are unlocked based on year statistics.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Achievement Definitions")]
    [Tooltip("List of all possible achievements in the game")]
    [SerializeField]
    private List<Achievement> allAchievements = new List<Achievement>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple AchievementManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Check which achievements were unlocked based on the year's statistics.
    /// </summary>
    /// <param name="summary">Year summary containing all stats</param>
    /// <returns>List of unlocked achievements</returns>
    public List<Achievement> GetUnlockedAchievements(YearSummary summary)
    {
        if (summary == null)
        {
            Debug.LogWarning("[AchievementManager] Cannot check achievements - YearSummary is null");
            return new List<Achievement>();
        }

        List<Achievement> unlocked = new List<Achievement>();

        foreach (var achievement in allAchievements)
        {
            if (achievement != null && achievement.IsUnlocked(summary))
            {
                unlocked.Add(achievement);
                Debug.Log($"[AchievementManager] Achievement Unlocked: {achievement.achievementName}");
            }
        }

        Debug.Log($"[AchievementManager] Total achievements unlocked: {unlocked.Count}/{allAchievements.Count}");
        return unlocked;
    }

    /// <summary>
    /// Get all available achievements (for debugging/UI).
    /// </summary>
    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(allAchievements);
    }

    /// <summary>
    /// Get achievement completion percentage (0-100).
    /// </summary>
    public float GetCompletionPercentage(YearSummary summary)
    {
        if (allAchievements.Count == 0)
            return 0f;

        int unlockedCount = GetUnlockedAchievements(summary).Count;
        return (float)unlockedCount / allAchievements.Count * 100f;
    }

    /// <summary>
    /// Get achievements grouped by category for display.
    /// </summary>
    public Dictionary<AchievementType, List<Achievement>> GetAchievementsByCategory(YearSummary summary)
    {
        var grouped = new Dictionary<AchievementType, List<Achievement>>();

        foreach (var achievement in allAchievements)
        {
            if (achievement == null)
                continue;

            if (!grouped.ContainsKey(achievement.achievementType))
            {
                grouped[achievement.achievementType] = new List<Achievement>();
            }

            // Only add unlocked achievements
            if (achievement.IsUnlocked(summary))
            {
                grouped[achievement.achievementType].Add(achievement);
            }
        }

        return grouped;
    }
}
