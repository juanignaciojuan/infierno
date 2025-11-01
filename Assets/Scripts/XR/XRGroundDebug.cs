using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

public class GroundDebug : MonoBehaviour
{
    public GravityProvider gravity;

    void Update()
    {
        if (gravity != null)
            Debug.Log("Grounded: " + gravity.isGrounded);
    }
}
