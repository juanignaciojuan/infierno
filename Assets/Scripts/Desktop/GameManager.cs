using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isPaused = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
            UIManager.instance?.ShowMessage("Game Paused");
        else
            UIManager.instance?.ClearMessage();
    }
}
