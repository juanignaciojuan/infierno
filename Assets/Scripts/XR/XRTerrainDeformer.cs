using System;
using UnityEngine;

namespace XR
{
    /// <summary>
    /// Runtime Terrain heightmap deformer that can stamp craters and reset to the initial state.
    /// - Caches a copy of the original heights on Awake.
    /// - ApplyCrater: lower terrain around world position using a smooth falloff.
    /// - ResetTerrain: restores original heights.
    /// Notes:
    /// - Works with standard Unity Terrain (Terrain + TerrainData).
    /// - Uses sub-region GetHeights/SetHeights for performance.
    /// - Depth is in meters; internally converted to heightmap delta using terrain size.y.
    /// </summary>
    [DisallowMultipleComponent]
    public class XRTerrainDeformer : MonoBehaviour
    {
        [Header("Target Terrain (auto if null)")]
        [SerializeField] private Terrain _terrain;

        [Header("Crater Defaults")]
        [Tooltip("Radius in meters for craters when no radius provided by caller.")]
    [Range(0.5f, 50f)] public float defaultRadius = 2.5f;
        [Tooltip("Maximum depth in meters at the center of the crater.")]
    [Range(0.1f, 50f)] public float defaultDepth = 0.8f;
        [Tooltip("Falloff sharpness. Higher = sharper edges (2..8 typical).")]
        [Range(0.5f, 12f)] public float defaultFalloff = 4f;
        [Tooltip("Maximum depth a crater can have, to prevent extreme values from causing issues.")]
    [Range(1f, 100f)] public float maxDepth = 15f;
        [Tooltip("If enabled, craters near maxDepth will also carve Terrain holes.")]
        public bool allowHolesAtMaxDepth = true;
        [Tooltip("Inner radius ratio (0.3-0.9) used for hole mask compared to crater radius.")]
        [Range(0.3f,0.9f)] public float holeInnerRadiusRatio = 0.55f;

        [Header("Crater Irregularity")]
        [Tooltip("Amount of rocky noise to apply to the crater floor. 0 = smooth.")]
        [Range(0f, 1f)] public float noiseAmount = 0.3f;
        [Tooltip("Scale of the noise pattern. Larger values = larger rock formations.")]
    [Range(0.5f, 200f)] public float noiseScale = 10f;

        [Header("Performance")]
        [Tooltip("Optional: maximum crater stamps allowed per second to avoid spikes. 0 = unlimited.")]
    [Range(0f, 60f)] public float maxCraterPerSecond = 0f;
        [Tooltip("Clamp crater edit size to this many samples square to keep SetHeights cheap.")]
        [Range(8, 1024)] public int maxEditSamples = 256;
        [Tooltip("Coalesce heightmap Sync calls to reduce spikes.")]
        public bool coalesceSync = true;
        [Tooltip("If coalescing, call SyncHeightmap every N frames (>=1)." )]
        [Range(1,10)] public int syncEveryNFrames = 3;

    private TerrainData _data;
        private float[,] _originalHeights;
    private bool[,] _originalHoles;
        private int _heightmapResolution;
    private int _holesResolution;
        private Vector3 _terrainSize;
        private Vector3 _terrainPos;
        private float _lastStampTime;
    private int _syncCountdown;

        private void Awake()
        {
            if (_terrain == null)
                _terrain = GetComponent<Terrain>();

            if (_terrain == null)
            {
                Debug.LogWarning("[XRTerrainDeformer] No Terrain found on GameObject. Add this to a Terrain.");
                enabled = false;
                return;
            }

            _data = _terrain.terrainData;
            _heightmapResolution = _data.heightmapResolution;
            _holesResolution = _data.holesResolution;
            _terrainSize = _data.size;
            _terrainPos = _terrain.transform.position;
            _syncCountdown = 0;

            try
            {
                _originalHeights = _data.GetHeights(0, 0, _heightmapResolution, _heightmapResolution);
                // Cache original holes mask to restore on exit
                _originalHoles = _data.GetHoles(0, 0, _holesResolution, _holesResolution);
            }
            catch (Exception e)
            {
                Debug.LogError($"[XRTerrainDeformer] Failed to cache original heights: {e.Message}");
                enabled = false;
            }
        }

