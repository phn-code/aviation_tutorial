using UnityEngine;
using System.Collections.Generic;
using TMPro;


/** 
Airflow, Force Arrows and COP Behaviour

Class contains definitions and behaviour for
- Procedural Airflow
- Force Arrows
- Centre of Pressure indicator

Current implementaiton of both Fore Arrows and COP indicator utilize hardcoded value that reflect the provided values for Module 2 Section 1
Procedural airflow simulation is more versitile and allows for dynamic modificaitons to behaviour.

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
*/
[ExecuteAlways]
public class ProceduralAirflow : MonoBehaviour
{

    // Enable/disable bool
    [Header("")]
    public bool enableParticleSystem = false;       /**< Enable or disable procedural airflow */
    public bool enableForceArrows = false;          /**< Enable or disable force arrows */
    public bool enableCentreOfPressure = false;     /**< Enable or disable centre of pressure arrow */

    // Wing references
    [Header("Wing References")]
    public Transform leadingEdge;                   /**< Transform located at wings leading edge */
    public Transform trailingEdge;                  /**< Transform located at wings trailing edge */

    // Arrow anchor point references
    [Header("Force Arrow Anchor Points")]
    public Transform liftAnchor;                    /**< Tranfrom for lift arrow generation */
    public Transform weightAnchor;                  /**< Transform for weight arrow generation */
    public Transform thrusAnchor;                   /**< Transform for thrust arrow generation */
    public Transform dragAnchor;                    /**< Transform for drag arrow generation */

    // Arrow Settings
    [Header("Arrow Settings")]
    public float defaultArrowSize = 1f;             /**< Default size of force arrows */
    public Material forceArrowMaterial;             /**< Material for force arrows */
    public GameObject arrowHeadModel;               /**< Arrow head game object for force arrows */

    [Header("COP Variables")]
    public GameObject copModel;                     /**< Prefab for centre of pressure indicator */

    // Particle system settings
    [Header("Particle System Settings")]
    public ParticleSystem ps;                       /**< Airflow particle system, if null script generates new particle system */
    public float emissionRate = 20f;                /**< Procedural Airflow, particles per second */
    public float forwardSpawnDistance = 0.5f;       /**< Percentage of  */
    public float particleSize = 0.05f;              /**< Size of particles */
    public int maxParticles = 1000;                 /**< Max number of particles from particle system. */

    // Flow speed bounds
    [Header("Flow Speed Bounds")]
    public float topMaxSpeed = 2f;                  /**< Max speed for particles on top of the wing */
    public float topMinSpeed = 1.5f;                /**< Min speed for particles on top of the wing */
    public float bottomMaxSpeed = 1f;               /**< Max speed for particles below the wing */
    public float bottomMinSpeed = 0.5f;             /**< Min speed for particles below the wing */

    // Simulation settings
    [Header("Simulation Settings")]
    public float airSeperationStrength = 0.5f;      /**< Multiplies y velocity of top particles to reduce airflow stick to wing chord at higher AOA */
    public float trailDownForcePercent = 0.2f;      /**< Percent of particle y direction to be maintained after trailing edge.  */
    public bool useFixedDeltaInEditor = true;       /**< Enable ariflow in the editor*/
    public float editorDelta = 1f / 60f;            /**< 60fps deltatime for editor sim */

