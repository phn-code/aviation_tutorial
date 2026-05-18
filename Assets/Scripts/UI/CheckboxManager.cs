using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/** 
A central class to manage checkboxes in tandem with activities to support the user 

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class CheckboxManager : MonoBehaviour
{
    [SerializeField] private List<CheckboxGroupUI> checkboxGroups = new List<CheckboxGroupUI>(); /**< Collection of all of the CheckboxGroupUI instances. */

    private int totalSteps = 0; /**< The number of steps in the current activity. */

    /**
    Called by the ModuleActivityScheduler, sets up a number of checkboxes and their descriptions based on activity content.
    @param instructions The series of instructions that each checkbox group's descriptions will be sequentially set to.
    @return void
    */
    public void SetupSteps(string[] instructions)
    {
        totalSteps = instructions.Length;

        // Hide all checkboxes and descriptions first before changing them and bringing them back changed, and with the correct quantity
        foreach (var group in checkboxGroups)
        {
            group.SetVisible(false);
            group.SetChecked(false);
        }

        for (int i = 0; i < totalSteps && i < checkboxGroups.Count; i++)
        {
            var group = checkboxGroups[i];

            group.SetLabel(instructions[i]);
            group.SetChecked(false); // Start unchecked!

            // Now we can enable the checkbox group
            group.SetVisible(true);
        }
    }

    /**
    Highlights the current step as a different colour, making it clear what the user needs to perform next.
    @param stepIndex The step that needs to be highlighted.
    @return void
    */
    public void HighlightStep(int stepIndex)
    {
        for (int i = 0; i < checkboxGroups.Count; i++)
        {
            var group = checkboxGroups[i];

            if (i == stepIndex)
            {
                group.labelText.color = Color.yellow;
            }
        }
    }

    /**
    Update the UI to reflect each completed step.
    @param stepIndex The step that needs to be marked as complete.
    @return void
    */
    public void CompleteStep(int stepIndex)
    {
        // Should never happen, but checking for invalid checkbox index
        if (stepIndex < 0 || stepIndex >= checkboxGroups.Count)
        {
            return;
        }

        var group = checkboxGroups[stepIndex];
        group.SetChecked(true);

        group.labelText.color = Color.green; // Change step colour to greed when completed
        group.labelText.ForceMeshUpdate();
    }
}