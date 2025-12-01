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

    [Tooltip("Button to open the How to Play panel")]
    [SerializeField] private Button howToPlayButton;

    [Header("Panel References")]
    [Tooltip("Reference to the Fleet Management Panel")]
    [SerializeField] private FleetManagementPanel fleetManagementPanel;

    [Tooltip("Reference to the Settings Controller")]
    [SerializeField] private SettingsController settingsController;

    [Tooltip("Reference to the How to Play Panel")]
    [SerializeField] private HowToPlayPanel howToPlayPanel;

    // Cached values to avoid unnecessary updates
    private int cachedBeeCount = 0;

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

        // Subscribe to how to play button click
        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.AddListener(OnHowToPlayButtonClicked);
        }
        else
        {
            Debug.LogWarning("HUDController: How to Play button not assigned!");
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

        // Validate How to Play Panel reference
        if (howToPlayPanel == null)
        {
            Debug.LogWarning("HUDController: How to Play Panel not assigned in Inspector!");
        }

        // Subscribe to bee count changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBeeCountChanged.AddListener(OnBeeCountChanged);
            // Initialize cached bee count
            cachedBeeCount = GameManager.Instance.TotalBeeCount;
        }
        else
        {
            Debug.LogWarning("HUDController: GameManager not found in scene!");
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
        // Only update timer display each frame (needs smooth updates)
        // Resource and bee count displays are now event-driven
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Updates the HUD text with current resource counts and bee count.
    /// Dynamically displays all pollen types currently in hive inventory.
    /// </summary>
    
    /// <summary>
    /// Called when the bee count changes.
    /// </summary>
    /// <param name="newBeeCount">New total bee count</param>
    private void OnBeeCountChanged(int newBeeCount)
    {
        cachedBeeCount = newBeeCount;
        UpdateDisplay();
    }
private void UpdateDisplay()
    {
        if (hudText == null)
        {
            Debug.LogWarning("HUDController: hudText is not assigned!");
            return;
        }

        // Use cached bee count (updated via events)
        int beeCount = cachedBeeCount;

        // Build resource display dynamically for all pollen types in inventory
        string resourceText = "";
        if (HiveController.Instance != null)
        {
            var inventory = HiveController.Instance.GetPollenInventory();

            foreach (var slot in inventory)
            {
                if (slot.pollenType == null)
                    continue;

                float count = slot.quantity;

                // Display count with 1 decimal if not a whole number
                string countStr = count % 1 == 0 ? count.ToString("F0") : count.ToString("F1");

                // Calculate net production rate (production - consumption)
                float productionRate = 0f;
                float consumptionRate = 0f;

                if (BeeFleetManager.Instance != null)
                {
                    productionRate = BeeFleetManager.Instance.GetPollenProductionRate(slot.pollenType);
                }

                if (RecipeProductionManager.Instance != null)
                {
                    consumptionRate = RecipeProductionManager.Instance.GetPollenConsumptionRate(slot.pollenType);
                }

                float netRate = productionRate - consumptionRate;

                // Format rate indicator with color
                string rateIndicator = "";
                if (netRate > 0.01f)
                {
                    rateIndicator = $" <color=#00FF00>(+{netRate:F1}/s)</color>";
                }
                else if (netRate < -0.01f)
                {
                    rateIndicator = $" <color=#FF4444>({netRate:F1}/s)</color>";
                }
                else if (Mathf.Abs(netRate) > 0.001f)
                {
                    // Near zero but not exactly zero
                    rateIndicator = $" <color=#FFFF00>({netRate:F2}/s)</color>";
                }

                resourceText += $"{slot.pollenType.pollenDisplayName}: {countStr}{rateIndicator}\n";
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

    /// <summary>
    /// Called when the How to Play button is clicked.
    /// Toggles the How to Play panel visibility.
    /// </summary>
    private void OnHowToPlayButtonClicked()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.TogglePanel();
        }
        else
        {
            Debug.LogWarning("HUDController: Cannot open How to Play panel - panel not found!");
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

        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveListener(OnHowToPlayButtonClicked);
        }

        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.RemoveListener(UpdateDisplay);
        }

        if (EconomyManager.Instance != null)
        {
            

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBeeCountChanged.RemoveListener(OnBeeCountChanged);
        }
EconomyManager.Instance.OnMoneyChanged.RemoveListener(UpdateMoneyDisplay);
        }
    }
}
