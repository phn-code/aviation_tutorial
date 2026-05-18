using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/**
Section 1 stall training activity controller

Manages a progressive pitch exercise where users practice increasing angle of attack
through six stages (3, 7, 10, 14, 16, 18) while observing aerodynamic effects.
Enables real-time visualization of force arrows, airflow particles, and center of
pressure to demonstrate the relationship between pitch angle and aerodynamic forces
leading to stall. Implements the IActivityController interface for integration with
the ModuleActivityScheduler system.

Training sequence:
1. Pitch - Progressive pitch targets with aerodynamic visualization
2. Complete - Activity finished, notifies scheduler

Key features:
- Real-time force arrow visualization (lift, weight, thrust, drag)
- Procedural airflow particle system
- Dynamic center of pressure indicator
- Progressive angle of attack demonstration leading to stall

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
*/
public class Sec1StallActivity : MonoBehaviour, IActivityController
{

    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance;  /**< Reference to singleton activity scheduler */

    [Header("timelines")]
    public TimelineAsset[] pitchTimelineAssets;     /**< Timeline assets for each pitch milestone (3, 7, 10, 14, 16, 18) */

    [Header("Input action references")]
    public InputActionReference rotationInput;      /**< VR controller rotation input action for aircraft pitch control */

    [Header("Control Settings")]
    public float pitchSensitivity = 1f;             /**< Pitch sensitivity multiplier for controller input */
    public float deadZone = 15f;                    /**< Controller deadzone threshold in degrees to filter noise */
    public float stepAdvanceTolerance = 3f;         /**< Tolerance in degrees for achieving target pitch before advancing */
    private Quaternion controllerOrigin = Quaternion.identity;  /**< Initial controller orientation captured at step start */
    private bool hasControllerOrigin = false;                   /**< Flag indicating if controller origin has been captured this step */

    [Header("Editor Debug settings")]
    public bool useMouseClicks = false;             /**< Enable mouse button control for testing without VR */
    public float mouseRollSpeed = 10f;              /**< Speed multiplier for mouse-based pitch control */

    /**
    Training progression states
    
    Defines the sequence of training steps in the stall activity.
    */
    private enum TurnStep
    {
        Pitch,      /**< User practices progressive pitch angles with aerodynamic visualization */
        Complete    /**< Activity finished, awaiting cleanup */
    }
    
    private float[] pitchTargets = { 3f, 7f, 10f, 14f, 16f, 18f };  /**< Progressive pitch angle targets in degrees leading to stall */
    private int currentPitchTarget = 0;                              /**< Index of current pitch target in pitchTargets array */
    private TurnStep currentStep;                                    /**< Current training step in the activity sequence */
    private bool inputLocked = false;                                /**< Flag to disable user input during transitions and timelines */
    private float currentPitch = 0f;                                 /**< Current aircraft pitch angle in degrees, updated each frame */

    private PlayableDirector[] pitchTimelines;          /**< PlayableDirectors corresponding to pitchTimelineAssets */
    private AxisRotationController aoaManipulator;      /**< Controller for aircraft pitch manipulation */
    private ProceduralAirflow proceduralAirflow;        /**< Airflow visualization system for force arrows, particles, and COP */

    /**
    Initialize activity on component enable
    
    Enables input actions, resets progression state to initial values, retrieves
    references to aircraft controllers and airflow visualization system, and finds
    PlayableDirectors for all timeline assets. Activates all aerodynamic visualization
    features (force arrows, particle system, center of pressure indicator).
    */
    public void OnEnable()
    {
        if (rotationInput != null && rotationInput.action != null)
        {
            rotationInput.action.Enable();
        }
        // reset progression
        currentStep = TurnStep.Pitch;
        currentPitchTarget = 0;
        inputLocked = false;
        currentPitch = 0f;
        // Manipulators
        aoaManipulator = ActivityHelper.getAxisRotationController(0f, 0f);
        proceduralAirflow = ActivityHelper.getProceduralAirflow();
        // Call helper to find directors for timeline assets
        pitchTimelines = ActivityHelper.FindPlayableDirector(pitchTimelineAssets);
        proceduralAirflow.enableForceArrows = true;
        proceduralAirflow.enableParticleSystem = true;
        proceduralAirflow.enableCentreOfPressure = true;
        Debug.Log("Sec1StallActivity OnEnable completed");
    }

