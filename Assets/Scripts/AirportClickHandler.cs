using UnityEngine;

/// <summary>
/// Handles mouse click detection on airport GameObjects to open the upgrade panel.
/// Requires a Collider component on the GameObject to detect clicks.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AirportClickHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the AirportController (automatically set if on same GameObject)")]
    [SerializeField] private AirportController airportController;

    [Tooltip("Reference to the upgrade panel UI")]
    [SerializeField] private AirportUpgradePanel upgradePanel;

    [Header("Visual Feedback")]
    [Tooltip("Material to use when hovering over airport (optional - will create default if not assigned)")]
    [SerializeField] private Material hoverMaterial;

    [Tooltip("Renderer component for visual feedback")]
    [SerializeField] private Renderer airportRenderer;

    [Tooltip("Color to brighten material on hover (if hoverMaterial not assigned)")]
    [SerializeField] private Color hoverTint = new Color(1.2f, 1.2f, 1.2f, 1f);

    // Internal state
    private Material originalMaterial;
    private Material runtimeHoverMaterial;
    private bool isHovering = false;

    private void Awake()
    {
        // Get AirportController reference if not set
        if (airportController == null)
        {
            airportController = GetComponent<AirportController>();
            if (airportController == null)
            {
                Debug.LogError($"AirportClickHandler on {gameObject.name}: No AirportController found!", this);
            }
        }

        // Get Renderer for visual feedback
        if (airportRenderer == null)
        {
            airportRenderer = GetComponent<Renderer>();
        }

        // Store original material and create hover material if needed
        if (airportRenderer != null)
        {
            originalMaterial = airportRenderer.material;

            // Create runtime hover material if no hover material assigned
            if (hoverMaterial == null)
            {
                runtimeHoverMaterial = new Material(originalMaterial);

                // Brighten the material for hover effect
                if (runtimeHoverMaterial.HasProperty("_Color"))
                {
                    Color originalColor = runtimeHoverMaterial.color;
                    runtimeHoverMaterial.color = new Color(
                        originalColor.r * hoverTint.r,
                        originalColor.g * hoverTint.g,
                        originalColor.b * hoverTint.b,
                        originalColor.a
                    );
                }

                // Add emission for glow effect
                if (runtimeHoverMaterial.HasProperty("_EmissionColor"))
                {
                    runtimeHoverMaterial.EnableKeyword("_EMISSION");
                    runtimeHoverMaterial.SetColor("_EmissionColor", Color.white * 0.2f);
                }
            }
        }

        // Find upgrade panel in scene if not set
        if (upgradePanel == null)
        {
            upgradePanel = FindFirstObjectByType<AirportUpgradePanel>();
            if (upgradePanel == null)
            {
                Debug.LogWarning($"AirportClickHandler on {gameObject.name}: No AirportUpgradePanel found in scene. Create one in the UI Canvas.", this);
            }
        }
    }

    private void OnMouseEnter()
    {
        // Visual feedback on hover
        isHovering = true;
        if (airportRenderer != null)
        {
            // Use assigned hover material if available, otherwise use runtime-generated one
            Material materialToUse = hoverMaterial != null ? hoverMaterial : runtimeHoverMaterial;

            if (materialToUse != null)
            {
                airportRenderer.material = materialToUse;
            }
        }

        // Change cursor (optional)
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        // Restore original material
        isHovering = false;
        if (airportRenderer != null && originalMaterial != null)
        {
            airportRenderer.material = originalMaterial;
        }
    }

    private void OnMouseDown()
    {
        // Open upgrade panel when clicked
        if (airportController != null && upgradePanel != null)
        {
            Debug.Log($"Airport {gameObject.name} clicked - opening upgrade panel");
            upgradePanel.ShowPanel(airportController);
        }
        else
        {
            Debug.LogWarning($"Cannot open upgrade panel for {gameObject.name}: Missing references");
        }
    }

    private void OnDestroy()
    {
        // Clean up material instances to prevent memory leaks
        if (airportRenderer != null && airportRenderer.material != originalMaterial)
        {
            Destroy(airportRenderer.material);
        }

        // Clean up runtime hover material
        if (runtimeHoverMaterial != null)
        {
            Destroy(runtimeHoverMaterial);
        }
    }
}
