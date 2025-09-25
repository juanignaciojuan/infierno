// InputActionsEnabler.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionsEnabler : MonoBehaviour
{
    public InputActionAsset actions; // arrastrar InputSystems_Actions aquí

    private void OnEnable()
    {
        if (actions != null) actions.Enable();
    }

    private void OnDisable()
    {
        if (actions != null) actions.Disable();
    }
}
