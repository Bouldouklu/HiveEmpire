using UnityEngine;

/// <summary>
/// Controls an airport that spawns airplanes to deliver resources to the city.
/// </summary>
public class AirportController : MonoBehaviour
{
    [Header("Airplane Spawning")]
    [Tooltip("Prefab of the airplane to spawn")]
    [SerializeField] private GameObject airplanePrefab;

    [Tooltip("Time in seconds between airplane spawns")]
    [SerializeField] private float spawnInterval = 3f;

    [Tooltip("Vertical offset for airplane spawn position")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Airport Settings")]
    [Tooltip("Resource type this airport produces")]
    [SerializeField] private string resourceType = "Unknown";

    private float nextSpawnTime;

    private void Start()
    {
        // Schedule first spawn
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void Update()
    {
        // Check if it's time to spawn a new airplane
        if (Time.time >= nextSpawnTime)
        {
            SpawnAirplane();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// Spawns an airplane at this airport's location
    /// </summary>
    private void SpawnAirplane()
    {
        if (airplanePrefab == null)
        {
            Debug.LogError($"Airplane prefab not assigned on {gameObject.name}");
            return;
        }

        if (CityController.Instance == null)
        {
            Debug.LogError($"No CityController found in scene. Cannot spawn airplane from {gameObject.name}");
            return;
        }

        // Calculate spawn position
        Vector3 spawnPosition = transform.position + spawnOffset;

        // Instantiate airplane
        GameObject airplaneObj = Instantiate(airplanePrefab, spawnPosition, Quaternion.identity);
        AirplaneController airplane = airplaneObj.GetComponent<AirplaneController>();

        if (airplane != null)
        {
            // Initialize airplane with home airport and destination
            airplane.Initialize(this.transform, CityController.Instance.transform);
        }
        else
        {
            Debug.LogError($"Spawned airplane from {gameObject.name} does not have AirplaneController component");
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize spawn position in Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + spawnOffset, 0.5f);
    }
}
