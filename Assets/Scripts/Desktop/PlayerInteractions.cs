using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[DisallowMultipleComponent]
public class PlayerInteractions : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public CharacterController characterController;
    public AudioSource playerAudioSource; // optional: assign a source on the player for SFX

    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer = ~0;

    [Header("Runtime (read-only)")]
    [Tooltip("Populated at runtime by raycast. Leave empty in inspector.")]
    public InteractableBase currentInteractable;

    [Header("Input Actions (assign from your InputActions asset)")]
    public InputActionReference interactAction;   // E (or Player/Interact)
    public InputActionReference openDoorAction;   // left-click / OpenDoor
    public InputActionReference escAction;        // Escape
    public InputActionReference crawlAction;      // Crouch (checked in Update)
    public InputActionReference randomTalkAction; // T
    public InputActionReference npcPOVAction;     // Q (hold)

    [Header("NPC POV")]
    public Camera npcCamera; // optional, assign NPC camera to look through while Q is held

    [Header("Crawl Settings")]
    public float normalHeight = 1.8f;
    public float crawlHeight = 0.6f;
    public float smoothSpeed = 8f;

    [Header("Audio Clips")]
    public AudioClip randomTalkClip;
    public AudioClip escClip;

    private bool isEscShowing = false;

    private void OnEnable()
    {
        if (interactAction != null) interactAction.action.performed += OnInteract;
        if (openDoorAction != null) openDoorAction.action.performed += OnOpenDoor;
        if (escAction != null) escAction.action.performed += OnEsc;
        if (randomTalkAction != null) randomTalkAction.action.performed += OnRandomTalk;

        if (npcPOVAction != null)
        {
            npcPOVAction.action.started += OnNpcPOVStarted;
            npcPOVAction.action.canceled += OnNpcPOVCanceled;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.action.performed -= OnInteract;
        if (openDoorAction != null) openDoorAction.action.performed -= OnOpenDoor;
        if (escAction != null) escAction.action.performed -= OnEsc;
        if (randomTalkAction != null) randomTalkAction.action.performed -= OnRandomTalk;

        if (npcPOVAction != null)
        {
            npcPOVAction.action.started -= OnNpcPOVStarted;
            npcPOVAction.action.canceled -= OnNpcPOVCanceled;
        }
    }

    private void Update()
    {
        DoHoverRaycast();
        HandleCrawl();
    }

    private void DoHoverRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Collide))
        {
            var hitInteract = hit.collider.GetComponentInParent<InteractableBase>();
            if (hitInteract != null)
            {
                if (currentInteractable != hitInteract)
                {
                    currentInteractable?.HideHover();
                    currentInteractable = hitInteract;
                    currentInteractable.ShowHover();
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.HideHover();
            currentInteractable = null;
        }
    }

    // E key and other "Interact" action: interact with non-door objects (NPC, pickups)
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (currentInteractable == null) return;
        // prevent E from opening doors (doors must be left-click)
        if (currentInteractable.CompareTag("Door")) return;
        currentInteractable.Interact();
    }

    // Left click / OpenDoor action: only affect Doors
    private void OnOpenDoor(InputAction.CallbackContext ctx)
    {
        if (currentInteractable == null) return;
        if (currentInteractable.CompareTag("Door"))
            currentInteractable.Interact();
    }

    // Random talk (T)
    private void OnRandomTalk(InputAction.CallbackContext ctx)
    {
        if (playerAudioSource != null && randomTalkClip != null)
            playerAudioSource.PlayOneShot(randomTalkClip);

        UIManager.instance?.ShowMessage("Random talk!");
    }

    // ESC
    private void OnEsc(InputAction.CallbackContext ctx)
    {
        if (isEscShowing) return;
        if (playerAudioSource != null && escClip != null)
            playerAudioSource.PlayOneShot(escClip);

        UIManager.instance?.ShowMessage("You can't escape from this world!");
        StartCoroutine(EscCooldown());
    }

    private IEnumerator EscCooldown()
    {
        isEscShowing = true;
        yield return new WaitForSeconds(2f);
        isEscShowing = false;
    }

    // NPC POV hold (Q): started = enable npc camera, canceled = revert
    private void OnNpcPOVStarted(InputAction.CallbackContext ctx)
    {
        if (npcCamera == null || playerCamera == null) return;
        npcCamera.enabled = true;
        playerCamera.enabled = false;
    }
    private void OnNpcPOVCanceled(InputAction.CallbackContext ctx)
    {
        if (npcCamera == null || playerCamera == null) return;
        npcCamera.enabled = false;
        playerCamera.enabled = true;
    }

    // Crawl: smoothing done in Update by querying the action's state
    private void HandleCrawl()
    {
        if (crawlAction == null || playerCamera == null) return;

        bool isCrawling = crawlAction.action.IsPressed();
        float targetY = isCrawling ? crawlHeight : normalHeight;

        Vector3 localPos = playerCamera.transform.localPosition;
        localPos.y = Mathf.Lerp(localPos.y, targetY, Time.deltaTime * smoothSpeed);
        playerCamera.transform.localPosition = localPos;

        if (characterController != null)
        {
            float targetHeight = isCrawling ? 0.9f : 1.8f;
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * smoothSpeed);
            Vector3 center = characterController.center;
            center.y = characterController.height / 2f;
            characterController.center = center;
        }
    }
}
