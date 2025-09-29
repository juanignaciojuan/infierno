using UnityEngine;

public class DronePatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    private int currentIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length; // loop infinito
        }
    }
}
