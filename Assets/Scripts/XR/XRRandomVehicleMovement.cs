using UnityEngine;

public class XRRandomVehicleMovement : MonoBehaviour
{
    public float speed = 5f;
    public float range = 10f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) > range)
        {
            transform.position = startPos;
            transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }
    }
}
