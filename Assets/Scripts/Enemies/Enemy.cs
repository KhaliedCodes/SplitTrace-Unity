using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEnemy, IDamagable
{
    [Header("Stats")]
    [SerializeField] private float health = 100;
    [SerializeField] private float Maxhealth = 100;
    [SerializeField] public float moveSpeed = 3.5f;
    [SerializeField] public float attackDamage = 15f;
    [SerializeField] public float detectionRange = 8f;
    [SerializeField] public float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;

    
    [Header("References")]
    public GameObject player;
    public NavMeshAgent navMeshAgent;
    public Animator animator;

    [Header("IEnemy Linking")]
    public float Health { get => health; set => health = value; }
    public float MaxHealth { get => Maxhealth; set => Maxhealth = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public new Transform transform => base.transform;
    public float DetectionRange
    {
        get => detectionRange; set => detectionRange = value;
    }
    public List<Transform> Waypoints { get => waypoints; set => waypoints = value; }
    public float WaypointStopTime { get => waypointStopTime; set => waypointStopTime = value; }
    public int CurrentWaypointIndex
    {
        get => _currentWaypointIndex;
        set => _currentWaypointIndex = value;
    }
    public GameObject Player => player;
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    public Animator Animator => animator;
    public bool IsDead => health <= 0;


    [Header("Patrol Settings")]
    public List<Transform> waypoints; 
    public float waypointStopTime = 2f;
    public int _currentWaypointIndex = 0;


    //Private Var

    private float _lastAttackTime;
    private IEnemyStates _currentState;
    [SerializeField] private DetectionChecker _detectionChecker;
    [SerializeField] private StartAttackChecker _attackChecker;
    private bool _playerInDetectionRange;
    private bool _playerInAttackRange;
    private Vector3 _lastKnownPlayerPosition;

    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }

    public Vector3 LastKnownPlayerPosition
    {
        get => _lastKnownPlayerPosition;
        set => _lastKnownPlayerPosition = value;
    }


    private void Awake()
    {
        _detectionChecker = GetComponentInChildren<DetectionChecker>();
        _attackChecker = GetComponentInChildren<StartAttackChecker>();

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

    //Checkers From Children
    public bool IsPlayerInDetectionRange => _playerInDetectionRange;
    public bool IsPlayerInAttackRange => _playerInAttackRange;

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
        if (Player == null) return false;

        Vector3 direction = (Player.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, Player.transform.position);

        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, out RaycastHit hit, distance))
        {

            if (hit.collider.CompareTag("Player"))
            { 
                _lastKnownPlayerPosition = Player.transform.position;
                return true;
            }
        }

        return false;
    }
    public bool CanAttack()
    {
        return Time.time > _lastAttackTime + attackCooldown;
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
        animator.Play("Die");
        navMeshAgent.speed = 0;
        ChangeState(new IdleState());
        navMeshAgent.isStopped = true;
        Destroy(gameObject, 2f);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 1. Detection Range (Yellow Wire Sphere)
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.25f); // Semi-transparent yellow
        Gizmos.DrawSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 2. Attack Range (Red Wire Sphere)
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Gizmos.DrawSphere(transform.position, 1.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);

        // 3. Player Connection (Green Line)
        if (Player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Player.transform.position);

            // Draw arrow toward player
            Vector3 direction = (Player.transform.position - transform.position).normalized;
            DrawArrow(transform.position, direction, 0.5f, 20f);
        }

        // 4. Waypoint System (Cyan)
        if (waypoints != null && waypoints.Count > 0)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Semi-transparent cyan
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                if (i > 0 && waypoints[i - 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                }
            }
            // Close the loop
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
        if (player == null) return;
        Vector3 dir = (Player.transform.position - transform.position).normalized;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, dir * detectionRange);



    }

    // Helper for drawing arrows
    void DrawArrow(Vector3 pos, Vector3 direction, float length, float angleDegrees)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, angleDegrees, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -angleDegrees, 0) * Vector3.back;
        Gizmos.DrawRay(pos + direction * length, right * length * 0.5f);
        Gizmos.DrawRay(pos + direction * length, left * length * 0.5f);
    }


#endif

}




