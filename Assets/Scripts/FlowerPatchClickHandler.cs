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

    [Header("Hover Visual Settings")]
    [SerializeField] private float hoverBrightness = 1.3f;
    [SerializeField] private Color hoverEmissionColor = new Color(0.2f, 0.2f, 0.1f);

    // Internal state
    [SerializeField] private Renderer flowerPatchRenderer;
    private Material originalMaterial;
    private Material hoverMaterial;
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

        if (flowerPatchRenderer != null && originalMaterial != null)
        {
            isHovering = true;

            // Create hover material with brighter appearance and emission
            hoverMaterial = new Material(originalMaterial);
            hoverMaterial.color = originalMaterial.color * hoverBrightness;

            // Add emission glow if supported
            if (hoverMaterial.HasProperty("_EmissionColor"))
            {
                hoverMaterial.EnableKeyword("_EMISSION");
                hoverMaterial.SetColor("_EmissionColor", hoverEmissionColor);
            }

            Debug.Log($"[FlowerPatchClickHandler] {gameObject.name}: Applying hover material", this);
            flowerPatchRenderer.material = hoverMaterial;
        }
        else
        {
            Debug.LogWarning($"[FlowerPatchClickHandler] {gameObject.name}: Cannot apply hover - renderer={flowerPatchRenderer != null}, originalMaterial={originalMaterial != null}", this);
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

            // Clean up hover material to prevent memory leak
            if (hoverMaterial != null)
            {
                Destroy(hoverMaterial);
                hoverMaterial = null;
            }
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

    private void OnDestroy()
    {
        // Clean up any runtime-created materials
        if (hoverMaterial != null)
        {
            Destroy(hoverMaterial);
            hoverMaterial = null;
        }
    }
}
