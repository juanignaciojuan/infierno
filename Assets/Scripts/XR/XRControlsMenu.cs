using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Toggles a controls menu using the new Input System.
/// </summary>
public class XRControlsMenu : MonoBehaviour
{
    [Tooltip("The UI GameObject (Canvas or Panel) to toggle.")]
    public GameObject controlsMenuRoot;

    [Tooltip("Input Action to toggle the menu (e.g. Menu button).")]
    public InputActionProperty toggleAction;

    private void OnEnable()
    {
        // Use Started/Canceled for Hold-to-Activate
        toggleAction.action.started += OnPress;
        toggleAction.action.canceled += OnRelease;
        toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleAction.action.started -= OnPress;
        toggleAction.action.canceled -= OnRelease;
        toggleAction.action.Disable();
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
        SetMenu(true);
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        SetMenu(false);
    }

    public void SetMenu(bool active)
    {
        if (controlsMenuRoot == null) return;
        controlsMenuRoot.SetActive(active);
        // Position the menu as a child of the camera so it stays in front of the player
        if (active && Camera.main != null)
        {
            // Make sure the canvas is world-space. We'll parent it to the camera and place at a local offset.
            Transform cam = Camera.main.transform;

            // Cache the original parent so we can restore when closed
            if (controlsMenuRoot.transform.parent != cam)
            {
                controlsMenuRoot.transform.SetParent(cam, worldPositionStays: true);
            }

            // Use a comfortable fixed offset in front of the camera (customize in inspector directly on the object if needed)
            controlsMenuRoot.transform.localPosition = new Vector3(0f, -0.2f, 1.5f);
            controlsMenuRoot.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            // When closing, un-parent so the menu stays in the scene hierarchy as before
            if (!active && controlsMenuRoot.transform.parent != null)
            {
                controlsMenuRoot.transform.SetParent(null, worldPositionStays: true);
            }
        }
    }
}
