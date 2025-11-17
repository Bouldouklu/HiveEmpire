using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace HiveEmpire.Editor
{
    /// <summary>
    /// Editor utility to generate new balanced HoneyRecipe assets based on BalancedParameters.md specifications.
    /// Use this to quickly create the 7 new recipes needed to expand from 5 to 12 total recipes.
    /// </summary>
    public class RecipeGenerator : EditorWindow
    {
        [MenuItem("Game/Generate New Recipes")]
        public static void GenerateNewRecipes()
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Generate New Recipes",
                "This will create 7 new HoneyRecipe assets in Assets/Resources/Recipes/:\n\n" +
                "1. OrchardBlossom (single-resource)\n" +
                "2. FieldGold (single-resource)\n" +
                "3. SpringBlend (dual-blend)\n" +
                "4. SummerBlend (dual-blend)\n" +
                "5. AutumnBlend (dual-blend)\n" +
                "6. SeasonalHarmony (multi-blend)\n" +
                "7. PremiumReserve (multi-blend)\n\n" +
                "Existing recipes will not be modified.\n\n" +
                "Continue?",
                "Generate",
                "Cancel"
            );

            if (!proceed) return;

            int createdCount = 0;

            // Single-Resource Recipes
            createdCount += CreateOrchardBlossom();
            createdCount += CreateFieldGold();

            // Dual-Blend Recipes
            createdCount += CreateSpringBlend();
            createdCount += CreateSummerBlend();
            createdCount += CreateAutumnBlend();

            // Multi-Blend Recipes
            createdCount += CreateSeasonalHarmony();
            createdCount += CreatePremiumReserve();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Recipe Generation Complete",
                $"Successfully created {createdCount} new recipe assets in Assets/Resources/Recipes/\n\n" +
                "Next steps:\n" +
                "1. Update existing recipes to use new 5-tier costs\n" +
                "2. Add new recipes to RecipeProductionManager in GameScene\n" +
                "3. Test balance with EconomySimulator",
                "OK"
            );

            Debug.Log($"RecipeGenerator: Created {createdCount} new recipe assets");
        }

        private static int CreateOrchardBlossom()
        {
            string path = "Assets/Resources/Recipes/OrchardBlossom.asset";
            if (AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path) != null)
            {
                Debug.LogWarning($"Recipe already exists at {path}, skipping");
                return 0;
            }

            HoneyRecipe recipe = ScriptableObject.CreateInstance<HoneyRecipe>();
            recipe.recipeName = "Orchard Blossom";
            recipe.description = "Sweet honey made from orchard flowers. A staple of early production.";
            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 25f;

            recipe.ingredients = new List<HoneyRecipe.Ingredient>
            {
                new HoneyRecipe.Ingredient { pollenType = ResourceType.OrchardPollen, quantity = 2 }
            };

            recipe.productionTimeSeconds = 8f;
            recipe.honeyValue = 5f;
            recipe.honeyColor = new Color(1f, 0.85f, 0.3f); // Light golden

            AssetDatabase.CreateAsset(recipe, path);
            Debug.Log($"Created recipe: {recipe.recipeName} at {path}");
            return 1;
        }

        private static int CreateFieldGold()
        {
            string path = "Assets/Resources/Recipes/FieldGold.asset";
            if (AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path) != null)
            {
                Debug.LogWarning($"Recipe already exists at {path}, skipping");
                return 0;
            }

            HoneyRecipe recipe = ScriptableObject.CreateInstance<HoneyRecipe>();
            recipe.recipeName = "Field Gold";
            recipe.description = "Premium honey from agricultural fields. Rich and expensive to produce.";
            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 400f;

            recipe.ingredients = new List<HoneyRecipe.Ingredient>
            {
                new HoneyRecipe.Ingredient { pollenType = ResourceType.AgriculturalFieldPollen, quantity = 6 }
            };

            recipe.productionTimeSeconds = 35f;
            recipe.honeyValue = 50f;
            recipe.honeyColor = new Color(1f, 0.7f, 0.1f); // Deep golden

            AssetDatabase.CreateAsset(recipe, path);
            Debug.Log($"Created recipe: {recipe.recipeName} at {path}");
            return 1;
        }

        private static int CreateSpringBlend()
        {
            string path = "Assets/Resources/Recipes/SpringBlend.asset";
            if (AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path) != null)
            {
                Debug.LogWarning($"Recipe already exists at {path}, skipping");
                return 0;
            }

            HoneyRecipe recipe = ScriptableObject.CreateInstance<HoneyRecipe>();
            recipe.recipeName = "Spring Blend";
            recipe.description = "Delicate blend of wild meadow and orchard pollens. Captures the essence of spring.";
            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 150f;

            recipe.ingredients = new List<HoneyRecipe.Ingredient>
            {
                new HoneyRecipe.Ingredient { pollenType = ResourceType.WildMeadowPollen, quantity = 2 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.OrchardPollen, quantity = 2 }
            };

            recipe.productionTimeSeconds = 20f;
            recipe.honeyValue = 25f;
            recipe.honeyColor = new Color(0.9f, 1f, 0.7f); // Spring green-gold

            AssetDatabase.CreateAsset(recipe, path);
            Debug.Log($"Created recipe: {recipe.recipeName} at {path}");
            return 1;
        }

        private static int CreateSummerBlend()
        {
            string path = "Assets/Resources/Recipes/SummerBlend.asset";
            if (AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path) != null)
            {
                Debug.LogWarning($"Recipe already exists at {path}, skipping");
                return 0;
            }

            HoneyRecipe recipe = ScriptableObject.CreateInstance<HoneyRecipe>();
            recipe.recipeName = "Summer Blend";
            recipe.description = "Vibrant combination of garden and marsh pollens. Peak summer flavor.";
            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 300f;

            recipe.ingredients = new List<HoneyRecipe.Ingredient>
            {
                new HoneyRecipe.Ingredient { pollenType = ResourceType.CultivatedGardenPollen, quantity = 3 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.MarshPollen, quantity = 3 }
            };

            recipe.productionTimeSeconds = 30f;
            recipe.honeyValue = 45f;
            recipe.honeyColor = new Color(1f, 0.95f, 0.4f); // Bright summer gold

            AssetDatabase.CreateAsset(recipe, path);
            Debug.Log($"Created recipe: {recipe.recipeName} at {path}");
            return 1;
        }

        private static int CreateAutumnBlend()
        {
            string path = "Assets/Resources/Recipes/AutumnBlend.asset";
            if (AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path) != null)
            {
                Debug.LogWarning($"Recipe already exists at {path}, skipping");
                return 0;
            }

            HoneyRecipe recipe = ScriptableObject.CreateInstance<HoneyRecipe>();
            recipe.recipeName = "Autumn Blend";
            recipe.description = "Rich harvest blend from forest edge and agricultural fields. Deep autumn warmth.";
            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 600f;

            recipe.ingredients = new List<HoneyRecipe.Ingredient>
            {
                new HoneyRecipe.Ingredient { pollenType = ResourceType.ForestEdgePollen, quantity = 4 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.AgriculturalFieldPollen, quantity = 4 }
            };

            recipe.productionTimeSeconds = 45f;
            recipe.honeyValue = 80f;
            recipe.honeyColor = new Color(0.9f, 0.5f, 0.2f); // Autumn amber

            AssetDatabase.CreateAsset(recipe, path);
            Debug.Log($"Created recipe: {recipe.recipeName} at {path}");
            return 1;
        }

        private static int CreateSeasonalHarmony()
        {
            string path = "Assets/Resources/Recipes/SeasonalHarmony.asset";
            if (AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path) != null)
            {
                Debug.LogWarning($"Recipe already exists at {path}, skipping");
                return 0;
            }

            HoneyRecipe recipe = ScriptableObject.CreateInstance<HoneyRecipe>();
            recipe.recipeName = "Seasonal Harmony";
            recipe.description = "Balanced blend of four biome pollens. Harmonious complexity.";
            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 800f;

            recipe.ingredients = new List<HoneyRecipe.Ingredient>
            {
                new HoneyRecipe.Ingredient { pollenType = ResourceType.WildMeadowPollen, quantity = 2 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.OrchardPollen, quantity = 2 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.CultivatedGardenPollen, quantity = 2 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.MarshPollen, quantity = 2 }
            };

            recipe.productionTimeSeconds = 50f;
            recipe.honeyValue = 120f;
            recipe.honeyColor = new Color(1f, 0.6f, 0.3f); // Complex blend color

            AssetDatabase.CreateAsset(recipe, path);
            Debug.Log($"Created recipe: {recipe.recipeName} at {path}");
            return 1;
        }

        private static int CreatePremiumReserve()
        {
            string path = "Assets/Resources/Recipes/PremiumReserve.asset";
            if (AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path) != null)
            {
                Debug.LogWarning($"Recipe already exists at {path}, skipping");
                return 0;
            }

            HoneyRecipe recipe = ScriptableObject.CreateInstance<HoneyRecipe>();
            recipe.recipeName = "Premium Reserve";
            recipe.description = "Exquisite five-pollen blend. Reserved for connoisseurs.";
            recipe.isUnlockedByDefault = false;
            recipe.unlockCost = 1500f;

            recipe.ingredients = new List<HoneyRecipe.Ingredient>
            {
                new HoneyRecipe.Ingredient { pollenType = ResourceType.WildMeadowPollen, quantity = 3 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.OrchardPollen, quantity = 3 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.CultivatedGardenPollen, quantity = 3 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.MarshPollen, quantity = 3 },
                new HoneyRecipe.Ingredient { pollenType = ResourceType.ForestEdgePollen, quantity = 3 }
            };

            recipe.productionTimeSeconds = 70f;
            recipe.honeyValue = 200f;
            recipe.honeyColor = new Color(0.95f, 0.45f, 0.15f); // Premium amber

            AssetDatabase.CreateAsset(recipe, path);
            Debug.Log($"Created recipe: {recipe.recipeName} at {path}");
            return 1;
        }

        [MenuItem("Game/Update Existing Recipes to 5 Tiers")]
        public static void UpdateExistingRecipes()
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Update Existing Recipes",
                "This will update ALL existing HoneyRecipe assets in Assets/Resources/Recipes/ to use the new 5-tier upgrade system.\n\n" +
                "Current recipes will have their upgrade costs and modifiers updated to match the balanced parameters.\n\n" +
                "This action can be undone with Ctrl+Z.\n\n" +
                "Continue?",
                "Update",
                "Cancel"
            );

            if (!proceed) return;

            string[] guids = AssetDatabase.FindAssets("t:HoneyRecipe", new[] { "Assets/Resources/Recipes" });
            int updatedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HoneyRecipe recipe = AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path);

                if (recipe != null)
                {
                    // Update to 5-tier system
                    recipe.upgradeCosts = new float[] { 100f, 300f, 800f, 2000f, 5000f };
                    recipe.ingredientReductionPercent = new float[] { 0f, 10f, 20f, 30f, 40f, 50f };
                    recipe.productionTimeReductionPercent = new float[] { 0f, 15f, 25f, 35f, 50f, 60f };
                    recipe.valueIncreasePercent = new float[] { 0f, 20f, 40f, 60f, 100f, 150f };

                    EditorUtility.SetDirty(recipe);
                    updatedCount++;
                    Debug.Log($"Updated recipe to 5-tier system: {recipe.recipeName} at {path}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Update Complete",
                $"Successfully updated {updatedCount} recipe assets to 5-tier upgrade system.",
                "OK"
            );

            Debug.Log($"RecipeGenerator: Updated {updatedCount} existing recipes to 5-tier system");
        }
    }
}
