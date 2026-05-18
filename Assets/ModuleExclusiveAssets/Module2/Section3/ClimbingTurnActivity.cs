using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/**
Climbing turn training activity controller

Manages a climbing turn exercise where users practice banking while maintaining a climbing
attitude, followed by an aggressive pitch to induce stall. Demonstrates the relationship
between climbing turns, slipstream effects, and stall conditions. Implements the
IActivityController interface for integration with the ModuleActivityScheduler system.

Training sequence:
1. SlipStream - Automated demonstration of slipstream effects during climb
2. Bank - User achieves 15 bank while maintaining 10 climb attitude
3. Stall - User increases pitch to 25 to induce stall condition
4. Complete - Activity finished, notifies scheduler

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
*/
public class ClimbingTurnActivity : MonoBehaviour, IActivityController
{
    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance;  /**< Reference to singleton activity scheduler */

    [Header("timeline assets")]
    public TimelineAsset postSlipStream;    /**< Timeline demonstrating slipstream effects during climb */
    public TimelineAsset postBank;          /**< Timeline played after achieving 15 bank target */
    public TimelineAsset postStall;         /**< Timeline demonstrating stall during climbing turn */

    [Header("Input action references")]
    public InputActionReference rotationInput;  /**< VR controller rotation input action for aircraft control */

    [Header("Control Settings")]
    public float bankSensitivity = 45f;         /**< Roll/pitch sensitivity multiplier for controller input */
    public float deadZone = 0.1f;               /**< Controller deadzone threshold in degrees to filter noise */
    public float stepAdvanceTolerance = 3f;     /**< Tolerance in degrees for achieving target angles before advancing */

    [Header("Editor Debug settings")]
    public bool useMouseClicks = true;          /**< Enable mouse button control for testing without VR */
    public float mouseRollSpeed = 10f;          /**< Speed multiplier for mouse-based roll/pitch control */

    /**
    Training progression states
    
    Defines the sequence of training steps in the climbing turn activity.
    */
    private enum TurnStep
    {
        SlipStream,     /**< Automated slipstream demonstration during initial climb */
        Bank,           /**< User performs banking while maintaining climb */
        Stall,          /**< User increases pitch to induce stall condition */
        Complete        /**< Activity finished, awaiting cleanup */
    }
    
    private bool rollInputLocked = false;       /**< Flag to disable roll input during transitions and timelines */
    private bool pitchInputLocked = false;      /**< Flag to disable pitch input during transitions and timelines */
    private TurnStep currentStep;               /**< Current training step in the activity sequence */
    private float currentBank = 0f;             /**< Current aircraft bank angle in degrees, updated each frame */

    private Quaternion controllerOrigin = Quaternion.identity;  /**< Initial controller orientation captured at step start */
    private bool hasControllerOrigin = false;                   /**< Flag indicating if controller origin has been captured this step */

    private bool hasNotifiedMAS = false;        /**< Failsafe flag to prevent multiple completion notifications to scheduler */

    // Manipulators
    private AxisRotationController aoaManipulator;  /**< Controller for aircraft pitch and roll manipulation */
    private BankGhostTrailBehaviour bankGhostTrail; /**< Visual trail effect showing aircraft bank history */
    
    // Timeline PlayableDirectors
    private PlayableDirector postSlipStreamDirector;    /**< PlayableDirector for slipstream demonstration timeline */
    private PlayableDirector postBankDirector;          /**< PlayableDirector for post-bank timeline */
    private PlayableDirector postStallDirector;         /**< PlayableDirector for stall demonstration timeline */

