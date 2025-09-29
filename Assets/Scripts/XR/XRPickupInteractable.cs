using UnityEngine;

public class XRPickupInteractable : XRInteractableBase
{
    public string pickupMessage = "You picked up an item.";

    public override void Interact()
    {
        if (isCollected) return;
        base.Interact();
        XRUIManager.Instance?.ShowMessage(pickupMessage);
    }
}
