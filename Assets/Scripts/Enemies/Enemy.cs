using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEnemy
{
    [Header("Stats")]
    [SerializeField] private int health = 100;
    [SerializeField] public float moveSpeed = 3.5f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] public float detectionRange = 8f;
    [SerializeField] public float StartAttakingRange = 2f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("References")]
    public GameObject player;
    public NavMeshAgent navMeshAgent;
    public Animator animator;

    // IEnemy implementation
    public int Health { get => health; set => health = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public Transform transform => base.transform;
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
    public List<Transform> waypoints;  // Assign in Inspector
    public float waypointStopTime = 2f;
    public int _currentWaypointIndex = 0;
    private float _waitTimer = 0;
    private bool _isWaiting = false;

    [Header("Combat")]
    public float attackRange = 1.5f;
    [SerializeField] private float startAttakingRange = 2f;

    //Properties

    private float _lastAttackTime;

    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }

    private IEnemyStates _currentState;


    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        navMeshAgent.speed = moveSpeed;
        animator = GetComponent<Animator>();
        ChangeState(new IdleState());

    }


    void Update()
    {
        if (!IsDead)
        {
            _currentState?.UpdateState(this);
        }
    }



    public void ChangeState(IEnemyStates newState)
    {
        _currentState?.ExitState(this);
        _currentState = newState;
        _currentState?.EnterState(this);
    }


    public bool IsPlayerInDetectionRange
    {
        get
        {
            if (Player == null) return false;
            return Vector3.Distance(transform.position, Player.transform.position) <= detectionRange;
        }
    }

    public bool IsPlayerInAttackRange
    {
        get
        {
            if (Player == null) return false;
            return Vector3.Distance(transform.position, Player.transform.position) <= attackRange;
        }
    }

    public bool CanAttack()
    {
        return Time.time > _lastAttackTime + attackCooldown;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (IsDead) Die();
    }

    public void DealDamage()
    {
        
    }

    public void Die()
    {
        animator.Play("Die");
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




