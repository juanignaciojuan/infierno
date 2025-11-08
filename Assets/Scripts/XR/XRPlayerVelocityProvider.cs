using UnityEngine;

/// <summary>
/// Estimates the player's world-space velocity by tracking a root transform (e.g., XR Origin).
/// Works with CharacterController locomotion (no Rigidbody required).
/// </summary>
[DisallowMultipleComponent]
public class XRPlayerVelocityProvider : MonoBehaviour
{
    [Tooltip("Root transform to track (XR Origin). If null, uses this transform.")]
    public Transform root;
    [Tooltip("Exponential smoothing factor (0=none, 1=aggressive).")]
    [Range(0f,1f)] public float smoothing = 0.25f;

    public Vector3 Velocity { get; private set; }

    private Vector3 _lastPos;
    private bool _initialized;

    private void Awake()
    {
        if (root == null) root = transform;
    }

    private void Update()
    {
        Vector3 p = root.position;
        if (!_initialized)
        {
            _lastPos = p;
            _initialized = true;
            Velocity = Vector3.zero;
            return;
        }
        Vector3 rawVel = (p - _lastPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPos = p;
        Velocity = Vector3.Lerp(rawVel, Velocity, smoothing);
    }
}
