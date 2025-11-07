using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages city resource demands, tracking delivery rates over a rolling 60-second window.
/// Demands scale by 20% every 60 seconds. Payment multiplier is 1.0x when demand is met, 0.5x when not met.
/// </summary>
public class DemandManager : MonoBehaviour
{
    #region Singleton
    public static DemandManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Events
    /// <summary>
    /// Fired when a demand changes or delivery rate updates.
    /// Parameters: ResourceType, current demand (per minute), current delivery rate (per minute)
    /// </summary>
    public UnityEvent<ResourceType, float, float> OnDemandChanged;
    #endregion

    #region Private Fields
    /// <summary>
    /// Active demands: resource type -> target deliveries per minute
    /// </summary>
    private Dictionary<ResourceType, float> activeDemands = new Dictionary<ResourceType, float>();

    /// <summary>
    /// Delivery history: resource type -> queue of delivery timestamps (in seconds since game start)
    /// </summary>
    private Dictionary<ResourceType, Queue<float>> deliveryHistory = new Dictionary<ResourceType, Queue<float>>();

    /// <summary>
    /// Timer for demand scaling (increases every 60 seconds)
    /// </summary>
    private float demandScalingTimer = 0f;

    private const float DEMAND_SCALING_INTERVAL = 60f; // Scale demands every 60 seconds
    private const float DEMAND_SCALING_MULTIPLIER = 1.2f; // Increase by 20%
    private const float ROLLING_WINDOW_DURATION = 60f; // Track deliveries over last 60 seconds
    private const float PAYMENT_MULTIPLIER_MET = 1.0f; // Full payment when demand met
    private const float PAYMENT_MULTIPLIER_NOT_MET = 0.5f; // Half payment when demand not met
    #endregion

