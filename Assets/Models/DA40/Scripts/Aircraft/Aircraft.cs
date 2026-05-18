using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Aircraft : MonoBehaviour
{
    [SerializeField] protected Aircraft_Part[] parts;



    // Returns a specified part of the aircraft by index number.
    // Refer to concrete classes for indexing of parts.
    public Aircraft_Part GetPart(int index)
    {
        return parts[index];
    }



    // Returns how many potential faults the aircraft has in total across its parts.
    public int NumberOfParts()
    {
        return parts.Length;
    } 



    // Returns how many potential faults the aircraft has in total across its parts.
    public int NumberOfFaults()
    {
        int sum = 0;
        for (int part = 0; part < parts.Length; part++)
        {
            sum = sum + parts[part].NumberOfFaults();
        }
        return sum;
    }



    // Loads the fault state in all parts based on predefined states.
    public void RunPractiseScenario()
    {

    }



    // Potentially sets a fault to malfunctioning in every aircraft part, based on their weighting.
    public void RandomiseFaults()
    {
        for (int part = 0; part < parts.Length; part++)
        {
            parts[part].RandomiseFaults();
        }
    }



    // Randomises a potential fault in a random part of the aircraft.
    public void RandomisePart()
    {
        int randomNum = Random.Range(0, parts.Length);
        parts[randomNum].RandomiseFault();
    }



    //
    public void Reset()
    {
        for (int part = 0; part < parts.Length; part++)
        {
            parts[part].ClearFaults();
        }
    }
}
