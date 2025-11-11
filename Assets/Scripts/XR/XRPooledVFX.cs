using System.Collections;
using UnityEngine;

/// <summary>
/// Auto-returns a VFX instance to VFXPool after its particle duration.
/// Added automatically by VFXPool if missing.
/// </summary>
public class PooledVFX : MonoBehaviour
{
    [HideInInspector] public GameObject _originPrefab;
    [Tooltip("Extra lifetime padding in seconds before returning to pool.")]
    public float extraLifetime = 0.25f;

    private Coroutine _coro;

    private void OnEnable()
    {
        if (_coro != null) StopCoroutine(_coro);
        _coro = StartCoroutine(ReturnWhenDone());
    }

    private void OnDisable()
    {
        if (_coro != null)
        {
            StopCoroutine(_coro);
            _coro = null;
        }
    }

    private IEnumerator ReturnWhenDone()
    {
        float wait = 2f;
        // Try to estimate particle lifetime
        var ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            // Approx: system duration + max start lifetime
            float maxStartLifetime = 0f;
            if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                maxStartLifetime = Mathf.Max(main.startLifetime.constantMin, main.startLifetime.constantMax);
            else if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                maxStartLifetime = main.startLifetime.constant;
            wait = main.duration + maxStartLifetime + extraLifetime;
        }
        yield return new WaitForSeconds(wait);
        VFXPool.Despawn(_originPrefab, gameObject);
    }
}
