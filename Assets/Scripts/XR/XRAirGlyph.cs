using UnityEngine;
using System.Collections;

/// <summary>
/// Displays a "glyph" (letter, quad, sprite) spawned by an air-drawing weapon.
/// Stays fixed where placed, faces the player's camera, then fades and destroys.
/// Works with either SpriteRenderer or MeshRenderer (single material) on the root.
/// </summary>
[DisallowMultipleComponent]
public class XRAirGlyph : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Total lifetime before the glyph is destroyed.")] public float lifetime = 5f;
    [Tooltip("Seconds at the end of lifetime spent fading out.")] public float fadeDuration = 1.5f;

    [Header("Billboard")]
    [Tooltip("If true, the glyph continually faces the main camera.")] public bool billboard = true;
    [Tooltip("Optional world-space scale override on spawn (0 keeps existing).")] public float uniformScaleOnSpawn = 0f;

    private float _spawnTime;
    private bool _fading;
    private Camera _cam;

    // Renderers we support
    private SpriteRenderer _sprite;
    private Renderer _genericRenderer;
    private MaterialPropertyBlock _mpb;
    private Color _initialColor = Color.white;

    public void Initialize(float customLifetime, float customFade)
    {
        if (customLifetime > 0f) lifetime = customLifetime;
        if (customFade > 0f) fadeDuration = customFade;
    }

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        if (_sprite == null)
        {
            _genericRenderer = GetComponent<Renderer>();
            if (_genericRenderer != null)
            {
                _mpb = new MaterialPropertyBlock();
                _genericRenderer.GetPropertyBlock(_mpb);
                if (_genericRenderer.sharedMaterial != null && _genericRenderer.sharedMaterial.HasProperty("_Color"))
                {
                    _initialColor = _genericRenderer.sharedMaterial.color;
                }
            }
        }
        else
        {
            _initialColor = _sprite.color;
        }

        _spawnTime = Time.time;
        if (uniformScaleOnSpawn > 0f)
        {
            transform.localScale = Vector3.one * uniformScaleOnSpawn;
        }
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (billboard && _cam != null)
        {
            Vector3 toCam = _cam.transform.position - transform.position;
            if (toCam.sqrMagnitude > 0.0001f)
            {
                // Stable face camera without flipping (use position based).
                transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
            }
        }

        float age = Time.time - _spawnTime;
        float timeToFade = lifetime - fadeDuration;
        if (!_fading && age >= timeToFade)
        {
            StartCoroutine(FadeAndDestroy());
            _fading = true;
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(1f, 0f, t);
            ApplyAlpha(alpha);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void ApplyAlpha(float a)
    {
        if (_sprite != null)
        {
            Color c = _initialColor;
            c.a = a;
            _sprite.color = c;
        }
        else if (_genericRenderer != null)
        {
            Color c = _initialColor;
            c.a = a;
            _mpb.SetColor("_Color", c);
            _genericRenderer.SetPropertyBlock(_mpb);
        }
    }
}
