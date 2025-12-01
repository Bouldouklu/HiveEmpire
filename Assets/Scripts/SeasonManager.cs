using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the seasonal time progression system for the year-long campaign.
/// Tracks current season, week progression, and applies seasonal modifiers.
/// </summary>
public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance { get; private set; }

    [Header("Season Configuration")]
    [Tooltip("Spring season data asset")]
    [SerializeField] private SeasonData springData;

    [Tooltip("Summer season data asset")]
    [SerializeField] private SeasonData summerData;

    [Tooltip("Autumn season data asset")]
    [SerializeField] private SeasonData autumnData;

    [Header("Time Settings")]
    [Tooltip("Enable or disable the entire season system")]
    [SerializeField] private bool enableSeasonSystem = true;

    [Tooltip("How many real seconds equal one game week (default: 60 = 1 minute per week)")]
    [SerializeField] private float realSecondsPerGameWeek = 60f;

    [Tooltip("Should the season timer start automatically on game start?")]
    [SerializeField] private bool startTimerOnAwake = true;

    [Header("Current State (Debug)")]
    [Tooltip("Current season of the year")]
    [SerializeField] private Season currentSeason = Season.Spring;

    [Tooltip("Current week number (1-21)")]
    [SerializeField] private int currentWeek = 1;

    [Tooltip("Time accumulated toward next week (in real seconds)")]
    [SerializeField] private float weekTimer = 0f;

    [Tooltip("Is the season timer currently running?")]
    [SerializeField] private bool isTimerRunning = false;

    // Public Properties
    /// <summary>Gets the current season</summary>
    public Season CurrentSeason => currentSeason;

    /// <summary>Gets the current week (1-21)</summary>
    public int CurrentWeek => currentWeek;

    /// <summary>Gets progress through current week (0-1)</summary>
    public float WeekProgress => Mathf.Clamp01(weekTimer / realSecondsPerGameWeek);

    /// <summary>Gets progress through entire year (0-1)</summary>
    public float YearProgress => Mathf.Clamp01((currentWeek - 1) / 21f);

    /// <summary>Gets the total number of weeks in the entire year</summary>
    public const int TOTAL_WEEKS_IN_YEAR = 21;

    // Events
    [Header("Events")]
    public UnityEvent<Season> OnSeasonChanged = new UnityEvent<Season>();
    public UnityEvent<int> OnWeekChanged = new UnityEvent<int>();
    public UnityEvent OnYearEnded = new UnityEvent();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[SeasonManager] Multiple instances detected. Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }

        Instance = this;

        // Validate season data references
        if (springData == null || summerData == null || autumnData == null)
        {
            Debug.LogError("[SeasonManager] Missing season data references! Please assign all season data assets in inspector.");
        }
    }

    private void Start()
    {
        if (!enableSeasonSystem)
        {
            Debug.Log("[SeasonManager] Season system is DISABLED. Enable 'enableSeasonSystem' to activate.");
            return;
        }

        if (startTimerOnAwake)
        {
            StartNewYear();
        }
    }

    private void Update()
    {
        if (!enableSeasonSystem || !isTimerRunning)
            return;

        // Accumulate time (respects Time.timeScale for game speed control)
        weekTimer += Time.deltaTime;

        // Check if a week has passed
        if (weekTimer >= realSecondsPerGameWeek)
        {
            weekTimer -= realSecondsPerGameWeek;
            AdvanceWeek();
        }

#if UNITY_EDITOR
        // Debug shortcuts for testing
        HandleDebugInputs();
#endif
    }

    /// <summary>
    /// Starts a new year-long campaign from Spring Week 1
    /// </summary>
    public void StartNewYear()
    {
        Debug.Log("[SeasonManager] Starting new year...");

        currentSeason = Season.Spring;
        currentWeek = 1;
        weekTimer = 0f;
        isTimerRunning = true;

        // Apply initial seasonal modifiers
        ApplySeasonalModifiers();

        // Broadcast events
        OnSeasonChanged?.Invoke(currentSeason);
        OnWeekChanged?.Invoke(currentWeek);

        Debug.Log($"[SeasonManager] Year started! Current: {currentSeason}, Week {currentWeek}");
    }

    /// <summary>
    /// Advances to the next week and handles season transitions
    /// </summary>
    private void AdvanceWeek()
    {
        currentWeek++;

        Debug.Log($"[SeasonManager] Week advanced to {currentWeek}");

        // Check if year has ended (Week 22 = Winter = Game Over)
        if (currentWeek > TOTAL_WEEKS_IN_YEAR)
        {
            EndYear();
            return;
        }

        // Check for season transitions
        Season previousSeason = currentSeason;
        currentSeason = GetSeasonForWeek(currentWeek);

        // If season changed, apply new modifiers
        if (currentSeason != previousSeason)
        {
            Debug.Log($"[SeasonManager] Season changed from {previousSeason} to {currentSeason}");
            ApplySeasonalModifiers();
            OnSeasonChanged?.Invoke(currentSeason);

            // Play season transition sound
            PlaySeasonTransitionSound(currentSeason);
        }

        // Broadcast week change
        OnWeekChanged?.Invoke(currentWeek);
    }

    /// <summary>
    /// Ends the year and triggers the end-game sequence
    /// </summary>
    private void EndYear()
    {
        Debug.Log("[SeasonManager] Year has ended! Winter has arrived.");
        isTimerRunning = false;
        OnYearEnded?.Invoke();
    }

    /// <summary>
    /// Determines which season a given week belongs to
    /// </summary>
    private Season GetSeasonForWeek(int week)
    {
        if (week <= 7)
            return Season.Spring;
        else if (week <= 14)
            return Season.Summer;
        else if (week <= 21)
            return Season.Autumn;
        else
            return Season.Winter; // Should never reach here during gameplay
    }

    /// <summary>
    /// Applies seasonal modifiers to game systems
    /// </summary>
    private void ApplySeasonalModifiers()
    {
        SeasonData currentData = GetCurrentSeasonData();
        if (currentData == null)
        {
            Debug.LogWarning("[SeasonManager] No season data available for current season!");
            return;
        }

        Debug.Log($"[SeasonManager] Applying modifiers for {currentData.seasonName}: " +
                  $"Income x{currentData.incomeModifier}, " +
                  $"Bee Speed x{currentData.beeSpeedModifier}, " +
                  $"Production Time x{currentData.productionTimeModifier}");

        // Note: Actual modifier application to game systems will be handled by
        // those systems subscribing to OnSeasonChanged event and querying GetCurrentSeasonData()
    }

    /// <summary>
    /// Plays the season transition sound effect
    /// </summary>
    private void PlaySeasonTransitionSound(Season season)
    {
        SeasonData data = GetCurrentSeasonData();
        if (data != null && data.seasonStartSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(data.seasonStartSound);
        }
    }

    /// <summary>
    /// Gets the season data for the current season
    /// </summary>
    public SeasonData GetCurrentSeasonData()
    {
        switch (currentSeason)
        {
            case Season.Spring:
                return springData;
            case Season.Summer:
                return summerData;
            case Season.Autumn:
                return autumnData;
            default:
                Debug.LogWarning($"[SeasonManager] No data defined for season: {currentSeason}");
                return null;
        }
    }

    /// <summary>
    /// Pauses the season timer
    /// </summary>
    public void PauseSeasonTimer()
    {
        isTimerRunning = false;
        Debug.Log("[SeasonManager] Season timer paused");
    }

    /// <summary>
    /// Resumes the season timer
    /// </summary>
    public void ResumeSeasonTimer()
    {
        isTimerRunning = true;
        Debug.Log("[SeasonManager] Season timer resumed");
    }

    /// <summary>
    /// Gets the conversion ratio (for display/debugging)
    /// </summary>
    public float GetRealSecondsPerGameWeek()
    {
        return realSecondsPerGameWeek;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Handles debug keyboard shortcuts for testing
    /// </summary>
    private void HandleDebugInputs()
    {
        // S = Skip to next season
        if (Input.GetKeyDown(KeyCode.S))
        {
            SkipToNextSeason();
        }

        // W = Skip to next week
        if (Input.GetKeyDown(KeyCode.W))
        {
            AdvanceWeek();
        }

        // R = Restart year
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartNewYear();
        }
    }
#endif

    /// <summary>
    /// Debug function to skip to the next season immediately
    /// </summary>
    private void SkipToNextSeason()
    {
        Season nextSeason = currentSeason switch
        {
            Season.Spring => Season.Summer,
            Season.Summer => Season.Autumn,
            Season.Autumn => Season.Winter,
            _ => Season.Winter
        };

        // Calculate the first week of next season
        int targetWeek = nextSeason switch
        {
            Season.Summer => 8,
            Season.Autumn => 15,
            Season.Winter => 22,
            _ => 1
        };

        currentWeek = targetWeek;
        weekTimer = 0f;

        if (currentWeek > TOTAL_WEEKS_IN_YEAR)
        {
            EndYear();
        }
        else
        {
            currentSeason = nextSeason;
            ApplySeasonalModifiers();
            OnSeasonChanged?.Invoke(currentSeason);
            OnWeekChanged?.Invoke(currentWeek);
            Debug.Log($"[SeasonManager] DEBUG: Skipped to {currentSeason}, Week {currentWeek}");
        }
    }

    private void OnDestroy()
    {
        // Clean up singleton reference
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

/// <summary>
/// Enum representing the four seasons
/// </summary>
public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}
