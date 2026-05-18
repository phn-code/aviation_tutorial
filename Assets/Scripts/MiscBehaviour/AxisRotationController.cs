using UnityEngine;
using System.Collections;

/** 
Helper class to perform rotation actions

Provides functions for performing different rotation actions on a game object.
Only performs roll and pitch rotations for a game object with a +x forward axis and +z left side axis.

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
*/
[ExecuteAlways]
public class AxisRotationController : MonoBehaviour
{
    private bool isLerpingPitch;        /**< Current state of pitch lerp */
    private bool isLerpingRoll;         /**< Current state of roll lerp */

    private float startRotationZ;       /**< Start for pitch lerp */
    private float targetRotationZ;      /**< Target for for pitch lerp */

    private float startRotationX;       /**<  Start for roll lerp */
    private float targetRotationX;      /**< Target for roll lerp */

    private float durationRoll;         /**< Duration of lerp in seconds */
    private float durationPitch;        /**< Duration of lerp in seconds */
    private float timeElapsedRoll;      /**< Current time elapsed during roll lerp*/
    private float timeElapsedPitch;     /**< Current time elapsed during pitch lerp*/

    /** 
    Lerp between initial and target angle

    Lerp from the current rotation of the parent GameObject to target angle around z axis.

    @param angle target angle for lerp
    @param lerpDuration duration of time to lerp to target
    */
    public void LerpAOA(float angle, float lerpDuration = 1.5f)
    {
        startRotationZ = transform.rotation.eulerAngles.z;
        targetRotationZ = angle;
        durationPitch = lerpDuration;
        timeElapsedPitch = 0f;
        isLerpingPitch = true;
        //Debug.Log($"LerpAOA started: from {startRotationZ} to {targetRotationZ}, duration {lerpDuration}");
    }

    /** 
    Lerp between initial and target angle

    Lerp from the current rotation of the parent GameObject to target angle around x axis.

    @param angle target angle for lerp
    @param lerpDuration duration of time to lerp to target
    */
    public void LerpBank(float angle, float lerpDuration = 1.5f)
    {
        startRotationX = transform.rotation.eulerAngles.x;
        targetRotationX = angle;
        durationRoll = lerpDuration;
        timeElapsedRoll = 0f;
        isLerpingRoll = true;
    }

    /** 
    Increment x rotaiton

    Adds parameter delta to parent GameObjects x rotation.
    Usually scaled according to deltatime.

    @param delta small -/+ value for rotation
    */
    public void IncrementBank(float delta)
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + delta, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    /** 
    Increment z rotaiton

    Adds parameter delta to parent GameObjects z rotation.
    Usually scaled according to deltatime.

    @param delta small -/+ value for rotation
    */
    public void IncrementAOA(float delta)
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + delta);
    }

    /**
    Increment y rotation (yaw)

    Adds parameter delta to the parent GameObjects y rotation.
    Usually scaled according to deltaTime.

    @param delta small -/+ value for rotation
    */
    public void IncrementYaw(float delta)
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + delta, transform.eulerAngles.z);  
    }

    /** 
    Main update loop

    Checks if pitch or roll needs to continue lerping and calls UpdatePitch() and UpdateRoll() respectivley
    */
    private void Update()
    {
        if (isLerpingPitch)
        {
            UpdatePitch();
        }

        if (isLerpingRoll)
        {
            UpdateRoll();
        }
    }

    /**
    Returns parent Quaternion rotation

    @return Quaternion
    */
    public Quaternion getRotation()
    {
        return transform.rotation;
    }

    /**
    Set x rotation of parent GameObject

    @param roll rotation value as a float
    */
    public void setRoll(float roll)
    {
        transform.rotation = Quaternion.Euler(roll, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    /**
    Set x rotation of parent GameObject

    @param pitch rotation value as a float
    */
    public void setPitch(float pitch)
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, pitch);
    }

    /**
    Check if Plane GameObject is lerping roll

    @return boolean for lerping state
    */
    public bool IsLerpingRoll()
    {
        return isLerpingRoll;
    }

    /**
    Check if Plane GameObject is lerping pitch

    @return boolean for lerping state
    */
    public bool IsLerpingPitch()
    {
        return isLerpingPitch;
    }

    /**
    Updates Roll according to lerp parameters

    Called by main Update() loop to perform roll lerp
    */
    private void UpdateRoll()
    {
        // increment timeElapsed
        timeElapsedRoll += Application.isPlaying ? Time.deltaTime : 1f / 60f; // fake deltaTime in editor
        // Calculate percentage completion
        float t = Mathf.Clamp01(timeElapsedRoll / durationRoll);

        // Lerp between start and target rotations
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = Mathf.LerpAngle(startRotationX, targetRotationX, t);

        transform.rotation = Quaternion.Euler(euler);

        if (t >= 1f) isLerpingRoll = false;
    }

    /**
    Updates Pitch according to lerp parameters

    Called by main Update() loop to perform pitch lerp
    */
    private void UpdatePitch()
    {
        // increment timeElapsed
        timeElapsedPitch += Application.isPlaying ? Time.deltaTime : 1f / 60f; // fake deltaTime in editor
        // Calculate percentage completion
        float t = Mathf.Clamp01(timeElapsedPitch / durationPitch);

        // Lerp between start and target rotations
        Vector3 euler = transform.rotation.eulerAngles;
        euler.z = Mathf.LerpAngle(startRotationZ, targetRotationZ, t);

        transform.rotation = Quaternion.Euler(euler);

        if (t >= 1f) isLerpingPitch = false;
    }
}