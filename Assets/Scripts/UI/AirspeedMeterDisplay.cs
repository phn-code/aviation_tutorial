using UnityEngine;
using TMPro;

/** 
A dynamic airspeed meter that is displayed on the UI.

@author Caleb Martin | marcy066@mymail.unisa.edu.au
*/

public class AirspeedMeterDisplay : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI AirspeedMeter; /**< The airspeed meter GameObject. */
    public float knots = 0; /**< The current airspeed value. */
    public float targetValue; /**< The target airspeed, used in interpolation. */
    private float moveSpeed; /**< The speed of airspeed interpolation. */
    public bool isMoving; /**< Flag that determines if airspeed interpolation is currently occurring. */

    void Start()
    {
        //setKnots(40f);
        moveTowardsKnotsTarget(40f, 10f);
    }

    void Update()
    {
        AirspeedMeter.text = knots.ToString("F0") + " knots"; // String formatted as 0 knots (no decimals)

        // Interpolation only happens if flag says it should
        if (isMoving)
        {
            knots = Mathf.MoveTowards(knots, targetValue, moveSpeed * Time.deltaTime);

            // If it's close enough to the target (accounting for rounding errors), we're done here!
            if (Mathf.Approximately(knots, targetValue))
            {
                isMoving = false;
            }
        }
    }

    /**
    Sets the airspeed to a new value immediately, without any smoothing/interpolation.
    @param newKnots The new value for the airspeed to be set to.
    @return void
    */
    void setKnots(float newKnots)
    {
        knots = newKnots;
        isMoving = false;
    }

    // Glorified mutator for targetValue that Update() then makes use of
    /**
    Special mutator for the airspeed that makes use of Update() to gradually interpolate towards a target value at a set speed.
    @param target The new value for the airspeed to be set to.
    @param speed The speed at which airspeed interpolation should occur.
    @return void
    */
    void moveTowardsKnotsTarget(float target, float speed)
    {
        targetValue = target;
        moveSpeed = Mathf.Abs(speed);
        isMoving = true;
    }
}
