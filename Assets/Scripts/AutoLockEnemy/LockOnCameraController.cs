using Cinemachine;
using UnityEngine;

public class RuntimeLockOnCamera : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera normalVirtualCamera;  // Your regular gameplay cam
    public CinemachineVirtualCamera targetGroupCamera;    // Camera using target group
    public CinemachineTargetGroup targetGroup;            // Target group component

    [Header("Player and Enemy")]
    public Transform playerTransform;
    public LayerMask enemyLayer;

    [Header("Lock-On Settings")]
    public float lockOnRange = 15f;
    public KeyCode toggleLockOnKey = KeyCode.E;

    private Transform currentTargetEnemy = null;
    private bool isLockedOn = false;

    void Start()
    {
        // Ensure cameras are correctly enabled at start
        normalVirtualCamera.gameObject.SetActive(true);
        targetGroupCamera.gameObject.SetActive(false);

        // Make sure the target group starts with just the player
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        targetGroup.AddMember(playerTransform, 1f, 1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleLockOnKey))
        {
            if (!isLockedOn)
            {
                // Try to find the nearest enemy to lock on
                currentTargetEnemy = FindNearestEnemy();

                if (currentTargetEnemy != null)
                {
                    LockOnEnemy(currentTargetEnemy);
                }
                else
                {
                    Debug.Log("No enemy found in range to lock on.");
                }
            }
            else
            {
                Unlock();
            }
        }

        // Optional: update target group if enemy moves
        if (isLockedOn && currentTargetEnemy != null)
        {
            UpdateTargetGroupPositions();
        }
    }

    Transform FindNearestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(playerTransform.position, lockOnRange, enemyLayer);
        Transform nearestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            if (enemyCollider.isTrigger) continue;

            float dist = Vector3.Distance(playerTransform.position, enemyCollider.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearestEnemy = enemyCollider.transform;
            }
        }
        return nearestEnemy;
    }

    void LockOnEnemy(Transform enemy)
    {
        isLockedOn = true;
        currentTargetEnemy = enemy;

        // Clear and add both player and enemy to target group
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        targetGroup.AddMember(playerTransform, 1f, 1f);
        targetGroup.AddMember(enemy, 1f, 1f);

        // Switch cameras
        normalVirtualCamera.gameObject.SetActive(false);
        targetGroupCamera.gameObject.SetActive(true);

        Debug.Log("Locked on enemy: " + enemy.name);
    }

    void Unlock()
    {
        isLockedOn = false;
        currentTargetEnemy = null;

        // Reset target group to only player
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        targetGroup.AddMember(playerTransform, 1f, 1f);

        // Switch back to normal camera
        targetGroupCamera.gameObject.SetActive(false);
        normalVirtualCamera.gameObject.SetActive(true);

        Debug.Log("Unlocked target.");
    }

    void UpdateTargetGroupPositions()
    {
        // This is mostly automatic, but you can force refresh or handle edge cases here if needed.
        // Cinemachine automatically tracks the transforms, so usually nothing needed here.
    }

    // Optional: visualize lock-on range in Editor
    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, lockOnRange);
        }
    }
}
