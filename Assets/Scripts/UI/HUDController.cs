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

    [Tooltip("TextMeshProUGUI component to display elapsed time")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Button References")]
    [Tooltip("Button to open the Fleet Management panel")]
    [SerializeField] private Button fleetButton;

    [Tooltip("Button to open the Settings panel")]
    [SerializeField] private Button settingsButton;

    [Header("Panel References")]
    [Tooltip("Reference to the Fleet Management Panel")]
    [SerializeField] private FleetManagementPanel fleetManagementPanel;

    [Tooltip("Reference to the Settings Controller")]
    [SerializeField] private SettingsController settingsController;

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

        // Subscribe to settings button click
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }
        else
        {
            Debug.LogWarning("HUDController: Settings button not assigned!");
        }

        // Validate Fleet Management Panel reference
        if (fleetManagementPanel == null)
        {
            Debug.LogWarning("HUDController: Fleet Management Panel not assigned in Inspector!");
        }

        // Validate Settings Controller reference
        if (settingsController == null)
        {
            Debug.LogWarning("HUDController: Settings Controller not assigned in Inspector!");
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

        // Initial display update
        UpdateDisplay();
        UpdateMoneyDisplay(0f);
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

        // Build resource display dynamically for all pollen types in inventory
        string resourceText = "";
        if (HiveController.Instance != null)
        {
            var inventory = HiveController.Instance.GetPollenInventory();

            foreach (var slot in inventory)
            {
                if (slot.pollenType == null)
                    continue;

                int count = slot.quantity;
                int capacity = HiveController.Instance.GetStorageCapacity(slot.pollenType);

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

                resourceText += $"<color={color}>{slot.pollenType.pollenDisplayName}: {count}/{capacity}</color>\n";
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

    /// <summary>
    /// Called when the Settings button is clicked.
    /// Toggles the Settings panel visibility.
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        if (settingsController != null)
        {
            settingsController.TogglePanel();
        }
        else
        {
            Debug.LogWarning("HUDController: Cannot open Settings panel - controller not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (fleetButton != null)
        {
            fleetButton.onClick.RemoveListener(OnFleetButtonClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        }

        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.RemoveListener(UpdateDisplay);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(UpdateMoneyDisplay);
        }
    }
}
