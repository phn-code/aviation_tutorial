using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


// I COPIED INCREASETHROTTLE.CS AND ADDDED COROUTINE TIMER!
public class AccelerateScript : MonoBehaviour, IActivityController
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


    IEnumerator WaitCoroutine() {
        yield return new WaitForSeconds(5);
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
        StopAllCoroutines();
        activityEnabled = false;
    }

    private void OnPadMoved(InputAction.CallbackContext ctx)
    {
        if (!activityEnabled || hasTriggered) return;

        Debug.Log("Left control pad moved!");

        Vector2 axis = ctx.ReadValue<Vector2>(); // Get the axis to be read from the controller


        // If axis.y > throttleThreshold for timerSeconds

        // Detect if control pad is pulled up/north beyond threshold
        if (axis.y > throttleThreshold)
        {
            StartCoroutine(WaitCoroutine()); // Wait for 5 seconds within range.

            // Step is considered completed
            hasTriggered = true;
            mas.OnExternalStepCompleted();

            activityEnabled = false;
            Destroy(gameObject);
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


    // I added an accesor method.
    public bool getHasTriggered() {
        return hasTriggered;
    }
}