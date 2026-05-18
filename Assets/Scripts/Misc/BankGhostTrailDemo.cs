using UnityEngine;
/** 
A test comment. A more bigger better comment

@author Author Name (for classes/interfaces).
*/
public class BankGhostTrailDemo : MonoBehaviour
{
    /** cool little test comment */
    public AxisRotationController manip;

    float delay = 0f; /**< the delay. the full stop breaks to comment in half */
    float delayTarget = 5f; /**< The delay target */
    bool started = false;
    void Start()
    {

    }

    /**
    The update loop

    Updates every frame because <3.
    and this is an extra line
    @param nothing this is literally nothing
    @param this doesnt exist, literall the param this, does. not. exist.
    @return no why would you think this returns something bozo
    */
    void Update()
    {
        if (delay >= delayTarget)
        {
            if (started)
            {
                return;
            }
            else
            {
                started = true;
                manip.LerpBank(69, 6);
            }
        }
        else
        {
            delay += Time.deltaTime;
            Debug.Log("Delay Target: " + delayTarget);
        }
    }
}
