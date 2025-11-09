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
    [Tooltip("Randomized seconds between drops (min).")]
    public float dropIntervalMin = 4f;
    [Tooltip("Randomized seconds between drops (max).")]
    public float dropIntervalMax = 9f;
    [Tooltip("Impulse applied along -bombSpawnPoint.up when the bomb spawns.")]
    public float initialDropForce = 2f;
    [Tooltip("Enable/disable bombing behavior at runtime.")]
    public bool enableBombing = true;
    [Tooltip("When true, prints debug info about bombing decisions.")]
    public bool debugLogs = false;
    [Tooltip("Ignore player distance and always drop (testing mode).")]
    public bool ignorePlayerDistance = false;
    [Tooltip("Force a fixed drop interval for testing (<=0 disables).")]
    public float testFixedInterval = 0f;

    [Header("Player Proximity Gate")]
    [Tooltip("Only drop bombs when within this distance to the player. If null, main camera will be used.")]
    public Transform player;
    public float maxDropDistance = 18f;
    public float nearDropBonusChance = 0.5f; // extra chance to drop when very close
    public float nearDistance = 8f;

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
        if (initialDropDelay > 0f)
        {
            yield return new WaitForSeconds(initialDropDelay);
        }

        while (true)
        {
            if (!enableBombing || bombPrefab == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Determine target (player or camera fallback)
            Transform target = player != null ? player : (Camera.main != null ? Camera.main.transform : null);

            bool shouldDrop = ignorePlayerDistance; // if ignoring distance, we always allow
            if (!shouldDrop)
            {
                if (target != null)
                {
                    float dist = Vector3.Distance(transform.position, target.position);
                    if (dist <= maxDropDistance)
                    {
                        shouldDrop = true;
                    }
                    else if (dist <= nearDistance) // nearDistance should be >= maxDropDistance for bonus, adjust docs or logic
                    {
                        // Bonus chance when inside nearDistance but outside max range
                        shouldDrop = Random.value < nearDropBonusChance;
                    }
                    if (debugLogs)
                        Debug.Log($"[DronePatrol] Dist={dist:F1} -> drop={shouldDrop}");
                }
                else
                {
                    // No target reference; allow drop to avoid stalling logic
                    shouldDrop = true;
                    if (debugLogs) Debug.Log("[DronePatrol] No target reference; allowing drop.");
                }
            }

            if (shouldDrop)
            {
                DropBomb();
            }

            float wait = testFixedInterval > 0f ? testFixedInterval : Random.Range(dropIntervalMin, dropIntervalMax);
            if (debugLogs) Debug.Log($"[DronePatrol] Next drop in {wait:F2}s");
            yield return new WaitForSeconds(wait);
        }
    }

    public void ForceDrop()
    {
        if (bombPrefab == null) return;
        DropBombInternal();
    }

    void DropBomb()
    {
        if (!enableBombing) return;
        if (bombPrefab == null) return;
        DropBombInternal();
    }

    void DropBombInternal()
    {
        // Fallback: if spawn point missing, use drone position.
        Vector3 pos = bombSpawnPoint != null ? bombSpawnPoint.position : transform.position + Vector3.down * 0.2f;
        Quaternion rot = bombSpawnPoint != null ? bombSpawnPoint.rotation : Quaternion.identity;
        GameObject bombInstance = Instantiate(bombPrefab, pos, rot);
        if (debugLogs) Debug.Log("[DronePatrol] Dropped bomb " + bombInstance.name);

        // Apply an initial downward impulse so the bomb clears the drone.
        if (bombInstance.TryGetComponent<Rigidbody>(out var bombBody))
        {
            // Ensure physics settings allow falling
            bombBody.isKinematic = false;
            bombBody.useGravity = true;
            // Reset velocity to ensure deterministic drop
            bombBody.linearVelocity = Vector3.zero;
            Vector3 dropDir = bombSpawnPoint != null ? -bombSpawnPoint.up : Vector3.down;
            if (initialDropForce != 0f)
            {
                // Use impulse so mass matters less; always push downward globally if spawnPoint is mis-oriented.
                Vector3 finalDir = dropDir.sqrMagnitude < 0.01f ? Vector3.down : dropDir;
                bombBody.AddForce(finalDir * initialDropForce, ForceMode.Impulse);
            }
            // Safety: if spawn point accidentally points upward, force a small downward velocity
            if (Vector3.Dot(dropDir, Vector3.down) < 0f)
            {
                bombBody.linearVelocity += Vector3.down * 2f;
            }
            bombBody.constraints = RigidbodyConstraints.None;
        }

        // Ensure the bomb arms after a short delay to avoid detonating against the drone.
        if (bombInstance.TryGetComponent<XRBomb>(out var bomb))
        {
            bomb.ArmAfter(0.25f);
        }

        // Temporarily ignore collision with drone to prevent instant detonation or sticking
        StartCoroutine(TemporaryIgnoreCollision(bombInstance));
    }

    private IEnumerator TemporaryIgnoreCollision(GameObject bomb)
    {
        if (bomb == null) yield break;
        Collider[] bombCols = bomb.GetComponentsInChildren<Collider>();
        Collider[] droneCols = GetComponentsInChildren<Collider>();
        foreach (var bc in bombCols)
        {
            foreach (var dc in droneCols)
            {
                if (bc != null && dc != null)
                {
                    Physics.IgnoreCollision(bc, dc, true);
                }
            }
        }
        yield return new WaitForSeconds(0.4f);
        foreach (var bc in bombCols)
        {
            foreach (var dc in droneCols)
            {
                if (bc != null && dc != null)
                {
                    Physics.IgnoreCollision(bc, dc, false);
                }
            }
        }
    }
}
