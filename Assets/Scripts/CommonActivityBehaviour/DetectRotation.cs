using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DetectRotation : MonoBehaviour
{


    public GameObject rightController; ///or primary controller.
    public GameObject DA_40;

    public InputActionReference locomotionToggle;

    // Toggle for d4_40 rotation on and off during activities or module.
    public Boolean rotationToggle = false;



    // Test

    // InputActionReference
    //public void OnEnable()
    //{

    //}

    //


    // Update is called once per frame
    void Update()
    {

        var rightControllerPosition = rightController.transform.position;
        var rightControllerRotation = rightController.transform.rotation;
        var DA_40_r = DA_40.transform.rotation; // Set to quaternion type..
        //Debug.Log("rPos: " + rightControllerPosition + " rRot: " + rightControllerRotation);

        if (rightControllerRotation != null) {

            // Moving left
            if (rightControllerRotation.z > 0.090) {
                //Debug.Log("moving left");

                if (rotationToggle == true)
                {
                    DA_40.transform.rotation = rightControllerRotation; // on axis...?
                }
            }

            // Median.
            if (rightControllerRotation.z <= 0.80 || rightControllerRotation.z >= 0.059) {
                //Debug.Log("In the median range.");

                if (rotationToggle == true)
                {
                    DA_40.transform.rotation = rightControllerRotation;
                }
            }

            // Moving right.
            if (rightControllerRotation.z < 0.060) {
                //Debug.Log("moving right");

                if (rotationToggle == true)
                {
                    DA_40.transform.rotation = rightControllerRotation;
                }
            }

        }

    }

}
