using UnityEngine;

/// <summary>
/// ScriptableObject that maps BiomeType to Material references.
/// Centralized material management for flower patches, pollen visuals, hover effects, and placeholders.
/// Single source of truth for all biome-related visual properties.
/// </summary>
[CreateAssetMenu(fileName = "FlowerPatchMaterialMapper", menuName = "Game/Flower Patch Material Mapper")]
public class FlowerPatchMaterialMapper : ScriptableObject
{
    [System.Serializable]
    public class FlowerPatchMaterialMapping
    {
        public BiomeType biomeType;
        public Material material;
    }

    [Header("Base Flower Patch Materials")]
    [Tooltip("Base materials for each flower patch biome type")]
    [SerializeField]
    private FlowerPatchMaterialMapping[] flowerPatchMaterials = new FlowerPatchMaterialMapping[6];

    [Header("Hover Effect Materials")]
    [Tooltip("Hover materials for each flower patch biome type (brighter variants)")]
    [SerializeField]
    private FlowerPatchMaterialMapping[] hoverMaterials = new FlowerPatchMaterialMapping[6];

    [Header("Hive Materials")]
    [Tooltip("Hover material for the hive (brighter variant)")]
    [SerializeField]
    private Material hiveHoverMaterial;

    [Header("Placeholder Materials")]
    [Tooltip("Material for placeholders when player can afford placement")]
    [SerializeField]
    private Material affordablePlaceholderMaterial;

    [Tooltip("Material for placeholders when player cannot afford placement")]
    [SerializeField]
    private Material unaffordablePlaceholderMaterial;

    [Tooltip("Default material for placeholders (neutral state)")]
    [SerializeField]
    private Material defaultPlaceholderMaterial;

    private static FlowerPatchMaterialMapper _instance;

    /// <summary>
    /// Singleton instance - loads from Resources folder on first access
    /// </summary>
    public static FlowerPatchMaterialMapper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<FlowerPatchMaterialMapper>("FlowerPatchMaterialMapper");

                if (_instance == null)
                {
                    Debug.LogError("FlowerPatchMaterialMapper not found in Resources folder! Please create one via Assets > Create > Game > Flower Patch Material Mapper");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Gets the base material associated with the specified biome type
    /// </summary>
    public Material GetFlowerPatchMaterial(BiomeType biomeType)
    {
        foreach (var mapping in flowerPatchMaterials)
        {
            if (mapping.biomeType == biomeType)
            {
                return mapping.material;
            }
        }

        Debug.LogWarning($"No material found for biome type: {biomeType}");
        return null;
    }

    /// <summary>
    /// Gets the hover material associated with the specified biome type
    /// </summary>
    public Material GetHoverMaterial(BiomeType biomeType)
    {
        foreach (var mapping in hoverMaterials)
        {
            if (mapping.biomeType == biomeType)
            {
                return mapping.material;
            }
        }

        Debug.LogWarning($"No hover material found for biome type: {biomeType}");
        return null;
    }

    /// <summary>
    /// Gets the pollen color for the specified biome type.
    /// Used for bee trail colors, pollen cube tinting, and UI elements.
    /// Extracts color directly from the biome's material to ensure consistency.
    /// </summary>
    public Color GetPollenColor(BiomeType biomeType)
    {
        Material patchMaterial = GetFlowerPatchMaterial(biomeType);

        if (patchMaterial == null)
        {
            Debug.LogWarning($"No material found for biome type: {biomeType}, returning yellow");
            return Color.yellow;
        }

        // Try URP _BaseColor property first (Unity 6 standard)
        if (patchMaterial.HasProperty("_BaseColor"))
        {
            return patchMaterial.GetColor("_BaseColor");
        }

        // Fallback to legacy _Color property
        if (patchMaterial.HasProperty("_Color"))
        {
            return patchMaterial.GetColor("_Color");
        }

        // Final fallback to material.color
        Debug.LogWarning($"Material for {biomeType} has no _BaseColor or _Color property, using material.color");
        return patchMaterial.color;
    }

    /// <summary>
    /// Gets the material for affordable placeholders (green)
    /// </summary>
    public Material GetAffordablePlaceholderMaterial()
    {
        if (affordablePlaceholderMaterial == null)
        {
            Debug.LogWarning("Affordable placeholder material not assigned in FlowerPatchMaterialMapper!");
        }
        return affordablePlaceholderMaterial;
    }

    /// <summary>
    /// Gets the material for unaffordable placeholders (red)
    /// </summary>
    public Material GetUnaffordablePlaceholderMaterial()
    {
        if (unaffordablePlaceholderMaterial == null)
        {
            Debug.LogWarning("Unaffordable placeholder material not assigned in FlowerPatchMaterialMapper!");
        }
        return unaffordablePlaceholderMaterial;
    }

    /// <summary>
    /// Gets the default material for placeholders (neutral)
    /// </summary>
    public Material GetDefaultPlaceholderMaterial()
    {
        if (defaultPlaceholderMaterial == null)
        {
            Debug.LogWarning("Default placeholder material not assigned in FlowerPatchMaterialMapper!");
        }
        return defaultPlaceholderMaterial;
    }

    /// <summary>
    /// Gets the hover material for the hive
    /// </summary>
    public Material GetHiveHoverMaterial()
    {
        if (hiveHoverMaterial == null)
        {
            Debug.LogWarning("Hive hover material not assigned in FlowerPatchMaterialMapper!");
        }
        return hiveHoverMaterial;
    }

    /// <summary>
    /// Validates that all biome types have assigned materials (Editor only)
    /// </summary>
    private void OnValidate()
    {
        #if UNITY_EDITOR
        // Ensure arrays have correct size for all biome types
        int biomeTypeCount = System.Enum.GetValues(typeof(BiomeType)).Length;

        if (flowerPatchMaterials == null || flowerPatchMaterials.Length != biomeTypeCount)
        {
            System.Array.Resize(ref flowerPatchMaterials, biomeTypeCount);
        }

        if (hoverMaterials == null || hoverMaterials.Length != biomeTypeCount)
        {
            System.Array.Resize(ref hoverMaterials, biomeTypeCount);
        }

        // Auto-assign biome types if not set
        BiomeType[] allBiomes = (BiomeType[])System.Enum.GetValues(typeof(BiomeType));

        for (int i = 0; i < biomeTypeCount; i++)
        {
            // Initialize base materials
            if (flowerPatchMaterials[i] == null)
            {
                flowerPatchMaterials[i] = new FlowerPatchMaterialMapping();
            }
            flowerPatchMaterials[i].biomeType = allBiomes[i];

            // Initialize hover materials
            if (hoverMaterials[i] == null)
            {
                hoverMaterials[i] = new FlowerPatchMaterialMapping();
            }
            hoverMaterials[i].biomeType = allBiomes[i];
        }

        // Validate hive materials
        if (hiveHoverMaterial == null)
        {
            Debug.LogWarning($"[{name}] Hive hover material is not assigned!");
        }

        // Validate placeholder materials
        if (affordablePlaceholderMaterial == null)
        {
            Debug.LogWarning($"[{name}] Affordable placeholder material is not assigned!");
        }

        if (unaffordablePlaceholderMaterial == null)
        {
            Debug.LogWarning($"[{name}] Unaffordable placeholder material is not assigned!");
        }

        if (defaultPlaceholderMaterial == null)
        {
            Debug.LogWarning($"[{name}] Default placeholder material is not assigned!");
        }
        #endif
    }
}
