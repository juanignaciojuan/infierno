using UnityEngine;

public class TerritoryZone : MonoBehaviour
{
    [Header("Mensajes")]
    [TextArea] public string enterMessage = "Entraste a una nueva zona.";
    [TextArea] public string exitMessage = "Saliste de la zona.";
    [Tooltip("Duración en segundos del mensaje en pantalla")]
    public float messageDuration = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && UIManager.instance != null)
        {
            UIManager.instance.ShowMessage(enterMessage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && UIManager.instance != null)
        {
            UIManager.instance.ShowMessage(exitMessage);
        }
    }
}
