using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages a "fake" POV switch by moving the player's XR Rig to an NPC's viewpoint.
/// This avoids the performance cost of a second camera.
/// </summary>
public class XRNPCPovSwitcher : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("The root of the player's XR Rig to be moved.")]
    public Transform playerXrRig;

    [Tooltip("The target transform on the NPC to align the camera with (e.g., the NPC's head or a 'Camera' child object).")]
    public Transform npcViewpoint;

    [Header("Input")]
    [Tooltip("The Input Action to trigger the POV switch.")]
    public InputActionReference togglePovAction;

    [Header("Player Control Scripts")]
    [Tooltip("Drag all player movement/input scripts here to disable them during the POV switch (e.g., CharacterControllerDriver, movement scripts).")]
    public MonoBehaviour[] playerControlScripts;

    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;
    private Transform originalPlayerParent;

    private bool isSwitchedToNpc = false;

    private void OnEnable()
    {
        if (togglePovAction != null)
        {
            togglePovAction.action.performed += OnTogglePov;
        }
    }

    private void OnDisable()
    {
        if (togglePovAction != null)
        {
            togglePovAction.action.performed -= OnTogglePov;
        }

        // Failsafe: If this object is disabled while switched, switch back.
        if (isSwitchedToNpc)
        {
            SwitchBackToPlayer();
        }
    }

    private void OnTogglePov(InputAction.CallbackContext context)
    {
        if (playerXrRig == null || npcViewpoint == null)
        {
            Debug.LogError("Player XR Rig or NPC Viewpoint is not assigned!", this);
            return;
        }

        if (!isSwitchedToNpc)
        {
            SwitchToNpc();
        }
        else
        {
            SwitchBackToPlayer();
        }
    }

    private void SwitchToNpc()
    {
        // 1. Store original state
        originalPlayerPosition = playerXrRig.position;
        originalPlayerRotation = playerXrRig.rotation;
        originalPlayerParent = playerXrRig.parent;

        // 2. Disable player controls
        SetPlayerControls(false);

        // 3. Move XR Rig to NPC viewpoint
        playerXrRig.SetParent(npcViewpoint, worldPositionStays: false);
        // The above line does the same as the two below, but is cleaner:
        // playerXrRig.parent = npcViewpoint;
        // playerXrRig.localPosition = Vector3.zero;
        // playerXrRig.localRotation = Quaternion.identity;

        isSwitchedToNpc = true;
    }

    private void SwitchBackToPlayer()
    {
        // 1. Restore original parent and transform
        playerXrRig.SetParent(originalPlayerParent, worldPositionStays: true);
        playerXrRig.position = originalPlayerPosition;
        playerXrRig.rotation = originalPlayerRotation;

        // 2. Re-enable player controls
        SetPlayerControls(true);

        isSwitchedToNpc = false;
    }

    private void SetPlayerControls(bool enabled)
    {
        foreach (var script in playerControlScripts)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
    }
}
