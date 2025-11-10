using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the main HUD display showing resource counts and bee statistics.
/// Updates in real-time as resources are delivered and bees are spawned.
/// Uses TextMeshPro for crisp, high-quality text rendering.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI component to display resource and bee counts")]
    [SerializeField] private TextMeshProUGUI hudText;

    [Tooltip("TextMeshProUGUI component to display player money (optional - falls back to hudText if not assigned)")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Tooltip("TextMeshProUGUI component to display hive demands (e.g., 'Wood: 3/5')")]
    [SerializeField] private TextMeshProUGUI demandText;

    [Tooltip("TextMeshProUGUI component to display elapsed time")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Button References")]
    [Tooltip("Button to open the Fleet Management panel")]
    [SerializeField] private Button fleetButton;

    [Header("Panel References")]
    [Tooltip("Reference to the Fleet Management Panel")]
    [SerializeField] private FleetManagementPanel fleetManagementPanel;

    private void Start()
    {
        // Subscribe to fleet button click
        if (fleetButton != null)
        {
            fleetButton.onClick.AddListener(OnFleetButtonClicked);
        }
        else
        {
            Debug.LogWarning("HUDController: Fleet button not assigned!");
        }

        // Validate Fleet Management Panel reference
        if (fleetManagementPanel == null)
        {
            Debug.LogWarning("HUDController: Fleet Management Panel not assigned in Inspector!");
        }

        // Subscribe to hive resource changes
        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.AddListener(UpdateDisplay);
        }
        else
        {
            Debug.LogError("HUDController: HiveController not found in scene!");
        }

        // Subscribe to economy changes
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.AddListener(UpdateMoneyDisplay);
        }
        else
        {
            Debug.LogWarning("HUDController: EconomyManager not found in scene!");
        }

        // Subscribe to demand changes
        if (DemandManager.Instance != null)
        {
            DemandManager.Instance.OnDemandChanged.AddListener(UpdateDemandDisplay);
        }
        else
        {
            Debug.LogWarning("HUDController: DemandManager not found in scene!");
        }

        // Initial display update
        UpdateDisplay();
        UpdateMoneyDisplay(0f);
        UpdateDemandDisplay(ResourceType.ForestPollen, 0f, 0f); // Initial call to set up demand display
    }

    private void Update()
    {
        // Update display each frame to show real-time bee count and timer
        // (could be optimized with events if needed)
        UpdateDisplay();
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Updates the HUD text with current resource counts and bee count.
    /// Dynamically displays all resource types defined in ResourceType enum.
    /// </summary>
    private void UpdateDisplay()
    {
        if (hudText == null)
        {
            Debug.LogWarning("HUDController: hudText is not assigned!");
            return;
        }

        // Get bee count from game manager
        int beeCount = 0;
        if (GameManager.Instance != null)
        {
            beeCount = GameManager.Instance.TotalBeeCount;
        }

        // Build resource display dynamically for all resource types with storage capacity
        string resourceText = "";
        if (HiveController.Instance != null)
        {
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                int count = HiveController.Instance.GetResourceCount(resourceType);
                int capacity = HiveController.Instance.GetStorageCapacity(resourceType);

                // Color code based on fullness
                string color = "#FFFFFF"; // White default
                if (count >= capacity)
                {
                    color = "#FF6666"; // Red when full
                }
                else if (count >= capacity * 0.8f)
                {
                    color = "#FFCC66"; // Yellow when 80%+ full
                }

                resourceText += $"<color={color}>{resourceType}: {count}/{capacity}</color>\n";
            }
        }

        // Format display text with bee count at top
        hudText.text = $"Bees: {beeCount}\n{resourceText.TrimEnd()}";
    }

    /// <summary>
    /// Updates the money display with the current player balance.
    /// </summary>
    /// <param name="newAmount">New money amount from EconomyManager</param>
    private void UpdateMoneyDisplay(float newAmount)
    {
        // Use dedicated money text if assigned, otherwise include in main HUD text
        if (moneyText != null)
        {
            moneyText.text = $"${newAmount:F0}";
        }
        else if (hudText != null)
        {
            // Fallback: prepend to existing HUD text
            // This will be called in addition to UpdateDisplay(), so we need to be careful
            Debug.LogWarning("HUDController: moneyText not assigned. Assign a TextMeshProUGUI component for money display.");
        }
    }

    /// <summary>
    /// Updates the timer display showing elapsed game time in MM:SS format.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            timerText.text = "00:00";
            return;
        }

        // Get elapsed time from GameManager
        float elapsedTime = GameManager.Instance.ElapsedTime;

        // Convert to minutes and seconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        // Format as MM:SS
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Updates the demand display showing current delivery rates vs targets.
    /// Format: "Wood: 3/5" (green if met, red if not met)
    /// Only shows resources with active demands.
    /// </summary>
    /// <param name="resourceType">The resource type that changed (not directly used, but required by event)</param>
    /// <param name="demand">Target demand per minute (not directly used, but required by event)</param>
    /// <param name="currentRate">Current delivery rate (not directly used, but required by event)</param>
    private void UpdateDemandDisplay(ResourceType resourceType, float demand, float currentRate)
    {
        if (demandText == null)
        {
            return;
        }

        if (DemandManager.Instance == null)
        {
            demandText.text = "";
            return;
        }

        // Get all active demands
        var activeDemands = DemandManager.Instance.GetAllActiveDemands();

        if (activeDemands.Count == 0)
        {
            demandText.text = "No active demands";
            return;
        }

        // Build demand display string
        string displayText = "CITY DEMANDS:\n";

        foreach (var kvp in activeDemands)
        {
            ResourceType resource = kvp.Key;
            float targetDemand = kvp.Value;
            float deliveryRate = DemandManager.Instance.GetCurrentDeliveryRate(resource);
            bool isMet = DemandManager.Instance.IsDemandMet(resource);

            // Color code: green if met, red if not met
            string color = isMet ? "#00FF00" : "#FF0000"; // Green or Red
            string resourceName = resource.ToString();

            // Format: "Wood: 3.0/5.0" with color (shows 1 decimal to track gradual demand increases)
            displayText += $"<color={color}>{resourceName}: {deliveryRate:F1}/{targetDemand:F1}</color>\n";
        }

        demandText.text = displayText.TrimEnd('\n');
    }

    /// <summary>
    /// Called when the Fleet button is clicked.
    /// Toggles the Fleet Management panel visibility.
    /// </summary>
    private void OnFleetButtonClicked()
    {
        if (fleetManagementPanel != null)
        {
            fleetManagementPanel.TogglePanel();
        }
        else
        {
            Debug.LogWarning("HUDController: Cannot open Fleet Management panel - panel not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (fleetButton != null)
        {
            fleetButton.onClick.RemoveListener(OnFleetButtonClicked);
        }

        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.RemoveListener(UpdateDisplay);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(UpdateMoneyDisplay);
        }

        if (DemandManager.Instance != null)
        {
            DemandManager.Instance.OnDemandChanged.RemoveListener(UpdateDemandDisplay);
        }
    }
}
