using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager for the global drone fleet system.
/// Manages a global pool of drones that can be allocated to individual airport routes.
/// Handles allocation, deallocation, and capacity constraints.
/// </summary>
public class DroneFleetManager : MonoBehaviour
{
    public static DroneFleetManager Instance { get; private set; }

    [Header("Fleet State")]
    [SerializeField]
    [Tooltip("Total number of drones owned by the player")]
    private int totalDronesOwned = 0;

    // Track drone allocation per airport (Airport -> allocated drone count)
    private Dictionary<AirportController, int> droneAllocations = new Dictionary<AirportController, int>();

    [Header("Events")]
    [Tooltip("Fired when drone allocation changes. Passes (airport, newDroneCount).")]
    public UnityEvent<AirportController, int> OnDroneAllocationChanged;

    [Tooltip("Fired when total drones owned changes. Passes new total.")]
    public UnityEvent<int> OnTotalDronesChanged;

    public int TotalDronesOwned => totalDronesOwned;

    private void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Gets the number of drones available for allocation (not currently assigned to any route).
    /// </summary>
    /// <returns>Number of unassigned drones in the global pool</returns>
    public int GetAvailableDrones()
    {
        int totalAllocated = 0;
        foreach (var allocation in droneAllocations.Values)
        {
            totalAllocated += allocation;
        }

        return totalDronesOwned - totalAllocated;
    }

    /// <summary>
    /// Gets the number of drones currently allocated to a specific airport.
    /// </summary>
    /// <param name="airport">Airport to query</param>
    /// <returns>Number of drones assigned to this airport's route</returns>
    public int GetAllocatedDrones(AirportController airport)
    {
        if (airport == null)
        {
            Debug.LogWarning("DroneFleetManager: Attempted to get allocation for null airport");
            return 0;
        }

        return droneAllocations.TryGetValue(airport, out int count) ? count : 0;
    }

    /// <summary>
    /// Adds drones to the global pool (called when purchasing airports or upgrading).
    /// </summary>
    /// <param name="count">Number of drones to add (must be positive)</param>
    public void AddDronesToPool(int count)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"DroneFleetManager: Attempted to add non-positive drone count: {count}");
            return;
        }

        totalDronesOwned += count;
        OnTotalDronesChanged?.Invoke(totalDronesOwned);

        Debug.Log($"DroneFleetManager: Added {count} drones. Total owned: {totalDronesOwned}, Available: {GetAvailableDrones()}");
    }

    /// <summary>
    /// Allocates one drone to an airport's route.
    /// Validates capacity constraints and drone availability.
    /// </summary>
    /// <param name="airport">Airport to allocate drone to</param>
    /// <returns>True if allocation succeeded, false if constraints violated</returns>
    public bool AllocateDrone(AirportController airport)
    {
        if (airport == null)
        {
            Debug.LogWarning("DroneFleetManager: Attempted to allocate drone to null airport");
            return false;
        }

        // Check if airport is at capacity
        int currentAllocation = GetAllocatedDrones(airport);
        if (currentAllocation >= airport.MaxDroneCapacity)
        {
            Debug.LogWarning($"DroneFleetManager: Airport '{airport.name}' is at capacity ({airport.MaxDroneCapacity})");
            return false;
        }

        // Check if drones are available
        if (GetAvailableDrones() <= 0)
        {
            Debug.LogWarning("DroneFleetManager: No drones available for allocation");
            return false;
        }

        // Allocate drone
        if (!droneAllocations.ContainsKey(airport))
        {
            droneAllocations[airport] = 0;
        }

        droneAllocations[airport]++;
        OnDroneAllocationChanged?.Invoke(airport, droneAllocations[airport]);

        Debug.Log($"DroneFleetManager: Allocated drone to '{airport.name}'. Now has {droneAllocations[airport]}/{airport.MaxDroneCapacity}. Available: {GetAvailableDrones()}");
        return true;
    }

    /// <summary>
    /// Deallocates one drone from an airport's route, returning it to the global pool.
    /// </summary>
    /// <param name="airport">Airport to deallocate drone from</param>
    /// <returns>True if deallocation succeeded, false if constraints violated</returns>
    public bool DeallocateDrone(AirportController airport)
    {
        if (airport == null)
        {
            Debug.LogWarning("DroneFleetManager: Attempted to deallocate drone from null airport");
            return false;
        }

        // Check if airport has any drones allocated
        int currentAllocation = GetAllocatedDrones(airport);
        if (currentAllocation <= 0)
        {
            Debug.LogWarning($"DroneFleetManager: Airport '{airport.name}' has no drones to deallocate");
            return false;
        }

        // Deallocate drone
        droneAllocations[airport]--;

        // Clean up dictionary entry if allocation reaches 0
        if (droneAllocations[airport] == 0)
        {
            droneAllocations.Remove(airport);
        }

        OnDroneAllocationChanged?.Invoke(airport, GetAllocatedDrones(airport));

        Debug.Log($"DroneFleetManager: Deallocated drone from '{airport.name}'. Now has {GetAllocatedDrones(airport)}/{airport.MaxDroneCapacity}. Available: {GetAvailableDrones()}");
        return true;
    }

    /// <summary>
    /// Unregisters an airport from the fleet system (called when airport is destroyed).
    /// Returns allocated drones to the global pool.
    /// </summary>
    /// <param name="airport">Airport being destroyed</param>
    public void UnregisterAirport(AirportController airport)
    {
        if (airport == null) return;

        if (droneAllocations.ContainsKey(airport))
        {
            int freedDrones = droneAllocations[airport];
            droneAllocations.Remove(airport);

            Debug.Log($"DroneFleetManager: Unregistered airport '{airport.name}'. Freed {freedDrones} drones. Available: {GetAvailableDrones()}");
        }
    }

    /// <summary>
    /// Gets all airports that currently have drone allocations.
    /// Useful for UI display and iteration.
    /// </summary>
    /// <returns>List of airports with allocated drones</returns>
    public List<AirportController> GetAllAllocatedAirports()
    {
        return new List<AirportController>(droneAllocations.Keys);
    }

    /// <summary>
    /// Sets total drones owned (useful for testing/save loading).
    /// WARNING: Does not adjust allocations. Only use for initialization.
    /// </summary>
    /// <param name="count">New total drone count</param>
    public void SetTotalDrones(int count)
    {
        totalDronesOwned = Mathf.Max(0, count);
        OnTotalDronesChanged?.Invoke(totalDronesOwned);

        Debug.Log($"DroneFleetManager: Total drones set to: {totalDronesOwned}");
    }
}