    // Internal data
    private ParticleSystem.Particle[] buffer;                   /**< Array of particles from the particle system */
    private List<Vector3> velocities = new List<Vector3>();     /**< Parallel List to buffer to store individual velocities */
    private List<bool> sideFlags = new List<bool>();            /**< Parallel List to buffer to store side flags, true = above wing, falase = below wing */
    float emissionsNextFrame = 0f;                              /**< Holds emmisions required per frame */
    float rotation = 0f;                                        /**< z rotation for arrows update loop */
    // Arrows
    private GameObject liftArrow;           /**< Instance of the lift force arrow game object */
    private GameObject weightArrow;         /**< Instance of the weight force arrow game object */
    private GameObject thrustArrow;         /**< Instance of the thrust force arrow game object */
    private GameObject dragArrow;           /**< Instance of the drag force arrow game object */
    // AOA stages
    private static float[] aoaStages = {3f, 7f, 10f, 14f, 16f, 18f};                /**< Predefined aoa stages for Force arrow and COP calc */
    // multipliers for default arrow size
    // used to lerp arro sizes based on AOA
    private static float[] liftValues = { 1.0f, 1.2f, 1.5f, 1.7f, 1.6f, 0.8f };     /**< Parallel array to aoaStages, stores mmultipliers to scale liftArrow */
    private static float[] dragValues = { 1.0f, 1.2f, 1.5f, 1.7f, 2.0f, 2.5f };     /**< Parallel array to aoaStages, stores multipliers to scale dragArrow */
    private static float[] thrustValues = { 1.0f, 1.2f, 1.5f, 1.6f, 1.0f, 0.6f };   /**< Parallel array to aoaStages, stores multipliers to scale thrustArrow  */

    // cop percentages from leading edge
    private static float[] copValues = { 0.25f, 0.24f, 0.22f, 0.20f, 0.20f, 0.45f };/**< Parallel array to aoaStages, stores multipliers to move cop arrow along plane chord  */

    // COP indicator
    private GameObject copGameObject;       /**< cop arrow insatnce */
    // Initialization

    /**
    Setup Particle System on awake

    Calls InitializeParticleSystem() and declares new Particle ::buffer with size of the particle systems internal buffer.
    */
    void Awake()
    {
        InitializeParticleSystem();
        buffer = new ParticleSystem.Particle[maxParticles];
    }

    /**
    Setup particle system for procedural airflow

    If particle system parameter is null generate a new particle system and set relevant variables to ensure compatability with existing functions.
    - EmitParticles()
    - UpdateParticles()
    */
    void InitializeParticleSystem()
    {
        if (ps == null)
        {
            ps = gameObject.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.maxParticles = maxParticles;
            // Particle lifetime will be manually controlled and a extended lifetime is to allow removal in edgecases
            main.startLifetime = 15f;
            main.startSize = particleSize;
            main.startSpeed = 0f; // we control manually
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.enabled = false; // manual emission

            var shape = ps.shape;
            shape.enabled = false; // spawn at exact positions

            //TODO add material creation

            Debug.Log("ParticleSystem auto created.");
        }

        // clear previous lists
        velocities.Clear();
        sideFlags.Clear();
    }

    /**
    Main update loop

    Handles Airflow particle, Force Arrows and COP indicator rendering. Uses fake delta time for editor rendering.
    */
    void Update()
    {
        if (leadingEdge == null || trailingEdge == null || ps == null) return;

        if (buffer == null || buffer.Length != maxParticles)
            buffer = new ParticleSystem.Particle[maxParticles];

        float dt = Application.isPlaying ? Time.deltaTime : (useFixedDeltaInEditor ? editorDelta : Time.deltaTime);

        if (enableParticleSystem)
        {
            // Emit new particles
            EmitParticles(dt);

            // Update existing particles
            UpdateParticles(dt);
        }

        //Debug.Log("Are force Arrows Enabled? " + enableForceArrows);
        if (enableForceArrows)
        {
            // Fix inconsistent toggle behaviour
            // Add a check for if arrows are enabled but not instantiated
            bool missingArrow = liftArrow == null || weightArrow == null || thrustArrow == null || dragArrow == null;
            // Update force arrows
            float newRotation = transform.rotation.eulerAngles.z;
            if (missingArrow || rotation != newRotation)
            {
                //Debug.Log("Is this working");
                rotation = newRotation;
                UpdateForceArrows();
            }
        }
        else
        {
            DestroyPreviousArrows();
        }
        //Debug.Log($"lift arrow: {liftArrow == null} weight arrow: {weightArrow == null} thrust arrow: {thrustArrow == null} drag arrow: {dragArrow == null}");

        if (enableCentreOfPressure)
        {
            updateCentreOfPressure();
        }
        else
        {
            destroyCentreOfPressure();
        }

        // Ensure particle system is playing
        if (!ps.isPlaying) ps.Play();

        // Editor simulation
        if (!Application.isPlaying)
            ps.Simulate(dt, true, false, true);
    }

