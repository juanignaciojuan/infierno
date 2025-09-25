using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UI_PickupItem : MonoBehaviour
{
    public Image pickupDisplay;
    public AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip dropSound;

    [Header("Input")]
    public InputActionReference pickupAction; // Click (PC) o Trigger (Quest)

    private bool isPickedUp = false;
    private static bool anyPicked = false;

    private void OnEnable()
    {
        if (pickupAction != null)
            pickupAction.action.performed += OnPickupPressed;
    }

    private void OnDisable()
    {
        if (pickupAction != null)
            pickupAction.action.performed -= OnPickupPressed;
    }

    private void OnPickupPressed(InputAction.CallbackContext ctx)
    {
        if (anyPicked && !isPickedUp) return;

        if (!isPickedUp) Pickup();
        else Drop();
    }

    private void Pickup()
    {
        isPickedUp = true;
        anyPicked = true;
        pickupDisplay.gameObject.SetActive(true);
        if (audioSource && pickupSound) audioSource.PlayOneShot(pickupSound);
    }

    private void Drop()
    {
        isPickedUp = false;
        anyPicked = false;
        pickupDisplay.gameObject.SetActive(false);
        if (audioSource && dropSound) audioSource.PlayOneShot(dropSound);
    }
}
