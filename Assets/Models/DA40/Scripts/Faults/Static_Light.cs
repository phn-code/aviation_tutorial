using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Static_Light : Visual_Fault
{
    [Header("Light Fault Parameters")]
    [SerializeField] protected Light light;
    [SerializeField] protected MeshRenderer affectedPart;
    [SerializeField] protected Material offMaterial;
    [SerializeField] protected Material onMaterial;
    [SerializeField] protected Color baseColour;
    [SerializeField] protected float maxIntensity;



    // Enables the light source and changes the mesh's material to be 'on'.
    public void TurnOn()
    {
        light.intensity = maxIntensity;
        ApplyMaterial(onMaterial);
    }



    // Disables the light source and changes the mesh's material to be 'off'.
    public void TurnOff()
    {
        light.intensity = 0f;
        ApplyMaterial(offMaterial);
    }



    // Disables the light source by switching it off if the part is malfunctioning.
    public override void GenerateFault()
    {
        base.GenerateFault();

        TurnOff();
    }



    // Enables the light source by removing the malfunction.
    public override void RemoveFault()
    {
        base.RemoveFault();

        RevertColour();
        TurnOn();
    }



    // Changes the light’s material to a specified one.
    public void ApplyMaterial(Material material)
    {
        Material[] materials = affectedPart.materials;
        materials[0] = material;
        affectedPart.materials = materials;
    }



    // Changes the light’s colour to a specified one.
    public void ChangeColour(Color colour)
    {
        light.color = colour;
    }



    // Returns the light to its default colour and material.
    public void RevertColour()
    {
        ChangeColour(baseColour);
        ApplyMaterial(onMaterial);
    }
}