    /** 
    Manual Airflow Particle Emission

    Spawns particles at a fixed rate and assigns initial ::velocities and ::sideFlags values.

    @param dt deltatime, fixed value for editor or actual suring play
    */
    void EmitParticles(float dt)
    {
        // calculate particles number of particles to spawn
        emissionsNextFrame += emissionRate * dt;

        // get integer value of particle spawn requirements and decrement counter
        int particlesToEmit = Mathf.FloorToInt(emissionsNextFrame);
        emissionsNextFrame -= particlesToEmit;

        // Return if no particles to spawn
        if (particlesToEmit <= 0) return;

        // get normalised vector of the chord
        Vector3 chordDir = (trailingEdge.position - leadingEdge.position).normalized;

        for (int i = 0; i < particlesToEmit; i++)
        {
            // Set the spawn position of the particle 
            var emitParams = new ParticleSystem.EmitParams();
            Vector3 spawnPos = leadingEdge.position - chordDir * forwardSpawnDistance;
            emitParams.position = spawnPos;

            emitParams.startLifetime = 5f;
            emitParams.startSize = particleSize;

            Vector3 toLeadingEdge = (leadingEdge.position - spawnPos).normalized;
            emitParams.velocity = toLeadingEdge * Random.Range(topMinSpeed, topMaxSpeed);

            // randomly assign particle as above or below the chord
            // set random velocity between top and min speeds for update loop conditions
            bool above = Random.value > 0.2f;
            Vector3 velocity;
            if (above)
            {
                Vector3 airflowDir = chordDir;
                airflowDir.y *= airSeperationStrength;
                airflowDir = airflowDir.normalized;
                velocity = airflowDir * Random.Range(topMinSpeed, topMaxSpeed);
                sideFlags.Add(true);
            }
            else
            {
                velocity = chordDir * Random.Range(bottomMinSpeed, bottomMaxSpeed);
                sideFlags.Add(false);
            }

            // Store the per particle velocities
            velocities.Add(velocity);

            // add particles to local buffer and particle system
            ps.Emit(emitParams, 1);
        }

    }

