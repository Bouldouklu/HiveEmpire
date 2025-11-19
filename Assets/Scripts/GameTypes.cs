/// <summary>
/// Defines the biome types for flower patches.
/// Each biome type has associated visual properties (materials, colors) defined in FlowerPatchMaterialMapper.
/// FlowerPatchData ScriptableObjects use this enum to identify which biome they represent.
/// </summary>
public enum BiomeType
{
    WildMeadow,         // Fast-producing biome, close to hive
    Orchard,            // Fast-producing biome, close to hive
    CultivatedGarden,   // Medium-producing biome, moderate distance
    Marsh,              // Slow-producing biome, distant from hive
    ForestEdge,         // Medium-producing biome, moderate distance
    AgriculturalField   // Slow-producing biome, distant from hive
}
