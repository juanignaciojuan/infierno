using UnityEngine;
using TMPro;
using System.Collections;

public class FallRespawnWithMessage : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;    // Where the player respawns

    [Header("UI & Sound")]
    public TMP_Text messageText;      // Your TextMeshPro UI
    public AudioSource audioSource;   // Sound to play
    public float displayTime = 3f;    // Duration to show message

    private bool isShowingMessage = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Stop Rigidbody movement if present
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = Vector3.zero;

            // Temporarily disable CharacterController to move player safely
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

            // Show message with sound
            if (!isShowingMessage)
                StartCoroutine(ShowMessage());
        }
    }

    private IEnumerator ShowMessage()
    {
        isShowingMessage = true;

        if (messageText != null)
            messageText.gameObject.SetActive(true);

        if (audioSource != null)
            audioSource.Play();

        yield return new WaitForSeconds(displayTime);

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        isShowingMessage = false;
    }
}
