using UnityEngine;

/// <summary>
/// Handles mouse click detection on flower patch GameObjects to open the upgrade panel.
/// Requires a Collider component on the GameObject to detect clicks.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FlowerPatchClickHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the FlowerPatchController (automatically set if on same GameObject)")]
    [SerializeField] private FlowerPatchController flowerPatchController;

    [Tooltip("Reference to the upgrade panel UI")]
    [SerializeField] private FlowerPatchUpgradePanel upgradePanel;

    // Internal state
    [SerializeField] private Renderer flowerPatchRenderer;
    private Material originalMaterial;
    private bool isHovering = false;

    private void Awake()
    {
        // Get FlowerPatchController reference if not set
        if (flowerPatchController == null)
        {
            flowerPatchController = GetComponent<FlowerPatchController>();
            if (flowerPatchController == null)
            {
                Debug.LogError($"FlowerPatchClickHandler on {gameObject.name}: No FlowerPatchController found!", this);
            }
        }

        // Get Renderer for visual feedback
        if (flowerPatchRenderer == null)
        {
            flowerPatchRenderer = GetComponent<Renderer>();
            if (flowerPatchRenderer == null)
            {
                flowerPatchRenderer = GetComponentInChildren<Renderer>();
                Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: Renderer found in children", this);
            }
            else
            {
                Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: Renderer found on root", this);
            }
        }

        if (flowerPatchRenderer == null)
        {
            Debug.LogError($"[FlowerPatchClickHandler] {gameObject.name}: NO RENDERER FOUND - hover effect will not work!", this);
        }
        else
        {
            originalMaterial = flowerPatchRenderer.sharedMaterial;
            Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: Original material captured: {originalMaterial?.name}", this);
        }

        // Find upgrade panel in scene if not set
        if (upgradePanel == null)
        {
            upgradePanel = FindFirstObjectByType<FlowerPatchUpgradePanel>();
            if (upgradePanel == null)
            {
                Debug.LogWarning($"FlowerPatchClickHandler on {gameObject.name}: No FlowerPatchUpgradePanel found in scene. Create one in the UI Canvas.", this);
            }
        }
    }

    private void OnMouseEnter()
    {
        Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: OnMouseEnter called", this);

        if (flowerPatchRenderer != null && originalMaterial != null && flowerPatchController != null)
        {
            isHovering = true;

            // Get pre-made hover material from FlowerPatchMaterialMapper
            if (FlowerPatchMaterialMapper.Instance != null)
            {
                Material hoverMaterial = FlowerPatchMaterialMapper.Instance.GetHoverMaterial(flowerPatchController.FlowerPatchData.biomeType);

                if (hoverMaterial != null)
                {
                    Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: Applying hover material", this);
                    flowerPatchRenderer.material = hoverMaterial;
                }
                else
                {
                    Debug.LogWarning($"[FlowerPatchClickHandler] {gameObject.name}: No hover material found for biome {flowerPatchController.FlowerPatchData.biomeType}", this);
                }
            }
            else
            {
                Debug.LogWarning($"[FlowerPatchClickHandler] {gameObject.name}: FlowerPatchMaterialMapper not available", this);
            }
        }
        else
        {
            Debug.LogWarning($"[FlowerPatchClickHandler] {gameObject.name}: Cannot apply hover - renderer={flowerPatchRenderer != null}, originalMaterial={originalMaterial != null}, controller={flowerPatchController != null}", this);
        }
    }

    private void OnMouseExit()
    {
        Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: OnMouseExit called", this);

        if (flowerPatchRenderer != null && originalMaterial != null && isHovering)
        {
            isHovering = false;
            Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: Restoring original material: {originalMaterial.name}", this);
            flowerPatchRenderer.material = originalMaterial;
            // No need to destroy hover material - it's a shared asset from FlowerPatchMaterialMapper
        }
        else
        {
            Debug.LogWarning($"[FlowerPatchClickHandler] {gameObject.name}: Cannot restore - renderer={flowerPatchRenderer != null}, originalMaterial={originalMaterial != null}, isHovering={isHovering}", this);
        }
    }

    private void OnMouseDown()
    {
        // Open upgrade panel when clicked
        if (flowerPatchController != null && upgradePanel != null)
        {
            Debug.Log($"Flower patch {gameObject.name} clicked - opening upgrade panel");
            upgradePanel.ShowPanel(flowerPatchController);
        }
        else
        {
            Debug.LogWarning($"Cannot open upgrade panel for {gameObject.name}: Missing references");
        }
    }

    // No OnDestroy needed - we now use shared materials from FlowerPatchMaterialMapper
}

