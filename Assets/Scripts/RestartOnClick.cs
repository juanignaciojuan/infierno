using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class RestartOnClick : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float interactionDistance = 5f;

    [Header("Input")]
    public InputActionReference clickAction; // Asignar click izquierdo en PC, trigger en Quest

    private void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (playerCamera == null || clickAction == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject && clickAction.action.WasPerformedThisFrame())
            {
                Debug.Log("[RestartOnClick] Object clicked!");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
