using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a honey recipe with ingredients, production time, and output value.
/// Recipes consume pollen from the hive inventory to produce honey that generates income.
/// </summary>
[CreateAssetMenu(fileName = "NewHoneyRecipe", menuName = "Game/Honey Recipe")]
public class HoneyRecipe : ScriptableObject
{
    [System.Serializable]
    public class Ingredient
    {
        public ResourceType pollenType;
        [Min(1)]
        public int quantity = 1;
    }

    [Header("Recipe Identity")]
    [Tooltip("Display name for this recipe")]
    public string recipeName = "New Honey Recipe";

    [Tooltip("Optional description of the honey produced")]
    [TextArea(2, 4)]
    public string description = "";

    [Header("Unlock System")]
    [Tooltip("Is this recipe available at the start of each year?")]
    public bool isUnlockedByDefault = false;

    [Tooltip("Money cost to unlock this recipe")]
    [Min(0f)]
    public float unlockCost = 0f;

    [Tooltip("Recipes that must be unlocked before this one becomes available")]
    public HoneyRecipe[] prerequisiteRecipes = new HoneyRecipe[0];

    [Header("Recipe Requirements")]
    [Tooltip("List of pollen ingredients and quantities required")]
    public List<Ingredient> ingredients = new List<Ingredient>();

    [Header("Production Settings")]
    [Tooltip("Time in seconds to produce this honey")]
    [Min(1f)]
    public float productionTimeSeconds = 5f;

    [Tooltip("Money value earned when this honey is produced")]
    [Min(0.01f)]
    public float honeyValue = 1f;

    [Header("Upgrade System (3 Tiers)")]
    [Tooltip("Money cost to upgrade to each tier (Tier 1, Tier 2, Tier 3)")]
    public float[] upgradeCosts = new float[] { 200f, 500f, 1000f };

    [Tooltip("Ingredient quantity reduction percentage per tier (Tier 0, 1, 2, 3)")]
    [Range(0f, 100f)]
    public float[] ingredientReductionPercent = new float[] { 0f, 10f, 20f, 30f };

    [Tooltip("Production time reduction percentage per tier (Tier 0, 1, 2, 3)")]
    [Range(0f, 100f)]
    public float[] productionTimeReductionPercent = new float[] { 0f, 15f, 25f, 35f };

    [Tooltip("Honey value increase percentage per tier (Tier 0, 1, 2, 3)")]
    [Range(0f, 200f)]
    public float[] valueIncreasePercent = new float[] { 0f, 20f, 40f, 60f };

    [Header("Visual (Optional)")]
    [Tooltip("Icon to display in UI")]
    public Sprite icon;

    [Tooltip("Color tint for this honey type")]
    public Color honeyColor = new Color(1f, 0.75f, 0.2f); // Golden honey color

