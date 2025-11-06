using UnityEngine;

/// <summary>
/// Controls airplane movement with smooth arc trajectories between airports and city.
/// </summary>
public class AirplaneController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of the airplane (units per second)")]
    [SerializeField] private float speed = 10f;

    [Tooltip("Maximum height of the flight arc")]
    [SerializeField] private float cruisingAltitude = 12f;

    [Tooltip("How smoothly the airplane rotates to face movement direction")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("State")]
    [Tooltip("Current movement state (for debugging)")]
    [SerializeField] private FlightState currentState = FlightState.Idle;

    [Header("Debug Visualization")]
    [Tooltip("Show flight path gizmos in Scene view")]
    [SerializeField] private bool showFlightPathGizmos = true;

    // References
    private Transform homeAirport;
    private Transform cityDestination;

    // Movement tracking
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 controlPoint1; // First control point (ascent)
    private Vector3 controlPoint2; // Second control point (descent)
    private float journeyProgress; // 0 to 1
    private float journeyDistance;

    private enum FlightState
    {
        Idle,
        ToCity,
        AtCity,
        ToAirport,
        AtAirport
    }

    /// <summary>
    /// Initializes the airplane with home airport and destination.
    /// Called by AirportController when spawned.
    /// </summary>
    public void Initialize(Transform airport, Transform city)
    {
        homeAirport = airport;
        cityDestination = city;

        // Start journey to city
        StartJourneyToCity();
    }

    private void Update()
    {
        switch (currentState)
        {
            case FlightState.ToCity:
                UpdateFlightMovement();
                break;

            case FlightState.AtCity:
                // Brief pause at city, then return home
                StartJourneyToAirport();
                break;

            case FlightState.ToAirport:
                UpdateFlightMovement();
                break;

            case FlightState.AtAirport:
                // Airplane has returned home - destroy it
                Destroy(gameObject);
                break;
        }
    }

    /// <summary>
    /// Starts the journey from home airport to city
    /// </summary>
    private void StartJourneyToCity()
    {
        if (cityDestination == null)
        {
            Debug.LogError("City destination not set!");
            return;
        }

        startPosition = transform.position;
        endPosition = CityController.Instance.LandingPosition;

        SetupFlightPath();
        currentState = FlightState.ToCity;
    }

    /// <summary>
    /// Starts the return journey from city to home airport
    /// </summary>
    private void StartJourneyToAirport()
    {
        if (homeAirport == null)
        {
            Debug.LogError("Home airport not set!");
            return;
        }

        startPosition = transform.position;
        endPosition = homeAirport.position + new Vector3(0f, 0.5f, 0f);

        SetupFlightPath();
        currentState = FlightState.ToAirport;
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
        float targetHeight = baseHeight + cruisingAltitude;

        // Control point 1: ascent phase (30% of horizontal distance)
        Vector3 cp1Horizontal = startPosition + horizontalDirection * 0.3f;
        controlPoint1 = new Vector3(cp1Horizontal.x, targetHeight, cp1Horizontal.z);

        // Control point 2: descent phase (70% of horizontal distance)
        Vector3 cp2Horizontal = startPosition + horizontalDirection * 0.7f;
        controlPoint2 = new Vector3(cp2Horizontal.x, targetHeight, cp2Horizontal.z);
    }

    /// <summary>
    /// Updates the airplane's position along the arc using bezier curve
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
    /// Called when airplane completes its current journey
    /// </summary>
    private void OnJourneyComplete()
    {
        if (currentState == FlightState.ToCity)
        {
            currentState = FlightState.AtCity;
        }
        else if (currentState == FlightState.ToAirport)
        {
            currentState = FlightState.AtAirport;
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize flight path in Scene view
        if (showFlightPathGizmos && (currentState == FlightState.ToCity || currentState == FlightState.ToAirport))
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
