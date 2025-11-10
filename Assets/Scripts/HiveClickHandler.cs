using UnityEngine;

/// <summary>
/// Handles mouse click and hover interactions with the beehive GameObject.
/// Opens the recipe display panel when clicked and provides visual hover feedback.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HiveClickHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RecipeDisplayPanel recipePanel;

    [Header("Hover Visual Settings")]
    [SerializeField] private float hoverBrightness = 1.3f;
    [SerializeField] private Color hoverEmissionColor = new Color(0.2f, 0.2f, 0.1f);

    private Renderer hiveRenderer;
    private Material originalMaterial;
    private Material hoverMaterial;
    private bool isHovering = false;

    private void Awake()
    {
        // Find the recipe panel if not assigned
        if (recipePanel == null)
        {
            recipePanel = FindFirstObjectByType<RecipeDisplayPanel>();
            if (recipePanel == null)
            {
                Debug.LogWarning("HiveClickHandler: RecipeDisplayPanel not found in scene. Click functionality will not work.");
            }
        }

        // Get renderer for hover effects
        hiveRenderer = GetComponent<Renderer>();
        if (hiveRenderer != null)
        {
            originalMaterial = hiveRenderer.material;
        }
    }

    private void OnMouseDown()
    {
        if (recipePanel != null)
        {
            recipePanel.TogglePanel();

            // Audio feedback could be added here if UI click sound is available
        }
        else
        {
            Debug.LogError("HiveClickHandler: Cannot open recipe panel - RecipeDisplayPanel reference is null.");
        }
    }

    private void OnMouseEnter()
    {
        if (hiveRenderer != null && originalMaterial != null)
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

            hiveRenderer.material = hoverMaterial;
        }
    }

    private void OnMouseExit()
    {
        if (hiveRenderer != null && originalMaterial != null && isHovering)
        {
            isHovering = false;
            hiveRenderer.material = originalMaterial;

            // Clean up hover material to prevent memory leak
            if (hoverMaterial != null)
            {
                Destroy(hoverMaterial);
                hoverMaterial = null;
            }
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
