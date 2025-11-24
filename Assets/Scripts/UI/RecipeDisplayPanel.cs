using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the recipe display panel UI showing all active recipes and their production status.
/// Lives on UIManagers GameObject and controls panel visibility and content.
/// </summary>
public class RecipeDisplayPanel : MonoBehaviour
{
    public static RecipeDisplayPanel Instance { get; private set; }

    [Header("Panel References")]
    [SerializeField] private GameObject panelBlocker;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private UnityEngine.UI.Button closeButton;

    [Header("Recipe List")]
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private GameObject recipeEntryPrefab;

    [Header("Update Settings")]
    [SerializeField] private float progressUpdateInterval = 0.3f;

    private List<RecipeEntryUI> activeRecipeEntries = new List<RecipeEntryUI>();
    private float timeSinceLastUpdate = 0f;
    private bool isPanelOpen = false;
    private bool isDirty = false; // Tracks if panel needs refresh when reopened

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("RecipeDisplayPanel: Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Validate references
        if (panelBlocker == null || panelRoot == null)
        {
            Debug.LogError("RecipeDisplayPanel: Panel blocker or panel root not assigned!");
        }

        if (recipeListContainer == null)
        {
            Debug.LogError("RecipeDisplayPanel: Recipe list container not assigned!");
        }

        if (recipeEntryPrefab == null)
        {
            Debug.LogError("RecipeDisplayPanel: Recipe entry prefab not assigned!");
        }

        // Subscribe to close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("RecipeDisplayPanel: Close button not assigned!");
        }

