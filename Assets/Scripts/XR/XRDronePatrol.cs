using System.Collections;
using UnityEngine;

public class DronePatrol : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    [Tooltip("Speed while patrolling between waypoints.")]
    public float waypointSpeed = 6f;
    [Tooltip("Consider waypoint reached within this XZ radius.")]
    public float waypointArrivalRadius = 0.5f;
    [Tooltip("If true and waypoints list is empty/short, auto-discover children named 'Waypoint' or under a child named 'Waypoints'.")]
    public bool autoFindWaypoints = true;

    [Header("Attack run")]
    [Tooltip("Player (or camera) to target during the attack run.")]
    public Transform player;
    [Tooltip("Horizontal speed while moving toward the player.")]
    public float attackSpeed = 8f;
    [Tooltip("Drop bomb when closer than this XZ distance to the player.")]
    public float attackDropRadius = 8f;
    [Tooltip("Maximum seconds to spend in an attack run before aborting and continuing to waypoint.")]
    public float maxAttackTime = 3f;
    [Tooltip("Use a random mid-leg fraction to trigger the attack.")]
    public bool randomMidpoint = true;
    [Tooltip("Random range [x,y] of leg fraction at which to trigger the attack.")]
    public Vector2 midpointRange = new Vector2(0.4f, 0.6f);
    [Tooltip("Fixed leg fraction at which to trigger the attack if randomMidpoint is false.")]
    [Range(0.05f, 0.95f)] public float midpointFraction = 0.5f;

    [Header("Altitude")]
    [Tooltip("Desired cruising altitude above ground. < -999 keeps current Y.")]
    public float altitudeY = 20f;

    [Header("Smoothing")]
    [Tooltip("Degrees per second to rotate (yaw) toward movement direction.")]
    public float turnSpeed = 180f;
    [Tooltip("Meters to look ahead along the current leg to curve corners.")]
    public float lookAhead = 2f;
    [Tooltip("Units per second squared for acceleration.")]
    public float acceleration = 20f;
    [Tooltip("Units per second squared for deceleration.")]
    public float deceleration = 30f;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public Transform bombSpawnPoint;
    [Tooltip("Impulse downward so bomb clears the drone.")]
    public float initialDropForce = 2f;
    public bool enableBombing = true;
    public bool debugLogs = false;

    private enum State { Patrol, Attack }
    private State _state = State.Patrol;
    private int _currentWp = 0; // moving from current -> next
    private Vector3 _legStartXZ;
    private Vector3 _legGoalXZ;
    private float _legLength;
    private bool _attackedThisLeg;
    private float _attackStartTime;
    private float _triggerFrac;
    private float _currentSpeed; // smoothed speed used for both patrol and attack

    private void OnEnable()
    {
        if (autoFindWaypoints && (waypoints == null || waypoints.Length < 2))
            AutoPopulateWaypoints();
        InitLeg(FindStartingIndex());
    }

    private int FindStartingIndex()
    {
        if (waypoints == null || waypoints.Length < 2) return 0;
        // Choose the nearest waypoint as current so we head toward the next
        int best = 0; float bestD = float.PositiveInfinity;
        Vector3 posXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Vector3 w = new Vector3(waypoints[i].position.x, 0f, waypoints[i].position.z);
            float d = Vector3.Distance(posXZ, w);
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    private void InitLeg(int fromIndex)
    {
        _currentWp = Mathf.Clamp(fromIndex, 0, (waypoints != null && waypoints.Length > 0) ? waypoints.Length - 1 : 0);
        if (waypoints == null || waypoints.Length < 2) return;
        int next = (_currentWp + 1) % waypoints.Length;
        Vector3 from = new Vector3(waypoints[_currentWp].position.x, 0f, waypoints[_currentWp].position.z);
        Vector3 to = new Vector3(waypoints[next].position.x, 0f, waypoints[next].position.z);
        _legStartXZ = from;
        _legGoalXZ = to;
        _legLength = Mathf.Max(0.01f, Vector3.Distance(from, to));
        _attackedThisLeg = false;
        _state = State.Patrol;
        _triggerFrac = randomMidpoint ? Mathf.Clamp01(Random.Range(midpointRange.x, midpointRange.y))
                                      : Mathf.Clamp01(midpointFraction);
        if (debugLogs) Debug.Log($"[DronePatrol] New leg {_currentWp}->{next}, trigger@{_triggerFrac:P0}");
    }

    private void Update()
    {
        // Fallback: if no waypoints, do nothing
        if (waypoints == null || waypoints.Length < 2)
        {
            // Simple fallback: hover toward player gently so at least moves.
            Transform tgt = GetTarget();
            if (tgt != null)
            {
                Vector3 from = transform.position;
                Vector3 goal = AltitudePosition(tgt.position);
                transform.position = Vector3.MoveTowards(from, goal, (waypointSpeed * 0.5f) * Time.deltaTime);
            }
            return;
        }

        switch (_state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    private void UpdatePatrol()
    {
        int next = (_currentWp + 1) % waypoints.Length;
        Vector3 goal = AltitudePosition(waypoints[next].position);
        Vector3 from = transform.position;

        // Compute a look-ahead target on the current leg to create a smooth curve
        Vector3 ahead = GetLookAheadTarget(transform.position, _legStartXZ, _legGoalXZ, lookAhead);
        Vector3 aheadGoal = AltitudePosition(ahead);

        float targetSpeed = waypointSpeed;
        float distToGoal = PlanarXZ(transform.position, goal);
        // Gentle braking as we approach the waypoint
        if (distToGoal < Mathf.Max(1f, lookAhead * 1.5f))
        {
            float t = Mathf.InverseLerp(0f, Mathf.Max(1f, lookAhead * 1.5f), distToGoal);
            targetSpeed = Mathf.Lerp(0.5f, waypointSpeed, t);
        }
        UpdateSmoothedSpeed(targetSpeed);
        MoveAndTurnTowards(aheadGoal, _currentSpeed);

        // Arrived at waypoint -> advance to next leg
        if (PlanarXZ(transform.position, goal) <= waypointArrivalRadius)
        {
            InitLeg(next);
            return;
        }

        // Trigger attack at mid-leg once per leg
        if (!_attackedThisLeg)
        {
            float traveled = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), _legStartXZ);
            float frac = traveled / _legLength;
            if (frac >= _triggerFrac && GetTarget() != null)
            {
                _state = State.Attack;
                _attackStartTime = Time.time;
                if (debugLogs) Debug.Log("[DronePatrol] Attack run started");
            }
        }
    }

    private void UpdateAttack()
    {
        Transform tgt = GetTarget();
        if (tgt == null)
        {
            // No target -> abort
            _state = State.Patrol;
            return;
        }

    Vector3 goal = AltitudePosition(tgt.position);
    UpdateSmoothedSpeed(attackSpeed);
    MoveAndTurnTowards(goal, _currentSpeed);

        float dist = PlanarXZ(transform.position, tgt.position);
        bool timeUp = (Time.time - _attackStartTime) >= maxAttackTime;
        bool closeEnough = dist <= attackDropRadius;

        if (closeEnough || timeUp)
        {
            if (enableBombing && bombPrefab != null)
                SpawnBomb();
            _attackedThisLeg = true;
            _state = State.Patrol;
            if (debugLogs) Debug.Log($"[DronePatrol] Attack run complete (d={dist:F1}, timeUp={timeUp})");
        }
    }

    private Vector3 AltitudePosition(Vector3 source)
    {
        float y = (altitudeY > -999f) ? altitudeY : transform.position.y;
        return new Vector3(source.x, y, source.z);
    }

    private void UpdateSmoothedSpeed(float targetSpeed)
    {
        float dt = Time.deltaTime;
        if (_currentSpeed < targetSpeed)
        {
            _currentSpeed = Mathf.Min(targetSpeed, _currentSpeed + Mathf.Max(0f, acceleration) * dt);
        }
        else
        {
            _currentSpeed = Mathf.Max(targetSpeed, _currentSpeed - Mathf.Max(0f, deceleration) * dt);
        }
    }

    private void MoveAndTurnTowards(Vector3 targetPos, float speed)
    {
        Vector3 from = transform.position;
        Vector3 to = targetPos;
        transform.position = Vector3.MoveTowards(from, to, Mathf.Max(0f, speed) * Time.deltaTime);

        Vector3 dir = to - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Mathf.Max(0f, turnSpeed) * Time.deltaTime);
        }
    }

    private static Vector3 GetLookAheadTarget(Vector3 currentWorld, Vector3 legStartXZ, Vector3 legGoalXZ, float lookAheadDist)
    {
        // Project current position to the leg (XZ) and move forward by lookAheadDist
        Vector3 p = new Vector3(currentWorld.x, 0f, currentWorld.z);
        Vector3 a = legStartXZ; Vector3 b = legGoalXZ;
        Vector3 ab = b - a; float abLen = ab.magnitude; if (abLen < 0.0001f) return b;
        Vector3 abN = ab / abLen;
        float t = Vector3.Dot(p - a, abN); // distance along the leg
        float ahead = Mathf.Clamp(t + Mathf.Max(0f, lookAheadDist), 0f, abLen);
        Vector3 onSeg = a + abN * ahead;
        return onSeg;
    }

    private void SpawnBomb()
    {
        Vector3 pos = bombSpawnPoint != null ? bombSpawnPoint.position : transform.position + Vector3.down * 0.2f;
        Quaternion rot = bombSpawnPoint != null ? bombSpawnPoint.rotation : Quaternion.identity;
        GameObject bomb = Instantiate(bombPrefab, pos, rot);

        if (bomb.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            Vector3 dropDir = bombSpawnPoint != null ? -bombSpawnPoint.up : Vector3.down;
            Vector3 finalDir = dropDir.sqrMagnitude < 0.01f ? Vector3.down : dropDir;
            if (initialDropForce != 0f) rb.AddForce(finalDir * initialDropForce, ForceMode.Impulse);
            if (Vector3.Dot(dropDir, Vector3.down) < 0f) rb.linearVelocity += Vector3.down * 2f;
        }
        if (bomb.TryGetComponent<XRBomb>(out var xrBomb)) xrBomb.ArmAfter(0.25f);
        StartCoroutine(TemporaryIgnoreCollision(bomb));
    }

    private IEnumerator TemporaryIgnoreCollision(GameObject bomb)
    {
        if (bomb == null) yield break;
        var bombCols = bomb.GetComponentsInChildren<Collider>();
        var droneCols = GetComponentsInChildren<Collider>();
        foreach (var bc in bombCols)
            foreach (var dc in droneCols)
                if (bc && dc) Physics.IgnoreCollision(bc, dc, true);
        yield return new WaitForSeconds(0.4f);
        foreach (var bc in bombCols)
            foreach (var dc in droneCols)
                if (bc && dc) Physics.IgnoreCollision(bc, dc, false);
    }

    private Transform GetTarget()
    {
        if (player != null) return player;
        if (Camera.main != null) return Camera.main.transform;
        return null;
    }

    private static float PlanarXZ(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f; return Vector3.Distance(a, b);
    }

    private void AutoPopulateWaypoints()
    {
        // Look for a direct child named "Waypoints"; if found, use its children.
        var root = transform.Find("Waypoints");
        System.Collections.Generic.List<Transform> list = new System.Collections.Generic.List<Transform>();
        if (root != null)
        {
            foreach (Transform c in root)
            {
                if (c != null) list.Add(c);
            }
        }
        else
        {
            // Fallback: collect children whose name starts with "Waypoint"
            foreach (Transform c in transform)
            {
                if (c != null && c.name.StartsWith("Waypoint")) list.Add(c);
            }
        }
        if (list.Count >= 2)
        {
            waypoints = list.ToArray();
            if (debugLogs) Debug.Log($"[DronePatrol] Auto-populated {waypoints.Length} waypoints.");
        }
    }
}
