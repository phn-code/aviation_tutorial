using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/**
Section 6 spin recovery training activity controller

Manages a PARE (Power-Ailerons-Rudder-Elevator) spin recovery training exercise where
users practice the four-step procedure to recover from an aircraft spin. The aircraft
begins in an active spin state (rotating continuously in yaw) and users must execute
the correct recovery sequence to stop the spin and return to level flight. Implements
the IActivityController interface for integration with the ModuleActivityScheduler system.

PARE Recovery Sequence:
1. Power to Idle - Reduce throttle to below 10%
2. Ailerons Neutral - Automated demonstration (passive step)
3. Rudder Opposite - Apply opposite rudder to stop spin rotation
4. Elevator Forward - Push nose down to break the stall
5. Level Flight - Recovery complete

Key features:
- Continuous yaw rotation simulation during spin
- Configurable spin direction (left or right)
- Throttle control with visual display
- Real-time feedback on recovery inputs
- Automatic spin cessation when correct inputs applied

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
*/
public class Sec6SpinRecoveryActivity : MonoBehaviour, IActivityController
{
    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance;  /**< Reference to singleton activity scheduler */
    public GameObject throttleCanvasPrefab;     /**< Prefab for throttle display UI canvas */
    private GameObject throttleCanvasInstance;  /**< Instantiated throttle display canvas */
    private TMP_Text throttleText;              /**< Text component for displaying throttle percentage */


    private GameObject plane;
    private GameObject planeDuplicate;
    private GameObject anchorPoint;

    [Header("Timelines")]
    public TimelineAsset[] pareStepTimelineAssets;  /**< Timeline for each PARE step completion (4 timelines) */

    [Header("Input Action References")]
    public InputActionReference rotationInput;      /**< Controller rotation for pitch/bank control */
    public InputActionReference throttleInput;      /**< VR controller input for throttle control (Y-axis) */

    [Header("Control Settings")]
    public float controlSensitivity = 1f;           /**< Roll/pitch sensitivity multiplier for controller input */
    public float deadZone = 15f;                    /**< Controller deadzone threshold in degrees to filter noise */

    [Header("Spin Settings")]
    public bool spinDirectionRight = true;          /**< Spin direction: true = right spin, false = left spin */
    public float spinRotationSpeed = 180f;          /**< Yaw rotation speed during spin in degrees per second */
    public float initialSpinBank = 60f;             /**< Initial bank angle for spin in degrees */
    public float initialSpinAOA = 25f;              /**< Initial angle of attack for spin in degrees (stalled condition) */
    
    [Header("Editor Debug Settings")]
    public bool useMouseInput = false;              /**< Use mouse for testing without VR */
    public float mouseControlSpeed = 10000f;        /**< Speed multiplier for mouse control */

    private Quaternion controllerOrigin = Quaternion.identity;  /**< Controller starting orientation captured at step start */
    private bool hasControllerOrigin = false;                   /**< Flag indicating if controller origin has been captured */
    
    private AxisRotationController aoaManipulator;              /**< Reference to aircraft rotation controller */
    
    private TurnStep currentStep;                               /**< Current step in PARE recovery sequence */
    
    /**
    PARE recovery sequence states
    
    Defines the five steps of the PARE spin recovery procedure.
    */
    private enum TurnStep
    {
        powerToIdle,        /**< User reduces throttle to idle position */
        aileronsNeutral,    /**< Automated demonstration of neutral ailerons */
        rudderOpposite,     /**< User applies rudder opposite to spin direction */
        elevatorForward,    /**< User pushes elevator forward to break stall */
        levelFlight         /**< Recovery complete, return to level flight */
    }

    private PlayableDirector[] pareStepTimelines;       /**< Directors for PARE step confirmation timelines */
    
    private bool inputLocked = false;                   /**< Flag to disable user input during transitions */
    private float stepStartTime = 0f;                   /**< Time when current step started (for minimum step duration) */
    private float minimumStepTime = 0.5f;               /**< Minimum time in seconds before step can complete (prevents instant completion) */
    
