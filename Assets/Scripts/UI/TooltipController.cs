using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Singleton controller for managing UI tooltips.
/// Displays contextual help text when hovering over UI elements.
/// </summary>
public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }

    [Header("Tooltip UI")]
    [Tooltip("The tooltip panel GameObject")]
    [SerializeField] private GameObject tooltipPanel;

    [Tooltip("Text component for tooltip content")]
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Configuration")]
    [Tooltip("Delay before showing tooltip (seconds)")]
    [SerializeField] private float showDelay = 0.5f;

    [Tooltip("Offset from cursor position")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(15f, -15f);

    [Tooltip("How far mouse must move to hide tooltip (pixels)")]
    [SerializeField] private float mouseMovementThreshold = 5f;

    private Coroutine showCoroutine;
    private RectTransform tooltipRectTransform;
    private Canvas parentCanvas;
    private Vector2 lastMousePosition;
    private bool isShowingTooltip = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get components
        if (tooltipPanel != null)
        {
            tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
        }

        // Start hidden
        HideTooltip();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Shows the tooltip with the specified text after a delay
    /// </summary>
    public void ShowTooltip(string text, Vector2 position)
    {
        // Cancel any existing show coroutine
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        // Start delayed show
        showCoroutine = StartCoroutine(ShowTooltipDelayed(text, position));
    }

    /// <summary>
    /// Shows the tooltip immediately at cursor position
    /// </summary>
    public void ShowTooltipImmediate(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            HideTooltip();
            return;
        }

        // Set text
        if (tooltipText != null)
        {
            tooltipText.text = text;
        }

        // Show panel
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }

        // Track that we're showing and store initial mouse position
        isShowingTooltip = true;
        lastMousePosition = Input.mousePosition;

        // Position at cursor
        UpdateTooltipPosition();
    }

    /// <summary>
    /// Hides the tooltip immediately
    /// </summary>
    public void HideTooltip()
    {
        // Cancel any pending show
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        // Hide panel
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        isShowingTooltip = false;
    }

    /// <summary>
    /// Updates tooltip position to follow cursor
    /// </summary>
    public void UpdateTooltipPosition()
    {
        if (tooltipPanel == null || !tooltipPanel.activeSelf || tooltipRectTransform == null)
        {
            return;
        }

        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;

        // Convert screen position to canvas local position
        if (parentCanvas != null)
        {
            RectTransform canvasRect = parentCanvas.transform as RectTransform;

            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For Screen Space Overlay, use direct mouse position
                localPoint = mousePos;
            }
            else
            {
                // For Screen Space Camera or World Space
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    mousePos,
                    parentCanvas.worldCamera,
                    out localPoint
                );
            }
        }
        else
        {
            localPoint = mousePos;
        }

        // Apply offset
        localPoint += cursorOffset;

        // Clamp to screen bounds
        ClampToScreen(ref localPoint);

        // Apply position
        tooltipRectTransform.position = localPoint;
    }

    private IEnumerator ShowTooltipDelayed(string text, Vector2 position)
    {
        // Wait for delay
        yield return new WaitForSeconds(showDelay);

        // Show tooltip
        ShowTooltipImmediate(text);

        showCoroutine = null;
    }

    private void ClampToScreen(ref Vector2 position)
    {
        if (tooltipRectTransform == null)
        {
            return;
        }

        // Get tooltip size
        Vector2 tooltipSize = tooltipRectTransform.sizeDelta;

        // For Screen Space Overlay, clamp to screen bounds
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Clamp to screen
            position.x = Mathf.Clamp(position.x, 0, Screen.width - tooltipSize.x);
            position.y = Mathf.Clamp(position.y, tooltipSize.y, Screen.height);
        }
        else if (parentCanvas != null)
        {
            // For other modes, clamp to canvas bounds
            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            if (canvasRect != null)
            {
                Vector2 canvasSize = canvasRect.sizeDelta;
                float minX = -canvasSize.x / 2f;
                float maxX = canvasSize.x / 2f - tooltipSize.x;
                float minY = -canvasSize.y / 2f + tooltipSize.y;
                float maxY = canvasSize.y / 2f;

                position.x = Mathf.Clamp(position.x, minX, maxX);
                position.y = Mathf.Clamp(position.y, minY, maxY);
            }
        }
    }

    private void Update()
    {
        // Check if mouse has moved significantly while tooltip is showing
        if (isShowingTooltip && tooltipPanel != null && tooltipPanel.activeSelf)
        {
            Vector2 currentMousePos = Input.mousePosition;
            float mouseDelta = Vector2.Distance(currentMousePos, lastMousePosition);

            // If mouse moved more than threshold, hide tooltip
            if (mouseDelta > mouseMovementThreshold)
            {
                HideTooltip();
                return;
            }
        }

        // Update position if tooltip is visible (only if mouse hasn't moved much)
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }
}
