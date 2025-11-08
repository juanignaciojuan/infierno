using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Adapter that converts distance to a scaled haptic amplitude.
/// Place this on the same object that raises the source UnityEvent (e.g., grenade/bomb),
/// and wire SourceEvent -> XRDistanceToAmplitude.InvokeScaled, then wire OnScaledAmplitude
/// into a Haptic Impulse Player method that accepts amplitude (if supported) or to a proxy
/// that sets amplitude before triggering.
///
/// If your Haptic Impulse Player only exposes a no-arg Play(), check 'triggerAfterSet'
/// and wire 'Trigger()' to it after the amplitude is set via a compatible receiver.
/// </summary>
[DisallowMultipleComponent]
public class XRDistanceToAmplitude : MonoBehaviour
{
    [Tooltip("Transform to measure distance from (usually XR Origin or Main Camera). If empty, Camera.main is used.")]
    public Transform target;

    [Header("Range Mapping (meters -> amplitude)")]
    [Tooltip("Distance at which amplitude = maxAmplitude.")]
    public float minDistance = 0.5f;
    [Tooltip("Distance at which amplitude = minAmplitude.")]
    public float maxDistance = 10f;

    [Header("Amplitude Output")]
    [Range(0f, 1f)] public float minAmplitude = 0.05f;
    [Range(0f, 1f)] public float maxAmplitude = 0.6f;

    [Header("Trigger Control")]
    [Tooltip("If true, invokes triggerAfterSetEvent after computing amplitude (for players that need a separate trigger).")]
    public bool triggerAfterSet = false;
    public UnityEvent triggerAfterSetEvent;

    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Tooltip("Event receiving the scaled amplitude (0..1). Wire this into a haptic component that supports amplitude input.")]
    public FloatEvent OnScaledAmplitude;

    public void InvokeScaled()
    {
        Transform t = target;
        if (t == null) t = Camera.main != null ? Camera.main.transform : null;
        if (t == null) return;

        float d = Vector3.Distance(t.position, transform.position);
        float amp = ComputeAmplitude(d);
        OnScaledAmplitude?.Invoke(amp);
        if (triggerAfterSet)
            triggerAfterSetEvent?.Invoke();
    }

    private float ComputeAmplitude(float distance)
    {
        if (maxDistance <= minDistance)
            return maxAmplitude;
        float normalized = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        float amp = Mathf.Lerp(maxAmplitude, minAmplitude, normalized);
        return Mathf.Clamp01(amp);
    }
}
