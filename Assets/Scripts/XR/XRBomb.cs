using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles the behavior of a bomb, typically dropped by a drone.
/// The bomb explodes on contact with the ground, spawning smaller grenades.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class XRBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [Tooltip("Seconds after spawn before the bomb can explode.")]
    [Min(0f)]
    public float armDelay = 0.2f;

    [Tooltip("Primary tag that will trigger the explosion. Leave empty to explode on any collision.")]
    public string groundTag = "Terrain";

    [Tooltip("Secondary tag that will also trigger the explosion, e.g., 'Ground'.")]
    public string secondaryGroundTag = "Ground";

    [Tooltip("If true, the bomb explodes on any collision once armed, regardless of tag.")]
    public bool explodeOnAnyCollision = false;

    [Header("Explosion Settings")]
    [Tooltip("The particle effect prefab to instantiate on explosion.")]
    public GameObject explosionEffectPrefab;

    [Tooltip("The sound to play on explosion.")]
    public AudioClip explosionSound;

    [Tooltip("The force applied by the explosion to surrounding objects.")]
    public float explosionForce = 1500f;

    [Tooltip("The radius of the explosion's effect.")]
    public float explosionRadius = 10f;

    [Header("Post-Explosion Settings")]
    [Tooltip("The grenade prefab to spawn after the bomb explodes.")]
    public GameObject grenadePrefab;

    [Tooltip("The number of grenades to spawn after the explosion.")]
    public int numberOfGrenadesToSpawn = 2;

    private AudioSource audioSource;
    private bool hasExploded = false;
    private bool isArmed = false;

    [Header("Haptics")]
    [Tooltip("Invoked on bomb explosion. Wire to controller Haptic Impulse Player.")]
    public UnityEvent onExplosionHaptics;

    [Header("Player Impact (Optional)")]
    [Tooltip("If true, player will be pushed even if they lack a Rigidbody.")]
    public bool affectPlayer = true;
    [Tooltip("Tag used to identify player root or collider for knockback.")]
    public string playerTag = "Player";
    [Tooltip("Horizontal knockback force applied if player has Rigidbody (explosion force override).")]
    public float playerPushForce = 600f;
    [Tooltip("Additional upward force applied to player.")]
    public float playerUpForce = 150f;

    private void Start()
    {
        // Get or add an AudioSource component.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        ArmAfter(armDelay);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded || !isArmed)
        {
            return;
        }

        if (ShouldDetonateFor(collision.gameObject))
        {
            hasExploded = true;
            Explode();
        }
    }

    /// <summary>
    /// Allows external callers to re-arm the bomb after a custom delay.
    /// </summary>
    public void ArmAfter(float delay)
    {
        CancelInvoke(nameof(Arm));

        if (delay <= 0f)
        {
            Arm();
        }
        else
        {
            Invoke(nameof(Arm), delay);
        }
    }

    private void Arm()
    {
        isArmed = true;
    }

    private bool ShouldDetonateFor(GameObject other)
    {
        if (explodeOnAnyCollision)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(groundTag) && other.tag == groundTag)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(secondaryGroundTag) && other.tag == secondaryGroundTag)
        {
            return true;
        }

        // TerrainCollider covers Unity terrains even if untagged.
        return other.GetComponent<TerrainCollider>() != null;
    }

    /// <summary>
    /// Triggers the explosion effects and spawns grenades.
    /// </summary>
    private void Explode()
    {
        // 1. Instantiate particle effect
        if (explosionEffectPrefab != null)
        {
            // Use pool instead of Instantiate+Destroy for performance
            GameObject explosionInstance = VFXPool.Spawn(explosionEffectPrefab, transform.position, transform.rotation);
        }

        // 2. Play explosion sound
        if (audioSource != null && explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // 3. Apply physics force & non-lethal NPC pushes
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
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

        // 3b. Player knockback (if player lacks rigidbody or needs an extra push)
        if (affectPlayer && !string.IsNullOrEmpty(playerTag))
        {
            foreach (Collider hit in colliders)
            {
                if (!hit || !hit.CompareTag(playerTag)) continue;
                Rigidbody rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    // Add stronger player-specific force
                    rb.AddExplosionForce(playerPushForce, transform.position, explosionRadius);
                    rb.AddForce(Vector3.up * playerUpForce, ForceMode.Impulse);
                }
                else
                {
                    // Fallback manual translation if no RB (e.g., XR rig root without physics)
                    Transform t = hit.transform;
                    Vector3 dir = (t.position - transform.position).normalized;
                    float scalar = playerPushForce / 600f; // scale relative to default
                    t.position += (dir + Vector3.up * 0.3f) * scalar;
                }
            }
        }

        // 4. Spawn grenades
    if (grenadePrefab != null && numberOfGrenadesToSpawn > 0)
        {
            for (int i = 0; i < numberOfGrenadesToSpawn; i++)
            {
                // Spawn with a slight upward and outward velocity to spread them out
                Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 0.5f;
                GameObject spawnedGrenade = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);
                
                // Give the spawned grenade a little push
                Rigidbody grenadeRb = spawnedGrenade.GetComponent<Rigidbody>();
                if (grenadeRb != null)
                {
                    grenadeRb.AddExplosionForce(200f, transform.position, 5f);
                }

                // Ensure spawned grenades are NOT armed until the player grabs them
                var grenadeComp = spawnedGrenade.GetComponent<XRGrenade>();
                if (grenadeComp != null)
                {
                    grenadeComp.requireGrabToArm = true; // must be grabbed to arm
                    // Do NOT call Arm/ArmAfter here; they remain safe until grabbed
                }
            }
        }

        // 5. Destroy the bomb GameObject
        onExplosionHaptics?.Invoke();

        // Optional: HapticsBus fire closest (strong) vs others (weak) if distance scaling desired
        HapticsBus.FireClosest(transform.position, 0.55f, 0.18f);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
