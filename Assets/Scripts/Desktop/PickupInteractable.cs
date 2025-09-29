using UnityEngine;

[DisallowMultipleComponent]
public class PickupInteractable : InteractableBase
{
    [Header("Pickup Settings")]
    public string pickupMessage = "You picked up an item.";

    public override void Interact()
    {
        if (isCollected) return;
        if (isLocked)
        {
            UIManager.instance?.ShowInteractHint("Locked");
            return;
        }

        // Use base Pickup logic
        base.Pickup();

        // Extra message override
        UIManager.instance?.ShowMessage(pickupMessage);
    }

    public override void ShowHover()
    {
        if (isCollected) return;
        UIManager.instance?.ShowInteractHint(hoverMessage != "" ? hoverMessage : "Pick up (E)");
        isHovering = true;
    }
}
