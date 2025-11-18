using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [Header("Unlock/Lock State")]
    [SerializeField] private Image lockOverlay;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Button unlockButton;
    [SerializeField] private TextMeshProUGUI unlockCostText;
    [SerializeField] private TextMeshProUGUI prerequisiteText;

    [Header("Upgrade System")]
    [SerializeField] private TextMeshProUGUI tierBadgeText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private GameObject upgradeTooltip;
    [SerializeField] private TextMeshProUGUI upgradeTooltipText;

    private HoneyRecipe recipe;
    private int priorityIndex;
    private int currentTier;
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

        // Set priority
        if (priorityText != null)
        {
            priorityText.text = $"Priority: {priorityIndex + 1}";
        }

        // Setup buttons
        SetupButtons();

        // Create ingredient entries
        CreateIngredientEntries();

        // Check unlock state and set up UI accordingly
        if (RecipeProgressionManager.Instance != null)
        {
            bool isUnlocked = RecipeProgressionManager.Instance.IsRecipeUnlocked(recipe);
            currentTier = RecipeProgressionManager.Instance.GetRecipeTier(recipe);

            if (isUnlocked)
            {
                ShowUnlockedState(currentTier);
            }
            else
            {
                ShowLockedState();
            }
        }
        else
        {
            // Fallback: show as unlocked if no progression manager
            ShowUnlockedState(0);
        }

        // Initial update
        UpdateProgress();
        UpdateIngredients();
        UpdateStatus();
    }

    private void OnEnable()
    {
        // Subscribe to money changes to update button affordability
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from money changes
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
        }
    }

    /// <summary>
    /// Called when player's money changes. Updates unlock/upgrade button affordability.
    /// </summary>
    private void OnMoneyChanged(float newMoneyAmount)
    {
        if (recipe == null)
            return;

        UpdateButtonAffordability();
    }

    /// <summary>
    /// Updates the interactable state of unlock and upgrade buttons based on affordability.
    /// </summary>
    private void UpdateButtonAffordability()
    {
        if (RecipeProgressionManager.Instance == null || recipe == null)
            return;

        bool isUnlocked = RecipeProgressionManager.Instance.IsRecipeUnlocked(recipe);

        if (!isUnlocked)
        {
            // Update unlock button affordability
            if (unlockButton != null)
            {
                bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(recipe.unlockCost);
                bool prerequisitesMet = RecipeProgressionManager.Instance.GetMissingPrerequisites(recipe).Count == 0;
                unlockButton.interactable = canAfford && prerequisitesMet;
            }
        }
        else
        {
            // Update upgrade button affordability
            if (upgradeButton != null && currentTier < 3)
            {
                bool canUpgrade = RecipeProgressionManager.Instance.CanUpgradeRecipe(recipe);
                upgradeButton.interactable = canUpgrade;
            }
        }
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

        // Unlock button
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }

        // Upgrade button with tooltip hover events
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

            // Add hover events for tooltip
            EventTrigger trigger = upgradeButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = upgradeButton.gameObject.AddComponent<EventTrigger>();
            }

            // Clear existing triggers
            trigger.triggers.Clear();

            // Pointer enter (show tooltip)
            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => { ShowUpgradeTooltip(); });
            trigger.triggers.Add(pointerEnter);

            // Pointer exit (hide tooltip)
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { HideUpgradeTooltip(); });
            trigger.triggers.Add(pointerExit);
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
    /// Updates the ingredient availability display with tier-adjusted quantities.
    /// </summary>
    public void UpdateIngredients()
    {
        if (HiveController.Instance == null || recipe == null)
        {
            return;
        }

        // Get inventory as dictionary for easier lookups
        Dictionary<FlowerPatchData, int> inventory = HiveController.Instance.GetPollenInventoryDictionary();

        // Get tier-adjusted ingredients
        List<HoneyRecipe.Ingredient> tierAdjustedIngredients = recipe.GetIngredients(currentTier);

        // Update each ingredient entry with tier-adjusted quantities
        for (int i = 0; i < ingredientEntries.Count && i < tierAdjustedIngredients.Count; i++)
        {
            IngredientEntryUI entry = ingredientEntries[i];
            HoneyRecipe.Ingredient tierIngredient = tierAdjustedIngredients[i];

            if (entry != null && tierIngredient.pollenType != null)
            {
                int available = inventory.ContainsKey(tierIngredient.pollenType) ? inventory[tierIngredient.pollenType] : 0;
                bool isSufficient = available >= tierIngredient.quantity;

                // Update the entry with tier-adjusted required quantity
                entry.Initialize(tierIngredient.pollenType, tierIngredient.quantity);
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

    #region Unlock/Lock State Management

    /// <summary>
    /// Shows the locked state visual: gray overlay, lock icon, unlock button, prerequisites.
    /// Disables all production controls.
    /// </summary>
    private void ShowLockedState()
    {
        // Show lock visual elements
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(true);
        if (lockIcon != null)
            lockIcon.gameObject.SetActive(true);

        // Show unlock button with cost
        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(true);

            // Check affordability and prerequisites
            bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(recipe.unlockCost);
            bool prerequisitesMet = RecipeProgressionManager.Instance != null &&
                                   RecipeProgressionManager.Instance.GetMissingPrerequisites(recipe).Count == 0;

            unlockButton.interactable = canAfford && prerequisitesMet;
        }

        // Show unlock cost
        if (unlockCostText != null)
        {
            unlockCostText.text = $"Unlock (${recipe.unlockCost:F0})";
        }

        // Show prerequisites if any are missing
        if (prerequisiteText != null && RecipeProgressionManager.Instance != null)
        {
            List<string> missingPrereqs = RecipeProgressionManager.Instance.GetMissingPrerequisites(recipe);
            if (missingPrereqs.Count > 0)
            {
                prerequisiteText.gameObject.SetActive(true);
                prerequisiteText.text = "Requires: " + string.Join(", ", missingPrereqs);
            }
            else
            {
                prerequisiteText.gameObject.SetActive(false);
            }
        }

        // Hide tier/upgrade elements
        if (tierBadgeText != null)
            tierBadgeText.gameObject.SetActive(false);
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);
        if (upgradeCostText != null)
            upgradeCostText.gameObject.SetActive(false);

        // Disable production controls
        if (increasePriorityButton != null)
            increasePriorityButton.interactable = false;
        if (decreasePriorityButton != null)
            decreasePriorityButton.interactable = false;
        if (pauseResumeButton != null)
            pauseResumeButton.interactable = false;

        // Set honey value to base (tier 0)
        UpdateHoneyValue(0);
    }

    /// <summary>
    /// Shows the unlocked state: hides lock elements, shows tier badge and upgrade button.
    /// Enables all production controls.
    /// </summary>
    private void ShowUnlockedState(int tier)
    {
        currentTier = tier;

        // Hide lock visual elements
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(false);
        if (lockIcon != null)
            lockIcon.gameObject.SetActive(false);
        if (unlockButton != null)
            unlockButton.gameObject.SetActive(false);
        if (unlockCostText != null)
            unlockCostText.gameObject.SetActive(false);
        if (prerequisiteText != null)
            prerequisiteText.gameObject.SetActive(false);

        // Show and update tier badge
        UpdateTierBadge(tier);

        // Show and update upgrade button
        UpdateUpgradeButton(tier);

        // Enable production controls
        if (increasePriorityButton != null)
            increasePriorityButton.interactable = true;
        if (decreasePriorityButton != null)
            decreasePriorityButton.interactable = true;
        if (pauseResumeButton != null)
            pauseResumeButton.interactable = true;

        // Update honey value with tier bonus
        UpdateHoneyValue(tier);
    }

    /// <summary>
    /// Updates the tier badge display.
    /// </summary>
    private void UpdateTierBadge(int tier)
    {
        if (tierBadgeText != null)
        {
            tierBadgeText.gameObject.SetActive(true);
            tierBadgeText.text = $"Tier {tier}/3";
        }
    }

    /// <summary>
    /// Updates the upgrade button state and cost display.
    /// </summary>
    private void UpdateUpgradeButton(int tier)
    {
        if (upgradeButton == null || upgradeCostText == null)
            return;

        // Check if can upgrade
        bool canUpgrade = tier < 3 && RecipeProgressionManager.Instance != null &&
                         RecipeProgressionManager.Instance.CanUpgradeRecipe(recipe);

        if (tier >= 3)
        {
            // Max tier reached - hide upgrade button
            upgradeButton.gameObject.SetActive(false);
            upgradeCostText.gameObject.SetActive(false);
        }
        else
        {
            // Show upgrade button
            upgradeButton.gameObject.SetActive(true);
            upgradeCostText.gameObject.SetActive(true);

            float cost = recipe.GetUpgradeCost(tier);
            upgradeCostText.text = $"Upgrade (${cost:F0})";
            upgradeButton.interactable = canUpgrade;
        }
    }

    /// <summary>
    /// Updates the honey value text with tier-adjusted value.
    /// </summary>
    private void UpdateHoneyValue(int tier)
    {
        if (honeyValueText != null && recipe != null)
        {
            float tierValue = recipe.GetHoneyValue(tier);
            honeyValueText.text = $"${tierValue:F0}";
        }
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// Button handler: Unlock recipe.
    /// </summary>
    private void OnUnlockButtonClicked()
    {
        if (RecipeProgressionManager.Instance == null || recipe == null)
            return;

        if (RecipeProgressionManager.Instance.TryUnlockRecipe(recipe))
        {
            Debug.Log($"Successfully unlocked recipe: {recipe.recipeName}");
            // Panel will auto-refresh via OnRecipeUnlocked event in RecipeDisplayPanel
        }
        else
        {
            Debug.LogWarning($"Cannot unlock {recipe.recipeName}: insufficient funds or prerequisites not met");
        }
    }

    /// <summary>
    /// Button handler: Upgrade recipe to next tier.
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (RecipeProgressionManager.Instance == null || recipe == null)
            return;

        if (RecipeProgressionManager.Instance.TryUpgradeRecipe(recipe))
        {
            // Update tier and refresh display
            int newTier = RecipeProgressionManager.Instance.GetRecipeTier(recipe);
            currentTier = newTier;

            Debug.Log($"Successfully upgraded recipe: {recipe.recipeName} to Tier {newTier}");

            // Refresh this entry's display
            UpdateTierBadge(newTier);
            UpdateUpgradeButton(newTier);
            UpdateHoneyValue(newTier);
            UpdateIngredients(); // Use new tier-adjusted quantities
        }
        else
        {
            Debug.LogWarning($"Cannot upgrade {recipe.recipeName}: insufficient funds or max tier reached");
        }
    }

    #endregion

    #region Upgrade Tooltip

    /// <summary>
    /// Shows the upgrade preview tooltip on hover.
    /// </summary>
    private void ShowUpgradeTooltip()
    {
        if (upgradeTooltip == null || upgradeTooltipText == null || recipe == null)
            return;

        if (currentTier >= 3)
        {
            // Max tier - no tooltip
            upgradeTooltip.SetActive(false);
            return;
        }

        // Build preview text
        string preview = BuildUpgradePreviewText(currentTier, currentTier + 1);
        upgradeTooltipText.text = preview;

        // Show tooltip
        upgradeTooltip.SetActive(true);
    }

    /// <summary>
    /// Hides the upgrade preview tooltip.
    /// </summary>
    private void HideUpgradeTooltip()
    {
        if (upgradeTooltip != null)
        {
            upgradeTooltip.SetActive(false);
        }
    }

    /// <summary>
    /// Builds the upgrade preview text showing before/after comparison.
    /// </summary>
    private string BuildUpgradePreviewText(int currentTier, int nextTier)
    {
        if (recipe == null)
            return "";

        StringBuilder preview = new StringBuilder();
        preview.AppendLine($"<b>Upgrade to Tier {nextTier}</b>\n");

        // Ingredients comparison
        var currentIngredients = recipe.GetIngredients(currentTier);
        var nextIngredients = recipe.GetIngredients(nextTier);

        preview.AppendLine("<b>Ingredients:</b>");
        foreach (var currentIng in currentIngredients)
        {
            var nextIng = nextIngredients.Find(i => i.pollenType == currentIng.pollenType);
            if (nextIng != null)
            {
                preview.AppendLine($"  {currentIng.pollenType}: {currentIng.quantity} \u2192 {nextIng.quantity}");
            }
        }

        // Production time comparison
        float currentTime = recipe.GetProductionTime(currentTier);
        float nextTime = recipe.GetProductionTime(nextTier);
        preview.AppendLine($"\n<b>Time:</b> {currentTime:F1}s \u2192 {nextTime:F1}s");

        // Value comparison
        float currentValue = recipe.GetHoneyValue(currentTier);
        float nextValue = recipe.GetHoneyValue(nextTier);
        preview.AppendLine($"<b>Value:</b> ${currentValue:F2} \u2192 ${nextValue:F2}");

        return preview.ToString();
    }

    #endregion

}
