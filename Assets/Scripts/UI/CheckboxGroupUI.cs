using UnityEngine;
using TMPro;
using UnityEngine.UI;

/** 
A group of UI checkboxes used in activity guidance.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class CheckboxGroupUI : MonoBehaviour
{
    public GameObject groupObject; /**< The checkbox group. */
    public TextMeshProUGUI labelText; /**< The label for the group of checkboxes. */
    public Image checkImage; /**< The checkmark image. */

    /**
    Sets the checkbox image active or not.
    @param isChecked The state for the checkbox to be set to.
    @return void
    */
    public void SetChecked(bool isChecked)
    {
        checkImage.enabled = isChecked;
    }

    /**
    Sets the checkbox label text.
    @param newText The text for the checkbox label to be set to.
    @return void
    */
    public void SetLabel(string newText)
    {
        labelText.text = newText;
    }

    // Show/hide entire checkbox item
    public void SetVisible(bool visible)
    {
        groupObject.SetActive(visible);
    }
}