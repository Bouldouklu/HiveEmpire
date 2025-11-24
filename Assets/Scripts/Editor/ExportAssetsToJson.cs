using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ExportAssetsToJson : EditorWindow
{
    [MenuItem("Tools/Export Assets to JSON")]
    public static void ExportToJson()
    {
        var exportData = new ExportData();

        // Export FlowerPatchData
        var flowerPatchDatas = Resources.LoadAll<FlowerPatchData>("FlowerPatchData");
        exportData.flowerPatches = flowerPatchDatas.Select(data => new FlowerPatchExport
        {
            name = data.name,
            biomeType = data.biomeType.ToString(),
            displayName = data.displayName,
            description = data.description,
            pollenDisplayName = data.pollenDisplayName,
            pollenDescription = data.pollenDescription,
            placementCost = data.placementCost,
            capacityUpgradeCosts = data.capacityUpgradeCosts.ToList(),
            bonusCapacityPerUpgrade = data.bonusCapacityPerUpgrade,
            maxCapacityTier = data.maxCapacityTier,
            baseCapacity = data.baseCapacity,
            gatheringDuration = data.gatheringDuration
        }).OrderBy(x => x.name).ToList();

        // Export Recipes
        var recipes = Resources.LoadAll<HoneyRecipe>("Recipes");
        exportData.recipes = recipes.Select(recipe => new RecipeExport
        {
            name = recipe.name,
            recipeName = recipe.recipeName,
            description = recipe.description,
            isUnlockedByDefault = recipe.isUnlockedByDefault,
            unlockCost = recipe.unlockCost,
            ingredients = recipe.ingredients.Select(ing => new IngredientExport
            {
                pollenTypeName = ing.pollenType != null ? ing.pollenType.displayName : "Unknown",
                quantity = ing.quantity
            }).ToList(),
            productionTimeSeconds = recipe.productionTimeSeconds,
            honeyValue = recipe.honeyValue,
            upgradeCosts = recipe.upgradeCosts.ToList(),
            ingredientReductionPercent = recipe.ingredientReductionPercent.ToList(),
            productionTimeReductionPercent = recipe.productionTimeReductionPercent.ToList(),
            valueIncreasePercent = recipe.valueIncreasePercent.ToList(),
            honeyColor = new ColorExport
            {
                r = recipe.honeyColor.r,
                g = recipe.honeyColor.g,
                b = recipe.honeyColor.b,
                a = recipe.honeyColor.a
            }
        }).OrderBy(x => x.name).ToList();

        // Export BeeFleetUpgrade
        var beeFleetUpgrade = Resources.Load<BeeFleetUpgradeData>("BeeFleetUpgrade/BeeFleetUpgradeData");
        if (beeFleetUpgrade != null)
        {
            exportData.beeFleetUpgrade = new BeeFleetUpgradeExport
            {
                name = beeFleetUpgrade.name,
                beePurchaseCosts = beeFleetUpgrade.beePurchaseCosts.ToList(),
                beesPerPurchase = beeFleetUpgrade.beesPerPurchase.ToList(),
                maxPurchaseTier = beeFleetUpgrade.maxPurchaseTier
            };
        }

        // Write to JSON
        string json = JsonUtility.ToJson(exportData, true);
        string path = Path.Combine(Application.dataPath, "Resources", "GameAssetsExport.json");
        File.WriteAllText(path, json);

        AssetDatabase.Refresh();
        Debug.Log($"Assets exported to: {path}");
        EditorUtility.DisplayDialog("Export Complete", $"Assets exported to:\n{path}", "OK");
    }
}

[System.Serializable]
public class ExportData
{
    public List<FlowerPatchExport> flowerPatches = new List<FlowerPatchExport>();
    public List<RecipeExport> recipes = new List<RecipeExport>();
    public BeeFleetUpgradeExport beeFleetUpgrade;
}

[System.Serializable]
public class FlowerPatchExport
{
    public string name;
    public string biomeType;
    public string displayName;
    public string description;
    public string pollenDisplayName;
    public string pollenDescription;
    public float placementCost;
    public List<float> capacityUpgradeCosts;
    public int bonusCapacityPerUpgrade;
    public int maxCapacityTier;
    public int baseCapacity;
    public float gatheringDuration;
}

[System.Serializable]
public class RecipeExport
{
    public string name;
    public string recipeName;
    public string description;
    public bool isUnlockedByDefault;
    public float unlockCost;
    public List<IngredientExport> ingredients;
    public float productionTimeSeconds;
    public float honeyValue;
    public List<float> upgradeCosts;
    public List<float> ingredientReductionPercent;
    public List<float> productionTimeReductionPercent;
    public List<float> valueIncreasePercent;
    public ColorExport honeyColor;
}

[System.Serializable]
public class IngredientExport
{
    public string pollenTypeName;
    public int quantity;
}

[System.Serializable]
public class ColorExport
{
    public float r;
    public float g;
    public float b;
    public float a;
}

[System.Serializable]
public class BeeFleetUpgradeExport
{
    public string name;
    public List<float> beePurchaseCosts;
    public List<int> beesPerPurchase;
    public int maxPurchaseTier;
}