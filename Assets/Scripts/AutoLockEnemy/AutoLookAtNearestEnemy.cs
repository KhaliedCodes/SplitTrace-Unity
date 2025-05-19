using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class AutoLookAtNearestEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public CinemachineVirtualCamera cinemachineCamera;

    [Header("Tracking Settings")]
    public float trackingRange = 15f;
    public float horizontalFieldOfView = 90f;
    public float verticalFieldOfView = 60f;  // Added vertical FOV limitation
    public float detectionInterval = 0.2f;   // Performance optimization: interval between detection checks

    [Header("Enemy Layer")]
    public LayerMask enemyLayer;

    [Header("Lock-On Settings")]
    public KeyCode toggleLockOnKey = KeyCode.E;
    public KeyCode nextTargetKey = KeyCode.Q;  // Added key for switching to next target
    public KeyCode previousTargetKey = KeyCode.Z;  // Added key for switching to previous target

    [Header("Cinemachine Group Camera")]
    public CinemachineVirtualCamera cinemachineGroupCamera;
    public CinemachineTargetGroup targetGroup;
    public Transform playerTransform;

    [Header("UI")]
    public GameObject targetIndicatorPrefab;

    [Header("Camera Priorities")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 0;

    [Header("Look At Target")]
    public Transform lookAtTarget; // The transform at head/eye level

    private Transform targetEnemy;
    private GameObject activeTargetIndicator;
    private bool _isLockedOn = false;
    private CustomStarterAssetsInputs inputHandler;
    private float nextDetectionTime = 0f;  // For detection timing
    private List<Transform> potentialTargets = new List<Transform>();  // Store all valid targets
    private int currentTargetIndex = -1;  // Track current target in the list

    // Store initial camera settings
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private Transform initialFollowTarget;
    private Transform initialLookAtTarget;

    // Additional camera state tracking
    private CinemachineTransposer transposer;
    private Vector3 initialCameraOffset;
    private bool initialCameraInitialized = false;

    public Transform currentTarget => targetEnemy;
    public bool isLockedOn => _isLockedOn && targetEnemy != null;

    void Awake()
    {
        playerTransform = transform;
        inputHandler = GetComponent<CustomStarterAssetsInputs>();
    }

    void Start()
    {
        SetupPlayerTransform();
        SetupCameras();

        // Store initial camera settings
        StoreInitialCameraSettings();

        // Validate enemy layer
        if (enemyLayer.value == 0)
        {
            Debug.LogError("Enemy layer is not set! Please assign a layer for enemies in the Inspector.");
        }

        // Validate camera reference
        if (cameraTransform == null)
        {
            Debug.LogWarning("Camera transform reference is missing. Will try to use main camera at runtime.");
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                Debug.Log("Found main camera to use for targeting.");
            }
        }
    }

    void StoreInitialCameraSettings()
    {
        if (cinemachineCamera != null)
        {
            initialFollowTarget = cinemachineCamera.Follow;
            initialLookAtTarget = cinemachineCamera.LookAt;

            // Store transposer offset if available
            transposer = cinemachineCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                initialCameraOffset = transposer.m_FollowOffset;
                Debug.Log($"Stored initial camera offset: {initialCameraOffset}");
            }
        }

        if (cameraTransform != null)
        {
            initialCameraPosition = cameraTransform.position;
            initialCameraRotation = cameraTransform.rotation;
            Debug.Log($"Stored initial camera position: {initialCameraPosition}");
        }

        initialCameraInitialized = true;
    }

    void SetupPlayerTransform()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;

                // Try to find LookAtTarget
                lookAtTarget = playerTransform.Find("LookAtTarget");
                if (lookAtTarget == null)
                {
                    Debug.LogWarning("LookAtTarget not found on Player. Using Player root as fallback.");
                    lookAtTarget = playerTransform;
                }
            }
            else
            {
                Debug.LogWarning("Player object not found!");
            }
        }
    }

    void SetupCameras()
    {
        // Setup target group with only the player at start
        if (targetGroup != null)
        {
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = playerTransform, weight = 1f, radius = 0.5f }
            };
        }

        // Setup Cinemachine Group Camera
        if (cinemachineGroupCamera != null)
        {
            cinemachineGroupCamera.Follow = targetGroup.transform;
            cinemachineGroupCamera.LookAt = targetGroup.transform;
            cinemachineGroupCamera.Priority = inactivePriority;
        }

        // Setup Cinemachine Main Camera
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = lookAtTarget;
            cinemachineCamera.LookAt = lookAtTarget != null ? lookAtTarget : playerTransform;
            cinemachineCamera.Priority = activePriority;
        }
    }

    void Update()
    {
        if (cameraTransform == null || cinemachineCamera == null) return;

        HandleLockOnToggle();

        if (_isLockedOn)
        {
            HandleTargetSwitching();
            ManageCurrentTarget();
        }
    }

    void HandleLockOnToggle()
    {
        if (Input.GetKeyDown(toggleLockOnKey))
        {
            _isLockedOn = !_isLockedOn;

            if (_isLockedOn)
            {
                // Existing lock-on enabling code...
            }
            else
            {
                ClearTargetIndicator();
                targetEnemy = null;
                currentTargetIndex = -1;
                potentialTargets.Clear();
                ResetGroupToPlayerOnly();

                // Switch to player camera
                cinemachineCamera.Priority = activePriority;
                cinemachineGroupCamera.Priority = inactivePriority;

                // Explicitly set Follow and LookAt to current targets
                if (cinemachineCamera != null)
                {
                    cinemachineCamera.Follow = lookAtTarget;
                    cinemachineCamera.LookAt = lookAtTarget != null ? lookAtTarget : playerTransform;
                }

                Debug.Log("Lock-on mode disabled");
            }
        }
    }

    void RestoreInitialCameraSettings()
    {
        if (!initialCameraInitialized)
        {
            Debug.LogWarning("Cannot restore camera settings - initial values not stored.");
            return;
        }

        // Restore the camera's Follow and LookAt targets
        if (cinemachineCamera != null)
        {
            Debug.Log($"Restoring camera Follow: {initialFollowTarget?.name ?? "null"} and LookAt: {initialLookAtTarget?.name ?? "null"}");

            cinemachineCamera.Follow = initialFollowTarget;
            cinemachineCamera.LookAt = initialLookAtTarget;

            // Restore transposer offset if available
            if (transposer != null)
            {
                Debug.Log($"Restoring camera offset from {transposer.m_FollowOffset} to {initialCameraOffset}");
                transposer.m_FollowOffset = initialCameraOffset;
            }

            // Force camera to update immediately to apply the settings
            if (cinemachineCamera.VirtualCameraGameObject != null)
            {
                cinemachineCamera.OnTargetObjectWarped(initialFollowTarget, Vector3.zero);

                // Reset the camera brain to ensure position update
                var brain = FindObjectOfType<CinemachineBrain>();
                if (brain != null)
                {
                    brain.ManualUpdate();
                }
            }
        }

        // If we need to force the physical camera position
        if (cameraTransform != null && Camera.main != null && Camera.main.transform == cameraTransform)
        {
            Debug.Log("Performing additional camera positioning reset...");

            // We may need to wait a frame for Cinemachine to update
            StartCoroutine(ResetCameraAfterDelay());
        }
    }

    System.Collections.IEnumerator ResetCameraAfterDelay()
    {
        // Wait for end of frame to let Cinemachine process first
        yield return new WaitForEndOfFrame();

        // If camera is still not at the right position, force it
        if (Vector3.Distance(cameraTransform.position, initialCameraPosition) > 0.1f)
        {
            Debug.Log($"Force resetting camera position from {cameraTransform.position} to {initialCameraPosition}");

            // Only force if we're using the standard camera (not lock-on)
            if (cinemachineCamera.Priority > cinemachineGroupCamera.Priority)
            {
                // Forcibly position the camera transform if needed
                cameraTransform.position = initialCameraPosition;
                cameraTransform.rotation = initialCameraRotation;
            }
        }
    }

    void HandleTargetSwitching()
    {
        if (potentialTargets.Count <= 1) return;

        bool targetSwitched = false;

        // Next target
        if (Input.GetKeyDown(nextTargetKey))
        {
            currentTargetIndex = (currentTargetIndex + 1) % potentialTargets.Count;
            targetSwitched = true;
        }

        // Previous target
        if (Input.GetKeyDown(previousTargetKey))
        {
            currentTargetIndex--;
            if (currentTargetIndex < 0) currentTargetIndex = potentialTargets.Count - 1;
            targetSwitched = true;
        }

        if (targetSwitched)
        {
            targetEnemy = potentialTargets[currentTargetIndex];
            ClearTargetIndicator();
            CreateTargetIndicator();
            SetGroupTargets(playerTransform, targetEnemy);
        }
    }

    void ManageCurrentTarget()
    {
        // Check if we need to update targets based on time interval
        if (Time.time >= nextDetectionTime)
        {
            nextDetectionTime = Time.time + detectionInterval;

            // Store the current target before refreshing
            Transform previousTarget = targetEnemy;

            // Refresh potential targets
            FindAllPotentialTargets();

            // If no targets found, exit lock-on mode
            if (potentialTargets.Count == 0)
            {
                _isLockedOn = false;
                ClearTargetIndicator();
                ResetToNormalCamera();
                return;
            }

            // Try to find previous target in the new list
            if (previousTarget != null)
            {
                int indexInNewList = potentialTargets.IndexOf(previousTarget);
                if (indexInNewList >= 0)
                {
                    // Previous target is still valid
                    currentTargetIndex = indexInNewList;
                    targetEnemy = previousTarget;
                }
                else
                {
                    // Previous target is no longer valid, select closest
                    SelectClosestTarget();
                    ClearTargetIndicator();
                    CreateTargetIndicator();
                }
            }
            else
            {
                // No previous target, select closest
                SelectClosestTarget();
                ClearTargetIndicator();
                CreateTargetIndicator();
            }

            // Update group targets with current enemy
            SetGroupTargets(playerTransform, targetEnemy);
        }

        // Update indicator position even if we're not refreshing targets
        UpdateTargetIndicator();
    }

    void FindAllPotentialTargets()
    {
        // Debug.Log($"Looking for enemies on layer: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(enemyLayer.value, 2)))}");
        Collider[] hits = Physics.OverlapSphere(playerTransform.position, trackingRange, enemyLayer);
        Debug.Log($"Found {hits.Length} colliders in range within enemyLayer mask");

        potentialTargets.Clear();

        // If no camera transform is assigned, try to find main camera
        if (cameraTransform == null)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                Debug.Log("Found main camera as fallback");
            }
            else
            {
                Debug.LogError("No camera reference found - can't determine field of view for targeting");
                return;
            }
        }

        foreach (Collider hit in hits)
        {
            // Skip trigger colliders unless you specifically want to target them
            if (hit.isTrigger)
            {
                // Debug.Log($"Skipping trigger: {hit.name}");
                continue;
            }

            Vector3 directionToEnemy = hit.transform.position - playerTransform.position;
            float distanceToEnemy = directionToEnemy.magnitude;

            // Skip if too close or too far (safety check)
            if (distanceToEnemy < 0.1f || distanceToEnemy > trackingRange * 1.1f)
            {
                // Debug.Log($"Enemy {hit.name} distance invalid: {distanceToEnemy}");
                continue;
            }

            // Check horizontal angle
            Vector3 horizontalDirection = directionToEnemy;
            horizontalDirection.y = 0;

            // Safety check for zero vector
            if (horizontalDirection.sqrMagnitude < 0.001f)
            {
                // Enemy is directly above or below, count as in FOV
                horizontalDirection = cameraTransform.forward;
                horizontalDirection.y = 0;
                if (horizontalDirection.sqrMagnitude < 0.001f)
                {
                    horizontalDirection = Vector3.forward; // Fallback if camera is looking straight up/down
                }
            }

            horizontalDirection.Normalize();
            Vector3 cameraForwardHorizontal = cameraTransform.forward;
            cameraForwardHorizontal.y = 0;

            // Safety check for zero vector
            if (cameraForwardHorizontal.sqrMagnitude < 0.001f)
            {
                cameraForwardHorizontal = Vector3.forward; // Fallback if camera is looking straight up/down
            }

            cameraForwardHorizontal.Normalize();

            float horizontalAngle = Vector3.Angle(cameraForwardHorizontal, horizontalDirection);

            // Temporarily skip vertical FOV check for debugging
            // This will only use horizontal FOV to see if that's the issue
            bool isInFOV = horizontalAngle <= horizontalFieldOfView / 2f;

            /*
            // Check vertical angle - comment out for now to debug the horizontal check first
            float verticalAngle = 0;
            try
            {
                Vector3 perpendicularToCameraForward = Vector3.Cross(cameraForwardHorizontal, Vector3.up).normalized;
                Vector3 projectionPlaneNormal = Vector3.Cross(perpendicularToCameraForward, cameraForwardHorizontal).normalized;
                
                Vector3 dirToEnemyProjected = Vector3.ProjectOnPlane(directionToEnemy.normalized, perpendicularToCameraForward);
                Vector3 cameraForwardProjected = Vector3.ProjectOnPlane(cameraTransform.forward, perpendicularToCameraForward);
                
                if (dirToEnemyProjected.sqrMagnitude > 0.001f && cameraForwardProjected.sqrMagnitude > 0.001f)
                {
                    verticalAngle = Vector3.Angle(dirToEnemyProjected, cameraForwardProjected);
                    isInFOV = horizontalAngle <= horizontalFieldOfView / 2f && verticalAngle <= verticalFieldOfView / 2f;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error calculating vertical angle: {e.Message}");
                // Fall back to horizontal only if there's an error
                isInFOV = horizontalAngle <= horizontalFieldOfView / 2f;
            }
            */

            if (isInFOV)
            {
                // Debug.Log($"Enemy {hit.name} is in FOV, checking line of sight");
                if (HasLineOfSight(hit.transform))
                {
                    Debug.Log($"Adding target: {hit.name} at distance {distanceToEnemy:F2}, angle {horizontalAngle:F1}°");
                    potentialTargets.Add(hit.transform);
                }
                else
                {
                    // Debug.Log($"No line of sight to {hit.name}");
                }
            }
            else
            {
                // Debug.Log($"Enemy {hit.name} outside FOV: horizontal angle {horizontalAngle:F1}° (limit: {horizontalFieldOfView/2f:F1}°)");
            }
        }

        Debug.Log($"Found {potentialTargets.Count} valid targets after FOV and line-of-sight checks");

        // Sort targets by distance from player
        potentialTargets.Sort((a, b) =>
            Vector3.Distance(playerTransform.position, a.position)
            .CompareTo(Vector3.Distance(playerTransform.position, b.position))
        );
    }

    void SelectClosestTarget()
    {
        if (potentialTargets.Count > 0)
        {
            targetEnemy = potentialTargets[0];
            currentTargetIndex = 0;
        }
        else
        {
            targetEnemy = null;
            currentTargetIndex = -1;
        }
    }

    bool HasLineOfSight(Transform target)
    {
        if (target == null) return false;

        Vector3 direction = target.position - playerTransform.position;
        float distance = direction.magnitude;

        // Ignore collision between player layer and enemy layer
        int layerMask = ~(1 << gameObject.layer | enemyLayer.value);

        // Start ray slightly above player to avoid hitting ground or own colliders
        Vector3 rayStart = playerTransform.position + Vector3.up * 0.5f;

        bool hasLineOfSight = !Physics.Raycast(rayStart, direction.normalized, distance, layerMask);

        // Debug rays are only visible in Scene view
        Debug.DrawRay(rayStart, direction.normalized * distance, hasLineOfSight ? Color.green : Color.red, 0.2f);

        // Debug.Log($"Line of sight check to {target.name}: {hasLineOfSight}");

        return hasLineOfSight;
    }

    void CreateTargetIndicator()
    {
        if (targetIndicatorPrefab != null && targetEnemy != null)
        {
            ClearTargetIndicator();
            activeTargetIndicator = Instantiate(targetIndicatorPrefab,
                targetEnemy.position + Vector3.up * 2f, Quaternion.identity);
            activeTargetIndicator.transform.SetParent(targetEnemy);
        }
    }

    void UpdateTargetIndicator()
    {
        if (activeTargetIndicator == null && targetIndicatorPrefab != null && targetEnemy != null)
        {
            CreateTargetIndicator();
        }
    }

    void ClearTargetIndicator()
    {
        if (activeTargetIndicator != null)
        {
            Destroy(activeTargetIndicator);
            activeTargetIndicator = null;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            // First refresh the target list to ensure we're up to date
            FindAllPotentialTargets();

            // Check if the requested target is in our potential list
            int index = potentialTargets.IndexOf(newTarget);
            if (index >= 0)
            {
                targetEnemy = newTarget;
                currentTargetIndex = index;
                _isLockedOn = true;
                CreateTargetIndicator();
                SetGroupTargets(playerTransform, targetEnemy);
                ResetToLockOnCamera();
            }
            else if (IsValidTarget(newTarget))
            {
                // If target is valid but not in list (maybe just appeared)
                potentialTargets.Add(newTarget);
                targetEnemy = newTarget;
                currentTargetIndex = potentialTargets.Count - 1;
                _isLockedOn = true;
                CreateTargetIndicator();
                SetGroupTargets(playerTransform, targetEnemy);
                ResetToLockOnCamera();
            }
        }
    }

    bool IsValidTarget(Transform target)
    {
        if (target == null) return false;

        // Check if enemy is on the right layer
        if (((1 << target.gameObject.layer) & enemyLayer.value) == 0) return false;

        // Check distance
        float distance = Vector3.Distance(playerTransform.position, target.position);
        if (distance > trackingRange) return false;

        // Check line of sight
        if (!HasLineOfSight(target)) return false;

        return true;
    }

    void SetGroupTargets(Transform player, Transform enemy)
    {
        if (targetGroup != null && enemy != null)
        {
            // Get the height offset for the player (to aim at upper body/head)
            float playerHeight = 1.8f; // Default human height estimation
            CharacterController playerController = player.GetComponent<CharacterController>();
            if (playerController != null)
            {
                playerHeight = playerController.height;
            }
            else
            {
                Collider playerCollider = player.GetComponent<Collider>();
                if (playerCollider != null)
                {
                    playerHeight = playerCollider.bounds.size.y;
                }
            }

            // Get the height offset for the enemy (to aim at upper body/head)
            float enemyHeight = 1.8f; // Default estimation
            CharacterController enemyController = enemy.GetComponent<CharacterController>();
            if (enemyController != null)
            {
                enemyHeight = enemyController.height;
            }
            else
            {
                Collider enemyCollider = enemy.GetComponent<Collider>();
                if (enemyCollider != null)
                {
                    enemyHeight = enemyCollider.bounds.size.y;
                }
            }

            // Force camera to target upper bodies rather than feet
            Vector3 playerTargetPos = player.position + Vector3.up * (playerHeight * 0.7f);
            Vector3 enemyTargetPos = enemy.position + Vector3.up * (enemyHeight * 0.7f);

            // We need to create empty GameObjects to serve as camera targets at the right heights
            EnsureTempTarget("PlayerUpperBodyTarget", player, playerTargetPos);
            EnsureTempTarget("EnemyUpperBodyTarget", enemy, enemyTargetPos);

            Transform playerUpperTarget = GameObject.Find("PlayerUpperBodyTarget").transform;
            Transform enemyUpperTarget = GameObject.Find("EnemyUpperBodyTarget").transform;

            // Update these targets' positions
            playerUpperTarget.position = playerTargetPos;
            enemyUpperTarget.position = enemyTargetPos;

            // Set the target group to use these elevated targets
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = playerUpperTarget, weight = 1.5f, radius = 0.3f },
                new CinemachineTargetGroup.Target { target = enemyUpperTarget, weight = 1f, radius = 0.3f }
            };

            Debug.Log($"Set camera to target player at height {playerTargetPos.y:F2} and enemy at height {enemyTargetPos.y:F2}");
        }
        else
        {
            Debug.LogError("Cannot set group targets: " +
                (targetGroup == null ? "targetGroup is null" : "enemy is null"));
        }
    }

    // Helper method to ensure we have target GameObjects at the correct heights
    void EnsureTempTarget(string name, Transform parent, Vector3 position)
    {
        GameObject targetObj = GameObject.Find(name);
        if (targetObj == null)
        {
            targetObj = new GameObject(name);
            targetObj.transform.position = position;
            targetObj.transform.parent = parent;
        }
    }

    void ResetGroupToPlayerOnly()
    {
        if (targetGroup != null)
        {
            // Get the height offset for the player (to aim at upper body/head)
            float playerHeight = 1.8f; // Default human height estimation
            if (playerTransform != null)
            {
                CharacterController playerController = playerTransform.GetComponent<CharacterController>();
                if (playerController != null)
                {
                    playerHeight = playerController.height;
                }
                else
                {
                    Collider playerCollider = playerTransform.GetComponent<Collider>();
                    if (playerCollider != null)
                    {
                        playerHeight = playerCollider.bounds.size.y;
                    }
                }
            }

            // Force camera to target upper body rather than feet
            Vector3 playerTargetPos = playerTransform.position + Vector3.up * (playerHeight * 0.7f);

            // We need to create an empty GameObject to serve as a camera target at the right height
            EnsureTempTarget("PlayerUpperBodyTarget", playerTransform, playerTargetPos);
            Transform playerUpperTarget = GameObject.Find("PlayerUpperBodyTarget").transform;

            // Update the target's position
            playerUpperTarget.position = playerTargetPos;

            targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = playerUpperTarget, weight = 1f, radius = 0.3f }
            };
        }
    }

    void ResetToNormalCamera()
    {
        cinemachineCamera.Priority = activePriority;
        cinemachineGroupCamera.Priority = inactivePriority;
        ResetGroupToPlayerOnly();

        // Also restore initial camera settings when resetting to normal camera
        RestoreInitialCameraSettings();
    }

    void ResetToLockOnCamera()
    {
        cinemachineCamera.Priority = inactivePriority;
        cinemachineGroupCamera.Priority = activePriority;
    }

    void OnDestroy()
    {
        // When this object is being destroyed, we need to clean up without trying to re-parent objects
        ClearTargetIndicator();

        // Remove the temporary target objects if they exist
        CleanupTempTargets();

        // Just reset camera priorities without changing parents
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Priority = activePriority;
        }

        if (cinemachineGroupCamera != null)
        {
            cinemachineGroupCamera.Priority = inactivePriority;
        }
    }

    void CleanupTempTargets()
    {
        // Find and destroy temporary target objects without re-parenting
        GameObject playerTarget = GameObject.Find("PlayerUpperBodyTarget");
        if (playerTarget != null)
        {
            Destroy(playerTarget);
        }

        GameObject enemyTarget = GameObject.Find("EnemyUpperBodyTarget");
        if (enemyTarget != null)
        {
            Destroy(enemyTarget);
        }
    }

    // Visualization of detection range and field of view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        if (cameraTransform != null)
        {
            // Horizontal FOV visualization
            Vector3 rightLimit = Quaternion.Euler(0, horizontalFieldOfView / 2f, 0) * cameraTransform.forward;
            Vector3 leftLimit = Quaternion.Euler(0, -horizontalFieldOfView / 2f, 0) * cameraTransform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + rightLimit * trackingRange);
            Gizmos.DrawLine(transform.position, transform.position + leftLimit * trackingRange);

            // Vertical FOV visualization
            Vector3 upLimit = Quaternion.Euler(verticalFieldOfView / 2f, 0, 0) * cameraTransform.forward;
            Vector3 downLimit = Quaternion.Euler(-verticalFieldOfView / 2f, 0, 0) * cameraTransform.forward;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + upLimit * trackingRange);
            Gizmos.DrawLine(transform.position, transform.position + downLimit * trackingRange);
        }
    }
}