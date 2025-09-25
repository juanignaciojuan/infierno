using UnityEngine;  // Required for MonoBehaviour, [Header], etc.

public class GenericInteractable : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string hoverMessage = "Interact";

    // Example fields
    public bool isLocked = false;
    public float interactDistance = 2f;

    private void Start()
    {
        // Initialization
    }

    private void Update()
    {
        // Example update logic
    }

    public void ShowHoverMessage()
    {
        UIManager.instance?.ShowInteractHint(hoverMessage);
    }

    public void HideHoverMessage()
    {
        UIManager.instance?.HideInteractHint();
    }
}