    /**
    Cleanup activity on component disable
    
    Disables input actions and deactivates all aerodynamic visualization features
    (force arrows, particle system, center of pressure indicator).
    */
    public void OnDisable()
    {
        if (rotationInput != null && rotationInput.action != null)
        {
            rotationInput.action.Disable();
        }
        proceduralAirflow.enableForceArrows = false;
        proceduralAirflow.enableParticleSystem = false;
        proceduralAirflow.enableCentreOfPressure = false;
    }

    /**
    Start the activity when triggered by ModuleActivityScheduler
    
    Called by the activity scheduler to begin the training sequence.
    Unlocks input for the first step.
    */
    public void StartActivity()
    {
        EnableCurrentStep();
    }

    public void StopActivity()
    {
        StopAllCoroutines();
        if (pitchTimelines != null)
        {
            foreach (var director in pitchTimelines)
            {
                if (director != null) director.Stop();
            }
        }
    }

    /**
    Unlock input for the current training step
    
    Resets input lock and controller origin flag to allow user control.
    Called after transitions and timeline completions.
    */
    public void EnableCurrentStep()
    {
        inputLocked = false;
        hasControllerOrigin = false;
    }

    /**
    Main update loop
    
    Routes execution to appropriate handler based on current training step.
    Notifies activity scheduler upon completion.
    */
    void Update()
    {
        switch ((TurnStep)currentStep)
        {
            case TurnStep.Pitch:
                HandlePitching();
                break;
            case TurnStep.Complete:
                mas.OnExternalStepCompleted();
                break;
        }
    }


    /**
    Handle progressive pitch training step
    
    Captures controller origin on first frame, reads current pitch angle, and checks
    if user has achieved the current target angle. When target is reached, locks input,
    plays confirmation timeline, and advances to next pitch target or completion. The
    progression through six pitch angles (3, 7, 10, 14, 16, 18) demonstrates
    increasing aerodynamic forces and changing center of pressure as the aircraft
    approaches stall conditions. Allows controller or debug mouse input when target
    not yet achieved.
    
    Aerodynamic visualizations update in real-time:
    - Force arrows show changing lift, drag, thrust, and weight magnitudes
    - Particle system shows airflow separation at higher angles
    - Center of pressure indicator moves along chord as angle increases
    */
    public void HandlePitching()
    {
        if (inputLocked) return;

        // Read current rotation from the input
        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();

        if (!hasControllerOrigin)
        {
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
            Debug.Log("Captured controller origin: " + controllerOrigin.eulerAngles);
        }


        currentPitch = ActivityHelper.getNormalisedPlanePitch(aoaManipulator);

        if (ActivityHelper.checkExactRotationTargetAchieved(currentPitch, pitchTargets[currentPitchTarget], stepAdvanceTolerance))
        {
            inputLocked = true;
            hasControllerOrigin = false;
            aoaManipulator.LerpAOA(pitchTargets[currentPitchTarget]);
            StartCoroutine(ActivityHelper.PlayTimeLine(pitchTimelines[currentPitchTarget], () =>
            {
                EnableCurrentStep();
                currentPitchTarget++;
                Debug.Log("currentPitch target: " + currentPitchTarget + " pitchTarget Length: " + pitchTargets.Length);
                if (currentPitchTarget >= pitchTargets.Length)
                {
                    // Advance to completion
                    currentStep = TurnStep.Complete;
                    Debug.Log("currentPitchTarget: " + currentPitchTarget + " pitchTarget length: " + pitchTargets.Length + " currentStep " + currentStep);
                }
                mas.OnExternalStepCompleted();
            }));
        }
        else
        {
            if (useMouseClicks)
            {
                ActivityHelper.DebugMouseControlPitch(mouseRollSpeed, aoaManipulator);
            }
            else
            {
                ActivityHelper.controllerPitchControl(pitchSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.x);
            }
        }
    }
}