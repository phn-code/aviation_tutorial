using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Damage : Visual_Fault
{
    [Header("Damage Fault Parameters")]
    [SerializeField] private MeshRenderer meshRenderer; // Mesh renderer to swap the material of.
    [SerializeField] private int materialSlot; // Material slot from renderer to replace.
    [SerializeField] private Material fineVisual; // Inspection passing texture.
    [SerializeField] private Material[] scuffedVisuals; // Inspection failing textures.
    [SerializeField] private GameObject[] hapticFields; // Haptic areas corresponding to scuffed visuals.



    // Assigns a scuffed varient of the material to the mesh renderer.
    public override void GenerateFault()
    {
        base.GenerateFault();

        int randomNum = Random.Range(0, scuffedVisuals.Length);

        Material[] materials = meshRenderer.materials;
        materials[materialSlot] = scuffedVisuals[randomNum];
        meshRenderer.materials = materials;

        for (int field = 0; field < hapticFields.Length; field++)
        {
            if (field == randomNum)
            {
                hapticFields[field].SetActive(true);
            }
            else
            {
                hapticFields[field].SetActive(false);
            }
        }
    }



    // Assigns the unscuffed material to the mesh renderer.
    public override void RemoveFault()
    {
        base.RemoveFault();

        Material[] materials = meshRenderer.materials;
        materials[materialSlot] = fineVisual;
        meshRenderer.materials = materials;

        for (int field = 0; field < hapticFields.Length; field++)
        {
            hapticFields[field].SetActive(false);
        }
    }
}