    // Spin animation
    private bool isSpinning = true;                     /**< Flag indicating if aircraft is currently in spinning state */
    private float spinDirection = -1f;                   /**< Spin direction multiplier: 1 for right spin, -1 for left spin */
    
    // Current aircraft state
    private float currentThrottle = 1f;                 /**< Current throttle input value from controller */
    private float currentBank = 0f;                     /**< Current bank angle in degrees, updated each frame */
    private float currentPitch = 0f;                    /**< Current pitch angle in degrees, updated each frame */

    // Throttle values
    private float totalThrottle = 100;      /**< Current throttle percentage (0-100) displayed to user */
    private float throttleIncrement = 20;   /**< Throttle change rate in percent per second */

    /**
    Initialize activity on component enable
    
    Enables input actions, resets progression state to initial PARE step, retrieves
    references to aircraft controller, finds PlayableDirectors for timeline assets,
    and initializes the aircraft into spin state with appropriate bank and AOA.
    */
    public void OnEnable()
    {
        if (rotationInput != null && rotationInput.action != null)
        {
            rotationInput.action.Enable();
        }
        if (throttleInput != null && throttleInput.action != null)
        {
            throttleInput.action.Enable();
        }
        // reset progression
        currentStep = TurnStep.powerToIdle;
        inputLocked = false;
        // Manipulators
        aoaManipulator = ActivityHelper.getAxisRotationController(0f, 0f);
        // Call helper to find directors for timeline assets
        pareStepTimelines = ActivityHelper.FindPlayableDirector(pareStepTimelineAssets);


        plane = GameObject.Find("DA_40");
        planeDuplicate = GameObject.Find("DA_40_Duplicate");
        anchorPoint = GameObject.Find("EmptyAnchorPoint");

        planeDuplicate.SetActive(false);
        plane.transform.position = anchorPoint.transform.position;

        // Initialize spin state
        InitializeSpinState();
    }

    /**
    Cleanup activity on component disable
    
    Disables input actions for rotation and throttle control.
    */
    public void OnDisable()
    {
        if (rotationInput != null && rotationInput.action != null)
        {
            rotationInput.action.Disable();
        }
        if (throttleInput != null && throttleInput.action != null)
        {
            throttleInput.action.Disable();
        }
    }

    /**
    Initialize aircraft into spin state
    
    Sets the aircraft to initial spin conditions with high AOA (stalled) and
    appropriate bank angle based on spin direction. Uses lerp for smooth transition
    into spin state.
    */
    private void InitializeSpinState()
    {
        if (aoaManipulator != null)
        {
            // Set nose down AOA for stall
            aoaManipulator.LerpAOA(initialSpinAOA, 0.5f);
            
            // Set bank angle for spin
            float spinBank = spinDirectionRight ? initialSpinBank : -initialSpinBank;
            aoaManipulator.LerpBank(spinBank, 0.5f);
        }
    }

    /**
    Start the activity when triggered by ModuleActivityScheduler
    
    Called by the activity scheduler to begin the PARE recovery sequence.
    Unlocks input for the first step.
    */
    public void StartActivity()
    {
        EnableCurrentStep();
    }

    public void StopActivity()
    {
        StopAllCoroutines();
        if (pareStepTimelines != null)
        {
            foreach (var director in pareStepTimelines)
            {
                if (director != null) director.Stop();
            }
        }
    }

    /**
    Unlock input for the current training step

    Resets input lock and controller origin flag to allow user control, and records
    the step start time for minimum duration enforcement.
    */
    public void EnableCurrentStep()
    {
        inputLocked = false;
        hasControllerOrigin = false;
        stepStartTime = Time.time;
        Debug.Log($"Enabled step: {currentStep}");
    }

