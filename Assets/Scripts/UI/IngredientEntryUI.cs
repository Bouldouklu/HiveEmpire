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
    public float RequiredQuantity { get; private set; }

    /// <summary>
    /// Initializes the ingredient entry with pollen type and required quantity.
    /// </summary>
    public void Initialize(FlowerPatchData pollenType, float required)
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
                // Fallback: use pollen color derived from FlowerPatchMaterialMapper
                resourceIcon.sprite = null;
                resourceIcon.color = FlowerPatchMaterialMapper.Instance != null
                    ? FlowerPatchMaterialMapper.Instance.GetPollenColor(pollenType.biomeType)
                    : Color.yellow;
            }
        }
    }

    /// <summary>
    /// Updates the display with current available quantity and color coding.
    /// </summary>
    public void UpdateAvailability(float available, Color textColor)
    {
        if (quantityText != null)
        {
            // Display with 1 decimal if it's not a whole number
            string availableStr = available % 1 == 0 ? available.ToString("F0") : available.ToString("F1");
            string requiredStr = RequiredQuantity % 1 == 0 ? RequiredQuantity.ToString("F0") : RequiredQuantity.ToString("F1");
            quantityText.text = $"{availableStr}/{requiredStr}";
            quantityText.color = textColor;
        }
    }
}