        /// <summary>
        /// Restores the Terrain heights to the state captured at Awake.
        /// </summary>
        public void ResetTerrain()
        {
            if (_data == null || _originalHeights == null) return;
            _data.SetHeightsDelayLOD(0, 0, _originalHeights);
            _data.SyncHeightmap();
            if (_originalHoles != null)
            {
                _data.SetHoles(0, 0, _originalHoles);
            }
        }

        /// <summary>
        /// Apply a crater at world position with provided settings; uses defaults when values <= 0.
        /// </summary>
        public void ApplyCrater(Vector3 worldPos, float radius = -1f, float depth = -1f, float falloff = -1f)
        {
            if (_data == null) return;
            if (maxCraterPerSecond > 0f)
            {
                if (Time.time - _lastStampTime < (1f / maxCraterPerSecond)) return;
                _lastStampTime = Time.time;
            }

            radius = (radius > 0f) ? radius : defaultRadius;
            depth = Mathf.Clamp((depth > 0f) ? depth : defaultDepth, 0.1f, maxDepth);
            falloff = (falloff > 0f) ? falloff : defaultFalloff;

            // Convert world position to normalized terrain coordinates [0,1]
            Vector3 localPos = worldPos - _terrain.transform.position;
            float nx = Mathf.Clamp01(localPos.x / _terrainSize.x);
            float nz = Mathf.Clamp01(localPos.z / _terrainSize.z);

            // Compute the area of the heightmap to edit
            int hmRes = _heightmapResolution;
            int cx = Mathf.RoundToInt(nx * (hmRes - 1));
            int cz = Mathf.RoundToInt(nz * (hmRes - 1));

            // Convert radius in meters to heightmap samples
            float metersPerSampleX = _terrainSize.x / (hmRes - 1);
            float metersPerSampleZ = _terrainSize.z / (hmRes - 1);
            int radSamplesX = Mathf.CeilToInt(radius / metersPerSampleX);
            int radSamplesZ = Mathf.CeilToInt(radius / metersPerSampleZ);

            int halfW = Mathf.Clamp(radSamplesX, 1, maxEditSamples / 2);
            int halfH = Mathf.Clamp(radSamplesZ, 1, maxEditSamples / 2);

            int xStart = Mathf.Clamp(cx - halfW, 0, hmRes - 1);
            int zStart = Mathf.Clamp(cz - halfH, 0, hmRes - 1);
            int xEnd = Mathf.Clamp(cx + halfW, 0, hmRes - 1);
            int zEnd = Mathf.Clamp(cz + halfH, 0, hmRes - 1);

            int width = xEnd - xStart + 1;
            int height = zEnd - zStart + 1;
            if (width <= 0 || height <= 0) return;

            float[,] heights = _data.GetHeights(xStart, zStart, width, height);

            // Debug output (can be disabled by commenting out)
            Debug.Log($"[XRTerrainDeformer] Crater stamp @ {worldPos}, r={radius:F2}, d={depth:F2}");

            float centerX = cx - xStart;
            float centerZ = cz - zStart;

            // Depth expressed in meters -> normalized height delta
            float heightScale = depth / _terrainSize.y;

            // Apply radial falloff depression
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Compute distance in meters from crater center
                    float dxMeters = (x - centerX) * metersPerSampleX;
                    float dzMeters = (z - centerZ) * metersPerSampleZ;
                    float dist = Mathf.Sqrt(dxMeters * dxMeters + dzMeters * dzMeters);
                    if (dist > radius) continue;

                    // Calculate base depression with falloff
                    float t = Mathf.Clamp01(1f - (dist / radius));
                    float fall = Mathf.Pow(t, falloff);
                    float depression = fall * heightScale;

                    // Add procedural noise for irregularity
                    if (noiseAmount > 0f)
                    {
                        float noiseX = (xStart + x) * noiseScale / _heightmapResolution;
                        float noiseZ = (zStart + z) * noiseScale / _heightmapResolution;
                        float noise = Mathf.PerlinNoise(noiseX, noiseZ) - 0.5f; // Center noise around 0
                        depression += noise * heightScale * noiseAmount;
                    }

                    float newH = heights[z, x] - depression;
                    heights[z, x] = Mathf.Max(0f, newH);
                }
            }

