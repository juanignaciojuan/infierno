using UnityEngine;
using UnityEngine.InputSystem;

public class HumanoidPOV : MonoBehaviour
{
    public Camera humanoidCamera;
    public Camera playerCamera;

    [Header("Input")]
    public InputActionReference switchAction; // Asignar Q en PC, botón en Quest

    void Update()
    {
        bool switchHeld = switchAction != null && switchAction.action.IsPressed();

        if (switchHeld)
        {
            humanoidCamera.enabled = true;
            playerCamera.enabled = false;
        }
        else
        {
            humanoidCamera.enabled = false;
            playerCamera.enabled = true;
        }
    }
}
