using UnityEngine;

public class StartButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject player; // Should be your root player object

    public void StartGame()
    {
        if (startCanvas != null) startCanvas.SetActive(false);
        if (gameplayCanvas != null) gameplayCanvas.SetActive(true);
        if (player != null)
        {
            player.SetActive(true);
            Debug.Log("✅ Player activated.");
        }
        else
        {
            Debug.LogError("❌ Player reference is missing in StartButtonManager!");
        }
    }
}
