using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized audio manager that handles music, ambient sounds, and SFX.
/// Subscribes to gameplay events to trigger audio feedback.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Music & Ambient")]
    [SerializeField] private AudioClip musicTrack;
    [SerializeField] private AudioClip ambientTrack;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip recipeCompletedClip;
    [SerializeField] private AudioClip flowerPatchUnlockedClip;
    [SerializeField] private AudioClip flowerPatchUpgradedClip;
    [SerializeField] private AudioClip capacityUpgradedClip;
    [SerializeField] private AudioClip pollenDiscardedClip;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;

    [Header("SFX Pool Settings")]
    [SerializeField] private int sfxPoolSize = 10;

    private Queue<AudioSource> sfxPool;
    private List<FlowerPatchController> subscribedFlowerPatches = new List<FlowerPatchController>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.parent = null; // Detach from parent to make root GameObject
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        InitializeSFXPool();
    }

    private void Start()
    {
        SubscribeToGameEvents();
        SubscribeToExistingFlowerPatches();
        StartMusicAndAmbient();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameEvents();
    }

    /// <summary>
    /// Initialize music and ambient AudioSource components if not assigned
    /// </summary>
    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        ambientSource.volume = ambientVolume;
    }

    /// <summary>
    /// Create a pool of AudioSource components for SFX playback
    /// </summary>
    private void InitializeSFXPool()
    {
        sfxPool = new Queue<AudioSource>();

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            sfxPool.Enqueue(source);
        }
    }

    /// <summary>
    /// Start playing music and ambient tracks
    /// </summary>
    private void StartMusicAndAmbient()
    {
        if (musicTrack != null && !musicSource.isPlaying)
        {
            musicSource.clip = musicTrack;
            musicSource.Play();
        }

        if (ambientTrack != null && !ambientSource.isPlaying)
        {
            ambientSource.clip = ambientTrack;
            ambientSource.Play();
        }
    }

    /// <summary>
    /// Subscribe to all relevant gameplay events (singleton managers only)
    /// </summary>
    private void SubscribeToGameEvents()
    {
        if (RecipeProductionManager.Instance != null)
        {
            RecipeProductionManager.Instance.OnRecipeCompleted.AddListener(OnRecipeCompleted);
        }

        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnPollenDiscarded.AddListener(OnPollenDiscarded);
        }
    }

    /// <summary>
    /// Subscribe to all existing flower patches in the scene
    /// </summary>
    private void SubscribeToExistingFlowerPatches()
    {
        FlowerPatchController[] flowerPatches = FindObjectsByType<FlowerPatchController>(FindObjectsSortMode.None);
        foreach (FlowerPatchController flowerPatch in flowerPatches)
        {
            RegisterFlowerPatch(flowerPatch);
        }
    }

    /// <summary>
    /// Unsubscribe from all gameplay events
    /// </summary>
    private void UnsubscribeFromGameEvents()
    {
        if (RecipeProductionManager.Instance != null)
        {
            RecipeProductionManager.Instance.OnRecipeCompleted.RemoveListener(OnRecipeCompleted);
        }

        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnPollenDiscarded.RemoveListener(OnPollenDiscarded);
        }

        // Unsubscribe from all flower patches
        foreach (FlowerPatchController flowerPatch in subscribedFlowerPatches)
        {
            if (flowerPatch != null)
            {
                flowerPatch.OnCapacityUpgraded.RemoveListener(OnCapacityUpgraded);
            }
        }
        subscribedFlowerPatches.Clear();
    }

    #region Event Handlers

    private void OnRecipeCompleted(HoneyRecipe recipe, float honeyValue)
    {
        if (recipeCompletedClip != null)
        {
            PlaySFX(recipeCompletedClip);
        }
    }

    private void OnCapacityUpgraded()
    {
        if (capacityUpgradedClip != null)
        {
            PlaySFX(capacityUpgradedClip);
        }
    }

    private void OnPollenDiscarded(ResourceType resourceType, int amount)
    {
        if (pollenDiscardedClip != null)
        {
            PlaySFX(pollenDiscardedClip, 0.6f); // Quieter warning sound
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Register a flower patch to receive audio event callbacks.
    /// Call this when a new flower patch is created.
    /// </summary>
    /// <param name="flowerPatch">The flower patch controller to subscribe to</param>
    public void RegisterFlowerPatch(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null || subscribedFlowerPatches.Contains(flowerPatch))
            return;

        flowerPatch.OnCapacityUpgraded.AddListener(OnCapacityUpgraded);
        subscribedFlowerPatches.Add(flowerPatch);
    }

    /// <summary>
    /// Unregister a flower patch from audio events.
    /// Call this when a flower patch is destroyed.
    /// </summary>
    /// <param name="flowerPatch">The flower patch controller to unsubscribe from</param>
    public void UnregisterFlowerPatch(FlowerPatchController flowerPatch)
    {
        if (flowerPatch == null || !subscribedFlowerPatches.Contains(flowerPatch))
            return;

        flowerPatch.OnCapacityUpgraded.RemoveListener(OnCapacityUpgraded);
        subscribedFlowerPatches.Remove(flowerPatch);
    }

    /// <summary>
    /// Play the flower patch unlock sound effect
    /// </summary>
    public void PlayFlowerPatchUnlockSound()
    {
        if (flowerPatchUnlockedClip != null)
        {
            PlaySFX(flowerPatchUnlockedClip);
        }
    }

    /// <summary>
    /// Play a sound effect using the pooled AudioSource system
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volumeMultiplier">Optional volume multiplier (default 1.0)</param>
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        if (source != null)
        {
            source.volume = sfxVolume * volumeMultiplier;
            source.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Set the music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    /// <summary>
    /// Set the ambient sound volume
    /// </summary>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
        {
            ambientSource.volume = ambientVolume;
        }
    }

    /// <summary>
    /// Set the SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Toggle music on/off
    /// </summary>
    public void ToggleMusic(bool enabled)
    {
        if (musicSource != null)
        {
            if (enabled && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
            else if (!enabled && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
    }

    /// <summary>
    /// Toggle ambient sound on/off
    /// </summary>
    public void ToggleAmbient(bool enabled)
    {
        if (ambientSource != null)
        {
            if (enabled && !ambientSource.isPlaying)
            {
                ambientSource.Play();
            }
            else if (!enabled && ambientSource.isPlaying)
            {
                ambientSource.Pause();
            }
        }
    }

    #endregion

    /// <summary>
    /// Get an available AudioSource from the pool
    /// </summary>
    private AudioSource GetAvailableSFXSource()
    {
        // Find first non-playing source
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If all sources are busy, dequeue and re-enqueue (oldest sound gets interrupted)
        AudioSource oldestSource = sfxPool.Dequeue();
        sfxPool.Enqueue(oldestSource);
        return oldestSource;
    }
}
