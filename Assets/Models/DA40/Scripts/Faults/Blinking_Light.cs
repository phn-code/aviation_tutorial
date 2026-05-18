using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Blinking_Light : Visual_Fault
{
    [Header("Strobe Fault Parameters")]
    [SerializeField] protected Static_Light affectedPart;
    protected bool turnedOn; // Whether the light's intensity is changing.
    protected float blinkDelayOn; // Time that the light remains on.
    protected float blinkDelayOff; // Time that the light remains off.
    protected float blinkDuration; // Timer until next state change in the light.



    // Injected code to be run by Start().
    protected override void StartUp()
    {
        base.StartUp();

        turnedOn = true;
        blinkDelayOn = 1.25f;
        blinkDelayOff = 0.25f;
        blinkDuration = blinkDelayOn;
    }



    // Injected code to be run once per frame by Update().
    protected override void Changes()
    {
        if (!isFaulty)
        {
            blinkDuration -= deltaTime;

            if (blinkDuration <= 0f)
            {
                if (turnedOn)
                {
                    turnedOn = false;
                    affectedPart.TurnOn();
                    blinkDuration = blinkDelayOff;
                }
                else
                {
                    turnedOn = true;
                    affectedPart.TurnOff();
                    blinkDuration = blinkDelayOn;
                }
            }
        }
    }



    // Clears the fault to its intended functional state.
    public override void RemoveFault()
    {
        base.RemoveFault();
    }



    // Handles the generation of the specified fault and its variants.
    public override void GenerateFault()
    {
        if (!affectedPart.IsFaulty())
        {
            base.GenerateFault();
        }
    }
}
