using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gating helper for UnityEvents: call InvokeIfClose() from another event and it will
/// only forward to onPassed if the target is within maxDistance. Useful to trigger
/// controller Haptic Impulse Players only when the player is near an explosion.
/// </summary>
[DisallowMultipleComponent]
public class XRDistanceEventGate : MonoBehaviour
{
    [Tooltip("Transform to measure distance from (usually XR Origin or Camera). If empty, will try Camera.main.")]
    public Transform target;

    [Tooltip("Maximum distance required to pass the gate.")]
    public float maxDistance = 8f;

    [Tooltip("Event invoked when the target is within maxDistance when InvokeIfClose is called.")]
    public UnityEvent onPassed;

    /// <summary>
    /// Checks distance and invokes onPassed if within maxDistance.
    /// Wire this method to your source UnityEvent (e.g., bomb onExplosionHaptics).
    /// </summary>
    public void InvokeIfClose()
    {
        Transform t = target;
        if (t == null)
        {
            if (Camera.main != null) t = Camera.main.transform;
            else t = null;
        }
        if (t == null)
        {
            // Nothing to compare against; do nothing silently
            return;
        }
        float dist = Vector3.Distance(t.position, transform.position);
        if (dist <= maxDistance)
        {
            onPassed?.Invoke();
        }
    }
}
