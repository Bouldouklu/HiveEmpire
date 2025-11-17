using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages honey production from recipes. Handles priority-based resource allocation,
/// production timing, and income generation when recipes complete.
/// </summary>
public class RecipeProductionManager : MonoBehaviour
{
    public static RecipeProductionManager Instance { get; private set; }

    [Header("Recipe Configuration")]
    [Tooltip("Active recipes - populated automatically from RecipeProgressionManager (unlocked recipes only)")]
    private List<HoneyRecipe> activeRecipes = new List<HoneyRecipe>();

    [Header("Events")]
    [Tooltip("Fired when a recipe completes production (passes recipe and honey value)")]
    public UnityEvent<HoneyRecipe, float> OnRecipeCompleted;

    // Track production state for each recipe
    private class ProductionState
    {
        public bool isProducing;
        public bool isPaused;
        public float timeRemaining;
        public float totalTime;
        public int tier; // Current upgrade tier when production started
    }

    private Dictionary<HoneyRecipe, ProductionState> productionStates = new Dictionary<HoneyRecipe, ProductionState>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple RecipeProductionManager instances detected. Destroying duplicate.");
            Destroy(this);
            return;
        }
        Instance = this;

        // Initialize production states
        InitializeRecipes();
    }

    private void Start()
    {
        // Subscribe to recipe unlock events
        if (RecipeProgressionManager.Instance != null)
        {
            RecipeProgressionManager.Instance.OnRecipeUnlocked.AddListener(OnRecipeUnlocked);

            // Initialize with all currently unlocked recipes (handles isUnlockedByDefault recipes)
            List<HoneyRecipe> unlockedRecipes = RecipeProgressionManager.Instance.GetAllUnlockedRecipes();
            foreach (var recipe in unlockedRecipes)
            {
                if (recipe != null && !activeRecipes.Contains(recipe))
                {
                    AddRecipe(recipe);
                    Debug.Log($"RecipeProductionManager: Added default-unlocked recipe '{recipe.recipeName}' to production");
                }
            }
        }
        else
        {
            Debug.LogWarning("RecipeProductionManager: RecipeProgressionManager not found! No recipes will be available.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Unsubscribe from events
        if (RecipeProgressionManager.Instance != null)
        {
            RecipeProgressionManager.Instance.OnRecipeUnlocked.RemoveListener(OnRecipeUnlocked);
        }
    }

    /// <summary>
    /// Callback when a recipe is unlocked - add it to active recipes automatically.
    /// </summary>
    private void OnRecipeUnlocked(HoneyRecipe recipe)
    {
        if (recipe != null && !activeRecipes.Contains(recipe))
        {
            AddRecipe(recipe);
            Debug.Log($"RecipeProductionManager: Auto-added unlocked recipe '{recipe.recipeName}' to production");
        }
    }

    private void Update()
    {
        // Update all active productions
        UpdateProductionTimers();

        // Try to start new recipes if ingredients are available
        TryStartRecipes();
    }

    /// <summary>
    /// Initialize production state tracking for all recipes.
    /// Called on Awake (may be empty) and when recipe list changes.
    /// </summary>
    private void InitializeRecipes()
    {
        productionStates.Clear();

        // activeRecipes may be empty on startup - will be populated in Start()
        foreach (var recipe in activeRecipes)
        {
            if (recipe != null)
            {
                productionStates[recipe] = new ProductionState
                {
                    isProducing = false,
                    isPaused = false,
                    timeRemaining = 0f,
                    totalTime = recipe.productionTimeSeconds,
                    tier = 0
                };
            }
        }
    }

    /// <summary>
    /// Update production timers and complete recipes when ready.
    /// </summary>
    private void UpdateProductionTimers()
    {
        List<HoneyRecipe> completedRecipes = new List<HoneyRecipe>();

        foreach (var kvp in productionStates)
        {
            var recipe = kvp.Key;
            var state = kvp.Value;

            // Only update timer if producing and not paused
            if (state.isProducing && !state.isPaused)
            {
                state.timeRemaining -= Time.deltaTime;

                if (state.timeRemaining <= 0f)
                {
                    completedRecipes.Add(recipe);
                }
            }
        }

        // Complete all finished recipes
        foreach (var recipe in completedRecipes)
        {
            CompleteRecipe(recipe);
        }
    }

    /// <summary>
    /// Try to start production for recipes that have ingredients available.
    /// Uses priority-based resource allocation.
    /// Only processes unlocked recipes.
    /// </summary>
    private void TryStartRecipes()
    {
        if (HiveController.Instance == null || RecipeProgressionManager.Instance == null)
            return;

        // Get current inventory snapshot
        var inventory = HiveController.Instance.GetPollenInventory();

        // Create a working copy for allocation simulation
        Dictionary<ResourceType, int> availableResources = new Dictionary<ResourceType, int>(inventory);

        // Process recipes in priority order (top of list = highest priority)
        foreach (var recipe in activeRecipes)
        {
            if (recipe == null)
                continue;

            // Skip locked recipes
            if (!RecipeProgressionManager.Instance.IsRecipeUnlocked(recipe))
                continue;

            var state = productionStates[recipe];

            // Skip if already producing or paused
            if (state.isProducing || state.isPaused)
                continue;

            // Get current upgrade tier for this recipe
            int tier = RecipeProgressionManager.Instance.GetRecipeTier(recipe);

            // Check if we can produce with currently available resources (tier-adjusted)
            if (CanProduceWithResources(recipe, tier, availableResources))
            {
                // Consume resources from working copy (for allocation)
                ConsumeResourcesFromPool(recipe, tier, availableResources);

                // Get tier-adjusted ingredients
                List<HoneyRecipe.Ingredient> adjustedIngredients = recipe.GetIngredients(tier);

                // Actually consume from hive inventory
                if (HiveController.Instance.TryConsumeResources(adjustedIngredients))
                {
                    // Start production with tier
                    StartProduction(recipe, tier);
                }
                else
                {
                    Debug.LogWarning($"Failed to consume resources for recipe '{recipe.recipeName}' despite allocation check passing.");
                }
            }
        }
    }

    /// <summary>
    /// Check if recipe can be produced with given resource pool (tier-adjusted).
    /// </summary>
    private bool CanProduceWithResources(HoneyRecipe recipe, int tier, Dictionary<ResourceType, int> resources)
    {
        List<HoneyRecipe.Ingredient> adjustedIngredients = recipe.GetIngredients(tier);
        foreach (var ingredient in adjustedIngredients)
        {
            if (!resources.ContainsKey(ingredient.pollenType) ||
                resources[ingredient.pollenType] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Consume resources from a resource pool (for allocation simulation, tier-adjusted).
    /// </summary>
    private void ConsumeResourcesFromPool(HoneyRecipe recipe, int tier, Dictionary<ResourceType, int> resources)
    {
        List<HoneyRecipe.Ingredient> adjustedIngredients = recipe.GetIngredients(tier);
        foreach (var ingredient in adjustedIngredients)
        {
            resources[ingredient.pollenType] -= ingredient.quantity;
        }
    }

    /// <summary>
    /// Start production for a recipe with tier and seasonal modifiers applied.
    /// </summary>
    private void StartProduction(HoneyRecipe recipe, int tier)
    {
        var state = productionStates[recipe];
        state.isProducing = true;
        state.tier = tier; // Store tier for completion

        // Get tier-adjusted production time
        float baseTierTime = recipe.GetProductionTime(tier);
        // Apply seasonal production time modifier on top of tier adjustment
        float modifiedTime = CalculateSeasonalProductionTime(baseTierTime);

        state.timeRemaining = modifiedTime;
        state.totalTime = modifiedTime;

        // Get tier-adjusted value for logging
        float tierValue = recipe.GetHoneyValue(tier);

        if (SeasonManager.Instance != null)
        {
            Debug.Log($"Started production: {recipe.recipeName} Tier {tier} (${tierValue:F2}, {modifiedTime:F1}s, base: {baseTierTime}s, season: {SeasonManager.Instance.CurrentSeason})");
        }
        else
        {
            Debug.Log($"Started production: {recipe.recipeName} Tier {tier} (${tierValue:F2}, {modifiedTime:F1}s)");
        }
    }

    /// <summary>
    /// Calculate the modified production time with seasonal modifier applied.
    /// </summary>
    private float CalculateSeasonalProductionTime(float baseTime)
    {
        if (SeasonManager.Instance == null)
            return baseTime;

        SeasonData currentSeason = SeasonManager.Instance.GetCurrentSeasonData();
        if (currentSeason == null)
            return baseTime;

        // Apply seasonal production time modifier
        float modifiedTime = baseTime * currentSeason.productionTimeModifier;

        return modifiedTime;
    }

    /// <summary>
    /// Complete a recipe production, generate income, and trigger visual effects.
    /// Uses tier-adjusted value from when production started.
    /// </summary>
    private void CompleteRecipe(HoneyRecipe recipe)
    {
        var state = productionStates[recipe];
        int tier = state.tier; // Use tier from when production started
        state.isProducing = false;
        state.timeRemaining = 0f;

        // Calculate final honey value with tier adjustment and seasonal modifiers
        float baseTierValue = recipe.GetHoneyValue(tier);
        float finalValue = CalculateSeasonalValue(baseTierValue);

        // Generate income
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.EarnMoney(finalValue);

            if (SeasonManager.Instance != null)
            {
                Debug.Log($"Completed recipe: {recipe.recipeName} Tier {tier} - Earned ${finalValue:F2} (base tier value: ${baseTierValue:F2}, season: {SeasonManager.Instance.CurrentSeason})");
            }
            else
            {
                Debug.Log($"Completed recipe: {recipe.recipeName} Tier {tier} - Earned ${finalValue:F2}");
            }
        }

        // Fire completion event for visual effects
        OnRecipeCompleted?.Invoke(recipe, finalValue);
    }

    /// <summary>
    /// Calculate the final honey value with seasonal income modifier applied.
    /// </summary>
    private float CalculateSeasonalValue(float baseValue)
    {
        if (SeasonManager.Instance == null)
            return baseValue;

        SeasonData currentSeason = SeasonManager.Instance.GetCurrentSeasonData();
        if (currentSeason == null)
            return baseValue;

        // Apply seasonal income modifier
        float modifiedValue = baseValue * currentSeason.incomeModifier;

        return modifiedValue;
    }

    /// <summary>
    /// Get the production progress for a specific recipe (0-1 range).
    /// </summary>
    public float GetRecipeProgress(HoneyRecipe recipe)
    {
        if (recipe == null || !productionStates.ContainsKey(recipe))
            return 0f;

        var state = productionStates[recipe];
        if (!state.isProducing || state.totalTime <= 0f)
            return 0f;

        return 1f - (state.timeRemaining / state.totalTime);
    }

    /// <summary>
    /// Check if a recipe is currently in production.
    /// </summary>
    public bool IsProducing(HoneyRecipe recipe)
    {
        if (recipe == null || !productionStates.ContainsKey(recipe))
            return false;

        return productionStates[recipe].isProducing;
    }

    /// <summary>
    /// Get the list of active recipes (ordered by priority).
    /// </summary>
    public List<HoneyRecipe> GetActiveRecipes()
    {
        return new List<HoneyRecipe>(activeRecipes);
    }

    /// <summary>
    /// Add a new recipe to the production list (lowest priority).
    /// </summary>
    public void AddRecipe(HoneyRecipe recipe)
    {
        if (recipe == null)
            return;

        if (!activeRecipes.Contains(recipe))
        {
            activeRecipes.Add(recipe);
            productionStates[recipe] = new ProductionState
            {
                isProducing = false,
                isPaused = false,
                timeRemaining = 0f,
                totalTime = recipe.productionTimeSeconds,
                tier = 0
            };
        }
    }

    /// <summary>
    /// Remove a recipe from the production list.
    /// </summary>
    public void RemoveRecipe(HoneyRecipe recipe)
    {
        if (recipe == null)
            return;

        activeRecipes.Remove(recipe);
        productionStates.Remove(recipe);
    }

    /// <summary>
    /// Move a recipe up in priority (swap with recipe above).
    /// </summary>
    public void IncreasePriority(HoneyRecipe recipe)
    {
        int index = activeRecipes.IndexOf(recipe);
        if (index > 0)
        {
            activeRecipes[index] = activeRecipes[index - 1];
            activeRecipes[index - 1] = recipe;
        }
    }

    /// <summary>
    /// Move a recipe down in priority (swap with recipe below).
    /// </summary>
    public void DecreasePriority(HoneyRecipe recipe)
    {
        int index = activeRecipes.IndexOf(recipe);
        if (index >= 0 && index < activeRecipes.Count - 1)
        {
            activeRecipes[index] = activeRecipes[index + 1];
            activeRecipes[index + 1] = recipe;
        }
    }

    /// <summary>
    /// Check if a recipe is currently paused.
    /// </summary>
    public bool IsPaused(HoneyRecipe recipe)
    {
        if (recipe == null || !productionStates.ContainsKey(recipe))
            return false;

        return productionStates[recipe].isPaused;
    }

    /// <summary>
    /// Pause production for a specific recipe.
    /// </summary>
    public void PauseRecipe(HoneyRecipe recipe)
    {
        if (recipe == null || !productionStates.ContainsKey(recipe))
            return;

        productionStates[recipe].isPaused = true;
    }

    /// <summary>
    /// Resume production for a specific recipe.
    /// </summary>
    public void ResumeRecipe(HoneyRecipe recipe)
    {
        if (recipe == null || !productionStates.ContainsKey(recipe))
            return;

        productionStates[recipe].isPaused = false;
    }

    /// <summary>
    /// Toggle pause state for a specific recipe.
    /// </summary>
    public void TogglePause(HoneyRecipe recipe)
    {
        if (recipe == null || !productionStates.ContainsKey(recipe))
            return;

        var state = productionStates[recipe];
        state.isPaused = !state.isPaused;
    }

    private void OnValidate()
    {
        // Rebuild production states when recipe list changes in editor
        if (Application.isPlaying)
        {
            InitializeRecipes();
        }
    }
}
