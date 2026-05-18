using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Light_Colour : Visual_Fault
{
    [Header("Colour Fault Parameters")]
    [SerializeField] protected Static_Light affectedPart;
    [SerializeField] protected Material[] alternateGlows;
    [SerializeField] protected Color[] alternateColours;



    // Clears the fault to its intended functional state.
    public override void RemoveFault()
    {
        base.RemoveFault();

        affectedPart.RevertColour();
    }



    // Handles the generation of the specified fault and its variants.
    public override void GenerateFault()
    {
        if (!affectedPart.IsFaulty())
        {
            base.GenerateFault();

            int randomNum = Random.Range(0, alternateColours.Length);

            affectedPart.ChangeColour(alternateColours[randomNum]);
            affectedPart.ApplyMaterial(alternateGlows[randomNum]);
        }
    }
}
