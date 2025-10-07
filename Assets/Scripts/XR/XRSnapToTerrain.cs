using UnityEngine;

public class GroundSnapper : MonoBehaviour
{
    [Tooltip("Objects with these tags will be aligned to the terrain surface.")]
    public string[] targetTags = { "NPC", "Item" };

    [Tooltip("LayerMask for the terrain or ground objects.")]
    public LayerMask groundLayer;

    private void Start()
    {
        foreach (var tag in targetTags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objs)
            {
                RaycastHit hit;
                Vector3 startPos = obj.transform.position + Vector3.up * 100f;

                if (Physics.Raycast(startPos, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                {
                    obj.transform.position = hit.point;
                }
            }
        }
    }
}
