using UnityEngine;

/// <summary>
/// Simple proxy that exposes Play() and Play(float amplitude, float duration) methods
/// so UnityEvents can trigger controller haptics even if the underlying component
/// doesn't provide public methods in the event dropdown.
///
/// Attach this to your Left/Right Controller GameObject and wire it to whichever
/// haptic component you use (e.g., Haptic Impulse Player) via Inspector.
/// </summary>
[DisallowMultipleComponent]
public class XRHapticImpulseProxy : MonoBehaviour
{
    [Header("Target Haptic Component")]
    [Tooltip("Any component that implements vibration. We'll try common method names via SendMessage.")]
    public Component target;

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultAmplitude = 0.2f;
    public float defaultDuration = 0.05f;
    public float defaultFrequency = 0f;

    // No-arg play for events that don't carry intensity
    public void Play()
    {
        if (!Application.isPlaying) return; // avoid editor noise
        if (target == null) return;
        if (TryInvoke(target, "Play")) return;
        if (TryInvoke(target, "Trigger")) return;
        if (TryInvoke(target, "Send", defaultAmplitude, defaultDuration, defaultFrequency)) return;
        TryInvoke(target, "Send", defaultAmplitude, defaultDuration);
    }

    // Play with amplitude
    public void PlayAmplitude(float amplitude)
    {
        if (!Application.isPlaying) return;
        if (target == null)
        {
            Play();
            return;
        }
        float amp = Mathf.Clamp01(amplitude);
        if (TryInvoke(target, "Trigger", amp)) return;
        if (TryInvoke(target, "Play", amp)) return;
        if (TryInvoke(target, "Send", amp, defaultDuration, defaultFrequency)) return;
        if (TryInvoke(target, "Send", amp, defaultDuration)) return;
        Play();
    }

    // Play with amplitude & duration
    public void PlayAD(float amplitude, float duration)
    {
        if (!Application.isPlaying) return;
        if (target == null)
        {
            PlayAmplitude(amplitude);
            return;
        }
        float amp = Mathf.Clamp01(amplitude);
        float dur = Mathf.Max(0f, duration);
        if (TryInvoke(target, "Send", amp, dur, defaultFrequency)) return;
        if (TryInvoke(target, "Send", amp, dur)) return;
        PlayAmplitude(amp);
    }

    private bool TryInvoke(Component comp, string method, params object[] args)
    {
        if (comp == null) return false;
        var m = comp.GetType().GetMethod(method, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (m == null) return false;
        try
        {
            m.Invoke(comp, args);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
