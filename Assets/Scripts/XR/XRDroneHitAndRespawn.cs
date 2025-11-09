using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Handles a drone being destroyed by a player-thrown grenade, falling to the ground,
/// disappearing after a short while, and respawning at its original spawn point.
/// Attach to the drone root. Requires a Collider. Optionally uses an existing Rigidbody
/// or will add one on death.
/// </summary>
[DisallowMultipleComponent]
public class XRDroneHitAndRespawn : MonoBehaviour
{
    [Header("Hit Detection")]
    [Tooltip("If true, only grenades that were grabbed by the player will kill the drone.")]
    public bool requirePlayerHandledGrenade = true;
    [Tooltip("If true, also react to trigger volumes (OnTriggerEnter). If false, only OnCollision.")]
    public bool acceptTriggerHits = true;

    [Header("Death VFX/SFX")]
    public GameObject deathVfxPrefab;
    public AudioClip deathSfx;
    [Tooltip("Optional hit VFX played immediately when a valid hit is detected.")]
    public GameObject hitVfxPrefab;
    [Tooltip("Optional hit sound played immediately on valid hit.")]
    public AudioClip hitSfx;

    [Header("Death Physics")]
    [Tooltip("Impulse applied when the drone is destroyed (knock-down effect).")]
    public float deathImpulse = 3f;
    [Tooltip("Upwards impulse on death.")]
    public float deathUpImpulse = 1f;

    [Header("Timers")]
    [Tooltip("Seconds the wreck remains visible on the ground before disappearing.")]
    public float disappearAfterSeconds = 3f;
    [Tooltip("Seconds after disappearance before respawn at original location.")]
    public float respawnDelay = 2f;

    private Vector3 _spawnPos;
    private Quaternion _spawnRot;
    private bool _dead;
    private AudioSource _audio;
    // Track whether we added a temporary Rigidbody at death (none existed originally)
    private bool _addedRuntimeRigidbody;
    private bool _rbOriginalKinematic;
    private bool _rbOriginalUseGravity;
    private RigidbodyConstraints _rbOriginalConstraints;

    [Header("Haptics")]
    [Tooltip("Invoked on valid hit/death events. Wire to controller Haptic Impulse Player.")]
    public UnityEvent onHitOrDeathHaptics;

    // Cached components that may be present
    private MonoBehaviour _patrolA; // DronePatrol
    private MonoBehaviour _patrolB; // XRDronePatrol if different name

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        _spawnPos = transform.position;
        _spawnRot = transform.rotation;
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }

        // Try find patrol scripts if attached
        _patrolA = GetComponent<DronePatrol>();
        // Attempt to find a different patrol type by name if needed
        if (_patrolA == null)
        {
            var comp = GetComponent("XRDronePatrol") as MonoBehaviour;
            if (comp != null) _patrolB = comp;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_dead) return;

        // If hit by a grenade, check conditions
        var grenade = collision.collider.GetComponentInParent<XRGrenade>();
        if (grenade != null)
        {
            if (!requirePlayerHandledGrenade || grenade.HasBeenPlayerHandled)
            {
                SpawnHitFeedback(collision.GetContact(0).point);
                onHitOrDeathHaptics?.Invoke();
                StartCoroutine(DieAndRespawn());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!acceptTriggerHits || _dead) return;
        var grenade = other.GetComponentInParent<XRGrenade>();
        if (grenade != null)
        {
            if (!requirePlayerHandledGrenade || grenade.HasBeenPlayerHandled)
            {
                SpawnHitFeedback(other.ClosestPoint(transform.position));
                onHitOrDeathHaptics?.Invoke();
                StartCoroutine(DieAndRespawn());
                return;
            }
        }

        // Also support a generic killer marker (e.g., rifle projectiles)
        var killer = other.GetComponentInParent<XRDroneKillerOnContact>();
        if (killer != null)
        {
            SpawnHitFeedback(other.ClosestPoint(transform.position));
            onHitOrDeathHaptics?.Invoke();
            StartCoroutine(DieAndRespawn());
        }
    }

    /// <summary>
    /// External trigger to kill this drone now (e.g., from a projectile script).
    /// </summary>
    public void KillNow()
    {
        if (_dead) return;
        StartCoroutine(DieAndRespawn());
    }

    private IEnumerator DieAndRespawn()
    {
        // Cache original rigidbody state if any BEFORE modifying it
        var existingRb = GetComponent<Rigidbody>();
        if (existingRb != null)
        {
            _rbOriginalKinematic = existingRb.isKinematic;
            _rbOriginalUseGravity = existingRb.useGravity;
            _rbOriginalConstraints = existingRb.constraints;
        }
        _dead = true;

        // VFX
        if (deathVfxPrefab)
        {
            var fx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps) Destroy(fx, ps.main.duration);
            else Destroy(fx, 5f);
        }
        // SFX
        if (deathSfx) _audio.PlayOneShot(deathSfx);

        // Disable patrol
        if (_patrolA) _patrolA.enabled = false;
        if (_patrolB) _patrolB.enabled = false;

        // Ensure a Rigidbody to fall
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb == null) { rb = gameObject.AddComponent<Rigidbody>(); _addedRuntimeRigidbody = true; }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(transform.forward * deathImpulse + Vector3.up * deathUpImpulse, ForceMode.Impulse);

        // Wait on the ground then hide
        yield return new WaitForSeconds(disappearAfterSeconds);
        SetRenderersEnabled(false);
        SetCollidersEnabled(false);

        // Respawn later
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    void Respawn()
    {
        // Reset position and rotation to initial state
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Re-enable components for respawn
        SetRenderersEnabled(true);
        SetCollidersEnabled(true);
        if (_patrolA) _patrolA.enabled = true;
        if (_patrolB) _patrolB.enabled = true;

        // Reset death flag
        _dead = false;

        // If a Rigidbody was used, reset its physics properties
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (_addedRuntimeRigidbody)
            {
                // We added this just for death fall; remove to restore original lightweight state
                Destroy(rb);
                _addedRuntimeRigidbody = false;
            }
            else
            {
                rb.isKinematic = _rbOriginalKinematic;
                rb.useGravity = _rbOriginalUseGravity;
                rb.constraints = _rbOriginalConstraints;
            }
        }

        // Reactivate the GameObject
        gameObject.SetActive(true);
    }

    private void SetRenderersEnabled(bool enabled)
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = enabled;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = enabled;
    }

    private void SpawnHitFeedback(Vector3 pos)
    {
        if (hitVfxPrefab)
        {
            var fx = Instantiate(hitVfxPrefab, pos, Quaternion.identity);
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps) Destroy(fx, ps.main.duration);
            else Destroy(fx, 2f);
        }
        if (hitSfx && _audio)
        {
            _audio.PlayOneShot(hitSfx);
        }
    }
}
