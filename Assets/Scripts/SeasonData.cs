using UnityEngine;

/// <summary>
/// ScriptableObject defining seasonal characteristics and modifiers.
/// Each season has unique global modifiers that affect recipe values, honey prices, and flower patch capacity.
/// </summary>
[CreateAssetMenu(fileName = "NewSeasonData", menuName = "Game/Season Data")]
public class SeasonData : ScriptableObject
{
    [Header("Season Identity")]
    [Tooltip("The season this data represents")]
    public Season seasonType;

    [Tooltip("Display name for this season")]
    public string seasonName = "Spring";

    [Tooltip("Color associated with this season (for UI tinting)")]
    public Color seasonColor = Color.green;

    [Tooltip("Icon to display in season UI widget")]
    public Sprite seasonIcon;

    [Header("Duration")]
    [Tooltip("How many weeks this season lasts (default: 8 weeks per season)")]
    [Min(1)]
    public int weeksInSeason = 8;

    [Header("Global Modifiers")]
    [Tooltip("Multiplier for honey recipe income (e.g., 1.5 = +50% income during Summer demand)")]
    [Min(0.1f)]
    public float incomeModifier = 1.0f;

    [Tooltip("Multiplier for bee flight speed (e.g., 1.1 = +10% faster bees in Spring)")]
    [Min(0.1f)]
    public float beeSpeedModifier = 1.0f;

    [Tooltip("Multiplier for recipe production time (e.g., 0.85 = 15% faster production in Autumn)")]
    [Min(0.1f)]
    public float productionTimeModifier = 1.0f;

    [Tooltip("Multiplier for hive storage capacity (e.g., 1.2 = +20% storage in Spring)")]
    [Min(0.1f)]
    public float storageCapacityModifier = 1.0f;

    [Header("Weather Event Weights (Phase 2)")]
    [Tooltip("Spawn weight for mild weather events (0-1, higher = more frequent)")]
    [Range(0f, 1f)]
    public float mildEventWeight = 0.5f;

    [Tooltip("Spawn weight for moderate weather events (0-1)")]
    [Range(0f, 1f)]
    public float moderateEventWeight = 0.3f;

    [Tooltip("Spawn weight for severe weather events (0-1)")]
    [Range(0f, 1f)]
    public float severeEventWeight = 0.2f;

    [Header("Audio/Visual")]
    [Tooltip("Sound effect played when this season begins")]
    public AudioClip seasonStartSound;

    [Tooltip("Color tint for ambient lighting during this season")]
    public Color ambientLightColor = Color.white;

    [Tooltip("Optional skybox material for this season")]
    public Material skyboxMaterial;

    [Header("Description")]
    [Tooltip("Flavor text describing this season's characteristics")]
    [TextArea(3, 5)]
    public string description = "A season of growth and opportunity.";

    /// <summary>
    /// Get a formatted string describing the season's active modifiers
    /// </summary>
    public string GetModifiersSummary()
    {
        string summary = $"{seasonName} Modifiers:\n";

        if (!Mathf.Approximately(incomeModifier, 1.0f))
        {
            float percentChange = (incomeModifier - 1.0f) * 100f;
            summary += $"• Income: {percentChange:+0;-0}%\n";
        }

        if (!Mathf.Approximately(beeSpeedModifier, 1.0f))
        {
            float percentChange = (beeSpeedModifier - 1.0f) * 100f;
            summary += $"• Bee Speed: {percentChange:+0;-0}%\n";
        }

        if (!Mathf.Approximately(productionTimeModifier, 1.0f))
        {
            float percentChange = (productionTimeModifier - 1.0f) * 100f;
            summary += $"• Production Time: {percentChange:+0;-0}%\n";
        }

        if (!Mathf.Approximately(storageCapacityModifier, 1.0f))
        {
            float percentChange = (storageCapacityModifier - 1.0f) * 100f;
            summary += $"• Storage: {percentChange:+0;-0}%\n";
        }

        return summary.TrimEnd('\n');
    }

    /// <summary>
    /// Validate season data configuration in the editor
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(seasonName))
        {
            seasonName = seasonType.ToString();
        }

        // Clamp modifiers to reasonable ranges
        incomeModifier = Mathf.Clamp(incomeModifier, 0.1f, 5.0f);
        beeSpeedModifier = Mathf.Clamp(beeSpeedModifier, 0.1f, 3.0f);
        productionTimeModifier = Mathf.Clamp(productionTimeModifier, 0.1f, 3.0f);
        storageCapacityModifier = Mathf.Clamp(storageCapacityModifier, 0.1f, 3.0f);

        // Validate weeks in season
        if (weeksInSeason < 1)
        {
            Debug.LogWarning($"SeasonData '{seasonName}' has invalid weeksInSeason ({weeksInSeason}). Setting to 8.", this);
            weeksInSeason = 8;
        }
    }
}
