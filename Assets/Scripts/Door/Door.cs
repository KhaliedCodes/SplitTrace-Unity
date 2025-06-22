using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smooth = 2f;

    private Vector3 startRotation;
    private bool isOpen = false;

    private void Start()
    {
        if (pivotPoint == null)
        {
            Debug.LogError("Pivot Point is not assigned to the door!");
            return;
        }
        startRotation = transform.eulerAngles;
    }

    private void Update()
    {
        if (pivotPoint == null) return;

        // Calculate target rotation
        Vector3 targetRotation = startRotation;
        if (isOpen)
        {
            targetRotation.y += openAngle;
        }

        // Make the door rotate around the pivot point
        transform.RotateAround(
            pivotPoint.position,
            Vector3.up,
            Mathf.LerpAngle(transform.eulerAngles.y, targetRotation.y, smooth * Time.deltaTime) - transform.eulerAngles.y
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = false;
        }
    }
}
