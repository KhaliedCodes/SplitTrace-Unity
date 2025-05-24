using UnityEngine;

public class IdleState : IEnemyStates
{
    private float _waypointTimer;

    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.SetBool("IsMoving", true);
        enemy.NavMeshAgent.speed = enemy.MoveSpeed;
        _waypointTimer = 0;

        if (enemy.Waypoints != null && enemy.Waypoints.Count > 0)
        {
            enemy.NavMeshAgent.SetDestination(enemy.Waypoints[enemy.CurrentWaypointIndex].position);
        }
    }

    public void UpdateState(IEnemy enemy)
    {
        // Check for player entering detection range
        if (enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new DetectionState());
        }
        if (enemy.Waypoints != null && enemy.Waypoints.Count > 0)
        {
            PatrolBehavior(enemy);
        }
       
    }

    private void PatrolBehavior(IEnemy enemy)
    {
        if (!enemy.NavMeshAgent.pathPending &&
            enemy.NavMeshAgent.remainingDistance <= enemy.NavMeshAgent.stoppingDistance)
        {
            _waypointTimer += Time.deltaTime;

            if (_waypointTimer >= enemy.WaypointStopTime)
            {
                enemy.CurrentWaypointIndex = (enemy.CurrentWaypointIndex + 1) % enemy.Waypoints.Count;
                enemy.NavMeshAgent.SetDestination(enemy.Waypoints[enemy.CurrentWaypointIndex].position);
                _waypointTimer = 0;
            }
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.Animator.SetBool("IsMoving", false);
    }


}