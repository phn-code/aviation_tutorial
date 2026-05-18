using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/**
Descending turn training activity controller

Manages a multi-step descending turn exercise where users practice simultaneous banking
and pitch control, followed by recovery and stall demonstration. Implements the
IActivityController interface for integration with the ModuleActivityScheduler system.

Training sequence:
1. Initial Setup - Automatically positions aircraft to -10 pitch
2. Pitch - User pitches down by -5 degrees.
3. Bank - User banks left by 30 degrees.
4. Pitch Up - User recovers from descent by pitching up to +5
5. Stall - Automated stall demonstration timeline
6. Complete - Activity finished, notifies scheduler

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
@author mahz | do not try to contact me.
*/
public class DescendingTurnActivity : MonoBehaviour, IActivityController
{
    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance;  /**< Reference to singleton activity scheduler */

    [Header("timeline assets")]
    public TimelineAsset postBankAndPitch;      /**< Timeline played after achieving bank and pitch targets */
    public TimelineAsset postPitchUp;           /**< Timeline played after pitch recovery */
    public TimelineAsset postStall;             /**< Timeline demonstrating stall scenario */

    [Header("Input action references")]
    public InputActionReference rotationInput;  /**< VR controller rotation input action for aircraft control */

    [Header("Control Settings")]
    public float bankSensitivity = 45f;         /**< Roll/pitch sensitivity multiplier for controller input */
    public float deadZone = 0.1f;               /**< Controller deadzone threshold in degrees to filter noise */
    public float stepAdvanceTolerance = 3f;     /**< Tolerance in degrees for achieving target angles before advancing */

    private Quaternion controllerOrigin = Quaternion.identity;  /**< Initial controller orientation captured at step start */
    private bool hasControllerOrigin = false;                   /**< Flag indicating if controller origin has been captured this step */

    private bool hasNotifiedMAS = false;        /**< Failsafe flag to prevent multiple completion notifications to scheduler */

    [Header("Editor Debug settings")]
    public bool useMouseClicks = true;          /**< Enable mouse button control for testing without VR */
    public float mouseRollSpeed = 10f;          /**< Speed multiplier for mouse-based roll/pitch control */

    /**
    Training progression states
    
    Defines the sequence of training steps in the descending turn activity.
    */
    private enum TurnStep
    {
        InitialSetup,       /**< Automated positioning to starting pitch angle */
        Pitch,              /** User pitches down by -5 degrees */
        Bank,               /** User banks left by 30 degrees. */
        PitchUp,            /**< User recovers from descent by pitching up */
        Stall,              /**< Automated stall demonstration */
        Complete,           /**< Activity finished, awaiting cleanup */
        PlayingTimeline     /**< Transitional state while timeline is playing */
    }
    
    private bool rollInputLocked = false;       /**< Flag to disable roll input during transitions and timelines */
    private bool pitchInputLocked = false;      /**< Flag to disable pitch input during transitions and timelines */
    private TurnStep currentStep;               /**< Current training step in the activity sequence */
    private float currentBank = 0f;             /**< Current aircraft bank angle in degrees, updated each frame */
    
    // Manipulators
    private AxisRotationController aoaManipulator;  /**< Controller for aircraft pitch and roll manipulation */
    private BankGhostTrailBehaviour bankGhostTrail; /**< Visual trail effect showing aircraft bank history */
    
    // Timeline PlayableDirectors
    private PlayableDirector postBankAndPitchDirector;  /**< PlayableDirector for post bank-and-pitch timeline */
    private PlayableDirector postPitchUpDirector;       /**< PlayableDirector for post pitch-up timeline */
    private PlayableDirector postStallDirector;         /**< PlayableDirector for stall demonstration timeline */

    /**
    Initialize activity on component enable
    
    Enables input actions, resets progression state to initial values, retrieves
    references to aircraft controllers and visual effects, and finds PlayableDirectors
    for all timeline assets. Initializes aircraft to -10 pitch for descending turn start.
    */
    public void OnEnable()
    {
        if (rotationInput != null && rotationInput.action != null)
        {
            rotationInput.action.Enable();
        }

        // reset progression
        currentStep = TurnStep.InitialSetup;
        rollInputLocked = false;
        pitchInputLocked = false;
        hasControllerOrigin = false;

        currentBank = 0f;

        aoaManipulator = ActivityHelper.getAxisRotationController(-10f, 0f);
        bankGhostTrail = ActivityHelper.getBankGhostTrail(true);

        // Call helper to find directors for timeline assets
        postBankAndPitchDirector = ActivityHelper.FindPlayableDirector(postBankAndPitch);
        postPitchUpDirector = ActivityHelper.FindPlayableDirector(postPitchUp);
        postStallDirector = ActivityHelper.FindPlayableDirector(postStall);
    }

