using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private List<Transform> PositionsToMoveTo = new();

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
        Transform targetPosition = PositionsToMoveTo[Random.Range(0, PositionsToMoveTo.Count)];
        while (!CheckNewPositionAvailable(targetPosition.position))
        {
            targetPosition = PositionsToMoveTo[Random.Range(0, PositionsToMoveTo.Count)];
        }
        transform.position = targetPosition.position;
    }
    

    bool CheckNewPositionAvailable(Vector3 NewPosition)
    {
        return NewPosition != transform.position;
    }
}
