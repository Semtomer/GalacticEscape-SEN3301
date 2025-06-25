using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a spaceship mesh procedurally based on parameters defined in the Inspector.
/// Creates individual GameObjects for each part (hull, wings, engine, etc.) as children.
/// Handles material creation and assignment for each part.
/// </summary>
public class SpaceshipGenerator : MonoBehaviour
{
    #region Variables

    // All serialized fields below are adjustable in the Unity Inspector.
    // They control the visual properties (size, position, rotation, color) of each spaceship part.

    // --- Nose Section ---
    [Header("Nose Details")]
    [Tooltip("Length of the nose cone along its local Z-axis.")]
    [SerializeField] private float noseDepth = 5.0f;           // Length of the nose cone
    [Tooltip("Width of the nose cone at its base (connection point).")]
    [SerializeField] private float noseBackWidth = 1.6f;       // Width at base of nose
    [Tooltip("Height of the nose cone at its base.")]
    [SerializeField] private float noseBackHeight = 1.2f;      // Height at base of nose
    [Tooltip("Width of the nose cone at its tip.")]
    [SerializeField] private float noseFrontWidth = 0.1f;      // Width at tip of nose
    [Tooltip("Height of the nose cone at its tip.")]
    [SerializeField] private float noseFrontHeight = 0.1f;     // Height at tip of nose
    [Tooltip("Local position of the nose relative to the parent GameObject.")]
    [SerializeField] private Vector3 nosePosition = new Vector3(0, -0.1707f, 0.919f); // Local position
    [Tooltip("Local rotation (Euler angles) of the nose.")]
    [SerializeField] private Vector3 noseRotation = new Vector3(-173.7f, 0, 0);      // Local rotation
    [Tooltip("Base color for the nose material.")]
    [SerializeField] private Color noseColor = new Color32(255, 255, 255, 255);         // Base color
    /// <summary>Internal material instance created for the nose part.</summary>
    private Material noseMaterial;  // Material instance for nose


    // --- Hull Section ---
    [Header("Hull Details")]
    [Tooltip("Width of the main body/hull.")]
    [SerializeField] private float hullWidth = 1.6f;           // Main body width
    [Tooltip("Height of the main body/hull.")]
    [SerializeField] private float hullHeight = 1.2f;          // Main body height
    [Tooltip("Length (depth) of the main body/hull.")]
    [SerializeField] private float hullDepth = 2.0f;           // Main body length
    [Tooltip("Local position of the hull.")]
    [SerializeField] private Vector3 hullPosition = new Vector3(0, 0.1f, -2.5f);
    [Tooltip("Local rotation (Euler angles) of the hull.")]
    [SerializeField] private Vector3 hullRotation = new Vector3(0, 0, 0);
    [Tooltip("Base color for the hull material.")]
    [SerializeField] private Color hullColor = new Color32(255, 255, 255, 255);
    /// <summary>Internal material instance created for the hull part.</summary>
    private Material hullMaterial;


    // --- Cockpit Glass Section ---
    [Header("Cockpit Glass Details")]
    [Tooltip("Width of the cockpit windshield.")]
    [SerializeField] private float cockpitGlassWidth = 1.0f;    // Windshield dimensions
    [Tooltip("Height of the cockpit windshield.")]
    [SerializeField] private float cockpitGlassHeight = 0.1f;
    [Tooltip("Length (depth) of the cockpit windshield.")]
    [SerializeField] private float cockpitGlassDepth = 1.2f;
    [Tooltip("Local position of the cockpit glass.")]
    [SerializeField] private Vector3 cockpitGlassPosition = new Vector3(0, 0.5825f, -0.75f);
    [Tooltip("Local rotation (Euler angles) of the cockpit glass.")]
    [SerializeField] private Vector3 cockpitGlassRotation = new Vector3(12.65f, 0, 0);
    [Tooltip("Base color for the cockpit glass material (often dark/black).")]
    [SerializeField] private Color cockpitGlassColor = new Color32(0, 0, 0, 255); // Typically dark/black
    /// <summary>Internal material instance created for the cockpit glass.</summary>
    private Material cockpitGlassMaterial;


    // --- Wing Section ---
    [Header("Wing Details")]
    [Tooltip("Length (depth) of the wings.")]
    [SerializeField] private float wingDepth = 4f;             // Wing length
    [Tooltip("Width of the wing where it connects to the hull (root).")]
    [SerializeField] private float wingBackWidth = 0.2f;       // Width at wing root
    [Tooltip("Height/Thickness of the wing at the root.")]
    [SerializeField] private float wingBackHeight = 2.05f;     // Height at wing root
    [Tooltip("Width of the wing at its outer tip.")]
    [SerializeField] private float wingFrontWidth = 0.05f;     // Width at wing tip
    [Tooltip("Height/Thickness of the wing at its outer tip.")]
    [SerializeField] private float wingFrontHeight = 0.1f;     // Height at wing tip
    [Tooltip("Local position of the right wing.")]
    [SerializeField] private Vector3 rightWingPosition = new Vector3(2.49f, 0f, -2.985f);
    [Tooltip("Local rotation (Euler angles) of the right wing.")]
    [SerializeField] private Vector3 rightWingRotation = new Vector3(0f, -76f, -90f);
    [Tooltip("Local position of the left wing.")]
    [SerializeField] private Vector3 leftWingPosition = new Vector3(-2.49f, 0f, -2.985f);
    [Tooltip("Local rotation (Euler angles) of the left wing.")]
    [SerializeField] private Vector3 leftWingRotation = new Vector3(0f, 76f, 90f);
    [Tooltip("Base color for the wing material.")]
    [SerializeField] private Color wingColor = new Color32(255, 255, 255, 255);
    /// <summary>Internal material instance created for the wings.</summary>
    private Material wingMaterial;


    // --- Wing Tip Glow Section (Shared Material) ---
    [Header("General Wing Tip Glow Details")]
    [Tooltip("Color for the glowing elements on the wing tips.")]
    [SerializeField] private Color wingTipGlowColor = new Color32(0, 255, 255, 255); // Cyan glow
    /// <summary>Internal emissive material instance used for all wing tip glow parts.</summary>
    private Material wingTipGlowMaterial;


    // --- Wing Tip Glow Part 1 ---
    [Header("Wing Tip Glow 1 Details")]
    [Tooltip("Width of the first glowing element on the wing tip.")]
    [SerializeField] private float wingTipGlow1Width = 0.1f;   // First glow element dimensions
    [Tooltip("Height of the first glowing element.")]
    [SerializeField] private float wingTipGlow1Height = 0.2f;
    [Tooltip("Depth/Length of the first glowing element.")]
    [SerializeField] private float wingTipGlow1Depth = 0.1f;
    [Tooltip("Local position of the first glow element relative *to the wing*")]
    [SerializeField] private Vector3 wingTipGlow1LocalPosition = new Vector3(0, -0.75f, 0.85f);
    [Tooltip("Local rotation of the first glow element relative *to the wing*")]
    [SerializeField] private Vector3 wingTipGlow1LocalRotation = new Vector3(104, 0, 90);


