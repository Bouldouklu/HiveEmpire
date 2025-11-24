using UnityEngine;

/// <summary>
/// Manages persistent high scores across multiple game sessions using PlayerPrefs.
/// Tracks best performance metrics and allows comparison with current run.
/// </summary>
public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance { get; private set; }

    // PlayerPrefs keys
    private const string KEY_BEST_MONEY_EARNED = "HighScore_MoneyEarned";
    private const string KEY_BEST_ENDING_MONEY = "HighScore_EndingMoney";
    private const string KEY_BEST_TRANSACTION = "HighScore_HighestTransaction";
    private const string KEY_BEST_RECIPES = "HighScore_TotalRecipes";
    private const string KEY_BEST_RESOURCES = "HighScore_TotalResources";
    private const string KEY_BEST_FLOWER_PATCHES = "HighScore_FlowerPatches";
    private const string KEY_BEST_BEE_FLEET = "HighScore_BeeFleet";
    private const string KEY_TOTAL_PLAYS = "Stats_TotalPlays";

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple HighScoreManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.parent = null; // Detach from parent to make root GameObject
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
    /// Check if the current year's stats beat any high scores and save them.
    /// </summary>
    /// <param name="summary">Current year's statistics</param>
    /// <returns>High score summary showing what records were broken</returns>
    public HighScoreComparison SaveIfHighScore(YearSummary summary)
    {
        if (summary == null)
        {
            Debug.LogWarning("[HighScoreManager] Cannot save high scores - YearSummary is null");
            return null;
        }

        HighScoreComparison comparison = new HighScoreComparison();

        // Check and save each stat
        comparison.isNewMoneyEarnedRecord = CheckAndSave(KEY_BEST_MONEY_EARNED, summary.totalMoneyEarned);
        comparison.isNewEndingMoneyRecord = CheckAndSave(KEY_BEST_ENDING_MONEY, summary.endingMoney);
        comparison.isNewTransactionRecord = CheckAndSave(KEY_BEST_TRANSACTION, summary.highestTransaction);
        comparison.isNewRecipesRecord = CheckAndSave(KEY_BEST_RECIPES, summary.totalRecipesCompleted);
        comparison.isNewFlowerPatchesRecord = CheckAndSave(KEY_BEST_FLOWER_PATCHES, summary.flowerPatchesPlaced);
        comparison.isNewBeeFleetRecord = CheckAndSave(KEY_BEST_BEE_FLEET, summary.peakBeeFleetSize);

        // Calculate total resources
        float totalResources = 0f;
        foreach (var kvp in summary.totalResourcesCollected)
        {
            totalResources += kvp.Value;
        }
        comparison.isNewResourcesRecord = CheckAndSave(KEY_BEST_RESOURCES, totalResources);

        // Load best scores for display
        comparison.bestMoneyEarned = PlayerPrefs.GetFloat(KEY_BEST_MONEY_EARNED, 0f);
        comparison.bestEndingMoney = PlayerPrefs.GetFloat(KEY_BEST_ENDING_MONEY, 0f);
        comparison.bestTransaction = PlayerPrefs.GetFloat(KEY_BEST_TRANSACTION, 0f);
        comparison.bestRecipes = PlayerPrefs.GetInt(KEY_BEST_RECIPES, 0);
        comparison.bestResources = PlayerPrefs.GetInt(KEY_BEST_RESOURCES, 0);
        comparison.bestFlowerPatches = PlayerPrefs.GetInt(KEY_BEST_FLOWER_PATCHES, 0);
        comparison.bestBeeFleet = PlayerPrefs.GetInt(KEY_BEST_BEE_FLEET, 0);

        // Increment total plays
        int totalPlays = PlayerPrefs.GetInt(KEY_TOTAL_PLAYS, 0) + 1;
        PlayerPrefs.SetInt(KEY_TOTAL_PLAYS, totalPlays);
        comparison.totalPlays = totalPlays;

        PlayerPrefs.Save();

        int recordsbroken = 0;
        if (comparison.isNewMoneyEarnedRecord) recordsbroken++;
        if (comparison.isNewEndingMoneyRecord) recordsbroken++;
        if (comparison.isNewTransactionRecord) recordsbroken++;
        if (comparison.isNewRecipesRecord) recordsbroken++;
        if (comparison.isNewResourcesRecord) recordsbroken++;
        if (comparison.isNewFlowerPatchesRecord) recordsbroken++;
        if (comparison.isNewBeeFleetRecord) recordsbroken++;

        Debug.Log($"[HighScoreManager] Year {totalPlays} completed. {recordsbroken} new records set!");

        return comparison;
    }

    /// <summary>
    /// Check if a value beats the current high score for a key, and save if it does.
    /// </summary>
    private bool CheckAndSave(string key, float value)
    {
        float currentBest = PlayerPrefs.GetFloat(key, 0f);
        if (value > currentBest)
        {
            PlayerPrefs.SetFloat(key, value);
            Debug.Log($"[HighScoreManager] New record for {key}: {value} (previous: {currentBest})");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if an integer value beats the current high score for a key, and save if it does.
    /// </summary>
    private bool CheckAndSave(string key, int value)
    {
        int currentBest = PlayerPrefs.GetInt(key, 0);
        if (value > currentBest)
        {
            PlayerPrefs.SetInt(key, value);
            Debug.Log($"[HighScoreManager] New record for {key}: {value} (previous: {currentBest})");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get all high scores (for display without comparing to current run).
    /// </summary>
    public HighScoreData GetHighScores()
    {
        return new HighScoreData
        {
            bestMoneyEarned = PlayerPrefs.GetFloat(KEY_BEST_MONEY_EARNED, 0f),
            bestEndingMoney = PlayerPrefs.GetFloat(KEY_BEST_ENDING_MONEY, 0f),
            bestTransaction = PlayerPrefs.GetFloat(KEY_BEST_TRANSACTION, 0f),
            bestRecipes = PlayerPrefs.GetInt(KEY_BEST_RECIPES, 0),
            bestResources = PlayerPrefs.GetInt(KEY_BEST_RESOURCES, 0),
            bestFlowerPatches = PlayerPrefs.GetInt(KEY_BEST_FLOWER_PATCHES, 0),
            bestBeeFleet = PlayerPrefs.GetInt(KEY_BEST_BEE_FLEET, 0),
            totalPlays = PlayerPrefs.GetInt(KEY_TOTAL_PLAYS, 0)
        };
    }

    /// <summary>
    /// Reset all high scores (for debugging or player request).
    /// </summary>
    public void ResetAllHighScores()
    {
        PlayerPrefs.DeleteKey(KEY_BEST_MONEY_EARNED);
        PlayerPrefs.DeleteKey(KEY_BEST_ENDING_MONEY);
        PlayerPrefs.DeleteKey(KEY_BEST_TRANSACTION);
        PlayerPrefs.DeleteKey(KEY_BEST_RECIPES);
        PlayerPrefs.DeleteKey(KEY_BEST_RESOURCES);
        PlayerPrefs.DeleteKey(KEY_BEST_FLOWER_PATCHES);
        PlayerPrefs.DeleteKey(KEY_BEST_BEE_FLEET);
        PlayerPrefs.DeleteKey(KEY_TOTAL_PLAYS);
        PlayerPrefs.Save();

        Debug.Log("[HighScoreManager] All high scores reset");
    }
}

#region Data Structures

/// <summary>
/// Stores current high scores without comparison data.
/// </summary>
public class HighScoreData
{
    public float bestMoneyEarned;
    public float bestEndingMoney;
    public float bestTransaction;
    public int bestRecipes;
    public int bestResources;
    public int bestFlowerPatches;
    public int bestBeeFleet;
    public int totalPlays;
}

/// <summary>
/// Comparison between current run and high scores, showing which records were broken.
/// </summary>
public class HighScoreComparison
{
    // New record flags
    public bool isNewMoneyEarnedRecord;
    public bool isNewEndingMoneyRecord;
    public bool isNewTransactionRecord;
    public bool isNewRecipesRecord;
    public bool isNewResourcesRecord;
    public bool isNewFlowerPatchesRecord;
    public bool isNewBeeFleetRecord;

    // Best scores for display
    public float bestMoneyEarned;
    public float bestEndingMoney;
    public float bestTransaction;
    public int bestRecipes;
    public int bestResources;
    public int bestFlowerPatches;
    public int bestBeeFleet;

    // Meta stats
    public int totalPlays;

    /// <summary>
    /// Check if any records were broken this run.
    /// </summary>
    public bool AnyNewRecords()
    {
        return isNewMoneyEarnedRecord || isNewEndingMoneyRecord || isNewTransactionRecord ||
               isNewRecipesRecord || isNewResourcesRecord || isNewFlowerPatchesRecord ||
               isNewBeeFleetRecord;
    }

    /// <summary>
    /// Get count of how many records were broken.
    /// </summary>
    public int GetNewRecordsCount()
    {
        int count = 0;
        if (isNewMoneyEarnedRecord) count++;
        if (isNewEndingMoneyRecord) count++;
        if (isNewTransactionRecord) count++;
        if (isNewRecipesRecord) count++;
        if (isNewResourcesRecord) count++;
        if (isNewFlowerPatchesRecord) count++;
        if (isNewBeeFleetRecord) count++;
        return count;
    }
}

#endregion
