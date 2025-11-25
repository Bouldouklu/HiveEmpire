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

    private Renderer hiveRenderer;
    private Material originalMaterial;
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
        if (hiveRenderer == null)
        {
            hiveRenderer = GetComponentInChildren<Renderer>();
            if (hiveRenderer != null)
            {
                Debug.Log($"[HiveClickHandler] {gameObject.name}: Renderer found in children", this);
            }
        }
        else
        {
            Debug.Log($"[HiveClickHandler] {gameObject.name}: Renderer found on root", this);
        }

        if (hiveRenderer == null)
        {
            Debug.LogError($"[HiveClickHandler] {gameObject.name}: NO RENDERER FOUND - hover effect will not work!", this);
        }
        else
        {
            originalMaterial = hiveRenderer.sharedMaterial;
            Debug.Log($"[HiveClickHandler] {gameObject.name}: Original material captured: {originalMaterial?.name}", this);
        }
    }

    private void OnMouseDown()
    {
        // Prevent interaction when clicking on UI
        if (UIBlocker.IsPointerOverUI())
        {
            return;
        }

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
        Debug.Log($"[HiveClickHandler] {gameObject.name}: OnMouseEnter called", this);

        if (hiveRenderer != null && originalMaterial != null)
        {
            isHovering = true;

            // Get pre-made hover material from FlowerPatchMaterialMapper
            if (FlowerPatchMaterialMapper.Instance != null)
            {
                Material hoverMaterial = FlowerPatchMaterialMapper.Instance.GetHiveHoverMaterial();

                if (hoverMaterial != null)
                {
                    Debug.Log($"[HiveClickHandler] {gameObject.name}: Applying hover material", this);
                    hiveRenderer.material = hoverMaterial;
                }
                else
                {
                    Debug.LogWarning($"[HiveClickHandler] {gameObject.name}: No hive hover material found", this);
                }
            }
            else
            {
                Debug.LogWarning($"[HiveClickHandler] {gameObject.name}: FlowerPatchMaterialMapper not available", this);
            }
        }
        else
        {
            Debug.LogWarning($"[HiveClickHandler] {gameObject.name}: Cannot apply hover - renderer={hiveRenderer != null}, originalMaterial={originalMaterial != null}", this);
        }
    }

    private void OnMouseExit()
    {
        Debug.Log($"[HiveClickHandler] {gameObject.name}: OnMouseExit called", this);

        if (hiveRenderer != null && originalMaterial != null && isHovering)
        {
            isHovering = false;
            Debug.Log($"[HiveClickHandler] {gameObject.name}: Restoring original material: {originalMaterial.name}", this);
            hiveRenderer.material = originalMaterial;
            // No need to destroy hover material - it's a shared asset from FlowerPatchMaterialMapper
        }
        else
        {
            Debug.LogWarning($"[HiveClickHandler] {gameObject.name}: Cannot restore - renderer={hiveRenderer != null}, originalMaterial={originalMaterial != null}, isHovering={isHovering}", this);
        }
    }

    // No OnDestroy needed - we now use shared materials from FlowerPatchMaterialMapper
}

