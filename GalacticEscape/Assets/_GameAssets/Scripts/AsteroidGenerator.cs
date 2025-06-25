using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Used for checking if lists are empty or have elements

/// <summary>
/// Generates multiple asteroid GameObjects with procedurally created meshes and materials.
/// This script acts as a manager/factory for creating asteroids in the scene,
/// typically controlled via the Inspector or other game systems.
/// </summary>
public class AsteroidGenerator : MonoBehaviour
{
    #region Variables

    [Header("Generation Control")]
    [Tooltip("How many asteroids to generate when the generation method is called.")]
    [SerializeField] private int numberOfAsteroids = 10;

    [Tooltip("The radius around this GameObject's position within which asteroids will be spawned.")]
    [SerializeField] private float spawnRadius = 10f;

    [Tooltip("Seed for the random number generator. Use 0 for a time-based seed, ensuring variation each run. A non-zero seed provides deterministic generation.")]
    [SerializeField] private int seed = 0;


    [Header("Asteroid Shape Settings")]
    [Tooltip("The minimum base radius for an asteroid before any deformation or scaling is applied.")]
    [SerializeField] private float minRadius = 1.5f;

    [Tooltip("The maximum base radius for an asteroid before any deformation or scaling is applied.")]
    [SerializeField] private float maxRadius = 2.5f;

    [Tooltip("Number of times to subdivide the base icosphere. Higher values result in more polygons and a more detailed, smoother base shape.")]
    [SerializeField][Range(0, 5)] private int subdivisions = 2;

    [Tooltip("Controls how much to randomly scale the base shape along X, Y, and Z axes before noise deformation. 0 = perfect sphere (no irregular scaling), 1 = maximum random scaling based on min/max scale factors.")]
    [SerializeField][Range(0f, 1f)] private float irregularity = 0.7f;

    [Tooltip("Minimum scaling factor applied to an axis if irregularity is 1.0.")]
    [SerializeField] private float minScaleFactor = 0.5f;

    [Tooltip("Maximum scaling factor applied to an axis if irregularity is 1.0.")]
    [SerializeField] private float maxScaleFactor = 1.5f;

    [Tooltip("Strength of the noise displacement applied to the mesh surface. Higher values create more pronounced deformations.")]
    [SerializeField][Range(0f, 1.5f)] private float deformationStrength = 0.5f;


    [Header("Noise Settings")]
    [Tooltip("Base scale of the Perlin/FBM noise pattern used for deformation. Larger values result in larger, smoother features on the asteroid surface.")]
    [SerializeField] private float noiseScale = 2.5f;

    [Tooltip("Number of noise layers (octaves) to combine for Fractional Brownian Motion (FBM). More octaves add more detail to the noise pattern.")]
    [SerializeField][Range(1, 8)] private int octaves = 5;

    [Tooltip("Controls how much the amplitude of each subsequent octave decreases (0-1). Lower values result in smoother noise as higher frequency details have less impact.")]
    [SerializeField][Range(0f, 1f)] private float persistence = 0.5f;

    [Tooltip("Controls how much the frequency of each subsequent octave increases (>1). Higher values result in finer, smaller details being added by later octaves.")]
    [SerializeField][Range(1f, 4f)] private float lacunarity = 2.0f;


    [Header("Asteroid Material Settings")]
    [Tooltip("If true, each generated asteroid will receive a random grayscale color. Otherwise, a default material color is used.")]
    [SerializeField] private bool useRandomGrayColor = true;

    [Tooltip("Minimum grayscale value (0-255) if random color is enabled.")]
    [SerializeField][Range(0, 255)] private byte minGrayValue = 77;

    [Tooltip("Maximum grayscale value (0-255) if random color is enabled.")]
    [SerializeField][Range(0, 255)] private byte maxGrayValue = 179;


    // --- Private Variables ---
    /// <summary>
    /// List to keep track of the generated asteroid GameObjects. Useful for clearing or managing them later.
    /// </summary>
    private List<GameObject> generatedAsteroids = new List<GameObject>();

    /// <summary>
    /// Cached instance of the default material. Used as a base if random colors are disabled or for properties not overridden by random colors.
    /// </summary>
    private Material defaultMaterialInstance;

