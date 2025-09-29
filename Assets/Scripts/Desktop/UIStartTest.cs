using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartTest : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("✅ Button clicked!");
        SceneManager.LoadScene("infierno");
    }
}
