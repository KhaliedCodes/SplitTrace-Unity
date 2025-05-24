using UnityEngine;

public class DetectionState : IEnemyStates
{
    private bool isSearching = false;
    private float searchStartTime;
    private readonly float searchDuration = 3f;

    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.SetBool("IsMoving", true);
        enemy.NavMeshAgent.speed = enemy.MoveSpeed * 1.5f;

        isSearching = false;
        searchStartTime = 0f;
    }

    public void UpdateState(IEnemy enemy)
    {
        // If player left detection zone entirely, return to idle
        if (!enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new IdleState());
            return;
        }

        // If player reference is lost, do nothing
        if (enemy.Player == null) return;

        if (enemy.HasLineOfSight())
        {
            // Player visible: pursue
            isSearching = false;

            enemy.LastKnownPlayerPosition = enemy.Player.transform.position;
            enemy.NavMeshAgent.SetDestination(enemy.Player.transform.position);

            if (enemy.IsPlayerInAttackRange)
            {
                enemy.ChangeState(new AttackState());
            }
        }
        else
        {
            // No line of sight
            if (!isSearching)
            {
                isSearching = true;
                searchStartTime = Time.time;

                enemy.NavMeshAgent.SetDestination(enemy.LastKnownPlayerPosition);
            }
            else
            {
                // Still searching for the player
                enemy.NavMeshAgent.SetDestination(enemy.LastKnownPlayerPosition);

                if (Time.time - searchStartTime >= searchDuration)
                {
                    // Search expired: go idle
                    enemy.ChangeState(new IdleState());
                }
            }
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.Animator.SetBool("IsMoving", false);
        isSearching = false;
    }
}
