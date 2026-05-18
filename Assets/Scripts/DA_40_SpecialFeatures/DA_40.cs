using System.Collections.Generic;
using UnityEngine;
using TMPro;

/** 
Handles mostly cosmetic elements of the DA-40 aircraft itself, including default appearance, sounds, component access and toggling, and propeller rotation.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class DA_40 : MonoBehaviour
{
    [Header("Plane Settings")]
    [SerializeField] private Color GlowColour; /**< The colour in which highlighted components will glow. */
    [SerializeField] private List<GameObject> InitialHighlightedComponents = new List<GameObject>(); /**< A collection of aircraft components (as GameObjects) that will start glowing at runtime. */
    [SerializeField] private bool CockpitClosed; /**< Flag determining if the cockpit of the aircraft is open or closed. */
    [SerializeField] private bool PorteClosed; /**< Flag determining if the porte of the aircraft is open or closed. */
    [SerializeField] private bool PropellerEnabled; /**< Flag determining if the front propeller is spinning or not. */
    [SerializeField] private bool GhostTrailEnabled; /**< Flag determining if the ghost trail is active or not. */
    [SerializeField] private ParticleSystem GhostTrail; /**< Particle system representing the "after-image", or ghost trail, of the aircraft. It's been implemented as a way to visualise the trajectory of the aircraft while it's in-flight. */
    [SerializeField] private bool AxialPanelsEnabled; /**< DEPRECATED: Flag determining if the axial panel visualisation system is active or not. */
    [SerializeField] private float PropellerRotationSpeed = 1000f; /**< The rotation speed of the front propeller. */
    [SerializeField] private bool IsPassthroughEnabled; /**< DEPRECATED: Flag determining if passthrough mode is enabled or not. */
    [SerializeField] private List<GameObject> AxialScrollingPlanes = new List<GameObject>(); //**< DEPRECATED: A collection of planes (not the aircraft, but two triangles arranged in a panel in 3D space) that dynamically represent the aircraft's dynamic translation in the world. Replaced by the ghost trail system (see ParticleSystem GhostTrail above). */
    [SerializeField] private float AxialScrollSpeed; /**< DEPRECATED: The sensitivity of the scroll speed at which the textures of the axial panels shift. */
    [SerializeField] private TextMeshProUGUI subtitles; /**< Reference to the subtitles displayed on the LocalCanvas of the user interface. */

    [Header("Sounds")]
    [SerializeField] private AudioSource FlightLoop; /**< The whirring sound of the engine. */
    [SerializeField] private AudioSource PropellerStart; /**< The startup sound of the engine when the plane is turned on. */
    [SerializeField] private AudioSource StallWarning; /**< The stall horn sound that plays when a wing (or wings) is stalling. */

    [Header("Dashboard")]
    [SerializeField] private GameObject Dashboard; /**< The dashboard of the aircraft. */

    [Header("Cockpit")]
    [SerializeField] private GameObject Canopy; /**< The canopy of the aircraft. */
    [SerializeField] private GameObject Porte; /**< The porte of the aircraft. */
    [SerializeField] private GameObject Vitres; /**< The vitres of the aircraft. */
    [SerializeField] private GameObject Interior; /**< The interior of the aircraft. */
    [SerializeField] private GameObject Seats; /**< The seats of the aircraft. */
    [SerializeField] private GameObject Yoke_R; /**< The right yoke of the aircraft. */
    [SerializeField] private GameObject Yoke_L; /**< The left yoke of the aircraft. */

    [Header("Fuselage")]
    [SerializeField] private GameObject FuselageFrame; /**< The fuselage frame of the aircraft. */
    [SerializeField] private GameObject Step_R; /**< The right step of the aircraft. */
    [SerializeField] private GameObject Step_L; /**< The left step of the aircraft. */
    [SerializeField] private GameObject Antennes; /**< The antennes of the aircraft. */
    [SerializeField] private GameObject PropellerHub; /**< The propeller hub of the aircraft. */
    [SerializeField] private GameObject Helice; /**< The helice of the aircraft. */
    [SerializeField] private GameObject Exhaust; /**< The exhaust of the aircraft. */

    [Header("Empennage")]
    [SerializeField] private GameObject EmpennageFrame; /**< The empennage frame of the aircraft. */
    [SerializeField] private GameObject Rudder; /**< The rudder of the aircraft. */
    [SerializeField] private GameObject Stabiliser; /**< The stabiliser of the aircraft. */
    [SerializeField] private GameObject Elevator; /**< The elevator of the aircraft. */
    [SerializeField] private GameObject TailGuard; /**< The tail guard of the aircraft. */
    [SerializeField] private GameObject TailLight; /**< The tail light of the aircraft. */

    [Header("Right Wing")]
    [SerializeField] private GameObject WingFrame_R; /**< The right wing frame of the aircraft. */
    [SerializeField] private GameObject Aileron_R; /**< The right aileron of the aircraft. */
    [SerializeField] private GameObject LandingFlap_R; /**< The right landing flap of the aircraft. */

    [Header("Left Wing")]
    [SerializeField] private GameObject WingFrame_L; /**< The left wing frame of the aircraft. */
    [SerializeField] private GameObject Aileron_L; /**< The left aileron of the aircraft. */
    [SerializeField] private GameObject LandingFlap_L; /**< The left landing flap of the aircraft. */
    [SerializeField] private GameObject LandingLightBulb; /**< The landing light bulb of the aircraft. */

    [Header("Wing Components")]
    [SerializeField] private GameObject InnerGuards; /**< The inner wing guards of the aircraft. */
    [SerializeField] private GameObject Struts; /**< The struts of the aircraft. */

    [Header("Landing Gear")]
    [SerializeField] private GameObject LandingStrutBrace_F; /**< The front landing strut brace of the aircraft. */
    [SerializeField] private GameObject LandingStrut_F; /**< The front landing strut of the aircraft. */
    [SerializeField] private GameObject OuterGuard_F; /**< The front outer guard of the aircraft. */
    [SerializeField] private GameObject InnerGuard_F; /**< The front inner guard of the aircraft. */
    [SerializeField] private GameObject Wheel_F; /**< The front wheel of the aircraft. */
    [SerializeField] private GameObject LandingStrut_R; /**< The right landing strut of the aircraft. */
    [SerializeField] private GameObject OuterGuard_R; /**< The right outer guard of the aircraft. */
    [SerializeField] private GameObject InnerGuard_R; /**< The right inner guard of the aircraft. */
    [SerializeField] private GameObject Wheel_R; /**< The right wheel of the aircraft. */
    [SerializeField] private GameObject LandingStrut_L; /**< The left landing strut of the aircraft. */
    [SerializeField] private GameObject OuterGuard_L; /**< The left outer guard of the aircraft. */
    [SerializeField] private GameObject InnerGuard_L; /**< The left inner guard of the aircraft. */
    [SerializeField] private GameObject Wheel_L; /**< The left wheel of the aircraft. */

    private bool lastTrailState;  /**< The previous state of the ghost trail. */

    private float propellerAcceleration = 500f;  /**< The rate at which the propeller rotation accelerates to full speed. */
    private float currentPropellerSpeed = 0f;  /**< The current rotation speed of the propeller. */


    void Start()
    {
        // Initially set the selected components to glow at runtime
        PlaneHighlighter.Highlight(InitialHighlightedComponents, GlowColour);
        // UnhighlightPlaneComponent(new List<GameObject> { Canopy, Porte });

        if (CockpitClosed)
        {
            Canopy.transform.localPosition = new Vector3(0.586243f, -1.451507f, -4.410744e-06f);
            Canopy.transform.localRotation = Quaternion.Euler(0f, 0f, 35.039f);
        }

        if (PorteClosed)
        {
            Porte.transform.localPosition = new Vector3(-0.05631721f, 0.2968897f, -0.3203334f);
            Porte.transform.localRotation = Quaternion.Euler(55.165f, 3.138f, -4.324f);
        }

        if (!AxialPanelsEnabled)
        {
            foreach (var axis in AxialScrollingPlanes)
            {
                axis.SetActive(false);
            }
        }

        // Initial setup of ghost trail
        if (GhostTrailEnabled)
            GhostTrail.Play();
        else
            GhostTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        lastTrailState = GhostTrailEnabled;

        // Clear subtitles
        subtitles.text = "";
    }


    void Update()
    {
        // Linear increase and decrease of propeller speed when toggled on and off respectively
        if (PropellerEnabled && Helice != null) // Rotate the propeller if it is toggled on
        {
            currentPropellerSpeed = Mathf.MoveTowards(currentPropellerSpeed, PropellerRotationSpeed, propellerAcceleration * Time.deltaTime);

            Helice.transform.Rotate(Vector3.right, -currentPropellerSpeed * Time.deltaTime);
        }
        else
        {
            currentPropellerSpeed = Mathf.MoveTowards(currentPropellerSpeed, 0f, propellerAcceleration * Time.deltaTime);
            Helice.transform.Rotate(Vector3.right, -currentPropellerSpeed * Time.deltaTime);
        }

        if (GhostTrailEnabled != lastTrailState) // Every frame, turn on the ghost trail if enabled by checking prev state
        {
            if (GhostTrailEnabled)
                GhostTrail.Play();
            else
                GhostTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            lastTrailState = GhostTrailEnabled;
        }

        // This is all deprecated!!! Doesn't need to be here anymore, but it is disabled in practice already.
        for (int i = 0; i < AxialScrollingPlanes.Count; i++) // For every axial plane (should only be 3)
        {
            var axis = AxialScrollingPlanes[i];
            if (axis == null) continue;

            var renderers = axis.GetComponentsInChildren<Renderer>(true);
            Vector2 offset = Vector2.zero; // Offset will be what the texture uses to scroll

            switch (i) // Each plane will read different position values to determine their axis' offset
            {
                case 0: // X plane offset - "back", relies on vehicle's Y and Z positions
                    offset = new Vector2(transform.position.y, transform.position.z) * AxialScrollSpeed;
                    //Debug.Log("Changing stuff!");
                    break;
                case 1: // Y plane offset - "left", relies on vehicle's X and Z positions
                    offset = new Vector2(transform.position.x, transform.position.z) * AxialScrollSpeed;
                    //Debug.Log("Changing stuff!");
                    break;
                case 2: // Z plane offset - "up", relies on vehicle's X and Y positions
                    offset = new Vector2(transform.position.y, transform.position.x) * AxialScrollSpeed;
                    //Debug.Log("Changing stuff!");
                    break;
            }

            foreach (var renderer in renderers) // Apply the offset
            {
                renderer.material.SetTextureOffset("_BaseMap", offset);
            }
        }
    }

    /**
    Plays a sound that is passed in as an AudioSource.
    @param sound The sound to be played.
    @return void
    */
    public void playSFX(AudioSource sound)
    {
        sound.Play();
    }

    /**
    Stops a sound that is currently playing, passed in as an AudioSource.
    @param sound The sound to be stopped.
    @return void
    */
    public void stopSFX(AudioSource sound)
    {
        sound.Stop();
    }

    /**
    Sets a new state for the propeller spin.
    @param newState The new state for the propeller.
    @return void
    */
    public void TogglePropeller(bool newState)
    {
        PropellerEnabled = newState;
    }

    public void EnableGhostTrail(bool enabled)
    {
        GhostTrailEnabled = enabled;
    }
}
