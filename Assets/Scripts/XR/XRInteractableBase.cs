using UnityEngine;

[DisallowMultipleComponent]
public class XRInteractableBase : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string hoverMessage = "Interact";
    public bool isLocked = false;

    [Header("Optional Pickup")]
    public bool isPickup = false;
    public AudioClip pickupSound;

    protected bool isCollected = false;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public virtual void Interact()
    {
        if (isLocked)
        {
            XRUIManager.Instance?.ShowInteractHint("Locked");
            return;
        }

        if (isPickup && !isCollected)
        {
            Pickup();
            return;
        }

        XRUIManager.Instance?.ShowMessage("Nothing happens.");
    }

    protected void Pickup()
    {
        isCollected = true;
        if (pickupSound != null) audioSource.PlayOneShot(pickupSound);
        XRUIManager.Instance?.ShowMessage("Item picked up!");
        gameObject.SetActive(false);
    }

    public virtual void ShowHover()
    {
        XRUIManager.Instance?.ShowInteractHint(hoverMessage);
    }

    public virtual void HideHover()
    {
        XRUIManager.Instance?.HideInteractHint();
    }
}
