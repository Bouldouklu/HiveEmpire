/// <summary>
/// Defines the types of resources that can be produced and transported.
/// Currently supports Oil (from Desert) and Fish (from Coastal).
/// </summary>
public enum ResourceType
{
    Oil,
    Fish
}

/// <summary>
/// Defines the biome types for airports, which determine what resource they produce.
/// </summary>
public enum BiomeType
{
    Desert,  // Produces Oil
    Coastal  // Produces Fish
}
