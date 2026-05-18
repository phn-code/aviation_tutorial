using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Rudder : Flap
{
    // Shifts the flap to the right, towards the limit provided.
    protected override void LiftFlap(float amount, float limit)
    {
        if (currentRotation != limit)
        {
            currentRotation += amount;
            flap.Rotate(-amount/3, amount, 0);

            if (currentRotation > limit)
            {
                currentRotation = limit;
                flap.localRotation = Quaternion.Euler(-limit/3, limit, 0f);
            }
        }
    }



    // Shifts the flap to the left, towards the limit provided.
    protected override void LowerFlap(float amount, float limit)
    {
        if (currentRotation != limit)
        {
            currentRotation -= amount;
            flap.Rotate(amount/3, -amount, 0);

            if (currentRotation < limit)
            {
                currentRotation = limit;
                flap.localRotation = Quaternion.Euler(-limit/3, limit, 0f);
            }
        }
    }
}