    /** 
    Manual Airflow Particle Update

    Determines position of each partiicle in relation to key positions along the chord.
    Updates particles velocity to coform to the chord of the wing. Particles with the ::sideFlags bool velue of
    - False, move along the winf base
    - True, move at a reduced y velocity to replicate airflow separation.

    @param dt deltatime, fixed value for editor or actual suring play
    */
    void UpdateParticles(float dt)
    {
        int count = ps.GetParticles(buffer);

        // the local position of leading and trailing edges
        Vector3 localLeading = transform.InverseTransformPoint(leadingEdge.position);
        Vector3 localTrailing = transform.InverseTransformPoint(trailingEdge.position);

        // calculate chord direction in world space
        Vector3 chordDir = (trailingEdge.position - leadingEdge.position).normalized;
        // calculate the chord length in local space
        float chordLengthLocal = Vector3.Distance(localTrailing, localLeading);

        // determine the chord orientation in local space. sign determines if teh trailing edge is at +x or -x in local space
        float sign = Mathf.Sign(localTrailing.x - localLeading.x);

        // calculate poiints for particle direction changes
        float deflectionStart = chordLengthLocal * .10f;     // 20% of the chord length from leading edge
        float trailDistance = chordLengthLocal * .50f;       // 50% of the chord length past trailing edge

        // Loop over all active particles
        for (int i = 0; i < count; i++)
        {
            // get particle world position and convert to local position
            Vector3 particleWorld = buffer[i].position;
            Vector3 particleLocal = transform.InverseTransformPoint(particleWorld);

            // Determines a particles position along the chord / displacement from leading edge
            // sign ensures distances along the chord are positive
            float alongChord = (particleLocal.x - localLeading.x) * sign;

            // check is particles are over trail distance
            if (alongChord > chordLengthLocal + trailDistance)
            {
                // Remove particle by swapping with last and reducing count
                int lastIdx = count - 1;
                if (i != lastIdx)
                {
                    buffer[i] = buffer[lastIdx];
                    velocities[i] = velocities[lastIdx];
                    sideFlags[i] = sideFlags[lastIdx];
                }
                count--;
                velocities.RemoveAt(lastIdx);
                sideFlags.RemoveAt(lastIdx);
                continue;
            }

            // if particle is at or past deflection point deflect based on side flags
            if (alongChord >= -deflectionStart && alongChord <= 0f)
            {
                // Deflect relative to planes local up 
                Vector3 offsetTargetLocal;
                if (sideFlags[i])
                {
                    // Particle is above the wing, deflect upwards
                    offsetTargetLocal = new Vector3(
                        localLeading.x + deflectionStart * sign,
                        localLeading.y + deflectionStart,
                        particleLocal.z
                    );
                }
                else
                {
                    // Particle is below the wing deflect downwards
                    offsetTargetLocal = new Vector3(
                        localLeading.x + deflectionStart * sign,
                        localLeading.y - deflectionStart,
                        particleLocal.z
                    );
                }

                // convert offset target to world coorinates
                Vector3 offsetTargetWorld = transform.TransformPoint(offsetTargetLocal);
                // get direction in world coordinates to target
                Vector3 targetDir = (offsetTargetWorld - particleWorld).normalized;

                float speed = velocities[i].magnitude;
                buffer[i].velocity = targetDir * speed;
            }

            // if particle is past the deflection target (leading edge) set velocity to stored velocities value
            else if (alongChord > deflectionStart && alongChord <= chordLengthLocal)
            {
                buffer[i].velocity = velocities[i];
            }
            // If particle is past trailing edge remove vertical velocity component
            else if (alongChord > chordLengthLocal && alongChord <= chordLengthLocal + trailDistance)
            {
                float horizontalSpeed = velocities[i].magnitude;
                // Remove y component from chordDir and normalize
                Vector3 chordDirNoY = new Vector3(chordDir.x, chordDir.y * trailDownForcePercent, chordDir.z).normalized;
                buffer[i].velocity = chordDirNoY * horizontalSpeed;
            }
            else
            {
                int n = 1;
            }

            // Apply velocity update to position
            buffer[i].position += buffer[i].velocity * dt;
        }

        // Hand updated particles back to the system for rendering
        ps.SetParticles(buffer, count);
    }


    /**
    Updates four force arrows

    Check if arrows exist and recreates if not. Sets each arrows position and scale
    */
    void UpdateForceArrows()
    {
        /*
        // TODO fix arrow offset by using bounding box calculation ot centre arrows.
        Renderer render = GetComponentInChildren<MeshRenderer>();
        Debug.LogError(render + "This is a debug message for force arrows");
        */
        if (liftArrow == null || weightArrow == null || thrustArrow == null || dragArrow == null)
        {
            CreateForceArrows();
        }

        // get the angle of the plane
        float aoa = transform.eulerAngles.z;

        SetArrow(liftArrow, liftAnchor.position, Vector3.up, GetLiftLength(aoa));
        SetArrow(weightArrow, weightAnchor.position, Vector3.down, GetWeightLength(aoa));
        SetArrow(thrustArrow, thrusAnchor.position, Vector3.right, GetThrustLength(aoa));
        SetArrow(dragArrow, dragAnchor.position, Vector3.left, GetDragLength(aoa));

    }

    /**
    Returns lift arrow length as a multiplier
    
    Checks the current angle of attack against the values in ::aoaStages.
    Returns a multiplier for the arrow length via linear interpolation between 
    the upper and lower bounds of the current aoa against the ::aoaStages.
    
    @param aoa Current angle of attack (pitch) of the plane
    @return Arrow length multiplier based on interpolated lift values
    */
    float GetLiftLength(float aoa) 
    {
        return defaultArrowSize * InterpolateForceValue(aoa, liftValues);
    }

    /**
    Returns weight arrow length as a constant multiplier
    
    Weight is constant and does not vary with angle of attack, so returns
    the base arrow size without any scaling factor applied.
    
    @param aoa Current angle of attack (pitch) of the plane (unused)
    @return Constant arrow length equal to defaultArrowSize
    */
    float GetWeightLength(float aoa) 
    { 
        return defaultArrowSize; 
    }

