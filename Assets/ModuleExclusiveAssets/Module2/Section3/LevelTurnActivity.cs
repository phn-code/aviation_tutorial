using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/**
Level turn training activity controller

Manages a progressive banking exercise where users practice aircraft turns at increasing
bank angles (15, 30, 45, 60) followed by a stall recovery demonstration. Implements
the IActivityController interface for integration with the ModuleActivityScheduler system.

Training sequence:
1. Bank - Practice banking at progressive angles with VR controller input
2. Stall Setup - Automatically position aircraft to 60 bank
3. Stall - Practice pitch control to induce controlled stall
4. Complete - Wait for activity termination

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
*/
public class LevelTurnActivity : MonoBehaviour, IActivityController
{
   private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance;  /**< Reference to singleton activity scheduler */

    [Header("timelines")]
    public TimelineAsset[] bankTimelineAssets;      /**< Timeline assets for each bank angle milestone (15, 30, 45, 60) */
    public TimelineAsset stallTimelineAsset;        /**< Timeline asset for stall demonstration sequence */

    [Header("Input action references")]
    public InputActionReference rotationInput;      /**< VR controller rotation input action for aircraft control */

    [Header("Control Settings")]
    public float bankSensitivity = 45f;             /**< Roll sensitivity multiplier for controller input */
    public float deadZone = 0.1f;                   /**< Controller deadzone threshold in degrees to filter noise */
    public float stepAdvanceTolerance = 3f;         /**< Tolerance in degrees for achieving target angles before advancing */

    private Quaternion controllerOrigin = Quaternion.identity;  /**< Initial controller orientation captured at step start */
    private bool hasControllerOrigin = false;                   /**< Flag indicating if controller origin has been captured this step */

    [Header("Editor Debug settings")]
    public bool useMouseClicks = false;             /**< Enable mouse button control for testing without VR */
    public float mouseRollSpeed = 10000f;           /**< Speed multiplier for mouse-based roll/pitch control */

    /**
    Training progression states
    
    Defines the sequence of training steps in the level turn activity.
    */
    private enum TurnStep
    {
        Bank,           /**< User practices progressive banking angles */
        stallSetup,     /**< Automated positioning to 60 bank for stall */
        Stall,          /**< User practices pitch control to induce stall */
        Complete        /**< Activity finished, awaiting cleanup */
    }
    
    private float[] bankTargets = { 15f, 30f, 45f, 60f };   /**< Progressive bank angle targets in degrees */
    private int currentBankTarget = 0;                      /**< Index of current bank target in bankTargets array */
    private TurnStep currentStep;                           /**< Current training step in the activity sequence */
    private bool inputLocked = false;                       /**< Flag to disable user input during transitions and timelines */
    private float currentBank = 0f;                         /**< Current aircraft bank angle in degrees, updated each frame */

    private PlayableDirector[] bankTimelines;       /**< PlayableDirectors corresponding to bankTimelineAssets */
    private PlayableDirector stallTimeline;         /**< PlayableDirector for stall demonstration timeline */

    private AxisRotationController aoaManipulator;  /**< Controller for aircraft pitch and roll manipulation */
    private BankGhostTrailBehaviour bankGhostTrail; /**< Visual trail effect showing aircraft bank history */

    /**
    Initialize activity on component enable
    
    Enables input actions, resets progression state to initial values, retrieves
    references to aircraft controllers and visual effects, and finds PlayableDirectors
    for all timeline assets.
    */
    public void OnEnable()
    {
        if (rotationInput != null && rotationInput.action != null) 
        {
            rotationInput.action.Enable();
        }
        // reset progression
        currentStep = TurnStep.Bank;
        currentBankTarget = 0;
        inputLocked = false;
        currentBank = 0f;
        // Manipulators
        aoaManipulator = ActivityHelper.getAxisRotationController(0f, 0f);
        bankGhostTrail = ActivityHelper.getBankGhostTrail(true);
        // Call helper to find directors for timeline assets
        bankTimelines = ActivityHelper.FindPlayableDirector(bankTimelineAssets);
        stallTimeline = ActivityHelper.FindPlayableDirector(stallTimelineAsset);
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
        if (bankTimelines != null)
        {
            foreach (var director in bankTimelines)
            {
                if (director != null) director.Stop();
            }
        }
        if (stallTimeline != null) stallTimeline.Stop();
    }

