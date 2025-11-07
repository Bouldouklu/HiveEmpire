using UnityEngine;

/// <summary>
/// Controls an airport that instantly provides resources based on its biome type.
/// Airports act as infinite resource dispensers - airplanes are the limiting factor.
/// Airplane spawning is now handled by RouteController component.
/// </summary>
public class AirportController : MonoBehaviour
{
    [Header("Airport Settings")]
    [Tooltip("Biome type determines what resource this airport produces")]
    [SerializeField] private BiomeType biomeType = BiomeType.Desert;

    /// <summary>
    /// Instantly provides a resource based on the airport's biome type.
    /// Called by airplanes when they need to pick up cargo.
    /// </summary>
    /// <returns>The resource type this airport produces</returns>
    public ResourceType GetResource()
    {
        // Map biome type to resource type
        return biomeType switch
        {
            BiomeType.Desert => ResourceType.Oil,
            BiomeType.Coastal => ResourceType.Fish,
            _ => ResourceType.Oil // Default fallback
        };
    }

    /// <summary>
    /// Gets the biome type of this airport (for external queries)
    /// </summary>
    public BiomeType GetBiomeType()
    {
        return biomeType;
    }
}
