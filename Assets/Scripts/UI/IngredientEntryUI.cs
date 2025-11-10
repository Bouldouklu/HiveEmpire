using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper class for individual ingredient display in recipe entries.
/// Shows resource icon and current/required quantity with color coding.
/// </summary>
public class IngredientEntryUI : MonoBehaviour
{
    [SerializeField] private Image resourceIcon;
    [SerializeField] private TextMeshProUGUI quantityText;

    public ResourceType ResourceType { get; private set; }
    public int RequiredQuantity { get; private set; }

    /// <summary>
    /// Initializes the ingredient entry with resource type and required quantity.
    /// </summary>
    public void Initialize(ResourceType type, int required)
    {
        ResourceType = type;
        RequiredQuantity = required;

        // Set resource icon (you may need to implement icon mapping)
        if (resourceIcon != null)
        {
            // TODO: Map ResourceType to sprite
            // For now, color-code by resource type
            resourceIcon.color = GetResourceColor(type);
        }
    }

    /// <summary>
    /// Updates the display with current available quantity and color coding.
    /// </summary>
    public void UpdateAvailability(int available, Color textColor)
    {
        if (quantityText != null)
        {
            quantityText.text = $"{available}/{RequiredQuantity}";
            quantityText.color = textColor;
        }
    }

    /// <summary>
    /// Gets a color representation for each resource type.
    /// </summary>
    private Color GetResourceColor(ResourceType type)
    {
        // Temporary color mapping - replace with actual icon sprites later
        switch (type)
        {
            case ResourceType.ForestPollen:
                return new Color(0.6f, 0.4f, 0.2f); // Brown (Wood)
            case ResourceType.PlainsPollen:
                return new Color(0.8f, 0.8f, 0.2f); // Yellow (Food)
            case ResourceType.MountainPollen:
                return new Color(0.5f, 0.5f, 0.5f); // Gray (Stone)
            case ResourceType.DesertPollen:
                return new Color(0.2f, 0.2f, 0.2f); // Black (Oil)
            case ResourceType.CoastalPollen:
                return new Color(0.2f, 0.5f, 0.8f); // Blue (Fish)
            case ResourceType.TundraPollen:
                return new Color(0.8f, 0.9f, 1.0f); // Light blue (Minerals)
            default:
                return Color.white;
        }
    }
}