    /// <summary>
    /// List to keep track of generated mesh instances. Essential for proper cleanup to prevent memory leaks, especially in the editor.
    /// </summary>
    private List<Mesh> generatedMeshes = new List<Mesh>();

    /// <summary>
    /// List to keep track of generated material instances. Essential for proper cleanup.
    /// </summary>
    private List<Material> generatedMaterialInstances = new List<Material>();

    #endregion

    #region Unity Methods

    /// <summary>
    /// Called in the Unity editor when a script variable is changed or the script is loaded.
    /// Enforces valid ranges for some serialized fields to prevent invalid configurations.
    /// </summary>
    void OnValidate()
    {
        if (minRadius < 0.1f) minRadius = 0.1f;
        if (maxRadius < minRadius) maxRadius = minRadius;
        if (numberOfAsteroids < 0) numberOfAsteroids = 0;
        if (spawnRadius < 0) spawnRadius = 0;
        if (minScaleFactor <= 0) minScaleFactor = 0.1f;
        if (maxScaleFactor < minScaleFactor) maxScaleFactor = minScaleFactor;
    }

    /// <summary>
    /// Called when the script instance is being destroyed.
    /// Ensures that all dynamically created mesh and material assets are cleaned up to prevent memory leaks.
    /// </summary>
    void OnDestroy()
    {
        ClearGeneratedData();
    }

    #endregion

    #region Public Generation Methods

    /// <summary>
    /// Generates the specified number of asteroids with procedural meshes and materials.
    /// This method can be called from the Inspector context menu (due to [ContextMenu] attribute) or programmatically.
    /// Prevents re-generation if asteroids managed by this instance already exist; <see cref="ClearAsteroids"/> must be called first.
    /// </summary>
    [ContextMenu("Generate Asteroids")]
    public void GenerateAsteroids()
    {
        if (generatedAsteroids.Count > 0)
        {
            Debug.LogWarning("Asteroids already generated by this instance. Please use 'Clear Generated Asteroids' first if you want to regenerate.", this.gameObject);
            return;
        }

        ClearGeneratedData(); // Ensure a clean state for new assets.

        if (seed != 0)
            Random.InitState(seed); // Use specified seed for deterministic generation.
        else
            Random.InitState(System.DateTime.Now.Millisecond); // Use time-based seed for variability.

        CreateDefaultMaterial(); // Ensure the default material is ready.

        int baseSeedForBatch = (seed != 0) ? seed : Random.Range(int.MinValue, int.MaxValue);

        for (int i = 0; i < numberOfAsteroids; i++)
        {
            int asteroidSpecificSeed = baseSeedForBatch + i * 37; // Unique, deterministic seed per asteroid.
            Random.InitState(asteroidSpecificSeed);

            GameObject asteroid = new GameObject("Asteroid_" + i);
            generatedAsteroids.Add(asteroid);

            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            asteroid.transform.position = spawnPosition;
            asteroid.transform.rotation = Random.rotation; // Random initial orientation.
            asteroid.transform.SetParent(transform); // Parent to this generator object.
            asteroid.tag = "Asteroid"; // Tag for collision detection.

            MeshFilter meshFilter = asteroid.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = asteroid.AddComponent<MeshRenderer>();

            float currentGeneratedRadius = Random.Range(minRadius, maxRadius);
            Vector3 currentNoiseOffset = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));

            Mesh tempMeshSource = CreateAsteroidMesh(
                currentGeneratedRadius,
                subdivisions,
                irregularity,
                deformationStrength,
                currentNoiseOffset,
                "Source"
            );

            Mesh meshInstanceForAsteroid = Instantiate(tempMeshSource); // Create a unique instance of the mesh.
            meshInstanceForAsteroid.name = "Mesh_Instance_" + i;
            generatedMeshes.Add(meshInstanceForAsteroid);
            meshFilter.mesh = meshInstanceForAsteroid;
            DestroyImmediateSafe(tempMeshSource); // Clean up the temporary source mesh.

