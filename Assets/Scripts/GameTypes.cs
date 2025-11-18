/// <summary>
/// [OBSOLETE] Defines the types of resources that can be produced and transported.
/// Use FlowerPatchData ScriptableObjects instead - pollen types are now defined in FlowerPatchData assets.
/// This enum is kept for backwards compatibility during migration.
/// </summary>
[System.Obsolete("Use FlowerPatchData ScriptableObjects instead. Pollen properties (name, color, icon) are now defined in FlowerPatchData assets.", false)]
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
/// [OBSOLETE] Defines the biome types for flower patches, which determine what resource they produce.
/// Use FlowerPatchData ScriptableObjects instead - biome types are now defined within FlowerPatchData assets.
/// This enum is kept for backwards compatibility during migration.
/// </summary>
[System.Obsolete("Use FlowerPatchData ScriptableObjects instead. Biome type is now a property within FlowerPatchData assets.", false)]
public enum BiomeType
{
    WildMeadow,         // Produces WildMeadowPollen (fast: 1 per 2s)
    Orchard,            // Produces OrchardPollen (fast: 1 per 2s)
    CultivatedGarden,   // Produces CultivatedGardenPollen (medium: 1 per 4s)
    Marsh,              // Produces MarshPollen (slow: 1 per 6s)
    ForestEdge,         // Produces ForestEdgePollen (medium: 1 per 3s)
    AgriculturalField   // Produces AgriculturalFieldPollen (slow: 1 per 5s)
}
