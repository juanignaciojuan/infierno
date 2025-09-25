using UnityEngine;

public class ProximityHint2D : MonoBehaviour
{
    [Header("Hint Settings")]
    public string hintMessage = "Press E to interact";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.instance.ShowInteractHint(hintMessage);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.instance.HideInteractHint();
        }
    }
}
