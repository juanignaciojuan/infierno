using UnityEngine;

public class DoorInteractable : InteractableBase
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float closeAngle = 0f;
    public float rotationSpeed = 5f;

    private bool isOpen = false;
    private Quaternion targetRotation;

    private void Awake()
    {
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public override void Interact()
    {
        if (isLocked)
        {
            UIManager.instance?.ShowInteractHint("Door is locked");
            return;
        }

        isOpen = !isOpen;
        float yRotation = isOpen ? openAngle : closeAngle;
        targetRotation = Quaternion.Euler(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
    }

    public override void ShowHover()
    {
        if (UIManager.instance == null) return;
        if (isLocked) UIManager.instance.ShowInteractHint("Door is locked");
        else UIManager.instance.ShowInteractHint(isOpen ? "Close door" : "Open door");
        isHovering = true;
    }
}
