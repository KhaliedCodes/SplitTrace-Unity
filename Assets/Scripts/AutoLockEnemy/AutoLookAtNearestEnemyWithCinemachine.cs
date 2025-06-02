using UnityEngine;
using Cinemachine;

public class AutoLookAtNearestEnemyWithCinemachine : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the camera transform")]
    public Transform cameraTransform;

    [Header("Cinemachine")]
    public CinemachineTargetGroup targetGroup;  // Assign Cinemachine Target Group here
    public Transform playerTransform;           // Assign player transform here

    [Header("Tracking Settings")]
    public float trackingRange = 15f;
    public float fieldOfView = 90f;
    public float turnSpeed = 5f;

    [Header("Enemy Layer")]
    [Tooltip("Only detect enemies on this layer")]
    public LayerMask enemyLayer;

    [Header("Lock-On Settings")]
    public KeyCode toggleLockOnKey = KeyCode.E;
    public float lockOnFeedbackDuration = 0.3f;

    [Tooltip("Optional UI element to show which enemy is targeted")]
    public GameObject targetIndicatorPrefab;

    private Transform targetEnemy;
    private GameObject activeTargetIndicator;
    private bool _isLockedOn = false;
    private CustomStarterAssetsInputs inputHandler;

    // Public properties
    public Transform currentTarget => targetEnemy;
    public bool isLockedOn => _isLockedOn && targetEnemy != null;

    void Awake()
    {
        if (playerTransform == null)
            playerTransform = transform;

        inputHandler = GetComponent<CustomStarterAssetsInputs>();
    }

    void Start()
    {
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
            else
                Debug.LogWarning("No camera found! Please assign the camera transform in the inspector.");
        }

        // Initialize target group to just player at start
        ResetTargetGroupToPlayer();
    }

    void Update()
    {
        if (cameraTransform == null) return;

        if (Input.GetKeyDown(toggleLockOnKey))
        {
            _isLockedOn = !_isLockedOn;

            if (_isLockedOn)
            {
                FindNearestEnemy();
                if (targetEnemy == null)
                {
                    _isLockedOn = false;
                    Debug.Log("No valid target in range/view");
                }
                else
                {
                    CreateTargetIndicator();
                    UpdateCinemachineTargets();
                }
            }
            else
            {
                ClearTargetIndicator();
                targetEnemy = null;
                ResetTargetGroupToPlayer();
            }
        }

        if (_isLockedOn)
        {
            if (targetEnemy != null)
            {
                float distanceToTarget = Vector3.Distance(playerTransform.position, targetEnemy.position);
                if (distanceToTarget > trackingRange * 1.5f)
                {
                    FindNearestEnemy();
                    if (targetEnemy == null)
                    {
                        _isLockedOn = false;
                        ClearTargetIndicator();
                        ResetTargetGroupToPlayer();
                    }
                    else
                    {
                        UpdateTargetIndicator();
                        UpdateCinemachineTargets();
                    }
                }
                else
                {
                    UpdateTargetIndicator();
                }
            }
            else
            {
                FindNearestEnemy();
                if (targetEnemy == null)
                {
                    _isLockedOn = false;
                    ClearTargetIndicator();
                    ResetTargetGroupToPlayer();
                }
                else
                {
                    CreateTargetIndicator();
                    UpdateCinemachineTargets();
                }
            }
        }
    }

    void FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(playerTransform.position, trackingRange, enemyLayer);

        float closestDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider hit in hits)
        {
            if (hit.isTrigger) continue;

            Vector3 directionToEnemy = hit.transform.position - playerTransform.position;
            directionToEnemy.y = 0;

            float angle = Vector3.Angle(cameraTransform.forward, directionToEnemy);

            if (angle <= fieldOfView / 2f)
            {
                if (HasLineOfSight(hit.transform))
                {
                    float distance = directionToEnemy.magnitude;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nearest = hit.transform;
                    }
                }
            }
        }

        targetEnemy = nearest;
    }

    bool HasLineOfSight(Transform target)
    {
        Vector3 direction = target.position - playerTransform.position;
        float distance = direction.magnitude;

        int layerMask = ~(1 << gameObject.layer | enemyLayer.value);

        Vector3 rayStart = playerTransform.position + Vector3.up * 0.5f;

        return !Physics.Raycast(rayStart, direction, distance, layerMask);
    }

    void CreateTargetIndicator()
    {
        if (targetIndicatorPrefab != null && targetEnemy != null)
        {
            ClearTargetIndicator();
            activeTargetIndicator = Instantiate(targetIndicatorPrefab,
                targetEnemy.position + Vector3.up * 2f,
                Quaternion.identity);
            activeTargetIndicator.transform.SetParent(targetEnemy);
        }
    }

    void UpdateTargetIndicator()
    {
        if (activeTargetIndicator == null && targetIndicatorPrefab != null && targetEnemy != null)
            CreateTargetIndicator();
    }

    void ClearTargetIndicator()
    {
        if (activeTargetIndicator != null)
        {
            Destroy(activeTargetIndicator);
            activeTargetIndicator = null;
        }
    }

    void UpdateCinemachineTargets()
    {
        if (targetGroup == null || playerTransform == null) return;

        if (targetEnemy != null)
        {
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
            {
                new CinemachineTargetGroup.Target { target = playerTransform, weight = 1, radius = 1 },
                new CinemachineTargetGroup.Target { target = targetEnemy, weight = 1, radius = 1 }
            };
        }
        else
        {
            ResetTargetGroupToPlayer();
        }
    }

    void ResetTargetGroupToPlayer()
    {
        if (targetGroup == null || playerTransform == null) return;

        targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
        {
            new CinemachineTargetGroup.Target { target = playerTransform, weight = 1, radius = 1 }
        };
    }

    // Optional: visualize detection range and FOV
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        if (cameraTransform != null)
        {
            Vector3 rightLimit = Quaternion.Euler(0, fieldOfView / 2f, 0) * cameraTransform.forward;
            Vector3 leftLimit = Quaternion.Euler(0, -fieldOfView / 2f, 0) * cameraTransform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + rightLimit * trackingRange);
            Gizmos.DrawLine(transform.position, transform.position + leftLimit * trackingRange);
        }
    }
}
