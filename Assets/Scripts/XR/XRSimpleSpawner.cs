using UnityEngine;

/// <summary>
/// A simple script that instantiates a random prefab at a specific point.
/// This is designed for debugging and isolating spawning issues.
/// </summary>
public class SimpleSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("The prefabs to spawn. One will be chosen at random.")]
    public GameObject[] prefabsToSpawn;

    [Tooltip("The point where the object will be spawned.")]
    public Transform spawnPoint;

    /// <summary>
    /// This public method should be called by an event, like a button press or an XR Interactable's 'Activated' event.
    /// </summary>
    public void SpawnObject()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogError("No prefabs assigned to spawn.", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point is not assigned.", this);
            return;
        }

        // Select a random prefab from the array.
        int randomIndex = Random.Range(0, prefabsToSpawn.Length);
        GameObject prefab = prefabsToSpawn[randomIndex];

        // Instantiate the prefab at the spawn point's position and rotation.
        // It is instantiated with no parent to ensure it exists independently in the scene.
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, null);

        Debug.Log($"Spawned '{prefab.name}' at {spawnPoint.position}");
    }
}
