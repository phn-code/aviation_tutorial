using System.Collections;
using NUnit.Framework.Interfaces;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/**
Static helper class for activity management and control

Provides utility functions for:
- Timeline and PlayableDirector management
- VR controller input processing and normalization
- Aircraft rotation control (pitch and roll)
- Debug mouse controls for testing
- Rotation clamping and target verification

@author Connor Freebairn | frecd002@mymail.unisa.edu.au
*/
public static class ActivityHelper
{
    /**
    Finds the PlayableDirector associated with a given TimelineAsset
    
    Searches all PlayableDirectors in the scene to find the one that uses the
    specified TimelineAsset. Logs a warning if the asset is null or no matching
    director is found.
    
    @param asset The TimelineAsset to search for
    @return The PlayableDirector using the asset, or null if not found
    */
    public static PlayableDirector FindPlayableDirector(TimelineAsset asset)
    {
        // Check for null asset
        if (asset == null)
        {
            Debug.LogWarning("TimeLineAsset is NULL: Cannot find PlayableDirector");
            return null;
        }

        // Get all PlayableDirectors in the scene
        PlayableDirector[] allDirectors = Object.FindObjectsOfType<PlayableDirector>();
        // Search for the Director that uses the specified asset
        for (int i = 0; i < allDirectors.Length; i++)
        {
            // Get the next Director
            PlayableDirector director = allDirectors[i];
            if (director.playableAsset == asset)
            {
                return director;
            }
        }

        Debug.LogWarning("No PlayableDirector found for timeline asset: " + asset.name);
        return null;
    }

    /**
    Finds PlayableDirectors for multiple TimelineAssets
    
    Searches all PlayableDirectors in the scene to find those that use the specified
    TimelineAssets. Returns an array with matching directors in corresponding indices.
    Null entries indicate no director was found for that asset.
    
    @param assets Array of TimelineAssets to search for
    @return Array of PlayableDirectors matching the input assets, or null if input is invalid
    */
    public static PlayableDirector[] FindPlayableDirector(TimelineAsset[] assets)
    {
        // Check for null or empty array
        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning("TimeLineAsset array is NULL or empty: Cannot find PlayableDirectors");
            return null;
        }

        // Get all PlayableDirectors in the scene
        PlayableDirector[] allDirectors = Object.FindObjectsOfType<PlayableDirector>();
        // Create an array to hold found Directors
        PlayableDirector[] foundDirectors = new PlayableDirector[assets.Length];