    /**
    Cleanup activity on component disable
    
    Disables input actions and turns off visual trail effects.
    */
    public void OnDisable()
    {
        if (rotationInput != null && rotationInput.action != null)
        {
            rotationInput.action.Disable();
        }
        bankGhostTrail.enable = false;
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
        if (postBankAndPitchDirector != null) postBankAndPitchDirector.Stop();
        if (postPitchUpDirector != null) postPitchUpDirector.Stop();
        if (postStallDirector != null) postStallDirector.Stop();
    }

    /**
    Unlock input for the current training step

    Resets roll and pitch input locks and controller origin flag to allow user control.
    Called after transitions and timeline completions.
    */
    public void EnableCurrentStep()
    {
        rollInputLocked = false;
        pitchInputLocked = false;
        hasControllerOrigin = false;
    }

    /**
    Main update loop
    
    Routes execution to appropriate handler based on current training step.
    Logs current step for debugging purposes.
    */
    void Update()
    {
        Debug.Log("Current Step: " + currentStep);
        switch ((TurnStep)currentStep)
        {
            case TurnStep.InitialSetup:
                CheckSetupComplete();
                break;
            case TurnStep.Pitch:
                HandlePitchDown();
                break;
            case TurnStep.Bank:
                HandleBank();
                break;
            case TurnStep.PitchUp:
                HandlePitch();
                break;
            case TurnStep.Stall:
                HandleStall();
                break;
            case TurnStep.Complete:
                if (!hasNotifiedMAS) // failsafe for preventing multiple triggers
                {
                    Debug.Log("Descending Turn Activity Complete - notifying MAS");
                    mas.OnExternalStepCompleted();
                    hasNotifiedMAS = true;
                }
                break;
        }
    }

    /**
    Check if initial setup positioning is complete
    
    Monitors the AxisRotationController lerp operations to detect when the aircraft
    has finished moving to the initial -10 pitch position. Advances to Pitch 
    step when both pitch and roll lerps are complete.
    */
    public void CheckSetupComplete()
    {
        if (!aoaManipulator.IsLerpingPitch() && !aoaManipulator.IsLerpingRoll())
        {
            currentStep = TurnStep.Pitch;
            EnableCurrentStep();
        }
    }

    /**
    Handle pitch down training step
    
    Captures controller origin on first frame, reads current pitch angle, and checks
    if user has achieved -5 pitch target to begin descending. When target is reached,
    locks input, advances to Bank step. Allows controller or debug mouse input when
    target not yet achieved.
    */

    public void HandlePitchDown () {
        if (pitchInputLocked) return;

        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();
        if (!hasControllerOrigin){
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
        }

        float currentPitch = ActivityHelper.getNormalisedPlanePitch(aoaManipulator);

        if (ActivityHelper.checkExactRotationTargetAchieved(currentPitch, -5f, stepAdvanceTolerance)){
            Debug.Log($"Pitch down target achieved: {currentPitch}");
            pitchInputLocked = true;
            hasControllerOrigin = false;
            aoaManipulator.LerpAOA(-5f, 0.5f);
            currentStep = TurnStep.Bank;
            EnableCurrentStep();
            mas.OnExternalStepCompleted();
        } else {
            if (useMouseClicks){
                ActivityHelper.DebugMouseControlPitchDown(mouseRollSpeed, aoaManipulator);
            } else {
                ActivityHelper.controllerPitchControl(bankSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.x);
            }
        }
    }

     /**
    Handle bank training step
    
    Captures controller origin on first frame, reads current bank angle, and checks
    if user has achieved 30 degree bank target. When target is reached, locks input,
    plays confirmation timeline, and advances to pitch recovery step. Allows controller
    or debug mouse input when target not yet achieved.
    */

    public void HandleBank() {
        if (rollInputLocked) return;

        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();
        if (!hasControllerOrigin){
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
        }

        float planeRoll = ActivityHelper.getNormalisedPlaneRoll(aoaManipulator);

        if (ActivityHelper.checkExactRotationTargetAchieved(planeRoll, 30f, stepAdvanceTolerance)){
            Debug.Log($"Bank target achieved: {planeRoll}");
            rollInputLocked = true;
            hasControllerOrigin = false;
            aoaManipulator.LerpBank(30f);
            StartCoroutine(ActivityHelper.PlayTimeLine(postBankAndPitchDirector, () => {
                Debug.Log("Timeline complete, advancing to PitchUp");
                currentStep = TurnStep.PitchUp;
                rollInputLocked = false;
                EnableCurrentStep();
                mas.OnExternalStepCompleted();
            }));
        } else {
            if (useMouseClicks) {
                ActivityHelper.DebugMouseControlRollLeft(mouseRollSpeed, aoaManipulator);
            } else {
                ActivityHelper.controllerRollControl(bankSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.z);
            }
        }
    }

