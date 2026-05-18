using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class Interact_Check : Fault
{
    protected float delayAmount; // Time in seconds it takes to be able to interact with the part again.
    protected float delayRemaining; // Remaining time of the reinteraction buffer.



    // Injected code to be run once per frame by Update().
    protected override void Changes()
    {
        if (delayRemaining > 0f)
        {
            delayRemaining -= deltaTime;
            if (delayRemaining < 0f)
            {
                delayRemaining = 0f;
            }
        }
    }



    // Called when the user initiates an interaction with the part.
    public virtual void Interact()
    {
        delayRemaining = delayAmount;
    }



    // Called when the user stops interacting with the part.
    public virtual void Release()
    {
        delayRemaining = 0f;
    }
}
