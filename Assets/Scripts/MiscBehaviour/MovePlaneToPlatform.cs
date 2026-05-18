using UnityEngine;

/** 
Helper to handle translating the DA-40 aircraft to and from the viewing platform in a Timeline.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class PlaneTeleporter : MonoBehaviour
{
    public Vector3 targetPosition; /**< The target position the aircraft should move to. */

    /**
    Performs teleportation on the GameObject this script is attached to, towards targetPosition.
    @return void
    */
    public void TeleportToTarget()
    {
        transform.position = targetPosition;

        // HARD-CODED: Subject to change in future. Keep for now.
        Vector3 localEuler = transform.rotation.eulerAngles;
        localEuler.y = 180f;
        transform.rotation = Quaternion.Euler(localEuler);
    }
}