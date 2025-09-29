using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LandingManager : MonoBehaviour
{
    public Button continueButton;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(LoadMainScene);
        }
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("Infierno");
    }
}
