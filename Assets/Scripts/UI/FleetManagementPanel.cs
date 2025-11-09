using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the fleet management panel.
/// Displays global drone fleet information and allows players to allocate/deallocate drones to airports.
/// Shows a list of all airports with allocation controls.
/// </summary>
public class FleetManagementPanel : MonoBehaviour
{
    [Header("UI References - Panel")]
    [Tooltip("Root panel GameObject to show/hide")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Close button")]
    [SerializeField] private Button closeButton;

    [Header("UI References - Global Stats")]
    [Tooltip("Text displaying total drones owned")]
    [SerializeField] private TextMeshProUGUI totalDronesText;

    [Tooltip("Text displaying drones currently assigned")]
    [SerializeField] private TextMeshProUGUI assignedDronesText;

    [Tooltip("Text displaying available drones")]
    [SerializeField] private TextMeshProUGUI availableDronesText;

    [Header("UI References - Airport List")]
    [Tooltip("Container for airport allocation entries (vertical layout group)")]
    [SerializeField] private Transform airportListContainer;

    [Tooltip("Prefab for airport allocation entry")]
    [SerializeField] private GameObject airportEntryPrefab;

    [Header("Colors")]
    [Tooltip("Color when airport has capacity for more drones")]
    [SerializeField] private Color hasCapacityColor = Color.green;

    [Tooltip("Color when airport is at full capacity")]
    [SerializeField] private Color atCapacityColor = Color.red;

    [Tooltip("Color when no drones are available to allocate")]
    [SerializeField] private Color noDronesAvailableColor = Color.yellow;

    // Track spawned airport entries
    private Dictionary<AirportController, GameObject> airportEntries = new Dictionary<AirportController, GameObject>();

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
        if (DroneFleetManager.Instance != null)
        {
            DroneFleetManager.Instance.OnDroneAllocationChanged.AddListener(OnDroneAllocationChanged);
            DroneFleetManager.Instance.OnTotalDronesChanged.AddListener(OnTotalDronesChanged);
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

        if (DroneFleetManager.Instance != null)
        {
            DroneFleetManager.Instance.OnDroneAllocationChanged.RemoveListener(OnDroneAllocationChanged);
            DroneFleetManager.Instance.OnTotalDronesChanged.RemoveListener(OnTotalDronesChanged);
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
        UpdateAirportList();
    }

    /// <summary>
    /// Updates global fleet statistics display
    /// </summary>
    private void UpdateGlobalStats()
    {
        if (DroneFleetManager.Instance == null) return;

        int totalDrones = DroneFleetManager.Instance.TotalDronesOwned;
        int availableDrones = DroneFleetManager.Instance.GetAvailableDrones();
        int assignedDrones = totalDrones - availableDrones;

        if (totalDronesText != null)
        {
            totalDronesText.text = $"Total Drones: {totalDrones}";
        }

        if (assignedDronesText != null)
        {
            assignedDronesText.text = $"Assigned: {assignedDrones}";
        }

        if (availableDronesText != null)
        {
            availableDronesText.text = $"Available: {availableDrones}";
        }
    }

    /// <summary>
    /// Updates the list of all airports with allocation controls
    /// </summary>
    private void UpdateAirportList()
    {
        if (airportListContainer == null || airportEntryPrefab == null)
        {
            Debug.LogWarning("FleetManagementPanel: Missing airport list container or prefab");
            return;
        }

        // Find all airports in the scene
        AirportController[] allAirports = FindObjectsByType<AirportController>(FindObjectsSortMode.None);

        // Remove entries for airports that no longer exist
        List<AirportController> airportsToRemove = new List<AirportController>();
        foreach (var kvp in airportEntries)
        {
            bool airportStillExists = false;
            foreach (var airport in allAirports)
            {
                if (airport == kvp.Key)
                {
                    airportStillExists = true;
                    break;
                }
            }

            if (!airportStillExists)
            {
                airportsToRemove.Add(kvp.Key);
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
        }

        foreach (var airport in airportsToRemove)
        {
            airportEntries.Remove(airport);
        }

        // Create or update entries for all existing airports
        foreach (var airport in allAirports)
        {
            if (!airportEntries.ContainsKey(airport))
            {
                // Create new entry
                GameObject entryObject = Instantiate(airportEntryPrefab, airportListContainer);
                airportEntries[airport] = entryObject;

                // Setup entry
                SetupAirportEntry(entryObject, airport);
            }

            // Update entry data
            UpdateAirportEntry(airportEntries[airport], airport);
        }
    }

    /// <summary>
    /// Sets up button listeners for an airport entry
    /// </summary>
    private void SetupAirportEntry(GameObject entryObject, AirportController airport)
    {
        // Find add button
        Button addButton = entryObject.transform.Find("AddButton")?.GetComponent<Button>();
        if (addButton != null)
        {
            addButton.onClick.AddListener(() => OnAllocateDroneClicked(airport));
        }

        // Find remove button
        Button removeButton = entryObject.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeButton != null)
        {
            removeButton.onClick.AddListener(() => OnDeallocateDroneClicked(airport));
        }
    }

    /// <summary>
    /// Updates an airport entry's display
    /// </summary>
    private void UpdateAirportEntry(GameObject entryObject, AirportController airport)
    {
        if (entryObject == null || airport == null) return;

        int allocatedDrones = DroneFleetManager.Instance != null ? DroneFleetManager.Instance.GetAllocatedDrones(airport) : 0;
        int capacity = airport.MaxDroneCapacity;
        int availableDrones = DroneFleetManager.Instance != null ? DroneFleetManager.Instance.GetAvailableDrones() : 0;

        // Update airport name
        TextMeshProUGUI nameText = entryObject.transform.Find("AirportNameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            string biomeName = airport.GetBiomeType().ToString();
            nameText.text = $"{biomeName} Airport";
        }

        // Update allocation text
        TextMeshProUGUI allocationText = entryObject.transform.Find("AllocationText")?.GetComponent<TextMeshProUGUI>();
        if (allocationText != null)
        {
            allocationText.text = $"{allocatedDrones} / {capacity}";

            // Color coding
            if (allocatedDrones >= capacity)
            {
                allocationText.color = atCapacityColor;
            }
            else if (availableDrones <= 0)
            {
                allocationText.color = noDronesAvailableColor;
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
            // Disable if at capacity OR no drones available
            bool canAdd = allocatedDrones < capacity && availableDrones > 0;
            addButton.interactable = canAdd;
        }

        // Update remove button
        Button removeButton = entryObject.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeButton != null)
        {
            // Disable if no drones allocated
            bool canRemove = allocatedDrones > 0;
            removeButton.interactable = canRemove;
        }
    }

    /// <summary>
    /// Called when allocate drone button is clicked for an airport
    /// </summary>
    private void OnAllocateDroneClicked(AirportController airport)
    {
        if (airport == null || DroneFleetManager.Instance == null) return;

        bool success = DroneFleetManager.Instance.AllocateDrone(airport);

        if (!success)
        {
            Debug.LogWarning($"Failed to allocate drone to {airport.name}");
        }

        // Immediately refresh UI to show updated allocation
        RefreshUI();
    }

    /// <summary>
    /// Called when deallocate drone button is clicked for an airport
    /// </summary>
    private void OnDeallocateDroneClicked(AirportController airport)
    {
        if (airport == null || DroneFleetManager.Instance == null) return;

        bool success = DroneFleetManager.Instance.DeallocateDrone(airport);

        if (!success)
        {
            Debug.LogWarning($"Failed to deallocate drone from {airport.name}");
        }

        // Immediately refresh UI to show updated allocation
        RefreshUI();
    }

    /// <summary>
    /// Called when drone allocation changes
    /// </summary>
    private void OnDroneAllocationChanged(AirportController airport, int newAllocation)
    {
        // Only update if panel is visible
        if (panelRoot != null && panelRoot.activeSelf)
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// Called when total drones owned changes
    /// </summary>
    private void OnTotalDronesChanged(int newTotal)
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
