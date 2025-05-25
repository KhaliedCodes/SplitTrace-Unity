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
    public float verticalFieldOfView = 60f;
    public float detectionInterval = 0.2f;

    [Header("Enemy Layer")]
    public LayerMask enemyLayer;

    [Header("Lock-On Settings")]
    public KeyCode toggleLockOnKey = KeyCode.E;
    public KeyCode nextTargetKey = KeyCode.Q;
    public KeyCode previousTargetKey = KeyCode.Z;

    // Improved camera transition settings
    [Header("Camera Transition")]
    [SerializeField] private float blendTime = 0.5f; // Time to blend between cameras
    [SerializeField] private CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseInOut;

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
    public Transform lookAtTarget;

    private Transform targetEnemy;
    private GameObject activeTargetIndicator;
    private bool _isLockedOn = false;
    private CustomStarterAssetsInputs inputHandler;
    private float nextDetectionTime = 0f;
    private List<Transform> potentialTargets = new List<Transform>();
    private int currentTargetIndex = -1;

    // Store initial camera settings
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private Transform initialFollowTarget;
    private Transform initialLookAtTarget;

    // Additional camera state tracking
    private CinemachineTransposer transposer;
    private Vector3 initialCameraOffset;
    private bool initialCameraInitialized = false;
    private CinemachineBrain cinemachineBrain;

    public Transform currentTarget => targetEnemy;
    public bool isLockedOn => _isLockedOn && targetEnemy != null;

    void Awake()
    {
        playerTransform = transform;
        inputHandler = GetComponent<CustomStarterAssetsInputs>();

        // Find the CinemachineBrain for managing blends
        cinemachineBrain = FindObjectOfType<CinemachineBrain>();
        if (cinemachineBrain == null)
        {
            Debug.LogWarning("CinemachineBrain not found. Camera transitions may not blend properly.");
        }
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

        // Ensure proper camera setup at start
        SwitchToPlayerCamera();
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

            // Set blend definition for smoother transitions
            if (cinemachineBrain != null)
            {
                CinemachineBlendDefinition blend = new CinemachineBlendDefinition(
                    blendStyle, blendTime);

                // This might require a custom transition setup in your CinemachineBrain
                // depending on your project configuration
            }
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
                // Find all potential targets
                FindAllPotentialTargets();

                // If no targets found, don't enable lock-on
                if (potentialTargets.Count == 0)
                {
                    _isLockedOn = false;
                    Debug.Log("No valid targets found - lock-on not enabled");
                    return;
                }

                // Select closest target and create indicator
                SelectClosestTarget();
                CreateTargetIndicator();

                // Switch to lock-on camera (no need for group targets anymore)
                SwitchToLockOnCamera();

                Debug.Log($"Lock-on mode enabled, targeting: {targetEnemy.name}");
            }
            else
            {
                // Clear target and reset to normal camera
                ClearTargetIndicator();
                targetEnemy = null;
                currentTargetIndex = -1;
                potentialTargets.Clear();

                // Reset camera to normal view - THIS IS THE KEY PART FOR LOCK-OFF
                SwitchToPlayerCamera();

                Debug.Log("Lock-on mode disabled");
            }
        }
    }

    // Improved method for switching to lock-on camera
    void SwitchToLockOnCamera()
    {
        // Store current camera settings before switching if not already stored
        if (!initialCameraInitialized)
        {
            StoreInitialCameraSettings();
        }

        // Instead of using group camera, modify the main camera to look at the enemy
        // while still following the player for true third-person lock-on
        if (cinemachineCamera != null && targetEnemy != null)
        {
            // Keep following the player (or lookAtTarget)
            cinemachineCamera.Follow = lookAtTarget != null ? lookAtTarget : playerTransform;

            // Make the camera look at the enemy instead
            cinemachineCamera.LookAt = targetEnemy;

            // Ensure main camera stays active
            cinemachineCamera.Priority = activePriority;
            cinemachineGroupCamera.Priority = inactivePriority;

            // Force update the virtual camera
            cinemachineCamera.PreviousStateIsValid = false;
        }

        // Force Cinemachine to update immediately for smoother transition
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(blendStyle, blendTime);
            cinemachineBrain.ManualUpdate();
        }

        Debug.Log("Switched to lock-on camera (third-person view looking at enemy)");
    }

    // Improved method for switching back to player camera
    void SwitchToPlayerCamera()
    {
        // Set appropriate camera priorities
        cinemachineGroupCamera.Priority = inactivePriority;
        cinemachineCamera.Priority = activePriority;

        // Reset group to only include player
        ResetGroupToPlayerOnly();

        // Restore original camera settings
        RestoreInitialCameraSettings();

        // Force Cinemachine to update immediately for smoother transition
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(blendStyle, blendTime);
            cinemachineBrain.ManualUpdate();
        }

        Debug.Log("Switched back to player camera");
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

            // Force update the virtual camera
            cinemachineCamera.PreviousStateIsValid = false;
        }
    }

    System.Collections.IEnumerator ResetCameraAfterDelay()
    {
        // Wait for end of frame to let Cinemachine process first
        yield return new WaitForEndOfFrame();

        // If camera is still not at the right position, force it
        if (cameraTransform != null && Vector3.Distance(cameraTransform.position, initialCameraPosition) > 0.1f)
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

    // Get all potential targets in range
    void FindAllPotentialTargets()
    {
        potentialTargets.Clear();
        currentTargetIndex = -1;

        if (Time.time < nextDetectionTime) return;
        nextDetectionTime = Time.time + detectionInterval;

        // Debug information
        Debug.Log($"Searching for enemies with layer mask: {enemyLayer.value} (Layer names: {GetLayerNames(enemyLayer)})");
        Debug.Log($"Player position: {transform.position}, Search range: {trackingRange}");

        // Get all colliders in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, trackingRange, enemyLayer);
        Debug.Log($"Found {hitColliders.Length} colliders in range with enemy layer");

        // If no colliders found, try finding all colliders to debug
        if (hitColliders.Length == 0)
        {
            Collider[] allColliders = Physics.OverlapSphere(transform.position, trackingRange);
            Debug.Log($"Total colliders in range (all layers): {allColliders.Length}");

            foreach (var col in allColliders)
            {
                Debug.Log($"Found collider: {col.name} on layer {col.gameObject.layer} ({LayerMask.LayerToName(col.gameObject.layer)})");
            }
        }

        foreach (var hitCollider in hitColliders)
        {
            Transform enemyTransform = hitCollider.transform;
            Debug.Log($"Checking enemy: {enemyTransform.name} at position {enemyTransform.position}");

            // Check if enemy is in field of view
            if (IsInFieldOfView(enemyTransform))
            {
                potentialTargets.Add(enemyTransform);
                Debug.Log($"Added {enemyTransform.name} to potential targets (in FOV)");
            }
            else
            {
                Debug.Log($"Enemy {enemyTransform.name} not in field of view");
            }
        }

        // Sort enemies by distance
        potentialTargets.Sort((a, b) =>
            Vector3.Distance(transform.position, a.position)
            .CompareTo(Vector3.Distance(transform.position, b.position)));

        Debug.Log($"Found {potentialTargets.Count} potential targets in range and FOV");
    }

    // Check if a target is within the field of view
    bool IsInFieldOfView(Transform target)
    {
        if (target == null || cameraTransform == null)
        {
            Debug.LogWarning("Target or camera transform is null in IsInFieldOfView");
            return false;
        }

        Vector3 directionToTarget = (target.position - cameraTransform.position).normalized;
        Vector3 cameraForward = cameraTransform.forward;

        // Calculate angle between camera forward and direction to target
        float angle = Vector3.Angle(cameraForward, directionToTarget);

        // For debugging
        Debug.Log($"FOV Check for {target.name}: Angle = {angle:F1}°, Max allowed = {horizontalFieldOfView * 0.5f:F1}°");

        // Simple horizontal FOV check (ignoring vertical for now to simplify debugging)
        bool isInFOV = angle <= horizontalFieldOfView * 0.5f;

        return isInFOV;
    }

    // Select the closest target from potential targets
    void SelectClosestTarget()
    {
        if (potentialTargets.Count == 0)
        {
            targetEnemy = null;
            currentTargetIndex = -1;
            return;
        }

        // Default to closest target (index 0 after sorting)
        currentTargetIndex = 0;
        targetEnemy = potentialTargets[currentTargetIndex];
    }

    // Handle switching between targets
    void HandleTargetSwitching()
    {
        if (potentialTargets.Count <= 1) return;

        bool targetSwitched = false;

        // Switch to next target
        if (Input.GetKeyDown(nextTargetKey))
        {
            currentTargetIndex = (currentTargetIndex + 1) % potentialTargets.Count;
            targetSwitched = true;
        }
        // Switch to previous target
        else if (Input.GetKeyDown(previousTargetKey))
        {
            currentTargetIndex--;
            if (currentTargetIndex < 0) currentTargetIndex = potentialTargets.Count - 1;
            targetSwitched = true;
        }

        if (targetSwitched && currentTargetIndex >= 0 && currentTargetIndex < potentialTargets.Count)
        {
            // Update target and indicator
            targetEnemy = potentialTargets[currentTargetIndex];
            UpdateTargetIndicator();

            // Update the camera to look at the new target
            if (cinemachineCamera != null)
            {
                cinemachineCamera.LookAt = targetEnemy;
                cinemachineCamera.PreviousStateIsValid = false;
            }

            Debug.Log($"Switched target to: {targetEnemy.name}");
        }
    }

    // Create target indicator UI
    void CreateTargetIndicator()
    {
        ClearTargetIndicator();

        if (targetEnemy != null && targetIndicatorPrefab != null)
        {
            activeTargetIndicator = Instantiate(targetIndicatorPrefab,
                targetEnemy.position + Vector3.up * 2f, Quaternion.identity);

            // Parent to the target for auto-following
            activeTargetIndicator.transform.SetParent(targetEnemy);
        }
    }

    // Update target indicator position
    void UpdateTargetIndicator()
    {
        if (activeTargetIndicator != null && targetEnemy != null)
        {
            // Destroy old indicator and create a new one attached to the new target
            ClearTargetIndicator();
            CreateTargetIndicator();
        }
    }

    // Clear the target indicator
    void ClearTargetIndicator()
    {
        if (activeTargetIndicator != null)
        {
            Destroy(activeTargetIndicator);
            activeTargetIndicator = null;
        }
    }

    // Set the target group to include both player and enemy
    void SetGroupTargets(Transform player, Transform enemy)
    {
        if (targetGroup != null && player != null && enemy != null)
        {
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = player, weight = 1.5f, radius = 0.5f },
                new CinemachineTargetGroup.Target { target = enemy, weight = 1f, radius = 0.3f }
            };
        }
    }

    // Reset target group to only include player
    void ResetGroupToPlayerOnly()
    {
        if (targetGroup != null && playerTransform != null)
        {
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = playerTransform, weight = 1f, radius = 0.5f }
            };
        }
    }

    // Manage the current target - check validity and update as needed
    void ManageCurrentTarget()
    {
        if (targetEnemy == null || !IsTargetValid(targetEnemy))
        {
            // Find new target if current is invalid
            RefreshTargets();
            return;
        }

        // Periodically refresh target list to check for new enemies
        if (Time.time >= nextDetectionTime)
        {
            RefreshTargets();
        }
    }

    // Check if a target is still valid
    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;

        // Check if target is active, in range and in view
        bool isActive = target.gameObject.activeSelf;
        bool isInRange = Vector3.Distance(transform.position, target.position) <= trackingRange;

        return isActive && isInRange;
    }

    // Refresh the target list and update current target if needed
    void RefreshTargets()
    {
        // Store current target for comparison
        Transform previousTarget = targetEnemy;

        // Find all potential targets
        FindAllPotentialTargets();

        // If no targets, disable lock-on
        if (potentialTargets.Count == 0)
        {
            _isLockedOn = false;
            ClearTargetIndicator();
            targetEnemy = null;
            SwitchToPlayerCamera();
            Debug.Log("No valid targets - lock-on disabled");
            return;
        }

        // Try to keep targeting the same enemy if possible
        if (previousTarget != null)
        {
            int prevIndex = potentialTargets.IndexOf(previousTarget);
            if (prevIndex >= 0)
            {
                currentTargetIndex = prevIndex;
                targetEnemy = potentialTargets[currentTargetIndex];
                return;
            }
        }

        // Otherwise select closest
        SelectClosestTarget();
        UpdateTargetIndicator();

        // Update camera to look at new target
        if (cinemachineCamera != null && targetEnemy != null)
        {
            cinemachineCamera.LookAt = targetEnemy;
            cinemachineCamera.PreviousStateIsValid = false;
        }
    }

    // Add this method to verify camera states for debugging
    public void DebugCameraStates()
    {
        Debug.Log($"Player Camera Priority: {cinemachineCamera.Priority}");
        Debug.Log($"Group Camera Priority: {cinemachineGroupCamera.Priority}");
        Debug.Log($"Current Active Camera: {(cinemachineCamera.Priority > cinemachineGroupCamera.Priority ? "Player Camera" : "Group Camera")}");
        Debug.Log($"Lock-on State: {_isLockedOn}");
        Debug.Log($"Main Camera Follow: {cinemachineCamera.Follow?.name ?? "null"}");
        Debug.Log($"Main Camera LookAt: {cinemachineCamera.LookAt?.name ?? "null"}");

        if (_isLockedOn && targetEnemy != null)
        {
            Debug.Log($"Current Target: {targetEnemy.name}");
        }
    }

    // Optional visualization of detection range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        // Visualize field of view if camera is available
        if (cameraTransform != null)
        {
            Gizmos.color = Color.blue;
            Vector3 cameraPos = cameraTransform.position;

            // Draw simple FOV visualization
            float halfHorizontalFOV = horizontalFieldOfView * 0.5f * Mathf.Deg2Rad;
            float viewDistance = trackingRange * 0.75f;

            Vector3 rightDir = cameraTransform.right;
            Vector3 forwardDir = cameraTransform.forward;

            Vector3 rightEdge = Quaternion.AngleAxis(horizontalFieldOfView * 0.5f, cameraTransform.up) * forwardDir;
            Vector3 leftEdge = Quaternion.AngleAxis(-horizontalFieldOfView * 0.5f, cameraTransform.up) * forwardDir;

            Gizmos.DrawLine(cameraPos, cameraPos + rightEdge * viewDistance);
            Gizmos.DrawLine(cameraPos, cameraPos + leftEdge * viewDistance);
        }

        // Draw lines to potential targets
        Gizmos.color = Color.green;
        foreach (var target in potentialTargets)
        {
            if (target != null)
            {
                Gizmos.DrawLine(transform.position, target.position);
            }
        }

        // Draw line to current target
        if (targetEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetEnemy.position);
        }
    }

    // Helper method to get layer names for debugging
    private string GetLayerNames(LayerMask layerMask)
    {
        var layerNames = new System.Collections.Generic.List<string>();

        for (int i = 0; i < 32; i++)
        {
            if ((layerMask.value & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layerNames.Add(layerName);
                }
                else
                {
                    layerNames.Add($"Layer {i}");
                }
            }
        }

        return string.Join(", ", layerNames);
    }
}