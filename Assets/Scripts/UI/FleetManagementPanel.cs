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
    private void OnCloseButtonClicked()
    {
        HidePanel();
    }
}
