using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Plays a list of audio clips in sequence and loops back to the beginning.
/// Requires an AudioSource component on the same GameObject.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioPlaylistPlayer : MonoBehaviour
{
    [Tooltip("The list of audio clips to be played in order.")]
    [SerializeField]
    private List<AudioClip> playlist = new List<AudioClip>();

    private AudioSource audioSource;
    private int currentTrackIndex = 0;

    void Awake()
    {
        // Get the AudioSource component attached to this GameObject.
        // The [RequireComponent] attribute ensures it will always exist.
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Configure the AudioSource to not loop individual tracks, as we are handling the playlist loop.
        audioSource.loop = false;

        // Start playing the first track if the playlist is not empty.
        if (playlist.Count > 0)
        {
            PlayTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        // Check if the playlist has tracks and if the current track has finished playing.
        if (playlist.Count > 0 && !audioSource.isPlaying)
        {
            // The current song has ended, so advance to the next one.
            PlayNextTrack();
        }
    }

    /// <summary>
    /// Plays the next track in the playlist, looping to the start if necessary.
    /// </summary>
    private void PlayNextTrack()
    {
        // Increment the track index.
        currentTrackIndex++;

        // If we've gone past the end of the playlist, loop back to the first track.
        if (currentTrackIndex >= playlist.Count)
        {
            currentTrackIndex = 0;
        }

        // Play the new track.
        PlayTrack(currentTrackIndex);
    }

    /// <summary>
    /// Plays a specific track from the playlist by its index.
    /// </summary>
    /// <param name="trackIndex">The index of the track to play.</param>
    private void PlayTrack(int trackIndex)
    {
        // Ensure the index is valid before trying to play.
        if (trackIndex >= 0 && trackIndex < playlist.Count)
        {
            audioSource.clip = playlist[trackIndex];
            audioSource.Play();
        }
    }
}
