/// <summary>
/// Defines the types of resources that can be produced and transported.
/// Supports 6 resource types from different biomes.
/// </summary>
public enum ResourceType
{
    ForestPollen,     // Forest biome
    PlainsPollen,     // Plains biome
    MountainPollen,    // Mountain biome
    DesertPollen,      // Desert biome
    CoastalPollen,     // Coastal biome
    TundraPollen  // Tundra biome
}

/// <summary>
/// Defines the biome types for airports, which determine what resource they produce.
/// Generation rates: Fast (Forest, Plains), Medium (Mountain, Coastal, Tundra), Slow (Desert)
/// </summary>
public enum BiomeType
{
    Forest,   // Produces ForestPollen (fast: 1 per 2s)
    Plains,   // Produces PlainsPollen (fast: 1 per 2s)
    Mountain, // Produces MountainPollen (medium: 1 per 4s)
    Desert,   // Produces DesertPollen (slow: 1 per 6s)
    Coastal,  // Produces CoastalPollen (medium: 1 per 3s)
    Tundra    // Produces TundraPollen (slow: 1 per 5s)
}