            SphereCollider asteroidCollider = asteroid.AddComponent<SphereCollider>();
            asteroidCollider.isTrigger = false; // Asteroids should have solid collisions.
            // Adjust collider radius based on the generated mesh size, with a small buffer.
            asteroidCollider.radius = currentGeneratedRadius * (1f + (irregularity * (maxScaleFactor - 1f) / 2f)) * 0.9f; // Approximate average radius after irregularity.

            Material materialInstanceForAsteroid = new Material(defaultMaterialInstance);
            materialInstanceForAsteroid.name = "Material_Instance_" + i;
            generatedMaterialInstances.Add(materialInstanceForAsteroid);

            if (useRandomGrayColor)
            {
                byte gray = (byte)Random.Range(minGrayValue, maxGrayValue + 1);
                Color randomColor = new Color32(gray, gray, gray, 255);
                if (materialInstanceForAsteroid.HasProperty("_BaseColor")) // URP Lit
                    materialInstanceForAsteroid.SetColor("_BaseColor", randomColor);
                else if (materialInstanceForAsteroid.HasProperty("_Color")) // Standard Shader
                    materialInstanceForAsteroid.SetColor("_Color", randomColor);
            }
            meshRenderer.material = materialInstanceForAsteroid;

            asteroid.AddComponent<Rigidbody>(); // Required by AsteroidAnimator.
            asteroid.AddComponent<AsteroidAnimator>(); // Handles movement and rotation.
        }
        Debug.Log($"{numberOfAsteroids} asteroids generated.");
    }

    /// <summary>
    /// Destroys all asteroid GameObjects previously generated by this script instance and cleans up associated mesh/material assets.
    /// This method can be called from the Inspector context menu.
    /// </summary>
    [ContextMenu("Clear Generated Asteroids")]
    public void ClearAsteroids()
    {
        foreach (var asteroid in generatedAsteroids)
            DestroyImmediateSafe(asteroid);
        generatedAsteroids.Clear();

        ClearGeneratedData(); // Clean up meshes and materials.
        Debug.Log("Generated asteroids and associated assets cleared.");
    }

    /// <summary>
    /// Cleans up dynamically generated mesh and material instances that were created by this script.
    /// This is crucial for preventing memory leaks, especially when working in the Unity editor.
    /// </summary>
    private void ClearGeneratedData()
    {
        foreach (var mesh in generatedMeshes)
            DestroyImmediateSafe(mesh);
        generatedMeshes.Clear();

        foreach (var mat in generatedMaterialInstances)
            DestroyImmediateSafe(mat);
        generatedMaterialInstances.Clear();
    }

    /// <summary>
    /// Safely destroys a Unity Object, using <see cref="Object.DestroyImmediate(Object)"/> in the editor (when not playing)
    /// and <see cref="Object.Destroy(Object)"/> during play mode.
    /// </summary>
    /// <param name="obj">The Unity Object to destroy.</param>
    private void DestroyImmediateSafe(Object obj)
    {
        if (obj == null) return;

        if (Application.isEditor && !Application.isPlaying)
            DestroyImmediate(obj);
        else
            Destroy(obj);
    }

    #endregion

    #region Material Handling

    /// <summary>
    /// Creates and caches the <see cref="defaultMaterialInstance"/> if it doesn't already exist.
    /// Attempts to use the URP Lit shader, falling back to the Standard shader if Lit is not found.
    /// Sets default color and PBR properties for a basic asteroid look.
    /// </summary>
    void CreateDefaultMaterial()
    {
        if (defaultMaterialInstance != null) return;

        Shader shaderToUse = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shaderToUse == null)
        {
            Debug.LogError("Could not find URP Lit or Standard shader for default asteroid material.");
            return;
        }

        defaultMaterialInstance = new Material(shaderToUse);
        Color midGray = new Color32(128, 128, 128, 255);

        if (defaultMaterialInstance.HasProperty("_BaseColor"))
            defaultMaterialInstance.SetColor("_BaseColor", midGray);
        else if (defaultMaterialInstance.HasProperty("_Color"))
            defaultMaterialInstance.SetColor("_Color", midGray);

        if (defaultMaterialInstance.HasProperty("_Metallic"))
            defaultMaterialInstance.SetFloat("_Metallic", 0.1f); // Low metallic
        if (defaultMaterialInstance.HasProperty("_Smoothness")) // URP Lit
            defaultMaterialInstance.SetFloat("_Smoothness", 0.1f); // Low smoothness
        else if (defaultMaterialInstance.HasProperty("_Glossiness")) // Standard Shader
            defaultMaterialInstance.SetFloat("_Glossiness", 0.1f); // Low glossiness
    }

    #endregion

    #region Mesh Creation Methods

    /// <summary>
    /// Creates the procedural mesh for a single asteroid based on the provided parameters.
    /// Starts with a base icosphere, subdivides it, applies irregular scaling, and deforms it using FBM noise.
    /// The resulting mesh is flat-shaded.
    /// </summary>
    /// <param name="radius">Base radius of the asteroid before scaling and deformation.</param>
    /// <param name="subdivisionSteps">Number of times the base icosphere is subdivided.</param>
    /// <param name="irregularityFactor">Factor (0-1) for random scaling applied to the base shape.</param>
    /// <param name="noiseDeformationStrength">Strength of the FBM noise displacement.</param>
    /// <param name="noiseSeedOffset">Unique offset for the noise function to vary asteroid appearances.</param>
    /// <param name="meshNameSuffix">Suffix to add to the generated mesh's name for easier identification.</param>
    /// <returns>The generated procedural <see cref="Mesh"/> for the asteroid, or null if an error occurred.</returns>
    private Mesh CreateAsteroidMesh(float radius, int subdivisionSteps, float irregularityFactor, float noiseDeformationStrength, Vector3 noiseSeedOffset, string meshNameSuffix)
    {
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        Dictionary<long, int> midpointVertexCache = new Dictionary<long, int>();

        CreateBaseIcosphere(radius, meshVertices, meshTriangles);

        for (int i = 0; i < subdivisionSteps; i++)
            SubdivideMesh(radius, meshVertices, meshTriangles, midpointVertexCache);

        List<Vector3> verticesAfterScaling = ApplyIrregularScaling(meshVertices, irregularityFactor);

        List<Vector3> finalDeformedVertices = verticesAfterScaling;
        if (noiseDeformationStrength > 0.001f) // Apply deformation only if strength is significant
            finalDeformedVertices = DeformVertices(verticesAfterScaling, noiseSeedOffset, radius, noiseDeformationStrength);

        Mesh asteroidMesh = new Mesh();
        asteroidMesh.name = $"Asteroid_{meshNameSuffix}";

        GenerateFlatShadedMesh(asteroidMesh, finalDeformedVertices, meshTriangles);
        asteroidMesh.RecalculateBounds(); // Essential for rendering and culling.
        return asteroidMesh;
    }

    /// <summary>
    /// Creates the 12 vertices and 20 triangles for a base Icosphere of a given radius.
    /// The icosphere serves as the starting point for the asteroid mesh.
    /// </summary>
    /// <param name="radius">The desired radius of the icosphere.</param>
    /// <param name="outVertices">List to be populated with the icosphere's vertex positions.</param>
    /// <param name="outTriangles">List to be populated with the icosphere's triangle indices.</param>
    private void CreateBaseIcosphere(float radius, List<Vector3> outVertices, List<int> outTriangles)
    {
        outVertices.Clear();
        outTriangles.Clear();

        float phi = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f; // Golden ratio

        // Add 12 vertices of an icosahedron
        AddVertex(new Vector3(-1, phi, 0).normalized * radius, outVertices);
        AddVertex(new Vector3(1, phi, 0).normalized * radius, outVertices);
        AddVertex(new Vector3(-1, -phi, 0).normalized * radius, outVertices);
        AddVertex(new Vector3(1, -phi, 0).normalized * radius, outVertices);
        AddVertex(new Vector3(0, -1, phi).normalized * radius, outVertices);
        AddVertex(new Vector3(0, 1, phi).normalized * radius, outVertices);
        AddVertex(new Vector3(0, -1, -phi).normalized * radius, outVertices);
        AddVertex(new Vector3(0, 1, -phi).normalized * radius, outVertices);
        AddVertex(new Vector3(phi, 0, -1).normalized * radius, outVertices);
        AddVertex(new Vector3(phi, 0, 1).normalized * radius, outVertices);
        AddVertex(new Vector3(-phi, 0, -1).normalized * radius, outVertices);
        AddVertex(new Vector3(-phi, 0, 1).normalized * radius, outVertices);

        // Add 20 triangles of the icosahedron (counter-clockwise winding)
        AddTriangle(0, 11, 5, outTriangles); AddTriangle(0, 5, 1, outTriangles);
        AddTriangle(0, 1, 7, outTriangles); AddTriangle(0, 7, 10, outTriangles);
        AddTriangle(0, 10, 11, outTriangles); AddTriangle(1, 5, 9, outTriangles);
        AddTriangle(5, 11, 4, outTriangles); AddTriangle(11, 10, 2, outTriangles);
        AddTriangle(10, 7, 6, outTriangles); AddTriangle(7, 1, 8, outTriangles);
        AddTriangle(3, 9, 4, outTriangles); AddTriangle(3, 4, 2, outTriangles);
        AddTriangle(3, 2, 6, outTriangles); AddTriangle(3, 6, 8, outTriangles);
        AddTriangle(3, 8, 9, outTriangles); AddTriangle(4, 9, 5, outTriangles);
        AddTriangle(2, 4, 11, outTriangles); AddTriangle(6, 2, 10, outTriangles);
        AddTriangle(8, 6, 7, outTriangles); AddTriangle(9, 8, 1, outTriangles);
    }

    /// <summary>
    /// Subdivides each triangle in the current mesh into four smaller triangles.
    /// New vertices created at edge midpoints are projected onto the sphere of the given radius.
    /// </summary>
    /// <param name="radius">The target radius for normalizing new midpoint vertices.</param>
    /// <param name="verticesList">List of current vertices; new vertices will be added to this list.</param>
    /// <param name="trianglesList">List of current triangles; this list will be cleared and replaced with new subdivided triangles.</param>
    /// <param name="midpointCache">A cache to store and retrieve indices of already created midpoint vertices to avoid duplicates.</param>
    private void SubdivideMesh(float radius, List<Vector3> verticesList, List<int> trianglesList, Dictionary<long, int> midpointCache)
    {
        List<int> newSubdividedTriangles = new List<int>();
        midpointCache.Clear();
        int originalTriangleCount = trianglesList.Count;

        for (int i = 0; i < originalTriangleCount; i += 3)
        {
            if (i + 2 >= trianglesList.Count) break;

            int v1Idx = trianglesList[i];
            int v2Idx = trianglesList[i + 1];
            int v3Idx = trianglesList[i + 2];

            if (v1Idx >= verticesList.Count || v2Idx >= verticesList.Count || v3Idx >= verticesList.Count || v1Idx < 0 || v2Idx < 0 || v3Idx < 0) continue;

            int m12Idx = GetMidpointIndex(v1Idx, v2Idx, radius, verticesList, midpointCache);
            int m23Idx = GetMidpointIndex(v2Idx, v3Idx, radius, verticesList, midpointCache);
            int m31Idx = GetMidpointIndex(v3Idx, v1Idx, radius, verticesList, midpointCache);

            if (m12Idx < 0 || m23Idx < 0 || m31Idx < 0) continue;

            AddTriangle(v1Idx, m12Idx, m31Idx, newSubdividedTriangles);
            AddTriangle(v2Idx, m23Idx, m12Idx, newSubdividedTriangles);
            AddTriangle(v3Idx, m31Idx, m23Idx, newSubdividedTriangles);
            AddTriangle(m12Idx, m23Idx, m31Idx, newSubdividedTriangles);
        }
        trianglesList.Clear();
        trianglesList.AddRange(newSubdividedTriangles);
    }

    /// <summary>
    /// Generates mesh data (vertices, triangles, normals) for flat shading.
    /// This involves duplicating vertices for each triangle face so each face can have its own distinct normal.
    /// </summary>
    /// <param name="targetMesh">The <see cref="Mesh"/> object to populate with flat-shaded data.</param>
    /// <param name="sourceVertices">List of unique vertex positions after all deformations.</param>
    /// <param name="sourceTriangles">List of triangle indices referencing the <paramref name="sourceVertices"/> list.</param>
    private void GenerateFlatShadedMesh(Mesh targetMesh, List<Vector3> sourceVertices, List<int> sourceTriangles)
    {
        if (sourceVertices == null || !sourceVertices.Any() || sourceTriangles == null || !sourceTriangles.Any()) return;

        List<Vector3> flatShadedVertices = new List<Vector3>();
        List<int> flatShadedTriangles = new List<int>();
        List<Vector3> flatShadedNormals = new List<Vector3>();

        for (int i = 0; i < sourceTriangles.Count; i += 3)
        {
            if (i + 2 >= sourceTriangles.Count) break;

            int idx1 = sourceTriangles[i];
            int idx2 = sourceTriangles[i + 1];
            int idx3 = sourceTriangles[i + 2];

            if (idx1 < 0 || idx2 < 0 || idx3 < 0 || idx1 >= sourceVertices.Count || idx2 >= sourceVertices.Count || idx3 >= sourceVertices.Count) continue;

            Vector3 v1Pos = sourceVertices[idx1];
            Vector3 v2Pos = sourceVertices[idx2];
            Vector3 v3Pos = sourceVertices[idx3];

            Vector3 faceNormal = Vector3.Cross(v2Pos - v1Pos, v3Pos - v1Pos).normalized;
            if (float.IsNaN(faceNormal.x) || faceNormal.sqrMagnitude < 0.0001f)
            {
                faceNormal = (v1Pos + v2Pos + v3Pos).normalized; // Fallback normal
                if (float.IsNaN(faceNormal.x) || faceNormal.sqrMagnitude < 0.0001f)
                    faceNormal = Vector3.up; // Final fallback
            }

            int currentBaseIndex = flatShadedVertices.Count;
            flatShadedVertices.Add(v1Pos);
            flatShadedVertices.Add(v2Pos);
            flatShadedVertices.Add(v3Pos);

            flatShadedTriangles.Add(currentBaseIndex);
            flatShadedTriangles.Add(currentBaseIndex + 1);
            flatShadedTriangles.Add(currentBaseIndex + 2);

            flatShadedNormals.Add(faceNormal);
            flatShadedNormals.Add(faceNormal);
            flatShadedNormals.Add(faceNormal);
        }

        if (!flatShadedVertices.Any()) return;

        targetMesh.Clear(); // Clear any previous data.
        targetMesh.vertices = flatShadedVertices.ToArray();
        targetMesh.triangles = flatShadedTriangles.ToArray();
        targetMesh.normals = flatShadedNormals.ToArray();
        // UVs are not explicitly generated for flat shading here; could be added if needed.
    }

    /// <summary>
    /// Applies non-uniform random scaling to a list of vertices.
    /// The degree of scaling is controlled by the <paramref name="irregularityAmount"/> factor.
    /// </summary>
    /// <param name="verticesToScale">The input list of vertex positions.</param>
    /// <param name="irregularityAmount">Factor (0-1) determining how much of the random scaling (defined by min/max scale factors) is applied. 0 means no scaling, 1 means full random scaling.</param>
    /// <returns>A new list containing the scaled vertex positions.</returns>
    private List<Vector3> ApplyIrregularScaling(List<Vector3> verticesToScale, float irregularityAmount)
    {
        if (verticesToScale == null || !verticesToScale.Any()) return new List<Vector3>();

        float scaleX = Random.Range(minScaleFactor, maxScaleFactor);
        float scaleY = Random.Range(minScaleFactor, maxScaleFactor);
        float scaleZ = Random.Range(minScaleFactor, maxScaleFactor);
        Vector3 randomScaleVector = new Vector3(scaleX, scaleY, scaleZ);

        Vector3 finalScale = Vector3.Lerp(Vector3.one, randomScaleVector, irregularityAmount);

        List<Vector3> resultVertices = new List<Vector3>(verticesToScale.Count);
        for (int i = 0; i < verticesToScale.Count; i++)
            resultVertices.Add(Vector3.Scale(verticesToScale[i], finalScale));
        return resultVertices;
    }

    /// <summary>
    /// Deforms vertex positions along their normalized direction (from origin) using Fractional Brownian Motion (FBM) noise.
    /// </summary>
    /// <param name="verticesToDeform">The list of vertex positions to deform.</param>
    /// <param name="noiseBaseOffset">A unique 3D offset for the noise calculation, allowing different asteroids to have different deformation patterns even with the same base shape.</param>
    /// <param name="shapeBaseRadius">The original base radius of the shape; used to scale the displacement appropriately.</param>
    /// <param name="strengthOfDeformation">Multiplier for the noise displacement, controlling the intensity of the deformation.</param>
    /// <returns>A new list containing the deformed vertex positions.</returns>
    private List<Vector3> DeformVertices(List<Vector3> verticesToDeform, Vector3 noiseBaseOffset, float shapeBaseRadius, float strengthOfDeformation)
    {
        if (verticesToDeform == null || !verticesToDeform.Any()) return new List<Vector3>();

        List<Vector3> resultVertices = new List<Vector3>(verticesToDeform.Count);
        for (int i = 0; i < verticesToDeform.Count; i++)
        {
            Vector3 currentVertex = verticesToDeform[i];
            Vector3 deformationDirection = currentVertex.normalized;
            if (deformationDirection.sqrMagnitude < 0.0001f) deformationDirection = Vector3.up; // Fallback for vertex at origin.

            float noiseVal = FBMNoise(
                (currentVertex.x + noiseBaseOffset.x) * noiseScale,
                (currentVertex.y + noiseBaseOffset.y) * noiseScale,
                (currentVertex.z + noiseBaseOffset.z) * noiseScale,
                octaves, persistence, lacunarity
            );
            float displacementAmount = noiseVal * strengthOfDeformation * shapeBaseRadius;
            resultVertices.Add(currentVertex + deformationDirection * displacementAmount);
        }
        return resultVertices;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Adds a vertex position to the given list and returns its index in that list.
    /// </summary>
    /// <param name="vertexPosition">The <see cref="Vector3"/> position of the vertex to add.</param>
    /// <param name="vertexCollection">The list to which the vertex will be added.</param>
    /// <returns>The index of the newly added vertex within <paramref name="vertexCollection"/>.</returns>
    private int AddVertex(Vector3 vertexPosition, List<Vector3> vertexCollection)
    {
        vertexCollection.Add(vertexPosition);
        return vertexCollection.Count - 1;
    }

    /// <summary>
    /// Adds three vertex indices to the given list, defining a single triangle.
    /// </summary>
    /// <param name="vIndex1">Index of the first vertex of the triangle.</param>
    /// <param name="vIndex2">Index of the second vertex of the triangle.</param>
    /// <param name="vIndex3">Index of the third vertex of the triangle.</param>
    /// <param name="triangleCollection">The list to which the triangle indices will be added.</param>
    private void AddTriangle(int vIndex1, int vIndex2, int vIndex3, List<int> triangleCollection)
    {
        triangleCollection.Add(vIndex1);
        triangleCollection.Add(vIndex2);
        triangleCollection.Add(vIndex3);
    }

    /// <summary>
    /// Calculates (or retrieves from cache) the index of the vertex at the midpoint of an edge defined by two vertex indices.
    /// The new midpoint vertex is normalized to the given radius to maintain a spherical base shape during subdivision.
    /// </summary>
    /// <param name="vertex1Index">Index of the first vertex of the edge.</param>
    /// <param name="vertex2Index">Index of the second vertex of the edge.</param>
    /// <param name="targetRadius">The radius to which the new midpoint vertex will be normalized.</param>
    /// <param name="allVertices">The list containing all vertex positions; a new vertex may be added to this list.</param>
    /// <param name="midpointIndexCache">A dictionary used to cache and reuse indices of already created midpoints, preventing duplicate vertices.</param>
    /// <returns>The index of the midpoint vertex in the <paramref name="allVertices"/> list, or -1 if an error occurs (e.g., invalid input indices).</returns>
    private int GetMidpointIndex(int vertex1Index, int vertex2Index, float targetRadius, List<Vector3> allVertices, Dictionary<long, int> midpointIndexCache)
    {
        long smallerIdx = Mathf.Min(vertex1Index, vertex2Index);
        long greaterIdx = Mathf.Max(vertex1Index, vertex2Index);
        long edgeKey = (smallerIdx << 32) + greaterIdx; // Unique key for the edge.

        if (midpointIndexCache.TryGetValue(edgeKey, out int cachedIndex))
            return cachedIndex;

        if (vertex1Index < 0 || vertex2Index < 0 || vertex1Index >= allVertices.Count || vertex2Index >= allVertices.Count) return -1; // Invalid index.

        Vector3 p1 = allVertices[vertex1Index];
        Vector3 p2 = allVertices[vertex2Index];
        Vector3 midPointPosition = (p1 + p2) / 2f;
        Vector3 normalizedMidpointPos = midPointPosition.normalized;

        if (normalizedMidpointPos.sqrMagnitude < 0.0001f) // Handle case where midpoint is at origin
        {
            normalizedMidpointPos = p1.normalized; // Fallback to one of the edge points
            if (normalizedMidpointPos.sqrMagnitude < 0.0001f) normalizedMidpointPos = Vector3.right; // Final fallback
        }

        int newMidpointIndex = AddVertex(normalizedMidpointPos * targetRadius, allVertices);
        midpointIndexCache.Add(edgeKey, newMidpointIndex);
        return newMidpointIndex;
    }
    #endregion

    #region Noise Functions (FBM)

    /// <summary>
    /// Calculates a 3D Fractional Brownian Motion (FBM) noise value.
    /// FBM is achieved by summing multiple layers (octaves) of Perlin noise,
    /// each with increasing frequency and decreasing amplitude.
    /// </summary>
    /// <param name="x">X coordinate for noise evaluation.</param>
    /// <param name="y">Y coordinate for noise evaluation.</param>
    /// <param name="z">Z coordinate for noise evaluation.</param>
    /// <param name="octaveCount">The number of Perlin noise layers (octaves) to sum.</param>
    /// <param name="persistenceValue">The multiplier for how much the amplitude of each successive octave decreases (typically 0-1).</param>
    /// <param name="lacunarityValue">The multiplier for how much the frequency of each successive octave increases (typically >1).</param>
    /// <returns>The FBM noise value, typically normalized to a range like [-1, 1].</returns>
    private float FBMNoise(float x, float y, float z, int octaveCount, float persistenceValue, float lacunarityValue)
    {
        float totalNoise = 0;
        float currentFrequency = 1;
        float currentAmplitude = 1;
        float maxPossibleAmplitude = 0; // For normalization

        for (int i = 0; i < octaveCount; i++)
        {
            // PerlinNoise3D returns [0,1], so map to [-1,1]
            float perlinSample = PerlinNoise3D(x * currentFrequency, y * currentFrequency, z * currentFrequency) * 2f - 1f;
            totalNoise += perlinSample * currentAmplitude;
            maxPossibleAmplitude += currentAmplitude;

            currentAmplitude *= persistenceValue;
            currentFrequency *= lacunarityValue;
        }
        return (maxPossibleAmplitude == 0) ? 0 : totalNoise / maxPossibleAmplitude; // Normalize
    }

    /// <summary>
    /// Approximates 3D Perlin noise by averaging 2D Perlin noise samples from three orthogonal planes (XY, YZ, XZ) and their converses.
    /// Unity's <see cref="Mathf.PerlinNoise(float, float)"/> is only 2D.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="z">Z coordinate.</param>
    /// <returns>An approximate 3D Perlin noise value in the range [0, 1].</returns>
    private static float PerlinNoise3D(float x, float y, float z)
    {
        float xyNoise = Mathf.PerlinNoise(x, y);
        float yzNoise = Mathf.PerlinNoise(y, z);
        float xzNoise = Mathf.PerlinNoise(x, z);
        float yxNoise = Mathf.PerlinNoise(y, x); // Converse planes
        float zyNoise = Mathf.PerlinNoise(z, y);
        float zxNoise = Mathf.PerlinNoise(z, x);
        return (xyNoise + yzNoise + xzNoise + yxNoise + zyNoise + zxNoise) / 6f; // Average the samples
    }
    #endregion
}