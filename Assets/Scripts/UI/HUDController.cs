using UnityEngine;
using TMPro;

/// <summary>
/// Controls the main HUD display showing resource counts and airplane statistics.
/// Updates in real-time as resources are delivered and airplanes are spawned.
/// Uses TextMeshPro for crisp, high-quality text rendering.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI component to display resource and airplane counts")]
    [SerializeField] private TextMeshProUGUI hudText;

    [Tooltip("TextMeshProUGUI component to display player money (optional - falls back to hudText if not assigned)")]
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start()
    {
        // Subscribe to city resource changes
        if (CityController.Instance != null)
        {
            CityController.Instance.OnResourcesChanged.AddListener(UpdateDisplay);
        }
        else
        {
            Debug.LogError("HUDController: CityController not found in scene!");
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
        // Update display each frame to show real-time airplane count
        // (could be optimized with events if needed)
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the HUD text with current resource counts and airplane count.
    /// </summary>
    private void UpdateDisplay()
    {
        if (hudText == null)
        {
            Debug.LogWarning("HUDController: hudText is not assigned!");
            return;
        }

        // Get resource counts from city
        int oilCount = 0;
        int fishCount = 0;

        if (CityController.Instance != null)
        {
            oilCount = CityController.Instance.GetResourceCount(ResourceType.Oil);
            fishCount = CityController.Instance.GetResourceCount(ResourceType.Fish);
        }

        // Get airplane count from game manager
        int airplaneCount = 0;
        if (GameManager.Instance != null)
        {
            airplaneCount = GameManager.Instance.TotalAirplaneCount;
        }

        // Format display text
        hudText.text = $"Oil: {oilCount}  Fish: {fishCount}\nAirplanes: {airplaneCount}";
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

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (CityController.Instance != null)
        {
            CityController.Instance.OnResourcesChanged.RemoveListener(UpdateDisplay);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(UpdateMoneyDisplay);
        }
    }
}
