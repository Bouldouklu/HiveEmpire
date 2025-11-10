using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI for a single recipe entry in the recipe display panel.
/// Shows recipe name, ingredients, progress, status, and control buttons.
/// </summary>
public class RecipeEntryUI : MonoBehaviour
{
    [Header("Recipe Info")]
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Image recipeIconImage;
    [SerializeField] private TextMeshProUGUI honeyValueText;

    [Header("Production Status")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Ingredients")]
    [SerializeField] private Transform ingredientListContainer;
    [SerializeField] private GameObject ingredientEntryPrefab;

    [Header("Priority Controls")]
    [SerializeField] private Button increasePriorityButton;
    [SerializeField] private Button decreasePriorityButton;
    [SerializeField] private TextMeshProUGUI priorityText;

    [Header("Actions")]
    [SerializeField] private Button pauseResumeButton;
    [SerializeField] private TextMeshProUGUI pauseResumeButtonText;

    [Header("Color Settings")]
    [SerializeField] private Color sufficientResourceColor = new Color(0.2f, 0.8f, 0.2f); // Green
    [SerializeField] private Color insufficientResourceColor = new Color(0.8f, 0.2f, 0.2f); // Red
    [SerializeField] private Color idleStatusColor = new Color(0.6f, 0.6f, 0.6f); // Gray
    [SerializeField] private Color producingStatusColor = new Color(0.8f, 0.8f, 0.2f); // Yellow
    [SerializeField] private Color waitingStatusColor = new Color(0.8f, 0.4f, 0.2f); // Orange

    private HoneyRecipe recipe;
    private int priorityIndex;
    private List<IngredientEntryUI> ingredientEntries = new List<IngredientEntryUI>();

    /// <summary>
    /// Initializes the recipe entry with data.
    /// </summary>
    public void Initialize(HoneyRecipe recipeData, int priority)
    {
        recipe = recipeData;
        priorityIndex = priority;

        if (recipe == null)
        {
            Debug.LogError("RecipeEntryUI: Cannot initialize with null recipe!");
            return;
        }

        // Set recipe name
        if (recipeNameText != null)
        {
            recipeNameText.text = recipe.recipeName;
        }

        // Set recipe icon
        if (recipeIconImage != null && recipe.icon != null)
        {
            recipeIconImage.sprite = recipe.icon;
            recipeIconImage.enabled = true;
        }
        else if (recipeIconImage != null)
        {
            recipeIconImage.enabled = false;
        }

        // Set honey value
        if (honeyValueText != null)
        {
            honeyValueText.text = $"${recipe.honeyValue:F0}";
        }

        // Set priority
        if (priorityText != null)
        {
            priorityText.text = $"Priority: {priorityIndex + 1}";
        }

        // Setup buttons
        SetupButtons();

        // Create ingredient entries
        CreateIngredientEntries();

        // Initial update
        UpdateProgress();
        UpdateIngredients();
        UpdateStatus();
    }

    /// <summary>
    /// Sets up button click listeners.
    /// </summary>
    private void SetupButtons()
    {
        if (increasePriorityButton != null)
        {
            increasePriorityButton.onClick.RemoveAllListeners();
            increasePriorityButton.onClick.AddListener(OnIncreasePriority);
        }

        if (decreasePriorityButton != null)
        {
            decreasePriorityButton.onClick.RemoveAllListeners();
            decreasePriorityButton.onClick.AddListener(OnDecreasePriority);
        }

        if (pauseResumeButton != null)
        {
            pauseResumeButton.onClick.RemoveAllListeners();
            pauseResumeButton.onClick.AddListener(OnPauseResume);
        }
    }

    /// <summary>
    /// Creates UI entries for each ingredient in the recipe.
    /// </summary>
    private void CreateIngredientEntries()
    {
        if (ingredientListContainer == null || ingredientEntryPrefab == null || recipe == null)
        {
            return;
        }

        // Clear existing entries
        foreach (IngredientEntryUI entry in ingredientEntries)
        {
            if (entry != null)
            {
                Destroy(entry.gameObject);
            }
        }
        ingredientEntries.Clear();

        // Create entry for each ingredient
        foreach (HoneyRecipe.Ingredient ingredient in recipe.ingredients)
        {
            GameObject entryObj = Instantiate(ingredientEntryPrefab, ingredientListContainer);
            IngredientEntryUI entryUI = entryObj.GetComponent<IngredientEntryUI>();

            if (entryUI != null)
            {
                entryUI.Initialize(ingredient.pollenType, ingredient.quantity);
                ingredientEntries.Add(entryUI);
            }
            else
            {
                Debug.LogError("RecipeEntryUI: Ingredient entry prefab missing IngredientEntryUI component!");
                Destroy(entryObj);
            }
        }
    }

    /// <summary>
    /// Updates the production progress bar and time remaining.
    /// </summary>
    public void UpdateProgress()
    {
        if (RecipeProductionManager.Instance == null || recipe == null)
        {
            return;
        }

        float progress = RecipeProductionManager.Instance.GetRecipeProgress(recipe);

        // Update progress bar
        if (progressBar != null)
        {
            progressBar.value = progress;
        }

        // Update status
        UpdateStatus();
    }

    /// <summary>
    /// Updates the ingredient availability display.
    /// </summary>
    public void UpdateIngredients()
    {
        if (HiveController.Instance == null || recipe == null)
        {
            return;
        }

        Dictionary<ResourceType, int> inventory = HiveController.Instance.GetPollenInventory();

        foreach (IngredientEntryUI entry in ingredientEntries)
        {
            if (entry != null)
            {
                int available = inventory.ContainsKey(entry.ResourceType) ? inventory[entry.ResourceType] : 0;
                bool isSufficient = available >= entry.RequiredQuantity;

                entry.UpdateAvailability(available, isSufficient ? sufficientResourceColor : insufficientResourceColor);
            }
        }
    }

    /// <summary>
    /// Updates the production status text and color.
    /// </summary>
    private void UpdateStatus()
    {
        if (RecipeProductionManager.Instance == null || recipe == null || statusText == null)
        {
            return;
        }

        bool isProducing = RecipeProductionManager.Instance.IsProducing(recipe);
        bool isPaused = RecipeProductionManager.Instance.IsPaused(recipe);
        bool canProduce = recipe.CanProduce(HiveController.Instance.GetPollenInventory());

        if (isPaused)
        {
            statusText.text = "Paused";
            statusText.color = idleStatusColor;
        }
        else if (isProducing)
        {
            statusText.text = "Producing";
            statusText.color = producingStatusColor;
        }
        else if (!canProduce)
        {
            statusText.text = "Waiting";
            statusText.color = waitingStatusColor;
        }
        else
        {
            statusText.text = "Idle";
            statusText.color = idleStatusColor;
        }

        // Update pause/resume button text
        if (pauseResumeButtonText != null)
        {
            pauseResumeButtonText.text = isPaused ? "Resume" : "Pause";
        }
    }

    /// <summary>
    /// Button handler: Increase recipe priority.
    /// </summary>
    private void OnIncreasePriority()
    {
        if (RecipeProductionManager.Instance != null && recipe != null)
        {
            RecipeProductionManager.Instance.IncreasePriority(recipe);

            // Audio feedback could be added here if UI click sound is available

            // Notify panel to refresh
            if (RecipeDisplayPanel.Instance != null)
            {
                RecipeDisplayPanel.Instance.OnPriorityChanged();
            }
        }
    }

    /// <summary>
    /// Button handler: Decrease recipe priority.
    /// </summary>
    private void OnDecreasePriority()
    {
        if (RecipeProductionManager.Instance != null && recipe != null)
        {
            RecipeProductionManager.Instance.DecreasePriority(recipe);

            // Audio feedback could be added here if UI click sound is available

            // Notify panel to refresh
            if (RecipeDisplayPanel.Instance != null)
            {
                RecipeDisplayPanel.Instance.OnPriorityChanged();
            }
        }
    }

    /// <summary>
    /// Button handler: Pause or resume recipe production.
    /// </summary>
    private void OnPauseResume()
    {
        if (RecipeProductionManager.Instance != null && recipe != null)
        {
            RecipeProductionManager.Instance.TogglePause(recipe);

            // Audio feedback could be added here if UI click sound is available

            // Update status immediately
            UpdateStatus();
        }
    }

}
