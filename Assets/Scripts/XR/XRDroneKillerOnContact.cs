using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple component placed on rifle projectiles (optional) that kills drones on contact.
/// It will look for XRDroneHitAndRespawn on the hit target or its parents and call KillNow().
/// Supports both collision and trigger-based hits (depending on projectile collider setup).
/// </summary>
[DisallowMultipleComponent]
public class XRDroneKillerOnContact : MonoBehaviour
{
    [Tooltip("If true, only kill drones that were already marked as 'vulnerable'. Not used yet but reserved.")]
    public bool requireVulnerableFlag = false;

    [Header("Haptics")]
    [Tooltip("Invoked when projectile kills a drone. Wire to Haptic Impulse Player.")]
    public UnityEvent onKillHaptics;

    [Tooltip("If true and no UnityEvent listeners are assigned, will auto play haptics on all XRHapticImpulseProxy components in the scene.")]
    public bool autoPlayIfUnassigned = true;
    [Tooltip("Amplitude used when auto playing haptics. Set <=0 to use Play() instead of PlayAmplitude(float).")]
    [Range(0f,1f)]
    public float autoAmplitude = 0.2f;

    private void OnCollisionEnter(Collision collision)
    {
        TryKill(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryKill(other);
    }

    private void TryKill(Collider col)
    {
        var hit = col.GetComponentInParent<XRDroneHitAndRespawn>();
        if (hit != null)
        {
            if (!requireVulnerableFlag) hit.KillNow();
            onKillHaptics?.Invoke();
            // Auto fallback if user couldn't wire scene controllers due to prefab context
            if (autoPlayIfUnassigned && (onKillHaptics == null || onKillHaptics.GetPersistentEventCount() == 0))
            {
                var proxies = FindObjectsByType<XRHapticImpulseProxy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var p in proxies)
                {
                    if (p == null) continue;
                    if (autoAmplitude > 0f) p.PlayAmplitude(autoAmplitude); else p.Play();
                }
            }
        }
    }
}
