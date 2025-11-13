/// <summary>
/// Defines the types of resources that can be produced and transported.
/// Supports 6 resource types from different biomes.
/// </summary>
public enum ResourceType
{
    WildMeadowPollen,           // Wild Meadow biome
    OrchardPollen,              // Orchard biome
    CultivatedGardenPollen,     // Cultivated Garden biome
    MarshPollen,                // Marsh biome
    ForestEdgePollen,           // Forest Edge biome
    AgriculturalFieldPollen     // Agricultural Field biome
}

/// <summary>
/// Defines the biome types for flower patches, which determine what resource they produce.
/// Generation rates: Fast (Wild Meadow, Orchard), Medium (Cultivated Garden, Forest Edge, Agricultural Field), Slow (Marsh)
/// </summary>
public enum BiomeType
{
    WildMeadow,         // Produces WildMeadowPollen (fast: 1 per 2s)
    Orchard,            // Produces OrchardPollen (fast: 1 per 2s)
    CultivatedGarden,   // Produces CultivatedGardenPollen (medium: 1 per 4s)
    Marsh,              // Produces MarshPollen (slow: 1 per 6s)
    ForestEdge,         // Produces ForestEdgePollen (medium: 1 per 3s)
    AgriculturalField   // Produces AgriculturalFieldPollen (slow: 1 per 5s)
}
