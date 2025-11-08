using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class XRRifleShoot : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("The different types of projectiles that can be fired. One will be chosen at random.")]
    public GameObject[] projectilePrefabs;
    public Transform muzzleTransform;
    public float projectileSpeed = 20f;

    [Header("Fire Control")]
    public float fireRate = 0.25f;

    [Header("Effects")]
    public AudioSource audioSource;
    public AudioClip[] shotSounds;
    public ParticleSystem muzzleParticles;
    public Light muzzleLight;
    public float lightFlashDuration = 0.05f;

    [Header("Recoil")]
    [Tooltip("The physical force applied to the rifle when firing.")]
    public float recoilForce = 5f;

    [Header("Drone Kill (Optional)")]
    [Tooltip("When enabled, each spawned projectile will kill drones on contact (like grenades).")]
    public bool projectilesKillDrones = false;
    [Tooltip("Delay before the projectile is allowed to kill (seconds), to avoid immediate self-hit.")]
    public float projectileKillerArmDelay = 0.05f;
    
    [Header("Player Motion Compensation")]
    [Tooltip("If true, adds player locomotion velocity to initial projectile speed (use with CharacterController rigs).")]
    public bool addPlayerVelocityToShots = true;
    [Tooltip("Provider that estimates XR Origin velocity.")]
    public XRPlayerVelocityProvider playerVelocityProvider;

    [Header("Hold Alignment / Lag Fix")]
    [Tooltip("Force XRGrabInteractable movement type to Instantaneous to reduce trailing.")]
    public bool forceInstantMovement = true;
    [Tooltip("Apply a constant forward offset while held (meters).")]
    public float heldForwardOffset = 0.06f;
    [Tooltip("Scale additional forward offset by player speed (meters per m/s).")]
    public float velocityForwardScale = 0.02f;
    [Tooltip("Smooth factor for offset (0 = snap).")]
    [Range(0f,1f)] public float offsetSmoothing = 0.3f;
    private Vector3 _currentExtraOffset = Vector3.zero;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private Rigidbody rifleRigidbody;
    private bool canFire = true;

    [Header("Haptics")]
    [Tooltip("Invoked when a shot is fired. Hook this to a Haptic Impulse Player on a controller.")]
    public UnityEvent onFireHaptics;

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rifleRigidbody = GetComponent<Rigidbody>();
        grab.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Multiple;
        if (forceInstantMovement)
        {
            try { grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous; } catch { }
        }
        if (rifleRigidbody)
        {
            rifleRigidbody.interpolation = RigidbodyInterpolation.None; // avoid interpolation lag
        }
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    private void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrabbed);
            grab.selectExited.RemoveListener(OnReleased);
        }
    }

    private bool _held;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor _currentInteractor;

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _held = true; _currentInteractor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor; _currentExtraOffset = Vector3.zero;
    }
    private void OnReleased(SelectExitEventArgs args)
    { _held = false; _currentInteractor = null; }

    private void LateUpdate()
    {
        if (!_held || _currentInteractor == null) return;
        // Determine forward basis (use interactor attach transform if present)
        Transform basis = _currentInteractor.transform;
        Vector3 desiredOffset = basis.forward * heldForwardOffset;
        if (playerVelocityProvider && velocityForwardScale > 0f)
        {
            float speed = playerVelocityProvider.Velocity.magnitude;
            desiredOffset += basis.forward * (speed * velocityForwardScale);
        }
        _currentExtraOffset = Vector3.Lerp(_currentExtraOffset, desiredOffset, 1f - offsetSmoothing);
        // Apply offset without breaking grab pose: move rigidbody or transform directly
        if (rifleRigidbody && rifleRigidbody.isKinematic)
        {
            rifleRigidbody.MovePosition(rifleRigidbody.position + _currentExtraOffset);
        }
        else
        {
            transform.position += _currentExtraOffset;
        }
    }

    /// <summary>
    /// This public method will be called by the XRGrabInteractable's "Activated" event in the Inspector.
    /// </summary>
    public void TryToFire()
    {
        if (canFire)
        {
            StartCoroutine(FireRoutine());
        }
    }

    private IEnumerator FireRoutine()
    {
        canFire = false;
        Fire();
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    [Tooltip("Spawn projectiles at end of frame to match final muzzle pose after pose adjustments.")]
    public bool spawnAtEndOfFrame = true;

    private void Fire()
    {
        if (projectilePrefabs.Length > 0 && muzzleTransform)
        {
            if (spawnAtEndOfFrame) StartCoroutine(SpawnProjectileAtEndOfFrame());
            else SpawnProjectileImmediate();

            // Apply recoil
            if (rifleRigidbody != null)
            {
                rifleRigidbody.AddForce(-muzzleTransform.forward * recoilForce, ForceMode.Impulse);
            }

            // Audio
            if (audioSource && shotSounds.Length > 0)
            {
                AudioClip clip = shotSounds[Random.Range(0, shotSounds.Length)];
                audioSource.PlayOneShot(clip);
            }

            // Haptics via UnityEvent so you can wire to Haptic Impulse Player in Inspector
            onFireHaptics?.Invoke();

            // Visuals
            if (muzzleParticles) muzzleParticles.Play();
            if (muzzleLight) StartCoroutine(LightFlash());
        }
    }

    private IEnumerator SpawnProjectileAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        SpawnProjectileImmediate();
    }

    private void SpawnProjectileImmediate()
    {
        int randomIndex = Random.Range(0, projectilePrefabs.Length);
        GameObject prefab = projectilePrefabs[randomIndex];
        GameObject proj = Instantiate(prefab, muzzleTransform.position, muzzleTransform.rotation, null);

        if (proj.TryGetComponent<XRProjectile>(out var projectile))
        {
            projectile.Arm(0.1f);
        }

        if (projectilesKillDrones)
        {
            var killer = proj.AddComponent<XRDroneKillerOnContact>();
            _ = projectileKillerArmDelay;
        }

        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 v = muzzleTransform.forward * projectileSpeed;
            if (addPlayerVelocityToShots && playerVelocityProvider != null)
            {
                v += playerVelocityProvider.Velocity;
            }
            rb.linearVelocity = v;
        }
    }

    private IEnumerator LightFlash()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(lightFlashDuration);
        muzzleLight.enabled = false;
    }
}
