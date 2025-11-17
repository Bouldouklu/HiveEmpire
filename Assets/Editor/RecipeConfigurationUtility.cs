using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor utility to batch-configure recipe unlock trees and upgrade curves.
/// Use: Tools → Game → Configure Recipe Progression
/// </summary>
public static class RecipeConfigurationUtility
{
    [MenuItem("Tools/Game/Configure Recipe Progression")]
    public static void ConfigureAllRecipes()
    {
        Debug.Log("[RecipeConfigurationUtility] Starting recipe configuration...");

        // Load all recipes from Resources folder
        HoneyRecipe[] allRecipes = Resources.LoadAll<HoneyRecipe>("Recipes");

        if (allRecipes.Length == 0)
        {
            Debug.LogError("[RecipeConfigurationUtility] No recipes found in Resources/Recipes folder!");
            return;
        }

        Debug.Log($"[RecipeConfigurationUtility] Found {allRecipes.Length} recipes");

        // Create a dictionary for easy lookup
        Dictionary<string, HoneyRecipe> recipeDict = new Dictionary<string, HoneyRecipe>();
        foreach (var recipe in allRecipes)
        {
            recipeDict[recipe.recipeName] = recipe;
        }

        // Configure ForestHoney - Starter recipe (unlocked by default)
        if (recipeDict.ContainsKey("ForestHoney"))
        {
            var recipe = recipeDict["ForestHoney"];
            Undo.RecordObject(recipe, "Configure ForestHoney");

            recipe.isUnlockedByDefault = true;
            recipe.unlockCost = 0f;
            recipe.prerequisiteRecipes = new HoneyRecipe[0];

            // Upgrade costs and curves
            recipe.upgradeCosts = new float[] { 200f, 500f, 1000f };
            recipe.ingredientReductionPercent = new float[] { 0f, 10f, 20f, 30f };
            recipe.productionTimeReductionPercent = new float[] { 0f, 15f, 25f, 35f };
            recipe.valueIncreasePercent = new float[] { 0f, 20f, 40f, 60f };

            EditorUtility.SetDirty(recipe);
            Debug.Log("[RecipeConfigurationUtility] Configured ForestHoney (starter recipe)");
        }

        // Configure WildflowerHoney - Requires ForestHoney
        if (recipeDict.ContainsKey("WildflowerHoney") && recipeDict.ContainsKey("ForestHoney"))
        {
            var recipe = recipeDict["WildflowerHoney"];
            Undo.RecordObject(recipe, "Configure WildflowerHoney");

            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 300f;
            recipe.prerequisiteRecipes = new HoneyRecipe[] { recipeDict["ForestHoney"] };

            // Upgrade costs and curves
            recipe.upgradeCosts = new float[] { 200f, 500f, 1000f };
            recipe.ingredientReductionPercent = new float[] { 0f, 10f, 20f, 30f };
            recipe.productionTimeReductionPercent = new float[] { 0f, 15f, 25f, 35f };
            recipe.valueIncreasePercent = new float[] { 0f, 20f, 40f, 60f };

            EditorUtility.SetDirty(recipe);
            Debug.Log("[RecipeConfigurationUtility] Configured WildflowerHoney (requires ForestHoney)");
        }

        // Configure MountainHoney - Requires WildflowerHoney
        if (recipeDict.ContainsKey("MountainHoney") && recipeDict.ContainsKey("WildflowerHoney"))
        {
            var recipe = recipeDict["MountainHoney"];
            Undo.RecordObject(recipe, "Configure MountainHoney");

            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 500f;
            recipe.prerequisiteRecipes = new HoneyRecipe[] { recipeDict["WildflowerHoney"] };

            // Upgrade costs and curves (more expensive for advanced recipe)
            recipe.upgradeCosts = new float[] { 300f, 700f, 1500f };
            recipe.ingredientReductionPercent = new float[] { 0f, 10f, 20f, 30f };
            recipe.productionTimeReductionPercent = new float[] { 0f, 15f, 25f, 35f };
            recipe.valueIncreasePercent = new float[] { 0f, 25f, 50f, 75f }; // Better value scaling

            EditorUtility.SetDirty(recipe);
            Debug.Log("[RecipeConfigurationUtility] Configured MountainHoney (requires WildflowerHoney)");
        }

        // Configure DesertBlossom - Requires MountainHoney
        if (recipeDict.ContainsKey("DesertBlossom") && recipeDict.ContainsKey("MountainHoney"))
        {
            var recipe = recipeDict["DesertBlossom"];
            Undo.RecordObject(recipe, "Configure DesertBlossom");

            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 800f;
            recipe.prerequisiteRecipes = new HoneyRecipe[] { recipeDict["MountainHoney"] };

            // Upgrade costs and curves (premium recipe)
            recipe.upgradeCosts = new float[] { 400f, 1000f, 2000f };
            recipe.ingredientReductionPercent = new float[] { 0f, 12f, 24f, 36f }; // Better efficiency
            recipe.productionTimeReductionPercent = new float[] { 0f, 18f, 30f, 42f }; // Faster production
            recipe.valueIncreasePercent = new float[] { 0f, 30f, 60f, 90f }; // Much better value

            EditorUtility.SetDirty(recipe);
            Debug.Log("[RecipeConfigurationUtility] Configured DesertBlossom (requires MountainHoney)");
        }

        // Configure PremiumBlend - Requires BOTH MountainHoney AND DesertBlossom
        if (recipeDict.ContainsKey("PremiumBlend") &&
            recipeDict.ContainsKey("MountainHoney") &&
            recipeDict.ContainsKey("DesertBlossom"))
        {
            var recipe = recipeDict["PremiumBlend"];
            Undo.RecordObject(recipe, "Configure PremiumBlend");

            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 1500f;
            recipe.prerequisiteRecipes = new HoneyRecipe[] {
                recipeDict["MountainHoney"],
                recipeDict["DesertBlossom"]
            };

            // Upgrade costs and curves (ultimate recipe)
            recipe.upgradeCosts = new float[] { 500f, 1500f, 3000f };
            recipe.ingredientReductionPercent = new float[] { 0f, 15f, 30f, 45f }; // Best efficiency
            recipe.productionTimeReductionPercent = new float[] { 0f, 20f, 35f, 50f }; // Fastest production
            recipe.valueIncreasePercent = new float[] { 0f, 40f, 80f, 120f }; // Explosive value scaling

            EditorUtility.SetDirty(recipe);
            Debug.Log("[RecipeConfigurationUtility] Configured PremiumBlend (requires MountainHoney + DesertBlossom)");
        }

        // Save all assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[RecipeConfigurationUtility] Recipe configuration complete!");
        EditorUtility.DisplayDialog(
            "Recipe Configuration Complete",
            $"Successfully configured {allRecipes.Length} recipes with unlock trees and upgrade curves.\n\nUnlock Order:\n1. ForestHoney (starter)\n2. WildflowerHoney → requires ForestHoney\n3. MountainHoney → requires WildflowerHoney\n4. DesertBlossom → requires MountainHoney\n5. PremiumBlend → requires MountainHoney + DesertBlossom",
            "OK"
        );
    }
}
