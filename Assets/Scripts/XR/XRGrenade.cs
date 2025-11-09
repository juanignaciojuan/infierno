using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Handles the behavior of a grenade in a VR environment.
/// The grenade explodes upon contact with objects tagged as "Ground".
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class XRGrenade : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("The particle effect prefab to instantiate on explosion.")]
    public GameObject explosionEffectPrefab;

    [Tooltip("The sound to play on explosion.")]
    public AudioClip explosionSound;

    [Tooltip("The force applied by the explosion to surrounding objects.")]
    public float explosionForce = 1000f;

    [Tooltip("The radius of the explosion's effect.")]
    public float explosionRadius = 5f;

    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    private bool wasGrabbed = false;
    private bool hasExploded = false;
    private bool isArmed = false;

    [Header("Haptics")]
    [Tooltip("Invoked on explosion. Wire to controller Haptic Impulse Player.")]
    public UnityEvent onExplosionHaptics;

    [Header("Arming")]
    [Tooltip("If true, grenade only arms after being grabbed. If false, can be armed externally (e.g. drone).")]
    public bool requireGrabToArm = true;
    [Tooltip("If > 0, automatically destroy (fail-safe) after this many seconds even if not exploded.")]
    public float maxLifetime = 0f;

    [Header("Player Impact (Optional)")]
    [Tooltip("If true, player will be pushed even if they lack a Rigidbody.")]
    public bool affectPlayer = true;
    [Tooltip("Tag used to identify player root or collider for knockback.")]
    public string playerTag = "Player";
    [Tooltip("Horizontal knockback force applied if player has Rigidbody (explosion force override).")]
    public float playerPushForce = 600f;
    [Tooltip("Additional upward force applied to player.")]
    public float playerUpForce = 150f;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
    }

    private void Start()
    {
        // Get the AudioSource component attached to this GameObject.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // If no AudioSource is present, add one to play the explosion sound.
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        wasGrabbed = true;
        if (requireGrabToArm)
        {
            Arm();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Explode only if armed and not yet exploded.
        if (isArmed && !hasExploded && ShouldExplodeFor(collision.gameObject))
        {
            hasExploded = true;
            Explode();
        }
    }

    private bool ShouldExplodeFor(GameObject other)
    {
        // Support common ground/terrain cases
        if (other.CompareTag("Ground")) return true;
        if (other.CompareTag("Terrain")) return true;
        if (other.GetComponent<TerrainCollider>() != null) return true;
        return false;
    }

    /// <summary>
    /// Triggers the explosion effects.
    /// </summary>
    private void Explode()
    {
        // 1. Instantiate particle effect
        if (explosionEffectPrefab != null)
        {
            // Use pooled VFX to reduce spikes/GC
            VFXPool.Spawn(explosionEffectPrefab, transform.position, transform.rotation);
        }

        // 2. Play explosion sound
        if (audioSource != null && explosionSound != null)
        {
            // Play the sound at the point of explosion without being attached to the grenade
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // 3. Apply physics force
        // Find all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            // If a collider has a Rigidbody, apply the explosion force
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Non-lethal impulse receiver (NPCs / walkers)
            var nl = hit.GetComponentInParent<XRNonLethalImpulseReceiver>();
            if (nl != null)
            {
                nl.ApplyImpulse(transform.position, explosionForce, explosionRadius);
            }
        }

        // Optional player knockback
        if (affectPlayer && !string.IsNullOrEmpty(playerTag))
        {
            foreach (Collider hit in colliders)
            {
                if (!hit || !hit.CompareTag(playerTag)) continue;
                Rigidbody rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    rb.AddExplosionForce(playerPushForce, transform.position, explosionRadius);
                    rb.AddForce(Vector3.up * playerUpForce, ForceMode.Impulse);
                }
                else
                {
                    Transform t = hit.transform;
                    Vector3 dir = (t.position - transform.position).normalized;
                    float scalar = playerPushForce / 600f;
                    t.position += (dir + Vector3.up * 0.3f) * scalar;
                }
            }
        }

        // 4. Destroy the grenade GameObject
    onExplosionHaptics?.Invoke();
    HapticsBus.FireClosest(transform.position, 0.45f, 0.12f);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif

    /// <summary>
    /// Arms the grenade so it can explode on ground impact.
    /// </summary>
    public void Arm()
    {
        isArmed = true;
        if (maxLifetime > 0f)
        {
            CancelInvoke(nameof(SelfDestruct));
            Invoke(nameof(SelfDestruct), maxLifetime);
        }
    }

    /// <summary>
    /// External helper for drones: arm after delay.
    /// </summary>
    public void ArmAfter(float delay)
    {
        if (delay <= 0f)
        {
            Arm();
        }
        else
        {
            Invoke(nameof(Arm), delay);
        }
    }

    /// <summary>
    /// Returns true if player has grabbed this grenade at least once (used by drone hit logic).
    /// </summary>
    public bool HasBeenPlayerHandled => wasGrabbed;

    private void SelfDestruct()
    {
        if (!hasExploded)
        {
            Explode();
        }
    }
}
