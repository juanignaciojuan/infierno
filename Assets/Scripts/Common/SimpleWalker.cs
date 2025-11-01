using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    public float speed = 2f;              // Velocidad de caminar
    public float walkDistance = 10f;      // Distancia máxima antes de girar

    private Vector3 startPos;
    private bool goingForward = true;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Mover hacia adelante
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Calcular distancia recorrida desde el inicio
        float distance = Vector3.Distance(startPos, transform.position);

        if (distance >= walkDistance)
        {
            // Girar 180 grados
            transform.Rotate(0, 180f, 0);
            goingForward = !goingForward;
        }
    }
}
