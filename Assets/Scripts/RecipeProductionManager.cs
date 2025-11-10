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
    [Tooltip("List of active recipes, ordered by priority (top = highest priority)")]
    [SerializeField]
    private List<HoneyRecipe> activeRecipes = new List<HoneyRecipe>();

    [Header("Events")]
    [Tooltip("Fired when a recipe completes production (passes recipe and honey value)")]
    public UnityEvent<HoneyRecipe, float> OnRecipeCompleted;

    // Track production state for each recipe
    private class ProductionState
    {
        public bool isProducing;
        public float timeRemaining;
        public float totalTime;
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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
    /// </summary>
    private void InitializeRecipes()
    {
        productionStates.Clear();
        foreach (var recipe in activeRecipes)
        {
            if (recipe != null)
            {
                productionStates[recipe] = new ProductionState
                {
                    isProducing = false,
                    timeRemaining = 0f,
                    totalTime = recipe.productionTimeSeconds
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

            if (state.isProducing)
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
    /// </summary>
    private void TryStartRecipes()
    {
        if (HiveController.Instance == null)
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

            var state = productionStates[recipe];

            // Skip if already producing
            if (state.isProducing)
                continue;

            // Check if we can produce with currently available resources
            if (CanProduceWithResources(recipe, availableResources))
            {
                // Consume resources from working copy (for allocation)
                ConsumeResourcesFromPool(recipe, availableResources);

                // Actually consume from hive inventory
                if (HiveController.Instance.TryConsumeResources(recipe))
                {
                    // Start production
                    StartProduction(recipe);
                }
                else
                {
                    Debug.LogWarning($"Failed to consume resources for recipe '{recipe.recipeName}' despite allocation check passing.");
                }
            }
        }
    }

    /// <summary>
    /// Check if recipe can be produced with given resource pool.
    /// </summary>
    private bool CanProduceWithResources(HoneyRecipe recipe, Dictionary<ResourceType, int> resources)
    {
        foreach (var ingredient in recipe.ingredients)
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
    /// Consume resources from a resource pool (for allocation simulation).
    /// </summary>
    private void ConsumeResourcesFromPool(HoneyRecipe recipe, Dictionary<ResourceType, int> resources)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            resources[ingredient.pollenType] -= ingredient.quantity;
        }
    }

    /// <summary>
    /// Start production for a recipe.
    /// </summary>
    private void StartProduction(HoneyRecipe recipe)
    {
        var state = productionStates[recipe];
        state.isProducing = true;
        state.timeRemaining = recipe.productionTimeSeconds;
        state.totalTime = recipe.productionTimeSeconds;

        Debug.Log($"Started production: {recipe.recipeName} (${recipe.honeyValue}, {recipe.productionTimeSeconds}s)");
    }

    /// <summary>
    /// Complete a recipe production, generate income, and trigger visual effects.
    /// </summary>
    private void CompleteRecipe(HoneyRecipe recipe)
    {
        var state = productionStates[recipe];
        state.isProducing = false;
        state.timeRemaining = 0f;

        // Generate income
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.EarnMoney(recipe.honeyValue);
            Debug.Log($"Completed recipe: {recipe.recipeName} - Earned ${recipe.honeyValue}");
        }

        // Fire completion event for visual effects
        OnRecipeCompleted?.Invoke(recipe, recipe.honeyValue);
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
                timeRemaining = 0f,
                totalTime = recipe.productionTimeSeconds
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

    private void OnValidate()
    {
        // Rebuild production states when recipe list changes in editor
        if (Application.isPlaying)
        {
            InitializeRecipes();
        }
    }
}
