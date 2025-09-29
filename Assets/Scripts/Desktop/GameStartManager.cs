using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class GameStartManager : MonoBehaviour
{
    public Button startButton;
    public GameObject titleTextObject;
    public GameObject instructionsTextObject;
    public FirstPersonController playerController;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
            playerController.enabled = false;

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);  // ✅ re-enable this line
        }
    }

    public void StartGame()
    {
        Debug.Log("Start button clicked!");

        if (startButton != null)
            startButton.gameObject.SetActive(false);
        if (titleTextObject != null)
            titleTextObject.SetActive(false);
        if (instructionsTextObject != null)
            instructionsTextObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.enabled = true;

        // ✅ enable PlayerInteractions here
        var interactions = playerController.GetComponent<PlayerInteractions>();
        if (interactions != null)
            interactions.enabled = true;
    }
}
