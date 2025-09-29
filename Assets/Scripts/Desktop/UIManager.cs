using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Message Panel")]
    public GameObject messagePanel;      // Panel containing background + text
    public TMP_Text messageText;
    public Image messageBackground;
    public float messageDuration = 2f;

    [Header("Interact Panel")]
    public GameObject interactPanel;     // Panel showing "Press E" hints
    public TMP_Text interactText;

    private Coroutine messageCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;

        HideAllUI();
    }

    #region Messages
    public void ShowMessage(string message)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        if (messagePanel != null)
            messagePanel.SetActive(true);

        if (messageText != null)
            messageText.text = message;

        yield return new WaitForSeconds(messageDuration);

        if (messageText != null)
            messageText.text = "";

        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    public void ShowLiveMessage(string message)
    {
        if (messagePanel != null)
            messagePanel.SetActive(true);

        if (messageText != null)
            messageText.text = message;
    }

    public void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";

        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
    #endregion

    #region Interact Hints
    public void ShowInteractHint(string hint = "Press E")
    {
        if (interactPanel != null)
            interactPanel.SetActive(true);

        if (interactText != null)
            interactText.text = hint;
    }

    public void HideInteractHint()
    {
        if (interactPanel != null)
            interactPanel.SetActive(false);
    }
    #endregion

    #region Utility
    public void HideAllUI()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);

        if (interactPanel != null)
            interactPanel.SetActive(false);
    }
    #endregion
}
