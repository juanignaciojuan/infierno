using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Plays random audio clips from lists when the X, Y, or B buttons are pressed on an Oculus Touch controller.
/// Requires the new Input System and an AudioSource component.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerVoiceController : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Reference to the Input Action for the X button press.")]
    public InputActionReference buttonXAction;

    [Tooltip("Reference to the Input Action for the Y button press.")]
    public InputActionReference buttonYAction;

    [Tooltip("Reference to the Input Action for the B button press.")]
    public InputActionReference buttonBAction;

    [Header("Audio Clips")]
    [Tooltip("Audio clips to be played when the X button is pressed.")]
    public AudioClip[] xButtonClips;

    [Tooltip("Audio clips to be played when the Y button is pressed.")]
    public AudioClip[] yButtonClips;

    [Tooltip("Audio clips to be played when the B button is pressed.")]
    public AudioClip[] bButtonClips;

    [Header("Audio Settings")]
    [Tooltip("Random pitch range (e.g., X=0.9, Y=1.1 for +/- 10%). Set X and Y to 1 for no variation.")]
    public Vector2 pitchRange = new Vector2(1f, 1f);

    private AudioSource audioSource;

    private const string VOICE_VOLUME_KEY = "PlayerVoiceVolume";

    private void Awake()
    {
        // Get the AudioSource component attached to this GameObject.
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 0.75f);
    }

    private void OnEnable()
    {
        // Register the button press events.
        if (buttonXAction != null) buttonXAction.action.performed += OnButtonXPressed;
        if (buttonYAction != null) buttonYAction.action.performed += OnButtonYPressed;
        if (buttonBAction != null) buttonBAction.action.performed += OnButtonBPressed;
    }

    private void OnDisable()
    {
        // Unregister the events to prevent memory leaks.
        if (buttonXAction != null) buttonXAction.action.performed -= OnButtonXPressed;
        if (buttonYAction != null) buttonYAction.action.performed -= OnButtonYPressed;
        if (buttonBAction != null) buttonBAction.action.performed -= OnButtonBPressed;
    }

    private void OnButtonXPressed(InputAction.CallbackContext context)
    {
        PlayRandomClip(xButtonClips);
    }

    private void OnButtonYPressed(InputAction.CallbackContext context)
    {
        PlayRandomClip(yButtonClips);
    }



    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        PlayRandomClip(bButtonClips);
    }

    /// <summary>
    /// Sets the volume of the voice audio source and saves it.
    /// </summary>
    /// <param name="volume">The volume level from 0.0 to 1.0.</param>
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
            PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, volume);
        }
    }

    /// <summary>
    /// Plays a random audio clip from the provided array, if the array is not empty.
    /// </summary>
    private void PlayRandomClip(AudioClip[] clips)
    {
        // Don't do anything if the audio source is already playing or the list is empty.
        if (audioSource.isPlaying || clips == null || clips.Length == 0)
        {
            return;
        }

        // Pick a random clip from the array.
        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clipToPlay = clips[randomIndex];

        // Play the chosen clip.
        if (clipToPlay != null)
        {
            // Apply a random pitch before playing.
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}
