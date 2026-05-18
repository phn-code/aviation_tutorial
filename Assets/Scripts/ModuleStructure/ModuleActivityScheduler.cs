using System.Collections.Generic;
using UnityEngine;
using TMPro;

/** 
Singleton management class that talks to the ModuleManager to schedule activities ahead of Timeline endings, forming the interactive components of each section that make up a Module.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class ModuleActivityScheduler : MonoBehaviour
{
    [SerializeField] private GameObject checkboxPanel; /**< The UI panel that contains all of the checkboxes/instructions/title. */
    [SerializeField] private CheckboxManager checkboxManager; /**< Manages toggling checkboxes and descriptions. */
    [SerializeField] private TextMeshProUGUI activityTitle; /**< Instruction title (e.g. "Increase Pitch", "Increase Power", etc.). */

    [SerializeField] private ModuleManager moduleManager; /**< Reference to the ModuleManager. */

    private ModuleActivities currentActivity; /**< The activity that is currently running. */
    private int currentStepIndex = 0; /**< The current step index in the activity. */
    private bool activityInProgress = false; /**< Flag to determine if an activity is currently in progress or not. */
    private GameObject activeControllerObject; /**< The spawned ModuleActivities prefab for the current activity (if applicable). */

    public static ModuleActivityScheduler Instance; /**< Singleton accessor reference. */

    /**
    The setup for the Singleton design pattern implementation occurs when the GameObject this script is attached to is instantiated in the scene at runtime.
    @return void
    */
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /**
    Called by ModuleManager when a new activity starts, to setup UI and controller inputs/activity scripts.
    @param newActivity The activity to commence.
    @param manager A reference to the ModuleManager.
    @return void
    */
    public void StartActivity(ModuleActivities newActivity, ModuleManager manager)
    {
        // Should never happen, but stop a new activity from starting if it's already in progress
        if (activityInProgress)
        {
            return;
        }

        // Refresh activity attribs to new slate
        currentActivity = newActivity;
        moduleManager = manager;
        currentStepIndex = 0;
        activityInProgress = true;

        // UI setup
        checkboxPanel.SetActive(true);
        SetupActivityUI();

        // Optionally instantiate controller prefab
        if (currentActivity.customScript != null)
        {
            activeControllerObject = Instantiate(currentActivity.customScript, new Vector3(0, 0, 0), Quaternion.identity);

            var controller = activeControllerObject.GetComponent<IActivityController>();
            if (controller != null)
            {
                controller.StartActivity();
            }
        }

        // Display first step! Let's get this party started
        DisplayCurrentStep();
    }

    /**
    Calls the checkboxManager to setup the special checbox UI for each activity, passing the relevant instructions.
    @return void
    */
    private void SetupActivityUI()
    {
        checkboxManager.SetupSteps(currentActivity.instructions);
    }

    /**
    Displays the activity text as the header of the checkbox panel in the UI.
    @return void
    */
    private void DisplayCurrentStep()
    {
        if (currentStepIndex < currentActivity.instructions.Length)
        {
            activityTitle.text = currentActivity.activityName;
            checkboxManager.HighlightStep(currentStepIndex);
        }
        else // If there are no more steps, the activity is complete!
        {
            CompleteActivity();
        }
    }

    /**
    When an activity's current step is completed, the MAS moves onto the next step if applicable. If there are no more steps, we move onto the following timeline instead by calling the moduleManager to proceed with relevant logic.
    @return void
    */
    private void OnStepCompleted()
    {
        Debug.Log($"Step completed: {currentStepIndex}");
        // Check that box!
        checkboxManager.CompleteStep(currentStepIndex);
        currentStepIndex++;

        if (currentStepIndex < currentActivity.instructions.Length)
        {
            Debug.Log("Displaying next step...");
            DisplayCurrentStep();
        }
        else
        {
            Debug.Log("All steps done, completing activity...");
            CompleteActivity();
        }
    }

    /**
    Public-facing method to complete a step, to be called by activities like RotateLeftController.
    @return void
    */
    public void OnExternalStepCompleted()
    {
        Debug.Log("MAS: OnExternalStepCompleted() called!");
        OnStepCompleted();
    }

    /**
    When an activity is completed, turn the UI checkbox panel off, cleanup flags, and call the moduleManager to move onto the next timeline.
    @return void
    */
    private void CompleteActivity()
    {
        Debug.Log($"Completing activity: {currentActivity}");
        // Cleaning up
        activityInProgress = false;
        if (activeControllerObject != null)
        {
            Destroy(activeControllerObject);
        }

        checkboxPanel.SetActive(false);
        currentActivity = null;

        Debug.Log("Calling ModuleManager.OnActivityComplete()");
        moduleManager.OnActivityComplete();
    }
    /*
    code used to reset activity to ensure that an activity is not running during changing scenes 
    */
    public void ActivityReset()
    {
        activityInProgress = false;
        currentStepIndex = 0;
        currentActivity = null;

        //steps of activities (referenced from startActivity function)
        if (activeControllerObject != null)
        {
            var controller = activeControllerObject.GetComponent<IActivityController>();
            if (controller != null)
            {
                controller.StopActivity(); // stop coroutines and internal PlayableDirectors before destroying
            }
            Destroy(activeControllerObject);
        }

        //stops the checkbox panel if there is one in the certain section
        checkboxPanel.SetActive(false);
    }
}