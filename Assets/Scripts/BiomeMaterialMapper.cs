using UnityEngine;

/// <summary>
/// ScriptableObject that maps BiomeType to Material references.
/// Used to assign appropriate materials to airplane cargo based on flowerPatch biome.
/// </summary>
[CreateAssetMenu(fileName = "BiomeMaterialMapper", menuName = "Game/Biome Material Mapper")]
public class BiomeMaterialMapper : ScriptableObject
{
    [System.Serializable]
    public class BiomeMaterialPair
    {
        public BiomeType biomeType;
        public Material material;
    }

    [SerializeField]
    private BiomeMaterialPair[] biomeMaterials = new BiomeMaterialPair[6];

    private static BiomeMaterialMapper _instance;

    /// <summary>
    /// Singleton instance - loads from Resources folder on first access
    /// </summary>
    public static BiomeMaterialMapper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<BiomeMaterialMapper>("BiomeMaterialMapper");

                if (_instance == null)
                {
                    Debug.LogError("BiomeMaterialMapper not found in Resources folder! Please create one via Assets > Create > Game > Biome Material Mapper");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Gets the material associated with the specified biome type
    /// </summary>
    public Material GetBiomeMaterial(BiomeType biomeType)
    {
        foreach (var pair in biomeMaterials)
        {
            if (pair.biomeType == biomeType)
            {
                return pair.material;
            }
        }

        Debug.LogWarning($"No material found for biome type: {biomeType}");
        return null;
    }

    /// <summary>
    /// Gets the accent color for the specified biome type.
    /// Used for trail colors and other visual effects.
    /// Extracts color directly from the biome's material to ensure consistency.
    /// </summary>
    public Color GetBiomeColor(BiomeType biomeType)
    {
        Material biomeMaterial = GetBiomeMaterial(biomeType);

        if (biomeMaterial == null)
        {
            Debug.LogWarning($"No material found for biome type: {biomeType}, returning white");
            return Color.white;
        }

        // Try URP _BaseColor property first (Unity 6 standard)
        if (biomeMaterial.HasProperty("_BaseColor"))
        {
            return biomeMaterial.GetColor("_BaseColor");
        }

        // Fallback to legacy _Color property
        if (biomeMaterial.HasProperty("_Color"))
        {
            return biomeMaterial.GetColor("_Color");
        }

        // Final fallback to material.color
        Debug.LogWarning($"Material for {biomeType} has no _BaseColor or _Color property, using material.color");
        return biomeMaterial.color;
    }

    /// <summary>
    /// Validates that all biome types have assigned materials (Editor only)
    /// </summary>
    private void OnValidate()
    {
        #if UNITY_EDITOR
        // Ensure array has correct size for all biome types
        int biomeTypeCount = System.Enum.GetValues(typeof(BiomeType)).Length;
        if (biomeMaterials == null || biomeMaterials.Length != biomeTypeCount)
        {
            System.Array.Resize(ref biomeMaterials, biomeTypeCount);
        }

        // Auto-assign biome types if not set
        BiomeType[] allBiomes = (BiomeType[])System.Enum.GetValues(typeof(BiomeType));
        for (int i = 0; i < biomeMaterials.Length; i++)
        {
            if (biomeMaterials[i] == null)
            {
                biomeMaterials[i] = new BiomeMaterialPair();
            }
            biomeMaterials[i].biomeType = allBiomes[i];
        }
        #endif
    }
}
