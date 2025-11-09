using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the flowerPatch upgrade panel.
/// Displays flowerPatch information and allows players to upgrade flowerPatchs.
/// This should be placed on a Canvas GameObject in the scene.
/// </summary>
public class FlowerPatchUpgradePanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root panel GameObject to show/hide")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Text displaying flowerPatch name/biome")]
    [SerializeField] private TextMeshProUGUI flowerPatchNameText;

    [Tooltip("Text displaying current tier")]
    [SerializeField] private TextMeshProUGUI currentTierText;

    [Tooltip("Text displaying current bee count")]
    [SerializeField] private TextMeshProUGUI currentBeesText;

    [Tooltip("Text displaying next tier bee count")]
    [SerializeField] private TextMeshProUGUI nextTierBeesText;

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

    [Header("Bee Allocation Display")]
    [Tooltip("Text showing current allocated bees vs capacity")]
    [SerializeField] private TextMeshProUGUI beeAllocationText;

    [Header("Colors")]
    [Tooltip("Color for affordable upgrades")]
    [SerializeField] private Color affordableColor = Color.green;

    [Tooltip("Color for unaffordable upgrades")]
    [SerializeField] private Color unaffordableColor = Color.red;

    [Tooltip("Color for max tier")]
    [SerializeField] private Color maxTierColor = Color.gray;

    // Current flowerPatch being upgraded
    private FlowerPatchController currentFlowerPatch;

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
    /// Shows the upgrade panel for a specific flowerPatch
    /// </summary>
    public void ShowPanel(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null)
        {
            Debug.LogError("FlowerPatchUpgradePanel: Cannot show panel for null flowerPatch");
            return;
        }

        currentFlowerPatch = flowerPatch;

        // Show panel
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Update UI with flowerPatch information
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

        currentFlowerPatch = null;
    }

    /// <summary>
    /// Updates all UI elements with current flowerPatch information
    /// </summary>
    private void UpdateUI()
    {
        if (currentFlowerPatch == null)
        {
            return;
        }

        // FlowerPatch name/biome
        if (flowerPatchNameText != null)
        {
            string biomeName = currentFlowerPatch.GetBiomeType().ToString();
            flowerPatchNameText.text = $"{biomeName} FlowerPatch";
        }

        // Current tier
        if (currentTierText != null)
        {
            currentTierText.text = $"Tier: {currentFlowerPatch.GetTierDisplayName()}";
        }

        // Bee allocation display
        if (beeAllocationText != null && BeeFleetManager.Instance != null)
        {
            int allocatedBees = BeeFleetManager.Instance.GetAllocatedBees(currentFlowerPatch);
            int capacity = currentFlowerPatch.MaxBeeCapacity;
            beeAllocationText.text = $"Bees: {allocatedBees} / {capacity}";
        }

        // Display bees added to pool through upgrades (repurposed from bee count)
        if (currentBeesText != null)
        {
            int beesAddedSoFar = currentFlowerPatch.GetMaxBeesForCurrentTier();
            currentBeesText.text = $"Bees added via upgrades: {beesAddedSoFar}";
        }

        // Capacity upgrade section
        UpdateCapacityUpgradeUI();

        // Check if can upgrade
        bool canUpgrade = currentFlowerPatch.CanUpgrade();
        float upgradeCost = currentFlowerPatch.GetUpgradeCost();
        int nextTierPlanes = currentFlowerPatch.GetNextTierBeeCount();
        bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(upgradeCost);

        if (canUpgrade)
        {
            // Next tier benefit - shows bees that will be added to global pool
            if (nextTierBeesText != null)
            {
                int beesAddedByUpgrade = nextTierPlanes - currentFlowerPatch.GetMaxBeesForCurrentTier();
                nextTierBeesText.text = $"Upgrade adds: +{beesAddedByUpgrade} bees to global pool";
                nextTierBeesText.gameObject.SetActive(true);
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
            if (nextTierBeesText != null)
            {
                nextTierBeesText.text = "MAX TIER";
                nextTierBeesText.color = maxTierColor;
                nextTierBeesText.gameObject.SetActive(true);
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
        if (currentFlowerPatch == null)
        {
            Debug.LogError("FlowerPatchUpgradePanel: No flowerPatch selected");
            return;
        }

        // Attempt to upgrade
        bool success = currentFlowerPatch.UpgradeFlowerPatch();

        if (success)
        {
            Debug.Log($"Successfully upgraded {currentFlowerPatch.gameObject.name} to tier {currentFlowerPatch.GetCurrentTier()}");

            // Update UI to reflect new tier
            UpdateUI();

            // Optionally close panel after upgrade (comment out if you want it to stay open)
            // HidePanel();
        }
        else
        {
            Debug.LogWarning($"Failed to upgrade {currentFlowerPatch.gameObject.name}");
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
        if (panelRoot != null && panelRoot.activeSelf && currentFlowerPatch != null)
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// Updates capacity upgrade UI elements
    /// </summary>
    private void UpdateCapacityUpgradeUI()
    {
        if (currentFlowerPatch == null) return;

        // Current capacity display
        if (currentCapacityText != null)
        {
            int capacity = currentFlowerPatch.MaxBeeCapacity;
            currentCapacityText.text = $"Capacity: {capacity} bees";
        }

        // Check if can upgrade capacity
        bool canUpgradeCapacity = currentFlowerPatch.CanUpgradeCapacity();
        float capacityCost = currentFlowerPatch.GetCapacityUpgradeCost();
        int nextCapacity = currentFlowerPatch.GetNextCapacity();
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
        if (currentFlowerPatch == null)
        {
            Debug.LogError("FlowerPatchUpgradePanel: No flowerPatch selected for capacity upgrade");
            return;
        }

        // Attempt to upgrade capacity
        bool success = currentFlowerPatch.UpgradeCapacity();

        if (success)
        {
            Debug.Log($"Successfully upgraded capacity for {currentFlowerPatch.gameObject.name} to {currentFlowerPatch.MaxBeeCapacity}");

            // Update UI to reflect new capacity
            UpdateUI();
        }
        else
        {
            Debug.LogWarning($"Failed to upgrade capacity for {currentFlowerPatch.gameObject.name}");
        }
    }
}
