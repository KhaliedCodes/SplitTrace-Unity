using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Vector3 boxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    private Quaternion orientation = Quaternion.identity;
    public float maxDistance = 100f;
    private HashSet<GameObject> currentlyVisible = new HashSet<GameObject>();
    Ray ray;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    void Update()
    {
        Collider[] candidates = Physics.OverlapSphere(transform.position, maxDistance);

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        HashSet<GameObject> visibleThisFrame = new HashSet<GameObject>();

        foreach (Collider candidate in candidates)
        {
            GameObject obj = candidate.gameObject;
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, candidate.bounds))
                continue; // Not in frustum

            // Optional: check if it’s actually visible (not behind something)
            // Vector3 dirToObject = candidate.bounds.center - transform.position;
            // float dist = dirToObject.magnitude;

            // if (Physics.Raycast(transform.position, dirToObject.normalized, out RaycastHit hit, dist))
            // {
            //     if (hit.collider != candidate)
            //         continue; // Occluded
            // }
            if (obj.CompareTag("MovingObject"))
                visibleThisFrame.Add(obj);
            if (!currentlyVisible.Contains(obj))
            {
                // Debug.Log("Seen: " + obj.name);
                // Call custom logic: e.g., obj.GetComponent<MyComponent>()?.OnSeen();
            }
            // Debug.Log("Visible object: " + candidate.name);
        }

        foreach (GameObject obj in currentlyVisible)
        {
            if (!visibleThisFrame.Contains(obj))
            {
                // Debug.Log("Exited View: " + obj.name);
                // Call exit logic: e.g., obj.GetComponent<MyComponent>()?.OnExitedView();
                obj.GetComponent<MovingObject>().MoveToNewPosition();
            }
        }

        currentlyVisible = visibleThisFrame;
    }
    bool IsVisibleToCamera(Transform target)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(target.position);

        // Is in front of camera and within screen bounds?
        bool isInView = viewportPoint.z > 0 &&
                        viewportPoint.x > 0 && viewportPoint.x < 1 &&
                        viewportPoint.y > 0 && viewportPoint.y < 1;

        if (!isInView)
            return false;

        // Raycast from camera to target
        Vector3 dirToTarget = (target.position - Camera.main.transform.position).normalized;
        float distance = Vector3.Distance(Camera.main.transform.position, target.position);

        if (Physics.Raycast(Camera.main.transform.position, dirToTarget, out RaycastHit hit, distance))
        {
            // Something is blocking the view
            return hit.transform == target;
        }

        // Nothing blocked it, or the hit is the target itself
        return true;
    }
    void OnDrawGizmos()
    {
        if (ray.direction == Vector3.zero) return;

        Vector3 start = ray.origin;
        Vector3 end = start + ray.direction.normalized * maxDistance;

        Gizmos.color = Color.cyan;

        // Draw the start and end boxes
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Draw start box
        Gizmos.matrix = Matrix4x4.TRS(start, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);

        // Draw end box
        Gizmos.matrix = Matrix4x4.TRS(end, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);

        // Optional: Draw interpolated boxes to simulate the cast path
        int steps = 10;
        for (int i = 1; i < steps; i++)
        {
            float t = i / (float)steps;
            Vector3 pos = Vector3.Lerp(start, end, t);
            Gizmos.matrix = Matrix4x4.TRS(pos, orientation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);
        }

        // Reset Gizmos matrix
        Gizmos.matrix = oldMatrix;
    }
}
