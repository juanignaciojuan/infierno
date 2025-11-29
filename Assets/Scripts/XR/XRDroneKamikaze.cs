using UnityEngine;

/// <summary>
/// A drone behavior that chases the player and dives to explode.
/// Based on XRDronePatrol movement logic for stability.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class XRDroneKamikaze : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    public bool autoFindPlayer = true;

    [Header("Movement Settings")]
    [Tooltip("Speed while chasing the player.")]
    public float chaseSpeed = 8f;
    [Tooltip("Speed while diving at the player.")]
    public float diveSpeed = 15f;
    [Tooltip("Turn speed (deg/sec).")]
    public float turnSpeed = 120f;
    [Tooltip("Acceleration.")]
    public float acceleration = 20f;
    [Tooltip("Deceleration.")]
    public float deceleration = 30f;

    [Header("Altitude Settings")]
    [Tooltip("Height above terrain to maintain while chasing.")]
    public float hoverHeight = 6f;
    [Tooltip("Layers to check for ground.")]
    public LayerMask groundLayers = ~0;

    [Header("Attack Settings")]
    [Tooltip("Distance to start diving at the player.")]
    public float diveDistance = 15f;
    [Tooltip("Distance to explode.")]
    public float explodeDistance = 1.5f;

    [Header("Explosion")]
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;
    public float explosionForce = 1200f;
    public float explosionRadius = 6f;
    public bool respawnAfterExplosion = true;
    public float respawnRange = 40f;

    [Header("Model Settings")]
    [Tooltip("Additional rotation to apply to the model (e.g. 0, 90, 0) if it faces the wrong way.")]
    public Vector3 modelRotationOffset = Vector3.zero;

    private enum State { Chase, Dive }
    private State _state = State.Chase;
    private bool _exploded = false;
    private float _currentSpeed;
    private Rigidbody _rb;

    private Vector3 _initialPosition;

    private void OnEnable()
    {
        _exploded = false;
        _state = State.Chase;
        if (target == null && autoFindPlayer) target = FindPlayer();
    }

    private void Start()
    {
        _initialPosition = transform.position;

        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.isKinematic = true; // We move via Transform
        }

        if (autoFindPlayer && target == null) target = FindPlayer();
    }

    private void Update()
    {
        if (_exploded) return;
        if (target == null)
        {
            if (autoFindPlayer) target = FindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        // State Switching
        if (_state == State.Chase)
        {
            if (dist <= diveDistance)
            {
                _state = State.Dive;
            }
        }
        else if (_state == State.Dive)
        {
            // If we missed and flew too far away, go back to chase
            if (dist > diveDistance * 2f)
            {
                _state = State.Chase;
            }
        }

        // Execution
        if (_state == State.Chase)
        {
            UpdateChase();
        }
        else
        {
            UpdateDive();
        }

        // Explosion Check
        if (dist <= explodeDistance)
        {
            Explode();
        }
    }

    private void UpdateChase()
    {
        // Calculate goal: Player's XZ, but at safe Altitude
        Vector3 targetPos = target.position;
        float terrainY = GetTerrainHeight(transform.position);
        float safeY = terrainY + hoverHeight;
        
        // If player is high up, fly at their level (but never below safeY)
        float goalY = Mathf.Max(safeY, targetPos.y);
        
        Vector3 goal = new Vector3(targetPos.x, goalY, targetPos.z);

        UpdateSmoothedSpeed(chaseSpeed);
        MoveAndTurnTowards(goal, _currentSpeed);
    }

    private void UpdateDive()
    {
        // Fly straight at player
        UpdateSmoothedSpeed(diveSpeed);
        MoveAndTurnTowards(target.position, _currentSpeed);
    }

    private void UpdateSmoothedSpeed(float targetSpeed)
    {
        float dt = Time.deltaTime;
        if (_currentSpeed < targetSpeed)
            _currentSpeed = Mathf.Min(targetSpeed, _currentSpeed + acceleration * dt);
        else
            _currentSpeed = Mathf.Max(targetSpeed, _currentSpeed - deceleration * dt);
    }

    private void MoveAndTurnTowards(Vector3 targetPos, float speed)
    {
        // Move
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Turn
        Vector3 dir = targetPos - transform.position;
        // If chasing, keep rotation flat. If diving, look at target.
        if (_state == State.Chase) dir.y = 0;
        
        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(modelRotationOffset);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    private float GetTerrainHeight(Vector3 pos)
    {
        // Start high up
        Vector3 origin = new Vector3(pos.x, 500f, pos.z);
        
        // Use RaycastAll to pierce through the player if they are blocking the ground
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 1000f, groundLayers);
        
        float maxHeight = -Mathf.Infinity;
        bool foundGround = false;

        foreach (var hit in hits)
        {
            // Ignore Player and Self
            if (hit.collider.CompareTag("Player")) continue;
            if (hit.transform == transform) continue;
            if (hit.collider.isTrigger) continue; // Ignore triggers usually

            if (hit.point.y > maxHeight)
            {
                maxHeight = hit.point.y;
                foundGround = true;
            }
        }

        if (foundGround) return maxHeight;
        
        // Fallback to Unity Terrain
        if (Terrain.activeTerrain != null)
        {
            return Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
        }
        
        // Absolute fallback
        return 0f; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_exploded) return;
        
        // Explode on Player or Ground
        if (collision.gameObject.CompareTag("Player") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Default") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Dunas") || // Your terrain layer
            (target != null && collision.transform == target))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 4f);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            // Don't apply explosion physics to the Player (avoid lifting them).
            if (c.CompareTag("Player")) continue;

            if (c.attachedRigidbody != null)
                c.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.5f, ForceMode.Impulse);
        }

        if (respawnAfterExplosion)
            Respawn();
        else
            Destroy(gameObject);
    }

    private void Respawn()
    {
        _exploded = false;
        
        // If target is lost, try to find it again
        if (target == null && autoFindPlayer) target = FindPlayer();
        
        if (target == null) return;

        // Respawn at the initial position
        transform.position = _initialPosition;
        _currentSpeed = 0f;
        
        if (_rb != null && !_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        
        _state = State.Chase;
    }

    private Transform FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) return p.transform;
        if (Camera.main != null) return Camera.main.transform;
        return null;
    }
}
