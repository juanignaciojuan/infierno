using UnityEngine;
using UnityEngine.InputSystem;

public class DebugClick : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Debug.Log("Mouse click detected by Input System");
    }
}