    /**
    Returns thrust arrow length as a multiplier
    
    Checks the current angle of attack against the values in ::aoaStages.
    Returns a multiplier for the arrow length via linear interpolation between 
    the upper and lower bounds of the current aoa against the ::aoaStages.
    
    @param aoa Current angle of attack (pitch) of the plane
    @return Arrow length multiplier based on interpolated thrust values
    */
    float GetThrustLength(float aoa) 
    {
        return defaultArrowSize * InterpolateForceValue(aoa, thrustValues);
    }

    /**
    Returns drag arrow length as a multiplier
    
    Checks the current angle of attack against the values in ::aoaStages.
    Returns a multiplier for the arrow length via linear interpolation between 
    the upper and lower bounds of the current aoa against the ::aoaStages.
    
    @param aoa Current angle of attack (pitch) of the plane
    @return Arrow length multiplier based on interpolated drag values
    */
    float GetDragLength(float aoa) 
    {
        return defaultArrowSize * InterpolateForceValue(aoa, dragValues);
    }

    /**
    Interpolates force coefficient values based on angle of attack
    
    Performs linear interpolation between force coefficient values based on the
    current angle of attack. The interpolation factor (t) is calculated as:
    t = (aoa - aoaStages[i]) / (aoaStages[i+1] - aoaStages[i])
    where t ranges from 0 (at lower bound) to 1 (at upper bound).
    
    For aoa values outside the defined ::aoaStages range, returns the minimum
    or maximum force value without interpolation.
    
    @param aoa Current angle of attack (pitch) of the plane
    @param forceValues Array of force coefficient values corresponding to ::aoaStages
    @return Interpolated force coefficient value
    */
    private float InterpolateForceValue(float aoa, float[] forceValues)
    {
        // Handle values below minimum stage
        if (aoa <= aoaStages[0]) return forceValues[0];

        // Handle values above maximum stage
        if (aoa >= aoaStages[aoaStages.Length - 1])
            return forceValues[forceValues.Length - 1];

        // Find the appropriate stage interval and interpolate
        for (int i = 0; i < aoaStages.Length - 1; i++)
        {
            if (aoa <= aoaStages[i + 1])
            {
                // Calculate interpolation factor 0 = start, 1 = end, 0.5 = halfway
                float t = (aoa - aoaStages[i]) / (aoaStages[i + 1] - aoaStages[i]);

                // Linearly interpolate between lower and upper force values
                return Mathf.Lerp(forceValues[i], forceValues[i + 1], t);
            }
        }
        // Fallback to maximum value
        return forceValues[forceValues.Length - 1];
    }

    /**
    Configures arrow transform properties
    
    Sets the scale, rotation, and position of a force arrow game object. The arrow is
    oriented to point in the specified direction and positioned so its base sits at
    the anchor point. Also scales and positions the arrow head child object.
    
    @param arrow The arrow game object to configure
    @param basePos World position for the base of the arrow
    @param dir Direction vector the arrow should point
    @param length Total length of the arrow in world units
    */
    // NEW
    void SetArrow(GameObject root, Vector3 basePos, Vector3 dir, float length)
    {
        float width = 0.1f;
        float height = 0.1f;
        float headSize = 0.2f;

        // Find the shaft child — this is what gets scaled
        Transform shaft = root.transform.Find(root.name.Replace("_Arrow_Root", "") + "_Shaft");
        if (shaft == null) return;

        shaft.localScale = new Vector3(width, height, length);

        Quaternion lookRotation = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion yRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        root.transform.rotation = yRotation * lookRotation;

        root.transform.position = basePos + root.transform.forward * (length * 0.5f);

        Transform headTransform = shaft.GetChild(0);
        headTransform.localPosition = new Vector3(0, 0, 0.5f);
        headTransform.localScale = new Vector3(headSize / width, headSize / height, headSize / length);

        // Position the label and push it toward the camera to prevent occlusion
        Transform label = root.transform.Find(root.name.Replace("_Arrow_Root", "") + "_Label");
        if (label != null)
        {
            label.localPosition = new Vector3(0, 0.25f, 0);

            // This is the fix for the weight label being hidden behind the arrow shaft
            if (Camera.main != null)
            {
                Vector3 toCamera = (Camera.main.transform.position - label.position).normalized;
                label.position += toCamera * 0.15f;
            }
        }
    }

