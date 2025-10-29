using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class RespawnOnTrigger : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint;           // asignar transform target para respawn
    public GameObject playerRoot;            // XR Origin o PlayerCapsule
    public float respawnDelay = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;          // audio source on this trigger or global
    public AudioClip respawnClip;

    [Header("Optional")]
    public bool fadeScreen = true;
    public float fadeTime = 0.3f;            // si usas un canvas fade

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Suponemos que el "player" lleva tag "Player" o usa playerRoot referencia
        if (playerRoot != null)
        {
            if (other.transform.IsChildOf(playerRoot.transform) || other.transform == playerRoot.transform)
            {
                StartCoroutine(DoRespawn());
            }
        }
        else
        {
            // fallback por tag
            if (other.CompareTag("Player"))
                StartCoroutine(DoRespawn());
        }
    }

    private IEnumerator DoRespawn()
    {
        // sonido
        if (audioSource != null && respawnClip != null)
            audioSource.PlayOneShot(respawnClip);

        // pantalla fade (si tienes sistema)
        if (fadeScreen)
        {
            // llama a tu UIManager para fade si lo tienes. Ejemplo:
            UIManager.instance?.ShowLiveMessage("Respawning...");
        }

        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null && playerRoot != null)
        {
            // Reinicio simple: mover el root del jugador
            playerRoot.transform.position = respawnPoint.position;
            playerRoot.transform.rotation = respawnPoint.rotation;

            // Si XR Rig usa CharacterController, resetear velocity si aplica
            var cc = playerRoot.GetComponent<CharacterController>();
            if (cc != null)
            {
                // reposition using Move to avoid weird collisions
                cc.enabled = false;
                playerRoot.transform.position = respawnPoint.position;
                cc.enabled = true;
            }

            UIManager.instance?.ShowMessage("Respawned!");
        }
    }
}
