using UnityEngine;

public class KeyPickup : InteractableBase
{
    [Header("Key Settings")]
    public string pickupMessage = "You picked up a key!";
    public AudioClip pickupSound;
    public DoorInteractable doorToUnlock; // assign in Inspector

    private AudioSource audioSource;
    private bool isCollected = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public override void Interact()
    {
        if (isCollected) return;
        isCollected = true;

        if (pickupSound != null) audioSource.PlayOneShot(pickupSound);
        UIManager.instance?.ShowMessage(pickupMessage);

        if (doorToUnlock != null) doorToUnlock.isLocked = false;

        gameObject.SetActive(false);
    }

    public override void ShowHover()
    {
        if (isCollected) return;
        UIManager.instance?.ShowInteractHint("Pick up key");
        isHovering = true;
    }
}
