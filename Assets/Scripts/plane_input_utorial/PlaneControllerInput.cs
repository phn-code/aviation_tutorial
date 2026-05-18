using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerDebugTest : MonoBehaviour
{
    public AxisRotationController axisController;
    public float rollThreshold = 30f;

    private bool hasTriggered = false;

    void Update()
    {
        if (Keyboard.current != null && 
            Keyboard.current.leftArrowKey.wasPressedThisFrame && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("TRIGGERED via keyboard! Banking plane left");
            axisController.LerpBank(30f, 1.5f);
            Invoke(nameof(ResetTrigger), 3f);
        }

        // Use full namespace to avoid conflict
        UnityEngine.XR.InputDevice leftController = 
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);

        if (leftController.isValid)
        {
            if (leftController.TryGetFeatureValue(
                UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotation))
            {
                Vector3 euler = rotation.eulerAngles;
                float signedZ = euler.z > 180f ? euler.z - 360f : euler.z;
                Debug.Log($"Controller Z: {signedZ:F1} | Need below: -{rollThreshold}");
            }
        }
    }

    private void ResetTrigger()
    {
        hasTriggered = false;
        axisController.LerpBank(0f, 1.5f);
        Debug.Log("Reset - press LEFT ARROW again to test");
    }
}