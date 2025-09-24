using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class EscMessage : MonoBehaviour
{
    [Header("UI & Sound")]
    public string messageToShow = "ESC/Menu pressed!";
    public AudioSource audioSource;
    public float displayTime = 3f;

    [Header("Input")]
    public InputActionReference escAction; // Asignar Escape en PC, Menu en Quest

    private bool isShowing = false;

    private void OnEnable()
    {
        if (escAction != null)
            escAction.action.performed += OnEscPressed;
    }

    private void OnDisable()
    {
        if (escAction != null)
            escAction.action.performed -= OnEscPressed;
    }

    private void OnEscPressed(InputAction.CallbackContext context)
    {
        if (!isShowing) TriggerMessage();
    }

    private void TriggerMessage()
    {
        if (UIManager.instance != null)
            UIManager.instance.ShowMessage(messageToShow);

        if (audioSource != null)
            audioSource.Play();

        StartCoroutine(MessageCooldown());
    }

    private IEnumerator MessageCooldown()
    {
        isShowing = true;
        yield return new WaitForSeconds(displayTime);
        isShowing = false;
    }
}
