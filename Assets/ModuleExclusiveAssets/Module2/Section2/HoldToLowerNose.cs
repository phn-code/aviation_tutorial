using System.Collections;
//using Unity.Tutorials.Core.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/**
@author Chintana Luu | luucy003@mymail.unisa.edu.au
*/


public class HoldToLowerNose : MonoBehaviour, IActivityController
{
    private ModuleActivityScheduler mas => ModuleActivityScheduler.Instance; // Reference to MAS singleton

    public InputActionReference rotateAction; // Input action for rotating the left controller

    public float minRotationThreshold = 0.1f; // Default, can be changed in inspector
    public float maxRotationThreshold = 90f;

    //public float minZPositionThreshold = 0.1f; // Default, can be changed in inspector
    //public float maxZPositionThreshold = 1.0f;

    Quaternion initialControllerRotation;
    Quaternion controllerRotation;
    Vector3 initialControllerRot;
    Vector3 controllerRot;

    //public ActionBasedController controller;

    private bool waitPeriodOver = false;


    // Controller Position.
    private bool activityEnabled = false; // Start as disabled

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
        StopAllCoroutines();
        activityEnabled = false;
    }


    IEnumerator WaitPeriod()
    {
        yield return new WaitForSeconds(3);
        waitPeriodOver = true;
    }



    /**
    This script checks if the nose is lowered in the correct range and then waits the for number of seconds specified in the Wait coroutine.
    However, for future developers I suggest adding some haptics and improving the threshold range to make it easier for the user to complete this step.

    and this is an extra line
    @param no params in this method lol
    @param none
    @return void
    */

    // Check if user has moved the controller forward.
    // i.e: PUSH THE CONTROL COLUMN FORWARD TO LOWER AOA.

    private void OnPositionChange(InputAction.CallbackContext ctx)
    {

        //if (initialControllerRotation == null)
        //{
        //    initialControllerRotation = ctx.ReadValue<Quaternion>(); // Store initial position.

        //    initialControllerRot = initialControllerRotation.eulerAngles;
        //}

        controllerRotation = ctx.ReadValue<Quaternion>();
        controllerRot = controllerRotation.eulerAngles;

        // Get the X rotation.
        float xRotation = Mathf.DeltaAngle(0f, controllerRotation.normalized.x); // fixed changed this to x

        if (!activityEnabled) return;


        // 0.1f, -30f
        if (xRotation >= minRotationThreshold && xRotation <= maxRotationThreshold)
        {
            //controller.SendHapticImpulse(1, 5f);

            //StartCoroutine(WaitPeriod());
            //if (initialControllerRot.magnitude > controllerRot.magnitude) // remove this for testing

            StartCoroutine(WaitPeriod());
            
            AxisRotationController aoaManip = GameObject.FindAnyObjectByType<AxisRotationController>();
            aoaManip.LerpAOA(-10); // Lower nose by 10 degrees.


            
        }
    }


    public void Update()
    {

        if (waitPeriodOver == true)
        {
            waitPeriodOver = false; // Run the code only once.

            mas.OnExternalStepCompleted();

            // This is a one-action activity, so disable after completed input
            activityEnabled = false;

            // Clean-up
            Destroy(gameObject); // there is only one step in the activity associated with this script.

        }

    }
}