    /**
    public void HandleBankAndPitch()
    {
        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();
        if (!hasControllerOrigin)
        {
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
        }

        Quaternion relativeRotation = Quaternion.Inverse(controllerOrigin) * currentRotation;
        float relativeZ = Mathf.DeltaAngle(0f, relativeRotation.eulerAngles.z);
        float relativeX = Mathf.DeltaAngle(0f, relativeRotation.eulerAngles.x);

        float planeRoll = ActivityHelper.getNormalisedPlaneRoll(aoaManipulator);
        float planePitch = ActivityHelper.getNormalisedPlanePitch(aoaManipulator);

        bool rollAchieved = ActivityHelper.checkExactRotationTargetAchieved(planeRoll, 30f, stepAdvanceTolerance);
        bool pitchAchieved = ActivityHelper.checkExactRotationTargetAchieved(planePitch, -5f, stepAdvanceTolerance);

        Debug.Log("rollAchieved: " + rollAchieved + " pitchAchieved: " + pitchAchieved);
        if (rollAchieved)
        {
            rollInputLocked = true;
            aoaManipulator.LerpBank(30f);
        }
        if (pitchAchieved)
        {
            // Here, lerp causes issue. fix tomorrow
            pitchInputLocked = true;
            aoaManipulator.LerpAOA(-5f);
        }

        if (rollInputLocked && pitchInputLocked)
        {
            currentStep = TurnStep.PlayingTimeline;
            hasControllerOrigin = false;
            Debug.Log("Both roll and pitch achieved, starting coroutine...");
            StartCoroutine(ActivityHelper.PlayTimeLine(postBankAndPitchDirector, () =>
            {
                Debug.Log("Timeline complete, advancing to PitchUp");
                currentStep = TurnStep.PitchUp;
                rollInputLocked = false;
                pitchInputLocked = false;
                EnableCurrentStep();
                mas.OnExternalStepCompleted();
            }));
        }

        if (!rollInputLocked)
        {
            if (useMouseClicks)
            {
                ActivityHelper.DebugMouseControlRollLeft(mouseRollSpeed, aoaManipulator);
            }
            else
            {
                ActivityHelper.controllerRollControl(bankSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.z);
            }
        }
        if (!pitchInputLocked)
        {
            if (useMouseClicks)
            {
                ActivityHelper.DebugMouseControlPitchUp(mouseRollSpeed, aoaManipulator);
            }
            else
            {
                ActivityHelper.controllerPitchControl(bankSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.x);
            }
        }
    }
    */

    /**
    Handle pitch recovery training step
    
    Captures controller origin on first frame, reads current pitch angle, and checks
    if user has achieved +5 pitch target to recover from the descent. When target is
    reached, locks input, plays confirmation timeline, and advances to stall demonstration.
    Allows controller or debug mouse input when target not yet achieved.
    */
    public void HandlePitch()
    {
        if (pitchInputLocked) return;

        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();
        if (!hasControllerOrigin)
        {
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
        }

        float currentPitch = ActivityHelper.getNormalisedPlanePitch(aoaManipulator);

        if (ActivityHelper.checkExactRotationTargetAchieved(currentPitch, 5f, stepAdvanceTolerance))
        {
            Debug.Log($"Pitch target achieved: {currentPitch}");
            pitchInputLocked = true;
            hasControllerOrigin = false;
            aoaManipulator.LerpAOA(5f, 0.5f);
            StartCoroutine(ActivityHelper.PlayTimeLine(postPitchUpDirector, () =>
            {
                pitchInputLocked = false;
                currentStep = TurnStep.Stall;
                EnableCurrentStep();
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
                ActivityHelper.controllerPitchControl(bankSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.x);
            }
        }
    }

    /**
    Handle automated stall demonstration step
    
    Locks pitch input and immediately plays the stall demonstration timeline.
    Advances to completion state after timeline finishes. This is a passive
    demonstration step with no user input required.
    */
    public void HandleStall()
    {
        if (pitchInputLocked) return;
        {
            pitchInputLocked = true;
            StartCoroutine(ActivityHelper.PlayTimeLine(postStallDirector, () =>
            {
                EnableCurrentStep();
                pitchInputLocked = false;
                currentStep = TurnStep.Complete;
            }));
        }
    }      
}
