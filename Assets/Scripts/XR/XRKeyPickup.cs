using UnityEngine;

public class XRKeyPickup : MonoBehaviour
{
    public AudioSource pickupSound;
    public GameObject keyModel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pickupSound != null) pickupSound.Play();
            XRGameManager.Instance.hasKey = true;
            if (keyModel != null) keyModel.SetActive(false);
        }
    }
}
