using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the end-of-year summary panel.
/// Displays year statistics, achievements, high score comparisons, and provides restart functionality.
/// This should be on the UIManager GameObject (not on Canvas).
/// </summary>
public class EndOfYearPanel : MonoBehaviour
{
    public static EndOfYearPanel Instance { get; private set; }

    [Header("Panel References")]
    [Tooltip("Root panel GameObject to show/hide")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Panel blocker (semi-transparent background)")]
    [SerializeField] private GameObject panelBlocker;

    [Header("Header Section")]
    [Tooltip("Main title text (e.g., 'Year Complete!')")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Tooltip("Subtitle text (e.g., 'Year 1 - Your First Season')")]
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Hero Stats Section")]
    [Tooltip("Total money earned display")]
    [SerializeField] private TextMeshProUGUI totalMoneyEarnedText;

    [Tooltip("Total recipes completed display")]
    [SerializeField] private TextMeshProUGUI totalRecipesText;

    [Tooltip("Total resources collected display")]
    [SerializeField] private TextMeshProUGUI totalResourcesText;

    [Header("Economic Stats Section")]
    [Tooltip("Starting money display")]
    [SerializeField] private TextMeshProUGUI startingMoneyText;

    [Tooltip("Ending money display")]
    [SerializeField] private TextMeshProUGUI endingMoneyText;

    [Tooltip("Highest transaction display")]
    [SerializeField] private TextMeshProUGUI highestTransactionText;

    [Header("Empire Stats Section")]
    [Tooltip("Flower patches placed display")]
    [SerializeField] private TextMeshProUGUI flowerPatchesText;

    [Tooltip("Peak bee fleet size display")]
    [SerializeField] private TextMeshProUGUI peakBeeFleetText;

    [Header("Season Breakdown Section")]
    [Tooltip("Spring season money earned")]
    [SerializeField] private TextMeshProUGUI springMoneyText;

    [Tooltip("Spring season recipes completed")]
    [SerializeField] private TextMeshProUGUI springRecipesText;

    [Tooltip("Summer season money earned")]
    [SerializeField] private TextMeshProUGUI summerMoneyText;

    [Tooltip("Summer season recipes completed")]
    [SerializeField] private TextMeshProUGUI summerRecipesText;

    [Tooltip("Autumn season money earned")]
    [SerializeField] private TextMeshProUGUI autumnMoneyText;

    [Tooltip("Autumn season recipes completed")]
    [SerializeField] private TextMeshProUGUI autumnRecipesText;

    [Header("Achievements Section")]
    [Tooltip("Container for achievement entries")]
    [SerializeField] private Transform achievementsContainer;

    [Tooltip("Text showing achievement count (e.g., '5/12 Achievements')")]
    [SerializeField] private TextMeshProUGUI achievementCountText;

    [Tooltip("Prefab for individual achievement display")]
    [SerializeField] private GameObject achievementEntryPrefab;

    [Header("High Score Section")]
    [Tooltip("Text showing high score comparison summary")]
    [SerializeField] private TextMeshProUGUI highScoreSummaryText;

    [Tooltip("Text showing new records message")]
    [SerializeField] private TextMeshProUGUI newRecordsText;

    [Header("Buttons")]
    [Tooltip("Play Again button")]
    [SerializeField] private Button playAgainButton;

    [Tooltip("Quit button (optional)")]
    [SerializeField] private Button quitButton;

    [Header("Colors")]
    [Tooltip("Color for new record indicators")]
    [SerializeField] private Color newRecordColor = Color.yellow;

    [Tooltip("Color for below record values")]
    [SerializeField] private Color belowRecordColor = Color.gray;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple EndOfYearPanel instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Setup button listeners
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }
        else
        {
            Debug.LogWarning("[EndOfYearPanel] Play Again Button is not assigned!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Hide panel by default
        HidePanel();
    }

    private void Start()
    {
        // Verify references
        ValidateReferences();

        // Subscribe to year end event
        TrySubscribeToSeasonManager();
    }

    /// <summary>
    /// Subscribe to SeasonManager's OnYearEnded event.
    /// </summary>
    private void TrySubscribeToSeasonManager()
    {
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnYearEnded.RemoveListener(OnYearEnded);
            SeasonManager.Instance.OnYearEnded.AddListener(OnYearEnded);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnYearEnded.RemoveListener(OnYearEnded);
        }
    }

