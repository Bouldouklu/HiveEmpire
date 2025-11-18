using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the flower patch unlock/purchase panel.
/// Displays flower patch information and allows players to unlock locked patches.
/// Attached to UIManagers GameObject (NOT on the panel itself).
/// </summary>
public class FlowerPatchUnlockPanel : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("Background blocker for click-outside-to-close functionality (on Canvas)")]
    [SerializeField] private GameObject panelBlocker;

    [Tooltip("Root panel GameObject to show/hide")]
    [SerializeField] private GameObject panelRoot;

    [Header("UI Text Elements")]
    [Tooltip("Text displaying flower patch name/biome")]
    [SerializeField] private TextMeshProUGUI flowerPatchNameText;

    [Tooltip("Text displaying flower patch description")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Tooltip("Text displaying unlock cost")]
    [SerializeField] private TextMeshProUGUI unlockCostText;

    [Tooltip("Unlock/Purchase button")]
    [SerializeField] private Button unlockButton;

    [Tooltip("Text on unlock button")]
    [SerializeField] private TextMeshProUGUI unlockButtonText;

    [Tooltip("Close button")]
    [SerializeField] private Button closeButton;

    // Current flower patch being unlocked
    private FlowerPatchController currentFlowerPatch;

    private void Awake()
    {
        // Setup button listeners
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }
        else
        {
            Debug.LogWarning("FlowerPatchUnlockPanel: Unlock button not assigned!");
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        else
        {
            Debug.LogWarning("FlowerPatchUnlockPanel: Close button not assigned!");
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
                Debug.LogWarning("FlowerPatchUnlockPanel: Panel blocker does not have a PanelBlocker component!");
            }
        }
        else
        {
            Debug.LogWarning("FlowerPatchUnlockPanel: Panel blocker not assigned!");
        }

        // Hide panel by default
        HidePanel();

        // Subscribe to economy changes to update affordability
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.RemoveListener(HidePanel);
            }
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
        }
    }

    /// <summary>
    /// Shows the unlock panel for a specific locked flower patch
    /// </summary>
    public void ShowPanel(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null)
        {
            Debug.LogError("FlowerPatchUnlockPanel: Cannot show panel for null flower patch");
            return;
        }

        if (!flowerPatch.IsLocked)
        {
            Debug.LogWarning($"FlowerPatchUnlockPanel: Flower patch {flowerPatch.name} is already unlocked");
            return;
        }

        currentFlowerPatch = flowerPatch;

        // Show blocker and panel
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(true);
        }
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Update UI with flower patch information
        UpdateUI();
    }

    /// <summary>
    /// Hides the unlock panel
    /// </summary>
    public void HidePanel()
    {
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(false);
        }
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        currentFlowerPatch = null;
    }

    /// <summary>
    /// Updates all UI elements with current flower patch information
    /// </summary>
    private void UpdateUI()
    {
        if (currentFlowerPatch == null || currentFlowerPatch.FlowerPatchData == null)
        {
            return;
        }

        FlowerPatchData data = currentFlowerPatch.FlowerPatchData;

        // Flower patch name
        if (flowerPatchNameText != null)
        {
            flowerPatchNameText.text = data.displayName;
        }

        // Description
        if (descriptionText != null)
        {
            if (!string.IsNullOrEmpty(data.description))
            {
                descriptionText.text = data.description;
            }
            else
            {
                descriptionText.text = $"Unlock this {data.biomeType} flower patch to start collecting {data.pollenDisplayName}.";
            }
        }

        // Unlock cost and button
        UpdateUnlockButton();
    }

    /// <summary>
    /// Updates unlock button state and cost display
    /// </summary>
    private void UpdateUnlockButton()
    {
        if (currentFlowerPatch == null) return;

        float unlockCost = currentFlowerPatch.GetUnlockCost();
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(unlockCost);

        // Get colors from material mapper (single source of truth)
        Color affordableColor = Color.green; // Fallback
        Color unaffordableColor = Color.red; // Fallback
        if (FlowerPatchMaterialMapper.Instance != null)
        {
            affordableColor = FlowerPatchMaterialMapper.Instance.GetAffordableColor();
            unaffordableColor = FlowerPatchMaterialMapper.Instance.GetUnaffordableColor();
        }

        // Update cost text
        if (unlockCostText != null)
        {
            unlockCostText.text = $"Cost: ${unlockCost:F0}";
            unlockCostText.color = canAfford ? affordableColor : unaffordableColor;
        }

        // Update button
        if (unlockButton != null)
        {
            unlockButton.interactable = canAfford;
        }

        if (unlockButtonText != null)
        {
            unlockButtonText.text = canAfford ? "Unlock" : "Insufficient Funds";
        }
    }

    /// <summary>
    /// Called when the close button is clicked
    /// </summary>
    private void OnCloseButtonClicked()
    {
        HidePanel();
    }

    /// <summary>
    /// Called when player money changes (updates affordability)
    /// </summary>
    private void OnMoneyChanged(float newAmount)
    {
        // Only update if panel is visible
        if (panelRoot != null && panelRoot.activeSelf && currentFlowerPatch != null)
        {
            UpdateUnlockButton();
        }
    }

    /// <summary>
    /// Called when the unlock button is clicked
    /// </summary>
    private void OnUnlockButtonClicked()
    {
        if (currentFlowerPatch == null)
        {
            Debug.LogError("FlowerPatchUnlockPanel: No flower patch selected for unlock");
            return;
        }

        // Attempt to unlock
        bool success = currentFlowerPatch.UnlockPatch();

        if (success)
        {
            Debug.Log($"Successfully unlocked {currentFlowerPatch.name}");

            // Close panel after successful unlock
            HidePanel();
        }
        else
        {
            Debug.LogWarning($"Failed to unlock {currentFlowerPatch.name}");
            // Panel stays open so player can see why it failed (via console/UI feedback)
        }
    }
}
