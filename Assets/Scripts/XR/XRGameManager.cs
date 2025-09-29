using UnityEngine;

public class XRGameManager : MonoBehaviour
{
    public static XRGameManager Instance;

    [Header("Player Progress")]
    public bool hasKey = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
