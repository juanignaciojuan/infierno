using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuHandler : MonoBehaviour
{
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Pause.performed += ctx => OnPause();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void OnPause()
    {
        Debug.Log("Pause pressed, returning to StartMenu");
        SceneManager.LoadScene("StartMenu");
    }
}
