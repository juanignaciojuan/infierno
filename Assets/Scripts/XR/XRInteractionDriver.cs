using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class XRInteractionDriver : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable xrInteractable;
    private XRInteractableBase interactable;

    private void Awake()
    {
        xrInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable = GetComponent<XRInteractableBase>();
    }

    private void OnEnable()
    {
        xrInteractable.selectEntered.AddListener(_ => interactable?.Interact());
        xrInteractable.hoverEntered.AddListener(_ => interactable?.ShowHover());
        xrInteractable.hoverExited.AddListener(_ => interactable?.HideHover());
    }

    private void OnDisable()
    {
        xrInteractable.selectEntered.RemoveAllListeners();
        xrInteractable.hoverEntered.RemoveAllListeners();
        xrInteractable.hoverExited.RemoveAllListeners();
    }
}
