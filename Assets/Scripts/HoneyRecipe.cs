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
    }
}
