using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Yokes : Visual_Fault
{
    [Header("Yoke Fault Parameters")]
    [SerializeField] protected Transform[] yokes;
    [SerializeField] protected bool inverted; // Inverts yoke rotations for right wing.



    // Shifts the yokes towards a specified direction.
    public void MoveYoke(float amount)
    {
        if (!isFaulty)
        {
            for (int yoke = 0; yoke < yokes.Length; yoke++)
            {
                if (inverted)
                {
                    yokes[yoke].Rotate(amount, 0f, 0f);
                }
                else
                {
                    yokes[yoke].Rotate(-amount, 0f, 0f);
                }
            }
        }
    }



    // Sets the yokes to a specified rotational amount.
    public void SetYoke(float limit)
    {
        if (!isFaulty)
        {
            for (int yoke = 0; yoke < yokes.Length; yoke++)
            {
                if (inverted)
                {
                    yokes[yoke].localRotation = Quaternion.Euler(limit, 0f, 0f);
                }
                else
                {
                    yokes[yoke].localRotation = Quaternion.Euler(limit, 0f, 0f);
                }
            }
        }
    }
}
