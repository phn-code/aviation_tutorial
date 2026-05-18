using UnityEngine;

/** 
Ambient class that slowly and gradualy rotates the skybox in the world to give the impression of moving clouds.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class SkyboxRotation : MonoBehaviour
{
    public float rotationSpeed = 0.1f; /**< Speed at which the clouds should move. */

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed); // Rotate clouds
    }
}