    /// <summary>
    /// Check if the hive has enough pollen in inventory to start this recipe.
    /// </summary>
    public bool CanProduce(Dictionary<ResourceType, int> inventory)
    {
        foreach (var ingredient in ingredients)
        {
            if (!inventory.ContainsKey(ingredient.pollenType) ||
                inventory[ingredient.pollenType] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get total ingredient cost for display purposes.
    /// </summary>
    public Dictionary<ResourceType, int> GetTotalIngredients()
    {
        Dictionary<ResourceType, int> totals = new Dictionary<ResourceType, int>();
        foreach (var ingredient in ingredients)
        {
            if (totals.ContainsKey(ingredient.pollenType))
            {
                totals[ingredient.pollenType] += ingredient.quantity;
            }
            else
            {
                totals[ingredient.pollenType] = ingredient.quantity;
            }
        }
        return totals;
    }

    /// <summary>
    /// Get production time adjusted for the given upgrade tier.
    /// </summary>
    /// <param name="tier">Upgrade tier (0-3)</param>
    public float GetProductionTime(int tier)
    {
        tier = Mathf.Clamp(tier, 0, 3);
        float reductionPercent = productionTimeReductionPercent[tier];
        return productionTimeSeconds * (1f - reductionPercent / 100f);
    }

    /// <summary>
    /// Get honey value adjusted for the given upgrade tier.
    /// </summary>
    /// <param name="tier">Upgrade tier (0-3)</param>
    public float GetHoneyValue(int tier)
    {
        tier = Mathf.Clamp(tier, 0, 3);
        float increasePercent = valueIncreasePercent[tier];
        return honeyValue * (1f + increasePercent / 100f);
    }

    /// <summary>
    /// Get ingredients adjusted for the given upgrade tier.
    /// </summary>
    /// <param name="tier">Upgrade tier (0-3)</param>
    public List<Ingredient> GetIngredients(int tier)
    {
        tier = Mathf.Clamp(tier, 0, 3);
        float reductionPercent = ingredientReductionPercent[tier];
        float multiplier = 1f - reductionPercent / 100f;

        List<Ingredient> adjustedIngredients = new List<Ingredient>();
        foreach (var ingredient in ingredients)
        {
            Ingredient adjusted = new Ingredient
            {
                pollenType = ingredient.pollenType,
                quantity = Mathf.Max(1, Mathf.RoundToInt(ingredient.quantity * multiplier))
            };
            adjustedIngredients.Add(adjusted);
        }
        return adjustedIngredients;
    }

    /// <summary>
    /// Get the cost to upgrade from current tier to next tier.
    /// </summary>
    /// <param name="currentTier">Current upgrade tier (0-2)</param>
    public float GetUpgradeCost(int currentTier)
    {
        if (currentTier < 0 || currentTier >= upgradeCosts.Length)
        {
            return 0f;
        }
        return upgradeCosts[currentTier];
    }

    /// <summary>
    /// Check if recipe can be upgraded from current tier.
    /// </summary>
    /// <param name="currentTier">Current upgrade tier (0-3)</param>
    public bool CanUpgrade(int currentTier)
    {
        return currentTier >= 0 && currentTier < 3;
    }

    /// <summary>
    /// Get maximum upgrade tier (3 tiers total).
    /// </summary>
    public int GetMaxTier()
    {
        return 3;
    }

    /// <summary>
    /// Check if the hive has enough pollen in inventory to start this recipe at given tier.
    /// </summary>
    public bool CanProduce(Dictionary<ResourceType, int> inventory, int tier)
    {
        List<Ingredient> adjustedIngredients = GetIngredients(tier);
        foreach (var ingredient in adjustedIngredients)
        {
            if (!inventory.ContainsKey(ingredient.pollenType) ||
                inventory[ingredient.pollenType] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Validate recipe configuration in the editor.
    /// </summary>
    private void OnValidate()
    {
        if (ingredients.Count == 0)
        {
            Debug.LogWarning($"Recipe '{recipeName}' has no ingredients defined!", this);
        }

        if (string.IsNullOrEmpty(recipeName))
        {
            recipeName = "Unnamed Recipe";
        }

        // Validate upgrade arrays have correct lengths
        if (upgradeCosts.Length != 3)
        {
            Debug.LogWarning($"Recipe '{recipeName}': upgradeCosts should have 3 elements (Tier 1, 2, 3 costs)", this);
        }

        if (ingredientReductionPercent.Length != 4)
        {
            Debug.LogWarning($"Recipe '{recipeName}': ingredientReductionPercent should have 4 elements (Tier 0, 1, 2, 3)", this);
        }

        if (productionTimeReductionPercent.Length != 4)
        {
            Debug.LogWarning($"Recipe '{recipeName}': productionTimeReductionPercent should have 4 elements (Tier 0, 1, 2, 3)", this);
        }

        if (valueIncreasePercent.Length != 4)
        {
            Debug.LogWarning($"Recipe '{recipeName}': valueIncreasePercent should have 4 elements (Tier 0, 1, 2, 3)", this);
        }

        // Check for circular prerequisites
        if (prerequisiteRecipes != null)
        {
            foreach (var prereq in prerequisiteRecipes)
            {
                if (prereq == this)
                {
                    Debug.LogError($"Recipe '{recipeName}' cannot be its own prerequisite!", this);
                }
            }
        }
    }
}
