using UnityEngine;

/// <summary>
/// Rotates the GameObject around its local Y-axis at a constant speed.
/// Ideal for simple looping animations like a spinning vinyl record, fan, or wheel.
/// </summary>
public class VinylSpinner : MonoBehaviour
{
    [Tooltip("The speed of rotation in degrees per second. 360 means one full rotation per second.")]
    [SerializeField]
    private float rotationSpeed = 45f;

    void Update()
    {
        // Rotate the object around its own local up axis (Y-axis).
        // Using Time.deltaTime makes the rotation smooth and frame-rate independent.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
