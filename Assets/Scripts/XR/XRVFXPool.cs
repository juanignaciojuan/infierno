using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Very small pooling utility for particle GameObjects.
/// Use VFXPool.Spawn(prefab, pos, rot) instead of Instantiate to reduce GC and spikes.
/// </summary>
public static class VFXPool
{
    private class Pool
    {
        public readonly Queue<GameObject> q = new Queue<GameObject>();
    }

    private static readonly Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Pool();
            _pools[prefab] = pool;
        }

        GameObject go = null;
        while (pool.q.Count > 0 && go == null)
        {
            go = pool.q.Dequeue();
        }

        if (go == null)
        {
            go = Object.Instantiate(prefab);
            var pv = go.GetComponent<PooledVFX>();
            if (pv == null) pv = go.AddComponent<PooledVFX>();
            pv._originPrefab = prefab; // internal link
        }

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        // If it has a ParticleSystem, ensure it restarts
        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Simulate(0f, true, true);
            ps.Play(true);
        }
        return go;
    }

    public static void Despawn(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null) return;
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Pool();
            _pools[prefab] = pool;
        }
        instance.SetActive(false);
        pool.q.Enqueue(instance);
    }
}
