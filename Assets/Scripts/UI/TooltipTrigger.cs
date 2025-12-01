using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component that triggers tooltip display on hover.
/// Attach to UI elements that should show tooltips.
/// </summary>
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Content")]
    [Tooltip("The tooltip text to display")]
    [SerializeField] [TextArea(2, 5)] private string tooltipText;

    /// <summary>
    /// Gets or sets the tooltip text
    /// </summary>
    public string TooltipText
    {
        get => tooltipText;
        set => tooltipText = value;
    }

    /// <summary>
    /// Called when pointer enters the UI element
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipController.Instance != null && !string.IsNullOrEmpty(tooltipText))
        {
            TooltipController.Instance.ShowTooltip(tooltipText, eventData.position);
        }
    }

    /// <summary>
    /// Called when pointer exits the UI element
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipController.Instance != null)
        {
            TooltipController.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        // Hide tooltip if this element is disabled while showing
        if (TooltipController.Instance != null)
        {
            TooltipController.Instance.HideTooltip();
        }
    }
}
