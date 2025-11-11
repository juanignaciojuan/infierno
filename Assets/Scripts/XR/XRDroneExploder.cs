using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Makes a drone explode (VFX + SFX + physics force) and spawn armed grenades when triggered.
/// Call Explode() from damage system, health reaching zero, or external event.
/// </summary>
[DisallowMultipleComponent]
public class DroneExploder : MonoBehaviour
{
    [Header("Explosion")]
    [Tooltip("Particle effect prefab for the drone explosion.")] public GameObject explosionEffectPrefab;
    [Tooltip("Explosion sound clip.")] public AudioClip explosionSound;
    [Tooltip("Physics force applied outward.")] public float explosionForce = 1200f;
    [Tooltip("Radius for physics force.")] public float explosionRadius = 6f;
    [Tooltip("Upwards force modifier for AddExplosionForce.")] public float upwardsModifier = 0.5f;

    [Header("Grenade Spawning")] 
    [Tooltip("Grenade prefab using XRGrenade script.")] public GameObject grenadePrefab;
    [Tooltip("Number of grenades to spawn on drone explosion.")] public int grenadesToSpawn = 2;
    [Tooltip("Random horizontal spread radius for spawned grenades.")] public float grenadeSpawnSpread = 0.6f;
    [Tooltip("Vertical offset added to spawn position.")] public float verticalOffset = 0.2f;
    [Tooltip("Delay after spawn before grenades arm (seconds). Set 0 for immediate.")] public float grenadeArmDelay = 0.15f;

    [Header("Drone Settings")] 
    [Tooltip("Optional destroy delay after explosion (lets audio finish)." )] public float destroyDelay = 0.1f;
    [Tooltip("If true, multiple Explode() calls are ignored once triggered.")] public bool singleUse = true;

    private AudioSource _audio;
    private bool _exploded;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }
    }

    /// <summary>
    /// Public trigger for explosion.
    /// </summary>
    public void Explode()
    {
        if (_exploded && singleUse) return;
        _exploded = true;

        // 1. VFX
        if (explosionEffectPrefab)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps) Destroy(fx, ps.main.duration);
            else Destroy(fx, 5f);
        }

        // 2. SFX
        if (explosionSound)
        {
            _audio.PlayOneShot(explosionSound);
        }

        // 3. Physics force
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            if (c.attachedRigidbody != null)
            {
                c.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
        }

        // 4. Spawn grenades
        SpawnGrenades();

        // 5. Disable drone visuals / colliders optionally before destroy
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

    private void SpawnGrenades()
    {
        if (grenadePrefab == null || grenadesToSpawn <= 0) return;

        for (int i = 0; i < grenadesToSpawn; i++)
        {
            Vector2 spread = Random.insideUnitCircle * grenadeSpawnSpread;
            Vector3 spawnPos = transform.position + new Vector3(spread.x, verticalOffset, spread.y);
            GameObject g = Instantiate(grenadePrefab, spawnPos, Random.rotation);

            // Give a mild outward shove
            if (g.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = (spawnPos - transform.position).normalized;
                rb.AddForce(dir * 2f, ForceMode.Impulse);
            }

            if (g.TryGetComponent<XRGrenade>(out var grenade))
            {
                grenade.requireGrabToArm = false; // drone spawned, so no grab needed
                grenade.ArmAfter(grenadeArmDelay);
            }
        }
    }

    // Optional test hook: explode on collision with ground or player
    private void OnCollisionEnter(Collision collision)
    {
        if (!_exploded && collision.collider.CompareTag("Ground"))
        {
            Explode();
        }
    }
}
