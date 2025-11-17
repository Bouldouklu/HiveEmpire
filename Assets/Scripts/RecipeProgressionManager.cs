using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages recipe unlock and upgrade progression.
/// Tracks which recipes are unlocked and their upgrade tiers during each year.
/// Resets with each new year campaign.
/// </summary>
public class RecipeProgressionManager : MonoBehaviour
{
    public static RecipeProgressionManager Instance { get; private set; }

    /// <summary>
    /// Runtime state for a single recipe.
    /// </summary>
    private class RecipeState
    {
        public bool isUnlocked;
        public int currentTier; // 0-3 (0 = base, 3 = max)
    }

    [Header("Recipe Configuration")]
    [Tooltip("All recipes in the game - assign manually in inspector")]
    [SerializeField]
    private List<HoneyRecipe> allRecipes = new List<HoneyRecipe>();

    [Header("Events")]
    [Tooltip("Fired when a recipe is unlocked. Passes the recipe.")]
    public UnityEvent<HoneyRecipe> OnRecipeUnlocked;

    [Tooltip("Fired when a recipe is upgraded. Passes the recipe and new tier.")]
    public UnityEvent<HoneyRecipe, int> OnRecipeUpgraded;

    // Runtime state storage (keyed by recipe name)
    private Dictionary<string, RecipeState> recipeStates = new Dictionary<string, RecipeState>();

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

        InitializeRecipeStates();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Initialize recipe states with defaults (locked, tier 0).
    /// Auto-unlock recipes marked as isUnlockedByDefault.
    /// </summary>
    private void InitializeRecipeStates()
    {
        recipeStates.Clear();

        if (allRecipes == null || allRecipes.Count == 0)
        {
            Debug.LogWarning("RecipeProgressionManager: No recipes assigned in inspector! Assign recipes to 'All Recipes' field.");
            return;
        }

        Debug.Log($"RecipeProgressionManager: Initializing {allRecipes.Count} recipes from inspector assignment");

        foreach (var recipe in allRecipes)
        {
            if (recipe == null)
            {
                Debug.LogWarning("RecipeProgressionManager: Null recipe found in inspector list, skipping");
                continue;
            }

            RecipeState state = new RecipeState
            {
                isUnlocked = recipe.isUnlockedByDefault,
                currentTier = 0
            };
            recipeStates[recipe.recipeName] = state;

            if (recipe.isUnlockedByDefault)
            {
                Debug.Log($"RecipeProgressionManager: Auto-unlocked recipe '{recipe.recipeName}'");
            }
        }
    }

    /// <summary>
    /// Reset all recipe states to initial configuration.
    /// Called by GameManager.ResetYear().
    /// </summary>
    public void ResetToInitialState()
    {
        Debug.Log("RecipeProgressionManager: Resetting to initial state");
        InitializeRecipeStates();
    }

    #region Query Methods

    /// <summary>
    /// Check if a recipe is unlocked.
    /// </summary>
    public bool IsRecipeUnlocked(HoneyRecipe recipe)
    {
        if (recipe == null || !recipeStates.ContainsKey(recipe.recipeName))
        {
            return false;
        }
        return recipeStates[recipe.recipeName].isUnlocked;
    }

    /// <summary>
    /// Get the current upgrade tier for a recipe (0-3).
    /// </summary>
    public int GetRecipeTier(HoneyRecipe recipe)
    {
        if (recipe == null || !recipeStates.ContainsKey(recipe.recipeName))
        {
            return 0;
        }
        return recipeStates[recipe.recipeName].currentTier;
    }

    /// <summary>
    /// Get all unlocked recipes.
    /// </summary>
    public List<HoneyRecipe> GetAllUnlockedRecipes()
    {
        List<HoneyRecipe> unlocked = new List<HoneyRecipe>();
        foreach (var recipe in allRecipes)
        {
            if (IsRecipeUnlocked(recipe))
            {
                unlocked.Add(recipe);
            }
        }
        return unlocked;
    }

    /// <summary>
    /// Get all recipes (locked and unlocked).
    /// </summary>
    public List<HoneyRecipe> GetAllRecipes()
    {
        return new List<HoneyRecipe>(allRecipes);
    }

    #endregion

    #region Unlock System

