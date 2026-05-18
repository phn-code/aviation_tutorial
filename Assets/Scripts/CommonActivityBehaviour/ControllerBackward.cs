using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/**
@author Chintana Luu | luucy003@mymail.unisa.edu.au
*/
public class ControllerBackward : MonoBehaviour, IActivityController
{
    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance; // Reference to MAS singleton

    public InputActionReference rotateAction; // Input action for rotating the left controller

    [Range(0,1)]
    public float intensity = 1f;
    public float duration = 5f;

    public float minZPositionThreshold = 0f; // Default, can be changed in inspector
    public float maxZPositionThreshold = 90f; // 1.0f

    private bool activityEnabled = false; // Start as disabled


    public void Start()
    {
        XRBaseInteractable interactable = GetComponent<XRBaseInteractable>();
        interactable.activated.AddListener(TriggerHaptic);
    }

    // Subscribe to event listener and enable input action
    private void OnEnable()
    {
        if (rotateAction != null)
        {
            //rotateAction.action.performed += OnRotatePerformed;
            rotateAction.action.performed += OnPositionChange;
            rotateAction.action.Enable();
        }
    }

    // Unsubscribe to event listener and disable input action
    private void OnDisable()
    {
        if (rotateAction != null)
        {
            //rotateAction.action.performed -= OnRotatePerformed;
            rotateAction.action.performed -= OnPositionChange;
            rotateAction.action.Disable();
        }
    }

    // 
    public void StartActivity()
    {
        activityEnabled = true;
        //      rotateAction.action.Enable();
    }

    public void StopActivity()
    {
        activityEnabled = false;
    }

    // Wrapper for TriggerHaptic method.
    public void TriggerHaptic(BaseInteractionEventArgs e) {

        if (e.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor) {
            TriggerHaptic(controllerInteractor.xrController);
        }
    }

    public void TriggerHaptic(XRBaseController controller) {
        if (intensity > 0) {
            controller.SendHapticImpulse(intensity, duration);
        }
    }


    private void OnPositionChange(InputAction.CallbackContext ctx)
    {

        Quaternion controllerPosition = ctx.ReadValue<Quaternion>();
        float zPosition = Mathf.DeltaAngle(0f, controllerPosition.normalized.z);
        //float position = ctx.ReadValue<float>();// Get the value from the controller

        if (!activityEnabled) return;

        // Same logic as ControllerForward.cs but using negative values this time.
        if (zPosition <= -minZPositionThreshold && zPosition >= -maxZPositionThreshold)
        {

            // Send a haptic to the user so the user knows they are within range, also could be a good idea to add an angle meter for the DA-40 rotation.


            AxisRotationController aoaManip = GameObject.FindAnyObjectByType<AxisRotationController>();
            aoaManip.LerpAOA(-10); // Lower AOA by 10 degrees.
            //currentIndex++;

            mas.OnExternalStepCompleted();

            // This is a one-action activity, so disable after completed input
            activityEnabled = false;

            // Clean-up
            Destroy(gameObject); // there is only one step in the activity associated with this script.
        }
    }
}