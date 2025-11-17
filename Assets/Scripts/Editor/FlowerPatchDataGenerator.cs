using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to automatically generate FlowerPatchData ScriptableObject assets
/// for all biome types with proper prefab references.
/// Usage: Assets -> Hive Empire -> Generate FlowerPatchData Assets
/// </summary>
public class FlowerPatchDataGenerator : EditorWindow
{
    [MenuItem("Assets/Hive Empire/Generate FlowerPatchData Assets")]
    public static void GenerateFlowerPatchDataAssets()
    {
        // Define the output directory
        string outputPath = "Assets/Resources/FlowerPatchData";

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder(outputPath))
        {
            Directory.CreateDirectory(outputPath.Replace("Assets/", Application.dataPath + "/"));
            AssetDatabase.Refresh();
        }

        // Define all biome configurations
        var biomeConfigs = new[]
        {
            new { BiomeType = BiomeType.WildMeadow, DisplayName = "Wild Meadow Patch", Cost = 10f, PrefabPath = "Assets/Prefabs/FlowerPatches/WildMeadowPatch.prefab" },
            new { BiomeType = BiomeType.Orchard, DisplayName = "Orchard Patch", Cost = 10f, PrefabPath = "Assets/Prefabs/FlowerPatches/OrchardPatch.prefab" },
            new { BiomeType = BiomeType.CultivatedGarden, DisplayName = "Cultivated Garden Patch", Cost = 20f, PrefabPath = "Assets/Prefabs/FlowerPatches/CultivatedGardenPatch.prefab" },
            new { BiomeType = BiomeType.ForestEdge, DisplayName = "Forest Edge Patch", Cost = 20f, PrefabPath = "Assets/Prefabs/FlowerPatches/ForestEdgePatch.prefab" },
            new { BiomeType = BiomeType.Marsh, DisplayName = "Marsh Patch", Cost = 30f, PrefabPath = "Assets/Prefabs/FlowerPatches/MarshPatch.prefab" },
            new { BiomeType = BiomeType.AgriculturalField, DisplayName = "Agricultural Field Patch", Cost = 30f, PrefabPath = "Assets/Prefabs/FlowerPatches/AgriculturalFieldPatch.prefab" }
        };

        int createdCount = 0;
        int updatedCount = 0;

        foreach (var config in biomeConfigs)
        {
            // Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(config.PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Could not find prefab at path: {config.PrefabPath}");
                continue;
            }

            // Create asset path
            string assetPath = $"{outputPath}/{config.BiomeType}FlowerPatchData.asset";

            // Check if asset already exists
            FlowerPatchData existingData = AssetDatabase.LoadAssetAtPath<FlowerPatchData>(assetPath);
            bool isUpdate = existingData != null;

            // Create or reuse the ScriptableObject
            FlowerPatchData data = existingData != null ? existingData : ScriptableObject.CreateInstance<FlowerPatchData>();

            // Configure the data
            data.biomeType = config.BiomeType;
            data.displayName = config.DisplayName;
            data.description = $"A flower patch in the {config.BiomeType} biome.";
            data.placementCost = config.Cost;
            data.flowerPatchPrefab = prefab;

            // Set capacity upgrade costs (standard values)
            data.capacityUpgradeCosts = new float[] { 100f, 250f, 500f };
            data.bonusCapacityPerUpgrade = 3;
            data.maxCapacityTier = 3;
            data.baseCapacity = 5;

            if (isUpdate)
            {
                // Mark the existing asset as dirty to save changes
                EditorUtility.SetDirty(data);
                updatedCount++;
                Debug.Log($"Updated existing FlowerPatchData: {assetPath}");
            }
            else
            {
                // Create new asset
                AssetDatabase.CreateAsset(data, assetPath);
                createdCount++;
                Debug.Log($"Created new FlowerPatchData: {assetPath}");
            }
        }

        // Save and refresh
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Show summary
        string message = $"FlowerPatchData Generation Complete!\n\nCreated: {createdCount} assets\nUpdated: {updatedCount} assets\n\nLocation: {outputPath}";
        EditorUtility.DisplayDialog("Success", message, "OK");

        Debug.Log($"[FlowerPatchDataGenerator] {message.Replace("\n", " ")}");
    }
}