    /**
    Unlock input for the current training step

    Resets input lock and controller origin flags to allow user control.
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
    */
    void Update()
    {
        switch ((TurnStep)currentStep)
        {
            case TurnStep.Bank:
                HandleBanking();
                break;
            case TurnStep.stallSetup:
                StallSetup();
                break;
            case TurnStep.Stall:
                HandleStall();
                break;
            case TurnStep.Complete:
                // Do nothing, wait for MAS to end activity
                break;
        }
    }

    /**
    Handle progressive banking training step
    
    Captures controller origin on first frame, reads current bank angle, and checks
    if user has achieved the current target angle. When target is reached, locks input,
    plays confirmation timeline, and advances to next bank target or stall setup.
    Allows controller or debug mouse input when target not yet achieved.
    */
    public void HandleBanking()
    {
        if (inputLocked) return;

        Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();

        if (!hasControllerOrigin)
        {
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
            Debug.Log("Captured controller origin: " + controllerOrigin.eulerAngles);
        }

        currentBank = ActivityHelper.getNormalisedPlaneRoll(aoaManipulator);

        if (ActivityHelper.checkRotationTargetAchieved(currentBank, bankTargets[currentBankTarget], stepAdvanceTolerance))
        {
            inputLocked = true;
            hasControllerOrigin = false;
            aoaManipulator.LerpBank(bankTargets[currentBankTarget] * Mathf.Sign(currentBank));
            StartCoroutine(ActivityHelper.PlayTimeLine(bankTimelines[currentBankTarget], () =>
            {
                EnableCurrentStep();
                currentBankTarget++;
                // TODO uncomment after testing
                mas.OnExternalStepCompleted();
                Debug.Log("currentbank target: " + currentBankTarget + " bankTarget Length: " + bankTargets.Length);
                if (currentBankTarget >= bankTargets.Length)
                {
                    // Advance to next stage
                    currentStep = TurnStep.stallSetup;
                    Debug.Log("currentbankTarget: " + currentBankTarget + " cbankTarget length: " + bankTargets.Length + " currentStep " + currentStep);
                }
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
    Automated setup for stall demonstration
    
    Locks input and automatically positions aircraft to 60 bank angle using lerp.
    Handles both positive and negative bank angles by detecting current bank sign.
    Advances to stall step when 60 target is achieved within tolerance.
    */
    public void StallSetup()
    {
        inputLocked = true;
        // set bank to 60 for initial state of stall timeline
        currentBank = ActivityHelper.getNormalisedPlaneRoll(aoaManipulator);
        if (currentBank < 0)
        {
            aoaManipulator.LerpBank(60f, 5f);
        }
        else
        {
            aoaManipulator.LerpBank(60f);
        }

        if (ActivityHelper.checkExactRotationTargetAchieved(currentBank, 60f, stepAdvanceTolerance))
        {
            aoaManipulator.LerpBank(60f, .5f);
            inputLocked = false;
            currentStep = TurnStep.Stall;
        }
    }

    /**
    Handle stall demonstration training step
    
    Captures controller origin on first frame, reads current pitch angle, and checks
    if user has achieved 10 pitch target to induce stall. When target is reached,
    locks input, plays stall timeline, and notifies activity scheduler of completion.
    Allows controller or debug mouse input for pitch control when target not yet achieved.
    */
    public void HandleStall()
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

        float currentPitch = ActivityHelper.getNormalisedPlanePitch(aoaManipulator);

        if (ActivityHelper.checkRotationTargetAchieved(currentPitch, 10f, stepAdvanceTolerance))
        {
            inputLocked = true;
            hasControllerOrigin = false;
            aoaManipulator.LerpAOA(10f);
            StartCoroutine(ActivityHelper.PlayTimeLine(stallTimeline, () =>
            {
                EnableCurrentStep();
                if (currentBankTarget >= bankTargets.Length)
                {
                    // Advance to next stage
                    currentStep = TurnStep.Stall;
                }                
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
