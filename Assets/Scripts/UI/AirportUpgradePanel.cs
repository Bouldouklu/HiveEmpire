using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the airport upgrade panel.
/// Displays airport information and allows players to upgrade airports.
/// This should be placed on a Canvas GameObject in the scene.
/// </summary>
public class AirportUpgradePanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root panel GameObject to show/hide")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Text displaying airport name/biome")]
    [SerializeField] private TextMeshProUGUI airportNameText;

    [Tooltip("Text displaying current tier")]
    [SerializeField] private TextMeshProUGUI currentTierText;

    [Tooltip("Text displaying current airplane count")]
    [SerializeField] private TextMeshProUGUI currentAirplanesText;

    [Tooltip("Text displaying next tier airplane count")]
    [SerializeField] private TextMeshProUGUI nextTierAirplanesText;

    [Tooltip("Text displaying upgrade cost")]
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    [Tooltip("Upgrade button")]
    [SerializeField] private Button upgradeButton;

    [Tooltip("Close button")]
    [SerializeField] private Button closeButton;

    [Tooltip("Text on upgrade button")]
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    [Header("Capacity Upgrade UI")]
    [Tooltip("Text displaying current capacity")]
    [SerializeField] private TextMeshProUGUI currentCapacityText;

    [Tooltip("Text displaying capacity upgrade cost")]
    [SerializeField] private TextMeshProUGUI capacityUpgradeCostText;

    [Tooltip("Capacity upgrade button")]
    [SerializeField] private Button capacityUpgradeButton;

    [Tooltip("Text on capacity upgrade button")]
    [SerializeField] private TextMeshProUGUI capacityUpgradeButtonText;

    [Header("Drone Allocation Display")]
    [Tooltip("Text showing current allocated drones vs capacity")]
    [SerializeField] private TextMeshProUGUI droneAllocationText;

    [Header("Colors")]
    [Tooltip("Color for affordable upgrades")]
    [SerializeField] private Color affordableColor = Color.green;

    [Tooltip("Color for unaffordable upgrades")]
    [SerializeField] private Color unaffordableColor = Color.red;

    [Tooltip("Color for max tier")]
    [SerializeField] private Color maxTierColor = Color.gray;

    // Current airport being upgraded
    private AirportController currentAirport;

    private void Awake()
    {
        // Setup button listeners
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }

        if (capacityUpgradeButton != null)
        {
            capacityUpgradeButton.onClick.AddListener(OnCapacityUpgradeButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
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
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
        }

        if (capacityUpgradeButton != null)
        {
            capacityUpgradeButton.onClick.RemoveListener(OnCapacityUpgradeButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
        }
    }

    /// <summary>
    /// Shows the upgrade panel for a specific airport
    /// </summary>
    public void ShowPanel(AirportController airport)
    {
        if (airport == null)
        {
            Debug.LogError("AirportUpgradePanel: Cannot show panel for null airport");
            return;
        }

        currentAirport = airport;

        // Show panel
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Update UI with airport information
        UpdateUI();
    }

    /// <summary>
    /// Hides the upgrade panel
    /// </summary>
    public void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        currentAirport = null;
    }

    /// <summary>
    /// Updates all UI elements with current airport information
    /// </summary>
    private void UpdateUI()
    {
        if (currentAirport == null)
        {
            return;
        }

        // Airport name/biome
        if (airportNameText != null)
        {
            string biomeName = currentAirport.GetBiomeType().ToString();
            airportNameText.text = $"{biomeName} Airport";
        }

        // Current tier
        if (currentTierText != null)
        {
            currentTierText.text = $"Tier: {currentAirport.GetTierDisplayName()}";
        }

        // Drone allocation display
        if (droneAllocationText != null && DroneFleetManager.Instance != null)
        {
            int allocatedDrones = DroneFleetManager.Instance.GetAllocatedDrones(currentAirport);
            int capacity = currentAirport.MaxDroneCapacity;
            droneAllocationText.text = $"Drones: {allocatedDrones} / {capacity}";
        }

        // Display drones added to pool through upgrades (repurposed from airplane count)
        if (currentAirplanesText != null)
        {
            int dronesAddedSoFar = currentAirport.GetMaxAirplanesForCurrentTier();
            currentAirplanesText.text = $"Drones added via upgrades: {dronesAddedSoFar}";
        }

        // Capacity upgrade section
        UpdateCapacityUpgradeUI();

        // Check if can upgrade
        bool canUpgrade = currentAirport.CanUpgrade();
        float upgradeCost = currentAirport.GetUpgradeCost();
        int nextTierPlanes = currentAirport.GetNextTierAirplaneCount();
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(upgradeCost);

        if (canUpgrade)
        {
            // Next tier benefit - shows drones that will be added to global pool
            if (nextTierAirplanesText != null)
            {
                int dronesAddedByUpgrade = nextTierPlanes - currentAirport.GetMaxAirplanesForCurrentTier();
                nextTierAirplanesText.text = $"Upgrade adds: +{dronesAddedByUpgrade} drones to global pool";
                nextTierAirplanesText.gameObject.SetActive(true);
            }

            // Upgrade cost
            if (upgradeCostText != null)
            {
                upgradeCostText.text = $"Cost: ${upgradeCost}";
                upgradeCostText.color = canAfford ? affordableColor : unaffordableColor;
            }

            // Upgrade button
            if (upgradeButton != null)
            {
                upgradeButton.interactable = canAfford;
            }

            if (upgradeButtonText != null)
            {
                upgradeButtonText.text = "Upgrade";
            }
        }
        else
        {
            // Max tier reached
            if (nextTierAirplanesText != null)
            {
                nextTierAirplanesText.text = "MAX TIER";
                nextTierAirplanesText.color = maxTierColor;
                nextTierAirplanesText.gameObject.SetActive(true);
            }

            if (upgradeCostText != null)
            {
                upgradeCostText.text = "";
            }

            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
            }

            if (upgradeButtonText != null)
            {
                upgradeButtonText.text = "Max Tier";
            }
        }
    }

    /// <summary>
    /// Called when the upgrade button is clicked
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (currentAirport == null)
        {
            Debug.LogError("AirportUpgradePanel: No airport selected");
            return;
        }

        // Attempt to upgrade
        bool success = currentAirport.UpgradeAirport();

        if (success)
        {
            Debug.Log($"Successfully upgraded {currentAirport.gameObject.name} to tier {currentAirport.GetCurrentTier()}");

            // Update UI to reflect new tier
            UpdateUI();

            // Optionally close panel after upgrade (comment out if you want it to stay open)
            // HidePanel();
        }
        else
        {
            Debug.LogWarning($"Failed to upgrade {currentAirport.gameObject.name}");
            // UI will be updated by OnMoneyChanged if it was an affordability issue
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
        if (panelRoot != null && panelRoot.activeSelf && currentAirport != null)
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// Updates capacity upgrade UI elements
    /// </summary>
    private void UpdateCapacityUpgradeUI()
    {
        if (currentAirport == null) return;

        // Current capacity display
        if (currentCapacityText != null)
        {
            int capacity = currentAirport.MaxDroneCapacity;
            currentCapacityText.text = $"Capacity: {capacity} drones";
        }

        // Check if can upgrade capacity
        bool canUpgradeCapacity = currentAirport.CanUpgradeCapacity();
        float capacityCost = currentAirport.GetCapacityUpgradeCost();
        int nextCapacity = currentAirport.GetNextCapacity();
        bool canAffordCapacity = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(capacityCost);

        if (canUpgradeCapacity)
        {
            // Capacity upgrade cost
            if (capacityUpgradeCostText != null)
            {
                capacityUpgradeCostText.text = $"Cost: ${capacityCost}";
                capacityUpgradeCostText.color = canAffordCapacity ? affordableColor : unaffordableColor;
            }

            // Capacity upgrade button
            if (capacityUpgradeButton != null)
            {
                capacityUpgradeButton.interactable = canAffordCapacity;
            }

            if (capacityUpgradeButtonText != null)
            {
                capacityUpgradeButtonText.text = $"Upgrade Capacity to {nextCapacity}";
            }
        }
        else
        {
            // Max capacity reached
            if (capacityUpgradeCostText != null)
            {
                capacityUpgradeCostText.text = "MAX CAPACITY";
                capacityUpgradeCostText.color = maxTierColor;
            }

            if (capacityUpgradeButton != null)
            {
                capacityUpgradeButton.interactable = false;
            }

            if (capacityUpgradeButtonText != null)
            {
                capacityUpgradeButtonText.text = "Max Capacity";
            }
        }
    }

    /// <summary>
    /// Called when the capacity upgrade button is clicked
    /// </summary>
    private void OnCapacityUpgradeButtonClicked()
    {
        if (currentAirport == null)
        {
            Debug.LogError("AirportUpgradePanel: No airport selected for capacity upgrade");
            return;
        }

        // Attempt to upgrade capacity
        bool success = currentAirport.UpgradeCapacity();

        if (success)
        {
            Debug.Log($"Successfully upgraded capacity for {currentAirport.gameObject.name} to {currentAirport.MaxDroneCapacity}");

            // Update UI to reflect new capacity
            UpdateUI();
        }
        else
        {
            Debug.LogWarning($"Failed to upgrade capacity for {currentAirport.gameObject.name}");
        }
    }
}
