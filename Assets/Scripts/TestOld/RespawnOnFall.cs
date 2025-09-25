using UnityEngine;

public class FallRespawn : MonoBehaviour
{
    public Transform respawnPoint;  // Assign in inspector

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the player
        if (other.CompareTag("Player"))
        {
            // Stop Rigidbody movement if present
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = Vector3.zero;

            // If using CharacterController, temporarily disable it to move
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = respawnPoint.position;
                cc.enabled = true;
            }
            else
            {
                other.transform.position = respawnPoint.position;
            }
        }
    }
}
