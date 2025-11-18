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

    public FlowerPatchData PollenType { get; private set; }
    public int RequiredQuantity { get; private set; }

    /// <summary>
    /// Initializes the ingredient entry with pollen type and required quantity.
    /// </summary>
    public void Initialize(FlowerPatchData pollenType, int required)
    {
        PollenType = pollenType;
        RequiredQuantity = required;

        // Set resource icon and color from FlowerPatchData
        if (resourceIcon != null && pollenType != null)
        {
            // Use pollenIcon if available, otherwise color-code
            if (pollenType.pollenIcon != null)
            {
                resourceIcon.sprite = pollenType.pollenIcon;
                resourceIcon.color = Color.white; // Reset color to show icon properly
            }
            else
            {
                // Fallback: use pollen color
                resourceIcon.sprite = null;
                resourceIcon.color = pollenType.pollenColor;
            }
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
}