    // --- Wing Tip Glow Part 2 ---
    [Header("Wing Tip Glow 2 Details")]
    [Tooltip("Width of the second glowing element on the wing tip.")]
    [SerializeField] private float wingTipGlow2Width = 0.2f;   // Second glow element dimensions
    [Tooltip("Height of the second glowing element.")]
    [SerializeField] private float wingTipGlow2Height = 0.06f;
    [Tooltip("Depth/Length of the second glowing element.")]
    [SerializeField] private float wingTipGlow2Depth = 0.1f;
    [Tooltip("Local position of the second glow element relative *to the wing*")]
    [SerializeField] private Vector3 wingTipGlow2LocalPosition = new Vector3(0, -0.0222f, -1.915f);
    [Tooltip("Local rotation of the second glow element relative *to the wing*")]
    [SerializeField] private Vector3 wingTipGlow2LocalRotation = new Vector3(104, 0, 90);


    // --- Tail Section ---
    [Header("Tail Details")]
    [Tooltip("Length (depth) of the tail fin/stabilizer.")]
    [SerializeField] private float tailDepth = 1.5f;           // Vertical stabilizer dimensions
    [Tooltip("Width of the tail fin at its base.")]
    [SerializeField] private float tailBackWidth = 0.2f;
    [Tooltip("Height/Thickness of the tail fin at its base.")]
    [SerializeField] private float tailBackHeight = 1f;
    [Tooltip("Width of the tail fin at its tip.")]
    [SerializeField] private float tailFrontWidth = 0.05f;
    [Tooltip("Height/Thickness of the tail fin at its tip.")]
    [SerializeField] private float tailFrontHeight = 0.25f;
    [Tooltip("Local position of the tail fin.")]
    [SerializeField] private Vector3 tailPosition = new Vector3(0, 1.29f, -3.2f);
    [Tooltip("Local rotation (Euler angles) of the tail fin.")]
    [SerializeField] private Vector3 tailRotation = new Vector3(75f, 0, 0);
    [Tooltip("Base color for the tail material.")]
    [SerializeField] private Color tailColor = new Color32(255, 255, 255, 255);
    /// <summary>Internal material instance created for the tail fin.</summary>
    private Material tailMaterial;


    // --- Engine Section ---
    [Header("Engine Details")]
    [Tooltip("Radius of the main engine cylinder.")]
    [SerializeField] private float engineRadius = 0.7f;        // Main engine dimensions
    [Tooltip("Height/Length of the main engine cylinder.")]
    [SerializeField] private float engineHeight = 0.6f;
    [Tooltip("Number of segments used to create the engine cylinder (more = smoother).")]
    [SerializeField] private int engineCylinderSegments = 16;  // Cylinder smoothness
    [Tooltip("Local position of the engine.")]
    [SerializeField] private Vector3 enginePosition = new Vector3(0, 0.1f, -3.8f);
    [Tooltip("Local rotation (Euler angles) of the engine.")]
    [SerializeField] private Vector3 engineRotation = new Vector3(180f, 0, 0);
    [Tooltip("Base color for the engine material (e.g., gray).")]
    [SerializeField] private Color engineColor = new Color32(128, 128, 128, 255);    // Gray
    [Tooltip("Color for the engine's glow effect.")]
    [SerializeField] private Color engineGlowColor = new Color32(0, 255, 255, 255);  // Cyan glow
    /// <summary>Internal material instance created for the engine body.</summary>
    private Material engineMaterial;
    /// <summary>Internal emissive material instance created for the engine glow.</summary>
    private Material engineGlowMaterial;


    // --- Guns Section ---
    [Header("Guns Details")]
    [Tooltip("Radius of the gun barrel cylinders.")]
    [SerializeField] private float gunRadius = 0.075f;         // Gun dimensions
    [Tooltip("Height/Length of the gun barrel cylinders.")]
    [SerializeField] private float gunHeight = 0.5f;
    [Tooltip("Number of segments for the gun cylinders (can be lower for performance).")]
    [SerializeField] private int gunCylinderSegments = 8;      // Lower smoothness for guns
    [Tooltip("Local position of the right nose-mounted gun.")]
    [SerializeField] private Vector3 rightNoseGunPosition = new Vector3(0.25f, -0.35f, 2.4f);
    [Tooltip("Local rotation (Euler angles) of the right nose-mounted gun.")]
    [SerializeField] private Vector3 rightNoseGunRotation = new Vector3(0, 0, 0);
    [Tooltip("Local position of the left nose-mounted gun.")]
    [SerializeField] private Vector3 leftNoseGunPosition = new Vector3(-0.25f, -0.35f, 2.4f);
    [Tooltip("Local rotation (Euler angles) of the left nose-mounted gun.")]
    [SerializeField] private Vector3 leftNoseGunRotation = new Vector3(0, 0, 0);
    [Tooltip("Local position of the right hull-mounted gun.")]
    [SerializeField] private Vector3 rightHullGunPosition = new Vector3(0.875f, 0.4f, -1.8f);
    [Tooltip("Local rotation (Euler angles) of the right hull-mounted gun.")]
    [SerializeField] private Vector3 rightHullGunRotation = new Vector3(0, 0, 0);
    [Tooltip("Local position of the left hull-mounted gun.")]
    [SerializeField] private Vector3 leftHullGunPosition = new Vector3(-0.875f, 0.4f, -1.8f);
    [Tooltip("Local rotation (Euler angles) of the left hull-mounted gun.")]
    [SerializeField] private Vector3 leftHullGunRotation = new Vector3(0, 0, 0);
    [Tooltip("Base color for the gun material (e.g., black).")]
    [SerializeField] private Color gunColor = new Color32(0, 0, 0, 255);            // Black guns
    [Tooltip("Color for the glowing gun tips.")]
    [SerializeField] private Color gunTipGlowColor = new Color32(0, 255, 255, 255); // Cyan tips
    /// <summary>Internal material instance created for the gun barrels.</summary>
    private Material gunMaterial;
    /// <summary>Internal emissive material instance created for the gun tip glows.</summary>
    private Material gunTipGlowMaterial;


    /// <summary>Cached reference to the URP Lit shader used for all materials.</summary>
    private Shader urpShader;
    #endregion

