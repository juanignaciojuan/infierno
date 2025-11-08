using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class XRProjectile : MonoBehaviour
{
    [Header("Lifetime")]
    [Tooltip("Total lifetime of the projectile in seconds.")]
    public float lifetime = 20f;
    [Tooltip("How many seconds before destruction the fade-out should start.")]
    public float fadeDuration = 2f;

    [Header("Impact Effect")]
    public GameObject impactEffect;

    private Transform cameraTransform;
    private SpriteRenderer spriteRenderer;
    private Collider projectileCollider;
    private bool isArmed = false;

    private void Awake()
    {
        // Get components and immediately disable the collider to prevent instant collision.
        projectileCollider = GetComponent<Collider>();
        projectileCollider.enabled = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Find the main camera to face it.
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Start the fade-out and destruction process.
        StartCoroutine(FadeOutAndDestroy());
    }

    /// <summary>
    /// Enables the collider after a short delay, allowing it to pass through the weapon that fired it.
    /// </summary>
    public void Arm(float delay)
    {
        StartCoroutine(ArmRoutine(delay));
    }

    private IEnumerator ArmRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        projectileCollider.enabled = true;
        isArmed = true;
    }

    private void Update()
    {
        // Billboarding: Make the sprite always face the camera.
        if (cameraTransform != null)
        {
            Vector3 toCam = cameraTransform.position - transform.position;
            if (toCam.sqrMagnitude > 0.0001f)
            {
                // Face the camera without flipping/mirroring artifacts
                transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
            }
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        // Wait for the main part of the projectile's life.
        yield return new WaitForSeconds(lifetime - fadeDuration);

        // Fade out over the remaining time.
        float timer = 0;
        Color startColor = spriteRenderer.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Finally, destroy the projectile.
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only register a collision if the projectile is armed.
        if (!isArmed) return;

        if (impactEffect)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
