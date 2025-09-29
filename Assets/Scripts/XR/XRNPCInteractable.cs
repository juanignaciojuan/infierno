using UnityEngine;

public class XRNPCInteractable : XRInteractableBase
{
    [TextArea] public string[] dialogueLines;

    private int currentIndex = 0;

    public override void Interact()
    {
        if (dialogueLines.Length == 0)
        {
            XRUIManager.Instance?.ShowMessage("This NPC is silent.");
            return;
        }

        if (currentIndex < dialogueLines.Length)
        {
            XRUIManager.Instance?.ShowMessage(dialogueLines[currentIndex]);
            currentIndex++;
        }
        else
        {
            XRUIManager.Instance?.ShowMessage("Dialogue ended.");
            currentIndex = 0;
        }
    }

    public override void ShowHover()
    {
        XRUIManager.Instance?.ShowInteractHint("Talk");
    }
}
