using UnityEngine;
using System.Collections.Generic; // Required for using List<T>

/// <summary>
/// Generates a procedural mesh and material for a fuel cell GameObject.
/// This script is intended to be attached to a prefab that will be instantiated as a fuel cell.
/// It creates a cylindrical mesh and applies a custom material with emissive properties.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] // Ensures these components are present.
public class FuelCellGenerator : MonoBehaviour
{
    [Header("Fuel Cell Shape")]
    [Tooltip("Radius of the fuel cell's cylindrical body.")]
    [SerializeField] private float radius = 0.5f;

    [Tooltip("Height (length) of the fuel cell's cylindrical body.")]
    [SerializeField] private float height = 2f;

    [Tooltip("Number of segments around the circumference of the cylinder. More segments result in a smoother, more rounded appearance.")]
    [SerializeField] private int segments = 12;

    [Header("Fuel Cell Material")]
    [Tooltip("The base color of the fuel cell material.")]
    [SerializeField] private Color cellColor = new Color(0.1f, 0.8f, 0.1f, 1f);

    [Tooltip("The emission color, making the fuel cell appear to glow.")]
    [SerializeField] private Color emissionColor = new Color(0.2f, 1f, 0.2f, 1f);

    [Tooltip("The intensity multiplier for the emission color.")]
    [SerializeField] private float emissionIntensity = 2.0f;

    /// <summary>
    /// Cached reference to the MeshFilter component.
    /// </summary>
    private MeshFilter meshFilter;

    /// <summary>
    /// Cached reference to the MeshRenderer component.
    /// </summary>
    private MeshRenderer meshRenderer;

