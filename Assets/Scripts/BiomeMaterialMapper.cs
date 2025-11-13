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
    /// Colors are from the AirFlow_ColorPalette.md design document.
    /// </summary>
    public Color GetBiomeColor(BiomeType biomeType)
    {
        switch (biomeType)
        {
            case BiomeType.WildMeadow:
                return new Color(0.561f, 0.702f, 0.6f); // #8fb399 - Muted sage green
            case BiomeType.CultivatedGarden:
                return new Color(0.557f, 0.624f, 0.69f); // #8e9fb0 - Cool slate blue
            case BiomeType.Marsh:
                return new Color(0.831f, 0.722f, 0.588f); // #d4b896 - Warm sand beige
            case BiomeType.Orchard:
                return new Color(0.788f, 0.722f, 0.561f); // #c9b88f - Golden wheat
            case BiomeType.ForestEdge:
                return new Color(0.427f, 0.584f, 0.671f); // #6d95ab - Ocean blue
            case BiomeType.AgriculturalField:
                return new Color(0.616f, 0.706f, 0.761f); // #9db4c2 - Icy blue-gray
            default:
                Debug.LogWarning($"No color defined for biome type: {biomeType}");
                return Color.white;
        }
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