    /**
    Creates all four force arrow game objects
    
    Destroys any existing arrows and instantiates new lift, weight, thrust, and drag
    arrows with their respective colors.
    */
    void CreateForceArrows() 
    {
        DestroyPreviousArrows(); // remove old arrows

        liftArrow = CreateArrow("Lift", Color.green);
        weightArrow = CreateArrow("Weight", Color.red);
        thrustArrow = CreateArrow("Thrust", Color.blue);
        dragArrow = CreateArrow("Drag", Color.yellow);
    }

    /**
    Creates a single force arrow game object
    
    Instantiates a cube primitive as the arrow shaft and attaches an arrow head model
    as a child. Assigns the specified color to both shaft and head materials.
    
    @param name Base name for the arrow (e.g. "Lift", "Drag")
    @param colour Color to apply to the arrow material
    @return The created arrow game object
    */
    // NEW
    GameObject CreateArrow(string name, Color colour)
    {
        GameObject root = new GameObject(name + "_Arrow_Root");

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = name + "_Shaft";
        shaft.transform.SetParent(root.transform);

        Renderer rend = shaft.GetComponent<Renderer>();
        Material mat = new Material(forceArrowMaterial);
        if (Application.isPlaying) { mat.color = colour; rend.material = mat; }
        else { rend.sharedMaterial.color = colour; }

        GameObject arrowHead = Instantiate(arrowHeadModel);
        arrowHead.transform.SetParent(shaft.transform);
        arrowHead.GetComponent<MeshRenderer>().material = mat;
        arrowHead.transform.localPosition = new Vector3(0, 0, 0.5f);
        arrowHead.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Label
        GameObject labelObj = new GameObject(name + "_Label");
        labelObj.transform.SetParent(root.transform);

        TextMeshPro textMesh = labelObj.AddComponent<TextMeshPro>();
        textMesh.text = name;
        textMesh.color = Color.black;
        textMesh.fontSize = 2.2f;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.alignment = TextAlignmentOptions.Center;
        labelObj.AddComponent<Billboard>();

        return root;
    }