    #region Unity Method
    /// <summary>
    /// Called when the script instance is first enabled.
    /// Clears previous parts, initializes materials, and creates the spaceship mesh.
    /// </summary>
    void Start()
    {
        // --- Cleanup Phase ---
        // Remove any GameObjects previously generated by this script
        // This ensures a clean slate each time Start is called (e.g., entering Play mode).
        foreach (Transform child in transform)
        {
            // Use DestroyImmediate in the editor when not playing, Destroy otherwise.
            if (Application.isPlaying)
                Destroy(child.gameObject);    // Runtime destruction
            else
                DestroyImmediate(child.gameObject); // Editor-time destruction (important for editor updates)
        }

        // --- Initialization Phase ---
        // Find and cache the URP Lit shader. Assumes URP is being used.
        // Finding it once in Start is more efficient than finding it repeatedly.
        urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null)
        {
            return; // Stop execution if shader is missing
        }

        // Create all necessary material instances based on Inspector colors.
        AssignMaterialVariables();

        // --- Generation Phase ---
        // Call the main method to build the spaceship mesh components.
        CreateSpaceship();
    }
    #endregion

    #region Material Creation Methods

    /// <summary>
    /// Creates and assigns runtime instances of materials for each spaceship part.
    /// Called once during initialization.
    /// </summary>
    void AssignMaterialVariables()
    {
        // Create standard materials for non-glowing parts
        noseMaterial = CreateMaterial(noseColor);
        hullMaterial = CreateMaterial(hullColor);
        cockpitGlassMaterial = CreateMaterial(cockpitGlassColor);
        wingMaterial = CreateMaterial(wingColor);
        tailMaterial = CreateMaterial(tailColor);
        engineMaterial = CreateMaterial(engineColor);
        gunMaterial = CreateMaterial(gunColor);

        // Create emissive materials for glowing parts, passing emission color and intensity
        wingTipGlowMaterial = CreateMaterial(wingTipGlowColor, wingTipGlowColor, 1.5f); // Use base color also as emission color
        engineGlowMaterial = CreateMaterial(engineGlowColor, engineGlowColor, 2.0f);
        gunTipGlowMaterial = CreateMaterial(gunTipGlowColor, gunTipGlowColor, 1.5f);
    }

    /// <summary>
    /// Creates a new Material instance using the cached URP Lit shader.
    /// Optionally configures emission properties.
    /// </summary>
    /// <param name="baseColor">The primary color (_BaseColor) of the material.</param>
    /// <param name="emissionColor">Optional color for emission (_EmissionColor). If null, emission is disabled.</param>
    /// <param name="emissionIntensity">Multiplier for the emission color's brightness.</param>
    /// <returns>A configured Material instance.</returns>
    Material CreateMaterial(Color baseColor, Color? emissionColor = null, float emissionIntensity = 1.0f)
    {
        // Create a new material instance using the cached shader
        Material mat = new Material(urpShader);
        // Set the main color
        mat.SetColor("_BaseColor", baseColor);

        // Configure emission if an emission color is provided
        if (emissionColor.HasValue)
        {
            mat.EnableKeyword("_EMISSION"); // Enable the emission feature on the shader
            // Calculate the final emission color by multiplying with intensity
            // This allows controlling brightness separate from the color itself.
            Color finalEmissionColor = new Color(emissionColor.Value.r, emissionColor.Value.g, emissionColor.Value.b, 1.0f) * emissionIntensity;
            mat.SetColor("_EmissionColor", finalEmissionColor);
        }
        else // If no emission color is specified
        {
            mat.DisableKeyword("_EMISSION"); // Ensure emission is disabled
        }

        // Return the newly created and configured material
        return mat;
    }
    #endregion

    #region Main Creation Method

    /// <summary>
    /// Orchestrates the creation of all individual spaceship parts by calling
    /// helper methods to generate meshes and create GameObjects.
    /// </summary>
    void CreateSpaceship()
    {
        // --- Create Core Components ---

        // 1. Nose Cone: Create the front section using a custom frustum mesh.
        CreateCustomMeshPart(
            "Nose", // GameObject name
            CreateCustomFrustumMesh(noseFrontWidth, noseFrontHeight, noseBackWidth, noseBackHeight, noseDepth), // Procedural mesh
            nosePosition,                       // Local position
            Quaternion.Euler(noseRotation),     // Local rotation
            noseMaterial                        // Assigned material
        );

        // 2. Hull: Create the main body using a custom cube mesh.
        CreateCustomMeshPart(
            "Hull",
            CreateCustomCubeMesh(hullWidth, hullHeight, hullDepth),
            hullPosition,
            Quaternion.Euler(hullRotation),
            hullMaterial
        );

        // 3. Cockpit Glass: Create the windshield area using a custom cube mesh.
        CreateCustomMeshPart(
            "Cockpit_Glass",
            CreateCustomCubeMesh(cockpitGlassWidth, cockpitGlassHeight, cockpitGlassDepth),
            cockpitGlassPosition,
            Quaternion.Euler(cockpitGlassRotation),
            cockpitGlassMaterial
        );

        // --- Create Wings and Wing Attachments ---

        // 4. Wings with Glows: Create wing pair and attach glow elements.
        //    First, generate the meshes needed for wings and glows.
        Mesh wingMesh = CreateCustomFrustumMesh(wingFrontWidth, wingFrontHeight, wingBackWidth, wingBackHeight, wingDepth);
        Mesh wingTipGlow1Mesh = CreateCustomCubeMesh(wingTipGlow1Width, wingTipGlow1Height, wingTipGlow1Depth);
        Mesh wingTipGlow2Mesh = CreateCustomCubeMesh(wingTipGlow2Width, wingTipGlow2Height, wingTipGlow2Depth);

        //    Then, create the wing pair and attach the glows as children.
        CreateWingPairWithGlows(
            wingMesh, wingMaterial,                     // Wing mesh and material
            rightWingPosition, Quaternion.Euler(rightWingRotation), // Right wing transform
            leftWingPosition, Quaternion.Euler(leftWingRotation),   // Left wing transform
            wingTipGlow1Mesh, wingTipGlow1LocalPosition, Quaternion.Euler(wingTipGlow1LocalRotation), // Glow 1 mesh and local transform
            wingTipGlow2Mesh, wingTipGlow2LocalPosition, Quaternion.Euler(wingTipGlow2LocalRotation), // Glow 2 mesh and local transform
            wingTipGlowMaterial                         // Shared material for glows
        );

        // --- Create Rear Components ---

        // 5. Tail Fin: Create the vertical stabilizer using a custom frustum mesh.
        CreateCustomMeshPart(
            "Tail",
            CreateCustomFrustumMesh(tailFrontWidth, tailFrontHeight, tailBackWidth, tailBackHeight, tailDepth),
            tailPosition,
            Quaternion.Euler(tailRotation),
            tailMaterial
        );

        // 6. Engine: Create the main engine cylinder and its glow effect.
        CreateEngine(
            "Engine",           // GameObject name
            enginePosition,     // Local position
            Quaternion.Euler(engineRotation), // Local rotation
            engineRadius, engineHeight,       // Engine dimensions
            engineMaterial      // Engine body material (glow uses engineGlowMaterial internally)
        );

        // --- Create Weapon Systems ---

        // 7. Guns: Create pairs of guns mounted on the nose and hull.
        // a. Nose Guns (Symmetric Pair)
        CreateGunSymmetricPair(
            "Nose_Gun",         // Base name for the gun GameObjects
            rightNoseGunPosition, Quaternion.Euler(rightNoseGunRotation), // Right gun transform
            leftNoseGunPosition, Quaternion.Euler(leftNoseGunRotation),   // Left gun transform
            gunRadius, gunHeight, // Gun dimensions
            gunMaterial         // Gun material (tip glow uses gunTipGlowMaterial internally)
        );

        // b. Hull Guns (Symmetric Pair)
        CreateGunSymmetricPair(
            "Hull_Gun",
            rightHullGunPosition, Quaternion.Euler(rightHullGunRotation),
            leftHullGunPosition, Quaternion.Euler(leftHullGunRotation),
            gunRadius, gunHeight,
            gunMaterial
        );
    }
    #endregion

    #region Helper Methods for Creating Parts

    /// <summary>
    /// Creates a standard spaceship part GameObject with a MeshFilter and MeshRenderer,
    /// parenting it to this generator's transform.
    /// </summary>
    /// <param name="name">Name for the new GameObject.</param>
    /// <param name="mesh">The Mesh to assign to the MeshFilter.</param>
    /// <param name="position">Local position relative to the parent.</param>
    /// <param name="rotation">Local rotation relative to the parent.</param>
    /// <param name="material">The Material to assign to the MeshRenderer.</param>
    /// <returns>The newly created GameObject.</returns>
    GameObject CreateCustomMeshPart(string name, Mesh mesh, Vector3 position, Quaternion rotation, Material material)
    {
        // Create a new empty GameObject
        GameObject part = new GameObject(name);
        // Set its parent to this script's GameObject transform
        part.transform.parent = transform;
        // Set its local position, rotation, and scale relative to the parent
        part.transform.localPosition = position;
        part.transform.localRotation = rotation;
        part.transform.localScale = Vector3.one; // Ensure default scale

        // Add a MeshFilter component and assign the provided mesh
        MeshFilter meshFilter = part.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Add a MeshRenderer component and assign the provided material
        MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        // Return the created part GameObject
        return part;
    }

    /// <summary>
    /// Creates a mesh part GameObject as a child of a specified parent transform.
    /// Useful for adding details like glows relative to another part (e.g., wing tips).
    /// </summary>
    /// <param name="name">Name for the new GameObject.</param>
    /// <param name="mesh">The Mesh to assign.</param>
    /// <param name="parent">The Transform to parent this part to.</param>
    /// <param name="localPosition">Local position relative to the specified parent.</param>
    /// <param name="localRotation">Local rotation relative to the specified parent.</param>
    /// <param name="material">The Material to assign.</param>
    /// <returns>The newly created child GameObject.</returns>
    GameObject CreateChildMeshPart(string name, Mesh mesh, Transform parent, Vector3 localPosition, Quaternion localRotation, Material material)
    {
        // Create a new empty GameObject
        GameObject part = new GameObject(name);
        // Set its parent to the provided parent transform
        part.transform.parent = parent;
        // Set its local position, rotation, and scale relative to the new parent
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = Vector3.one;

        // Add MeshFilter and MeshRenderer components
        MeshFilter meshFilter = part.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        // Return the created child part
        return part;
    }

    /// <summary>
    /// Creates a symmetric pair of wings (left and right) and attaches specified glow elements to each wing tip.
    /// </summary>
    /// <param name="wingMesh">The mesh to use for both wings.</param>
    /// <param name="wingMaterial">The material for the wings.</param>
    /// <param name="rightWingPos">World position for the right wing.</param>
    /// <param name="rightWingRot">World rotation for the right wing.</param>
    /// <param name="leftWingPos">World position for the left wing.</param>
    /// <param name="leftWingRot">World rotation for the left wing.</param>
    /// <param name="glow1Mesh">Mesh for the first glow element.</param>
    /// <param name="glow1LocalPosition">Local position of glow 1 relative to the wing.</param>
    /// <param name="glow1LocalRotation">Local rotation of glow 1 relative to the wing.</param>
    /// <param name="glow2Mesh">Mesh for the second glow element.</param>
    /// <param name="glow2LocalPosition">Local position of glow 2 relative to the wing.</param>
    /// <param name="glow2LocalRotation">Local rotation of glow 2 relative to the wing.</param>
    /// <param name="glowMaterial">The material to use for all glow elements.</param>
    void CreateWingPairWithGlows(
        Mesh wingMesh, Material wingMaterial,
        Vector3 rightWingPos, Quaternion rightWingRot,
        Vector3 leftWingPos, Quaternion leftWingRot,
        Mesh glow1Mesh, Vector3 glow1LocalPosition, Quaternion glow1LocalRotation,
        Mesh glow2Mesh, Vector3 glow2LocalPosition, Quaternion glow2LocalRotation,
        Material glowMaterial
    )
    {
        // Create the right wing GameObject using the standard part creation method
        GameObject rightWing = CreateCustomMeshPart("Right_Wing", wingMesh, rightWingPos, rightWingRot, wingMaterial);
        // Create the left wing GameObject
        GameObject leftWing = CreateCustomMeshPart("Left_Wing", wingMesh, leftWingPos, leftWingRot, wingMaterial);

        // Add the first glow element as a child of the right wing
        CreateChildMeshPart("Wing_Tip_Glow_1", glow1Mesh, rightWing.transform, glow1LocalPosition, glow1LocalRotation, glowMaterial);
        // Add the second glow element as a child of the right wing
        CreateChildMeshPart("Wing_Tip_Glow_2", glow2Mesh, rightWing.transform, glow2LocalPosition, glow2LocalRotation, glowMaterial);

        // Add the first glow element as a child of the left wing (using the same local transforms)
        CreateChildMeshPart("Wing_Tip_Glow_1", glow1Mesh, leftWing.transform, glow1LocalPosition, glow1LocalRotation, glowMaterial);
        // Add the second glow element as a child of the left wing
        CreateChildMeshPart("Wing_Tip_Glow_2", glow2Mesh, leftWing.transform, glow2LocalPosition, glow2LocalRotation, glowMaterial);
    }

    /// <summary>
    /// Creates the main engine GameObject using a cylinder mesh and adds a glow effect.
    /// </summary>
    /// <param name="name">Name for the engine GameObject.</param>
    /// <param name="position">Local position of the engine.</param>
    /// <param name="rotation">Local rotation of the engine.</param>
    /// <param name="radius">Radius of the engine cylinder.</param>
    /// <param name="height">Height/Length of the engine cylinder.</param>
    /// <param name="material">Material for the engine body.</param>
    void CreateEngine(string name, Vector3 position, Quaternion rotation, float radius, float height, Material material)
    {
        // Create the main engine part using a custom cylinder mesh
        GameObject engine = CreateCustomMeshPart(
            name,
            CreateCustomCylinderMesh(radius, height, engineCylinderSegments), // Generate cylinder mesh
            position,
            rotation,
            material
        );

        // Add the engine glow effect as a child part
        CreateEngineGlow(
            engine.transform, // Parent the glow to the engine
            radius,           // Pass engine radius for glow sizing
            height            // Pass engine height for glow positioning
        );
    }

    /// <summary>
    /// Creates the glowing part (usually a smaller cylinder) at the back of the engine.
    /// </summary>
    /// <param name="engineTransform">The Transform of the parent engine GameObject.</param>
    /// <param name="engineRadius">The radius of the parent engine.</param>
    /// <param name="engineHeight">The height/length of the parent engine.</param>
    void CreateEngineGlow(Transform engineTransform, float engineRadius, float engineHeight)
    {
        // Define dimensions for the glow effect (slightly smaller than the engine)
        float engineGlowRadius = engineRadius * 0.9f; // Make glow radius 90% of engine radius
        float engineGlowHeight = engineHeight * 0.05f; // Make glow height 5% of engine height (a thin disk)

        // Calculate the local position for the glow at the back of the engine cylinder
        // Assumes the engine cylinder mesh is centered, so back is at +Z half-height.
        Vector3 engineGlowPosition = new Vector3(0, 0, engineHeight / 2f + engineGlowHeight / 2f); // Position just behind the engine end

        // Create the glow GameObject as a standard part (will be reparented)
        GameObject engineGlow = CreateCustomMeshPart(
            "Engine_Glow", // Name for the glow GameObject
                           // Create a cylinder mesh for the glow, potentially with fewer segments for performance
            CreateCustomCylinderMesh(engineGlowRadius, engineGlowHeight, engineCylinderSegments / 2),
            engineGlowPosition,        // Initial position (will be overridden by parenting)
            Quaternion.identity,       // Initial rotation (will be overridden by parenting)
            engineGlowMaterial         // Use the dedicated engine glow material
        );

        // Set the parent of the glow to the engine's transform
        engineGlow.transform.parent = engineTransform;
        // Reset local position and rotation relative to the new parent (engine)
        engineGlow.transform.localPosition = engineGlowPosition;
        engineGlow.transform.localRotation = Quaternion.identity; // Align glow with engine's local axes
    }

    /// <summary>
    /// Creates a single gun GameObject using a cylinder mesh and adds a glowing tip.
    /// </summary>
    /// <param name="name">Name for the gun GameObject.</param>
    /// <param name="position">Local position of the gun.</param>
    /// <param name="rotation">Local rotation of the gun.</param>
    /// <param name="radius">Radius of the gun barrel.</param>
    /// <param name="height">Height/Length of the gun barrel.</param>
    /// <param name="material">Material for the gun barrel.</param>
    void CreateGun(string name, Vector3 position, Quaternion rotation, float radius, float height, Material material)
    {
        // Create the main gun barrel part
        GameObject gun = CreateCustomMeshPart(
            name,
            CreateCustomCylinderMesh(radius, height, gunCylinderSegments), // Generate cylinder mesh for the gun
            position,
            rotation,
            material
        );

        // Add the glowing tip effect as a child part
        CreateGunTipGlow(
            gun.transform, // Parent the glow to the gun
            radius,        // Pass gun radius for sizing
            height         // Pass gun height for positioning
        );
    }

    /// <summary>
    /// Creates the glowing tip effect for a gun barrel.
    /// </summary>
    /// <param name="gunTransform">The Transform of the parent gun GameObject.</param>
    /// <param name="gunRadius">The radius of the parent gun barrel.</param>
    /// <param name="gunHeight">The height/length of the parent gun barrel.</param>
    void CreateGunTipGlow(Transform gunTransform, float gunRadius, float gunHeight)
    {
        // Define dimensions for the glow (slightly smaller than the barrel)
        float gunTipGlowRadius = gunRadius * 0.8f; // Make glow radius 80% of gun radius
        float glowHeight = gunHeight * 0.1f;     // Make glow height 10% of gun height (a small tip)

        // Calculate the local position for the glow at the very tip of the gun barrel
        // Assumes cylinder mesh is centered, so tip is at +Z half-height.
        Vector3 gunTipGlowPosition = new Vector3(0, 0, gunHeight / 2f);

        // Create the glow effect as a child part of the gun
        CreateChildMeshPart(
            "Gun_Tip_Glow", // Name for the glow GameObject
                            // Create a cylinder mesh for the glow, potentially with fewer segments
            CreateCustomCylinderMesh(gunTipGlowRadius, glowHeight, gunCylinderSegments / 2),
            gunTransform,           // Parent to the gun
            gunTipGlowPosition,     // Local position relative to the gun
            Quaternion.identity,    // Local rotation relative to the gun (aligned)
            gunTipGlowMaterial      // Use the dedicated gun tip glow material
        );
    }

    /// <summary>
    /// Creates a pair of identical guns, positioned and rotated symmetrically.
    /// </summary>
    /// <param name="namePrefix">Base name used for the gun GameObjects (e.g., "Nose_Gun").</param>
    /// <param name="rightPosition">Local position of the right gun.</param>
    /// <param name="rightRotation">Local rotation of the right gun.</param>
    /// <param name="leftPosition">Local position of the left gun.</param>
    /// <param name="leftRotation">Local rotation of the left gun.</param>
    /// <param name="radius">Radius for both gun barrels.</param>
    /// <param name="height">Height/Length for both gun barrels.</param>
    /// <param name="material">Material for both gun barrels.</param>
    void CreateGunSymmetricPair(string namePrefix, Vector3 rightPosition, Quaternion rightRotation, Vector3 leftPosition, Quaternion leftRotation, float radius, float height, Material material)
    {
        // Create the right gun by calling the single gun creation method
        CreateGun("Right_" + namePrefix, rightPosition, rightRotation, radius, height, material);

        // Create the left gun (mirrored position/rotation)
        CreateGun("Left_" + namePrefix, leftPosition, leftRotation, radius, height, material);
    }
    #endregion

    #region Mesh Creation Methods

    /// <summary>
    /// Creates a generic mesh with 6 rectangular faces based on 24 provided vertices.
    /// Assumes vertices are ordered correctly for front, back, top, bottom, right, left faces (4 vertices each).
    /// Also calculates standard normals and UVs for a basic rectangular shape.
    /// </summary>
    /// <param name="vertices">An array of exactly 24 Vector3 points defining the corners of the 6 faces.</param>
    /// <param name="meshName">A name to assign to the generated Mesh asset.</param>
    /// <returns>A configured Mesh object, or null if the vertex array is invalid.</returns>
    Mesh CreateRectangleMeshFromVertices(Vector3[] vertices, string meshName)
    {
        // Validate input vertex array size
        if (vertices == null || vertices.Length != 24)
        {
            return null; // Return null if validation fails
        }

        // Create a new Mesh object
        Mesh mesh = new Mesh();
        mesh.name = meshName; // Assign the provided name

        // --- Define Triangles ---
        // Each face is made of two triangles (quad). Indices refer to the `vertices` array.
        // The order defines the winding (counter-clockwise for visible front faces).
        int[] triangles = new int[]
        {
            // Front face (vertices 0, 1, 2, 3)
            0, 3, 2,  // Triangle 1 (BottomLeft, TopLeft, TopRight)
            0, 2, 1,  // Triangle 2 (BottomLeft, TopRight, BottomRight)
            // Back face (vertices 4, 5, 6, 7) - Reversed winding for outward normal
            4, 5, 6,  // Triangle 1 (BottomLeft, BottomRight, TopRight)
            4, 6, 7,  // Triangle 2 (BottomLeft, TopRight, TopLeft)
            // Top face (vertices 8, 9, 10, 11) - Indices 8-11 often reuse 3, 2, 6, 7
            8, 11, 10, // Triangle 1
            8, 10, 9,  // Triangle 2
            // Bottom face (vertices 12, 13, 14, 15) - Indices 12-15 often reuse 0, 4, 5, 1
            12, 15, 14, // Triangle 1 (Correct winding: FrontBottomLeft, FrontBottomRight, BackBottomRight)
            12, 14, 13, // Triangle 2 (Correct winding: FrontBottomLeft, BackBottomRight, BackBottomLeft)
            // Right face (vertices 16, 17, 18, 19) - Indices 16-19 often reuse 1, 2, 6, 5
            16, 17, 18, // Triangle 1
            16, 18, 19, // Triangle 2
            // Left face (vertices 20, 21, 22, 23) - Indices 20-23 often reuse 0, 3, 7, 4
            20, 22, 21, // Triangle 1 (Correct winding: FrontBottomLeft, BackTopLeft, FrontTopLeft)
            20, 23, 22  // Triangle 2 (Correct winding: FrontBottomLeft, BackBottomLeft, BackTopLeft)
        };

        // --- Define Normals ---
        // One normal vector per vertex. For sharp edges, vertices are duplicated,
        // so each group of 4 vertices for a face gets the same normal.
        Vector3[] normals = new Vector3[]
        {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back,             // Front face points -Z
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, // Back face points +Z
            Vector3.up, Vector3.up, Vector3.up, Vector3.up,                     // Top face points +Y
            Vector3.down, Vector3.down, Vector3.down, Vector3.down,             // Bottom face points -Y
            Vector3.right, Vector3.right, Vector3.right, Vector3.right,         // Right face points +X
            Vector3.left, Vector3.left, Vector3.left, Vector3.left              // Left face points -X
        };

        // --- Define UVs (Texture Coordinates) ---
        // One UV coordinate per vertex. Maps the texture onto each face.
        Vector2[] uvs = new Vector2[24];
        // Define standard UV corners
        Vector2 uv00 = new Vector2(0, 0); // Bottom-left
        Vector2 uv10 = new Vector2(1, 0); // Bottom-right
        Vector2 uv01 = new Vector2(0, 1); // Top-left
        Vector2 uv11 = new Vector2(1, 1); // Top-right

        // Assign UVs to each face's vertices (order matters for correct mapping)
        // The assignments below correspond to the vertex order for each face.
        uvs[0] = uv00; uvs[1] = uv10; uvs[2] = uv11; uvs[3] = uv01; // Front face UVs
        uvs[4] = uv10; uvs[5] = uv00; uvs[6] = uv01; uvs[7] = uv11; // Back face UVs (flipped horizontally)
        uvs[8] = uv01; uvs[9] = uv11; uvs[10] = uv10; uvs[11] = uv00; // Top face UVs (rotated)
        uvs[12] = uv00; uvs[13] = uv01; uvs[14] = uv11; uvs[15] = uv10; // Bottom face UVs
        uvs[16] = uv00; uvs[17] = uv01; uvs[18] = uv11; uvs[19] = uv10; // Right face UVs
        uvs[20] = uv10; uvs[21] = uv11; uvs[22] = uv01; uvs[23] = uv00; // Left face UVs (flipped horizontally)

        // --- Assign data to Mesh ---
        mesh.vertices = vertices;     // Set vertex positions
        mesh.triangles = triangles;   // Set triangle indices
        mesh.normals = normals;       // Set vertex normals
        mesh.uv = uvs;                // Set texture coordinates
        mesh.RecalculateBounds();     // Calculate the axis-aligned bounding box

        // Return the completed mesh
        return mesh;
    }

    /// <summary>
    /// Creates a mesh shaped like a frustum (a pyramid with the top cut off).
    /// Useful for shapes like nose cones or wings that taper.
    /// Builds the 24 vertices required and calls CreateRectangleMeshFromVertices.
    /// </summary>
    /// <param name="frontWidth">Width of the smaller front face.</param>
    /// <param name="frontHeight">Height of the smaller front face.</param>
    /// <param name="backWidth">Width of the larger back face.</param>
    /// <param name="backHeight">Height of the larger back face.</param>
    /// <param name="depth">Length of the frustum along its central axis.</param>
    /// <returns>A frustum-shaped Mesh object.</returns>
    Mesh CreateCustomFrustumMesh(float frontWidth, float frontHeight, float backWidth, float backHeight, float depth)
    {
        // Calculate half dimensions for easier vertex placement (centered at origin)
        float halfDepth = depth / 2f;
        float halfFrontWidth = frontWidth / 2f;
        float halfFrontHeight = frontHeight / 2f;
        float halfBackWidth = backWidth / 2f;
        float halfBackHeight = backHeight / 2f;

        // Array to hold the 24 vertices (4 for each of the 6 faces)
        Vector3[] vertices = new Vector3[24];

        // Define vertices for the Front Face (at Z = -halfDepth)
        vertices[0] = new Vector3(-halfFrontWidth, -halfFrontHeight, -halfDepth); // Bottom-Left
        vertices[1] = new Vector3(halfFrontWidth, -halfFrontHeight, -halfDepth); // Bottom-Right
        vertices[2] = new Vector3(halfFrontWidth, halfFrontHeight, -halfDepth); // Top-Right
        vertices[3] = new Vector3(-halfFrontWidth, halfFrontHeight, -halfDepth); // Top-Left

        // Define vertices for the Back Face (at Z = +halfDepth)
        vertices[4] = new Vector3(-halfBackWidth, -halfBackHeight, halfDepth); // Bottom-Left
        vertices[5] = new Vector3(halfBackWidth, -halfBackHeight, halfDepth); // Bottom-Right
        vertices[6] = new Vector3(halfBackWidth, halfBackHeight, halfDepth); // Top-Right
        vertices[7] = new Vector3(-halfBackWidth, halfBackHeight, halfDepth); // Top-Left

        // Define vertices for the Top Face (reusing vertices from front/back top edges)
        vertices[8] = vertices[3]; // Front Top-Left
        vertices[9] = vertices[2]; // Front Top-Right
        vertices[10] = vertices[6]; // Back Top-Right
        vertices[11] = vertices[7]; // Back Top-Left

        // Define vertices for the Bottom Face (reusing vertices from front/back bottom edges)
        vertices[12] = vertices[0]; // Front Bottom-Left
        vertices[13] = vertices[4]; // Back Bottom-Left
        vertices[14] = vertices[5]; // Back Bottom-Right
        vertices[15] = vertices[1]; // Front Bottom-Right

        // Define vertices for the Right Face (reusing vertices from front/back right edges)
        vertices[16] = vertices[1]; // Front Bottom-Right
        vertices[17] = vertices[2]; // Front Top-Right
        vertices[18] = vertices[6]; // Back Top-Right
        vertices[19] = vertices[5]; // Back Bottom-Right

        // Define vertices for the Left Face (reusing vertices from front/back left edges)
        vertices[20] = vertices[0]; // Front Bottom-Left
        vertices[21] = vertices[3]; // Front Top-Left
        vertices[22] = vertices[7]; // Back Top-Left
        vertices[23] = vertices[4]; // Back Bottom-Left

        // Create the mesh using the generic rectangle mesh creation function
        return CreateRectangleMeshFromVertices(vertices, "CustomFrustumMesh");
    }

    /// <summary>
    /// Creates a simple rectangular prism (cube) mesh centered at the origin.
    /// Builds the 24 vertices required and calls CreateRectangleMeshFromVertices.
    /// </summary>
    /// <param name="width">Width along the X-axis.</param>
    /// <param name="height">Height along the Y-axis.</param>
    /// <param name="depth">Depth/Length along the Z-axis.</param>
    /// <returns>A cube-shaped Mesh object.</returns>
    Mesh CreateCustomCubeMesh(float width, float height, float depth)
    {
        // Calculate half dimensions for centered placement
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        float halfDepth = depth / 2f;

        // Array to hold the 24 vertices
        Vector3[] vertices = new Vector3[24];

        // Define vertices for the Front Face (Z = -halfDepth)
        vertices[0] = new Vector3(-halfWidth, -halfHeight, -halfDepth); // Bottom-Left
        vertices[1] = new Vector3(halfWidth, -halfHeight, -halfDepth); // Bottom-Right
        vertices[2] = new Vector3(halfWidth, halfHeight, -halfDepth); // Top-Right
        vertices[3] = new Vector3(-halfWidth, halfHeight, -halfDepth); // Top-Left

        // Define vertices for the Back Face (Z = +halfDepth)
        vertices[4] = new Vector3(-halfWidth, -halfHeight, halfDepth); // Bottom-Left
        vertices[5] = new Vector3(halfWidth, -halfHeight, halfDepth); // Bottom-Right
        vertices[6] = new Vector3(halfWidth, halfHeight, halfDepth); // Top-Right
        vertices[7] = new Vector3(-halfWidth, halfHeight, halfDepth); // Top-Left

        // Define vertices for the Top Face (reusing vertices)
        vertices[8] = vertices[3]; // Front Top-Left
        vertices[9] = vertices[2]; // Front Top-Right
        vertices[10] = vertices[6]; // Back Top-Right
        vertices[11] = vertices[7]; // Back Top-Left

        // Define vertices for the Bottom Face (reusing vertices)
        vertices[12] = vertices[0]; // Front Bottom-Left
        vertices[13] = vertices[4]; // Back Bottom-Left
        vertices[14] = vertices[5]; // Back Bottom-Right
        vertices[15] = vertices[1]; // Front Bottom-Right

        // Define vertices for the Right Face (reusing vertices)
        vertices[16] = vertices[1]; // Front Bottom-Right
        vertices[17] = vertices[2]; // Front Top-Right
        vertices[18] = vertices[6]; // Back Top-Right
        vertices[19] = vertices[5]; // Back Bottom-Right

        // Define vertices for the Left Face (reusing vertices)
        vertices[20] = vertices[0]; // Front Bottom-Left
        vertices[21] = vertices[3]; // Front Top-Left
        vertices[22] = vertices[7]; // Back Top-Left
        vertices[23] = vertices[4]; // Back Bottom-Left

        // Create the mesh using the generic rectangle mesh creation function
        return CreateRectangleMeshFromVertices(vertices, "CustomCubeMesh");
    }

    /// <summary>
    /// Creates a procedural cylindrical mesh with specified dimensions and smoothness.
    /// Generates vertices, triangles, normals, and UVs for the cylinder sides and end caps.
    /// </summary>
    /// <param name="radius">Radius of the cylinder.</param>
    /// <param name="height">Height (length) of the cylinder along the Z-axis.</param>
    /// <param name="segments">Number of rectangular segments around the circumference (more segments = smoother curve).</param>
    /// <returns>A cylinder-shaped Mesh object.</returns>
    Mesh CreateCustomCylinderMesh(float radius, float height, int segments)
    {
        // Create a new Mesh object
        Mesh mesh = new Mesh();
        mesh.name = "CustomCylinderMesh";

        // --- Input Validation ---
        // Ensure minimum segments for a valid shape
        if (segments < 3)
            segments = 3; // Enforce minimum

        // Ensure positive radius
        if (radius <= 0)
            radius = 0.1f; // Use a small default if invalid

        // Ensure positive height
        if (height <= 0)
            height = 0.1f; // Use a small default if invalid

        // --- Setup ---
        float halfHeight = height / 2f; // Calculate half height for centered placement
        float segmentAngle = 360f / segments; // Angle covered by each segment in degrees

        // Use dynamic lists to build mesh data (more flexible than fixed arrays)
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        // --- Create Cylinder Sides ---
        // Iterate through each segment around the circumference
        for (int i = 0; i < segments; i++)
        {
            // Calculate angles for the current segment's start and end edges (in radians)
            float angle0 = Mathf.Deg2Rad * (i * segmentAngle);
            // Use modulo to wrap the last segment back to the first vertex
            float angle1 = Mathf.Deg2Rad * ((i + 1) % segments * segmentAngle);

            // Calculate X and Y coordinates on the circle for the segment edges
            float x0 = radius * Mathf.Cos(angle0);
            float y0 = radius * Mathf.Sin(angle0);
            float x1 = radius * Mathf.Cos(angle1);
            float y1 = radius * Mathf.Sin(angle1);

            // Define the four vertices of the rectangular quad for this segment
            // Order: Bottom-Left, Bottom-Right, Top-Right, Top-Left
            Vector3 bl = new Vector3(x0, y0, -halfHeight); // Bottom-Left vertex
            Vector3 br = new Vector3(x1, y1, -halfHeight); // Bottom-Right vertex
            Vector3 tr = new Vector3(x1, y1, halfHeight); // Top-Right vertex
            Vector3 tl = new Vector3(x0, y0, halfHeight); // Top-Left vertex

            // Store the starting index for the vertices being added in this iteration
            int vIndex = vertices.Count;

            // Add the four vertices to the list
            vertices.Add(bl);
            vertices.Add(br);
            vertices.Add(tr);
            vertices.Add(tl);

            // Calculate normals for smooth shading (pointing outwards from the center axis)
            Vector3 normal0 = new Vector3(x0, y0, 0).normalized; // Normal at angle0
            Vector3 normal1 = new Vector3(x1, y1, 0).normalized; // Normal at angle1

            // Add normals for each vertex (vertices share normals with adjacent segments for smoothness)
            normals.Add(normal0); // Bottom-Left uses normal0
            normals.Add(normal1); // Bottom-Right uses normal1
            normals.Add(normal1); // Top-Right uses normal1
            normals.Add(normal0); // Top-Left uses normal0

            // Calculate UV coordinates for texture mapping (unwrap the cylinder side)
            float u0 = (float)i / segments;       // U coordinate for the left edge
            float u1 = (float)(i + 1) / segments; // U coordinate for the right edge
            // V coordinate goes from 0 (bottom) to 1 (top)
            uvs.Add(new Vector2(u0, 0)); // Bottom-Left UV
            uvs.Add(new Vector2(u1, 0)); // Bottom-Right UV
            uvs.Add(new Vector2(u1, 1)); // Top-Right UV
            uvs.Add(new Vector2(u0, 1)); // Top-Left UV

            // Define the two triangles that make up the quad for this segment
            // Triangle 1: Bottom-Left, Bottom-Right, Top-Right
            triangles.Add(vIndex + 0);
            triangles.Add(vIndex + 1);
            triangles.Add(vIndex + 2);
            // Triangle 2: Bottom-Left, Top-Right, Top-Left
            triangles.Add(vIndex + 0);
            triangles.Add(vIndex + 2);
            triangles.Add(vIndex + 3);
        }

        // --- Create Top Cap ---
        // Add the center vertex for the top cap
        Vector3 topCenter = new Vector3(0, 0, halfHeight); // Center point at the top
        int topCenterIndex = vertices.Count; // Get its index
        vertices.Add(topCenter);
        normals.Add(Vector3.forward); // Normal points along the positive Z-axis (outwards from the cap)
        uvs.Add(new Vector2(0.5f, 0.5f)); // UV coordinate at the texture center

        // Add vertices around the top edge (reusing positions but need unique normals/UVs for the cap)
        int topCapStartIndex = vertices.Count; // Starting index for top edge vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * segmentAngle);
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle); // Using Z
            // Add vertex at the edge
            vertices.Add(new Vector3(x, y, halfHeight));
            // All top cap vertices share the same normal, pointing outwards (+Z)
            normals.Add(Vector3.forward);
            // Calculate circular UV mapping for the cap
            uvs.Add(new Vector2(x / (2 * radius) + 0.5f, y / (2 * radius) + 0.5f)); // Map circle to UV space
        }

        // Create triangles for the top cap (like a fan)
        for (int i = 0; i < segments; i++)
        {
            int currentEdgeIndex = topCapStartIndex + i;
            // Wrap around for the last triangle connecting back to the first edge vertex
            int nextEdgeIndex = topCapStartIndex + (i + 1) % segments;
            // Add triangle: Center -> Current Edge -> Next Edge
            triangles.Add(topCenterIndex);
            triangles.Add(currentEdgeIndex);
            triangles.Add(nextEdgeIndex);
        }

        // --- Create Bottom Cap ---
        // Add the center vertex for the bottom cap
        Vector3 bottomCenter = new Vector3(0, 0, -halfHeight); // Center point at the bottom
        int bottomCenterIndex = vertices.Count; // Get its index
        vertices.Add(bottomCenter);
        normals.Add(Vector3.back); // Normal points along the negative Z-axis (outwards from the cap)
        uvs.Add(new Vector2(0.5f, 0.5f)); // UV coordinate at the texture center

        // Add vertices around the bottom edge
        int bottomCapStartIndex = vertices.Count; // Starting index for bottom edge vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * segmentAngle);
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle); // Using Z
            // Add vertex at the edge
            vertices.Add(new Vector3(x, y, -halfHeight));
            // All bottom cap vertices share the same normal, pointing outwards (-Z)
            normals.Add(Vector3.back);
            // Calculate circular UV mapping (same as top cap)
            uvs.Add(new Vector2(x / (2 * radius) + 0.5f, y / (2 * radius) + 0.5f));
        }

        // Create triangles for the bottom cap (fan, reversed winding order)
        for (int i = 0; i < segments; i++)
        {
            int currentEdgeIndex = bottomCapStartIndex + i;
            // Wrap around for the last triangle
            int nextEdgeIndex = bottomCapStartIndex + (i + 1) % segments;
            // Add triangle: Center -> Next Edge -> Current Edge (Reversed order for correct normal facing down)
            triangles.Add(bottomCenterIndex);
            triangles.Add(nextEdgeIndex);
            triangles.Add(currentEdgeIndex);
        }

        // --- Assign Data to Mesh ---
        mesh.vertices = vertices.ToArray();   // Convert lists to arrays and assign
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();             // Important for rendering and culling

        // Return the completed cylinder mesh
        return mesh;
    }
    #endregion
}