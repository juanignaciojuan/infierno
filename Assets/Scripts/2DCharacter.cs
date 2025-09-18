using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.Audio;

public class NPC2DCharacter : MonoBehaviour
{
    [Header("Billboard Settings")]
    private Transform playerCamera;

    [Header("Dialogue Settings")]
    public GameObject dialogueUI;       // UI panel for dialogue
    public TMP_Text dialogueText;       // Text component inside panel
    [TextArea] public string npcDialogue = "Hello traveler, welcome to my world!";
    private bool playerInRange = false;

    [Header("Interaction Prompt")]
    public GameObject promptUI;         // UI panel or text for "Press E"
    public TMP_Text promptText;         // TMP text for the prompt
    public string defaultPrompt = "Press E to interact";

    [Header("Audio Settings")]
    public AudioSource audioSource;     // Shared audio source on the NPC
    public AudioClip dialogueSound;     // Played when opening dialogue
    public AudioClip responseSound;     // Played when closing dialogue

    void Start()
    {
        playerCamera = Camera.main.transform;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (promptUI != null)
        {
            promptUI.SetActive(false); 
            if (promptText != null)
                promptText.text = defaultPrompt;
        }
    }

    void LateUpdate()
    {
        // Billboard effect (always face camera)
        Vector3 lookPos = playerCamera.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        // Interaction
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDialogue();
        }
    }

    private void ToggleDialogue()
    {
        if (dialogueUI == null || dialogueText == null) return;

        if (dialogueUI.activeSelf)
        {
            // Closing dialogue
            dialogueUI.SetActive(false);
            if (audioSource != null && responseSound != null)
                audioSource.PlayOneShot(responseSound);
        }
        else
        {
            // Opening dialogue
            dialogueUI.SetActive(true);
            dialogueText.text = npcDialogue;

            if (audioSource != null && dialogueSound != null)
                audioSource.PlayOneShot(dialogueSound);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
            if (dialogueUI != null) dialogueUI.SetActive(false);
        }
    }
}
