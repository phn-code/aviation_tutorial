using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TiltController : MonoBehaviour, IActivityController
{
    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance; // Reference to MAS singleton

    public InputActionReference inputAction; // Input action for detecting rotation of controller
    public float rotationThreshold = 25f; // Default, can be changed in inspector

    // Inspector setting which axis to monitor input for
    public enum RotationAxis { Pitch, Yaw, Roll }
    public RotationAxis monitoredAxis;

    private Quaternion initialRotation;

    private bool activityEnabled = false; // Start as disabled
    private bool hasTriggered = false; // Prevent multi-triggering

    // Subscribe to event listener and enable input action
    private void OnEnable()
    {
        if (inputAction != null)
        {
            inputAction.action.performed += OnRotationChanged;
            inputAction.action.Enable();
        }
    }

    // Unsubscribe to event listener and disable input action
    private void OnDisable()
    {
        if (inputAction != null)
        {
            inputAction.action.performed -= OnRotationChanged;
            inputAction.action.Disable();
        }
    }

    // 
    public void StartActivity()
    {
        activityEnabled = true;
        hasTriggered = false;
        StartCoroutine(CaptureInitialRotationDelayed());
    }

    public void StopActivity()
    {
        StopAllCoroutines();
        activityEnabled = false;
    }

    // Capture a neutral position to calculate rotation change from (prevent instant triggering, depending on which direction the user has the controller sat in when the activity begins)
    private IEnumerator CaptureInitialRotationDelayed()
    {
        yield return new WaitForSeconds(0.1f); // short delay
        if (inputAction != null)
        {
            initialRotation = inputAction.action.ReadValue<Quaternion>();
            Debug.Log($"[TiltController] Captured neutral: {initialRotation.eulerAngles}");
        }
    }

    // Called whenever rotation input is received (likely every frame let's be real here)
    private void OnRotationChanged(InputAction.CallbackContext ctx)
    {
        if (!activityEnabled || hasTriggered) return;

        // Get the current angle of rotation from the controller
        Quaternion currentRotation = ctx.ReadValue<Quaternion>();

        // Obtain the difference between the current rotation and the initial neutral rotation
        Vector3 deltaEuler = (Quaternion.Inverse(initialRotation) * currentRotation).eulerAngles;

        // Normalize from 0 to 360deg to -180 to 180deg using helper script for predictable rotation comparison
        deltaEuler.x = NormalizeAngle(deltaEuler.x);
        deltaEuler.y = NormalizeAngle(deltaEuler.y);
        deltaEuler.z = NormalizeAngle(deltaEuler.z);

        // Pick the axis we�re monitoring
        float valueToCheck = 0f;
        switch (monitoredAxis)
        {
            case RotationAxis.Pitch:
                valueToCheck = deltaEuler.x;
                break;
            case RotationAxis.Yaw:
                valueToCheck = deltaEuler.y;
                break;
            case RotationAxis.Roll:
                valueToCheck = deltaEuler.z;
                break;
        }

        // The comparison! If rotation exceeds the threshold of the selected axis (in the correct polarity/direction +/-), we are in business!
        if ((rotationThreshold > 0 && valueToCheck >= rotationThreshold) || (rotationThreshold < 0 && valueToCheck <= rotationThreshold))
        {
            hasTriggered = true;
            mas.OnExternalStepCompleted();

            activityEnabled = false;
            Destroy(gameObject);
        }

        Debug.Log($"dEuler = {deltaEuler} | valueToCheck({monitoredAxis}) = {valueToCheck}");
    }

    // Normalize angle to workable range if applicable
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}