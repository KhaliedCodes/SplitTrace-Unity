using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;

public class RangedEnemy : MonoBehaviour, IEnemy
{
    [Header("Stats")]
    public int health = 100;
    public float moveSpeed = 2.5f;
    public float attackDamage = 10f;
    public float detectionRange = 12f;
    public float attackRange = 8f;
    public float attackCooldown = 1.5f;
    public float startAttakingRange = 6f;

    [Header("Patrol Settings")]
    public List<Transform> waypoints;
    public float waypointStopTime = 2f;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0;
    private bool isWaiting = false;

    [Header("References")]
    public GameObject player;
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public GameObject projectilePrefab;

    private IEnemyStates currentState;
    private float lastAttackTime;

    // IEnemy implementation
    public int Health { get => health; set => health = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public Transform transform => base.transform;
    public float DetectionRange { get => detectionRange; set => detectionRange = value; }
    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }
    public GameObject Player => player;
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    public Animator Animator => animator;
    public bool IsDead => health <= 0;
    public List<Transform> Waypoints { get => waypoints; set => waypoints = value; }
    public float WaypointStopTime { get => waypointStopTime; set => waypointStopTime = value; }
    public int CurrentWaypointIndex { get => currentWaypointIndex; set => currentWaypointIndex = value; }

    public bool IsPlayerInDetectionRange => player != null &&
        Vector3.Distance(transform.position, player.transform.position) <= detectionRange;

    public bool IsPlayerInAttackRange => player != null &&
        Vector3.Distance(transform.position, player.transform.position) <= attackRange;

    public bool CanAttack() => Time.time > lastAttackTime + attackCooldown;

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
            currentState?.UpdateState(this);
        }
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

        if (projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = direction * 20f;
        }

        lastAttackTime = Time.time;
    }

    public void MoveToNextWaypoint()
    {
        if (waypoints.Count == 0) return;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    public void ChangeState(IEnemyStates newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (IsDead) Die();
    }

    public void Die()
    {
        animator.SetTrigger("Die");
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
            string stateName = currentState?.GetType().Name ?? "Null";
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