            _data.SetHeightsDelayLOD(xStart, zStart, heights);
            if (coalesceSync && syncEveryNFrames > 1)
            {
                if (_syncCountdown <= 0) _syncCountdown = syncEveryNFrames;
                _syncCountdown--;
                if (_syncCountdown <= 0)
                {
                    _data.SyncHeightmap();
                }
            }
            else
            {
                _data.SyncHeightmap();
            }

            // Optional: cut a terrain hole when reaching maximum depth
            if (allowHolesAtMaxDepth && depth >= maxDepth * 0.95f)
            {
                // Map crater center and radius from heightmap space to holes space
                int hcRes = _holesResolution;
                int hcx = Mathf.RoundToInt(nx * (hcRes - 1));
                int hcz = Mathf.RoundToInt(nz * (hcRes - 1));
                float metersPerHoleX = _terrainSize.x / (hcRes - 1);
                float metersPerHoleZ = _terrainSize.z / (hcRes - 1);
                int holeRadX = Mathf.CeilToInt((radius * holeInnerRadiusRatio) / metersPerHoleX);
                int holeRadZ = Mathf.CeilToInt((radius * holeInnerRadiusRatio) / metersPerHoleZ);

                int hxStart = Mathf.Clamp(hcx - holeRadX, 0, hcRes - 1);
                int hzStart = Mathf.Clamp(hcz - holeRadZ, 0, hcRes - 1);
                int hxEnd = Mathf.Clamp(hcx + holeRadX, 0, hcRes - 1);
                int hzEnd = Mathf.Clamp(hcz + holeRadZ, 0, hcRes - 1);
                int hWidth = hxEnd - hxStart + 1;
                int hHeight = hzEnd - hzStart + 1;
                if (hWidth > 0 && hHeight > 0)
                {
                    bool[,] holes = _data.GetHoles(hxStart, hzStart, hWidth, hHeight);
                    for (int z = 0; z < hHeight; z++)
                    {
                        for (int x = 0; x < hWidth; x++)
                        {
                            float dx = (x + hxStart) - hcx;
                            float dz = (z + hzStart) - hcz;
                            float dist = Mathf.Sqrt(dx * dx * metersPerHoleX * metersPerHoleX + dz * dz * metersPerHoleZ * metersPerHoleZ);
                            if (dist <= radius * holeInnerRadiusRatio)
                            {
                                // In Unity Terrain holes mask, FALSE indicates a hole (carved out)
                                holes[z, x] = false;
                            }
                        }
                    }
                    _data.SetHoles(hxStart, hzStart, holes);
                }
            }
        }

    // Editor-only: ensure terrain returns to original state when stopping Play Mode
    // This prevents accidental asset changes during Windows editor testing while having no effect on device builds.
#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        ResetTerrain();
    }
#endif

        /// <summary>
        /// Optional gizmo to see default radius at a test point.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_terrain == null) _terrain = GetComponent<Terrain>();
            if (_terrain == null) return;
            Gizmos.color = new Color(0.9f, 0.4f, 0.1f, 0.4f);
            // Visualize at terrain center
            Vector3 c = _terrain.transform.position + _terrain.terrainData.size * 0.5f;
            c.y = _terrain.SampleHeight(c) + _terrain.transform.position.y + 0.1f;
            Gizmos.DrawWireSphere(c, defaultRadius);
        }
    }
}
