using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

/// <summary>
/// Spawns "glyph" prefabs (quads/sprites) a short distance in front of the rifle's muzzle
/// and leaves them frozen in air, like drawing letters in space. Intended to be bound to
/// XRGrabInteractable.Activated via the Inspector using TryToDraw().
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class XRAirDrawShoot : MonoBehaviour
{
    [Header("Glyph Prefabs")]
    [Tooltip("Prefabs to spawn as static glyphs (quads or sprites). One is chosen at random each shot.")]
    public GameObject[] glyphPrefabs;

    [Header("Placement")]
    public Transform muzzleTransform;
    [Tooltip("Distance in meters forward from the muzzle where the glyph should appear.")]
    public float offsetDistance = 0.12f;

    [Header("Timing")]
    [Tooltip("Minimum time between glyph placements.")]
    public float fireRate = 0.08f;
    [Tooltip("How long a spawned glyph lives before disappear.")]
    public float glyphLifetime = 6f;
    [Tooltip("How long the glyph takes to fade out at the end of its lifetime.")]
    public float fadeDuration = 1.5f;

    [Header("Visual/Audio Effects (optional)")]
    public AudioSource audioSource;
    public AudioClip[] shotSounds;
    public ParticleSystem muzzleParticles;
    public Light muzzleLight;
    public float lightFlashDuration = 0.03f;

    [Header("Behavior")]
    [Tooltip("Force spawned glyphs to face the main camera every frame.")]
    public bool billboardToCamera = true;
    [Tooltip("Disable physics on the spawned glyph (recommended). If a Rigidbody exists, set kinematic & disable gravity.")]
    public bool disablePhysicsOnSpawn = true;
    [Tooltip("If the glyph has a Collider, disable it to guarantee no accidental hits.")]
    public bool disableColliderOnSpawn = true;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;
    private bool _canFire = true;

    private void Awake()
    {
        _grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        // Do not auto-bind events here; expose public TryToDraw() and bind it via Inspector.
    }

    /// <summary>
    /// Public method to bind in XRGrabInteractable.Activated.
    /// </summary>
    public void TryToDraw()
    {
        if (_canFire)
        {
            StartCoroutine(FireRoutine());
        }
    }

    private IEnumerator FireRoutine()
    {
        _canFire = false;
        SpawnGlyph();
        yield return new WaitForSeconds(fireRate);
        _canFire = true;
    }

    private void SpawnGlyph()
    {
        if (glyphPrefabs == null || glyphPrefabs.Length == 0 || muzzleTransform == null)
            return;

        int index = Random.Range(0, glyphPrefabs.Length);
        GameObject prefab = glyphPrefabs[index];

        Vector3 pos = muzzleTransform.position + muzzleTransform.forward * offsetDistance;
        Quaternion rot = muzzleTransform.rotation;

        GameObject glyph = Instantiate(prefab, pos, rot, null); // ensure no parent so it stays in world

        // Ensure it stays static in space (no physics)
        if (disablePhysicsOnSpawn && glyph.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (disableColliderOnSpawn && glyph.TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }

        // Attach/Configure glyph controller
        var ctrl = glyph.GetComponent<XRAirGlyph>();
        if (ctrl == null) ctrl = glyph.AddComponent<XRAirGlyph>();
        ctrl.billboard = billboardToCamera;
        ctrl.Initialize(glyphLifetime, fadeDuration);

        // Optional shot effects
        if (audioSource && shotSounds != null && shotSounds.Length > 0)
        {
            audioSource.PlayOneShot(shotSounds[Random.Range(0, shotSounds.Length)]);
        }
        if (muzzleParticles) muzzleParticles.Play();
        if (muzzleLight) StartCoroutine(LightFlash());
    }

    private IEnumerator LightFlash()
    {
        if (muzzleLight == null) yield break;
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(lightFlashDuration);
        muzzleLight.enabled = false;
    }
}
