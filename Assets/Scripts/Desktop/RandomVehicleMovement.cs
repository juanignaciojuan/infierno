using UnityEngine;

public class RandomVehicleMovement : MonoBehaviour
{
    public float forceStrength = 200f;
    public float torqueStrength = 100f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InvokeRepeating(nameof(ApplyRandomForce), 1f, 1f); // cada 2 seg aplica un empujón
    }

    void ApplyRandomForce()
    {
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        rb.AddForce(randomDir * forceStrength);
        rb.AddTorque(Vector3.up * Random.Range(-torqueStrength, torqueStrength));
    }
}