    /// <summary>
    /// Check if a recipe can be unlocked (prerequisites met + affordability).
    /// </summary>
    public bool CanUnlockRecipe(HoneyRecipe recipe)
    {
        if (recipe == null)
        {
            return false;
        }

        // Already unlocked
        if (IsRecipeUnlocked(recipe))
        {
            return false;
        }

        // Check prerequisites
        if (!ArePrerequisitesMet(recipe))
        {
            return false;
        }

        // Check affordability
        if (EconomyManager.Instance != null && !EconomyManager.Instance.CanAfford(recipe.unlockCost))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if all prerequisite recipes are unlocked.
    /// </summary>
    private bool ArePrerequisitesMet(HoneyRecipe recipe)
    {
        if (recipe.prerequisiteRecipes == null || recipe.prerequisiteRecipes.Length == 0)
        {
            return true;
        }

        foreach (var prereq in recipe.prerequisiteRecipes)
        {
            if (prereq == null)
            {
                continue; // Skip null prerequisites
            }

            if (!IsRecipeUnlocked(prereq))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get list of missing prerequisite recipe names.
    /// </summary>
    public List<string> GetMissingPrerequisites(HoneyRecipe recipe)
    {
        List<string> missing = new List<string>();

        if (recipe.prerequisiteRecipes == null || recipe.prerequisiteRecipes.Length == 0)
        {
            return missing;
        }

        foreach (var prereq in recipe.prerequisiteRecipes)
        {
            if (prereq == null)
            {
                continue;
            }

            if (!IsRecipeUnlocked(prereq))
            {
                missing.Add(prereq.recipeName);
            }
        }

        return missing;
    }

    /// <summary>
    /// Attempt to unlock a recipe. Returns true if successful.
    /// </summary>
    public bool TryUnlockRecipe(HoneyRecipe recipe)
    {
        if (!CanUnlockRecipe(recipe))
        {
            Debug.LogWarning($"RecipeProgressionManager: Cannot unlock recipe '{recipe.recipeName}'");
            return false;
        }

        // Spend money
        if (EconomyManager.Instance != null && !EconomyManager.Instance.SpendMoney(recipe.unlockCost))
        {
            return false;
        }

        // Unlock recipe
        recipeStates[recipe.recipeName].isUnlocked = true;
        Debug.Log($"RecipeProgressionManager: Unlocked recipe '{recipe.recipeName}' for ${recipe.unlockCost}");

        // Fire event
        OnRecipeUnlocked?.Invoke(recipe);

        return true;
    }

    #endregion

    #region Upgrade System

    /// <summary>
    /// Check if a recipe can be upgraded (unlocked + not max tier + affordability).
    /// </summary>
    public bool CanUpgradeRecipe(HoneyRecipe recipe)
    {
        if (recipe == null)
        {
            return false;
        }

        // Must be unlocked
        if (!IsRecipeUnlocked(recipe))
        {
            return false;
        }

        int currentTier = GetRecipeTier(recipe);

        // Check if more tiers available
        if (!recipe.CanUpgrade(currentTier))
        {
            return false;
        }

        // Check affordability
        float upgradeCost = recipe.GetUpgradeCost(currentTier);
        if (EconomyManager.Instance != null && !EconomyManager.Instance.CanAfford(upgradeCost))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Attempt to upgrade a recipe to the next tier. Returns true if successful.
    /// </summary>
    public bool TryUpgradeRecipe(HoneyRecipe recipe)
    {
        if (!CanUpgradeRecipe(recipe))
        {
            Debug.LogWarning($"RecipeProgressionManager: Cannot upgrade recipe '{recipe.recipeName}'");
            return false;
        }

        int currentTier = GetRecipeTier(recipe);
        float upgradeCost = recipe.GetUpgradeCost(currentTier);

        // Spend money
        if (EconomyManager.Instance != null && !EconomyManager.Instance.SpendMoney(upgradeCost))
        {
            return false;
        }

        // Upgrade tier
        int newTier = currentTier + 1;
        recipeStates[recipe.recipeName].currentTier = newTier;
        Debug.Log($"RecipeProgressionManager: Upgraded recipe '{recipe.recipeName}' to Tier {newTier} for ${upgradeCost}");

        // Fire event
        OnRecipeUpgraded?.Invoke(recipe, newTier);

        return true;
    }

    #endregion
}
