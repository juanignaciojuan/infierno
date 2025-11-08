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
        }
    }
}