        // Search for Directors for each asset
        for (int i = 0; i < assets.Length; i++)
        {
            // Get the next asset
            TimelineAsset asset = assets[i];
            foundDirectors[i] = null;

            // Check for null asset
            if (asset != null)
            {
                // Search for the Director that uses this asset
                for (int j = 0; j < allDirectors.Length; j++)
                {
                    PlayableDirector director = allDirectors[j];
                    if (director.playableAsset == asset)
                    {
                        foundDirectors[i] = director;
                        break;
                    }
                }
                // If still null
                if (foundDirectors[i] == null)
                {
                    Debug.LogWarning("No PlayableDirector found for timeline asset: " + asset.name);
                }
            }
            else
            {
                Debug.LogWarning("TimelineAsset at index " + i + " is NULL: Cannot find PlayableDirector");
            }
        }
        return foundDirectors;
    }

    /**
    Retrieves the ProceduralAirflow component from the scene
    
    Searches for and returns the first ProceduralAirflow component found in the scene.
    Logs an error if the component cannot be found.
    
    @return The ProceduralAirflow component, or null if not found or error occurs
    */
    public static ProceduralAirflow getProceduralAirflow()
    {
        try
        {
            return GameObject.FindObjectOfType<ProceduralAirflow>();
        }
        catch
        {
            Debug.LogError("ProceduralAirflow not found in scene!");
            return null;
        }
    }

    /**
    Retrieves the AxisRotationController component from the scene
    
    Searches for and returns the first AxisRotationController component found in the scene.
    Logs an error if the component cannot be found.
    
    @return The AxisRotationController component, or null if not found or error occurs
    */
    public static AxisRotationController getAxisRotationController()
    {
        try
        {
            return GameObject.FindObjectOfType<AxisRotationController>();
        }
        catch
        {
            Debug.LogError("AOAManipulator not found in scene!");
            return null;
        }
    }

    /**
    Retrieves and configures the AxisRotationController with initial rotation values
    
    Searches for the AxisRotationController and sets its angle of attack and bank angle
    to the specified values using lerp functions.
    
    @param aoa Angle of attack to set (default: 0)
    @param bank Bank angle to set (default: 0)
    @return The configured AxisRotationController, or null if not found or error occurs
    */
    public static AxisRotationController getAxisRotationController(float aoa = 0, float bank = 0)
    {
        try
        {
            AxisRotationController manip = GameObject.FindObjectOfType<AxisRotationController>();
            manip.LerpAOA(aoa);
            manip.LerpBank(bank);
            return manip;
        }
        catch
        {
            Debug.LogError("AOAManipulator not found in scene!");
            return null;
        }
    }

    /**
    Retrieves and configures the BankGhostTrailBehaviour component
    
    Searches for the BankGhostTrailBehaviour component and sets its enable state.
    
    @param enable Whether to enable the ghost trail (default: true)
    @return The BankGhostTrailBehaviour component, or null if not found or error occurs
    */
    public static BankGhostTrailBehaviour getBankGhostTrail(bool enable = true)
    {
        try
        {
            BankGhostTrailBehaviour trail = GameObject.FindObjectOfType<BankGhostTrailBehaviour>();
            trail.enable = enable;
            return trail;
        }
        catch
        {
            Debug.LogError("BankGhostTrailBehaviour not found in scene!");
            return null;
        }
    }

    /**
    Returns normalized Z-axis rotation from VR controller input
    
    Reads the controller rotation quaternion and calculates the Z-axis rotation relative
    to an initial rotation value. The result is normalized between -180 and 180 degrees
    using Mathf.DeltaAngle.
    
    @param input InputActionReference for reading controller rotation
    @param initial Initial Z rotation offset in degrees (default: 0)
    @return Normalized Z rotation in degrees, range [-180, 180]
    */
    public static float normalizedControllerZRotation(InputActionReference input, float initial = 0)
    {
        Quaternion controllerRotation = input.action.ReadValue<Quaternion>();

        // Create initial rotation quaternion
        Quaternion initialRotation = Quaternion.Euler(0, 0, initial);

        // Calculate relative rotation
        Quaternion relativeRotation = Quaternion.Inverse(initialRotation) * controllerRotation;

        // Extract Z rotation from relative quaternion
        Vector3 relativeEuler = relativeRotation.eulerAngles;
        float zRotation = Mathf.DeltaAngle(0, relativeEuler.z);

        //Debug.Log($"controller rotation: {controllerRotation.eulerAngles}  initial z rotation: {initial}  normalised z rotation {zRotation}");
        return zRotation;
    }

    /**
    Returns normalized X-axis rotation from VR controller input
    
    Reads the controller rotation quaternion and calculates the X-axis rotation relative
    to an initial rotation value. The result is normalized between -180 and 180 degrees
    using Mathf.DeltaAngle.
    
    @param input InputActionReference for reading controller rotation
    @param initial Initial X rotation offset in degrees (default: 0)
    @return Normalized X rotation in degrees, range [-180, 180]
    */
    public static float normalizedControllerXRotation(InputActionReference input, float initial = 0)
    {
        Quaternion controllerRotation = input.action.ReadValue<Quaternion>();

        // Create initial rotation quaternion
        Quaternion initialRotation = Quaternion.Euler(initial, 0, 0);

        // Calculate relative rotation
        Quaternion relativeRotation = Quaternion.Inverse(initialRotation) * controllerRotation;

        // Extract X rotation from relative quaternion
        Vector3 relativeEuler = relativeRotation.eulerAngles;
        float xRotation = Mathf.DeltaAngle(0, relativeEuler.x);

        //Debug.Log($"controller rotation: {controllerRotation.eulerAngles}  initial x rotation: {initial}  normalised x rotation {xRotation}");
        return xRotation;
    }

    /**
    Debug function for mouse-based roll control
    
    Allows testing roll control using left and right mouse buttons. Left button rolls
    left (negative), right button rolls right (positive).
    
    @param speed Roll speed multiplier
    @param manip AxisRotationController to manipulate
    */
    public static void DebugMouseControlRoll(float speed, AxisRotationController manip)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            manip.IncrementBank(-speed * Time.deltaTime);  // Negative left
        }
        if (Mouse.current.rightButton.isPressed)
        {
            manip.IncrementBank(speed * Time.deltaTime);  // Positive right
        }
    }

    /**
    Debug function for mouse-based pitch control
    
    Allows testing pitch control using left and right mouse buttons. Left button pitches
    down (negative), right button pitches up (positive).
    
    @param speed Pitch speed multiplier
    @param manip AxisRotationController to manipulate
    */
    public static void DebugMouseControlPitch(float speed, AxisRotationController manip)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            manip.IncrementAOA(-speed * Time.deltaTime);  // Negative left
        }
        if (Mouse.current.rightButton.isPressed)
        {
            manip.IncrementAOA(speed * Time.deltaTime);  // Positive right
        }
    }

    /**
    Debug function for mouse-based left roll control
    
    Allows testing roll left using only the right mouse button.
    
    @param speed Roll speed multiplier
    @param manip AxisRotationController to manipulate
    */
    public static void DebugMouseControlRollLeft(float speed, AxisRotationController manip)
    {
        if (Mouse.current.rightButton.isPressed)
        {
            //Debug.Log("Using Mouse Clicks for Roll" + speed + " " + Time.deltaTime);
            manip.IncrementBank(speed * Time.deltaTime);
        }
    }

    /**
    Debug function for mouse-based pitch up control
    
    Allows testing pitch up using only the left mouse button.
    
    @param speed Pitch speed multiplier
    @param manip AxisRotationController to manipulate
    */
    public static void DebugMouseControlPitchUp(float speed, AxisRotationController manip)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            manip.IncrementAOA(speed * Time.deltaTime);
        }
    }

    /**
    debug moust control for Stalls in Descending Turns - Checklist -> mahir
    */
    public static void DebugMouseControlPitchDown(float speed, AxisRotationController manip)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            manip.IncrementAOA(-speed * Time.deltaTime);
        }
    }

    /**
    Processes VR controller input for aircraft roll control
    
    Reads controller Z-axis rotation and applies it to the aircraft's bank angle with
    deadzone filtering and speed scaling. Clamps the resulting roll to the specified range.
    
    @param speed Maximum roll speed multiplier
    @param manip AxisRotationController to manipulate
    @param input InputActionReference for reading controller rotation
    @param deadZone Deadzone threshold below which input is ignored
    @param initial Initial controller Z rotation offset (default: 0)
    @param vector Clamp range for roll as Vector2(min, max) (default: -70 to 70 degrees)
    */
    public static void controllerRollControl(float speed, AxisRotationController manip, InputActionReference input, float deadZone, float initial = 0, Vector2 vector = default)
    {
        if (vector == default)
        {
            vector.x = -70f;
            vector.y = 70f;
        }

        float planeX = Mathf.DeltaAngle(0, manip.getRotation().eulerAngles.x);
        planeX = clampRange(planeX, vector);
        manip.setRoll(planeX);

        float zRotation = normalizedControllerZRotation(input, initial);
        if (Mathf.Abs(zRotation) > deadZone)
        {
            zRotation = getRotationSpeed(zRotation, speed, deadZone);
            manip.IncrementBank(zRotation * Time.deltaTime);
        }
    }

    /**
    Processes VR controller input for aircraft pitch control
    
    Reads controller X-axis rotation and applies it to the aircraft's angle of attack with
    deadzone filtering and speed scaling. Inverts the input so pulling back increases AOA.
    Clamps the resulting pitch to the specified range.
    
    @param speed Maximum pitch speed multiplier
    @param manip AxisRotationController to manipulate
    @param input InputActionReference for reading controller rotation
    @param deadZone Deadzone threshold below which input is ignored
    @param initial Initial controller X rotation offset (default: 0)
    @param vector Clamp range for pitch as Vector2(min, max) (default: -40 to 40 degrees)
    */
    public static void controllerPitchControl(float speed, AxisRotationController manip, InputActionReference input, float deadZone, float initial = 0, Vector2 vector = default)
    {
        if (vector == default)
        {
            vector.x = -40f;
            vector.y = 40f;
        }

        float planeZ = Mathf.DeltaAngle(0, manip.getRotation().eulerAngles.z);
        planeZ = clampRange(planeZ, vector);
        manip.setPitch(planeZ);

        float xRotation = normalizedControllerXRotation(input, initial);
        if (Mathf.Abs(xRotation) > deadZone)
        {
            // reverse pitch control as pulling back increases AOA
            xRotation = getRotationSpeed(xRotation, speed, deadZone);
            manip.IncrementAOA(-1 * xRotation * Time.deltaTime);
        }
    }

    /**
    Clamps a rotation value within a specified range
    
    @param rotation Rotation value to clamp
    @param range Vector2 containing min (x) and max (y) clamp values
    @return Clamped rotation value
    */
    public static float clampRange(float rotation, Vector2 range)
    {
        float result = Mathf.Clamp(rotation, range.x, range.y);
        return result;
    }

    /**
    Calculates rotation speed with deadzone compensation
    
    Scales the rotation speed proportionally to how far the input exceeds the deadzone.
    Preserves the sign of the original rotation.
    
    @param rotation Input rotation value
    @param speed Speed multiplier
    @param deadZone Deadzone threshold
    @return Scaled rotation speed with sign preserved
    */
    public static float getRotationSpeed(float rotation, float speed, float deadZone)
    {
        // Make roll speed proportional to how far past the deadzone the input is
        float rotSpeed = (Mathf.Abs(rotation) - deadZone) * speed;
        rotSpeed = Mathf.Sign(rotation) * rotSpeed;
        return rotSpeed;
    }

    /**
    Checks if current rotation is within tolerance of target (absolute value comparison)
    
    Compares the absolute values of current and target rotation. Useful when direction
    doesn't matter, only magnitude (e.g., checking if at 30 degrees regardless of sign).
    
    @param currentRotation Current rotation value
    @param targetRotation Target rotation value
    @param tolerance Acceptable difference threshold
    @return True if within tolerance, false otherwise
    */
    public static bool checkRotationTargetAchieved(float currentRotation, float targetRotation, float tolerance)
    {
        if (Mathf.Abs(Mathf.Abs(currentRotation) - targetRotation) < tolerance)
        {
            return true;
        }
        return false;
    }

    /**
    Checks if current rotation exactly matches target within tolerance
    
    Compares current and target rotation directly, respecting sign. Useful for checking
    if at a specific signed rotation (e.g., exactly at +30 or -30 degrees).
    
    @param currentRotation Current rotation value
    @param targetRotation Target rotation value
    @param tolerance Acceptable difference threshold
    @return True if within tolerance, false otherwise
    */
    public static bool checkExactRotationTargetAchieved(float currentRotation, float targetRotation, float tolerance)
    {
        //Debug.Log("Exact Rotation: " + (currentRotation - targetRotation));
        if (Mathf.Abs(currentRotation - targetRotation) < tolerance)
        {
            return true;
        }
        return false;
    }

    /**
    Gets the aircraft's normalized roll angle
    
    Extracts and normalizes the X-axis rotation (roll) from the AxisRotationController
    to a range of -180 to 180 degrees.
    
    @param manip AxisRotationController to query
    @return Normalized roll angle in degrees, range [-180, 180]
    */
    public static float getNormalisedPlaneRoll(AxisRotationController manip)
    {
        float xRot = manip.getRotation().eulerAngles.x;
        return Mathf.DeltaAngle(0f, xRot);
    }

    /**
    Gets the aircraft's normalized pitch angle
    
    Extracts and normalizes the Z-axis rotation (pitch) from the AxisRotationController
    to a range of -180 to 180 degrees.
    
    @param manip AxisRotationController to query
    @return Normalized pitch angle in degrees, range [-180, 180]
    */
    public static float getNormalisedPlanePitch(AxisRotationController manip)
    {
        float zRot = manip.getRotation().eulerAngles.z;
        return Mathf.DeltaAngle(0f, zRot);
    }

    /**
    Coroutine to play a Unity Timeline and invoke callback on completion
    
    Plays the specified PlayableDirector and waits until the timeline completes before
    invoking the provided callback action. If the director or asset is null, waits for
    the fallback duration before invoking the callback.
    
    @param director PlayableDirector containing the timeline to play
    @param onComplete Callback action to invoke when timeline completes
    @param fallBackWait Fallback wait time in seconds if director is invalid (default: 1s)
    @return IEnumerator for coroutine execution
    */
    public static IEnumerator PlayTimeLine(PlayableDirector director, System.Action onComplete, float fallBackWait = 1f)
    {
        if (director != null && director.playableAsset != null)
        {
            director.Play();

            bool isComplete = false;
            // Subscribe to the stopped event
            // Lambda called when timeline ends setting isComplete to true
            director.stopped += (d) => { isComplete = true; };

            // Wait until timeline is complete
            while (!isComplete)
            {
                yield return null;
            }
            // Call lambda fuction passed in to signal activity complete
            onComplete.Invoke();
        }
        else
        {
            Debug.Log("Yeilding as fallback for unassigned PlayableDirector (Timeline)");
            yield return new WaitForSeconds(fallBackWait);
            onComplete.Invoke();
        }
    }
}
