using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Aircraft_Part : MonoBehaviour
{
    [Header("Potential Faults")]
    [SerializeField] private Fault[] faults;



    // Returns how many potential faults the part has.
    public int NumberOfFaults()
    {
        return faults.Length;
    }



    // Returns a specified fault by index number.
    public Fault GetFault(int index)
    {
        return faults[index];
    }



    // Resets a specified fault to its functional state.
    public void ClearFault(int fault)
    {
        if (fault >= 0 || fault < faults.Length)
        {
            faults[fault].RemoveFault();
            faults[fault].ResetButton();
        }
    }



    // Resets all faults to their functional state.
    public void ClearFaults()
    {
        for (int fault = 0; fault < faults.Length; fault++)
        {
            faults[fault].RemoveFault();
            faults[fault].ResetButton();
        }
    }



    // Sets a specific fault to its malfunctioning state.
    public void GenerateFault(int fault)
    {
        if (fault >= 0 || fault < faults.Length)
        {
            faults[fault].GenerateFault();
        }
    }



    // Sets all faults to their malfunctioning state.
    public void GenerateFaults()
    {
        for (int fault = 0; fault < faults.Length; fault++)
        {
            faults[fault].GenerateFault();
        }
    }



    // Sets a random potential fault to be malfunctioning.
    public void RandomiseFault()
    {
        int randomNum = Random.Range(0, faults.Length);
        faults[randomNum].GenerateFault();
    }



    // Potentially sets any number of faults to their malfunctioning state, based upon their individual weightings.
    public void RandomiseFaults()
    {
        for (int fault = 0; fault < faults.Length; fault++)
        {
            faults[fault].Randomise();
        }
    }
}
