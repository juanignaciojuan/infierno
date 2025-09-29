using UnityEngine;

[DisallowMultipleComponent]
public class DoorInteractable : InteractableBase
{
    [Header("Door Settings")]
    public float openAngle = 90f;    // angle relative to initial Y
    public float closeAngle = 0f;    // relative offset at closed state (usually 0)
    public float rotationSpeed = 5f;

    private bool isOpen = false;
    private Quaternion targetRotation;
    private float initialY;

    private void Awake()
    {
        base.Awake();
        initialY = transform.eulerAngles.y;
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
        float y = initialY + (isOpen ? openAngle : closeAngle);
        targetRotation = Quaternion.Euler(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }

    public override void ShowHover()
    {
        if (isLocked)
            UIManager.instance?.ShowInteractHint("Door is locked");
        else
            UIManager.instance?.ShowInteractHint(isOpen ? "Close (Left Click)" : "Open (Left Click)");
        isHovering = true;
    }
}
