using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[DisallowMultipleComponent]
public class NPCInteractable : InteractableBase
{
    [Header("Dialogue Settings")]
    [TextArea] public string[] dialogueLines;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Buttons (optional)")]
    public Button option1Button;
    public Button option2Button;
    public string option1Label = "Option 1";
    public string option2Label = "Option 2";

    [Header("Player control (drag your FirstPersonController or any script that can be toggled)")]
    public MonoBehaviour playerControllerToDisable;

    [Header("Audio (optional)")]
    public AudioClip humanoidSpeechClip;
    public AudioClip responseClip;
    private int currentIndex = 0;
    private bool dialogueActive = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public override void Interact()
    {
        if (dialoguePanel == null || dialogueText == null)
        {
            UIManager.instance?.ShowMessage("This NPC is silent.");
            return;
        }

        if (!dialogueActive)
        {
            dialogueActive = true;
            dialoguePanel.SetActive(true);
            currentIndex = 0;

            // lock player movement & show cursor
            if (playerControllerToDisable != null) playerControllerToDisable.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Play humanoid speech if present
            if (humanoidSpeechClip != null) audioSource.PlayOneShot(humanoidSpeechClip);

            // set button labels and listeners
            if (option1Button != null)
            {
                option1Button.onClick.RemoveAllListeners();
                option1Button.onClick.AddListener(() => OptionSelected(option1Label));
            }
            if (option2Button != null)
            {
                option2Button.onClick.RemoveAllListeners();
                option2Button.onClick.AddListener(() => OptionSelected(option2Label));
            }

            ShowNextLine();
        }
        else
        {
            // advance
            ShowNextLine();
        }
    }

    private void ShowNextLine()
    {
        if (currentIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentIndex];
            currentIndex++;
        }
        else
        {
            CloseDialogue();
        }
    }

    private void OptionSelected(string choice)
    {
        Debug.Log($"NPC: Player chose {choice}");
        if (responseClip != null) audioSource.PlayOneShot(responseClip);
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        dialogueActive = false;
        currentIndex = 0;

        if (playerControllerToDisable != null) playerControllerToDisable.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void ShowHover()
    {
        UIManager.instance?.ShowInteractHint("Talk (E)");
        isHovering = true;
    }
}
