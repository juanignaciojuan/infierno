using UnityEngine;

[ExecuteAlways]
public class XRSnapToTerrain : MonoBehaviour
{
    public LayerMask groundLayer;
    public float offset = 0.05f; // Small vertical offset to prevent clipping
    public float alignSpeed = 5f; // How fast the object rotates to align with the slope

    // Allow external systems to temporarily suspend snapping (e.g., during explosion pushes)
    private float _suspendUntil;
    public void Suspend(float seconds)
    {
        _suspendUntil = Mathf.Max(_suspendUntil, Time.time + Mathf.Max(0f, seconds));
    }

    void LateUpdate()
    {
        if (Application.isPlaying && Time.time < _suspendUntil) return;
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
