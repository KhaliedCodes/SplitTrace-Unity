using UnityEngine;

public class IdleState : IEnemyStates
{
    private float _waypointTimer;
    private bool _isWaiting;

    public void EnterState(IEnemy enemy)
    {
        _waypointTimer = 0f;
        _isWaiting = false;

        enemy.NavMeshAgent.speed = enemy.MoveSpeed;

        if (enemy.Waypoints != null && enemy.Waypoints.Count > 0)
        {
            enemy.NavMeshAgent.SetDestination(enemy.Waypoints[enemy.CurrentWaypointIndex].position);
        }
    }

    public void UpdateState(IEnemy enemy)
    {
        float currentSpeed = enemy.NavMeshAgent.velocity.magnitude;
        enemy.Animator.SetFloat("speed", currentSpeed);

        // Check for player entering detection range
        if (enemy.IsPlayerInDetectionRange && enemy.HasLineOfSight())
        {
            enemy.ChangeState(new DetectionState());
            return;
        }

        if (enemy.Waypoints != null && enemy.Waypoints.Count > 0)
        {
            PatrolBehavior(enemy);
        }
    }

    private void PatrolBehavior(IEnemy enemy)
    {
        if (!_isWaiting && 
            !enemy.NavMeshAgent.pathPending &&
            enemy.NavMeshAgent.remainingDistance <= enemy.NavMeshAgent.stoppingDistance)
        {
            // Start waiting at the waypoint
            _isWaiting = true;
            _waypointTimer = 0f;

            // Stop movement & animation
            enemy.NavMeshAgent.ResetPath();
            enemy.Animator.SetFloat("speed", 0f);
        }

        if (_isWaiting)
        {
            _waypointTimer += Time.deltaTime;

            if (_waypointTimer >= enemy.WaypointStopTime)
            {
                // Move to next waypoint
                enemy.CurrentWaypointIndex = (enemy.CurrentWaypointIndex + 1) % enemy.Waypoints.Count;
                enemy.NavMeshAgent.SetDestination(enemy.Waypoints[enemy.CurrentWaypointIndex].position);
                _isWaiting = false;
            }
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.Animator.SetFloat("speed", 0f);
    }
}