    #region Initialization
    private void Start()
    {
        // Initialize event if null
        if (OnDemandChanged == null)
        {
            OnDemandChanged = new UnityEvent<ResourceType, float, float>();
        }
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        // Clean up old delivery records
        CleanOldDeliveries();

        // Handle demand scaling timer
        demandScalingTimer += Time.deltaTime;
        if (demandScalingTimer >= DEMAND_SCALING_INTERVAL)
        {
            demandScalingTimer -= DEMAND_SCALING_INTERVAL;
            ScaleAllDemands();
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Adds a new resource demand or updates existing demand.
    /// </summary>
    /// <param name="resourceType">The resource type to demand</param>
    /// <param name="demandPerMinute">Target deliveries per minute</param>
    public void AddDemand(ResourceType resourceType, float demandPerMinute)
    {
        activeDemands[resourceType] = demandPerMinute;

        // Initialize delivery history if needed
        if (!deliveryHistory.ContainsKey(resourceType))
        {
            deliveryHistory[resourceType] = new Queue<float>();
        }

        // Fire event to update UI
        float currentRate = GetCurrentDeliveryRate(resourceType);
        OnDemandChanged?.Invoke(resourceType, demandPerMinute, currentRate);

        Debug.Log($"DemandManager: Added demand for {resourceType} - Target: {demandPerMinute}/min");
    }

    /// <summary>
    /// Records a delivery for a specific resource type.
    /// </summary>
    /// <param name="resourceType">The resource that was delivered</param>
    public void RecordDelivery(ResourceType resourceType)
    {
        // Initialize delivery history if needed
        if (!deliveryHistory.ContainsKey(resourceType))
        {
            deliveryHistory[resourceType] = new Queue<float>();
        }

        // Record timestamp
        deliveryHistory[resourceType].Enqueue(Time.time);

        // Fire event to update UI
        if (activeDemands.ContainsKey(resourceType))
        {
            float currentRate = GetCurrentDeliveryRate(resourceType);
            OnDemandChanged?.Invoke(resourceType, activeDemands[resourceType], currentRate);
        }
    }

    /// <summary>
    /// Gets the current delivery rate for a resource type over the last 60 seconds.
    /// </summary>
    /// <param name="resourceType">The resource type to check</param>
    /// <returns>Number of deliveries in the last 60 seconds</returns>
    public float GetCurrentDeliveryRate(ResourceType resourceType)
    {
        if (!deliveryHistory.ContainsKey(resourceType))
        {
            return 0f;
        }

        // Count deliveries in the rolling window
        float cutoffTime = Time.time - ROLLING_WINDOW_DURATION;
        return deliveryHistory[resourceType].Count(timestamp => timestamp >= cutoffTime);
    }

    /// <summary>
    /// Checks if the demand for a resource type is currently met.
    /// </summary>
    /// <param name="resourceType">The resource type to check</param>
    /// <returns>True if current delivery rate meets or exceeds demand</returns>
    public bool IsDemandMet(ResourceType resourceType)
    {
        if (!activeDemands.ContainsKey(resourceType))
        {
            return true; // No demand means always met
        }

        float currentRate = GetCurrentDeliveryRate(resourceType);
        float targetDemand = activeDemands[resourceType];
        return currentRate >= targetDemand;
    }

    /// <summary>
    /// Gets the payment multiplier for a resource type based on demand fulfillment.
    /// </summary>
    /// <param name="resourceType">The resource type to check</param>
    /// <returns>1.0 if demand met, 0.5 if not met</returns>
    public float GetPaymentMultiplier(ResourceType resourceType)
    {
        return IsDemandMet(resourceType) ? PAYMENT_MULTIPLIER_MET : PAYMENT_MULTIPLIER_NOT_MET;
    }

    /// <summary>
    /// Gets the current demand target for a resource type.
    /// </summary>
    /// <param name="resourceType">The resource type to check</param>
    /// <returns>Target deliveries per minute, or 0 if no demand exists</returns>
    public float GetDemand(ResourceType resourceType)
    {
        return activeDemands.ContainsKey(resourceType) ? activeDemands[resourceType] : 0f;
    }

    /// <summary>
    /// Gets all active resource demands.
    /// </summary>
    /// <returns>Dictionary of resource type to demand per minute</returns>
    public Dictionary<ResourceType, float> GetAllActiveDemands()
    {
        return new Dictionary<ResourceType, float>(activeDemands);
    }

    /// <summary>
    /// Removes a demand (useful for testing or future features).
    /// </summary>
    /// <param name="resourceType">The resource type to remove demand for</param>
    public void RemoveDemand(ResourceType resourceType)
    {
        if (activeDemands.Remove(resourceType))
        {
            Debug.Log($"DemandManager: Removed demand for {resourceType}");
            OnDemandChanged?.Invoke(resourceType, 0f, 0f);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Removes delivery records older than the rolling window.
    /// </summary>
    private void CleanOldDeliveries()
    {
        float cutoffTime = Time.time - ROLLING_WINDOW_DURATION;

        foreach (var kvp in deliveryHistory)
        {
            // Remove old timestamps from the front of the queue
            while (kvp.Value.Count > 0 && kvp.Value.Peek() < cutoffTime)
            {
                kvp.Value.Dequeue();
            }
        }
    }

    /// <summary>
    /// Scales all active demands by 20% (increases difficulty over time).
    /// </summary>
    private void ScaleAllDemands()
    {
        List<ResourceType> resourceTypes = new List<ResourceType>(activeDemands.Keys);

        foreach (ResourceType resourceType in resourceTypes)
        {
            float oldDemand = activeDemands[resourceType];
            float newDemand = oldDemand * DEMAND_SCALING_MULTIPLIER;
            activeDemands[resourceType] = newDemand;

            // Fire event to update UI
            float currentRate = GetCurrentDeliveryRate(resourceType);
            OnDemandChanged?.Invoke(resourceType, newDemand, currentRate);

            Debug.Log($"DemandManager: Scaled {resourceType} demand from {oldDemand:F1}/min to {newDemand:F1}/min");
        }
    }
    #endregion
}
