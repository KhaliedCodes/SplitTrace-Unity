using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class CinemachineCameraSwitcher : MonoBehaviour
{
    [SerializeField] private List<CinemachineVirtualCamera> virtualCameras;
    [SerializeField] private float switchDelay = 0.5f;

    private int currentIndex = 0;
    private bool isSwitching = false;

    private void Start()
    {
        SetActiveCamera(currentIndex);
    }

    private void Update()
    {
        if (isSwitching) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SwitchCamera(currentIndex + 1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            SwitchCamera(currentIndex - 1);
    }

    private void SwitchCamera(int newIndex)
    {
        if (newIndex >= 0 && newIndex < virtualCameras.Count)
        {
            currentIndex = newIndex;
            StartCoroutine(SwitchWithDelay());
        }
    }

    private System.Collections.IEnumerator SwitchWithDelay()
    {
        isSwitching = true;
        SetActiveCamera(currentIndex);
        yield return new WaitForSeconds(switchDelay);
        isSwitching = false;
    }

    private void SetActiveCamera(int index)
    {
        for (int i = 0; i < virtualCameras.Count; i++)
        {
            virtualCameras[i].enabled = (i == index);
        }
    }
}
