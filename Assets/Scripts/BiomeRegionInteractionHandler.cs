using UnityEngine;

/// <summary>
/// Handles mouse hover and click interactions for BiomeRegions.
/// Highlights entire region on hover and opens appropriate UI panel on click.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BiomeRegionInteractionHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The biome region this handler controls")]
    [SerializeField] private BiomeRegion biomeRegion;

    [Header("UI References")]
    [Tooltip("Panel shown when region is locked")]
    [SerializeField] private FlowerPatchUnlockPanel unlockPanel;

    [Tooltip("Panel shown when region is unlocked")]
    [SerializeField] private FlowerPatchUpgradePanel upgradePanel;

    private void Awake()
    {
        // Auto-find biome region if not assigned
        if (biomeRegion == null)
        {
            biomeRegion = GetComponent<BiomeRegion>();
            if (biomeRegion == null)
            {
                biomeRegion = GetComponentInParent<BiomeRegion>();
            }
        }

        if (biomeRegion == null)
        {
            Debug.LogError($"BiomeRegionInteractionHandler on '{name}' has no BiomeRegion reference!", this);
        }

        // Find UI panels if not assigned
        if (unlockPanel == null)
        {
            unlockPanel = FindFirstObjectByType<FlowerPatchUnlockPanel>();
        }

        if (upgradePanel == null)
        {
            upgradePanel = FindFirstObjectByType<FlowerPatchUpgradePanel>();
        }

        // Ensure collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"BiomeRegionInteractionHandler on '{name}' has no Collider! Adding BoxCollider.", this);
            gameObject.AddComponent<BoxCollider>();
        }
    }

    private void OnMouseEnter()
    {
        if (biomeRegion == null) return;

        // Apply hover material to entire region
        biomeRegion.ApplyHoverMaterial();
    }

    private void OnMouseExit()
    {
        if (biomeRegion == null) return;

        // Restore original material
        biomeRegion.RestoreOriginalMaterial();
    }

    private void OnMouseDown()
    {
        if (biomeRegion == null)
        {
            Debug.LogWarning("Cannot interact with BiomeRegion - no reference!");
            return;
        }

        // Check if region is locked
        if (biomeRegion.IsLocked)
        {
            // Show unlock panel
            if (unlockPanel != null)
            {
                unlockPanel.ShowPanelForRegion(biomeRegion);
            }
            else
            {
                Debug.LogWarning($"Cannot show unlock panel for {biomeRegion.name} - panel not found!");
            }
        }
        else
        {
            // Show upgrade panel
            if (upgradePanel != null)
            {
                upgradePanel.ShowPanelForRegion(biomeRegion);
            }
            else
            {
                Debug.LogWarning($"Cannot show upgrade panel for {biomeRegion.name} - panel not found!");
            }
        }
    }

    /// <summary>
    /// Sets the biome region reference (used during runtime setup)
    /// </summary>
    public void SetBiomeRegion(BiomeRegion region)
    {
        biomeRegion = region;
    }

    private void OnValidate()
    {
        // Auto-find biome region in editor
        if (biomeRegion == null)
        {
            biomeRegion = GetComponent<BiomeRegion>();
            if (biomeRegion == null)
            {
                biomeRegion = GetComponentInParent<BiomeRegion>();
            }
        }
    }
}
