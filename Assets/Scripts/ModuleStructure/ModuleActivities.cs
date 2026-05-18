using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "NewModuleActivity", menuName = "Module Activity")] // Add a menu control to make an activity!!! This is so cool i love this

public class ModuleActivities : ScriptableObject
{
    public string activityName;
    public string[] instructions; // The list of instructions, each corresponding to one of six checkboxes

    public bool usesCustomScript = false; // If an activity is more involved, we can opt to use a custom script to handle it instead. This will act as an Adapter of sorts to whatever goes on in the activity
    public GameObject customScript; // The actual GameObject that contains the custom script, if needed (this will be instanced from a prefab!!)

    public bool playerInputRequired = true; // Acts as a check that when toggled, completes the activity

    public int totalSteps = 1; // Total number of input steps for instructions, max of six, default to one
    public int currentStep = 0;

    // If activity will be handled through a custom script, instantiate the prefab that contains said script
    public GameObject PrefabSetup(Transform parent)
    {
        if (usesCustomScript && customScript != null)
        {
            // Instantiate the new script from a prefab
            GameObject instance = GameObject.Instantiate(customScript, parent);
            instance.name = $"{activityName}_ControllerInstance";
            return instance;
        }

        // No custom script required, skip prefab setup
        return null;
    }

    // Self-explanatory.
    public bool AdvanceStep()
    {
        currentStep++;

        // Check if the whole activity has been completed
        if (currentStep >= totalSteps)
        {
            return true;
        }

        // Activity is not yet complete, move onto next step
        return false;
    }
}