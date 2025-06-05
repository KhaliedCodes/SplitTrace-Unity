using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class RangedEnemy : MonoBehaviour, IEnemy, IDamagable
{
    [Header("Stats")]
    [SerializeField] private float health = 100;
    [SerializeField] private float Maxhealth = 100;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] public float attackDamage = 10f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Trigger Settings")]
    [SerializeField] private Vector3 _colliderCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Color _detectionGizmoColor = new Color(1, 0.92f, 0.016f, 0.3f);
    [SerializeField] private Color _attackGizmoColor = new Color(1, 0, 0, 0.3f);

    [Header("References")]
    public GameObject player;
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public GameObject projectilePrefab;
    public GameObject StunprojectilePrefab;

    [Header("Patrol Settings")]
    public List<Transform> waypoints;
    public float waypointStopTime = 2f;
    public int _currentWaypointIndex = 0;

    // Private variables
    private float _lastAttackTime;
    private IEnemyStates _currentState;

    [SerializeField] private RangedDetectionChecker _detectionChecker;
    [SerializeField] private StartRangedAttackChecker _attackChecker;
    private bool _playerInDetectionRange;
    private bool _playerInAttackRange;

    [SerializeField] float RayCastY;

    [SerializeField] private Vector3 _lastKnownPlayerPosition;
    private bool _isSearching = false;
    private float _searchStartTime;
    private float _searchDuration = 4f;

    [Header("IEnemy Linking")]
    public float Health { get => health; set => health = value; }
    public float MaxHealth { get => Maxhealth; set => Maxhealth = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public new Transform transform => base.transform;
    public float DetectionRange { get => detectionRange; set => detectionRange = value; }
    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }
    public GameObject Player => player;
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    public Animator Animator => animator;
    public bool IsDead => health <= 0;
    public List<Transform> Waypoints { get => waypoints; set => waypoints = value; }
    public float WaypointStopTime { get => waypointStopTime; set => waypointStopTime = value; }
    public int CurrentWaypointIndex { get => _currentWaypointIndex; set => _currentWaypointIndex = value; }

    public bool IsPlayerInDetectionRange => _playerInDetectionRange;
    public bool IsPlayerInAttackRange => _playerInAttackRange;

    public Vector3 LastKnownPlayerPosition
    {
        get => _lastKnownPlayerPosition;
        set => _lastKnownPlayerPosition = value;
    }


    private void Awake()
    {
        _detectionChecker = GetComponentInChildren<RangedDetectionChecker>();
        _attackChecker = GetComponentInChildren<StartRangedAttackChecker>();

        if (_detectionChecker != null)
        {
            _detectionChecker.Initialize(detectionRange, this);
        }
        if (_attackChecker != null)
        {
           _attackChecker.Initialize(attackRange, this);
        }
    }

    private void Start()
    {
        navMeshAgent.speed = moveSpeed;
        animator = GetComponent<Animator>();
        ChangeState(new IdleState());
        health = Maxhealth;
    }

    void Update()
    {
        if (!IsDead)
        {
            _currentState?.UpdateState(this);
        }
    }

    public void SetPlayerInDetectionRange(bool inRange)
    {
        _playerInDetectionRange = inRange;
    }

    public void SetPlayerInAttackRange(bool inRange)
    {
        _playerInAttackRange = inRange;
    }

    public void ChangeState(IEnemyStates newState)
    {
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState?.EnterState(this);
    }

    public bool HasLineOfSight()
    {
        if (!player) return false;

        Vector3 origin = transform.position + Vector3.up * RayCastY; 
        Vector3 direction = (player.transform.position + new Vector3(0,1,0) - origin).normalized;
        float distance = Vector3.Distance(origin, player.transform.position);

        Debug.DrawRay(origin, direction * distance, Color.red);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.gameObject == player)
            {
                return true;
            }
        }

        return false;
    }


    public bool CanAttack()
    {
         return Time.time > _lastAttackTime + attackCooldown;
    }

    public void ShootProjectile()
    {
        if (!CanAttack() || projectilePrefab == null || player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        GameObject projectile = Instantiate(
            projectilePrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.LookRotation(direction)
        );

        // Pass the damage to the bullet
        if (projectile.TryGetComponent<EnemyBulletEffector>(out var bulletEffector))
        {
            bulletEffector.DamageAmount = attackDamage;
        }

        if (projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = direction * 20f;
        }

        _lastAttackTime = Time.time;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (IsDead) Die();
    }

    public float UpdateHealth(float heal, float Damage)
    {
        health += heal;
        return health;
    }

    public void DealDamage()
    {
      
    }

    public void Die()
    {
        ChangeState(new DeathState());
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 1. Detection Range (Yellow Wire Sphere)
        Gizmos.color = _detectionGizmoColor;
        Gizmos.DrawSphere(transform.position + _colliderCenter, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + _colliderCenter, detectionRange);

        // 2. Attack Range (Red Wire Sphere)
        Gizmos.color = _attackGizmoColor;
        Gizmos.DrawSphere(transform.position + _colliderCenter, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + _colliderCenter, attackRange);

        // 3. Player Connection (Green Line)
        if (Player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Player.transform.position);

            Vector3 direction = (Player.transform.position - transform.position).normalized;
            DrawArrow(transform.position, direction, 0.5f, 20f);
        }

        // 4. Waypoint System (Cyan)
        if (waypoints != null && waypoints.Count > 0)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                if (i > 0 && waypoints[i - 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                }
            }
            if (waypoints.Count > 1 && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
            }
        }

        // 5. Current State Indicator
        if (Application.isPlaying)
        {
            string stateName = _currentState?.GetType().Name ?? "Null";
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                                    $"State: {stateName}\nHealth: {health}", style);
        }
    }

    void DrawArrow(Vector3 pos, Vector3 direction, float length, float angleDegrees)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, angleDegrees, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -angleDegrees, 0) * Vector3.back;
        Gizmos.DrawRay(pos + direction * length, right * length * 0.5f);
        Gizmos.DrawRay(pos + direction * length, left * length * 0.5f);
    }
#endif
}