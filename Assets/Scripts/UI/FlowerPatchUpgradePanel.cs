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

    [Tooltip("Close button")]
    [SerializeField] private Button closeButton;

    [Header("Capacity Upgrade UI")]
    [Tooltip("Text displaying current capacity")]
    [SerializeField] private TextMeshProUGUI currentCapacityText;

    [Tooltip("Text displaying capacity upgrade effect")]
    [SerializeField] private TextMeshProUGUI capacityUpgradeEffectText;

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

        // Bee allocation display
        if (beeAllocationText != null && BeeFleetManager.Instance != null)
        {
            int allocatedBees = BeeFleetManager.Instance.GetAllocatedBees(currentFlowerPatch);
            int capacity = currentFlowerPatch.MaxBeeCapacity;
            beeAllocationText.text = $"Bees: {allocatedBees} / {capacity}";
        }

        // Capacity upgrade section
        UpdateCapacityUpgradeUI();
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

        // Current capacity display with breakdown
        if (currentCapacityText != null)
        {
            int capacity = currentFlowerPatch.MaxBeeCapacity;
            int capacityTier = currentFlowerPatch.GetCapacityTier();

            // Show breakdown: base + bonus from capacity upgrades (capacity is now independent of nectar flow tiers)
            string breakdown = $"Capacity: {capacity} bees";
            if (capacityTier > 0)
            {
                int baseCapacity = 5; // Default base capacity
                int bonusCapacity = capacity - baseCapacity;
                breakdown += $" ({baseCapacity} base + {bonusCapacity} from upgrades)";
            }

            currentCapacityText.text = breakdown;
        }

        // Capacity upgrade section
        bool canUpgradeCapacity = currentFlowerPatch.CanUpgradeCapacity();
        float capacityCost = currentFlowerPatch.GetCapacityUpgradeCost();
        int nextCapacity = currentFlowerPatch.GetNextCapacity();
        bool canAffordCapacity = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(capacityCost);

        if (canUpgradeCapacity)
        {
            // Calculate capacity bonus that will be added
            int capacityBonus = nextCapacity - currentFlowerPatch.MaxBeeCapacity;
            int currentCapacityTier = currentFlowerPatch.GetCapacityTier();

            // Upgrade effect - shows tier and capacity that will be added
            if (capacityUpgradeEffectText != null)
            {
                capacityUpgradeEffectText.text = $"Tier {currentCapacityTier + 1}: +{capacityBonus} capacity";
                capacityUpgradeEffectText.color = affordableColor;
            }

            // Upgrade cost
            if (capacityUpgradeCostText != null)
            {
                capacityUpgradeCostText.text = $"Cost: ${capacityCost}";
                capacityUpgradeCostText.color = canAffordCapacity ? affordableColor : unaffordableColor;
            }

            // Upgrade button
            if (capacityUpgradeButton != null)
            {
                capacityUpgradeButton.interactable = canAffordCapacity;
            }

            if (capacityUpgradeButtonText != null)
            {
                int maxCapacityTier = currentFlowerPatch.GetMaxCapacityTier();
                capacityUpgradeButtonText.text = $"Upgrade ({currentCapacityTier + 1}/{maxCapacityTier})";
            }
        }
        else
        {
            // Max capacity reached - show in effect text
            if (capacityUpgradeEffectText != null)
            {
                capacityUpgradeEffectText.text = "MAX CAPACITY";
                capacityUpgradeEffectText.color = maxTierColor;
            }

            // Clear cost text when maxed
            if (capacityUpgradeCostText != null)
            {
                capacityUpgradeCostText.text = "";
            }

            // Disable button
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
