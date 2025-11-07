using UnityEngine;

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
            GameObject explosionInstance = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = explosionInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(explosionInstance, ps.main.duration);
            }
            else
            {
                Destroy(explosionInstance, 5f); // Fallback
            }
        }

        // 2. Play explosion sound
        if (audioSource != null && explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // 3. Apply physics force
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
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
            }
        }

        // 5. Destroy the bomb GameObject
        Destroy(gameObject);
    }
}
