using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.Playables;
using System.Collections;

/**
@author - mahir
*/

public class BufferingVibrationController : MonoBehaviour
{
    private HapticImpulsePlayer leftHaptic;
    private HapticImpulsePlayer rightHaptic;
    private PlayableDirector activeTimeline;
    private ModuleManager moduleManager;

    public float vibrationIntensity = 1.0f;
    public float vibrationDuration = 8f;
    public float vibrationStartTime = 8.69f;

    private bool hasTriggered = false;

    private void Start()
    {
        moduleManager = FindObjectOfType<ModuleManager>();
    }

    private void Update()
    {
        if (moduleManager == null)
        {
            Debug.Log("[Vibration] moduleManager is NULL");
            return;
        }

        PlayableDirector currentDirector = moduleManager.GetActiveDirector;
        if (currentDirector == null)
        {
            Debug.Log("[Vibration] currentDirector is NULL");
            return;
        }

        string timelineName = currentDirector.playableAsset != null ? currentDirector.playableAsset.name : "Unknown";

        if (!timelineName.Contains("S2_Timeline_2"))
        {
            if (hasTriggered)
                hasTriggered = false;
            return;
        }

        activeTimeline = currentDirector;

        if (leftHaptic == null && rightHaptic == null)
        {
            InitializeControllers();
        }

        if (!currentDirector.isActiveAndEnabled)
        {
            Debug.Log("[Vibration] currentDirector is not active");
            return;
        }

        double currentTime = currentDirector.time;

        if (!hasTriggered && currentTime >= vibrationStartTime)
        {
            Debug.Log($"[Vibration] TRIGGER EVENT at time {currentTime:F3}s (target: {vibrationStartTime}s)");
            TriggerBufferingVibration();
            hasTriggered = true;
        }
    }

    private void InitializeControllers()
    {
        HapticImpulsePlayer[] haptics = FindObjectsOfType<HapticImpulsePlayer>();
        Debug.Log($"[Vibration] Found {haptics.Length} HapticImpulsePlayers");

        foreach (HapticImpulsePlayer haptic in haptics)
        {
            if (haptic.gameObject.name.Contains("Left"))
            {
                leftHaptic = haptic;
                Debug.Log($"[Vibration] Assigned LEFT: {haptic.gameObject.name}");
            }
            else if (haptic.gameObject.name.Contains("Right"))
            {
                rightHaptic = haptic;
                Debug.Log($"[Vibration] Assigned RIGHT: {haptic.gameObject.name}");
            }
        }

        if (leftHaptic == null && haptics.Length > 0)
        {
            leftHaptic = haptics[0];
            Debug.Log($"[Vibration] Assigned LEFT by order: {haptics[0].gameObject.name}");
        }
        if (rightHaptic == null && haptics.Length > 1)
        {
            rightHaptic = haptics[1];
            Debug.Log($"[Vibration] Assigned RIGHT by order: {haptics[1].gameObject.name}");
        }
    }

    private void TriggerBufferingVibration()
    {
        Debug.Log($"[Vibration] Left haptic: {(leftHaptic != null ? "FOUND" : "NULL")}");
        Debug.Log($"[Vibration] Right haptic: {(rightHaptic != null ? "FOUND" : "NULL")}");

        StartCoroutine(SustainedVibration());
    }

    private IEnumerator SustainedVibration()
    {
        float elapsed = 0f;
        float pulseDuration = 0.1f;

        while (elapsed < vibrationDuration)
        {
            leftHaptic?.SendHapticImpulse(vibrationIntensity, pulseDuration);
            rightHaptic?.SendHapticImpulse(vibrationIntensity, pulseDuration);
            yield return new WaitForSeconds(pulseDuration);
            elapsed += pulseDuration;
        }

        Debug.Log($"[Vibration] HAPTIC FINISHED (lasted {vibrationDuration}s)");
    }
}