    /**
    Destroys all existing force arrow game objects
    
    Searches for and destroys any existing lift, weight, thrust, or drag arrows in the
    scene. Uses DestroyImmediate in edit mode and Destroy in play mode.
    */
    // NEW
    void DestroyPreviousArrows()
    {
        string[] arrowNames = { "Lift_Arrow_Root", "Weight_Arrow_Root", "Thrust_Arrow_Root", "Drag_Arrow_Root" };
        foreach (string name in arrowNames)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing);
                else DestroyImmediate(existing);
            }
        }
    }

    /**
    Updates centre of pressure indicator position
    
    Creates the COP indicator if it doesn't exist, then calculates its position along
    the wing chord based on the current angle of attack. Positions the indicator below
    the chord at the calculated percentage distance from the leading edge.
    */
    void updateCentreOfPressure()
    {
        if (copGameObject == null)
        {
            copGameObject = createCopIndicator("COP_Indicator", Color.grey);
        }

        // get the angle of the plane
        float aoa = transform.eulerAngles.z;

        // Get the percentage along the chord
        float percentage = getCopPercentage(aoa);
        //Debug.Log("COP percentage: " + percentage);

        // Calculate chord direction and length
        Vector3 chordDir = (trailingEdge.position - leadingEdge.position).normalized;
        float chordLength = Vector3.Distance(leadingEdge.position, trailingEdge.position);

        // Calculate position along the chord
        Vector3 copPos = leadingEdge.position + chordDir * (chordLength * percentage);

        // Offset below the chord 
        Vector3 offset = -transform.up * 0.3f;
        copGameObject.transform.position = copPos + offset;
    }

    /**
    Creates centre of pressure indicator game object
    
    Destroys any existing COP indicator and instantiates a new one from the COP model
    prefab with the specified name and color.
    
    @param name Name to assign to the COP indicator game object
    @param colour Color to apply to the indicator (currently unused in implementation)
    @return The created COP indicator game object
    */
    GameObject createCopIndicator(string name, Color colour)
    {
        destroyCentreOfPressure();

        // Root container — always scale (1,1,1), so label offsets are predictable
        GameObject copRoot = new GameObject(name);

        GameObject copModelInstance = Instantiate(copModel);
        copModelInstance.transform.SetParent(copRoot.transform);
        copModelInstance.transform.localPosition = Vector3.zero;

        // Label is parented to root, not the scaled model
        GameObject labelObj = new GameObject("COP_Label");
        labelObj.transform.SetParent(copRoot.transform);
        labelObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);

        TextMeshPro textMesh = labelObj.AddComponent<TextMeshPro>();
        textMesh.text = "CoP";
        textMesh.color = Color.black;
        textMesh.fontSize = 1.5f;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.alignment = TextAlignmentOptions.Center;
        labelObj.AddComponent<Billboard>();

        return copRoot;
    }

    /**
    Returns centre of pressure position as percentage of chord
    
    Calculates the COP position along the wing chord as a percentage from the leading
    edge based on angle of attack. Uses linear interpolation between predefined stages
    in ::aoaStages and ::copValues arrays.
    
    @param aoa Current angle of attack (pitch) of the plane
    @return Percentage distance from leading edge (0.0 = leading edge, 1.0 = trailing edge)
    */
    float getCopPercentage(float aoa)
    {
        // same logic as arrow calculations
        if (aoa <= aoaStages[0]) return copValues[0];
        if (aoa <= aoaStages[1]) return Mathf.Lerp(copValues[0], copValues[1], (aoa - aoaStages[0]) / (aoaStages[1] - aoaStages[0]));
        if (aoa <= aoaStages[2]) return Mathf.Lerp(copValues[1], copValues[2], (aoa - aoaStages[1]) / (aoaStages[2] - aoaStages[1]));
        if (aoa <= aoaStages[3]) return Mathf.Lerp(copValues[2], copValues[3], (aoa - aoaStages[2]) / (aoaStages[3] - aoaStages[2]));
        if (aoa <= aoaStages[4]) return Mathf.Lerp(copValues[3], copValues[4], (aoa - aoaStages[3]) / (aoaStages[4] - aoaStages[3]));
        if (aoa <= aoaStages[5]) return Mathf.Lerp(copValues[4], copValues[5], (aoa - aoaStages[4]) / (aoaStages[5] - aoaStages[4]));
        return copValues[5];
    }

    /**
    Destroys the centre of pressure indicator game object
    
    Searches for and destroys the COP indicator if it exists in the scene. Uses
    DestroyImmediate in edit mode and Destroy in play mode.
    */
    void destroyCentreOfPressure()
    {
        GameObject existing = GameObject.Find("COP_Indicator");
        // Schedule duplicates for destruction in play mode
        if (Application.isPlaying)
            Destroy(existing);
        // Else in edit mode destroy immediately (Since destroy is tied to the end of the next frame which doesnt trigger in edit mode)
        else
            DestroyImmediate(existing);
    }

    /**
    Draws editor gizmos for visualization
    
    Renders visual guides in the Unity editor showing the wing chord line (yellow),
    top airflow guide (cyan), and bottom airflow guide (magenta) at the chord midpoint.
    */
    void OnDrawGizmos()
    {
        if (leadingEdge != null && trailingEdge != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(leadingEdge.position, trailingEdge.position);

            // draw small offset lines for top/bottom airflow guidance
            Vector3 chordDir = (trailingEdge.position - leadingEdge.position).normalized;
            float chordLen = Vector3.Distance(leadingEdge.position, trailingEdge.position);
            Vector3 midpoint = leadingEdge.position + chordDir * chordLen * 0.5f;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(midpoint, midpoint + Vector3.up * 0.2f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(midpoint, midpoint + Vector3.down * 0.2f);
        }
    }
}
