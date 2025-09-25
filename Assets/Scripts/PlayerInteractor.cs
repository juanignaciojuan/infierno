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
    public InputActionReference primaryInteract;   // Click / Trigger
    public InputActionReference secondaryInteract; // E / A-X
    public InputActionReference talkAction;        // Y-B / Keyboard "Talk"

    private InteractableBase currentInteractable;

    private void OnEnable()
    {
        if (primaryInteract != null) primaryInteract.action.performed += OnPrimaryInteract;
        if (secondaryInteract != null) secondaryInteract.action.performed += OnSecondaryInteract;
        if (talkAction != null) talkAction.action.performed += OnTalk;
    }

    private void OnDisable()
    {
        if (primaryInteract != null) primaryInteract.action.performed -= OnPrimaryInteract;
        if (secondaryInteract != null) secondaryInteract.action.performed -= OnSecondaryInteract;
        if (talkAction != null) talkAction.action.performed -= OnTalk;
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

        if (currentInteractable != null)
        {
            currentInteractable.HideHover();
            currentInteractable = null;
        }
    }

    private void OnPrimaryInteract(InputAction.CallbackContext ctx)
    {
        currentInteractable?.Interact();
    }

    private void OnSecondaryInteract(InputAction.CallbackContext ctx)
    {
        currentInteractable?.Interact();
    }

    private void OnTalk(InputAction.CallbackContext ctx)
    {
        dialogueManager?.PlayRandomDialogue();
    }
}
