using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;



public class HapticFeedback : MonoBehaviour
{
    private float intensity;
    private float duration;



    // Sends haptic command to the corrosponding controller.
    private void TriggerHaptic(XRBaseController controller)
    {
        controller.SendHapticImpulse(0.25f, 1f);
    }



    // Activate controller vibration on enter.
    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "Hand")
        {
            XRBaseController controller = collider.GetComponent<XRBaseController>();
            TriggerHaptic(controller);
        }
    }
}
