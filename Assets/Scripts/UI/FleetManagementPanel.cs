using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the fleet management panel.
/// Displays global bee fleet information and allows players to allocate/deallocate bees to flowerPatchs.
/// Shows a list of all flowerPatchs with allocation controls.
/// </summary>
public class FleetManagementPanel : MonoBehaviour
{
    [Header("UI References - Panel")]
    [Tooltip("Background blocker for click-outside-to-close functionality (on Canvas)")]
    [SerializeField] private GameObject panelBlocker;

    [Tooltip("Root panel GameObject to show/hide")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Close button")]
    [SerializeField] private Button closeButton;

    [Header("UI References - Global Stats")]
    [Tooltip("Text displaying total bees owned")]
    [SerializeField] private TextMeshProUGUI totalBeesText;

    [Tooltip("Text displaying bees currently assigned")]
    [SerializeField] private TextMeshProUGUI assignedBeesText;

    [Tooltip("Text displaying available bees")]
    [SerializeField] private TextMeshProUGUI availableBeesText;

    [Header("UI References - Bee Purchase")]
    [Tooltip("Button to purchase more bees")]
    [SerializeField] private Button buyBeesButton;

    [Tooltip("Text on buy bees button")]
    [SerializeField] private TextMeshProUGUI buyBeesButtonText;

    [Tooltip("Text displaying next bee purchase info (+X bees for Y gold)")]
    [SerializeField] private TextMeshProUGUI beePurchaseInfoText;

    [Header("UI References - FlowerPatch List")]
    [Tooltip("Container for flowerPatch allocation entries (vertical layout group)")]
    [SerializeField] private Transform flowerPatchListContainer;

    [Tooltip("Prefab for flowerPatch allocation entry")]
    [SerializeField] private GameObject flowerPatchEntryPrefab;

    [Header("Colors")]
    [Tooltip("Color when flowerPatch has capacity for more bees")]
    [SerializeField] private Color hasCapacityColor = Color.green;

    [Tooltip("Color when flowerPatch is at full capacity")]
    [SerializeField] private Color atCapacityColor = Color.red;

    [Tooltip("Color when no bees are available to allocate")]
    [SerializeField] private Color noBeesAvailableColor = Color.yellow;

    // Track spawned flowerPatch entries
    private Dictionary<FlowerPatchController, GameObject> flowerPatchEntries = new Dictionary<FlowerPatchController, GameObject>();

    private void Awake()
    {
        // Setup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        if (buyBeesButton != null)
        {
            buyBeesButton.onClick.AddListener(OnBuyBeesButtonClicked);
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
                Debug.LogWarning("FleetManagementPanel: Panel blocker does not have a PanelBlocker component!");
            }
        }
        else
        {
            Debug.LogWarning("FleetManagementPanel: Panel blocker not assigned!");
        }

        // Hide panel by default
        HidePanel();

        // Subscribe to fleet manager events
        if (BeeFleetManager.Instance != null)
        {
            BeeFleetManager.Instance.OnBeeAllocationChanged.AddListener(OnBeeAllocationChanged);
            BeeFleetManager.Instance.OnTotalBeesChanged.AddListener(OnTotalBeesChanged);
        }

        // Subscribe to economy events (for capacity upgrade affordability)
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        if (buyBeesButton != null)
        {
            buyBeesButton.onClick.RemoveListener(OnBuyBeesButtonClicked);
        }

        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.RemoveListener(HidePanel);
            }
        }

        if (BeeFleetManager.Instance != null)
        {
            BeeFleetManager.Instance.OnBeeAllocationChanged.RemoveListener(OnBeeAllocationChanged);
            BeeFleetManager.Instance.OnTotalBeesChanged.RemoveListener(OnTotalBeesChanged);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
        }
    }

    /// <summary>
    /// Shows the fleet management panel
    /// </summary>
    public void ShowPanel()
    {
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(true);
        }
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Refresh all data
        RefreshUI();
    }

    /// <summary>
    /// Hides the fleet management panel
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
    }

    /// <summary>
    /// Toggles panel visibility
    /// </summary>
    public void TogglePanel()
    {
        if (panelRoot != null)
        {
            if (panelRoot.activeSelf)
            {
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
        }
    }

    /// <summary>
    /// Refreshes all UI elements
    /// </summary>
    private void RefreshUI()
    {
        UpdateGlobalStats();
        UpdateFlowerPatchList();
    }

    /// <summary>
    /// Updates global fleet statistics display
    /// </summary>
    private void UpdateGlobalStats()
    {
        if (BeeFleetManager.Instance == null) return;

        int totalBees = BeeFleetManager.Instance.TotalBeesOwned;
        int availableBees = BeeFleetManager.Instance.GetAvailableBees();
        int assignedBees = totalBees - availableBees;

        if (totalBeesText != null)
        {
            totalBeesText.text = $"Total Bees: {totalBees}";
        }

        if (assignedBeesText != null)
        {
            assignedBeesText.text = $"Assigned: {assignedBees}";
        }

        if (availableBeesText != null)
        {
            availableBeesText.text = $"Available: {availableBees}";
        }

        // Update bee purchase UI
        UpdateBeePurchaseUI();
    }

    /// <summary>
    /// Updates bee purchase UI (Buy Bees button and info text)
    /// </summary>
    private void UpdateBeePurchaseUI()
    {
        if (BeeFleetManager.Instance == null) return;

        bool canPurchase = BeeFleetManager.Instance.CanPurchaseBees();
        float purchaseCost = BeeFleetManager.Instance.GetBeePurchaseCost();
        int beeAmount = BeeFleetManager.Instance.GetBeePurchaseAmount();
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(purchaseCost);

        if (canPurchase)
        {
            // Update info text
            if (beePurchaseInfoText != null)
            {
                int currentTier = BeeFleetManager.Instance.GetCurrentPurchaseTier();
                int maxTier = BeeFleetManager.Instance.GetMaxPurchaseTier();
                beePurchaseInfoText.text = $"Tier {currentTier + 1}/{maxTier}: +{beeAmount} bees for ${purchaseCost}";
            }

            // Update button
            if (buyBeesButton != null)
            {
                buyBeesButton.interactable = canAfford;
            }

            if (buyBeesButtonText != null)
            {
                buyBeesButtonText.text = canAfford ? "Buy Bees" : "Buy Bees (Can't Afford)";
            }
        }
        else
        {
            // Max tier reached
            if (beePurchaseInfoText != null)
            {
                beePurchaseInfoText.text = "MAX TIER - All bees purchased";
            }

            if (buyBeesButton != null)
            {
                buyBeesButton.interactable = false;
            }

            if (buyBeesButtonText != null)
            {
                buyBeesButtonText.text = "Max Bees";
            }
        }
    }

    /// <summary>
    /// Updates the list of all flowerPatchs with allocation controls
    /// </summary>
    private void UpdateFlowerPatchList()
    {
        if (flowerPatchListContainer == null || flowerPatchEntryPrefab == null)
        {
            Debug.LogWarning("FleetManagementPanel: Missing flowerPatch list container or prefab");
            return;
        }

        // Find all flowerPatchs in the scene
        FlowerPatchController[] allFlowerPatchs = FindObjectsByType<FlowerPatchController>(FindObjectsSortMode.None);

        // Remove entries for flowerPatchs that no longer exist
        List<FlowerPatchController> flowerPatchsToRemove = new List<FlowerPatchController>();
        foreach (var kvp in flowerPatchEntries)
        {
            bool flowerPatchStillExists = false;
            foreach (var flowerPatch in allFlowerPatchs)
            {
                if (flowerPatch == kvp.Key)
                {
                    flowerPatchStillExists = true;
                    break;
                }
            }

            if (!flowerPatchStillExists)
            {
                flowerPatchsToRemove.Add(kvp.Key);
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
        }

        foreach (var flowerPatch in flowerPatchsToRemove)
        {
            flowerPatchEntries.Remove(flowerPatch);
        }

        // Create or update entries for all existing flowerPatchs
        foreach (var flowerPatch in allFlowerPatchs)
        {
            if (!flowerPatchEntries.ContainsKey(flowerPatch))
            {
                // Create new entry
                GameObject entryObject = Instantiate(flowerPatchEntryPrefab, flowerPatchListContainer);
                flowerPatchEntries[flowerPatch] = entryObject;

                // Setup entry
                SetupFlowerPatchEntry(entryObject, flowerPatch);
            }

            // Update entry data
            UpdateFlowerPatchEntry(flowerPatchEntries[flowerPatch], flowerPatch);
        }
    }

    /// <summary>
    /// Sets up button listeners for an flowerPatch entry
    /// </summary>
    private void SetupFlowerPatchEntry(GameObject entryObject, FlowerPatchController flowerPatch)
    {
        // Find add button
        Button addButton = entryObject.transform.Find("AddButton")?.GetComponent<Button>();
        if (addButton != null)
        {
            addButton.onClick.AddListener(() => OnAllocateBeeClicked(flowerPatch));
        }

        // Find remove button
        Button removeButton = entryObject.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeButton != null)
        {
            removeButton.onClick.AddListener(() => OnDeallocateBeeClicked(flowerPatch));
        }
    }

    /// <summary>
    /// Updates an flowerPatch entry's display
    /// </summary>
    private void UpdateFlowerPatchEntry(GameObject entryObject, FlowerPatchController flowerPatch)
    {
        if (entryObject == null || flowerPatch == null) return;

        int allocatedBees = BeeFleetManager.Instance != null ? BeeFleetManager.Instance.GetAllocatedBees(flowerPatch) : 0;
        int capacity = flowerPatch.MaxBeeCapacity;
        int availableBees = BeeFleetManager.Instance != null ? BeeFleetManager.Instance.GetAvailableBees() : 0;

        // Update flowerPatch name
        TextMeshProUGUI nameText = entryObject.transform.Find("FlowerPatchNameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            string biomeName = flowerPatch.GetBiomeType().ToString();
            nameText.text = $"{biomeName} FlowerPatch";
        }

        // Update allocation text
        TextMeshProUGUI allocationText = entryObject.transform.Find("AllocationText")?.GetComponent<TextMeshProUGUI>();
        if (allocationText != null)
        {
            allocationText.text = $"{allocatedBees} / {capacity}";

            // Color coding
            if (allocatedBees >= capacity)
            {
                allocationText.color = atCapacityColor;
            }
            else if (availableBees <= 0)
            {
                allocationText.color = noBeesAvailableColor;
            }
            else
            {
                allocationText.color = hasCapacityColor;
            }
        }

        // Update add button
        Button addButton = entryObject.transform.Find("AddButton")?.GetComponent<Button>();
        if (addButton != null)
        {
            // Disable if at capacity OR no bees available
            bool canAdd = allocatedBees < capacity && availableBees > 0;
            addButton.interactable = canAdd;
        }

        // Update remove button
        Button removeButton = entryObject.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeButton != null)
        {
            // Disable if no bees allocated
            bool canRemove = allocatedBees > 0;
            removeButton.interactable = canRemove;
        }
    }

    /// <summary>
    /// Called when allocate bee button is clicked for an flowerPatch
    /// </summary>
    private void OnAllocateBeeClicked(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null || BeeFleetManager.Instance == null) return;

        bool success = BeeFleetManager.Instance.AllocateBee(flowerPatch);

        if (!success)
        {
            Debug.LogWarning($"Failed to allocate bee to {flowerPatch.name}");
        }

        // Immediately refresh UI to show updated allocation
        RefreshUI();
    }

    /// <summary>
    /// Called when deallocate bee button is clicked for an flowerPatch
    /// </summary>
    private void OnDeallocateBeeClicked(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null || BeeFleetManager.Instance == null) return;

        bool success = BeeFleetManager.Instance.DeallocateBee(flowerPatch);

        if (!success)
        {
            Debug.LogWarning($"Failed to deallocate bee from {flowerPatch.name}");
        }

        // Immediately refresh UI to show updated allocation
        RefreshUI();
    }

    /// <summary>
    /// Called when bee allocation changes
    /// </summary>
    private void OnBeeAllocationChanged(FlowerPatchController flowerPatch, int newAllocation)
    {
        // Only update if panel is visible
        if (panelRoot != null && panelRoot.activeSelf)
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// Called when total bees owned changes
    /// </summary>
    private void OnTotalBeesChanged(int newTotal)
    {
        // Only update if panel is visible
        if (panelRoot != null && panelRoot.activeSelf)
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// Called when player money changes
    /// </summary>
    private void OnMoneyChanged(float newAmount)
    {
        // Only update if panel is visible
        if (panelRoot != null && panelRoot.activeSelf)
        {
            // Capacity upgrade affordability may have changed
            RefreshUI();
        }
    }

    /// <summary>
    /// Called when the close button is clicked
    /// </summary>
    /// <summary>
    /// Handles Buy Bees button click
    /// </summary>
    private void OnBuyBeesButtonClicked()
    {
        if (BeeFleetManager.Instance == null)
        {
            Debug.LogWarning("FleetManagementPanel: BeeFleetManager instance not found");
            return;
        }

        // Attempt to purchase bees
        bool success = BeeFleetManager.Instance.PurchaseBees();

        if (success)
        {
            // Update UI to reflect purchase (UpdateGlobalStats will be called via OnTotalBeesChanged event)
            Debug.Log("FleetManagementPanel: Bee purchase successful!");
        }
        else
        {
            Debug.Log("FleetManagementPanel: Bee purchase failed (see BeeFleetManager logs for details)");
        }
    }

    private void OnCloseButtonClicked()
    {
        HidePanel();
    }
}