    /// <summary>
    /// Validate that all required references are assigned.
    /// </summary>
    private void ValidateReferences()
    {
        if (panelRoot == null)
        {
            Debug.LogError("[EndOfYearPanel] Panel Root is not assigned!");
        }

        if (panelBlocker == null)
        {
            Debug.LogError("[EndOfYearPanel] Panel Blocker is not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Called when the year ends - gather stats and show panel.
    /// </summary>
    private void OnYearEnded()
    {
        ShowYearSummary();
    }

    /// <summary>
    /// Gather all statistics and display the year summary panel.
    /// </summary>
    public void ShowYearSummary()
    {
        // Get year stats
        YearSummary summary = null;
        if (YearStatsTracker.Instance != null)
        {
            summary = YearStatsTracker.Instance.GetYearSummary();
        }
        else
        {
            Debug.LogError("[EndOfYearPanel] YearStatsTracker not found!");
            return;
        }

        // Save high scores
        HighScoreComparison highScores = null;
        if (HighScoreManager.Instance != null)
        {
            highScores = HighScoreManager.Instance.SaveIfHighScore(summary);
        }

        // Get achievements
        List<Achievement> achievements = new List<Achievement>();
        if (AchievementManager.Instance != null)
        {
            achievements = AchievementManager.Instance.GetUnlockedAchievements(summary);
        }

        // Display all data
        DisplaySummary(summary, highScores, achievements);

        // Show panel and pause game
        ShowPanel();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Display all summary data in the UI.
    /// </summary>
    private void DisplaySummary(YearSummary summary, HighScoreComparison highScores, List<Achievement> achievements)
    {
        // Header
        if (titleText != null)
        {
            titleText.text = "Year Complete!";
        }

        if (subtitleText != null && highScores != null)
        {
            subtitleText.text = $"Year {highScores.totalPlays}";
        }

        // Hero stats
        if (totalMoneyEarnedText != null)
        {
            totalMoneyEarnedText.text = $"Money Earned: ${summary.totalMoneyEarned:F0}";
        }

        if (totalRecipesText != null)
        {
            totalRecipesText.text = $"Recipes Completed: {summary.totalRecipesCompleted}";
        }

        if (totalResourcesText != null)
        {
            int totalResources = GetTotalResourceCount(summary.totalResourcesCollected);
            totalResourcesText.text = $"Resources Collected: {totalResources}";
        }

        // Economic stats
        if (startingMoneyText != null)
        {
            startingMoneyText.text = $"Starting Money: ${summary.startingMoney:F0}";
        }

        if (endingMoneyText != null)
        {
            endingMoneyText.text = $"Ending Money: ${summary.endingMoney:F0}";
        }

        // Highest transaction removed - not interesting with only 5 recipes
        if (highestTransactionText != null)
        {
            highestTransactionText.gameObject.SetActive(false);
        }

        // Empire stats
        if (flowerPatchesText != null)
        {
            flowerPatchesText.text = $"Flower Patches: {summary.flowerPatchesPlaced}";
        }

        if (peakBeeFleetText != null)
        {
            peakBeeFleetText.text = $"Peak Bee Fleet: {summary.peakBeeFleetSize}";
        }

        // Season breakdown
        DisplaySeasonStats(summary);

        // Achievements
        DisplayAchievements(achievements);

        // High scores
        DisplayHighScores(highScores);
    }

    /// <summary>
    /// Display season-by-season breakdown.
    /// </summary>
    private void DisplaySeasonStats(YearSummary summary)
    {
        // Spring
        if (springMoneyText != null)
        {
            springMoneyText.text = $"Revenue: ${summary.springStats.moneyEarned:F0}";
        }
        if (springRecipesText != null)
        {
            springRecipesText.text = $"Recipes: {summary.springStats.recipesCompleted}";
        }

        // Summer
        if (summerMoneyText != null)
        {
            summerMoneyText.text = $"Revenue: ${summary.summerStats.moneyEarned:F0}";
        }
        if (summerRecipesText != null)
        {
            summerRecipesText.text = $"Recipes: {summary.summerStats.recipesCompleted}";
        }

        // Autumn
        if (autumnMoneyText != null)
        {
            autumnMoneyText.text = $"Revenue: ${summary.autumnStats.moneyEarned:F0}";
        }
        if (autumnRecipesText != null)
        {
            autumnRecipesText.text = $"Recipes: {summary.autumnStats.recipesCompleted}";
        }
    }

    /// <summary>
    /// Display unlocked achievements.
    /// </summary>
    private void DisplayAchievements(List<Achievement> achievements)
    {
        // Clear existing achievement entries
        if (achievementsContainer != null)
        {
            foreach (Transform child in achievementsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Update achievement count
        if (achievementCountText != null && AchievementManager.Instance != null)
        {
            int total = AchievementManager.Instance.GetAllAchievements().Count;
            achievementCountText.text = $"{achievements.Count}/{total} Achievements";
        }

        // Create achievement entries (if prefab is assigned)
        if (achievementEntryPrefab != null && achievementsContainer != null)
        {
            foreach (var achievement in achievements)
            {
                GameObject entry = Instantiate(achievementEntryPrefab, achievementsContainer);

                // Find text component and set achievement info
                TextMeshProUGUI nameText = entry.transform.Find("AchievementName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI descText = entry.transform.Find("AchievementDescription")?.GetComponent<TextMeshProUGUI>();

                if (nameText != null)
                {
                    nameText.text = achievement.achievementName;
                }

                if (descText != null)
                {
                    descText.text = achievement.description;
                }
            }
        }
    }

    /// <summary>
    /// Display high score comparison.
    /// </summary>
    private void DisplayHighScores(HighScoreComparison comparison)
    {
        if (comparison == null)
            return;

        // Hide new records text (feature removed for future redesign)
        if (newRecordsText != null)
        {
            newRecordsText.gameObject.SetActive(false);
        }

        // High score summary
        if (highScoreSummaryText != null)
        {
            StringBuilder summary = new StringBuilder();
            summary.AppendLine("<b>Your Highscore:</b>");
            summary.AppendLine($"Money Earned: ${comparison.bestMoneyEarned:F0}");
            summary.AppendLine($"Recipes: {comparison.bestRecipes}");
            summary.AppendLine($"Resources: {comparison.bestResources}");
            summary.AppendLine($"Flower Patches: {comparison.bestFlowerPatches}");
            summary.AppendLine($"Bee Fleet: {comparison.bestBeeFleet}");

            highScoreSummaryText.text = summary.ToString();
        }
    }

    /// <summary>
    /// Calculate total resources from dictionary.
    /// </summary>
    private int GetTotalResourceCount(Dictionary<ResourceType, int> resources)
    {
        int total = 0;
        foreach (var kvp in resources)
        {
            total += kvp.Value;
        }
        return total;
    }

    /// <summary>
    /// Show the panel.
    /// </summary>
    private void ShowPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            Debug.LogError("[EndOfYearPanel] Panel Root is NULL! Cannot show panel.");
        }

        if (panelBlocker != null)
        {
            panelBlocker.SetActive(true);
        }
        else
        {
            Debug.LogError("[EndOfYearPanel] Panel Blocker is NULL!");
        }
    }

    /// <summary>
    /// Hide the panel.
    /// </summary>
    private void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (panelBlocker != null)
        {
            panelBlocker.SetActive(false);
        }
    }

    /// <summary>
    /// Called when Play Again button is clicked.
    /// </summary>
    private void OnPlayAgainClicked()
    {
        // Hide panel and resume time
        HidePanel();
        Time.timeScale = 1f;

        // Trigger game reset
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetYear();
        }
        else
        {
            Debug.LogError("[EndOfYearPanel] GameManager not found - cannot reset year");
        }
    }

    /// <summary>
    /// Called when Quit button is clicked.
    /// </summary>
    private void OnQuitClicked()
    {
        // Hide panel and resume time
        HidePanel();
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
