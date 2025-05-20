using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour
{

    [SerializeField] private List<MovingObject> MovingObjects = new();
    bool IsOn = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchLight();
        }
    }

    public void SwitchLight()
    {

        IsOn = !IsOn;
        foreach (MovingObject movingObject in MovingObjects)
        {
            if (!IsOn)
            {
                movingObject.MoveToNewPosition();
            }
        }
    }
}
