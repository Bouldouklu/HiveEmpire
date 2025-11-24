using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages a multi-hex biome region with unified unlock/upgrade mechanics.
/// Handles region-wide capacity, bee distribution, and visual state.
/// </summary>
public class BiomeRegion : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Configuration data for this biome region")]
    [SerializeField] private BiomeRegionData regionData;

    [Header("Hex Tiles")]
    [Tooltip("All hex tiles that make up this region")]
    [SerializeField] private List<HexTile> hexTiles = new List<HexTile>();

    [Header("State")]
    [Tooltip("Is this region locked (not yet purchased)?")]
    [SerializeField] private bool isLocked = true;

    [Tooltip("Current capacity upgrade tier (0 = base, 1+ = upgraded)")]
    [SerializeField] private int capacityTier = 0;

    [Tooltip("Bonus capacity from upgrades")]
    [SerializeField] private int capacityBonus = 0;

    [Header("Events")]
    [Tooltip("Fired when region is unlocked")]
    public UnityEvent OnRegionUnlocked = new UnityEvent();

    [Tooltip("Fired when capacity is upgraded")]
    public UnityEvent OnCapacityUpgraded = new UnityEvent();

    // Bee distribution tracking
    private int nextSpawnTileIndex = 0;
    private Material originalMaterial;
    private Material lockedMaterial;

    /// <summary>
    /// Gets the biome type of this region
    /// </summary>
    public BiomeType BiomeType => regionData != null ? regionData.biomeType : BiomeType.WildMeadow;

    /// <summary>
    /// Gets whether this region is currently locked
    /// </summary>
    public bool IsLocked => isLocked;

    /// <summary>
    /// Gets the maximum bee capacity for this region
    /// </summary>
    public int MaxBeeCapacity => regionData.BaseCapacity + capacityBonus;

    /// <summary>
    /// Gets the current capacity tier
    /// </summary>
    public int CapacityTier => capacityTier;

    /// <summary>
    /// Gets the number of hex tiles in this region
    /// </summary>
    public int HexTileCount => hexTiles.Count;

    /// <summary>
    /// Gets the region configuration data
    /// </summary>
    public BiomeRegionData RegionData => regionData;

    /// <summary>
    /// Gets all hex tiles in this region
    /// </summary>
    public IReadOnlyList<HexTile> HexTiles => hexTiles.AsReadOnly();

    private void Awake()
    {
        // Auto-discover hex tiles if not assigned
        if (hexTiles == null || hexTiles.Count == 0)
        {
            hexTiles = new List<HexTile>(GetComponentsInChildren<HexTile>());
        }

        // Ensure all tiles reference this region
        foreach (var tile in hexTiles)
        {
            if (tile != null)
            {
                tile.SetParentRegion(this);
            }
        }

        // Cache materials
        if (regionData != null && FlowerPatchMaterialMapper.Instance != null)
        {
            originalMaterial = FlowerPatchMaterialMapper.Instance.GetFlowerPatchMaterial(regionData.biomeType);
            lockedMaterial = FlowerPatchMaterialMapper.Instance.GetLockedMaterial(regionData.biomeType);
        }

        // Apply initial visual state
        UpdateVisualState();
    }

    /// <summary>
    /// Initializes the region from configuration data
    /// </summary>
    public void InitializeFromData(BiomeRegionData data)
    {
        if (data == null)
        {
            Debug.LogError($"Cannot initialize BiomeRegion '{name}' with null data!", this);
            return;
        }

        regionData = data;
        capacityBonus = 0;
        capacityTier = 0;
        isLocked = true;

        // Cache materials
        if (FlowerPatchMaterialMapper.Instance != null)
        {
            originalMaterial = FlowerPatchMaterialMapper.Instance.GetFlowerPatchMaterial(data.biomeType);
            lockedMaterial = FlowerPatchMaterialMapper.Instance.GetLockedMaterial(data.biomeType);
        }

        UpdateVisualState();

        Debug.Log($"BiomeRegion '{name}' initialized: Biome={data.biomeType}, HexTiles={hexTiles.Count}, Capacity={MaxBeeCapacity}");
    }

    /// <summary>
    /// Gets the cost to unlock this region
    /// </summary>
    public float GetUnlockCost()
    {
        return regionData != null ? regionData.UnlockCost : 0f;
    }

    /// <summary>
    /// Attempts to unlock this region. Returns true if successful.
    /// </summary>
    public bool UnlockRegion()
    {
        if (!isLocked)
        {
            Debug.LogWarning($"BiomeRegion '{name}' is already unlocked!");
            return false;
        }

        if (regionData == null)
        {
            Debug.LogError($"Cannot unlock BiomeRegion '{name}' without configuration data!", this);
            return false;
        }

        float unlockCost = regionData.UnlockCost;

        // Check affordability
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(unlockCost))
        {
            Debug.Log($"Cannot afford to unlock {name}. Cost: ${unlockCost}, Available: ${EconomyManager.Instance?.CurrentMoney ?? 0}");
            return false;
        }

        // Deduct cost
        EconomyManager.Instance.SpendMoney(unlockCost);

        // Unlock region
        isLocked = false;
        UpdateVisualState();

        // Fire event
        OnRegionUnlocked?.Invoke();

        // Play sound if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFlowerPatchUnlockSound();
        }

        Debug.Log($"BiomeRegion '{name}' unlocked! Cost: ${unlockCost}");
        return true;
    }

    /// <summary>
    /// Gets the cost for the next capacity upgrade
    /// </summary>
    public float GetCapacityUpgradeCost()
    {
        if (regionData == null) return 0f;
        return regionData.GetCapacityUpgradeCost(capacityTier);
    }

    /// <summary>
    /// Checks if capacity can be upgraded
    /// </summary>
    public bool CanUpgradeCapacity()
    {
        if (regionData == null) return false;
        return regionData.CanUpgradeCapacity(capacityTier);
    }

    /// <summary>
    /// Gets the capacity after the next upgrade
    /// </summary>
    public int GetNextCapacity()
    {
        if (regionData == null) return MaxBeeCapacity;
        return MaxBeeCapacity + regionData.BonusCapacityPerUpgrade;
    }

    /// <summary>
    /// Attempts to upgrade capacity. Returns true if successful.
    /// </summary>
    public bool UpgradeCapacity()
    {
        if (!CanUpgradeCapacity())
        {
            Debug.LogWarning($"Cannot upgrade capacity for {name}. Already at max tier or no data.");
            return false;
        }

        float upgradeCost = GetCapacityUpgradeCost();

        // Check affordability
        if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(upgradeCost))
        {
            Debug.Log($"Cannot afford capacity upgrade for {name}. Cost: ${upgradeCost}");
            return false;
        }

        // Deduct cost
        EconomyManager.Instance.SpendMoney(upgradeCost);

        // Apply upgrade
        capacityTier++;
        capacityBonus += regionData.BonusCapacityPerUpgrade;

        // Fire event
        OnCapacityUpgraded?.Invoke();

        Debug.Log($"BiomeRegion '{name}' capacity upgraded to tier {capacityTier}. New capacity: {MaxBeeCapacity}");
        return true;
    }

    /// <summary>
    /// Gets the next hex tile to spawn a bee at (distributed across region)
    /// </summary>
    public HexTile GetNextBeeSpawnTile()
    {
        if (hexTiles == null || hexTiles.Count == 0)
        {
            Debug.LogWarning($"BiomeRegion '{name}' has no hex tiles for bee spawning!");
            return null;
        }

        // Round-robin distribution across tiles
        HexTile tile = hexTiles[nextSpawnTileIndex];
        nextSpawnTileIndex = (nextSpawnTileIndex + 1) % hexTiles.Count;
        return tile;
    }

    /// <summary>
    /// Gets a random hex tile for visual variety (alternative to round-robin)
    /// </summary>
    public HexTile GetRandomBeeSpawnTile()
    {
        if (hexTiles == null || hexTiles.Count == 0)
        {
            Debug.LogWarning($"BiomeRegion '{name}' has no hex tiles for bee spawning!");
            return null;
        }

        int randomIndex = Random.Range(0, hexTiles.Count);
        return hexTiles[randomIndex];
    }

    /// <summary>
    /// Gets the center position of the region (average of all tile positions)
    /// </summary>
    public Vector3 GetRegionCenter()
    {
        if (hexTiles == null || hexTiles.Count == 0)
        {
            return transform.position;
        }

        Vector3 sum = Vector3.zero;
        foreach (var tile in hexTiles)
        {
            if (tile != null)
            {
                sum += tile.transform.position;
            }
        }

        return sum / hexTiles.Count;
    }

    /// <summary>
    /// Applies a material to all hex tiles in the region
    /// </summary>
    public void ApplyMaterialToAllTiles(Material material)
    {
        if (material == null)
        {
            Debug.LogWarning($"Cannot apply null material to BiomeRegion '{name}'");
            return;
        }

        foreach (var tile in hexTiles)
        {
            if (tile != null)
            {
                tile.ApplyMaterial(material);
            }
        }
    }

    /// <summary>
    /// Updates the visual state of all tiles based on lock status
    /// </summary>
    public void UpdateVisualState()
    {
        if (isLocked && lockedMaterial != null)
        {
            ApplyMaterialToAllTiles(lockedMaterial);
        }
        else if (!isLocked && originalMaterial != null)
        {
            ApplyMaterialToAllTiles(originalMaterial);
        }
    }

    /// <summary>
    /// Applies hover material to all tiles in the region
    /// </summary>
    public void ApplyHoverMaterial()
    {
        if (FlowerPatchMaterialMapper.Instance == null || regionData == null) return;

        Material hoverMaterial = FlowerPatchMaterialMapper.Instance.GetHoverMaterial(regionData.biomeType);
        if (hoverMaterial != null)
        {
            ApplyMaterialToAllTiles(hoverMaterial);
        }
    }

    /// <summary>
    /// Restores original material to all tiles
    /// </summary>
    public void RestoreOriginalMaterial()
    {
        UpdateVisualState();
    }

    private void OnValidate()
    {
        // Auto-discover hex tiles in editor
        if (hexTiles == null || hexTiles.Count == 0)
        {
            hexTiles = new List<HexTile>(GetComponentsInChildren<HexTile>());
        }

        // Validate capacity tier
        if (capacityTier < 0)
        {
            capacityTier = 0;
        }

        if (capacityBonus < 0)
        {
            capacityBonus = 0;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw region bounds in editor
        if (hexTiles != null && hexTiles.Count > 0)
        {
            Gizmos.color = isLocked ? Color.red : Color.green;
            Vector3 center = GetRegionCenter();
            Gizmos.DrawWireSphere(center, 1f);

            // Draw lines connecting tiles
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            foreach (var tile in hexTiles)
            {
                if (tile != null)
                {
                    Gizmos.DrawLine(center, tile.transform.position);
                }
            }
        }
    }
}
