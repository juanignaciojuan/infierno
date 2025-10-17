using UnityEngine;

public class StickToTerrain : MonoBehaviour
{
    [Tooltip("Vertical offset between the model's pivot and the terrain surface (e.g. adjust to match feet)")]
    public float heightOffset = 0.0f;

    [Tooltip("Smoothly interpolate toward terrain height (0 = instant snap)")]
    public float smoothSpeed = 0.0f;

    Terrain terrain;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
            Debug.LogWarning($"{name}: No active Terrain found in the scene.");
    }

    void LateUpdate()
    {
        if (terrain == null)
            return;

        Vector3 pos = transform.position;

        // Get terrain height at current XZ
        float terrainHeight = terrain.SampleHeight(pos) + terrain.GetPosition().y + heightOffset;

        if (smoothSpeed > 0f)
        {
            // Smoothly move toward the terrain height
            pos.y = Mathf.Lerp(pos.y, terrainHeight, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // Snap instantly
            pos.y = terrainHeight;
        }

        transform.position = pos;
    }

#if UNITY_EDITOR
    // Optional visual debug
    void OnDrawGizmosSelected()
    {
        if (Terrain.activeTerrain == null) return;
        Vector3 p = transform.position;
        float h = Terrain.activeTerrain.SampleHeight(p) + Terrain.activeTerrain.GetPosition().y;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(p.x, h, p.z), p);
    }
#endif
}
