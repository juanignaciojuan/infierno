using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueManager : MonoBehaviour
{
    [Header("Audio / Subtitles")]
    public List<AudioClip> dialogueClips = new List<AudioClip>();
    public List<string> subtitles = new List<string>();

    [Header("AudioSource")]
    public AudioSource audioSource;

    [Header("Options")]
    public float subtitleDurationOverride = 0f;

    private List<int> unusedIndices = new List<int>();
    private bool playerNear = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            Debug.LogWarning("[PlayerDialogueManager] No AudioSource assigned.");

        ResetPool();
    }

    public void SetPlayerNear(bool state)
    {
        playerNear = state;
    }

    public void PlayRandomDialogue()
    {
        if (!playerNear) return;

        if (audioSource == null || dialogueClips.Count == 0)
        {
            Debug.LogWarning("[PlayerDialogueManager] No AudioSource or clips set.");
            return;
        }

        if (unusedIndices.Count == 0)
            ResetPool();

        int pickIndex = unusedIndices[Random.Range(0, unusedIndices.Count)];
        unusedIndices.Remove(pickIndex);

        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.clip = dialogueClips[pickIndex];
        audioSource.Play();

        if (UIManager.instance != null)
        {
            string text = (subtitles != null && pickIndex < subtitles.Count) ? subtitles[pickIndex] : "";
            UIManager.instance.ShowMessage(text);
        }

        Debug.Log($"[PlayerDialogueManager] Playing clip index {pickIndex}: {audioSource.clip?.name}");
    }

    private void ResetPool()
    {
        unusedIndices.Clear();
        for (int i = 0; i < dialogueClips.Count; i++)
            unusedIndices.Add(i);
    }
}
