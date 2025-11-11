using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks gameplay statistics throughout the year for end-of-year summary.
/// Subscribes to events from various managers to collect cumulative and per-season stats.
/// </summary>
public class YearStatsTracker : MonoBehaviour
{
    public static YearStatsTracker Instance { get; private set; }

    #region Year Statistics

    // Money & Economic Stats
    private float startingMoney;
    private float lastKnownMoney; // Track previous money for delta calculation
    private float totalMoneyEarned;
    private float highestTransaction;

    // Production Statistics
    private int totalRecipesCompleted;
    private Dictionary<string, int> recipesByName = new Dictionary<string, int>();
    private Dictionary<ResourceType, int> totalResourcesCollected = new Dictionary<ResourceType, int>();

    // Empire Building Stats
    private int flowerPatchesPlaced;
    private int peakBeeFleetSize;

    #endregion

    #region Per-Season Statistics

    private class SeasonStats
    {
        public float moneyEarned;
        public int recipesCompleted;
        public Dictionary<ResourceType, int> resourcesCollected = new Dictionary<ResourceType, int>();
    }

    private Dictionary<Season, SeasonStats> seasonalStats = new Dictionary<Season, SeasonStats>();
    private Season currentSeason = Season.Spring;

    #endregion

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple YearStatsTracker instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeStats();
    }

    private void Start()
    {
        // Subscribe to events after all managers are initialized
        SubscribeToEvents();

        // Capture starting money after EconomyManager initializes
        if (EconomyManager.Instance != null)
        {
            startingMoney = EconomyManager.Instance.CurrentMoney;
            lastKnownMoney = startingMoney;
        }
        else
        {
            Debug.LogError("[YearStatsTracker] EconomyManager not found - money tracking will not work!");
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Initialize or reset all statistics to starting values.
    /// </summary>
    private void InitializeStats()
    {
        startingMoney = 0f;
        lastKnownMoney = 0f;
        totalMoneyEarned = 0f;
        highestTransaction = 0f;
        totalRecipesCompleted = 0;
        recipesByName.Clear();
        totalResourcesCollected.Clear();
        flowerPatchesPlaced = 0;
        peakBeeFleetSize = 0;

        // Initialize seasonal stats for all 3 playable seasons
        seasonalStats.Clear();
        seasonalStats[Season.Spring] = new SeasonStats();
        seasonalStats[Season.Summer] = new SeasonStats();
        seasonalStats[Season.Autumn] = new SeasonStats();

        currentSeason = Season.Spring;

        Debug.Log("[YearStatsTracker] Stats initialized");
    }

    /// <summary>
    /// Subscribe to events from all manager systems.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
            EconomyManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
        }

        if (RecipeProductionManager.Instance != null)
        {
            RecipeProductionManager.Instance.OnRecipeCompleted.RemoveListener(OnRecipeCompleted);
            RecipeProductionManager.Instance.OnRecipeCompleted.AddListener(OnRecipeCompleted);
        }

        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.RemoveListener(OnResourcesChanged);
            HiveController.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
        }

        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.RemoveListener(OnSeasonChanged);
            SeasonManager.Instance.OnSeasonChanged.AddListener(OnSeasonChanged);
        }
    }

    /// <summary>
    /// Unsubscribe from all manager events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
        }

        if (RecipeProductionManager.Instance != null)
        {
            RecipeProductionManager.Instance.OnRecipeCompleted.RemoveListener(OnRecipeCompleted);
        }

        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.RemoveListener(OnResourcesChanged);
        }

        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.RemoveListener(OnSeasonChanged);
        }
    }

    #region Event Handlers

    /// <summary>
    /// Track money changes (we only count increases as "earned").
    /// </summary>
    private void OnMoneyChanged(float newTotal)
    {
        // Calculate change from last known value
        float change = newTotal - lastKnownMoney;

        // Only track positive changes (money earned, not spent)
        if (change > 0f)
        {
            totalMoneyEarned += change;

            // Track per-season
            if (seasonalStats.ContainsKey(currentSeason))
            {
                seasonalStats[currentSeason].moneyEarned += change;
            }

            // Track highest transaction
            if (change > highestTransaction)
            {
                highestTransaction = change;
            }
        }

        // Update last known money
        lastKnownMoney = newTotal;
    }

    /// <summary>
    /// Track recipe completions by type and count.
    /// </summary>
    private void OnRecipeCompleted(HoneyRecipe recipe, float honeyValue)
    {
        if (recipe == null)
            return;

        // Increment total count
        totalRecipesCompleted++;

        // Track by recipe name
        string recipeName = recipe.recipeName;
        if (!recipesByName.ContainsKey(recipeName))
        {
            recipesByName[recipeName] = 0;
        }
        recipesByName[recipeName]++;

        // Track per-season
        if (seasonalStats.ContainsKey(currentSeason))
        {
            seasonalStats[currentSeason].recipesCompleted++;
        }
    }

    /// <summary>
    /// Track resources collected from bees.
    /// Note: OnResourcesChanged fires for both additions and consumptions.
    /// We track additions by comparing inventory increases.
    /// </summary>
    private void OnResourcesChanged()
    {
        if (HiveController.Instance == null)
            return;

        // Get current inventory
        var currentInventory = HiveController.Instance.GetPollenInventory();

        // Track peak bee fleet size
        if (BeeFleetManager.Instance != null)
        {
            int currentBeeCount = BeeFleetManager.Instance.TotalBeesOwned;
            if (currentBeeCount > peakBeeFleetSize)
            {
                peakBeeFleetSize = currentBeeCount;
            }
        }

        // Track flower patches placed
        if (EconomyManager.Instance != null)
        {
            flowerPatchesPlaced = EconomyManager.Instance.GetTotalFlowerPatchsPlaced();
        }

        // Note: We're tracking this event, but detailed resource counting
        // would require comparing previous vs current inventory.
        // For now, we'll track resources via a separate method if needed.
    }

    /// <summary>
    /// Track season changes to update per-season stats.
    /// </summary>
    private void OnSeasonChanged(Season newSeason)
    {
        currentSeason = newSeason;
        Debug.Log($"[YearStatsTracker] Season changed to: {newSeason}");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Manually track resources collected (call this when bees deliver pollen).
    /// </summary>
    public void RecordResourcesCollected(List<ResourceType> resources)
    {
        if (resources == null || resources.Count == 0)
            return;

        foreach (ResourceType resource in resources)
        {
            // Track total
            if (!totalResourcesCollected.ContainsKey(resource))
            {
                totalResourcesCollected[resource] = 0;
            }
            totalResourcesCollected[resource]++;

            // Track per-season
            if (seasonalStats.ContainsKey(currentSeason))
            {
                var seasonStats = seasonalStats[currentSeason];
                if (!seasonStats.resourcesCollected.ContainsKey(resource))
                {
                    seasonStats.resourcesCollected[resource] = 0;
                }
                seasonStats.resourcesCollected[resource]++;
            }
        }
    }

    /// <summary>
    /// Get a complete summary of the year's statistics.
    /// </summary>
    public YearSummary GetYearSummary()
    {
        float endingMoney = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentMoney : 0f;

        return new YearSummary
        {
            // Money stats
            startingMoney = this.startingMoney,
            endingMoney = endingMoney,
            totalMoneyEarned = this.totalMoneyEarned,
            highestTransaction = this.highestTransaction,

            // Production stats
            totalRecipesCompleted = this.totalRecipesCompleted,
            recipesByName = new Dictionary<string, int>(this.recipesByName),
            totalResourcesCollected = new Dictionary<ResourceType, int>(this.totalResourcesCollected),

            // Empire stats
            flowerPatchesPlaced = this.flowerPatchesPlaced,
            peakBeeFleetSize = this.peakBeeFleetSize,

            // Seasonal breakdown
            springStats = GetSeasonStatsSummary(Season.Spring),
            summerStats = GetSeasonStatsSummary(Season.Summer),
            autumnStats = GetSeasonStatsSummary(Season.Autumn)
        };
    }

    /// <summary>
    /// Get stats summary for a specific season.
    /// </summary>
    private SeasonStatsSummary GetSeasonStatsSummary(Season season)
    {
        if (!seasonalStats.ContainsKey(season))
        {
            return new SeasonStatsSummary
            {
                seasonName = season.ToString(),
                moneyEarned = 0f,
                recipesCompleted = 0,
                resourcesCollected = new Dictionary<ResourceType, int>()
            };
        }

        var stats = seasonalStats[season];
        return new SeasonStatsSummary
        {
            seasonName = season.ToString(),
            moneyEarned = stats.moneyEarned,
            recipesCompleted = stats.recipesCompleted,
            resourcesCollected = new Dictionary<ResourceType, int>(stats.resourcesCollected)
        };
    }

    /// <summary>
    /// Reset all statistics (call when starting a new year).
    /// </summary>
    public void ResetStats()
    {
        InitializeStats();

        // Capture new starting money and reset tracking
        if (EconomyManager.Instance != null)
        {
            startingMoney = EconomyManager.Instance.CurrentMoney;
            lastKnownMoney = startingMoney;
        }

        Debug.Log("[YearStatsTracker] Stats reset for new year");
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Complete summary of the year's statistics.
/// </summary>
public class YearSummary
{
    // Money & Economic
    public float startingMoney;
    public float endingMoney;
    public float totalMoneyEarned;
    public float highestTransaction;

    // Production
    public int totalRecipesCompleted;
    public Dictionary<string, int> recipesByName;
    public Dictionary<ResourceType, int> totalResourcesCollected;

    // Empire Building
    public int flowerPatchesPlaced;
    public int peakBeeFleetSize;

    // Seasonal Breakdown
    public SeasonStatsSummary springStats;
    public SeasonStatsSummary summerStats;
    public SeasonStatsSummary autumnStats;
}

/// <summary>
/// Statistics for a single season.
/// </summary>
public class SeasonStatsSummary
{
    public string seasonName;
    public float moneyEarned;
    public int recipesCompleted;
    public Dictionary<ResourceType, int> resourcesCollected;
}

#endregion
