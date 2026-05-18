using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Aileron : Flap
{
    [SerializeField] protected Yokes yokes;



    // Raises the flap and yokes upwards towards the limit provided.
    protected override void LiftFlap(float amount, float limit)
    {
        if (currentRotation != limit)
        {
            currentRotation += amount;
            flap.Rotate(0f, 0f, amount);
            yokes.MoveYoke(amount);

            if (currentRotation > limit)
            {
                currentRotation = limit;
                flap.localRotation = Quaternion.Euler(0f, 0f, limit);
                yokes.SetYoke(limit);
            }
        }
    }


    // Lowers the flap and yokes upwards towards the limit provided.
    protected override void LowerFlap(float amount, float limit)
    {
        if (currentRotation != limit)
        {
            currentRotation -= amount;
            flap.Rotate(0f, 0f, -amount);
            yokes.MoveYoke(-amount);

            if (currentRotation < limit)
            {
                currentRotation = limit;
                flap.localRotation = Quaternion.Euler(0f, 0f, limit);
                yokes.SetYoke(limit);
            }
        }
    }
}
