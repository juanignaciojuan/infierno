using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DebugSelectLogger : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEnter);
        interactable.selectExited.AddListener(OnSelectExit);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEnter);
        interactable.selectExited.RemoveListener(OnSelectExit);
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log("SELECT ENTER by: " + args.interactorObject);
    }

    void OnSelectExit(SelectExitEventArgs args)
    {
        Debug.Log("SELECT EXIT by: " + args.interactorObject);
    }
}
