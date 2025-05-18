using UnityEngine;

public class IdleState : IEnemyStates
{
    private float _waypointTimer;

    public void EnterState(Enemy enemy)
    {
        enemy.animator.Play("Walk");
        enemy.navMeshAgent.speed = enemy.moveSpeed;
        _waypointTimer = 0;

        if (enemy.waypoints.Count > 0)
        {
            enemy.navMeshAgent.SetDestination(enemy.waypoints[enemy._currentWaypointIndex].position);
        }
    }

    public void UpdateState(Enemy enemy)
    {
        // Check for player entering detection range
        if (enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new DetectionState());
            return;
        }

        // Patrol logic
        if (enemy.waypoints.Count > 0 &&
            enemy.navMeshAgent.remainingDistance <= 0.5f)
        {
            _waypointTimer += Time.deltaTime;

            if (_waypointTimer >= enemy.waypointStopTime)
            {
                enemy._currentWaypointIndex = (enemy._currentWaypointIndex + 1) % enemy.waypoints.Count;
                enemy.navMeshAgent.SetDestination(enemy.waypoints[enemy._currentWaypointIndex].position);
                _waypointTimer = 0;
            }
        }
    }

    public void ExitState(Enemy enemy) { }
}