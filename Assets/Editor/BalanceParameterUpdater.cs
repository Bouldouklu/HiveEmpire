using UnityEngine;
using UnityEditor;

namespace HiveEmpire.Editor
{
    /// <summary>
    /// Batch updates all existing ScriptableObject assets to use new balanced parameters.
    /// Fixes array size mismatches caused by upgrading from 3-tier to 5-tier systems.
    /// </summary>
    public class BalanceParameterUpdater : EditorWindow
    {
        [MenuItem("Game/Fix All Assets - Update to 5-Tier System")]
        public static void UpdateAllAssets()
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Update All Assets to New Balance Parameters",
                "This will update ALL existing assets to use the new balanced parameters:\n\n" +
                "- FlowerPatchData: 3 → 5 capacity tiers\n" +
                "- HoneyRecipe: 3 → 5 upgrade tiers\n" +
                "- BeeFleetUpgradeData: 4 → 10 bee purchase tiers\n\n" +
                "This will fix console errors caused by array size mismatches.\n\n" +
                "Backup recommended before proceeding.\n\n" +
                "Continue?",
                "Update All",
                "Cancel"
            );

            if (!proceed) return;

            int updatedCount = 0;

            updatedCount += UpdateAllFlowerPatchData();
            updatedCount += UpdateAllHoneyRecipes();
            updatedCount += UpdateAllBeeFleetUpgradeData();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Update Complete",
                $"Successfully updated {updatedCount} assets to new balanced parameters.\n\n" +
                "Console errors should now be resolved.\n\n" +
                "Next steps:\n" +
                "1. Check Unity Console for any remaining errors\n" +
                "2. Run 'Game/Generate New Recipes' to create 7 new recipe assets\n" +
                "3. Test gameplay with new balance parameters",
                "OK"
            );

            Debug.Log($"[BalanceParameterUpdater] Updated {updatedCount} assets total");
        }

        private static int UpdateAllFlowerPatchData()
        {
            Debug.Log("[BalanceParameterUpdater] Updating FlowerPatchData assets...");

            string[] guids = AssetDatabase.FindAssets("t:FlowerPatchData", new[] { "Assets/Resources/FlowerPatchData" });
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                FlowerPatchData data = AssetDatabase.LoadAssetAtPath<FlowerPatchData>(path);

                if (data != null)
                {
                    // Update to 5-tier capacity system with new costs
                    data.capacityUpgradeCosts = new float[] { 50f, 150f, 400f, 900f, 2000f };
                    data.bonusCapacityPerUpgrade = 5;
                    data.maxCapacityTier = 5;

                    // Update placement costs based on balanced parameters
                    // This is optional - you may want to keep custom costs per patch
                    // Uncomment if you want to standardize:
                    /*
                    switch (data.biomeType)
                    {
                        case BiomeType.WildMeadow:
                            data.placementCost = 1f;
                            break;
                        case BiomeType.Orchard:
                            data.placementCost = 20f;
                            break;
                        case BiomeType.CultivatedGarden:
                            data.placementCost = 75f;
                            break;
                        case BiomeType.Marsh:
                            data.placementCost = 200f;
                            break;
                        case BiomeType.ForestEdge:
                            data.placementCost = 500f;
                            break;
                        case BiomeType.AgriculturalField:
                            data.placementCost = 1200f;
                            break;
                    }
                    */

                    EditorUtility.SetDirty(data);
                    count++;
                    Debug.Log($"  ✓ Updated: {data.name} ({data.biomeType})");
                }
            }

            Debug.Log($"[BalanceParameterUpdater] Updated {count} FlowerPatchData assets");
            return count;
        }

        private static int UpdateAllHoneyRecipes()
        {
            Debug.Log("[BalanceParameterUpdater] Updating HoneyRecipe assets...");

            string[] guids = AssetDatabase.FindAssets("t:HoneyRecipe", new[] { "Assets/Resources/Recipes" });
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HoneyRecipe recipe = AssetDatabase.LoadAssetAtPath<HoneyRecipe>(path);

                if (recipe != null)
                {
                    // Update to 5-tier upgrade system
                    recipe.upgradeCosts = new float[] { 100f, 300f, 800f, 2000f, 5000f };
                    recipe.ingredientReductionPercent = new float[] { 0f, 10f, 20f, 30f, 40f, 50f };
                    recipe.productionTimeReductionPercent = new float[] { 0f, 15f, 25f, 35f, 50f, 60f };
                    recipe.valueIncreasePercent = new float[] { 0f, 20f, 40f, 60f, 100f, 150f };

                    EditorUtility.SetDirty(recipe);
                    count++;
                    Debug.Log($"  ✓ Updated: {recipe.recipeName}");
                }
            }

            Debug.Log($"[BalanceParameterUpdater] Updated {count} HoneyRecipe assets");
            return count;
        }

        private static int UpdateAllBeeFleetUpgradeData()
        {
            Debug.Log("[BalanceParameterUpdater] Updating BeeFleetUpgradeData assets...");

            string[] guids = AssetDatabase.FindAssets("t:BeeFleetUpgradeData");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BeeFleetUpgradeData data = AssetDatabase.LoadAssetAtPath<BeeFleetUpgradeData>(path);

                if (data != null)
                {
                    // Update to 10-tier bee purchase system
                    data.beePurchaseCosts = new float[] { 25f, 60f, 150f, 350f, 800f, 1800f, 4000f, 8500f, 18000f, 38000f };
                    data.beesPerPurchase = new int[] { 2, 3, 5, 8, 12, 18, 25, 35, 50, 70 };
                    data.maxPurchaseTier = 10;

                    EditorUtility.SetDirty(data);
                    count++;
                    Debug.Log($"  ✓ Updated: {data.name}");
                }
            }

            Debug.Log($"[BalanceParameterUpdater] Updated {count} BeeFleetUpgradeData assets");
            return count;
        }
    }
}
