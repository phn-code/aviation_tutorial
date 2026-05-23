using UnityEngine;
using System.Collections;

public class ThrottleController : MonoBehaviour
{
    [Header("Movement")]
    public Transform planeTransform;       // drag your airplane root here
    public float maxSpeed = 80f;           // units/sec at full throttle
    public float flyAwayAcceleration = 3f; // how fast it ramps to max during fly-away

    [Header("Optional Audio")]
    public AudioSource engineAudioSource;  // looping engine hum (optional)
    public float minPitch = 0.4f;
    public float maxPitch = 1.8f;

    [HideInInspector] public float currentThrottle = 0f; // 0–1, read by UI if needed

    private bool isFlyingAway = false;

    private void Update()
    {
        // Drive engine audio pitch from throttle (optional, won't crash if null)
        if (engineAudioSource != null)
            engineAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, currentThrottle);

        // Fly-away movement is handled in the coroutine below
    }

    // ── Called by tutorial to smoothly show throttle increasing ──────────────
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

    // ── Called at the end of the tutorial — plane flies off into the distance ─
    public IEnumerator FlyAway()
    {
        // --- Phase 1: Engine slams to full throttle ---
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

        // --- Phase 2: Accelerate forward and pitch nose up ---
        float speed = 5f;
        float travelledDistance = 0f;
        float flyDistance = 600f;
        float currentPitch = 0f;
        float targetPitch = -25f; // negative = nose pitches up in Unity

        while (travelledDistance < flyDistance)
        {
            // Speed ramps up fast
            speed = Mathf.MoveTowards(speed, maxSpeed, flyAwayAcceleration * Time.deltaTime * 50f);

            // Nose gradually lifts
            currentPitch = Mathf.MoveTowards(currentPitch, targetPitch, 12f * Time.deltaTime);
            planeTransform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

            // Move forward along the plane's own axis
            float delta = speed * Time.deltaTime;
            planeTransform.Translate(Vector3.right * delta, Space.Self);
            travelledDistance += delta;

            yield return null;
        }

        // Plane is gone — hide it
        planeTransform.gameObject.SetActive(false);
    }
}