    /**
    Main update loop
    
    Continuously rotates aircraft during spin, captures controller origin, reads
    current aircraft state, and routes execution to appropriate PARE step handler.
    */
    void Update()
    {
        if (isSpinning)
        {
            SpinPlane();
        }

        if (inputLocked) return;

        // Capture controller origin on first input
        if (!hasControllerOrigin && rotationInput != null && rotationInput.action != null)
        {
            Quaternion currentRotation = rotationInput.action.ReadValue<Quaternion>();
            controllerOrigin = currentRotation;
            hasControllerOrigin = true;
            Debug.Log("Captured controller origin: " + controllerOrigin.eulerAngles);
        }

        // Read current aircraft state
        currentBank = ActivityHelper.getNormalisedPlaneRoll(aoaManipulator);
        currentPitch = ActivityHelper.getNormalisedPlanePitch(aoaManipulator);
        
        // Read throttle if available
        if (throttleInput != null && throttleInput.action != null)
        {
            currentThrottle = throttleInput.action.ReadValue<Vector2>().y;
        }

        switch (currentStep)
        {
            case TurnStep.powerToIdle:
                HandlePowerToIdle();
                break;
            case TurnStep.aileronsNeutral:
                HandleAileronsNeutral();
                break;
            case TurnStep.rudderOpposite:
                HandleRudderOpposite();
                break;
            case TurnStep.elevatorForward:
                HandleElevatorForward();
                break;
            case TurnStep.levelFlight:
                // Wait for mas to end activity
                break;
        }
    }

    /**
    Handle power reduction to idle step (PARE step 1)
    
    User must reduce throttle below 10% to complete this step. Continuously updates
    totalThrottle based on controller input and displays current value on canvas.
    When throttle drops below threshold, locks input, plays confirmation timeline,
    advances to next step, and destroys throttle display canvas.
    */
    private void HandlePowerToIdle()
    {
        //totalThrottle += Mathf.Sign(currentThrottle) * Time.deltaTime * throttleIncrement;
        
        if (Mathf.Abs(currentThrottle) > 0.3f)  // Only update if input is significant
        {
            totalThrottle += Mathf.Sign(currentThrottle) * Time.deltaTime * throttleIncrement;
        }
        
        totalThrottle = Mathf.Clamp(totalThrottle, 0, 100);

        Debug.Log($"Total throttle: {totalThrottle}     currentThrottle: {currentThrottle}");
        updateThrottleDisplay();
        // Check if throttle is at idle 
        if (totalThrottle < 10)
        {
            inputLocked = true;
            hasControllerOrigin = false;
            
            Debug.Log("Power to idle achieved!");
            
            StartCoroutine(ActivityHelper.PlayTimeLine(pareStepTimelines[0], () =>
            {
                currentStep = TurnStep.aileronsNeutral;
                // destroy throttle canvas
                Destroy(throttleCanvasInstance);
                EnableCurrentStep();
                mas.OnExternalStepCompleted();
            }));
        }
    }

    /**
    Handle ailerons neutral step (PARE step 2)
    
    Automated/passive demonstration step with no user input required. Immediately
    locks input and plays timeline, then advances to rudder opposite step.
    */
    private void HandleAileronsNeutral()
    {
        inputLocked = true;
        StartCoroutine(ActivityHelper.PlayTimeLine(pareStepTimelines[1], () =>
        {
            currentStep = TurnStep.rudderOpposite;
            EnableCurrentStep();
            mas.OnExternalStepCompleted();
        }));
    }

    /**
    Handle rudder opposite spin direction step (PARE step 3)
    
    User must apply rudder in direction opposite to spin by rolling controller:
    - Right spin requires left rudder (negative bank < -10)
    - Left spin requires right rudder (positive bank > 10)
    
    When correct rudder is applied and minimum time elapsed, stops spin rotation,
    locks input, lerps bank to neutral, plays confirmation timeline, and advances
    to elevator forward step. Allows controller or debug mouse input while waiting
    for correct input.
    */
    private void HandleRudderOpposite()
    {
        // User needs to roll controller opposite to spin direction
        bool rudderCorrect = false;
        
        if (spinDirectionRight)
        {
            // Right spin needs left rudder (negative bank)
            rudderCorrect = currentBank < -10f;
        }
        else
        {
            // Left spin needs right rudder (positive bank)
            rudderCorrect = currentBank > 10f;
        }

        if (rudderCorrect && (Time.time - stepStartTime) > minimumStepTime)
        {
            inputLocked = true;
            hasControllerOrigin = false;
            
            // Stop the spin!
            isSpinning = false;
            
            Debug.Log("Rudder opposite applied - stopping spin rotation!");
            
            // Lock at current bank to show rudder applied
            aoaManipulator.LerpBank(0, 5f);
            
            StartCoroutine(ActivityHelper.PlayTimeLine(pareStepTimelines[2], () =>
            {
                currentStep = TurnStep.elevatorForward;
                EnableCurrentStep();
                mas.OnExternalStepCompleted();
            }));
        }
        else
        {
            // Allow user to control bank/rudder
            if (useMouseInput)
            {
                ActivityHelper.DebugMouseControlRoll(mouseControlSpeed, aoaManipulator);
            }
            else
            {
                ActivityHelper.controllerRollControl(controlSensitivity, aoaManipulator, rotationInput, deadZone, controllerOrigin.eulerAngles.z);
            }
        }
    }

