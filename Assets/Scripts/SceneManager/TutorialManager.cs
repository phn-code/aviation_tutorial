using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip introClip;
    public AudioClip rollLeftClip;
    public AudioClip rollRightClip;
    public AudioClip stepCompleteClip;

    [Header("Plane")]
    public AxisRotationController axisController;

    [Header("UI")]
    public GameObject tutorialTextBox;
    public TextMeshProUGUI tutorialText;
    public ControllerHintAnimation controllerHint;


    [Header("Detection Settings")]
    public float rollThreshold = 30f;

    [Header("Throttle")]
    public AudioClip throttleClip;
    public ThrottleController throttleController;

    public float joystickThreshold = 0.5f;

    private void Start()
    {
        tutorialTextBox.SetActive(false);
        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        // --- Intro ---
        yield return new WaitForSeconds(1f);
        audioSource.PlayOneShot(introClip);
        yield return new WaitForSeconds(introClip.length + 1f);

        // --- Roll Left ---
        yield return StartCoroutine(TeachRollLeft());

        // --- Roll Right ---
        yield return StartCoroutine(TeachRollRight());

        // --- Throttle + Fly Away ---
        yield return StartCoroutine(TeachThrottle());

        Debug.Log("Tutorial Complete!");
    }

    // ─────────────────────────────────────────
    // ROLL LEFT
    // ─────────────────────────────────────────
    private IEnumerator TeachRollLeft()
    {
        tutorialTextBox.SetActive(true);
        tutorialText.text = "Tilt your left controller to the left to roll the aircraft";
        yield return new WaitForSeconds(1.5f);

        audioSource.PlayOneShot(rollLeftClip);
        yield return new WaitForSeconds(rollLeftClip.length + 0.5f);

        // Show controller hint animation
        controllerHint.ShowRollLeft();
        tutorialText.text = "Go ahead — tilt your left controller to the left";
        yield return new WaitUntil(() => IsControllerRolledLeft());

        // Hide hint when player does it
        controllerHint.Hide();
        tutorialText.text = "Great job! Watch the aircraft roll left";
        axisController.LerpBank(30f, 1.5f);
        yield return new WaitForSeconds(2f);

        audioSource.PlayOneShot(stepCompleteClip);
        tutorialText.text = "Step Complete!";
        yield return new WaitForSeconds(1.5f);

        axisController.LerpBank(0f, 1.5f);
        yield return new WaitForSeconds(2f);
        tutorialTextBox.SetActive(false);
        yield return new WaitForSeconds(1f);
    }

    // ─────────────────────────────────────────
    // ROLL RIGHT
    // ─────────────────────────────────────────
    private IEnumerator TeachRollRight()
    {
        tutorialTextBox.SetActive(true);
        tutorialText.text = "Tilt your left controller to the right to roll the aircraft";
        yield return new WaitForSeconds(1.5f);

        audioSource.PlayOneShot(rollRightClip);
        yield return new WaitForSeconds(rollRightClip.length + 0.5f);

        // Show controller hint animation
        controllerHint.ShowRollRight();
        tutorialText.text = "Go ahead — tilt your left controller to the right";
        yield return new WaitUntil(() => IsControllerRolledRight());

        // Hide hint when player does it
        controllerHint.Hide();
        tutorialText.text = "Great job! Watch the aircraft roll right";
        axisController.LerpBank(-30f, 1.5f);
        yield return new WaitForSeconds(2f);

        audioSource.PlayOneShot(stepCompleteClip);
        tutorialText.text = "Step Complete!";
        yield return new WaitForSeconds(1.5f);

        axisController.LerpBank(0f, 1.5f);
        yield return new WaitForSeconds(2f);
        tutorialTextBox.SetActive(false);
        yield return new WaitForSeconds(1f);
    }

    // ─────────────────────────────────────────
    // DETECTION
    // ─────────────────────────────────────────
    private bool IsControllerRolledLeft()
    {
        // Keyboard fallback for testing
        if (Keyboard.current != null && Keyboard.current.leftArrowKey.isPressed)
        {
            Debug.Log("Keyboard trigger - LEFT ARROW pressed");
            return true;
        }

        UnityEngine.XR.InputDevice leftController =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);

        if (leftController.isValid)
        {
            if (leftController.TryGetFeatureValue(
                UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotation))
            {
                Vector3 euler = rotation.eulerAngles;
                float signedZ = euler.z > 180f ? euler.z - 360f : euler.z;
                Debug.Log($"Controller Z: {signedZ:F1}");
                return signedZ < -rollThreshold;
            }
        }

        return false;
    }

    private bool IsControllerRolledRight()
    {
        // Keyboard fallback for testing
        if (Keyboard.current != null && Keyboard.current.rightArrowKey.isPressed)
        {
            Debug.Log("Keyboard trigger - RIGHT ARROW pressed");
            return true;
        }

        UnityEngine.XR.InputDevice leftController =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);

        if (leftController.isValid)
        {
            if (leftController.TryGetFeatureValue(
                UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotation))
            {
                Vector3 euler = rotation.eulerAngles;
                float signedZ = euler.z > 180f ? euler.z - 360f : euler.z;
                Debug.Log($"Controller Z: {signedZ:F1}");
                return signedZ > rollThreshold;
            }
        }

        return false;
    }

        // ─────────────────────────────────────────
    // THROTTLE
    // ─────────────────────────────────────────
    // ─────────────────────────────────────────
// THROTTLE
// ─────────────────────────────────────────
    private IEnumerator TeachThrottle()
    {
        tutorialTextBox.SetActive(true);
        tutorialText.text = "Now let's control the throttle";
        yield return new WaitForSeconds(1.5f);

        audioSource.PlayOneShot(throttleClip);
        yield return new WaitForSeconds(throttleClip.length + 0.5f);

        controllerHint.ShowThrottleForward();
        tutorialText.text = "Push the left joystick forward to increase the throttle";

        yield return new WaitUntil(() => IsJoystickPushedForward());

        // The moment they push — immediate reaction
        controllerHint.Hide();
        tutorialText.text = "Hold on tight!";
        yield return new WaitForSeconds(0.8f);

        tutorialTextBox.SetActive(false);

        // Hand straight off to the fly away
        axisController.enabled = false;
        yield return StartCoroutine(throttleController.FlyAway());
    }

    private bool IsJoystickPushedForward()
    {
        // Keyboard fallback for testing
        if (Keyboard.current != null && Keyboard.current.upArrowKey.isPressed)
        {
            Debug.Log("Keyboard trigger - UP ARROW pressed");
            return true;
        }

        UnityEngine.XR.InputDevice leftController =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);

        if (leftController.isValid)
        {
            if (leftController.TryGetFeatureValue(
                UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 axis))
            {
                Debug.Log($"Left Joystick Y: {axis.y:F2}");
                return axis.y > joystickThreshold;
            }
        }

        return false;
    }
}