using UnityEngine;

// Drop this on an empty GameObject in your test scene.
// It prints lightweight frame/timing stats without any asmdef or test framework.
// Combine with Unity Profiler + Memory Profiler for deeper dives.
public class XRPerfProbe : MonoBehaviour
{
    [Header("Averages (seconds)")]
    [Tooltip("Rolling window size for averaging (frames)")]
    public int window = 120; // ~1.6s at 72Hz

    [Tooltip("Optional: cap Application.targetFrameRate during test (<=0 keeps project setting)")]
    public int targetFps = 72;

    [Tooltip("Log every N seconds (0 disables console logs)")]
    public float logEverySeconds = 2f;

    [Tooltip("Optional reference to player for distance gating notes")]
    public Transform player;

    private float[] _frameTimes;
    private int _idx;
    private float _accum;
    private float _lastLog;

    void Awake()
    {
        _frameTimes = new float[Mathf.Max(30, window)];
        _idx = 0; _accum = 0f;
        if (targetFps > 0) Application.targetFrameRate = targetFps;
        QualitySettings.vSyncCount = 0; // avoid vsync masking
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        // ring buffer
        _accum -= _frameTimes[_idx];
        _frameTimes[_idx] = dt;
        _accum += dt;
        _idx = (_idx + 1) % _frameTimes.Length;

        if (logEverySeconds > 0f && Time.unscaledTime - _lastLog >= logEverySeconds)
        {
            _lastLog = Time.unscaledTime;
            float avg = _accum / _frameTimes.Length;
            float fps = (avg > 0f) ? 1f / avg : 0f;
            // CPU and GPU times shown in the Profiler; here we emit a simple summary
            Debug.Log($"[XRPerfProbe] avg dt={avg*1000f:F2}ms  fps={fps:F1}  window={_frameTimes.Length} frames");
        }
    }
}
