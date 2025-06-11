using UnityEngine;


public enum CameraMode
{
    Default,
    Detective
}
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public CameraMode CurrentCameraMode = CameraMode.Default;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
