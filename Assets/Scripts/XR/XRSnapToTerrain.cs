using UnityEngine;

[ExecuteAlways]
public class XRSnapToTerrain : MonoBehaviour
{
    public LayerMask groundLayer;
    public float offset = 0.05f; // Small vertical offset to prevent clipping
    public float alignSpeed = 5f; // How fast the object rotates to align with the slope

    void LateUpdate()
    {
        if (groundLayer == 0) return;

        // Raycast down from above the object
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 5f, groundLayer))
        {
            // Snap to terrain height
            Vector3 pos = transform.position;
            pos.y = hit.point.y + offset;
            transform.position = pos;

            // Smoothly align rotation with terrain normal
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * alignSpeed);
        }
    }
}
