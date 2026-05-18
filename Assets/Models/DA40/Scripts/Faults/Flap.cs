using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Flap : Interact_Check
{
    public enum FlapPositions { Neutral, Up, Down }
    [Header("Flap Fault Parameters")]
    [SerializeField] protected Transform flap;

    protected FlapPositions desiredPosition;
    protected float neutralRotation;
    [SerializeField] protected float maximumRotation;
    [SerializeField] protected float minimalRotation;
    protected float currentRotation; // Used to hold the current rotation, without angle looping.



    // Injected code to be run by Start().
    protected override void StartUp()
    {
        base.StartUp();

        desiredPosition = FlapPositions.Neutral;
        neutralRotation = 0f;
        currentRotation = 0f;
    }



    // Injected code to be run once per frame by Update().
    protected override void Changes()
    {
        base.Changes();

        switch(desiredPosition)
        {
            case (FlapPositions.Neutral):
                if (currentRotation > neutralRotation)
                {
                    LowerFlap(deltaTime * 8, neutralRotation);
                }
                else
                {
                    LiftFlap(deltaTime * 8, neutralRotation);
                }
                break;

            case (FlapPositions.Up):
                LiftFlap(deltaTime * 8, maximumRotation);
                break;

            case (FlapPositions.Down):
                LowerFlap(deltaTime * 8, minimalRotation);
                break;
        }
    }



    // Called when the user initiates an interaction with the part.
    public override void Interact()
    {
        base.Interact();

        if (!isFaulty)
        {
            if (desiredPosition == FlapPositions.Up)
            {
                desiredPosition = FlapPositions.Down; // set to lowered if raised.
            }
            else if (desiredPosition == FlapPositions.Down)
            {
                desiredPosition = FlapPositions.Up; // set to raised if lowered.
            }
            else
            {
                desiredPosition = FlapPositions.Up; // set to raised if neutral.
            }
        }
    }



    // Called when the user stops interacting with the part.
    public virtual void Release()
    {
        base.Release();

        desiredPosition = FlapPositions.Neutral;
    }



    // Raises the flap upwards towards the limit provided.
    protected virtual void LiftFlap(float amount, float limit)
    {
        if (currentRotation != limit)
        {
            currentRotation += amount;
            flap.Rotate(0f, 0f, amount);

            if (currentRotation > limit)
            {
                currentRotation = limit;
                flap.localRotation = Quaternion.Euler(0f, 0f, limit);
            }
        }
    }



    // Lowers the flap upwards towards the limit provided.
    protected virtual void LowerFlap(float amount, float limit)
    {
        if (currentRotation != limit)
        {
            currentRotation -= amount;
            flap.Rotate(0f, 0f, -amount);

            if (currentRotation < limit)
            {
                currentRotation = limit;
                flap.localRotation = Quaternion.Euler(0f, 0f, limit);
            }
        }
    }
}
