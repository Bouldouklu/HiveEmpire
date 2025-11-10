using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the settings panel UI for adjusting audio volume levels.
/// Attached to UIManagers GameObject (NOT on the panel itself).
/// Manages three volume sliders: Music, Ambient, and SFX.
/// </summary>
public class SettingsController : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("Background blocker for click-outside-to-close functionality (on Canvas)")]
    [SerializeField] private GameObject panelBlocker;

    [Tooltip("Root GameObject for the settings panel (on Canvas)")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Close button to hide the panel")]
    [SerializeField] private Button closeButton;

    [Header("Volume Sliders")]
    [Tooltip("Slider for music volume (0-1)")]
    [SerializeField] private Slider musicVolumeSlider;

    [Tooltip("Slider for ambient sound volume (0-1)")]
    [SerializeField] private Slider ambientVolumeSlider;

    [Tooltip("Slider for SFX volume (0-1)")]
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Volume Labels (Optional)")]
    [Tooltip("Text label showing current music volume percentage")]
    [SerializeField] private TextMeshProUGUI musicVolumeLabel;

    [Tooltip("Text label showing current ambient volume percentage")]
    [SerializeField] private TextMeshProUGUI ambientVolumeLabel;

    [Tooltip("Text label showing current SFX volume percentage")]
    [SerializeField] private TextMeshProUGUI sfxVolumeLabel;

    private void Awake()
    {
        // Subscribe to close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("SettingsController: Close button not assigned!");
        }

        // Subscribe to panel blocker for click-outside-to-close
        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.AddListener(HidePanel);
            }
            else
            {
                Debug.LogWarning("SettingsController: Panel blocker does not have a PanelBlocker component!");
            }
        }
        else
        {
            Debug.LogWarning("SettingsController: Panel blocker not assigned!");
        }

        // Initialize panel as hidden
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(false);
        }
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Start()
    {
        InitializeSliders();
        SubscribeToSliderEvents();
    }

    /// <summary>
    /// Initialize sliders with current AudioManager volume values
    /// </summary>
    private void InitializeSliders()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("SettingsController: AudioManager not found!");
            return;
        }

        // Set slider values from serialized AudioManager fields
        // Note: We need to access the private fields via the AudioManager's current volume state
        // Since the AudioManager doesn't expose getters, we'll use the default values
        // and the sliders will update the actual volumes when changed

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = 0.7f; // Default from AudioManager
            UpdateMusicVolumeLabel(musicVolumeSlider.value);
        }

        if (ambientVolumeSlider != null)
        {
            ambientVolumeSlider.minValue = 0f;
            ambientVolumeSlider.maxValue = 1f;
            ambientVolumeSlider.value = 0.5f; // Default from AudioManager
            UpdateAmbientVolumeLabel(ambientVolumeSlider.value);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.value = 0.8f; // Default from AudioManager
            UpdateSFXVolumeLabel(sfxVolumeSlider.value);
        }
    }

    /// <summary>
    /// Subscribe to slider value change events
    /// </summary>
    private void SubscribeToSliderEvents()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (ambientVolumeSlider != null)
        {
            ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    #region Event Handlers

    /// <summary>
    /// Called when music volume slider value changes
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        UpdateMusicVolumeLabel(value);
    }

    /// <summary>
    /// Called when ambient volume slider value changes
    /// </summary>
    private void OnAmbientVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAmbientVolume(value);
        }
        UpdateAmbientVolumeLabel(value);
    }

    /// <summary>
    /// Called when SFX volume slider value changes
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        UpdateSFXVolumeLabel(value);
    }

    #endregion

    #region Label Updates

    /// <summary>
    /// Update music volume label with percentage
    /// </summary>
    private void UpdateMusicVolumeLabel(float value)
    {
        if (musicVolumeLabel != null)
        {
            musicVolumeLabel.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    /// <summary>
    /// Update ambient volume label with percentage
    /// </summary>
    private void UpdateAmbientVolumeLabel(float value)
    {
        if (ambientVolumeLabel != null)
        {
            ambientVolumeLabel.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    /// <summary>
    /// Update SFX volume label with percentage
    /// </summary>
    private void UpdateSFXVolumeLabel(float value)
    {
        if (sfxVolumeLabel != null)
        {
            sfxVolumeLabel.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    #endregion

    #region Panel Visibility

    /// <summary>
    /// Show the settings panel
    /// </summary>
    public void ShowPanel()
    {
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(true);
        }
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Hide the settings panel
    /// </summary>
    public void HidePanel()
    {
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(false);
        }
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Toggle the settings panel visibility
    /// </summary>
    public void TogglePanel()
    {
        bool isActive = panelRoot != null && panelRoot.activeSelf;

        if (isActive)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from all events to prevent memory leaks
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HidePanel);
        }

        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.RemoveListener(HidePanel);
            }
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        if (ambientVolumeSlider != null)
        {
            ambientVolumeSlider.onValueChanged.RemoveListener(OnAmbientVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
    }
}
