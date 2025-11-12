using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight object pool for bombs (XRBomb).
/// Keyed by prefab so you can have more than one bomb variant.
/// </summary>
public static class BombPool
{
    private class Pool { public readonly Queue<GameObject> q = new Queue<GameObject>(); }
    private static readonly Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();

    public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
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
        }

        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        if (go.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if (go.TryGetComponent<XRBomb>(out var bomb))
        {
            bomb._poolPrefabRef = prefab; // internal link for return
            bomb.ResetStateForSpawn();
        }
        return go;
    }

    public static void Return(GameObject instance)
    {
        if (instance == null) return;
        var bomb = instance.GetComponent<XRBomb>();
        GameObject key = bomb != null ? bomb._poolPrefabRef : null;
        if (key == null)
        {
            // Not pooled; just destroy to avoid leaks
            Object.Destroy(instance);
            return;
        }
        if (!_pools.TryGetValue(key, out var pool))
        {
            pool = new Pool();
            _pools[key] = pool;
        }
        instance.SetActive(false);
        pool.q.Enqueue(instance);
    }

    public static void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Pool();
            _pools[prefab] = pool;
        }
        for (int i = 0; i < count; i++)
        {
            var go = Object.Instantiate(prefab);
            go.SetActive(false);
            pool.q.Enqueue(go);
        }
    }
}