        // Subscribe to panel blocker for click-outside-to-close
        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.AddListener(HidePanel);
            }
            else
            {
                Debug.LogWarning("RecipeDisplayPanel: Panel blocker does not have a PanelBlocker component!");
            }
        }
        else
        {
            Debug.LogWarning("RecipeDisplayPanel: Panel blocker not assigned!");
        }

        // Start with panel hidden
        HidePanel();
    }

    private void OnEnable()
    {
        // Subscribe to events for instant feedback
        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
        }

        if (RecipeProductionManager.Instance != null)
        {
            RecipeProductionManager.Instance.OnRecipeCompleted.AddListener(OnRecipeCompleted);
        }

        // Subscribe to recipe progression events
        if (RecipeProgressionManager.Instance != null)
        {
            RecipeProgressionManager.Instance.OnRecipeUnlocked.AddListener(OnRecipeUnlocked);
            RecipeProgressionManager.Instance.OnRecipeUpgraded.AddListener(OnRecipeUpgraded);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (HiveController.Instance != null)
        {
            HiveController.Instance.OnResourcesChanged.RemoveListener(OnResourcesChanged);
        }

        if (RecipeProductionManager.Instance != null)
        {
            RecipeProductionManager.Instance.OnRecipeCompleted.RemoveListener(OnRecipeCompleted);
        }

        // Unsubscribe from recipe progression events
        if (RecipeProgressionManager.Instance != null)
        {
            RecipeProgressionManager.Instance.OnRecipeUnlocked.RemoveListener(OnRecipeUnlocked);
            RecipeProgressionManager.Instance.OnRecipeUpgraded.RemoveListener(OnRecipeUpgraded);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from all events to prevent memory leaks
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HidePanel);
        }

        if (panelBlocker != null)
        {
            PanelBlocker blocker = panelBlocker.GetComponent<PanelBlocker>();
            if (blocker != null)
            {
                blocker.OnClickedOutside.RemoveListener(HidePanel);
            }
        }
    }

    private void Update()
    {
        // Periodic update for smooth progress bars (hybrid strategy)
        if (isPanelOpen)
        {
            timeSinceLastUpdate += Time.deltaTime;

            if (timeSinceLastUpdate >= progressUpdateInterval)
            {
                timeSinceLastUpdate = 0f;
                UpdateRecipeProgress();
            }
        }
    }

    /// <summary>
    /// Shows the recipe display panel and populates it with active recipes.
    /// </summary>
    public void ShowPanel()
    {
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(true);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        isPanelOpen = true;

        // Always refresh when opening - this catches any changes that happened while closed
        RefreshRecipeList();
        isDirty = false;

        // Audio feedback could be added here if panel open sound is available
    }

    /// <summary>
    /// Hides the recipe display panel.
    /// </summary>
    public void HidePanel()
    {
        if (panelBlocker != null)
        {
            panelBlocker.SetActive(false);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        isPanelOpen = false;

        // Audio feedback could be added here if panel close sound is available
    }

    /// <summary>
    /// Toggles the panel open/closed state.
    /// </summary>
    public void TogglePanel()
    {
        if (isPanelOpen)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    /// <summary>
    /// Refreshes the entire recipe list showing ALL recipes (locked + unlocked).
    /// </summary>
    private void RefreshRecipeList()
    {
        if (RecipeProgressionManager.Instance == null || recipeListContainer == null)
        {
            Debug.LogWarning("RecipeDisplayPanel: RecipeProgressionManager or container not available!");
            return;
        }

        // Clear existing entries
        ClearRecipeEntries();

        // Get ALL recipes (locked + unlocked) from RecipeProgressionManager
        List<HoneyRecipe> allRecipes = RecipeProgressionManager.Instance.GetAllRecipes();

        if (allRecipes == null || allRecipes.Count == 0)
        {
            Debug.LogWarning("RecipeDisplayPanel: No recipes found in RecipeProgressionManager!");
            return;
        }

        // Create UI entry for each recipe (locked recipes will show grayed out)
        for (int i = 0; i < allRecipes.Count; i++)
        {
            HoneyRecipe recipe = allRecipes[i];
            GameObject entryObj = Instantiate(recipeEntryPrefab, recipeListContainer);
            RecipeEntryUI entryUI = entryObj.GetComponent<RecipeEntryUI>();

            if (entryUI != null)
            {
                entryUI.Initialize(recipe, i);
                activeRecipeEntries.Add(entryUI);
            }
            else
            {
                Debug.LogError("RecipeDisplayPanel: Recipe entry prefab missing RecipeEntryUI component!");
                Destroy(entryObj);
            }
        }
    }

    /// <summary>
    /// Clears all recipe entry UI elements.
    /// </summary>
    private void ClearRecipeEntries()
    {
        foreach (RecipeEntryUI entry in activeRecipeEntries)
        {
            if (entry != null)
            {
                Destroy(entry.gameObject);
            }
        }
        activeRecipeEntries.Clear();
    }

    /// <summary>
    /// Updates progress bars for all recipe entries (called periodically).
    /// </summary>
    private void UpdateRecipeProgress()
    {
        foreach (RecipeEntryUI entry in activeRecipeEntries)
        {
            if (entry != null)
            {
                entry.UpdateProgress();
            }
        }
    }

    /// <summary>
    /// Event handler: Called when hive resources change.
    /// Updates ingredient availability colors.
    /// </summary>
    private void OnResourcesChanged()
    {
        // Update ingredient displays for all entries if we have any entries
        // This ensures real-time updates regardless of panel visibility
        if (activeRecipeEntries.Count > 0)
        {
            foreach (RecipeEntryUI entry in activeRecipeEntries)
            {
                if (entry != null)
                {
                    entry.UpdateIngredients();
                }
            }
        }
    }

    /// <summary>
    /// Event handler: Called when a recipe completes production.
    /// Provides visual feedback and refreshes the list.
    /// </summary>
    private void OnRecipeCompleted(HoneyRecipe completedRecipe, float honeyValue)
    {
        if (!isPanelOpen)
        {
            return;
        }

        // Visual feedback could be added here (particle effect, flash, etc.)

        // Refresh the entire list since priorities may have changed
        RefreshRecipeList();
    }

    /// <summary>
    /// Called by RecipeEntryUI when priority changes.
    /// Refreshes the list to reflect new order.
    /// </summary>
    public void OnPriorityChanged()
    {
        RefreshRecipeList();
    }

    /// <summary>
    /// Event handler: Called when a recipe is unlocked.
    /// Updates the specific recipe entry to show unlocked state.
    /// </summary>
    private void OnRecipeUnlocked(HoneyRecipe unlockedRecipe)
    {
        Debug.Log($"RecipeDisplayPanel: Recipe unlocked - {unlockedRecipe.recipeName}.");

        if (isPanelOpen)
        {
            // Delay update by one frame to allow button animation to complete
            StartCoroutine(UpdateRecipeEntryNextFrame(unlockedRecipe));
        }
        else
        {
            // Panel is closed - mark as dirty for next open
            isDirty = true;
        }
    }

    /// <summary>
    /// Coroutine: Delays recipe entry update by one frame to prevent visual artifacts.
    /// </summary>
    private System.Collections.IEnumerator UpdateRecipeEntryNextFrame(HoneyRecipe recipe)
    {
        yield return null; // Wait one frame

        // Find the specific entry for this recipe
        RecipeEntryUI targetEntry = activeRecipeEntries.Find(e => e != null && e.Recipe == recipe);

        if (targetEntry != null)
        {
            // Re-initialize this specific entry to show unlocked state
            int index = activeRecipeEntries.IndexOf(targetEntry);
            targetEntry.Initialize(recipe, index);
            Debug.Log($"RecipeDisplayPanel: Updated entry for {recipe.recipeName} to show unlocked state.");
        }
        else
        {
            // Fallback: full refresh if entry not found (shouldn't happen normally)
            Debug.LogWarning($"RecipeDisplayPanel: Could not find entry for {recipe.recipeName}, performing full refresh.");
            RefreshRecipeList();
        }
    }

    /// <summary>
    /// Event handler: Called when a recipe is upgraded.
    /// Updates the specific recipe entry to show new tier and stats.
    /// </summary>
    private void OnRecipeUpgraded(HoneyRecipe upgradedRecipe, int newTier)
    {
        Debug.Log($"RecipeDisplayPanel: Recipe upgraded - {upgradedRecipe.recipeName} to Tier {newTier}.");

        if (isPanelOpen)
        {
            // Delay update by one frame to allow button animation to complete
            StartCoroutine(UpdateRecipeEntryNextFrame(upgradedRecipe));
        }
        else
        {
            // Panel is closed - mark as dirty for next open
            isDirty = true;
        }
    }

}
