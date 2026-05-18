using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Displacement : Visual_Fault
{
    [Header("Displaced Fault Parameters")]
    [SerializeField] private GameObject affectedPart;
    [SerializeField] private Transform[] alternatePositions; // Transforms used for alternate positions.
    private Vector3[] worldPositions;
    private Quaternion[] worldRotations;



    // Injected code to be run by Start().
    protected override void StartUp()
    {

        worldPositions = new Vector3[alternatePositions.Length + 1];
        worldRotations = new Quaternion[alternatePositions.Length + 1];

        worldPositions[0] = affectedPart.transform.position;
        worldRotations[0] = affectedPart.transform.rotation;

        for (int altTransform = 0; altTransform < alternatePositions.Length; altTransform++)
        {
            worldPositions[altTransform+1] = alternatePositions[altTransform].position;
            worldRotations[altTransform+1] = alternatePositions[altTransform].rotation;
        }
    }



    // Handles the generation of the specified fault and its variants.
    public override void GenerateFault()
    {
        base.GenerateFault();

        int randomNum = Random.Range(0, worldPositions.Length);
        affectedPart.transform.position = worldPositions[randomNum];
        affectedPart.transform.rotation = worldRotations[randomNum];
        if (randomNum == 0)
        {
            affectedPart.SetActive(false);
        }
        else
        {
            affectedPart.SetActive(true);
        }
    }



    // Clears the fault to its intended functional state.
    public override void RemoveFault()
    {
        base.RemoveFault();
        affectedPart.SetActive(true);
        affectedPart.transform.position = worldPositions[0];
        affectedPart.transform.rotation = worldRotations[0];
    }
}
