using UnityEngine;
using System.Collections;

/// <summary>
/// Smoothly applies explosion impulses to a CharacterController-based XR rig without needing a Rigidbody.
/// Attach to XR Origin root. Call ReceiveImpulse(position, force, radius) from explosion code.
/// </summary>
public class XRExplosionImpulseReceiver : MonoBehaviour
{
    [Tooltip("Max horizontal displacement applied by a single explosion.")]
    public float maxHorizontalDisplacement = 2f;
    [Tooltip("Max vertical displacement applied by a single explosion.")]
    public float maxVerticalDisplacement = 0.8f;
    [Tooltip("Total duration of the push lerp.")]
    public float pushDuration = 0.35f;
    [Tooltip("Curve shaping the push (0..1 time).")]
    public AnimationCurve pushCurve = AnimationCurve.EaseInOut(0,0,1,1);

    private Coroutine currentRoutine;

    public void ReceiveImpulse(Vector3 explosionPos, float force, float radius)
    {
        Vector3 toRig = transform.position - explosionPos;
        float dist = toRig.magnitude;
        if (radius <= 0f) radius = 1f;
        float atten = Mathf.Clamp01(1f - dist / radius);
        Vector3 dir = toRig.normalized;
        Vector3 targetOffset = dir * (atten * maxHorizontalDisplacement);
        targetOffset.y += atten * maxVerticalDisplacement;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(DoPush(targetOffset));
    }

    private IEnumerator DoPush(Vector3 offset)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + offset;
        float t = 0f;
        while (t < pushDuration)
        {
            t += Time.deltaTime;
            float eval = pushCurve.Evaluate(Mathf.Clamp01(t / pushDuration));
            transform.position = Vector3.Lerp(startPos, endPos, eval);
            yield return null;
        }
    }
}
