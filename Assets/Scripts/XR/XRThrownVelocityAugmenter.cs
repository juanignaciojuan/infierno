using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Adds player locomotion velocity to a grabbed object's release velocity so it doesn't lag behind while the player is running.
/// Attach to grenades or other thrown items with XRGrabInteractable.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class XRThrownVelocityAugmenter : MonoBehaviour
{
    public XRPlayerVelocityProvider velocityProvider;
    [Tooltip("Multiplier for adding player velocity on release.")]
    public float velocityScale = 1.0f;

    private XRGrabInteractable grab;
    private Rigidbody rb;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (grab != null)
        {
            grab.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grab != null)
        {
            grab.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (velocityProvider == null)
        {
            velocityProvider = Object.FindFirstObjectByType<XRPlayerVelocityProvider>();
        }
        if (velocityProvider != null && rb != null)
        {
            rb.linearVelocity += velocityProvider.Velocity * velocityScale;
        }
    }
}
