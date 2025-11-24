using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI display for the seasonal system.
/// Shows current season, week progress, and active seasonal modifiers.
/// </summary>
public class SeasonUI : MonoBehaviour
{
    [Header("Season Display")]
    [Tooltip("Image displaying the current season icon")]
    [SerializeField] private Image seasonIconImage;

    [Tooltip("Text displaying the current season name")]
    [SerializeField] private TextMeshProUGUI seasonNameText;

    [Tooltip("Panel background to tint with season color")]
    [SerializeField] private Image seasonBackgroundPanel;

    [Header("Week Progress")]
    [Tooltip("Text displaying current week (e.g., 'Week 5 / 24')")]
    [SerializeField] private TextMeshProUGUI weekCounterText;

    [Tooltip("Progress bar showing completion of current week")]
    [SerializeField] private Slider weekProgressBar;

    [Tooltip("Progress bar showing completion of entire year")]
    [SerializeField] private Slider yearProgressBar;

    [Header("Season Modifiers")]
    [Tooltip("Text displaying active seasonal modifiers")]
    [SerializeField] private TextMeshProUGUI modifiersText;

    [Tooltip("Container for modifier icons (optional)")]
    [SerializeField] private Transform modifierIconsContainer;

    [Header("Colors")]
    [Tooltip("Default background color when no season data available")]
    [SerializeField] private Color defaultBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("Animation")]
    [Tooltip("Duration of season transition color lerp in seconds")]
    [SerializeField] private float transitionAnimationDuration = 0.5f;

    private Color targetBackgroundColor;
    private Color currentBackgroundColor;
    private float transitionTimer = 0f;
    private bool isTransitioning = false;

    private void Start()
    {
        // Subscribe to season manager events in Start to ensure SeasonManager.Instance exists
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.AddListener(OnSeasonChanged);
            SeasonManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);

            Debug.Log("[SeasonUI] Successfully subscribed to SeasonManager events");

            // Initial update
            UpdateSeasonDisplay();
            UpdateWeekDisplay();
        }
        else
        {
            Debug.LogError("[SeasonUI] SeasonManager not found! UI will not update. Make sure SeasonManager is in the scene.");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.RemoveListener(OnSeasonChanged);
            SeasonManager.Instance.OnWeekChanged.RemoveListener(OnWeekChanged);
        }
    }

    private void Update()
    {
        // Update progress bars every frame for smooth animation
        if (SeasonManager.Instance != null)
        {
            UpdateProgressBars();
        }

        // Handle background color transition animation
        if (isTransitioning && seasonBackgroundPanel != null)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionAnimationDuration);
            currentBackgroundColor = Color.Lerp(currentBackgroundColor, targetBackgroundColor, t);
            seasonBackgroundPanel.color = currentBackgroundColor;

            if (t >= 1.0f)
            {
                isTransitioning = false;
            }
        }
    }

    /// <summary>
    /// Called when the season changes
    /// </summary>
    private void OnSeasonChanged(Season newSeason)
    {
        Debug.Log($"[SeasonUI] Season changed to {newSeason}");
        UpdateSeasonDisplay();
    }

    /// <summary>
    /// Called when the week changes
    /// </summary>
    private void OnWeekChanged(int newWeek)
    {
        Debug.Log($"[SeasonUI] Week changed to {newWeek}");
        UpdateWeekDisplay();
    }

    /// <summary>
    /// Updates the season name, icon, and background color
    /// </summary>
    private void UpdateSeasonDisplay()
    {
        if (SeasonManager.Instance == null)
        {
            Debug.LogWarning("[SeasonUI] UpdateSeasonDisplay called but SeasonManager.Instance is null");
            return;
        }

        SeasonData seasonData = SeasonManager.Instance.GetCurrentSeasonData();

        if (seasonData == null)
        {
            Debug.LogWarning("[SeasonUI] No season data available for current season!");
            return;
        }

        Debug.Log($"[SeasonUI] Updating display for season: {seasonData.seasonName}");

        // Update season name
        if (seasonNameText != null)
        {
            seasonNameText.text = seasonData.seasonName;
            Debug.Log($"[SeasonUI] Set season name text to: {seasonData.seasonName}");
        }
        else
        {
            Debug.LogWarning("[SeasonUI] seasonNameText is not assigned in inspector!");
        }

        // Update season icon
        if (seasonIconImage != null)
        {
            if (seasonData.seasonIcon != null)
            {
                seasonIconImage.sprite = seasonData.seasonIcon;
                seasonIconImage.enabled = true;
            }
            else
            {
                seasonIconImage.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("[SeasonUI] seasonIconImage is not assigned in inspector!");
        }

        // Animate background color change
        if (seasonBackgroundPanel != null)
        {
            targetBackgroundColor = new Color(
                seasonData.seasonColor.r,
                seasonData.seasonColor.g,
                seasonData.seasonColor.b,
                seasonBackgroundPanel.color.a // Preserve alpha
            );

            currentBackgroundColor = seasonBackgroundPanel.color;
            transitionTimer = 0f;
            isTransitioning = true;
        }
        else
        {
            Debug.LogWarning("[SeasonUI] seasonBackgroundPanel is not assigned in inspector!");
        }

        // Update modifiers text
        UpdateModifiersDisplay(seasonData);
    }

    /// <summary>
    /// Updates the week counter text
    /// </summary>
    private void UpdateWeekDisplay()
    {
        if (SeasonManager.Instance == null)
        {
            Debug.LogWarning("[SeasonUI] UpdateWeekDisplay called but SeasonManager.Instance is null");
            return;
        }

        int currentWeek = SeasonManager.Instance.CurrentWeek;

        if (weekCounterText != null)
        {
            weekCounterText.text = $"Week {currentWeek} / {SeasonManager.TOTAL_WEEKS_IN_YEAR}";
            Debug.Log($"[SeasonUI] Set week counter to: Week {currentWeek} / {SeasonManager.TOTAL_WEEKS_IN_YEAR}");
        }
        else
        {
            Debug.LogWarning("[SeasonUI] weekCounterText is not assigned in inspector!");
        }
    }

    /// <summary>
    /// Updates progress bars (called every frame for smooth animation)
    /// </summary>
    private void UpdateProgressBars()
    {
        if (SeasonManager.Instance == null)
            return;

        // Update week progress bar
        if (weekProgressBar != null)
        {
            weekProgressBar.value = SeasonManager.Instance.WeekProgress;
        }

        // Update year progress bar
        if (yearProgressBar != null)
        {
            yearProgressBar.value = SeasonManager.Instance.YearProgress;
        }
    }

    /// <summary>
    /// Updates the seasonal modifiers display
    /// </summary>
    private void UpdateModifiersDisplay(SeasonData seasonData)
    {
        if (modifiersText == null)
        {
            Debug.LogWarning("[SeasonUI] modifiersText is not assigned in inspector!");
            return;
        }

        // Build modifier text
        string modifierText = "";

        // Income modifier
        if (!Mathf.Approximately(seasonData.incomeModifier, 1.0f))
        {
            float percentChange = (seasonData.incomeModifier - 1.0f) * 100f;
            modifierText += $"Income: {percentChange:+0;-0}%\n";
        }

        // Bee speed modifier
        if (!Mathf.Approximately(seasonData.beeSpeedModifier, 1.0f))
        {
            float percentChange = (seasonData.beeSpeedModifier - 1.0f) * 100f;
            modifierText += $"Bee Speed: {percentChange:+0;-0}%\n";
        }

        // Production time modifier
        if (!Mathf.Approximately(seasonData.productionTimeModifier, 1.0f))
        {
            float percentChange = (seasonData.productionTimeModifier - 1.0f) * 100f;
            modifierText += $"Production Time: {percentChange:+0;-0}%\n";
        }

        // If no modifiers, show "No active modifiers"
        if (string.IsNullOrEmpty(modifierText))
        {
            modifierText = "No active modifiers";
        }

        modifiersText.text = modifierText.TrimEnd('\n');
        Debug.Log($"[SeasonUI] Set modifiers text to: {modifiersText.text}");
    }

    /// <summary>
    /// Force refresh the entire UI display (useful for debugging)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateSeasonDisplay();
        UpdateWeekDisplay();
        UpdateProgressBars();
    }
}
