/// <summary>
/// Defines the types of resources that can be produced and transported.
/// Supports 6 resource types from different biomes.
/// </summary>
public enum ResourceType
{
    Wood,     // Forest biome
    Food,     // Plains biome
    Stone,    // Mountain biome
    Oil,      // Desert biome
    Fish,     // Coastal biome
    Minerals  // Tundra biome
}

/// <summary>
/// Defines the biome types for airports, which determine what resource they produce.
/// Generation rates: Fast (Forest, Plains), Medium (Mountain, Coastal, Tundra), Slow (Desert)
/// </summary>
public enum BiomeType
{
    Forest,   // Produces Wood (fast: 1 per 2s)
    Plains,   // Produces Food (fast: 1 per 2s)
    Mountain, // Produces Stone (medium: 1 per 4s)
    Desert,   // Produces Oil (slow: 1 per 6s)
    Coastal,  // Produces Fish (medium: 1 per 3s)
    Tundra    // Produces Minerals (slow: 1 per 5s)
}
