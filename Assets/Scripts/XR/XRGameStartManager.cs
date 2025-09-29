using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGameStartManager : MonoBehaviour
{
    [Header("References")]
    public GameObject startMenuCanvas;  // World-space canvas with Start button
    public GameObject xrOrigin;         // XR Origin prefab

    private bool hasStarted = false;

    void Start()
    {
        /*if (xrOrigin != null) xrOrigin.SetActive(false);*/
        if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
    }

    // Called by Button OnClick()
    public void StartGame()
    {
        if (hasStarted) return;

        if (startMenuCanvas != null) startMenuCanvas.SetActive(false);
        if (xrOrigin != null) xrOrigin.SetActive(true);

        hasStarted = true;
    }
}
