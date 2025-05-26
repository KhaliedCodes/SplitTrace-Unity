using Cinemachine;
using UnityEngine;

public class RuntimeLockOnCamera : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;  // Single virtual camera to control

    [Header("Player and Enemy")]
    [SerializeField] private Transform cameraRootTransform;
    [SerializeField] private Transform playerTransform;  // Player transform for reference
    [SerializeField] private LayerMask enemyLayer;

    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnRange = 15f;
    [SerializeField] private float playerRotationSpeed = 5f; // Speed at which player rotates to face enemy
    [SerializeField] private KeyCode toggleLockOnKey = KeyCode.E;

    [Header("Camera Behavior Settings")]
    [SerializeField] private float movementThreshold = 0.1f;        // Minimum movement to consider player "moving"
    [SerializeField] private float stopDelay = 0.5f;               // Time to wait after stopping before looking at enemy
    [SerializeField] private float lookAtTransitionSpeed = 2f;     // Speed of camera transition when looking at enemy
    [SerializeField] private float enemyFocusDistance = 8f;        // Distance to maintain from player when focusing on enemy
    [SerializeField] private float normalFollowDistance = 6f;      // Normal follow distance from player

    private Transform currentTargetEnemy = null;
    private bool isLockedOn = false;
    private Vector3 lastPlayerPosition;
    private float timeSinceLastMovement = 0f;
    private bool isLookingAtEnemy = false;

    // Camera component references
    private CinemachineTransposer transposer;
    private CinemachineComposer composer;

    // Original camera settings
    private Vector3 originalFollowOffset;
    private Vector3 originalScreenPosition;
    private Transform originalLookAt;

    void Start()
    {
        // Get camera components
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();

        // Store original settings
        if (transposer != null)
        {
            originalFollowOffset = transposer.m_FollowOffset;
        }

        if (composer != null)
        {
            originalScreenPosition = new Vector3(composer.m_ScreenX, composer.m_ScreenY, 0);
        }

        originalLookAt = virtualCamera.LookAt;

        // Initialize player position tracking
        lastPlayerPosition = cameraRootTransform.position;

        // Ensure camera follows player initially
        virtualCamera.Follow = cameraRootTransform;
        virtualCamera.LookAt = cameraRootTransform;
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

        // Handle movement detection and camera behavior when locked on
        if (isLockedOn && currentTargetEnemy != null)
        {
            HandleMovementDetection();
            UpdateCameraBehavior();
            CheckEnemyRange();
        }
    }

    void HandleMovementDetection()
    {
        Vector3 currentPosition = cameraRootTransform.position;
        float movementDistance = Vector3.Distance(currentPosition, lastPlayerPosition);

        if (movementDistance > movementThreshold)
        {
            // Player is moving
            timeSinceLastMovement = 0f;
            lastPlayerPosition = currentPosition;

            // If we were looking at enemy, start transitioning back to normal view
            if (isLookingAtEnemy)
            {
                isLookingAtEnemy = false;
            }
        }
        else
        {
            // Player is stationary
            timeSinceLastMovement += Time.deltaTime;

            // If player has been still long enough, start looking at enemy
            if (timeSinceLastMovement >= stopDelay && !isLookingAtEnemy)
            {
                isLookingAtEnemy = true;
            }
        }
    }

    void UpdateCameraBehavior()
    {
        if (isLookingAtEnemy)
        {
            // Transition to looking at enemy
            TransitionToEnemyFocus();
        }
        else
        {
            // Transition back to normal third-person view
            TransitionToNormalView();
        }
    }

    void TransitionToEnemyFocus()
    {
        float transitionProgress = Mathf.SmoothStep(0, 1, (timeSinceLastMovement - stopDelay) * lookAtTransitionSpeed);

        // Calculate position between player and enemy for better framing
        Vector3 midPoint = Vector3.Lerp(cameraRootTransform.position, currentTargetEnemy.position, 0.3f);

        // Smoothly transition camera to focus on the area between player and enemy
        if (transposer != null)
        {
            Vector3 targetOffset = Vector3.Lerp(originalFollowOffset,
                CalculateEnemyFocusOffset(), transitionProgress);
            transposer.m_FollowOffset = targetOffset;
        }

        // Adjust screen composition to frame both player and enemy
        if (composer != null)
        {
            Vector3 targetScreenPos = Vector3.Lerp(originalScreenPosition,
                new Vector3(0.3f, 0.5f, 0), transitionProgress);
            composer.m_ScreenX = targetScreenPos.x;
            composer.m_ScreenY = targetScreenPos.y;
        }

        // Gradually look towards a point between player and enemy
        Vector3 lookAtPoint = Vector3.Lerp(cameraRootTransform.position, midPoint, transitionProgress);
        virtualCamera.LookAt = CreateLookAtTarget(lookAtPoint);

        // Rotate player to face enemy when looking at them
        if (currentTargetEnemy != null)
        {
            Vector3 directionToEnemy = currentTargetEnemy.position - playerTransform.position;
            directionToEnemy.y = 0; // Keep rotation horizontal

            if (directionToEnemy != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation,
                    targetRotation,
                    Time.deltaTime * playerRotationSpeed);
            }
        }
    }

    void TransitionToNormalView()
    {
        float transitionSpeed = lookAtTransitionSpeed * Time.deltaTime;

        // Smoothly return to original camera settings
        if (transposer != null)
        {
            transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset,
                originalFollowOffset, transitionSpeed);
        }

        if (composer != null)
        {
            Vector3 currentScreenPos = new Vector3(composer.m_ScreenX, composer.m_ScreenY, 0);
            Vector3 targetScreenPos = Vector3.Lerp(currentScreenPos, originalScreenPosition, transitionSpeed);
            composer.m_ScreenX = targetScreenPos.x;
            composer.m_ScreenY = targetScreenPos.y;
        }

        // Return to looking at player
        virtualCamera.LookAt = cameraRootTransform;
    }

    Vector3 CalculateEnemyFocusOffset()
    {
        // Calculate a camera offset that frames both player and enemy better
        Vector3 directionToEnemy = (currentTargetEnemy.position - cameraRootTransform.position).normalized;
        Vector3 sideOffset = Vector3.Cross(directionToEnemy, Vector3.up) * 2f;

        return originalFollowOffset + sideOffset + Vector3.up * 1f;
    }

    Transform CreateLookAtTarget(Vector3 position)
    {
        // Create or update a temporary transform for camera to look at
        GameObject lookAtObject = GameObject.Find("TempCameraLookAt");
        if (lookAtObject == null)
        {
            lookAtObject = new GameObject("TempCameraLookAt");
        }

        lookAtObject.transform.position = position;
        return lookAtObject.transform;
    }

    Transform FindNearestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(cameraRootTransform.position, lockOnRange, enemyLayer);
        Transform nearestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            if (enemyCollider.isTrigger) continue;

            float dist = Vector3.Distance(cameraRootTransform.position, enemyCollider.transform.position);
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
        isLookingAtEnemy = false;
        timeSinceLastMovement = 0f;
        lastPlayerPosition = cameraRootTransform.position;

        // Camera remains following player but will adjust behavior based on movement
        virtualCamera.Follow = cameraRootTransform;
        virtualCamera.LookAt = cameraRootTransform;

        Debug.Log("Locked on enemy: " + enemy.name);
    }

    void Unlock()
    {
        isLockedOn = false;
        currentTargetEnemy = null;
        isLookingAtEnemy = false;
        timeSinceLastMovement = 0f;

        // Reset camera to original settings
        virtualCamera.Follow = cameraRootTransform;
        virtualCamera.LookAt = originalLookAt ?? cameraRootTransform;

        if (transposer != null)
        {
            transposer.m_FollowOffset = originalFollowOffset;
        }

        if (composer != null)
        {
            composer.m_ScreenX = originalScreenPosition.x;
            composer.m_ScreenY = originalScreenPosition.y;
        }

        // Clean up temporary look at target
        GameObject tempLookAt = GameObject.Find("TempCameraLookAt");
        if (tempLookAt != null)
        {
            DestroyImmediate(tempLookAt);
        }

        Debug.Log("Unlocked target.");
    }

    void CheckEnemyRange()
    {
        // Check if enemy is still valid and in range
        if (currentTargetEnemy == null ||
            Vector3.Distance(cameraRootTransform.position, currentTargetEnemy.position) > lockOnRange * 1.2f)
        {
            Debug.Log("Enemy out of range or destroyed, unlocking.");
            Unlock();
        }
    }

    // Optional: visualize lock-on range in Editor
    void OnDrawGizmosSelected()
    {
        if (cameraRootTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cameraRootTransform.position, lockOnRange);

            // Show movement threshold
            if (isLockedOn)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(cameraRootTransform.position, movementThreshold);

                // Show line to locked enemy
                if (currentTargetEnemy != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(cameraRootTransform.position, currentTargetEnemy.position);
                }
            }
        }
    }
}