using UnityEngine;
using System.Collections;

/// <summary>
/// Receives explosion impulses (from bombs/grenades) and applies a temporary push + small hop
/// without killing or disabling the NPC. Intended for walkers using XRSnapToTerrain.
/// Call ApplyImpulse(origin, force, radius) from explosion code.
/// </summary>
[DisallowMultipleComponent]
public class XRNonLethalImpulseReceiver : MonoBehaviour
{
    [Tooltip("Reference to XRSnapToTerrain on this NPC (optional). If assigned, snapping is paused during push.")]
    public XRSnapToTerrain snapper;
    [Tooltip("Maximum horizontal displacement applied by one impulse.")]
    public float maxHorizontalDisplacement = 1.2f;
    [Tooltip("Vertical hop added on impulse (scaled by attenuation).")]
    public float maxVerticalDisplacement = 0.4f;
    [Tooltip("Duration of the push lerp.")]
    public float pushDuration = 0.35f;
    [Tooltip("Curve shaping displacement over time (0..1).")]
    public AnimationCurve pushCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [Tooltip("If true, re-enable terrain snapping halfway through vertical hop (smoother land).")]
    public bool resumeSnapMidway = true;

    private Coroutine _routine;
    private bool _snapPaused;

    /// <summary>
    /// Apply a non-lethal impulse. 'force' approximates strength; 'radius' sets attenuation.
    /// </summary>
    public void ApplyImpulse(Vector3 explosionPos, float force, float radius)
    {
        if (radius <= 0f) radius = 1f;
        Vector3 toNpc = transform.position - explosionPos;
        float dist = toNpc.magnitude;
        float atten = Mathf.Clamp01(1f - dist / radius);
        if (atten <= 0f) return; // outside blast

        Vector3 dir = toNpc.sqrMagnitude < 0.0001f ? Vector3.up : toNpc.normalized;
        Vector3 targetOffset = dir * (atten * maxHorizontalDisplacement);
        targetOffset.y += atten * maxVerticalDisplacement;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(DoPush(targetOffset));
    }

    private IEnumerator DoPush(Vector3 offset)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + offset;
        float t = 0f;
        bool snapResumed = false;
        PauseSnap(true);
        while (t < pushDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / pushDuration);
            float eval = pushCurve.Evaluate(norm);
            Vector3 newPos = Vector3.Lerp(startPos, endPos, eval);
            transform.position = newPos;
            if (resumeSnapMidway && !_snapPaused && !snapResumed)
            {
                // Already resumed externally; skip
                snapResumed = true;
            }
            else if (resumeSnapMidway && _snapPaused && !snapResumed && norm > 0.5f)
            {
                PauseSnap(false); // allow terrain resnap during descend
                snapResumed = true;
            }
            yield return null;
        }
        PauseSnap(false);
    }

    private void PauseSnap(bool pause)
    {
        if (snapper == null) snapper = GetComponent<XRSnapToTerrain>();
        if (snapper == null) return;
        if (pause)
        {
            if (!_snapPaused)
            {
                snapper.enabled = false;
                _snapPaused = true;
            }
        }
        else
        {
            if (_snapPaused)
            {
                snapper.enabled = true;
                _snapPaused = false;
            }
        }
    }
}
