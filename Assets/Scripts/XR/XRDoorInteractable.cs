using UnityEngine;

public class XRDoorInteractable : XRInteractableBase
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float closeAngle = 0f;
    public float rotationSpeed = 5f;

    private bool isOpen = false;
    private Quaternion targetRotation;
    private float initialY;

    private void Start()
    {
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
            XRUIManager.Instance?.ShowInteractHint("Door is locked");
            return;
        }

        isOpen = !isOpen;
        float y = initialY + (isOpen ? openAngle : closeAngle);
        targetRotation = Quaternion.Euler(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }

    public override void ShowHover()
    {
        if (isLocked)
            XRUIManager.Instance?.ShowInteractHint("Door is locked");
        else
            XRUIManager.Instance?.ShowInteractHint(isOpen ? "Close" : "Open");
    }
}
