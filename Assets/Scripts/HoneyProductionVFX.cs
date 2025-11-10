using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Handles visual effects for honey production completion.
/// Shows particle effects and income popups when recipes are completed.
/// </summary>
[RequireComponent(typeof(RecipeProductionManager))]
public class HoneyProductionVFX : MonoBehaviour
{
    [Header("Particle Effects")]
    [Tooltip("Particle system to play when honey is produced")]
    [SerializeField]
    private ParticleSystem honeyProductionParticles;

    [Tooltip("If true, creates particles at hive position. If false, uses assigned particle system.")]
    [SerializeField]
    private bool createParticlesAtRuntime = true;

    [Header("Income Popup")]
    [Tooltip("Prefab for floating income text (optional)")]
    [SerializeField]
    private GameObject incomePopupPrefab;

    [Tooltip("Offset position for income popup relative to hive")]
    [SerializeField]
    private Vector3 popupOffset = new Vector3(0f, 2f, 0f);

    [Tooltip("Duration for popup to fade out")]
    [SerializeField]
    private float popupDuration = 2f;

    [Tooltip("How far popup should rise")]
    [SerializeField]
    private float popupRiseDistance = 1.5f;

    private RecipeProductionManager productionManager;

    private void Awake()
    {
        productionManager = GetComponent<RecipeProductionManager>();

        // Subscribe to recipe completion events
        if (productionManager != null)
        {
            productionManager.OnRecipeCompleted.AddListener(OnRecipeCompleted);
        }

        // Create particle system at runtime if needed
        if (createParticlesAtRuntime && honeyProductionParticles == null)
        {
            CreateDefaultParticleSystem();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (productionManager != null)
        {
            productionManager.OnRecipeCompleted.RemoveListener(OnRecipeCompleted);
        }
    }

    /// <summary>
    /// Called when a recipe completes production.
    /// </summary>
    private void OnRecipeCompleted(HoneyRecipe recipe, float honeyValue)
    {
        if (recipe == null)
            return;

        // Play particle effects
        PlayProductionParticles(recipe);

        // Show income popup
        ShowIncomePopup(honeyValue, recipe.honeyColor);
    }

    /// <summary>
    /// Play particle effects for honey production.
    /// </summary>
    private void PlayProductionParticles(HoneyRecipe recipe)
    {
        if (honeyProductionParticles == null)
            return;

        // Set particle color to match honey
        var main = honeyProductionParticles.main;
        main.startColor = recipe.honeyColor;

        // Play particles
        honeyProductionParticles.Play();
    }

    /// <summary>
    /// Show floating income text popup.
    /// </summary>
    private void ShowIncomePopup(float value, Color color)
    {
        // If we have a prefab, instantiate it
        if (incomePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + popupOffset;
            GameObject popup = Instantiate(incomePopupPrefab, spawnPosition, Quaternion.identity);

            // Try to set text if it has a TextMeshPro component
            TextMeshPro textComponent = popup.GetComponent<TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = $"+${value:F2}";
                textComponent.color = color;
            }

            // Start fade and rise animation
            StartCoroutine(AnimatePopup(popup, spawnPosition));
        }
        else
        {
            // Fallback: Just log to console
            Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>Honey Produced! +${value:F2}</color>");
        }
    }

    /// <summary>
    /// Animate popup rising and fading out.
    /// </summary>
    private IEnumerator AnimatePopup(GameObject popup, Vector3 startPosition)
    {
        float elapsed = 0f;
        Vector3 endPosition = startPosition + Vector3.up * popupRiseDistance;

        TextMeshPro textComponent = popup.GetComponent<TextMeshPro>();
        Color startColor = textComponent != null ? textComponent.color : Color.white;

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupDuration;

            // Move up
            popup.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            // Fade out
            if (textComponent != null)
            {
                Color newColor = startColor;
                newColor.a = Mathf.Lerp(1f, 0f, t);
                textComponent.color = newColor;
            }

            yield return null;
        }

        // Destroy popup when done
        Destroy(popup);
    }

    /// <summary>
    /// Create a default particle system if none is assigned.
    /// </summary>
    private void CreateDefaultParticleSystem()
    {
        GameObject particleObj = new GameObject("HoneyProductionParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.up * 0.5f;

        honeyProductionParticles = particleObj.AddComponent<ParticleSystem>();

        // Configure default particle settings
        var main = honeyProductionParticles.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 2f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.75f, 0.2f); // Golden honey color
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false; // Don't play automatically
        main.loop = false; // Don't loop - only play when triggered

        var emission = honeyProductionParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15, 20)
        });

        var shape = honeyProductionParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var renderer = honeyProductionParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

        // Explicitly stop the particle system to prevent any initial emission
        honeyProductionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        Debug.Log("Created default particle system for honey production VFX");
    }

    /// <summary>
    /// Test method to manually trigger effects (for debugging in editor).
    /// </summary>
    [ContextMenu("Test VFX")]
    private void TestVFX()
    {
        ShowIncomePopup(25f, new Color(1f, 0.75f, 0.2f));
        if (honeyProductionParticles != null)
        {
            honeyProductionParticles.Play();
        }
    }
}
