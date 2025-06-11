using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private List<MovingObjectPosition> PositionsToMoveTo = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MoveToNewPosition()
    {
        // Check if there are any positions to move to
        if (PositionsToMoveTo.Count == 0)
        {
            Debug.LogWarning("No positions to move to.");
            return;
        }

        // Move the object to the first position in the list
        MovingObjectPosition targetPosition = PositionsToMoveTo[Random.Range(0, PositionsToMoveTo.Count)]; ;
        bool availableToMove = false;
        for (int i = 0;  i < 3; i++)
        {
            if (!CheckNewPositionAvailable(targetPosition))
            {
                targetPosition = PositionsToMoveTo[Random.Range(0, PositionsToMoveTo.Count)];
                availableToMove = false;
            }
            else
            {
                availableToMove = true;
                break;
            }
        };
        if(!availableToMove)
        {
            Debug.LogWarning("No available positions to move to after checking 3 times.");
            return;
        }
        transform.position = targetPosition.transform.position;
    }
    

    bool CheckNewPositionAvailable(MovingObjectPosition NewPosition)
    {
        return NewPosition.transform.position != transform.position && NewPosition.IsVisibleToCamera == false;
    }
}
