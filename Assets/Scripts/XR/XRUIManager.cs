using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class XRUIManager : MonoBehaviour
{
    public static XRUIManager Instance;

    [Header("Message Panel")]
    public GameObject messagePanel;
    public TMP_Text messageText;
    public float messageDuration = 2f;

    [Header("Interact Hint Panel")]
    public GameObject interactPanel;
    public TMP_Text interactText;

    private Coroutine messageCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        HideAllUI();
    }

    public void ShowMessage(string message)
    {
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        if (messagePanel != null) messagePanel.SetActive(true);
        if (messageText != null) messageText.text = message;

        yield return new WaitForSeconds(messageDuration);

        if (messageText != null) messageText.text = "";
        if (messagePanel != null) messagePanel.SetActive(false);
    }

    public void ShowInteractHint(string hint)
    {
        if (interactPanel != null) interactPanel.SetActive(true);
        if (interactText != null) interactText.text = hint;
    }

    public void HideInteractHint()
    {
        if (interactPanel != null) interactPanel.SetActive(false);
    }

    public void HideAllUI()
    {
        if (messagePanel != null) messagePanel.SetActive(false);
        if (interactPanel != null) interactPanel.SetActive(false);
    }
}