    /// <summary>
    /// Cached reference to the shader used for the fuel cell's material (URP Lit or Standard).
    /// </summary>
    private Shader urpLitShader;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes component references, finds the shader, and triggers the fuel cell generation.
    /// </summary>
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found on FuelCellGenerator object. It should be automatically added by RequireComponent.", this.gameObject);
            enabled = false; // Disable script if critical component is missing.
            return;
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component not found on FuelCellGenerator object. It should be automatically added by RequireComponent.", this.gameObject);
            enabled = false;
            return;
        }

        // Attempt to find the Universal Render Pipeline (URP) Lit shader, fallback to Standard shader.
        urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            urpLitShader = Shader.Find("Standard"); // Fallback for non-URP projects or if Lit is unavailable.
            if (urpLitShader == null)
            {
                Debug.LogError("Neither URP Lit nor Standard shader could be found for FuelCellGenerator. Cannot create material.", this.gameObject);
                enabled = false;
                return;
            }
        }
        GenerateFuelCellOnSelf(); // Generate the mesh and material for this GameObject.
    }

    /// <summary>
    /// Generates the procedural mesh and material for this GameObject, effectively creating the fuel cell's visual representation.
    /// Also adds and configures a CapsuleCollider for interaction.
    /// </summary>
    void GenerateFuelCellOnSelf()
    {
        // Create and assign the procedural cylindrical mesh.
        Mesh cellMesh = CreateCustomCylinderMesh(radius, height, segments);
        meshFilter.mesh = cellMesh;

        // Create and assign the custom material with emissive properties.
        Material cellMaterial = CreateMaterial(cellColor, emissionColor, emissionIntensity);
        meshRenderer.material = cellMaterial;

        // Add and configure a CapsuleCollider for physics interactions (e.g., OnTriggerEnter).
        CapsuleCollider cCollider = GetComponent<CapsuleCollider>();
        if (cCollider == null) cCollider = gameObject.AddComponent<CapsuleCollider>();
        cCollider.isTrigger = true;       // Fuel cells are typically triggers.
        cCollider.radius = radius;        // Match collider radius to mesh radius.
        cCollider.height = height;        // Match collider height to mesh height.
        cCollider.direction = 1;          // 1 for Y-axis alignment (vertical cylinder).
    }

    /// <summary>
    /// Creates a new <see cref="Material"/> instance using the cached shader.
    /// Configures the material's base color and optional emission properties.
    /// </summary>
    /// <param name="baseColor">The primary albedo color of the material.</param>
    /// <param name="emissiveColor">Optional color for emission. If null or not provided, emission is disabled.</param>
    /// <param name="intensity">Multiplier for the <paramref name="emissiveColor"/>'s brightness.</param>
    /// <returns>A newly created and configured <see cref="Material"/> instance.</returns>
    Material CreateMaterial(Color baseColor, Color? emissiveColor = null, float intensity = 1.0f)
    {
        Material mat = new Material(urpLitShader);
        mat.SetColor("_BaseColor", baseColor); // Property name for URP Lit and Standard (main color).

        if (emissiveColor.HasValue)
        {
            mat.EnableKeyword("_EMISSION"); // Enable emission on the material.
            // Apply intensity to the emission color. For HDR colors, this correctly scales brightness.
            Color finalEmission = emissiveColor.Value * intensity;
            mat.SetColor("_EmissionColor", finalEmission); // Property name for URP Lit and Standard.
        }
        else
        {
            mat.DisableKeyword("_EMISSION"); // Ensure emission is disabled if no color is provided.
        }
        return mat;
    }

    #region Mesh Creation Methods (CreateCustomCylinderMesh)
    /// <summary>
    /// Creates a procedural cylindrical mesh oriented along the Y-axis, with specified dimensions and segment count.
    /// Generates vertices, triangles, normals, and UV coordinates for the cylinder's sides and end caps.
    /// </summary>
    /// <param name="cylinderRadius">Radius of the cylinder.</param>
    /// <param name="cylinderHeight">Height of the cylinder (along the Y-axis).</param>
    /// <param name="cylinderSegments">Number of rectangular segments around the circumference. Minimum is 3.</param>
    /// <returns>A procedurally generated cylinder <see cref="Mesh"/>.</returns>
    Mesh CreateCustomCylinderMesh(float cylinderRadius, float cylinderHeight, int cylinderSegments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralCylinder";

        // Input validation.
        if (cylinderSegments < 3) cylinderSegments = 3;
        if (cylinderRadius <= 0) cylinderRadius = 0.01f;
        if (cylinderHeight <= 0) cylinderHeight = 0.01f;

        float halfHeight = cylinderHeight / 2f;
        float segmentAngle = 360f / cylinderSegments;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        // Create Cylinder Sides
        for (int i = 0; i < cylinderSegments; i++)
        {
            float angle0 = Mathf.Deg2Rad * (i * segmentAngle);
            float angle1 = Mathf.Deg2Rad * ((i + 1) % cylinderSegments * segmentAngle);

            // X and Z coordinates for a Y-axis oriented cylinder.
            float x0 = cylinderRadius * Mathf.Cos(angle0);
            float z0 = cylinderRadius * Mathf.Sin(angle0);
            float x1 = cylinderRadius * Mathf.Cos(angle1);
            float z1 = cylinderRadius * Mathf.Sin(angle1);

            // Vertices for one segment quad (bottom-left, bottom-right, top-right, top-left).
            Vector3 bl = new Vector3(x0, -halfHeight, z0);
            Vector3 br = new Vector3(x1, -halfHeight, z1);
            Vector3 tr = new Vector3(x1, halfHeight, z1);
            Vector3 tl = new Vector3(x0, halfHeight, z0);

            int vIndex = vertices.Count;
            vertices.AddRange(new Vector3[] { bl, br, tr, tl });

            // Normals for the sides point outwards from the Y-axis.
            Vector3 normal0 = new Vector3(x0, 0, z0).normalized;
            Vector3 normal1 = new Vector3(x1, 0, z1).normalized;
            normals.AddRange(new Vector3[] { normal0, normal1, normal1, normal0 });

            // UVs for the sides (unwrap the cylinder).
            float u0 = (float)i / cylinderSegments;
            float u1 = (float)(i + 1) / cylinderSegments;
            uvs.AddRange(new Vector2[] { new Vector2(u0, 0), new Vector2(u1, 0), new Vector2(u1, 1), new Vector2(u0, 1) });

            // Triangles for the side quad (ensure CCW winding from outside).
            triangles.AddRange(new int[] { vIndex, vIndex + 2, vIndex + 1 }); // bl, tr, br
            triangles.AddRange(new int[] { vIndex, vIndex + 3, vIndex + 2 }); // bl, tl, tr
        }

        // Create Top Cap
        Vector3 topCenter = new Vector3(0, halfHeight, 0);
        int topCenterIndex = vertices.Count;
        vertices.Add(topCenter);
        normals.Add(Vector3.up); // Normal points upwards.
        uvs.Add(new Vector2(0.5f, 0.5f)); // UV center.

        int topCapVertexStart = vertices.Count;
        for (int i = 0; i < cylinderSegments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * segmentAngle);
            float x = cylinderRadius * Mathf.Cos(angle);
            float z = cylinderRadius * Mathf.Sin(angle);
            vertices.Add(new Vector3(x, halfHeight, z));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(x / (2 * cylinderRadius) + 0.5f, z / (2 * cylinderRadius) + 0.5f)); // Circular UV mapping.
        }
        for (int i = 0; i < cylinderSegments; i++)
        {
            int current = topCapVertexStart + i;
            int next = topCapVertexStart + (i + 1) % cylinderSegments;
            // Triangles (CCW when viewed from above, normal is +Y).
            triangles.AddRange(new int[] { topCenterIndex, next, current });
        }

        // Create Bottom Cap
        Vector3 bottomCenter = new Vector3(0, -halfHeight, 0);
        int bottomCenterIndex = vertices.Count;
        vertices.Add(bottomCenter);
        normals.Add(Vector3.down); // Normal points downwards.
        uvs.Add(new Vector2(0.5f, 0.5f));

        int bottomCapVertexStart = vertices.Count;
        for (int i = 0; i < cylinderSegments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * segmentAngle);
            float x = cylinderRadius * Mathf.Cos(angle);
            float z = cylinderRadius * Mathf.Sin(angle);
            vertices.Add(new Vector3(x, -halfHeight, z));
            normals.Add(Vector3.down);
            uvs.Add(new Vector2(x / (2 * cylinderRadius) + 0.5f, z / (2 * cylinderRadius) + 0.5f));
        }
        for (int i = 0; i < cylinderSegments; i++)
        {
            int current = bottomCapVertexStart + i;
            int next = bottomCapVertexStart + (i + 1) % cylinderSegments;
            // Triangles (CCW when viewed from below, normal is -Y, so vertices are CW from above).
            triangles.AddRange(new int[] { bottomCenterIndex, current, next });
        }

        // Assign data to the mesh.
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds(); // Important for culling and other calculations.
        mesh.RecalculateNormals(); // Unity recalculate if manual normals are problematic.
        return mesh;
    }
    #endregion
}