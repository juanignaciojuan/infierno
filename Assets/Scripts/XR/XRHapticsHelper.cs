using UnityEngine;

/// <summary>
/// Utility for simple controller haptic pulses. Call Pulse(interactor, amplitude, duration).
/// PulseAll will try common interactors in scene.
/// </summary>
[System.Obsolete("XRHapticsHelper is deprecated. Use UnityEvents wired to Haptic Impulse Player components instead.")]
public static class XRHapticsHelper
{
    public static void Pulse(object interactor, float amplitude, float duration) { /* no-op */ }
    public static void PulseAll(float amplitude, float duration) { /* no-op */ }
}
