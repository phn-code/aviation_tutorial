using UnityEngine;

public class ControllerHintAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float tiltAngle = 45f;       // Increased for steeper tilt
    public float tiltSpeed = 1.2f;      // Speed of animation
    public float pauseAtNeutral = 0.5f; // Pause at neutral before repeating

    public enum HintDirection { Left, Right, Up, Down }
    public HintDirection currentDirection = HintDirection.Left;

    private bool isAnimating = false;
    private float timer = 0f;

    void Start()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isAnimating) return;

        timer += Time.deltaTime * tiltSpeed;

        // Mathf.Abs(Mathf.Sin) creates a 0 → 1 → 0 wave (never goes negative)
        // This means it always starts and ends at neutral (0)
        float t = Mathf.Abs(Mathf.Sin(timer));

        float angle = 0f;

        switch (currentDirection)
        {
            case HintDirection.Left:
                // Goes from 0 → negative (left) → 0
                angle = -tiltAngle * t;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                break;

            case HintDirection.Right:
                // Goes from 0 → positive (right) → 0
                angle = tiltAngle * t;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                break;

            case HintDirection.Up:
                angle = -tiltAngle * t;
                transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                break;

            case HintDirection.Down:
                angle = tiltAngle * t;
                transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                break;
        }
    }

    public void ShowRollLeft()
    {
        currentDirection = HintDirection.Left;
        timer = 0f;
        isAnimating = true;
        gameObject.SetActive(true);
    }

    public void ShowRollRight()
    {
        currentDirection = HintDirection.Right;
        timer = 0f;
        isAnimating = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        isAnimating = false;
        // Reset to neutral when hidden
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(false);
    }

    public void ShowThrottleForward()
    {
        // placeholder — add your animation here later
    }
}