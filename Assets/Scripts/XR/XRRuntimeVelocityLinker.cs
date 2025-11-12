using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Automatically finds and links the scene's XRPlayerVelocityProvider
/// to an XRThrownVelocityAugmenter on a prefab at runtime.
/// This solves the issue of not being able to link scene objects to prefabs.
/// </summary>
[RequireComponent(typeof(XRThrownVelocityAugmenter))]
[RequireComponent(typeof(XRGrabInteractable))]
public class XRRuntimeVelocityLinker : MonoBehaviour
{
    private XRThrownVelocityAugmenter _velocityAugmenter;
    private XRGrabInteractable _grabInteractable;
    private bool _isLinked = false;

    // Cache a reference to the scene's provider to avoid repeated lookups.
    private static XRPlayerVelocityProvider _cachedProvider;

    private void Awake()
    {
        _velocityAugmenter = GetComponent<XRThrownVelocityAugmenter>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        _grabInteractable.selectEntered.AddListener(OnFirstGrab);
        // Reset link status if this is a pooled object being reused.
        _isLinked = false;
    }

    private void OnDisable()
    {
        _grabInteractable.selectEntered.RemoveListener(OnFirstGrab);
    }

    /// <summary>
    /// On the first grab, find the velocity provider in the scene and link it.
    /// </summary>
    private void OnFirstGrab(SelectEnterEventArgs args)
    {
        if (_isLinked || _velocityAugmenter == null)
        {
            return;
        }

        // If we already have a provider assigned in the inspector, do nothing.
        if (_velocityAugmenter.velocityProvider != null)
        {
            _isLinked = true;
            return;
        }

        // Find the provider in the scene.
        // Use a cached reference for performance so we only search once per scene.
        if (_cachedProvider == null)
        {
            _cachedProvider = FindFirstObjectByType<XRPlayerVelocityProvider>();
        }

        if (_cachedProvider != null)
        {
            // Link it!
            _velocityAugmenter.velocityProvider = _cachedProvider;
            _isLinked = true;
        }
        else
        {
            Debug.LogWarning("XRRuntimeVelocityLinker: Could not find an XRPlayerVelocityProvider in the scene to link.", this);
        }
    }
}