    /**
    Initialize activity on component enable
    
    Enables input actions, resets progression state to initial values, retrieves
    references to aircraft controllers and visual effects, and finds PlayableDirectors
    for all timeline assets. Initializes aircraft to 10 pitch (climbing attitude).
    */
    public void OnEnable()
    {
        if (rotationInput != null && rotationInput.action != null)
        {
            rotationInput.action.Enable();
        }

        // reset progression
        currentStep = TurnStep.SlipStream;
        rollInputLocked = false;
        pitchInputLocked = false;
        hasControllerOrigin = false;
        currentBank = 0f;

        aoaManipulator = ActivityHelper.getAxisRotationController(10f, 0f);
        bankGhostTrail = ActivityHelper.getBankGhostTrail(true);

        // Call helper to find directors for timeline assets
        postSlipStreamDirector = ActivityHelper.FindPlayableDirector(postSlipStream);
        postBankDirector = ActivityHelper.FindPlayableDirector(postBank);
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
        if (postSlipStreamDirector != null) postSlipStreamDirector.Stop();
        if (postBankDirector != null) postBankDirector.Stop();
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
    Handles completion notification with failsafe to prevent duplicate calls.
    */
    void Update()
    {
        switch ((TurnStep)currentStep)
        {
            case TurnStep.SlipStream:
                HandleSlipStream();
                break;
            case TurnStep.Bank:
                HandleBanking();
                break;
            case TurnStep.Stall:
                HandleStall();
                break;
            case TurnStep.Complete:
                if (!hasNotifiedMAS) // failsafe for preventing multiple triggers
                {
                    Debug.Log("Climbing Turn Activity Complete - notifying MAS");
                    mas.OnExternalStepCompleted();
                    hasNotifiedMAS = true;
                }
                break;
        }
    }

    /**
    Handle automated slipstream demonstration step
    
    Locks input and immediately plays the slipstream demonstration timeline on first call.
    Advances to banking step after timeline completes. This is a passive demonstration
    step with no user input required.
    
    @todo Slipstream animation/effect needs implementation
    */
    public void HandleSlipStream()
    {
        //TODO Slipstream Animation/effect
        if (rollInputLocked) return;
            rollInputLocked = true;
            StartCoroutine(ActivityHelper.PlayTimeLine(postSlipStreamDirector, () =>
            {

                currentStep = TurnStep.Bank;
                rollInputLocked = false;
            }));
    }

    /**
    Handle banking during climb training step
    
    Captures controller origin on first frame, reads current bank angle, and checks
    if user has achieved 15 bank target while maintaining the 10 climb attitude.
    When target is reached, locks input, plays confirmation timeline, and advances
    to stall demonstration. Allows controller or debug mouse input when target not
    yet achieved.
    */
    public void HandleBanking()
    {
        if (rollInputLocked) return;

        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();

        if (!hasControllerOrigin)
        {
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
            Debug.Log("Captured controller origin (Bank): " + controllerOrigin.eulerAngles);
        }

        currentBank = ActivityHelper.getNormalisedPlaneRoll(aoaManipulator);

        if (ActivityHelper.checkExactRotationTargetAchieved(currentBank, 15f, stepAdvanceTolerance))
        {
            hasControllerOrigin = false;
            rollInputLocked = true;
            aoaManipulator.LerpBank(15f);
            StartCoroutine(ActivityHelper.PlayTimeLine(postBankDirector, () =>
            {
                pitchInputLocked = false;
                currentStep = TurnStep.Stall;
                // TODO uncomment after testing
                mas.OnExternalStepCompleted();
            }));
        }
        else
        {
            if (useMouseClicks)
            {
                ActivityHelper.DebugMouseControlRoll(mouseRollSpeed, aoaManipulator);
            }
            else
            {
                ActivityHelper.controllerRollControl(bankSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.z);
            }
        }
    }

    /**
    Handle stall induction training step
    
    Captures controller origin on first frame, reads current pitch angle, and checks
    if user has achieved 25 pitch target to induce stall during the climbing turn.
    Uses absolute value comparison (checkRotationTargetAchieved) allowing either
    positive or negative 25 pitch. When target is reached, locks input, plays stall
    demonstration timeline, and advances to completion. Allows controller or debug
    mouse input when target not yet achieved.
    */
    public void HandleStall()
    {
        if (pitchInputLocked) return;

        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();

        if (!hasControllerOrigin)
        {
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
            Debug.Log("Captured controller origin (Stall): " + controllerOrigin.eulerAngles);
        }

        float currentPitch = ActivityHelper.getNormalisedPlanePitch(aoaManipulator);

        if (ActivityHelper.checkRotationTargetAchieved(currentPitch, 25f, stepAdvanceTolerance))
        {
            pitchInputLocked = true;
            hasControllerOrigin = false;
            aoaManipulator.LerpAOA(25f);
            StartCoroutine(ActivityHelper.PlayTimeLine(postStallDirector, () =>
            {
                // Advance to next stage
                currentStep = TurnStep.Complete;
                // TODO uncomment after testing
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
}