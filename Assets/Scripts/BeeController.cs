using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls bee movement with smooth arc trajectories between flower patches and hive.
/// Bees continuously loop between flower patch and hive until manually destroyed.
/// </summary>
public class BeeController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base speed of the bee (units per second) - modified by seasonal effects")]
    [SerializeField] private float baseSpeed = 6f;

    [Tooltip("Current effective speed (base * seasonal modifier)")]
    [SerializeField] private float speed = 6f;

    [Tooltip("Maximum height of the flight arc")]
    [SerializeField] private float flightAltitude = 2f;

    /// <summary>
    /// Public accessor for flight altitude (used by RouteController for spawn interval calculations)
    /// </summary>
    public float FlightAltitude => flightAltitude;

    /// <summary>
    /// Public accessor for base speed (used by RouteController for spawn interval calculations)
    /// </summary>
    public float BaseSpeed => baseSpeed;

    [Tooltip("How smoothly the bee rotates to face movement direction")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("State")]
    [Tooltip("Current movement state (for debugging)")]
    [SerializeField] private FlightState currentState = FlightState.Idle;

    [Header("Pollen Settings")]
    [Tooltip("Maximum amount of pollen this bee can carry per trip")]
    [SerializeField] private int pollenCapacity = 1;

    [Header("Debug Visualization")]
    [Tooltip("Show flight path gizmos in Scene view")]
    [SerializeField] private bool showFlightPathGizmos = true;

    [Header("Debug Info")]
    [Tooltip("Current pollen being carried (for debugging)")]
    [SerializeField] private List<FlowerPatchData> currentPollen = new List<FlowerPatchData>();

    // References
    private Transform homeFlowerPatch;
    private Transform hiveDestination;
    private FlowerPatchController homeFlowerPatchController;
    private GameObject pollenObject;
    private MeshRenderer pollenRenderer;
    private TrailRenderer trailRenderer;

    // Movement tracking
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 controlPoint1; // First control point (ascent)
    private Vector3 controlPoint2; // Second control point (descent)
    private float journeyProgress; // 0 to 1
    private float journeyDistance;

    // Gathering behavior
    private float gatheringDuration; // Cached from FlowerPatchData
    private float gatheringTimer; // Tracks elapsed time in gathering state
    private Vector3 gatheringCenterPosition; // Reference point for hovering
    private float perlinOffsetX; // Random seed for Perlin noise X
    private float perlinOffsetY; // Random seed for Perlin noise Y

    [Header("Gathering Settings")]
    [Tooltip("Maximum distance from gathering center during hover (units)")]
    [SerializeField] private float gatheringRadius = 1.5f;

    [Tooltip("Speed of Perlin noise sampling (higher = faster/more erratic movement)")]
    [SerializeField] private float gatheringSpeed = 0.5f;

    [Tooltip("Smoothness of position transitions during gathering (higher = smoother)")]
    [SerializeField] private float gatheringLerpSpeed = 2f;

    private enum FlightState
    {
        Idle,
        ToHive,
        AtHive,
        ToFlowerPatch,
        AtFlowerPatch,
        Gathering
    }

    /// <summary>
    /// Initializes the bee with home flower patch and hive destination.
    /// Called by RouteController when spawned.
    /// </summary>
    public void Initialize(Transform flowerPatch, Transform hive)
    {
        homeFlowerPatch = flowerPatch;
        hiveDestination = hive;

        // Get reference to flower patch controller
        homeFlowerPatchController = flowerPatch.GetComponent<FlowerPatchController>();
        if (homeFlowerPatchController == null)
        {
            Debug.LogError($"Bee {name}: Home flower patch {flowerPatch.name} does not have FlowerPatchController component!");
        }

        // Cache pollen GameObject references
        Transform pollenTransform = transform.Find("Pollen");
        if (pollenTransform != null)
        {
            pollenObject = pollenTransform.gameObject;
            pollenRenderer = pollenObject.GetComponent<MeshRenderer>();

            if (pollenRenderer == null)
            {
                Debug.LogWarning($"Bee {name}: Pollen GameObject found but has no MeshRenderer!");
            }

            // Start with pollen hidden (bee starts empty at flower patch)
            pollenObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Bee {name}: No 'Pollen' child GameObject found! Pollen visibility will not work.");
        }

        // Cache trail renderer reference
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            Debug.LogWarning($"Bee {name}: No TrailRenderer component found! Trail colors will not work.");
        }

        // Register with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterBee();
        }
        else
        {
            Debug.LogWarning($"Bee {name}: GameManager not found in scene!");
        }

        // Start at flower patch, ready to pick up pollen
        currentState = FlightState.AtFlowerPatch;

        // Subscribe to season changes for speed modifiers
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.AddListener(OnSeasonChanged);
            // Apply initial seasonal modifier
            ApplySeasonalSpeedModifier();
        }

        // Cache gathering duration from FlowerPatchData
        if (homeFlowerPatchController != null && homeFlowerPatchController.FlowerPatchData != null)
        {
            gatheringDuration = homeFlowerPatchController.FlowerPatchData.gatheringDuration;
        }
        else
        {
            Debug.LogWarning($"Bee {name}: Could not cache gathering duration. Using default value of 2.5 seconds.");
            gatheringDuration = 2.5f;
        }

        // Generate random Perlin noise seeds for unique movement patterns per bee
        perlinOffsetX = Random.Range(0f, 1000f);
        perlinOffsetY = Random.Range(0f, 1000f);
    }

    /// <summary>
    /// Called when the season changes to update bee speed
    /// </summary>
    private void OnSeasonChanged(Season newSeason)
    {
        ApplySeasonalSpeedModifier();
    }

    /// <summary>
    /// Applies the current seasonal speed modifier to the bee's speed
    /// </summary>
    private void ApplySeasonalSpeedModifier()
    {
        if (SeasonManager.Instance == null)
        {
            speed = baseSpeed;
            return;
        }

        SeasonData currentSeason = SeasonManager.Instance.GetCurrentSeasonData();
        if (currentSeason == null)
        {
            speed = baseSpeed;
            return;
        }

        // Apply seasonal bee speed modifier
        speed = baseSpeed * currentSeason.beeSpeedModifier;
    }

    private void Update()
    {
        switch (currentState)
        {
            case FlightState.ToHive:
                UpdateFlightMovement();
                break;

            case FlightState.AtHive:
                // Deliver pollen and return home
                DeliverPollen();
                StartJourneyToFlowerPatch();
                break;

            case FlightState.ToFlowerPatch:
                UpdateFlightMovement();
                break;

            case FlightState.AtFlowerPatch:
                // Start gathering behavior instead of immediately picking up pollen
                StartGathering();
                break;

            case FlightState.Gathering:
                // Hover above flower patch with Perlin noise movement
                UpdateGatheringMovement();
                break;
        }
    }

    /// <summary>
    /// Starts the journey from home flower patch to hive
    /// </summary>
    private void StartJourneyToHive()
    {
        if (hiveDestination == null)
        {
            Debug.LogError("Hive destination not set!");
            return;
        }

        startPosition = transform.position;
        endPosition = HiveController.Instance.LandingPosition;

        SetupFlightPath();
        currentState = FlightState.ToHive;
    }

    /// <summary>
    /// Starts the return journey from hive to home flower patch
    /// </summary>
    private void StartJourneyToFlowerPatch()
    {
        if (homeFlowerPatch == null)
        {
            Debug.LogError("Home flower patch not set!");
            return;
        }

        startPosition = transform.position;
        endPosition = homeFlowerPatch.position + new Vector3(0f, 0.5f, 0f);

        SetupFlightPath();
        currentState = FlightState.ToFlowerPatch;
    }

    /// <summary>
    /// Starts the gathering phase where bee hovers above flower patch
    /// </summary>
    private void StartGathering()
    {
        // Reset gathering timer
        gatheringTimer = 0f;

        // Set gathering center position (current position)
        gatheringCenterPosition = transform.position;

        // Transition to gathering state
        currentState = FlightState.Gathering;

        Debug.Log($"Bee {name}: Started gathering at {homeFlowerPatchController.GetBiomeType()} patch for {gatheringDuration} seconds");
    }

    /// <summary>
    /// Sets up the arc flight path using cubic bezier curve with two control points
    /// </summary>
    private void SetupFlightPath()
    {
        // Reset progress
        journeyProgress = 0f;

        // Calculate journey distance (straight line)
        journeyDistance = Vector3.Distance(startPosition, endPosition);

        // Calculate control points to create a flat cruising section
        // First control point: 1/3 of the way horizontally, at cruising altitude
        Vector3 horizontalDirection = endPosition - startPosition;
        horizontalDirection.y = 0f; // Flatten to horizontal plane

        float baseHeight = Mathf.Max(startPosition.y, endPosition.y);
        float targetHeight = baseHeight + flightAltitude;

        // Control point 1: ascent phase (30% of horizontal distance)
        Vector3 cp1Horizontal = startPosition + horizontalDirection * 0.3f;
        controlPoint1 = new Vector3(cp1Horizontal.x, targetHeight, cp1Horizontal.z);

        // Control point 2: descent phase (70% of horizontal distance)
        Vector3 cp2Horizontal = startPosition + horizontalDirection * 0.7f;
        controlPoint2 = new Vector3(cp2Horizontal.x, targetHeight, cp2Horizontal.z);
    }

    /// <summary>
    /// Updates the bee's position along the arc using bezier curve
    /// </summary>
    private void UpdateFlightMovement()
    {
        // Increment progress based on speed and distance
        float progressIncrement = (speed * Time.deltaTime) / journeyDistance;
        journeyProgress += progressIncrement;

        if (journeyProgress >= 1f)
        {
            // Journey complete
            journeyProgress = 1f;
            OnJourneyComplete();
            return;
        }

        // Calculate position on cubic bezier curve
        Vector3 newPosition = CalculateCubicBezierPoint(journeyProgress, startPosition, controlPoint1, controlPoint2, endPosition);

        // Calculate direction for rotation
        Vector3 direction = newPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            // Rotate to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move to new position
        transform.position = newPosition;
    }

    /// <summary>
    /// Updates the bee's position during gathering phase using Perlin noise for smooth random movement
    /// </summary>
    private void UpdateGatheringMovement()
    {
        // Increment gathering timer
        gatheringTimer += Time.deltaTime;

        // Check if gathering is complete
        if (gatheringTimer >= gatheringDuration)
        {
            // Show pollen visually (gathering complete)
            PickupPollen();

            // Start journey to hive
            StartJourneyToHive();
            return;
        }

        // Calculate Perlin noise offsets for smooth random movement
        float time = Time.time * gatheringSpeed;

        // Sample Perlin noise for X and Z axes (horizontal movement)
        float noiseX = Mathf.PerlinNoise(perlinOffsetX + time, 0f);
        float noiseZ = Mathf.PerlinNoise(perlinOffsetY + time, 0f);

        // Sample Perlin noise for Y axis (vertical bobbing) - slower movement
        float noiseY = Mathf.PerlinNoise(time * 0.5f, 1000f);

        // Convert noise values (0-1) to offset values (-1 to 1)
        Vector3 offset = new Vector3(
            (noiseX - 0.5f) * 2f * gatheringRadius,      // X: full radius
            (noiseY - 0.5f) * 0.5f,                       // Y: gentle bobbing (half of radius)
            (noiseZ - 0.5f) * 2f * gatheringRadius       // Z: full radius
        );

        // Calculate target position
        Vector3 targetPosition = gatheringCenterPosition + offset;

        // Smoothly move towards target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, gatheringLerpSpeed * Time.deltaTime);

        // Calculate direction for rotation (face movement direction)
        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            // Rotate to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Calculates a point on a cubic bezier curve
    /// </summary>
    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Cubic Bezier formula: B(t) = (1-t)³ * P0 + 3(1-t)²t * P1 + 3(1-t)t² * P2 + t³ * P3
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0; // (1-t)³ * P0
        point += 3f * uu * t * p1; // 3(1-t)²t * P1
        point += 3f * u * tt * p2; // 3(1-t)t² * P2
        point += ttt * p3; // t³ * P3

        return point;
    }

    /// <summary>
    /// Calculates the arc length of a cubic Bezier curve using numerical integration.
    /// This matches the flight path setup used by SetupFlightPath().
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="end">Ending position</param>
    /// <param name="flightAltitude">Height of the arc above base height</param>
    /// <param name="segments">Number of segments for numerical integration (higher = more accurate)</param>
    /// <returns>Approximate arc length of the Bezier curve</returns>
    public static float CalculateBezierArcLength(Vector3 start, Vector3 end, float flightAltitude, int segments = 100)
    {
        // Calculate control points using the same logic as SetupFlightPath()
        Vector3 horizontalDirection = end - start;
        horizontalDirection.y = 0f; // Flatten to horizontal plane

        float baseHeight = Mathf.Max(start.y, end.y);
        float targetHeight = baseHeight + flightAltitude;

        // Control point 1: ascent phase (30% of horizontal distance)
        Vector3 cp1Horizontal = start + horizontalDirection * 0.3f;
        Vector3 controlPoint1 = new Vector3(cp1Horizontal.x, targetHeight, cp1Horizontal.z);

        // Control point 2: descent phase (70% of horizontal distance)
        Vector3 cp2Horizontal = start + horizontalDirection * 0.7f;
        Vector3 controlPoint2 = new Vector3(cp2Horizontal.x, targetHeight, cp2Horizontal.z);

        // Numerical integration: sample the curve and sum distances between sample points
        float arcLength = 0f;
        Vector3 previousPoint = start;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 currentPoint = CalculateCubicBezierPointStatic(t, start, controlPoint1, controlPoint2, end);
            arcLength += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return arcLength;
    }

    /// <summary>
    /// Static version of CalculateCubicBezierPoint for use in static methods
    /// </summary>
    private static Vector3 CalculateCubicBezierPointStatic(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Cubic Bezier formula: B(t) = (1-t)³ * P0 + 3(1-t)²t * P1 + 3(1-t)t² * P2 + t³ * P3
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0; // (1-t)³ * P0
        point += 3f * uu * t * p1; // 3(1-t)²t * P1
        point += 3f * u * tt * p2; // 3(1-t)t² * P2
        point += ttt * p3; // t³ * P3

        return point;
    }

    /// <summary>
    /// Called when bee completes its current journey
    /// </summary>
    private void OnJourneyComplete()
    {
        if (currentState == FlightState.ToHive)
        {
            currentState = FlightState.AtHive;
        }
        else if (currentState == FlightState.ToFlowerPatch)
        {
            currentState = FlightState.AtFlowerPatch;
        }
    }

    /// <summary>
    /// Picks up pollen from the home flower patch. Instantly fills pollen sacs to capacity.
    /// </summary>
    private void PickupPollen()
    {
        if (homeFlowerPatchController == null)
        {
            Debug.LogWarning($"Bee {name}: Cannot pick up pollen - no flower patch controller reference!");
            return;
        }

        // Clear any existing pollen
        currentPollen.Clear();

        // Get the flower patch data
        FlowerPatchData patchData = homeFlowerPatchController.GetFlowerPatchData();
        if (patchData == null)
        {
            Debug.LogWarning($"Bee {name}: FlowerPatchController has no FlowerPatchData!");
            return;
        }

        // Fill pollen sacs to capacity
        for (int i = 0; i < pollenCapacity; i++)
        {
            currentPollen.Add(patchData);
        }

        Debug.Log($"Bee {name}: Picked up {currentPollen.Count} {patchData.pollenDisplayName}");

        // Show pollen and apply pollen color/material
        if (pollenObject != null && pollenRenderer != null)
        {
            pollenObject.SetActive(true);

            // Use pollenMaterial if available, otherwise use derived color
            if (patchData.pollenMaterial != null)
            {
                pollenRenderer.material = patchData.pollenMaterial;
            }
            else if (FlowerPatchMaterialMapper.Instance != null)
            {
                // Use color derived from FlowerPatchMaterialMapper
                pollenRenderer.material.color = FlowerPatchMaterialMapper.Instance.GetPollenColor(patchData.biomeType);
            }
            else
            {
                // Ultimate fallback if mapper not available
                Debug.LogWarning("FlowerPatchMaterialMapper not available, using yellow as fallback");
                pollenRenderer.material.color = Color.yellow;
            }
        }

        // Update trail color to match pollen color (derived from material)
        Color pollenColor = FlowerPatchMaterialMapper.Instance != null
            ? FlowerPatchMaterialMapper.Instance.GetPollenColor(patchData.biomeType)
            : Color.yellow;
        UpdateTrailColor(pollenColor);
    }

    /// <summary>
    /// Delivers pollen to the hive. Passes resources to HiveController for processing.
    /// </summary>
    private void DeliverPollen()
    {
        if (currentPollen.Count == 0)
        {
            Debug.LogWarning($"Bee {name}: Arrived at hive with no pollen!");
            return;
        }

        // Deliver resources to hive
        if (HiveController.Instance != null)
        {
            HiveController.Instance.ReceiveResources(currentPollen);
            Debug.Log($"Bee {name}: Delivered {currentPollen.Count} pollen to hive");
        }
        else
        {
            Debug.LogWarning($"Bee {name}: HiveController not found! Pollen lost.");
        }

        // Clear pollen after delivery
        currentPollen.Clear();

        // Hide pollen after delivery
        if (pollenObject != null)
        {
            pollenObject.SetActive(false);
        }

        // Clear trail to provide visual feedback of delivery
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    /// <summary>
    /// Updates the trail renderer color gradient to match the biome color.
    /// Maintains the fade from opaque to transparent over the trail lifetime.
    /// </summary>
    private void UpdateTrailColor(Color biomeColor)
    {
        if (trailRenderer == null)
        {
            return;
        }

        // Clear existing trail points first - gradient only affects NEW points
        trailRenderer.Clear();

        // Set both startColor/endColor AND gradient for maximum compatibility
        trailRenderer.startColor = biomeColor;
        trailRenderer.endColor = new Color(biomeColor.r, biomeColor.g, biomeColor.b, 0f); // Same color but transparent

        // Also set the gradient (some shaders use this instead)
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(biomeColor, 0f),  // Start of trail
                new GradientColorKey(biomeColor, 1f)   // End of trail
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),  // Opaque at start
                new GradientAlphaKey(0f, 1f)   // Transparent at end
            }
        );

        trailRenderer.colorGradient = gradient;
    }

    private void OnDestroy()
    {
        // Unregister from GameManager when destroyed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterBee();
        }

        // Unsubscribe from season changes
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged.RemoveListener(OnSeasonChanged);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize flight path in Scene view
        if (showFlightPathGizmos && (currentState == FlightState.ToHive || currentState == FlightState.ToFlowerPatch))
        {
            Gizmos.color = new Color(74f/255f, 85f/255f, 104f/255f); // #4a5568

            // Draw cubic bezier curve segments
            Vector3 previousPoint = startPosition;
            for (float t = 0.05f; t <= 1f; t += 0.05f)
            {
                Vector3 currentPoint = CalculateCubicBezierPoint(t, startPosition, controlPoint1, controlPoint2, endPosition);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            // Draw control points
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(controlPoint1, 0.5f);
            //Gizmos.DrawWireSphere(controlPoint2, 0.5f);

            // Draw lines to control points to show the bezier handles
            // Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
            // Gizmos.DrawLine(startPosition, controlPoint1);
            // Gizmos.DrawLine(controlPoint2, endPosition);
        }
    }
}
