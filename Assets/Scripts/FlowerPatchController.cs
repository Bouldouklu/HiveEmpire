using UnityEngine;

/// <summary>
/// Minimal data holder for pollen properties in the region-based system.
/// Stores FlowerPatchData reference for pollen type, icon, and material.
/// All unlock/capacity logic is handled by BiomeRegion component.
/// This component is attached to BiomeRegion GameObjects.
/// </summary>
public class FlowerPatchController : MonoBehaviour
{
    [Header("Pollen Data")]
    [Tooltip("FlowerPatchData defining pollen type and properties for this region")]
    [SerializeField] private FlowerPatchData flowerPatchData;

    [Header("References")]
    [Tooltip("Parent BiomeRegion this controller belongs to")]
    [SerializeField] private BiomeRegion parentBiomeRegion;

    private void Awake()
    {
        // Auto-find parent BiomeRegion if not assigned
        if (parentBiomeRegion == null)
        {
            parentBiomeRegion = GetComponent<BiomeRegion>();
            if (parentBiomeRegion == null)
            {
                parentBiomeRegion = GetComponentInParent<BiomeRegion>();
            }
        }

        if (parentBiomeRegion == null)
        {
            Debug.LogWarning($"FlowerPatchController on '{name}' has no parent BiomeRegion. This is expected for legacy single-patch mode.", this);
        }
    }

    /// <summary>
    /// Public property to access FlowerPatchData.
    /// </summary>
    public FlowerPatchData FlowerPatchData => flowerPatchData;

    /// <summary>
    /// Gets the FlowerPatchData ScriptableObject that defines this patch's pollen properties.
    /// Called by bees when they need to pick up pollen.
    /// </summary>
    /// <returns>The FlowerPatchData defining this patch's pollen type and properties</returns>
    public FlowerPatchData GetFlowerPatchData()
    {
        return flowerPatchData;
    }

    /// <summary>
    /// Gets the parent BiomeRegion this controller belongs to
    /// </summary>
    public BiomeRegion GetBiomeRegion()
    {
        return parentBiomeRegion;
    }

    /// <summary>
    /// Gets the biome type from the FlowerPatchData
    /// </summary>
    public BiomeType GetBiomeType()
    {
        return flowerPatchData != null ? flowerPatchData.biomeType : BiomeType.WildMeadow;
    }

    /// <summary>
    /// Initializes this controller from FlowerPatchData ScriptableObject.
    /// Called during region setup.
    /// </summary>
    /// <param name="data">The FlowerPatchData containing pollen configuration</param>
    public void InitializeFromData(FlowerPatchData data)
    {
        if (data == null)
        {
            Debug.LogError($"FlowerPatchController on {gameObject.name}: Cannot initialize from null FlowerPatchData!", this);
            return;
        }

        flowerPatchData = data;
        Debug.Log($"FlowerPatchController initialized with {data.name}: Biome={data.biomeType}, Pollen={data.pollenDisplayName}");
    }

    /// <summary>
    /// Sets the parent BiomeRegion reference
    /// </summary>
    public void SetBiomeRegion(BiomeRegion region)
    {
        parentBiomeRegion = region;
    }

    // ============================================
    // LEGACY COMPATIBILITY METHODS
    // These methods provide compatibility with existing systems
    // that expect the old FlowerPatchController interface
    // ============================================

    /// <summary>
    /// [LEGACY] Gets whether this flower patch is locked.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public bool IsLocked
    {
        get
        {
            if (parentBiomeRegion != null)
            {
                return parentBiomeRegion.IsLocked;
            }
            return false; // Default to unlocked if no region
        }
    }

    /// <summary>
    /// [LEGACY] Gets the maximum bee capacity.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public int MaxBeeCapacity
    {
        get
        {
            if (parentBiomeRegion != null)
            {
                return parentBiomeRegion.MaxBeeCapacity;
            }
            return 0; // Default to 0 if no region
        }
    }

    /// <summary>
    /// [LEGACY] Gets the unlock cost.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public float GetUnlockCost()
    {
        if (parentBiomeRegion != null)
        {
            return parentBiomeRegion.GetUnlockCost();
        }
        return flowerPatchData != null ? flowerPatchData.placementCost : 0f;
    }

    /// <summary>
    /// [LEGACY] Checks if capacity can be upgraded.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public bool CanUpgradeCapacity()
    {
        if (parentBiomeRegion != null)
        {
            return parentBiomeRegion.CanUpgradeCapacity();
        }
        return false;
    }

    /// <summary>
    /// [LEGACY] Gets the capacity upgrade cost.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public float GetCapacityUpgradeCost()
    {
        if (parentBiomeRegion != null)
        {
            return parentBiomeRegion.GetCapacityUpgradeCost();
        }
        return 0f;
    }

    /// <summary>
    /// [LEGACY] Gets the next capacity after upgrade.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public int GetNextCapacity()
    {
        if (parentBiomeRegion != null)
        {
            return parentBiomeRegion.GetNextCapacity();
        }
        return 0;
    }

    /// <summary>
    /// [LEGACY] Gets the current capacity tier.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public int GetCapacityTier()
    {
        if (parentBiomeRegion != null)
        {
            return parentBiomeRegion.CapacityTier;
        }
        return 0;
    }

    /// <summary>
    /// [LEGACY] Gets the maximum capacity tier.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public int GetMaxCapacityTier()
    {
        if (parentBiomeRegion != null && parentBiomeRegion.RegionData != null)
        {
            return parentBiomeRegion.RegionData.MaxCapacityTier;
        }
        return 0;
    }

    /// <summary>
    /// [LEGACY] Attempts to unlock this flower patch.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public bool UnlockPatch()
    {
        if (parentBiomeRegion != null)
        {
            return parentBiomeRegion.UnlockRegion();
        }
        Debug.LogWarning($"FlowerPatchController on '{name}': Cannot unlock without parent BiomeRegion");
        return false;
    }

    /// <summary>
    /// [LEGACY] Attempts to upgrade capacity.
    /// Delegates to parent BiomeRegion if available.
    /// </summary>
    public bool UpgradeCapacity()
    {
        if (parentBiomeRegion != null)
        {
            return parentBiomeRegion.UpgradeCapacity();
        }
        Debug.LogWarning($"FlowerPatchController on '{name}': Cannot upgrade capacity without parent BiomeRegion");
        return false;
    }

    /// <summary>
    /// [LEGACY] UnityEvents for backward compatibility with UI panels.
    /// These delegate to the parent BiomeRegion's events.
    /// </summary>
    public UnityEngine.Events.UnityEvent OnCapacityUpgraded
    {
        get { return parentBiomeRegion != null ? parentBiomeRegion.OnCapacityUpgraded : new UnityEngine.Events.UnityEvent(); }
    }

    public UnityEngine.Events.UnityEvent OnUnlocked
    {
        get { return parentBiomeRegion != null ? parentBiomeRegion.OnRegionUnlocked : new UnityEngine.Events.UnityEvent(); }
    }

    private void OnValidate()
    {
        // Auto-find parent BiomeRegion in editor
        if (parentBiomeRegion == null)
        {
            parentBiomeRegion = GetComponent<BiomeRegion>();
            if (parentBiomeRegion == null)
            {
                parentBiomeRegion = GetComponentInParent<BiomeRegion>();
            }
        }
    }
}
