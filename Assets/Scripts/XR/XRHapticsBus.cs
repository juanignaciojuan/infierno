using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central haptics manager: collects all XRHapticImpulseProxy instances at runtime
/// and provides simple static fire methods.
/// </summary>
public static class HapticsBus
{
    private static readonly List<XRHapticImpulseProxy> _proxies = new List<XRHapticImpulseProxy>(8);
    private static float _lastRefreshTime;
    private const float RefreshInterval = 2f;

    public static void Register(XRHapticImpulseProxy p)
    {
        if (p == null) return;
        if (!_proxies.Contains(p)) _proxies.Add(p);
    }

    public static void Unregister(XRHapticImpulseProxy p)
    {
        if (p == null) return;
        _proxies.Remove(p);
    }

    private static void EnsureList()
    {
        if (_proxies.Count > 0 && Time.time < _lastRefreshTime + RefreshInterval) return;
        _proxies.RemoveAll(x => x == null);
        if (_proxies.Count == 0)
        {
            var found = Object.FindObjectsByType<XRHapticImpulseProxy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _proxies.AddRange(found);
        }
        _lastRefreshTime = Time.time;
    }

    /// <summary>
    /// Fire amplitude+duration on all proxies.
    /// </summary>
    public static void FireAll(float amplitude, float duration)
    {
        if (!Application.isPlaying) return;
        EnsureList();
        foreach (var p in _proxies)
        {
            if (p == null) continue;
            p.PlayAD(amplitude, duration);
        }
    }

    /// <summary>
    /// Fire amplitudeClose on the closest proxy to worldPos, and amplitudeFar on the others.
    /// Duration uses each proxy default; use FireAll for unified durations.
    /// </summary>
    public static void FireClosest(Vector3 worldPos, float amplitudeClose, float amplitudeFar)
    {
        if (!Application.isPlaying) return;
        EnsureList();
        if (_proxies.Count == 0) return;

        int closestIdx = -1;
        float closestSqr = float.PositiveInfinity;
        for (int i = 0; i < _proxies.Count; i++)
        {
            var p = _proxies[i];
            if (p == null) continue;
            float d = (p.transform.position - worldPos).sqrMagnitude;
            if (d < closestSqr) { closestSqr = d; closestIdx = i; }
        }

        for (int i = 0; i < _proxies.Count; i++)
        {
            var p = _proxies[i];
            if (p == null) continue;
            if (i == closestIdx) p.PlayAmplitude(amplitudeClose);
            else p.PlayAmplitude(amplitudeFar);
        }
    }
}
