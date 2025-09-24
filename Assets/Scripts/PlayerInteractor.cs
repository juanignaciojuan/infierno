using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    public float interactDistance = 3f;
    public LayerMask interactLayer = ~0;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerDialogueManager dialogueManager;

    [Header("Input Actions")]
    public InputActionReference primaryInteract;   // trigger
    public InputActionReference secondaryInteract; // A/X
    public InputActionReference randomTalk;        // Y/B

    private InteractableBase currentInteractable;

    private void OnEnable()
    {
        primaryInteract.action.performed += OnPrimaryInteract;
        secondaryInteract.action.performed += OnSecondaryInteract;
        randomTalk.action.performed += OnRandomTalk;
    }

    private void OnDisable()
    {
        primaryInteract.action.performed -= OnPrimaryInteract;
        secondaryInteract.action.performed -= OnSecondaryInteract;
        randomTalk.action.performed -= OnRandomTalk;
    }

    private void Update()
    {
        DoHoverRaycast();
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

        // Nothing hit
        if (currentInteractable != null)
        {
            currentInteractable.HideHover();
            currentInteractable = null;
        }
    }

    private void OnPrimaryInteract(InputAction.CallbackContext ctx)
    {
        if (currentInteractable == null) return;
        if (currentInteractable.interactionMode == InteractableBase.InteractionMode.LeftClick ||
            currentInteractable.interactionMode == InteractableBase.InteractionMode.Both)
            currentInteractable.Interact();
    }

    private void OnSecondaryInteract(InputAction.CallbackContext ctx)
    {
        if (currentInteractable == null) return;
        if (currentInteractable.interactionMode == InteractableBase.InteractionMode.EKey ||
            currentInteractable.interactionMode == InteractableBase.InteractionMode.Both)
            currentInteractable.Interact();
    }

    private void OnRandomTalk(InputAction.CallbackContext ctx)
    {
        dialogueManager?.PlayRandomDialogue();
    }
}