using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HeadSpawner : MonoBehaviour
{
    public GameObject headPrefab;
    public float spawnInterval = 10f;
    public Transform spawnPoint;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnHead), spawnInterval, spawnInterval);
    }

    void SpawnHead()
    {
        if (headPrefab != null && spawnPoint != null)
        {
            GameObject head = Instantiate(headPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
