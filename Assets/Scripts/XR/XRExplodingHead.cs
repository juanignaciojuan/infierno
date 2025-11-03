using UnityEngine;

public class ExplodingHead : MonoBehaviour
{
    public GameObject explosionEffect;
    public AudioClip explosionSound;
    public float destroyDelay = 2f;

    private bool exploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!exploded && collision.relativeVelocity.magnitude > 1f)
        {
            exploded = true;
            if (explosionEffect)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            
            if (explosionSound)
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            
            Destroy(gameObject, destroyDelay);
        }
    }
}
