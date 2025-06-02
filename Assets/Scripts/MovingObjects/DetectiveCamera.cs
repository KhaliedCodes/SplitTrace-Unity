using Cinemachine;
using UnityEngine;

public class DetectiveCamera : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera activeCamera;
    [SerializeField] Camera mainCamera;
    [SerializeField] CinemachineVirtualCamera detectiveCamera;
    [SerializeField] CinemachineVirtualCamera defaultCamera;
    int activeCameraPriorityModifier = 31331;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeCamera();
    }

    // Update is called once per frame
    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            ChangeCamera();
        }
    }


    void ChangeCamera()
    {
        if (activeCamera == defaultCamera)
        {
            setCameraPriority(defaultCamera, detectiveCamera);
        }
        else if (activeCamera == detectiveCamera)
        {
            setCameraPriority(detectiveCamera, defaultCamera);
        }
        else
        {
            defaultCamera.Priority += activeCameraPriorityModifier;
            activeCamera = defaultCamera;
        }
    }

    void setCameraPriority(CinemachineVirtualCamera CurrentCameraMode, CinemachineVirtualCamera NewComerMode)
    {
        CurrentCameraMode.Priority -= activeCameraPriorityModifier;
        NewComerMode.Priority += activeCameraPriorityModifier;
        activeCamera = NewComerMode;
    }

}
