using UnityEngine;
using UnityEngine.InputSystem;

public class IncreaseThrottle : MonoBehaviour, IActivityController
{
    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance; // Reference to MAS singleton

    public InputActionReference inputAction; // Input action for control pad on left controller
    public float throttleThreshold = 0.1f; // Default, can be changed in inspector
    private bool activityEnabled = false; // Start as disabled

    private bool hasTriggered = false; // Prevent multi-triggering

    // Subscribe to event listener and enable input action
    private void OnEnable()
    {
        if (inputAction != null)
        {
            inputAction.action.performed += OnPadMoved;
            inputAction.action.canceled += OnPadReleased;
            inputAction.action.Enable();
        }
    }

    // Unsubscribe to event listener and disable input action
    private void OnDisable()
    {
        if (inputAction != null)
        {
            inputAction.action.performed -= OnPadMoved;
            inputAction.action.canceled -= OnPadReleased;
            inputAction.action.Disable();
        }
    }

    // 
    public void StartActivity()
    {
        activityEnabled = true;
        hasTriggered = false;
    }

    public void StopActivity()
    {
        activityEnabled = false;
    }

    private void OnPadMoved(InputAction.CallbackContext ctx)
    {
        if (!activityEnabled || hasTriggered) return;

        Debug.Log("Left control pad moved!");

        Vector2 axis = ctx.ReadValue<Vector2>(); // Get the axis to be read from the controller


        // Detect if control pad is pulled up/north beyond threshold
        if (axis.y > throttleThreshold)
        {

            // Put this here for testing that script works!
            //AOAManipulator aoaManip = GameObject.FindAnyObjectByType<AOAManipulator>();
            //aoaManip.LerpAOA(-30);

            // Step is considered completed
            hasTriggered = true;
            mas.OnExternalStepCompleted();

            activityEnabled = false;
            Destroy(gameObject);
        }
        else {

            AxisRotationController aoaManip = GameObject.FindAnyObjectByType<AxisRotationController>();
            aoaManip.LerpAOA(-180); // Rotate to different angles to test
        }
    }

    private void OnPadReleased(InputAction.CallbackContext ctx)
    {
        // Reset trigger when the player releases the stick
        Vector2 axis = ctx.ReadValue<Vector2>();
        if (Mathf.Abs(axis.y) < 0.1f)
        {
            hasTriggered = false;
        }
    }
}