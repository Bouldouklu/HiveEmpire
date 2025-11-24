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
        [Tooltip("The flower patch data that defines the pollen type required")]
        public FlowerPatchData pollenType;

        [Min(0.1f)]
        [Tooltip("Quantity of this pollen type required (supports decimals for early game)")]
        public float quantity = 1f;
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

    [Header("Upgrade System (5 Tiers)")]
    [Tooltip("Money cost to upgrade to each tier (Tier 1, Tier 2, Tier 3, Tier 4, Tier 5)")]
    public float[] upgradeCosts = new float[] { 100f, 300f, 800f, 2000f, 5000f };

    [Tooltip("Ingredient quantity reduction percentage per tier (Tier 0, 1, 2, 3, 4, 5)")]
    [Range(0f, 100f)]
    public float[] ingredientReductionPercent = new float[] { 0f, 10f, 20f, 30f, 40f, 50f };

    [Tooltip("Production time reduction percentage per tier (Tier 0, 1, 2, 3, 4, 5)")]
    [Range(0f, 100f)]
    public float[] productionTimeReductionPercent = new float[] { 0f, 15f, 25f, 35f, 50f, 60f };

    [Tooltip("Honey value increase percentage per tier (Tier 0, 1, 2, 3, 4, 5)")]
    [Min(0f)]
    public float[] valueIncreasePercent = new float[] { 0f, 20f, 40f, 60f, 100f, 150f };

    [Header("Visual (Optional)")]
    [Tooltip("Icon to display in UI")]
    public Sprite icon;

    [Tooltip("Color tint for this honey type")]
    public Color honeyColor = new Color(1f, 0.75f, 0.2f); // Golden honey color

    /// <summary>
    /// Check if the hive has enough pollen in inventory to start this recipe.
    /// Works with Dictionary<FlowerPatchData, float> inventory.
    /// </summary>
    public bool CanProduce(Dictionary<FlowerPatchData, float> inventory)
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient.pollenType == null)
            {
                Debug.LogWarning($"Recipe '{recipeName}' has ingredient with null pollenType");
                return false;
            }

            if (!inventory.ContainsKey(ingredient.pollenType) ||
                inventory[ingredient.pollenType] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the hive has enough pollen in inventory to start this recipe.
    /// Works with List<PollenInventorySlot> inventory.
    /// </summary>
    public bool CanProduce(List<PollenInventorySlot> inventory)
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient.pollenType == null)
            {
                Debug.LogWarning($"Recipe '{recipeName}' has ingredient with null pollenType");
                return false;
            }

            PollenInventorySlot slot = inventory.Find(s => s.pollenType == ingredient.pollenType);
            if (slot == null || slot.quantity < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get total ingredient cost for display purposes.
    /// Returns Dictionary<FlowerPatchData, float>.
    /// </summary>
    public Dictionary<FlowerPatchData, float> GetTotalIngredients()
    {
        Dictionary<FlowerPatchData, float> totals = new Dictionary<FlowerPatchData, float>();
        foreach (var ingredient in ingredients)
        {
            if (ingredient.pollenType == null)
                continue;

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
    /// <param name="tier">Upgrade tier (0-5)</param>
    public float GetProductionTime(int tier)
    {
        tier = Mathf.Clamp(tier, 0, 5);
        float reductionPercent = productionTimeReductionPercent[tier];
        return productionTimeSeconds * (1f - reductionPercent / 100f);
    }

    /// <summary>
    /// Get honey value adjusted for the given upgrade tier.
    /// </summary>
    /// <param name="tier">Upgrade tier (0-5)</param>
    public float GetHoneyValue(int tier)
    {
        tier = Mathf.Clamp(tier, 0, 5);
        float increasePercent = valueIncreasePercent[tier];
        return honeyValue * (1f + increasePercent / 100f);
    }

    /// <summary>
    /// Get ingredients adjusted for the given upgrade tier.
    /// </summary>
    /// <param name="tier">Upgrade tier (0-5)</param>
    public List<Ingredient> GetIngredients(int tier)
    {
        tier = Mathf.Clamp(tier, 0, 5);
        float reductionPercent = ingredientReductionPercent[tier];
        float multiplier = 1f - reductionPercent / 100f;

        List<Ingredient> adjustedIngredients = new List<Ingredient>();
        foreach (var ingredient in ingredients)
        {
            Ingredient adjusted = new Ingredient
            {
                pollenType = ingredient.pollenType,
                quantity = Mathf.Max(0.1f, ingredient.quantity * multiplier) // Min 0.1 to support decimal values
            };
            adjustedIngredients.Add(adjusted);
        }
        return adjustedIngredients;
    }

    /// <summary>
    /// Get the cost to upgrade from current tier to next tier.
    /// </summary>
    /// <param name="currentTier">Current upgrade tier (0-4)</param>
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
    /// <param name="currentTier">Current upgrade tier (0-5)</param>
    public bool CanUpgrade(int currentTier)
    {
        return currentTier >= 0 && currentTier < 5;
    }

    /// <summary>
    /// Get maximum upgrade tier (5 tiers total).
    /// </summary>
    public int GetMaxTier()
    {
        return 5;
    }

    /// <summary>
    /// Check if the hive has enough pollen in inventory to start this recipe at given tier.
    /// Works with Dictionary<FlowerPatchData, float> inventory.
    /// </summary>
    public bool CanProduce(Dictionary<FlowerPatchData, float> inventory, int tier)
    {
        List<Ingredient> adjustedIngredients = GetIngredients(tier);
        foreach (var ingredient in adjustedIngredients)
        {
            if (ingredient.pollenType == null)
                return false;

            if (!inventory.ContainsKey(ingredient.pollenType) ||
                inventory[ingredient.pollenType] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check if the hive has enough pollen in inventory to start this recipe at given tier.
    /// Works with List<PollenInventorySlot> inventory.
    /// </summary>
    public bool CanProduce(List<PollenInventorySlot> inventory, int tier)
    {
        List<Ingredient> adjustedIngredients = GetIngredients(tier);
        foreach (var ingredient in adjustedIngredients)
        {
            if (ingredient.pollenType == null)
                return false;

            PollenInventorySlot slot = inventory.Find(s => s.pollenType == ingredient.pollenType);
            if (slot == null || slot.quantity < ingredient.quantity)
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
        if (upgradeCosts.Length != 5)
        {
            Debug.LogWarning($"Recipe '{recipeName}': upgradeCosts should have 5 elements (Tier 1, 2, 3, 4, 5 costs)", this);
        }

        if (ingredientReductionPercent.Length != 6)
        {
            Debug.LogWarning($"Recipe '{recipeName}': ingredientReductionPercent should have 6 elements (Tier 0, 1, 2, 3, 4, 5)", this);
        }

        if (productionTimeReductionPercent.Length != 6)
        {
            Debug.LogWarning($"Recipe '{recipeName}': productionTimeReductionPercent should have 6 elements (Tier 0, 1, 2, 3, 4, 5)", this);
        }

        if (valueIncreasePercent.Length != 6)
        {
            Debug.LogWarning($"Recipe '{recipeName}': valueIncreasePercent should have 6 elements (Tier 0, 1, 2, 3, 4, 5)", this);
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
