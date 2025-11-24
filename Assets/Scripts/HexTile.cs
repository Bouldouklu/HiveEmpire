using UnityEngine;

/// <summary>
/// Represents a single hex tile within a BiomeRegion.
/// Provides visual representation and bee spawn point.
/// All game logic is handled by the parent BiomeRegion.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class HexTile : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Parent biome region this tile belongs to")]
    [SerializeField] private BiomeRegion parentRegion;

    [Tooltip("Mesh renderer for material swapping")]
    [SerializeField] private MeshRenderer tileRenderer;

    [Header("Configuration")]
    [Tooltip("Position offset for bee spawn point (relative to tile center)")]
    [SerializeField] private Vector3 beeSpawnOffset = new Vector3(0f, 2f, 0f);

    /// <summary>
    /// Gets the parent BiomeRegion this tile belongs to
    /// </summary>
    public BiomeRegion ParentRegion => parentRegion;

    /// <summary>
    /// Gets the world position where bees should spawn/gather at this tile
    /// </summary>
    public Vector3 BeeGatherPosition => transform.position + beeSpawnOffset;

    /// <summary>
    /// Gets the mesh renderer for material swapping
    /// </summary>
    public MeshRenderer TileRenderer => tileRenderer;

    private void Awake()
    {
        // Auto-assign renderer if not set
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<MeshRenderer>();
        }

        // Auto-find parent region if not set
        if (parentRegion == null)
        {
            parentRegion = GetComponentInParent<BiomeRegion>();
        }

        if (parentRegion == null)
        {
            Debug.LogError($"HexTile '{name}' has no parent BiomeRegion!", this);
        }
    }

    /// <summary>
    /// Sets the parent BiomeRegion (used during prefab instantiation)
    /// </summary>
    public void SetParentRegion(BiomeRegion region)
    {
        parentRegion = region;
    }

    /// <summary>
    /// Applies a material to this tile's renderer
    /// </summary>
    public void ApplyMaterial(Material material)
    {
        if (tileRenderer != null && material != null)
        {
            tileRenderer.material = material;
        }
    }

    private void OnValidate()
    {
        // Auto-assign renderer in editor
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<MeshRenderer>();
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize bee gather position in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(BeeGatherPosition, 0.3f);
    }
}
