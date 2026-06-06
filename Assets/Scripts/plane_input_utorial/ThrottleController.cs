using UnityEngine;
using System.Collections;

public class ThrottleController : MonoBehaviour
{
    [Header("Movement")]
    public Transform planeTransform;       
    public float maxSpeed = 80f;           
    public float flyAwayAcceleration = 3f; 

    [Header("Optional Audio")]
    public AudioSource engineAudioSource;  
    public float minPitch = 0.4f;
    public float maxPitch = 1.8f;

    [HideInInspector] public float currentThrottle = 0f; 

    private bool isFlyingAway = false;

    private void Update()
    {
        if (engineAudioSource != null)
            engineAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, currentThrottle);

    }

    //Called by tutorial to smoothly show throttle increasing 
    public void LerpThrottle(float targetThrottle, float duration)
    {
        StartCoroutine(LerpThrottleCoroutine(targetThrottle, duration));
    }

    private IEnumerator LerpThrottleCoroutine(float target, float duration)
    {
        float start = currentThrottle;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentThrottle = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        currentThrottle = target;
    }

    // alled at the end of the tutorial plane flies off into the distance 
    public IEnumerator FlyAway()
    {
        // Phase 1: Engine slams to full throttle 
        float elapsed = 0f;
        float rampDuration = 1.2f;
        float startThrottle = currentThrottle;
        while (elapsed < rampDuration)
        {
            elapsed += Time.deltaTime;
            currentThrottle = Mathf.Lerp(0f, 1f, elapsed / rampDuration);
            yield return null;
        }
        currentThrottle = 1f;

        // Accelerate forward and pitch nose up 
        float speed = 5f;
        float travelledDistance = 0f;
        float flyDistance = 600f;
        float currentPitch = 0f;
        float targetPitch = -25f;
        float startYaw = planeTransform.localEulerAngles.y;

        Vector3 flyDirection = Vector3.right;
        flyDirection.y = 0f;
        flyDirection.Normalize();

        while (travelledDistance < flyDistance)
        {
            speed = Mathf.MoveTowards(speed, maxSpeed, flyAwayAcceleration * Time.deltaTime * 50f);

            currentPitch = Mathf.MoveTowards(currentPitch, targetPitch, 12f * Time.deltaTime);
            planeTransform.localRotation = Quaternion.Euler(currentPitch, startYaw, 0f);

            float delta = speed * Time.deltaTime;
            planeTransform.position += flyDirection * delta;
            travelledDistance += delta;

            yield return null;
        }
        planeTransform.gameObject.SetActive(false); 

    }
}