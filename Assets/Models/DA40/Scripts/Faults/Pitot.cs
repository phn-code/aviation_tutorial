using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Pitot : Visual_Fault
{
    [Header("Pitot Fault Parameters")]
    [SerializeField] private GameObject hapticField;



    // Assigns a scuffed varient of the material to the mesh renderer.
    public override void GenerateFault()
    {
        base.GenerateFault();

        hapticField.SetActive(false);
    }



    // Assigns the unscuffed material to the mesh renderer.
    public override void RemoveFault()
    {
        base.RemoveFault();

        hapticField.SetActive(true);
    }
}
