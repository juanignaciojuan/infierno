using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerDialogueManager : MonoBehaviour
{
    [Header("Audio / Subtitles")]
    [Tooltip("Clips de diálogo a reproducir aleatoriamente sin repetirse hasta agotar.")]
    public List<AudioClip> dialogueClips = new List<AudioClip>();
    [Tooltip("Subtítulos asociados a cada clip (mismo tamaño que dialogueClips).")]
    public List<string> subtitles = new List<string>();

    [Header("AudioSource")]
    public AudioSource audioSource;

    [Header("Opciones")]
    public bool allowKeyHold = false; 
    public float subtitleDurationOverride = 0f;

    // Pool de índices para no repetir
    private List<int> unusedIndices = new List<int>();

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            Debug.LogWarning("[PlayerDialogueManager] No AudioSource asignado.");

        ResetPool();
    }

    private void Update()
    {
        bool pressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            pressed = allowKeyHold ? Keyboard.current.tKey.isPressed : Keyboard.current.tKey.wasPressedThisFrame;
#endif
        if (!pressed)
            pressed = allowKeyHold ? Input.GetKey(KeyCode.T) : Input.GetKeyDown(KeyCode.T);

        if (pressed)
        {
            Debug.Log("T press detected");
            PlayRandomDialogue();
        }
    }

    private void PlayRandomDialogue()
    {
        if (audioSource == null || dialogueClips.Count == 0)
        {
            Debug.LogWarning("[PlayerDialogueManager] No hay AudioSource o clips configurados.");
            return;
        }

        if (unusedIndices.Count == 0)
            ResetPool();

        // Elegir índice aleatorio del pool
        int pickIndex = unusedIndices[Random.Range(0, unusedIndices.Count)];
        unusedIndices.Remove(pickIndex);

        // Forzar el nuevo audio
        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.clip = dialogueClips[pickIndex];
        audioSource.Play();

        // Mostrar subtítulo
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

        Debug.Log("[PlayerDialogueManager] Pool reseteado.");
    }
}