    /**
    Handle elevator forward to break stall step (PARE step 4)
    
    User must push controller forward to pitch nose down below -10, breaking the
    stall condition. When correct pitch achieved and minimum time elapsed, locks input,
    lerps AOA to 3 (recovered flight), plays confirmation timeline, and advances to
    level flight completion. Allows controller or debug mouse input while waiting for
    correct input.
    */
    private void HandleElevatorForward()
    {
        // User needs to pitch controller forward (nose down)
        if (currentPitch < -10f && (Time.time - stepStartTime) > minimumStepTime)
        {
            inputLocked = true;
            hasControllerOrigin = false;
            
            Debug.Log("Elevator forward - breaking the stall!");
            
            // Break the stall by reducing AOA
            aoaManipulator.LerpAOA(3f, 5f);
            
            StartCoroutine(ActivityHelper.PlayTimeLine(pareStepTimelines[3], () =>
            {
                currentStep = TurnStep.levelFlight;
                EnableCurrentStep();
                mas.OnExternalStepCompleted();
            }));
        }
        else
        {
            // Allow user to control pitch
            if (useMouseInput)
            {
                ActivityHelper.DebugMouseControlPitch(mouseControlSpeed, aoaManipulator);
            }
            else
            {
                ActivityHelper.controllerPitchControl(controlSensitivity, aoaManipulator, 
                    rotationInput, deadZone, controllerOrigin.eulerAngles.x);
            }
        }
    }

    /**
    Continuously rotate aircraft in yaw to simulate spinning
    
    Called each frame while isSpinning flag is true. Applies continuous yaw rotation
    at spinRotationSpeed in the direction specified by spinDirection multiplier.
    Stops when isSpinning is set to false in HandleRudderOpposite.
    */
    private void SpinPlane()
    {
        // Continuously rotate the aircraft in yaw to simulate spinning
        if (aoaManipulator != null)
        {
            float yawIncrement = spinRotationSpeed * spinDirection;
            aoaManipulator.IncrementYaw(yawIncrement * Time.deltaTime);
        }
    }

    /**
    Update throttle display canvas with current value
    
    Instantiates throttle canvas on first call and positions it in world space.
    Updates the TMP_Text component with current totalThrottle percentage value.
    
    @note Uses hardcoded position values that should be replaced with more robust system
    */
    private void updateThrottleDisplay()
    {
        if (throttleCanvasInstance == null)
        {
            throttleCanvasInstance = Instantiate(throttleCanvasPrefab);
            // This is bad, magic numbers are bad. please replace with a more robust system 
            throttleCanvasInstance.transform.position = new Vector3(798, 1450, 1897);
            throttleCanvasInstance.transform.rotation = Quaternion.Euler(0, -154, 0);

            // Find the child object named "ThrottleNumber" and get its TMP_Text
            TMP_Text[] texts = throttleCanvasInstance.GetComponentsInChildren<TMP_Text>(true);

            foreach (var t in texts)
            {
                if (t.gameObject.name == "ThrottleNumber")
                {
                    throttleText = t;
                    break;
                }
            }

            if (throttleText == null)
            {
                Debug.LogError("ERROR: Could not find TMP_Text named 'ThrottleNumber' under the prefab.");
            }
        }

        // Update the displayed throttle value
        if (throttleText != null)
        {
            // mahir - remove the decimals
            throttleText.text = ((int)totalThrottle).ToString();
        }

    }
}