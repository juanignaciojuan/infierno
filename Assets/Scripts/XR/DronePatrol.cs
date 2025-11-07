using System.Collections;
using UnityEngine;

public class DronePatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float speed = 2f;
    
    [Header("Bombing Settings")]
    [Tooltip("Prefab that will be instantiated and dropped.")]
    public GameObject bombPrefab;
    [Tooltip("Position/orientation where the bomb should spawn.")]
    public Transform bombSpawnPoint;
    [Tooltip("Seconds to wait before the first bomb is dropped.")]
    public float initialDropDelay = 2f;
    [Tooltip("Seconds between subsequent bomb drops.")]
    public float dropInterval = 10f;
    [Tooltip("Impulse applied along -bombSpawnPoint.up when the bomb spawns.")]
    public float initialDropForce = 2f;

    private Coroutine dropRoutine;

    private int currentIndex = 0;

    private void OnEnable()
    {
        dropRoutine = StartCoroutine(DropLoop());
    }

    private void OnDisable()
    {
        if (dropRoutine != null)
        {
            StopCoroutine(dropRoutine);
            dropRoutine = null;
        }
    }

    void Update()
    {
        HandlePatrol();
    }

    void HandlePatrol()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length; // loop infinito
        }
    }

    private IEnumerator DropLoop()
    {
        if (bombPrefab == null || bombSpawnPoint == null)
        {
            yield break;
        }

        if (initialDropDelay > 0f)
        {
            yield return new WaitForSeconds(initialDropDelay);
        }

        while (true)
        {
            DropBomb();

            if (dropInterval <= 0f)
            {
                yield break;
            }

            yield return new WaitForSeconds(dropInterval);
        }
    }

    void DropBomb()
    {
        // Instantiate the bomb prefab at the designated spawn point.
        GameObject bombInstance = Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation);

        // Apply an initial downward impulse so the bomb clears the drone.
        if (bombInstance.TryGetComponent<Rigidbody>(out var bombBody))
        {
            bombBody.linearVelocity = Vector3.zero;
            if (initialDropForce != 0f)
            {
                bombBody.AddForce(-bombSpawnPoint.up * initialDropForce, ForceMode.VelocityChange);
            }
        }

        // Ensure the bomb arms after a short delay to avoid detonating against the drone.
        if (bombInstance.TryGetComponent<XRBomb>(out var bomb))
        {
            bomb.ArmAfter(0.25f);
        }
    }
}
