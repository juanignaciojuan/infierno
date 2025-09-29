using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InteractableBase : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string hoverMessage = "Press E";
    public bool isLocked = false;

    [Header("Optional Pickup")]
    public bool isPickup = false;
    public AudioClip pickupSound;
    public Image pickupDisplay; // optional UI image to toggle on pickup

    protected bool isHovering = false;
    protected bool isCollected = false;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Called by the player to perform the interaction. Subclasses should override when needed.
    /// Default behavior: if pickup -> pick it up; if locked -> show locked hint; else show a default message.
    /// </summary>
    public virtual void Interact()
    {
        if (isLocked)
        {
            UIManager.instance?.ShowInteractHint("Locked");
            return;
        }

        if (isPickup && !isCollected)
        {
            Pickup();
            return;
        }

        // Default fallback
        UIManager.instance?.ShowMessage("Nothing happens.");
    }

    protected void Pickup()
    {
        isCollected = true;

        if (pickupSound != null) audioSource.PlayOneShot(pickupSound);
        if (pickupDisplay != null) pickupDisplay.gameObject.SetActive(true);

        UIManager.instance?.ShowMessage("Item picked up!");
        gameObject.SetActive(false);
    }

    public virtual void ShowHover()
    {
        UIManager.instance?.ShowInteractHint(hoverMessage);
        isHovering = true;
    }

    public virtual void HideHover()
    {
        UIManager.instance?.HideInteractHint();
        isHovering = false;
    }
}
