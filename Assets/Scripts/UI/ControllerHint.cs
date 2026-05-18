using UnityEngine;
using System.Collections;

public class ControllerHint : MonoBehaviour
{
    [SerializeField] private Transform controllerModel;
    [SerializeField] private float tiltAngle = 35f;        // How far it tilts right
    [SerializeField] private float tiltSpeed = 1.5f;       // Speed of the rock

    private Quaternion startRotation;
    private Coroutine animCoroutine;

    void OnEnable()
    {
        startRotation = controllerModel.localRotation;
        animCoroutine = StartCoroutine(RockLoop());
    }

    void OnDisable()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        controllerModel.localRotation = startRotation;
    }

    IEnumerator RockLoop()
    {
        while (true)
        {
            // Rock from upright to tilted right and back
            float t = (Mathf.Sin(Time.time * tiltSpeed) + 1f) / 2f; // 0 to 1
            float angle = Mathf.Lerp(0f, tiltAngle, t);
            controllerModel.localRotation = startRotation * Quaternion.Euler(0f, 0f, -angle);
            yield return null;
        }
    }
}