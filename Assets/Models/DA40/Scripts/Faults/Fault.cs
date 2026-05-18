using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public abstract class Fault : MonoBehaviour
{
    protected float deltaTime;
    [SerializeField] protected Toggle boardDisplay;

    [Header("Generic Fault Parameters")]
    [SerializeField] protected bool isFaulty; // Flags the component as having a fault with it.
    [SerializeField] protected bool isTagged; // Whether the user has tagged the fault as being present.
    [SerializeField] protected int weighting; // How likely the part is (as a percentile) to generate with a fault.



    // Start is called before the first frame update.
    void Start()
    {
        StartUp();
        RemoveFault(); // Ensures there are no faults on bootup.
        SetWeighting(15); // Presets weighting to 0

        deltaTime = 0f;
    }



    // Injected code to be run by Start().
    protected virtual void StartUp()
    {

    }



    // Update is called once per frame.
    void Update()
    {
        deltaTime = Time.deltaTime;
        Changes();
    }



    // Injected code to be run once per frame by Update().
    protected virtual void Changes()
    {

    }



    // Changes the colour of the associated simulation board's display button.
    public void ColourButton()
    {
        if (isFaulty && isTagged)
        {
            boardDisplay.image.color = Color.green;
        }
        else if (!isFaulty && isTagged)
        {
            boardDisplay.image.color = Color.yellow;
        }
        else if (isFaulty && !isTagged)
        {
            boardDisplay.image.color = Color.red;
        }
        else
        {
            boardDisplay.image.color = Color.white;
        }
    }



    // Sets whether the associated simulation board's button can be interacted with.
    public void EnableButton(bool state)
    {
        boardDisplay.interactable = state;
    }



    // Resets all of the associated button's parameters to default.
    public void ResetButton()
    {
        boardDisplay.isOn = false;
        isTagged = false;
        ColourButton();
        EnableButton(true);
    }



    // Defined getter for retrieving the fault's state.
    public bool IsFaulty()
    {
        return isFaulty;
    }



    // Flips the tagged status between true and false.
    public void ToggleTag()
    {
        if (isTagged)
        {
            isTagged = false;
        }
        else
        {
            isTagged = true;
        }
    }



    // Defined getter for retrieving the fault’s tagged state.
    public bool IsTagged()
    {
        return isTagged;
    }



    // Defined setter for adjusting weighting within defined limits.
    public void SetWeighting(int weight)
    {
        if (weight > 100)
        {
            weight = 100;
        }
        else if (weight < 0)
        {
            weight = 0;
        }
        weighting = weight;
    }



    // Clears the fault to its intended functional state.
    public virtual void RemoveFault()
    {
        isFaulty = false;
    }



    // Handles the generation of the specified fault and its variants.
    public virtual void GenerateFault()
    {
        isFaulty = true;
    }



    // Potentially has the fault be activated, depending on the weighting value.
    public virtual void Randomise()
    {
        int randomNum = Random.Range(1, 101);

        if (randomNum <= weighting)
        {
            GenerateFault();
        }
        else
        {
            RemoveFault();
        }
    }
}
