using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the "How to Play" panel UI that explains game mechanics.
/// Provides minimal onboarding for new players without a complex tutorial system.
/// Attached to UIManagers GameObject (NOT on the panel itself).
/// </summary>
public class HowToPlayPanel : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("Background blocker for click-outside-to-close functionality (on Canvas)")]
    [SerializeField] private GameObject panelBlocker;

    [Tooltip("Root GameObject for the How to Play panel (on Canvas)")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Close button to hide the panel")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        // Subscribe to close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("HowToPlayPanel: Close button not assigned!");
        }

        // Subscribe to panel blocker for click-outside-to-close
        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.AddListener(HidePanel);
            }
            else
            {
                Debug.LogWarning("HowToPlayPanel: PanelBlocker component not found on panelBlocker GameObject!");
            }
        }

        // Start hidden
        HidePanel();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HidePanel);
        }

        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.RemoveListener(HidePanel);
            }
        }
    }

    /// <summary>
    /// Shows the How to Play panel
    /// </summary>
    public void ShowPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (panelBlocker != null)
        {
            panelBlocker.SetActive(true);
        }

        Debug.Log("How to Play panel shown");
    }

    /// <summary>
    /// Hides the How to Play panel
    /// </summary>
    public void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (panelBlocker != null)
        {
            panelBlocker.SetActive(false);
        }

        Debug.Log("How to Play panel hidden");
    }

    /// <summary>
    /// Toggles the panel visibility
    /// </summary>
    public void TogglePanel()
    {
        if (panelRoot != null && panelRoot.activeSelf)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }
}
