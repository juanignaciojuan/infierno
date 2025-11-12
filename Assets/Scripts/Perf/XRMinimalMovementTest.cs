using UnityEngine;

// Attach to any GameObject (eg. a Cube). It moves on a simple figure-8 or circle.
// Purpose: sanity-check update smoothness on Quest without any test framework.
public class XRMinimalMovementTest : MonoBehaviour
{
    public enum Path { Circle, Figure8, PingPong }
    public Path path = Path.Circle;

    [Tooltip("Motion radius/extent in meters")] public float radius = 2f;
    [Tooltip("Cycles per second")] public float hz = 0.2f;
    [Tooltip("Lock Y to this altitude (set < -999 to keep current Y)")] public float y = -999f;

    private Vector3 _origin;

    void Start()
    {
        _origin = transform.position;
        if (y > -999f) _origin.y = y;
    }

    void Update()
    {
        float t = Time.time * hz * Mathf.PI * 2f;
        Vector3 p = _origin;
        switch (path)
        {
            case Path.Circle:
                p += new Vector3(Mathf.Cos(t), 0f, Mathf.Sin(t)) * radius;
                break;
            case Path.Figure8:
                p += new Vector3(Mathf.Sin(t), 0f, Mathf.Sin(t) * Mathf.Cos(t)) * radius;
                break;
            case Path.PingPong:
                p += new Vector3(Mathf.PingPong(Time.time * hz * radius * 4f, radius * 2f) - radius, 0f, 0f);
                break;
        }
        transform.position = p;
    }
}
